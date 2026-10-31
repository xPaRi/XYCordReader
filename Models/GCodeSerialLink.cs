using System.IO.Ports;
using System.Text.RegularExpressions;

namespace XYCordReader.Models;

/// <summary>
/// Sériová linka ke G-code stroji.
/// Skládá příchozí data do celých řádků a posílá vždy jen jeden příkaz, další až po jeho "ok".
/// </summary>
public sealed partial class GCodeSerialLink : IDisposable
{
    /// <summary>
    /// Výchozí doba čekání na "ok".
    /// </summary>
    public const int DefaultTimeoutMs = 5000;

    private sealed record Command(string Line, int TimeoutMs, string? Key);

    private readonly SerialPort _SerialPort = new();

    /// <summary>
    /// Chrání frontu příkazů, aktuální příkaz a časovač.
    /// </summary>
    private readonly object _Lock = new();

    /// <summary>
    /// Serializuje zpracování příchozích dat.
    /// </summary>
    private readonly object _ReadLock = new();

    private readonly LinkedList<Command> _Queue = new();

    private Command? _Current;

    private Timer? _Timer;

    private string _ReadBuffer = string.Empty;

    public GCodeSerialLink()
    {
        _SerialPort.NewLine = "\n";
        _SerialPort.WriteTimeout = 2000;
        _SerialPort.DataReceived += DataReceivedHandler;
        _SerialPort.ErrorReceived += (sender, e) => Error?.Invoke($"Serial port error: {e.EventType}.");
    }

    /// <summary>
    /// Chyba komunikace nebo hlášení stroje (může přijít z libovolného vlákna).
    /// </summary>
    public event Action<string>? Error;

    /// <summary>
    /// Přijatá pozice stroje (odpověď na M114).
    /// </summary>
    public event Action<decimal, decimal, decimal>? PositionReceived;

    /// <summary>
    /// Linka se sama zavřela kvůli chybě.
    /// </summary>
    public event Action? Closed;

    public bool IsOpen => _SerialPort.IsOpen;

    /// <summary>
    /// Počet příkazů čekajících ve frontě (bez právě odeslaného).
    /// </summary>
    public int QueuedCount
    {
        get { lock (_Lock) return _Queue.Count; }
    }

    public void Open(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
    {
        _SerialPort.PortName = portName;
        _SerialPort.BaudRate = baudRate;
        _SerialPort.Parity = parity;
        _SerialPort.DataBits = dataBits;
        _SerialPort.StopBits = stopBits;

        _SerialPort.Open();
    }

    public void Close()
    {
        lock (_Lock)
        {
            _Queue.Clear();
            _Current = null;
            _Timer?.Dispose();
            _Timer = null;
        }

        lock (_ReadLock)
        {
            _ReadBuffer = string.Empty;
        }

        try
        {
            _SerialPort.Close();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    /// <summary>
    /// Zařadí příkaz do fronty.
    /// </summary>
    /// <param name="line">Řádek bez koncového znaku nového řádku.</param>
    /// <param name="timeoutMs">Jak dlouho se čeká na "ok".</param>
    /// <param name="key">Příkaz se stejným klíčem, který ještě čeká ve frontě, se zahodí a nový se zařadí na konec.</param>
    public void Send(string line, int timeoutMs = DefaultTimeoutMs, string? key = null)
    {
        if (!IsOpen)
            return;

        lock (_Lock)
        {
            if (key != null)
            {
                for (var node = _Queue.First; node != null;)
                {
                    var next = node.Next;

                    if (node.Value.Key == key)
                        _Queue.Remove(node);

                    node = next;
                }
            }

            _Queue.AddLast(new Command(line, timeoutMs, key));
        }

        Pump();
    }

    /// <summary>
    /// Odešle další příkaz z fronty, pokud na žádný nečekáme.
    /// </summary>
    private void Pump()
    {
        Command command;

        lock (_Lock)
        {
            if (_Current != null || _Queue.First == null)
                return;

            command = _Queue.First.Value;
            _Queue.RemoveFirst();
            _Current = command;

            _Timer?.Dispose();
            _Timer = new Timer(_ => OnTimeout(command), null, command.TimeoutMs, Timeout.Infinite);
        }

        try
        {
            Debug.WriteLine($">> {command.Line}");
            _SerialPort.WriteLine(command.Line);
        }
        catch (Exception ex)
        {
            Fail($"Cannot write to {_SerialPort.PortName}: {ex.Message}");
        }
    }

    private void OnTimeout(Command command)
    {
        lock (_Lock)
        {
            if (_Current != command)
                return;

            _Current = null;
        }

        Error?.Invoke($"No response to '{command.Line}' within {command.TimeoutMs / 1000.0:0.#} s.");

        Pump();
    }

    private void OnOk()
    {
        lock (_Lock)
        {
            _Current = null;
            _Timer?.Dispose();
            _Timer = null;
        }

        Pump();
    }

    /// <summary>
    /// Ohlásí chybu a linku zavře (mimo vlákno, které chybu zjistilo, aby nedošlo k zablokování).
    /// </summary>
    private void Fail(string message)
    {
        Error?.Invoke(message);

        Task.Run(() =>
        {
            Close();
            Closed?.Invoke();
        });
    }

    [GeneratedRegex(@"^X:\s*(?<x>-?\d+(?:\.\d+)?)\s+Y:\s*(?<y>-?\d+(?:\.\d+)?)\s+Z:\s*(?<z>-?\d+(?:\.\d+)?)")]
    private static partial Regex PositionRegex();

    private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
    {
        lock (_ReadLock)
        {
            try
            {
                _ReadBuffer += _SerialPort.ReadExisting();
            }
            catch (Exception ex)
            {
                Fail($"Cannot read from {_SerialPort.PortName}: {ex.Message}");
                return;
            }

            int index;

            while ((index = _ReadBuffer.IndexOf('\n')) >= 0)
            {
                var line = _ReadBuffer[..index].Trim();
                _ReadBuffer = _ReadBuffer[(index + 1)..];

                if (line.Length > 0)
                    ProcessLine(line);
            }
        }
    }

    private void ProcessLine(string line)
    {
        Debug.WriteLine($"<< {line}");

        if (line.StartsWith("ok", StringComparison.OrdinalIgnoreCase))
        {
            OnOk();
            return;
        }

        var match = PositionRegex().Match(line);

        if (match.Success
            && decimal.TryParse(match.Groups["x"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var x)
            && decimal.TryParse(match.Groups["y"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var y)
            && decimal.TryParse(match.Groups["z"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var z))
        {
            PositionReceived?.Invoke(x, y, z);
            return;
        }

        if (line.StartsWith("Error", StringComparison.OrdinalIgnoreCase) || line.StartsWith("!!"))
        {
            Error?.Invoke($"Machine: {line}");
        }
    }

    public void Dispose()
    {
        Close();
        _SerialPort.Dispose();
    }
}

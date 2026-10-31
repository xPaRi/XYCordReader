using System.IO.Ports;
using XYCordReader.Models;
using Syncfusion.Data.Extensions;
using Microsoft.Win32;


namespace XYCordReader.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public MainViewModel()
    {
        PortName = PortNameList.LastOrDefault() ?? string.Empty;

        CoordinateAbs.PropertyChanged += (sender, e) => CoordinateRel_Recalculate();
        CoordinateZero.PropertyChanged += (sender, e) => CoordinateRel_Recalculate();

        StoredCoordinatesList = new StoredCoordinatesList(CoordinateZero);

        _SerialLink = new GCodeSerialLink();
        _SerialLink.Error += message => SetStatus(message, true);
        _SerialLink.PositionReceived += SerialLink_PositionReceived;
        _SerialLink.Closed += () => OnUi(() =>
        {
            CoordinateAbs.Clear();
            NotifyConnectionChanged();
        });

        AllowZ = AppRegistry.GetData(nameof(AllowZ), true);
    }

    /// <summary>
    /// Název aplikace
    /// </summary>
    public static string Title => $"{Assembly.GetExecutingAssembly().GetTitle()} ({Assembly.GetExecutingAssembly().GetVersion()})";

    /// <summary>
    /// Sada velikostí kroků a rychlostí
    /// </summary>
    public StepLengthAndSpeedList StepLengthAndSpeedList { get; } = [];

    public RegistryKey AppRegistry => RegistryExt.GetUserAppRegistryKey(Assembly.GetExecutingAssembly());

    #region Serial Communication

    public static int BaudRate => 115200;

    public static Parity Parity => Parity.None;

    public static int DataBits = 8;

    public static StopBits StopBits = StopBits.One;

    /// <summary>
    /// Seznam názvů portů
    /// </summary>
    public IEnumerable<string> PortNameList => SerialPort.GetPortNames().OrderBy(it => it);

    /// <summary>
    /// Aktualizuje seznam portů.
    /// </summary>
    [RelayCommand]
    private void RefreshPortNameList() => OnPropertyChanged(nameof(PortNameList));


    /// <summary>
    /// Přes tuto linku probíhá komunikace se strojem.
    /// </summary>
    private readonly GCodeSerialLink _SerialLink;

    /// <summary>
    /// Aktuálně vybraný port
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ConnectionText))]
    public partial string PortName { get; set; }

    public bool IsConnected => _SerialLink.IsOpen;

    /// <summary>
    /// Popis připojení pro stavový řádek.
    /// </summary>
    public string ConnectionText => IsConnected ? $"{PortName}, {BaudRate} Bd" : "Disconnected";

    private void NotifyConnectionChanged()
    {
        OnPropertyChanged(nameof(IsConnected));
        OnPropertyChanged(nameof(ConnectionText));
    }


    /// <summary>
    /// Připojení k vybranému portu.
    /// </summary>
    [RelayCommand]
    private void OpenPort()
    {
        try
        {
            _SerialLink.Open(PortName, BaudRate, Parity, DataBits, StopBits);

            SetStatus($"Connected to {PortName}.");

            GetCurrentCoordinates();
        }
        catch (Exception ex)
        {
            SetStatus($"Cannot open {PortName}: {ex.Message}", true);
        }

        NotifyConnectionChanged();
    }


    /// <summary>
    /// Odpojení portu.
    /// </summary>
    [RelayCommand]
    private void ClosePort()
    {
        if (!_SerialLink.IsOpen)
            return;

        _SerialLink.Close();

        lock (_ExpectedLock)
        {
            _HasExpected = false;
        }

        CoordinateAbs.Clear();

        NotifyConnectionChanged();

        SetStatus("Disconnected.");
    }

    /// <summary>
    /// Zahoumování (G28).
    /// </summary>
    [RelayCommand]
    private void Homing()
    {
        if (!_SerialLink.IsOpen)
            return;

        lock (_ExpectedLock)
        {
            _HasExpected = false;

            _SerialLink.Send("G28 W", HomingTimeoutMs); //Homing
            _SerialLink.Send("M114", key: SyncKey); //Skutečná pozice po homingu (bez G92, pracuje se se souřadnicemi stroje)
        }
    }

    /// <summary>
    /// Homing může trvat dlouho.
    /// </summary>
    private const int HomingTimeoutMs = 120_000;


    /// <summary>
    /// Získá aktuální souřadnice XYZ (M114).
    /// </summary>
    [RelayCommand]
    private void GetCurrentCoordinates()
    {
        if (!_SerialLink.IsOpen)
            return;

        lock (_ExpectedLock)
        {
            _HasExpected = false; //pozice ze stroje se převezme bez porovnávání

            _SerialLink.Send("M114", key: SyncKey);
        }
    }

    /// <summary>
    /// Klíče příkazů, u kterých má ve frontě zůstat jen ten poslední.
    /// </summary>
    private const string MoveKey = "move";
    private const string SyncKey = "sync";

    /// <summary>
    /// Tolerance při porovnání očekávané a skutečné pozice (M114 vrací dvě desetinná místa).
    /// </summary>
    private const decimal PositionTolerance = 0.011m;

    /// <summary>
    /// Chrání očekávanou pozici a frontu příkazů, aby se odpověď na M114 neporovnala se zastaralou pozicí.
    /// </summary>
    private readonly object _ExpectedLock = new();

    /// <summary>
    /// Pozice, kam se má stroj podle odeslaných příkazů dostat.
    /// </summary>
    private readonly CoorXYZ _Expected = new();

    private bool _HasExpected;

    /// <summary>
    /// Po pohybu počká na dojetí (M400) a zeptá se stroje na skutečnou pozici (M114).
    /// Při dalším pohybu se dotaz přesune na konec fronty, takže se vyhodnotí až po posledním pohybu.
    /// </summary>
    private void RequestPositionCheck()
    {
        _SerialLink.Send("M400", SyncTimeoutMs, SyncKey + "-wait");
        _SerialLink.Send("M114", key: SyncKey);
    }

    private const int SyncTimeoutMs = 60_000;

    private void SerialLink_PositionReceived(decimal x, decimal y, decimal z)
    {
        string? message = null;
        var isError = false;

        lock (_ExpectedLock)
        {
            if (_HasExpected)
            {
                if (_SerialLink.QueuedCount > 0)
                    return; //od odeslání dotazu přibyl další pohyb, odpověď je zastaralá

                _HasExpected = false;

                if (Math.Abs(x - _Expected.X) > PositionTolerance
                    || Math.Abs(y - _Expected.Y) > PositionTolerance
                    || Math.Abs(z - _Expected.Z) > PositionTolerance)
                {
                    message = FormattableString.Invariant(
                        $"Machine is at X{x} Y{y} Z{z}, expected X{_Expected.X} Y{_Expected.Y} Z{_Expected.Z}. Position corrected.");
                    isError = true;
                }
                else
                {
                    message = "Machine position confirmed.";
                }
            }
        }

        OnUi(() =>
        {
            CoordinateAbs.Set(x, y, z);

            if (message != null)
                SetStatus(message, isError);
        });
    }

    #endregion

    #region Status bar

    /// <summary>
    /// Text ve stavovém řádku.
    /// </summary>
    [ObservableProperty]
    public partial string StatusMessage { get; private set; } = "Select a port and press Open.";

    /// <summary>
    /// Stavový řádek ukazuje chybu.
    /// </summary>
    [ObservableProperty]
    public partial bool IsStatusError { get; private set; }

    /// <summary>
    /// Nastaví text stavového řádku (lze volat z libovolného vlákna).
    /// </summary>
    private void SetStatus(string message, bool isError = false)
    {
        Debug.WriteLine($"Status{(isError ? " (error)" : string.Empty)}: {message}");

        OnUi(() =>
        {
            StatusMessage = message;
            IsStatusError = isError;
        });
    }

    private static void OnUi(Action action)
    {
        var dispatcher = System.Windows.Application.Current?.Dispatcher;

        if (dispatcher == null || dispatcher.CheckAccess())
            action();
        else
            dispatcher.BeginInvoke(action);
    }

    #endregion
    
    #region UI and other

    /// <summary>
    /// Povolí nebo zakáže viditelnost ovládání osy Z.
    /// </summary>
    [ObservableProperty]
    public partial bool AllowZ { get; set; }

    partial void OnAllowZChanged(bool value) => AppRegistry.SetData(nameof(AllowZ), value);


    /// <summary>
    /// Ukončení aplikace.
    /// </summary>
    [RelayCommand]
    private static void TryClose() => Application.Current.Shutdown();

    #endregion

    #region Moving Command

    private void MoveXYZ(int directionX, int directionY, int directionZ, string? modifier)
    {
        if (!_SerialLink.IsOpen)
            return;

        var stepLengthAndSpeed = StepLengthAndSpeedList.FirstOrDefault(it => it.ModifierKeys == modifier, StepLengthAndSpeedList[0]);

        CoordinateAbs.SetWithDirection(stepLengthAndSpeed.StepLength, directionX, directionY, directionZ);

        //Při držení klávesy se neposlané pohyby sloučí do jednoho, který míří na poslední cíl
        lock (_ExpectedLock)
        {
            _Expected.Set(CoordinateAbs);
            _HasExpected = true;

            _SerialLink.Send(
                FormattableString.Invariant($"G1 X{CoordinateAbs.X} Y{CoordinateAbs.Y} Z{CoordinateAbs.Z} F{stepLengthAndSpeed.Speed}"),
                key: MoveKey);

            RequestPositionCheck();
        }
    }


    /// <summary>
    /// Goto Selected XY
    /// </summary>
    [RelayCommand]
    private void GotoXY(string? modifier) 
    {
        if (!_SerialLink.IsOpen)
            return;

        var index = StoredCoordinatesSelectedIndex;

        if (index < 0)
            return;

        var coordinates = StoredCoordinatesList[index].AbsCoordinate;

        //Z se záměrně nemění (slouží jen k ostření)
        CoordinateAbs.X = coordinates.X;
        CoordinateAbs.Y = coordinates.Y;

        var stepLengthAndSpeed = StepLengthAndSpeedList.FirstOrDefault(it => it.ModifierKeys == modifier, StepLengthAndSpeedList[0]);

        lock (_ExpectedLock)
        {
            _Expected.Set(CoordinateAbs);
            _HasExpected = true;

            _SerialLink.Send(
                FormattableString.Invariant($"G1 X{coordinates.X} Y{coordinates.Y} F{stepLengthAndSpeed.Speed}"),
                key: MoveKey);

            RequestPositionCheck();
        }
    }


    #endregion

    #region Coordinate Abs

    /// <summary>
    /// Souřadnice stroje.
    /// </summary>
    public CoorXYZ CoordinateAbs { get; } = new();

    /// <summary>
    /// X+
    /// </summary>
    [RelayCommand]
    private void XUp(string? modifier) => MoveXYZ(1, 0, 0, modifier);

    /// <summary>
    /// X-
    /// </summary>
    [RelayCommand]
    private void XDown(string? modifier) => MoveXYZ(-1, 0, 0, modifier);


    /// <summary>
    /// Y+
    /// </summary>
    [RelayCommand]
    private void YUp(string? modifier) => MoveXYZ(0, 1, 0, modifier);

    /// <summary>
    /// Y-
    /// </summary>
    [RelayCommand]
    private void YDown(string? modifier) => MoveXYZ(0, -1, 0, modifier);


    /// <summary>
    /// Z+
    /// </summary>
    [RelayCommand]
    private void ZUp(string? modifier) => MoveXYZ(0, 0, 1, modifier);

    /// <summary>
    /// Z-
    /// </summary>
    [RelayCommand]
    private void ZDown(string? modifier) => MoveXYZ(0, 0, -1, modifier);


    #endregion

    #region Coordinate Zero 

    /// <summary>
    /// Souřadnice nulového bodu pro výpočet relativních souřadnic.
    /// </summary>
    public CoorXYZ CoordinateZero { get; } = new();

    /// <summary>
    /// Nastaví Zero podle X
    /// </summary>
    [RelayCommand]
    private void SetZeroX() => CoordinateZero.X = CoordinateAbs.X;

    /// <summary>
    /// Nastaví Zero podle Y
    /// </summary>
    [RelayCommand]
    private void SetZeroY() => CoordinateZero.Y = CoordinateAbs.Y;

    /// <summary>
    /// Nastaví Zero podle Z
    /// </summary>
    [RelayCommand]
    private void SetZeroZ() => CoordinateZero.Z = CoordinateAbs.Z;

    #endregion

    #region Coordinate Rel

    /// <summary>
    /// Relativní souřadnice přepočtené g4 absolutních souřadnic a souřadnic Zero.
    /// </summary>
    public CoorXYZ CoordinateRel { get; } = new();

    /// <summary>
    /// Přepočítá relativní souřadnice
    /// </summary>
    private void CoordinateRel_Recalculate() => CoordinateRel.SetWithDecrement(CoordinateAbs, CoordinateZero);

    #endregion

    #region Coordinate List Action

    /// <summary>
    /// Aktuálně vybraný řádek v seznamu souřadnic
    /// </summary>
    [ObservableProperty]
    public partial int StoredCoordinatesSelectedIndex { get; set; } = -1;

    /// <summary>
    /// Seznam souřadnic.
    /// </summary>
    public StoredCoordinatesList StoredCoordinatesList { get; }

    private StoredCoordinates GetCoordinate() => new StoredCoordinates(CoordinateAbs, CoordinateZero);

    /// <summary>
    /// Přidá na konec seznamu relativní souřadnice
    /// </summary>
    [RelayCommand]
    private void AddRelCoordinate()
    {
        StoredCoordinatesList.Add(GetCoordinate());

        StoredCoordinatesSelectedIndex = StoredCoordinatesList.Count - 1;
    }

    /// <summary>
    /// Vloží před aktuání pozici kurzoru do seznamu relativní souřadnice
    /// </summary>
    
    [RelayCommand]
    private void InsertRelCoordinate()
    {
        var index = StoredCoordinatesSelectedIndex;

        if (index < 0)
        {
            AddRelCoordinate();
            return;
        }

        StoredCoordinatesList.Insert(index, GetCoordinate());

        StoredCoordinatesSelectedIndex = index;            
    }

    /// <summary>
    /// Odstraní vybrané souřadnice ze seznamu
    /// </summary>
    public void DeleteRelCoordinateSelectedItemsCmd(ObservableCollection<object> storedCoordinates)
    {
        if (storedCoordinates.Count <= 0)
            return;

        var index = StoredCoordinatesSelectedIndex;

        storedCoordinates.Cast<StoredCoordinates>()
            .ToList() //jinak to vyletí na nemožnosti enumerovat odstraňované položky
            .ForEach(it => StoredCoordinatesList.Remove(it));

        if (StoredCoordinatesList.Count <= 0)
            return;

        if (index >= StoredCoordinatesList.Count)
        {
            StoredCoordinatesSelectedIndex = StoredCoordinatesList.Count - 1;
            return;
        }

        if (index < 0)
        {
            StoredCoordinatesSelectedIndex = 0;
            return;
        }

        StoredCoordinatesSelectedIndex = index;
    }

    #endregion

    #region Export

    /// <summary>
    /// Název posledního použitého souboru.
    /// </summary>
    public string LastExportFile
    {
        get => AppRegistry.GetData(nameof(LastExportFile), "data.csv");
        set => AppRegistry.SetData(nameof(LastExportFile), value);
    }

    /// <summary>
    /// Uloží vše do souboru.
    /// </summary>
    /// <param name="fileName"></param>
    public void ExportAll(string fileName)
    {
        LastExportFile = fileName;

        StoredCoordinatesList.ExportAll(fileName);
    }

    /// <summary>
    /// Uloží pouze relativní data do souboru.
    /// </summary>
    /// <param name="fileName"></param>
    public void ExportRelative(string fileName)
    {
        LastExportFile = fileName;

        StoredCoordinatesList.ExportRelative(fileName, AllowZ);
    }

    #endregion
}

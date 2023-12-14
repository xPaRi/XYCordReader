using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Windows.Input;
using XYCordReader.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;


namespace XYCordReader.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Na tomto portu bude probíhat komunikace
        /// </summary>
        private static SerialPort? _SerialPort = null;

        public MainViewModel() 
        {
            _PortName = PortNameList.Last().ToString();
        }

        #region Helpers

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Serial Reader

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            var serialPort = (SerialPort)sender;
            var indata = serialPort.ReadExisting();

            if (indata != null)
            {
                Debug.WriteLine(indata);

                if (indata.StartsWith("X:"))
                {
                    //X:0.00 Y:0.00 Z:0.00 E:0.00 Count X:1400 Y:-400 Z:800
                    var aValues = indata.Split(new char[] { ' ', ':', 'X', 'Y', 'Z', 'E' }, StringSplitOptions.RemoveEmptyEntries);

                    if (decimal.TryParse(aValues[0], CultureInfo.InvariantCulture, out var x)
                        && decimal.TryParse(aValues[1], CultureInfo.InvariantCulture, out var y)
                        && decimal.TryParse(aValues[2], CultureInfo.InvariantCulture, out var z))
                    {
                        CurrentX = x;
                        CurrentY = y;
                        CurrentZ = z;
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Název aplikace
        /// </summary>
        public static string Title => "XY CORD READER";

        public static int BaudRate => 115200;

        public static Parity Parity => Parity.None;

        public static int DataBits = 8;

        public static StopBits StopBits = StopBits.One;

        /// <summary>
        /// Seznam názvů portů
        /// </summary>
        public IEnumerable<string> PortNameList => SerialPort.GetPortNames().OrderBy(it=>it);

        /// <summary>
        /// Aktuální krok
        /// </summary>
        public decimal CurrentStep
        {
            get => _CurrentStep;
            set
            {
                if (value == _CurrentStep) 
                    return;

                _CurrentStep = value;

                OnPropertyChanged(nameof(CurrentStep));
            }
        }

        private decimal _CurrentStep = 1;

        /// <summary>
        /// Aktuální rychlost
        /// </summary>
        public decimal CurrentSpeed
        {
            get => _CurrentSpeed;
            set
            {
                if (value == _CurrentSpeed) 
                    return;

                _CurrentSpeed = value;

                OnPropertyChanged(nameof(CurrentSpeed));
            }
        }

        private decimal _CurrentSpeed = 2000;

        /// <summary>
        /// Aktuální souřadnice X
        /// </summary>
        public decimal CurrentX
        { 
            get => _CurrentX;
            set 
            {
                if (_CurrentX.Equals(value))
                    return;

                if (value < 0)
                    value = 0;

                _CurrentX = Math.Round(value, 2);

                OnPropertyChanged(nameof(CurrentX));
            }
        }

        private decimal _CurrentX = 0;

        /// <summary>
        /// Aktuální souřadnice Y
        /// </summary>
        public decimal CurrentY
        { 
            get => _CurrentY;
            set 
            {
                if (_CurrentY.Equals(value))
                    return;

                if (value < 0)
                    value = 0;

                _CurrentY = Math.Round(value, 2);

                OnPropertyChanged(nameof(CurrentY));
            }
        }

        private decimal _CurrentY = 0;

        /// <summary>
        /// Aktuální souřadnice Z
        /// </summary>
        public decimal CurrentZ 
        { 
            get => _CurrentZ;
            set 
            {
                if (_CurrentZ.Equals(value))
                    return;

                if (value < 0)
                    value = 0;

                _CurrentZ = Math.Round(value, 2);

                OnPropertyChanged(nameof(CurrentZ));
            }
        }

        private decimal _CurrentZ = 0;


        /// <summary>
        /// Aktuálně vybraný port
        /// </summary>
        public string PortName
        {
            get => _PortName;
            set
            {
                if (value == _PortName) return;
                
                _PortName = value;

                OnPropertyChanged(nameof(PortName));
            }
        }

        private string _PortName;

        public bool IsConnected => (_SerialPort != null && _SerialPort.IsOpen);

        #region Action

        /// <summary>
        /// Aktualizuje seznam portů.
        /// </summary>
        public ICommand RefreshPortNameList => _RefreshPortNameList ??= new CommandHandler(RefreshPortListCmd, true);

        private ICommand? _RefreshPortNameList;

        private void RefreshPortListCmd() => OnPropertyChanged(nameof(PortNameList));


        /// <summary>
        /// Ukončení aplikace.
        /// </summary>
        public ICommand TryClose => _TryClose ??= new CommandHandler(TryCloseCmd, true);

        private ICommand? _TryClose;

        private void TryCloseCmd() => System.Windows.Application.Current.Shutdown();

        /// <summary>
        /// Připojení k vybranému portu.
        /// </summary>
        public ICommand ConnectPort => _ConnectPort ??= new CommandHandler(ConnectPortCmd, true);

        private ICommand? _ConnectPort;

        private void ConnectPortCmd() 
        {
            _SerialPort = new SerialPort(PortName, BaudRate, Parity, DataBits, StopBits);
            _SerialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

            try
            {
                _SerialPort.Open();

            }
            catch (Exception ex)
            { 
                Debug.WriteLine(ex.Message);
            }

            OnPropertyChanged(nameof(IsConnected));
        }

        /// <summary>
        /// Odpojení portu.
        /// </summary>
        public ICommand DisConnectPort => _DisConnectPort ??= new CommandHandler(DisConnectPortCmd, true);

        private ICommand? _DisConnectPort;

        private void DisConnectPortCmd() 
        {
            if (_SerialPort == null || !_SerialPort.IsOpen) 
                return;

            _SerialPort.Close();

            _SerialPort.DataReceived -= new SerialDataReceivedEventHandler(DataReceivedHandler);

            OnPropertyChanged(nameof(IsConnected));
        }

        /// <summary>
        /// Zahoumování (G28).
        /// </summary>
        public ICommand Homing => _Homing ??= new CommandHandler(HomingCmd, true);

        private ICommand? _Homing;

        private void HomingCmd() 
        {
            if (_SerialPort == null || !_SerialPort.IsOpen) 
                return;

            _SerialPort.WriteLine("G28 W"); //Homing
            _SerialPort.WriteLine("G92 X0 Y0 Z0"); //Set zero position

            CurrentX = 0;
            CurrentY = 0;
            CurrentZ = 0;
        }

        /// <summary>
        /// Získá aktuální souřadnice XYZ (M114).
        /// </summary>
        public ICommand GetCurrentXYZ => _GetCurrentXYZ ??= new CommandHandler(GetCurrentXYZCmd, true);

        private ICommand? _GetCurrentXYZ;

        private void GetCurrentXYZCmd()
        {
            if (_SerialPort == null || !_SerialPort.IsOpen)
                return;

            _SerialPort.WriteLine("M114");
        }


        #endregion

        #region Moving Command

        private void MoveXYZ(int setX, int setY, int setZ)
        {
            if (_SerialPort == null || !_SerialPort.IsOpen)
                return;

            CurrentX += setX * CurrentStep;
            CurrentY += setY * CurrentStep;
            CurrentZ += setZ * CurrentStep;

            _SerialPort.WriteLine($"G1 X{CurrentX} Y{CurrentY} Z{CurrentZ} F{CurrentSpeed}");
        }

        /// <summary>
        /// X+
        /// </summary>
        public ICommand XUp => _XUp ??= new CommandHandler(XUpCmd, true);

        private ICommand? _XUp;

        private void XUpCmd() => MoveXYZ(1, 0, 0);

        /// <summary>
        /// X-
        /// </summary>
        public ICommand XDown => _XDown ??= new CommandHandler(XDownCmd, true);

        private ICommand? _XDown;

        private void XDownCmd() => MoveXYZ(-1, 0, 0);


        /// <summary>
        /// Y+
        /// </summary>
        public ICommand YUp => _YUp ??= new CommandHandler(YUpCmd, true);

        private ICommand? _YUp;

        private void YUpCmd() => MoveXYZ(0, 1, 0);

        /// <summary>
        /// Y-
        /// </summary>
        public ICommand YDown => _YDown ??= new CommandHandler(YDownCmd, true);

        private ICommand? _YDown;

        private void YDownCmd() => MoveXYZ(0, -1, 0);


        /// <summary>
        /// Z+
        /// </summary>
        public ICommand ZUp => _ZUp ??= new CommandHandler(ZUpCmd, true);

        private ICommand? _ZUp;

        private void ZUpCmd() => MoveXYZ(0, 0, 1);

        /// <summary>
        /// Z-
        /// </summary>
        public ICommand ZDown => _ZDown ??= new CommandHandler(ZDownCmd, true);

        private ICommand? _ZDown;

        private void ZDownCmd() => MoveXYZ(0, 0, -1);

        #endregion



    }
}

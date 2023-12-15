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

        /// <summary>
        /// Oddělovače pro čtení dat zaslaných seriovou linkou...
        /// </summary>
        private static readonly char[] SeparatorList = [' ', ':', 'X', 'Y', 'Z', 'E'];

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
                    var aValues = indata.Split(SeparatorList, StringSplitOptions.RemoveEmptyEntries);

                    if (aValues.Length >= 3
                        && decimal.TryParse(aValues[0], CultureInfo.InvariantCulture, out var x)
                        && decimal.TryParse(aValues[1], CultureInfo.InvariantCulture, out var y)
                        && decimal.TryParse(aValues[2], CultureInfo.InvariantCulture, out var z))
                    {
                        CurrentXYZ.Set(x, y, z);
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Název aplikace
        /// </summary>
        public static string Title => "XYZ CORD READER";

        public static int BaudRate => 115200;

        public static Parity Parity => Parity.None;

        public static int DataBits = 8;

        public static StopBits StopBits = StopBits.One;

        /// <summary>
        /// Seznam názvů portů
        /// </summary>
        public IEnumerable<string> PortNameList => SerialPort.GetPortNames().OrderBy(it=>it);

        /// <summary>
        /// Sada velikostí kroků a rychlostí
        /// </summary>
        public StepLengthAndSpeedList StepLengthAndSpeedList
        {
            get => _StepLengthAndSpeedList;
            set => _StepLengthAndSpeedList = value;
        }

        private StepLengthAndSpeedList _StepLengthAndSpeedList = new();

        /// <summary>
        /// Aktuální souřadnice stroje
        /// </summary>
        public Coordinate CurrentXYZ
        { 
            get => _CurrentXYZ;
            set => _CurrentXYZ = value;
        }

        private Coordinate _CurrentXYZ = new();

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

        /// <summary>
        /// Povolí nebo zakáže viditelnost ovládání osy Z.
        /// </summary>
        public bool AllowZ
        {
            get => _AllowZ;
            set
            {
                if (_AllowZ == value)
                    return;

                _AllowZ = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AllowZ)));
            }
        }

        private bool _AllowZ = false;


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

            CurrentXYZ.Homing();
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

        #region Set Zero

        /// <summary>
        /// Nastaví Zero podle X
        /// </summary>
        public ICommand SetZeroX => _SetZeroX ??= new CommandHandler(SetZeroXCmd, true);

        private ICommand? _SetZeroX;

        private void SetZeroXCmd() => CurrentXYZ.SetZeroByAbsX();

        /// <summary>
        /// Nastaví Zero podle Y
        /// </summary>
        public ICommand SetZeroY => _SetZeroY ??= new CommandHandler(SetZeroYCmd, true);

        private ICommand? _SetZeroY;

        private void SetZeroYCmd() => CurrentXYZ.SetZeroByAbsY();

        /// <summary>
        /// Nastaví Zero podle Z
        /// </summary>
        public ICommand SetZeroZ => _SetZeroZ ??= new CommandHandler(SetZeroZCmd, true);

        private ICommand? _SetZeroZ;

        private void SetZeroZCmd() => CurrentXYZ.SetZeroByAbsZ();

        /// <summary>
        /// Nastaví Zero podle XYZ
        /// </summary>
        public ICommand SetZeroXYZ => _SetZeroXYZ ??= new CommandHandler(SetZeroXYZCmd, true);

        private ICommand? _SetZeroXYZ;

        private void SetZeroXYZCmd() => CurrentXYZ.SetZeroByAbsXYZ();

        /// <summary>
        /// Nastaví Zero podle XY
        /// </summary>
        public ICommand SetZeroXY => _SetZeroXY ??= new CommandHandler(SetZeroXYCmd, true);

        private ICommand? _SetZeroXY;

        private void SetZeroXYCmd() => CurrentXYZ.SetZeroByAbsXY();

        #endregion

        #region Moving Command

        private void MoveXYZ(int directionX, int directionY, int directionZ, ModifierKeys modifier)
        {
            if (_SerialPort == null || !_SerialPort.IsOpen)
                return;

            var stepLengthAndSpeed = StepLengthAndSpeedList.FirstOrDefault(it=>it.ModifierKeys == modifier, StepLengthAndSpeedList[0]);

            CurrentXYZ.Add(stepLengthAndSpeed, directionX, directionY, directionZ);

            //Jen pro ladění, ať se futr nejezdí s tiskárnou
            _SerialPort.WriteLine($"G1 X{CurrentXYZ.AbsX} Y{CurrentXYZ.AbsY} Z{CurrentXYZ.AbsZ} F{stepLengthAndSpeed.Speed}");
        }

        /// <summary>
        /// X+
        /// </summary>
        public ICommand XUp => _XUp ??= new CommandHandlerWithModifiers(modifier => XUpCmd(modifier), true);

        private ICommand? _XUp;

        private void XUpCmd(ModifierKeys modifier) => MoveXYZ(1, 0, 0, modifier);

        /// <summary>
        /// X-
        /// </summary>
        public ICommand XDown => _XDown ??= new CommandHandlerWithModifiers(modifier => XDownCmd(modifier), true);

        private ICommand? _XDown;

        private void XDownCmd(ModifierKeys modifier) => MoveXYZ(-1, 0, 0, modifier);


        /// <summary>
        /// Y+
        /// </summary>
        public ICommand YUp => _YUp ??= new CommandHandlerWithModifiers(modifier => YUpCmd(modifier), true);

        private ICommand? _YUp;

        private void YUpCmd(ModifierKeys modifier) => MoveXYZ(0, 1, 0, modifier);

        /// <summary>
        /// Y-
        /// </summary>
        public ICommand YDown => _YDown ??= new CommandHandlerWithModifiers(modifier => YDownCmd(modifier), true);

        private ICommand? _YDown;

        private void YDownCmd(ModifierKeys modifier) => MoveXYZ(0, -1, 0, modifier);


        /// <summary>
        /// Z+
        /// </summary>
        public ICommand ZUp => _ZUp ??= new CommandHandlerWithModifiers(modifier => ZUpCmd(modifier), true);

        private ICommand? _ZUp;

        private void ZUpCmd(ModifierKeys modifier) => MoveXYZ(0, 0, 1, modifier);

        /// <summary>
        /// Z-
        /// </summary>
        public ICommand ZDown => _ZDown ??= new CommandHandlerWithModifiers(modifier => ZDownCmd(modifier), true);

        private ICommand? _ZDown;

        private void ZDownCmd(ModifierKeys modifier) => MoveXYZ(0, 0, -1, modifier);

        #endregion

        #region Coordinate List Action

        /// <summary>
        /// Přidá na konec seznamu relativní souřadnice
        /// </summary>
        public ICommand AddRelCoordinate => _AddRelCoordinate ??= new CommandHandlerWithModifiers(modifier => AddRelCoordinateCmd(modifier), true);

        private ICommand? _AddRelCoordinate;

        private void AddRelCoordinateCmd(ModifierKeys modifier) => MoveXYZ(1, 0, 0, modifier);

        /// <summary>
        /// Vloží před aktuání pozici kurzoru do seznamu relativní souřadnice
        /// </summary>
        public ICommand InsertRelCoordinate => _InsertRelCoordinate ??= new CommandHandlerWithModifiers(modifier => InsertRelCoordinateCmd(modifier), true);

        private ICommand? _InsertRelCoordinate;

        private void InsertRelCoordinateCmd(ModifierKeys modifier) => MoveXYZ(1, 0, 0, modifier);

        /// <summary>
        /// Odstraní vybrané souřadnice ze seznamu
        /// </summary>
        public ICommand DeleteRelCoordinate => _DeleteRelCoordinate ??= new CommandHandlerWithModifiers(modifier => DeleteRelCoordinateCmd(modifier), true);

        private ICommand? _DeleteRelCoordinate;

        private void DeleteRelCoordinateCmd(ModifierKeys modifier) => MoveXYZ(1, 0, 0, modifier);


        #endregion

    }
}

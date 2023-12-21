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
using System.Collections.ObjectModel;
using Syncfusion.Data.Extensions;


namespace XYCordReader.ViewModels
{
    public class MainViewModel : NotifyPropertyChangedBase
    {
        public MainViewModel() 
        {
            _PortName = PortNameList.Last().ToString();

            _CoordinateAbs.PropertyChanged += (sender, e) => CoordinateRel_Recalculate();
            _CoordinateZero.PropertyChanged += (sender, e) => CoordinateRel_Recalculate();

            _StoredCoordinatesList = new StoredCoordinatesList(CoordinateZero);

            _SerialPort = new SerialPort();
            _SerialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
        }

        /// <summary>
        /// Název aplikace
        /// </summary>
        public static string Title => "XY CORD READER";

        /// <summary>
        /// Sada velikostí kroků a rychlostí
        /// </summary>
        public StepLengthAndSpeedList StepLengthAndSpeedList
        {
            get => _StepLengthAndSpeedList;
            set => _StepLengthAndSpeedList = value;
        }

        private StepLengthAndSpeedList _StepLengthAndSpeedList = new();


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
        public ICommand RefreshPortNameList => _RefreshPortNameList ??= new CommandHandler(RefreshPortListCmd, true);

        private ICommand? _RefreshPortNameList;
        private void RefreshPortListCmd() => OnPropertyChanged(nameof(PortNameList));


        /// <summary>
        /// Na tomto portu bude probíhat komunikace
        /// </summary>
        private readonly SerialPort _SerialPort;

        /// <summary>
        /// Aktuálně vybraný port
        /// </summary>
        public string PortName
        {
            get => _PortName;
            set => SetValue(ref _PortName, value);
        }
        
        private string _PortName;

        public bool IsConnected => _SerialPort.IsOpen;


        /// <summary>
        /// Připojení k vybranému portu.
        /// </summary>
        public ICommand OpenPort => _OpenPort ??= new CommandHandler(OpenPortCmd, true);

        private ICommand? _OpenPort;

        private void OpenPortCmd()
        {
            _SerialPort.PortName = PortName;
            _SerialPort.BaudRate = BaudRate;
            _SerialPort.Parity = Parity;
            _SerialPort.DataBits = DataBits;
            _SerialPort.StopBits = StopBits;
            
            try
            {
                _SerialPort.Open();

                GetCurrentCoordinatesCmd();
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
        public ICommand ClosePort => _ClosePort ??= new CommandHandler(ClosePortCmd, true);

        private ICommand? _ClosePort;
        private void ClosePortCmd()
        {
            if (_SerialPort == null || !_SerialPort.IsOpen)
                return;

            _SerialPort.Close();

            CoordinateAbs.Clear();

            OnPropertyChanged(nameof(IsConnected));
        }

        /// <summary>
        /// Zahoumování (G28).
        /// </summary>
        public ICommand Homing => _Homing ??= new CommandHandler(HomingCmd, true);

        private ICommand? _Homing;

        private void HomingCmd()
        {
            if (!_SerialPort.IsOpen)
                return;

            _SerialPort.WriteLine("G28 W"); //Homing
            _SerialPort.WriteLine("G92 X0 Y0 Z0"); //Set zero position

            CoordinateAbs.Clear();
        }


        /// <summary>
        /// Získá aktuální souřadnice XYZ (M114).
        /// </summary>
        public ICommand GetCurrentCoordinates => _GetCurrentCoordinates ??= new CommandHandler(GetCurrentCoordinatesCmd, true);

        private ICommand? _GetCurrentCoordinates;

        private void GetCurrentCoordinatesCmd()
        {
            if (!_SerialPort.IsOpen)
                return;

            _SerialPort.WriteLine("M114");
        }

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
                        CoordinateAbs.Set(x, y, z);
                    }
                }
            }
        }

        #endregion

        #region UI and other

        /// <summary>
        /// Povolí nebo zakáže viditelnost ovládání osy Z.
        /// </summary>
        public bool AllowZ
        {
            get => _AllowZ;
            set => SetValue(ref _AllowZ, value);
        }

        private bool _AllowZ = true;


        /// <summary>
        /// Ukončení aplikace.
        /// </summary>
        public ICommand TryClose => _TryClose ??= new CommandHandler(TryCloseCmd, true);

        private ICommand? _TryClose;

        private void TryCloseCmd() => System.Windows.Application.Current.Shutdown();

        #endregion

        #region Moving Command

        private void MoveXYZ(int directionX, int directionY, int directionZ, string modifier)
        {
            if (!_SerialPort.IsOpen)
                return;

            var stepLengthAndSpeed = StepLengthAndSpeedList.FirstOrDefault(it => it.ModifierKeys == modifier, StepLengthAndSpeedList[0]);

            CoordinateAbs.SetWithDirection(stepLengthAndSpeed.StepLength, directionX, directionY, directionZ);

            _SerialPort.WriteLine($"G1 X{CoordinateAbs.X} Y{CoordinateAbs.Y} Z{CoordinateAbs.Z} F{stepLengthAndSpeed.Speed}");
        }


        /// <summary>
        /// Goto Selected XY
        /// </summary>
        public ICommand GotoXY => _GotoXY ??= new CommandHandlerWithModifiers(modifier => GotoXYCmd(modifier), true);

        private ICommand? _GotoXY;

        private void GotoXYCmd(string modifier) 
        {
            if (!_SerialPort.IsOpen)
                return;

            var index = StoredCoordinatesSelectedIndex;

            if (index < 0) 
                return;

            var coordinates = StoredCoordinatesList[index].AbsCoordinate;

            CoordinateAbs.Set(coordinates);

            var stepLengthAndSpeed = StepLengthAndSpeedList.FirstOrDefault(it => it.ModifierKeys == modifier, StepLengthAndSpeedList[0]);

            _SerialPort.WriteLine($"G1 X{coordinates.X} Y{coordinates.Y} F{stepLengthAndSpeed.Speed}");
        }


        #endregion

        #region Coordinate Abs

        /// <summary>
        /// Souřadnice stroje.
        /// </summary>
        public CoorXYZ CoordinateAbs
        {
            get => _CoordinateAbs;
        }

        private CoorXYZ _CoordinateAbs = new CoorXYZ(true);

        /// <summary>
        /// X+
        /// </summary>
        public ICommand XUp => _XUp ??= new CommandHandlerWithModifiers(modifier => XUpCmd(modifier), true);

        private ICommand? _XUp;

        private void XUpCmd(string modifier) => MoveXYZ(1, 0, 0, modifier);

        /// <summary>
        /// X-
        /// </summary>
        public ICommand XDown => _XDown ??= new CommandHandlerWithModifiers(modifier => XDownCmd(modifier), true);

        private ICommand? _XDown;

        private void XDownCmd(string modifier) => MoveXYZ(-1, 0, 0, modifier);


        /// <summary>
        /// Y+
        /// </summary>
        public ICommand YUp => _YUp ??= new CommandHandlerWithModifiers(modifier => YUpCmd(modifier), true);

        private ICommand? _YUp;

        private void YUpCmd(string modifier) => MoveXYZ(0, 1, 0, modifier);

        /// <summary>
        /// Y-
        /// </summary>
        public ICommand YDown => _YDown ??= new CommandHandlerWithModifiers(modifier => YDownCmd(modifier), true);

        private ICommand? _YDown;

        private void YDownCmd(string modifier) => MoveXYZ(0, -1, 0, modifier);


        /// <summary>
        /// Z+
        /// </summary>
        public ICommand ZUp => _ZUp ??= new CommandHandlerWithModifiers(modifier => ZUpCmd(modifier), true);

        private ICommand? _ZUp;

        private void ZUpCmd(string modifier) => MoveXYZ(0, 0, 1, modifier);

        /// <summary>
        /// Z-
        /// </summary>
        public ICommand ZDown => _ZDown ??= new CommandHandlerWithModifiers(modifier => ZDownCmd(modifier), true);

        private ICommand? _ZDown;

        private void ZDownCmd(string modifier) => MoveXYZ(0, 0, -1, modifier);


        #endregion

        #region Coordinate Zero 

        /// <summary>
        /// Souřadnice nulového bodu pro výpočet relativních souřadnic.
        /// </summary>
        public CoorXYZ CoordinateZero
        {
            get => _CoordinateZero;
        }

        private CoorXYZ _CoordinateZero = new();

        /// <summary>
        /// Nastaví Zero podle X
        /// </summary>
        public ICommand SetZeroX => _SetZeroX ??= new CommandHandler(SetZeroXCmd, true);

        private ICommand? _SetZeroX;

        private void SetZeroXCmd() => CoordinateZero.X = CoordinateAbs.X;

        /// <summary>
        /// Nastaví Zero podle Y
        /// </summary>
        public ICommand SetZeroY => _SetZeroY ??= new CommandHandler(SetZeroYCmd, true);

        private ICommand? _SetZeroY;

        private void SetZeroYCmd() => CoordinateZero.Y = CoordinateAbs.Y;

        /// <summary>
        /// Nastaví Zero podle Z
        /// </summary>
        public ICommand SetZeroZ => _SetZeroZ ??= new CommandHandler(SetZeroZCmd, true);

        private ICommand? _SetZeroZ;
        private void SetZeroZCmd() => CoordinateZero.Z = CoordinateAbs.Z;

        #endregion

        #region Coordinate Rel

        /// <summary>
        /// Relativní souřadnice přepočtené g4 absolutních souřadnic a souřadnic Zero.
        /// </summary>
        public CoorXYZ CoordinateRel
        {
            get => _CoordinateRel;
        }

        private CoorXYZ _CoordinateRel = new();

        /// <summary>
        /// Přepočítá relativní souřadnice
        /// </summary>
        private void CoordinateRel_Recalculate() => CoordinateRel.SetWithDecrement(CoordinateAbs, CoordinateZero);

        #endregion

        #region Coordinate List Action

        /// <summary>
        /// Aktuálně vybraný řádek v seznamu souřadnic
        /// </summary>
        public int StoredCoordinatesSelectedIndex
        {
            get => _StoredCoordinatesSelectedIndex;
            set => SetValue(ref _StoredCoordinatesSelectedIndex, value);
        }

        private int _StoredCoordinatesSelectedIndex = -1;

        /// <summary>
        /// Seznam souřadnic.
        /// </summary>
        public StoredCoordinatesList StoredCoordinatesList
        {
            get => _StoredCoordinatesList;
            set
            {
                if (_StoredCoordinatesList == value)
                    return;

                _StoredCoordinatesList = value;

                OnPropertyChanged();
            }
        }

        private StoredCoordinatesList _StoredCoordinatesList;

        /// <summary>
        /// Seznam vybraných souřadnic.
        /// </summary>
        public StoredCoordinatesList StoredCoordinatesSelection
        {
            get => _StoredCoordinatesSelection;
            set
            {
                _StoredCoordinatesSelection = value;

                Debug.WriteLine($"Selected: {_StoredCoordinatesSelection.Count}");

                OnPropertyChanged();
            }
        }

        private StoredCoordinatesList _StoredCoordinatesSelection = new StoredCoordinatesList(new CoorXYZ());


        private StoredCoordinates GetCoordinate()
        {
            return new StoredCoordinates(CoordinateAbs, CoordinateZero);
        }

        /// <summary>
        /// Přidá na konec seznamu relativní souřadnice
        /// </summary>
        public ICommand AddRelCoordinate => _AddRelCoordinate ??= new CommandHandler(AddRelCoordinateCmd, true);

        private ICommand? _AddRelCoordinate;

        private void AddRelCoordinateCmd()
        {
            StoredCoordinatesList.AddCoordinates(GetCoordinate());

            StoredCoordinatesSelectedIndex = StoredCoordinatesList.Count - 1;
        }

        /// <summary>
        /// Vloží před aktuání pozici kurzoru do seznamu relativní souřadnice
        /// </summary>
        public ICommand InsertRelCoordinate => _InsertRelCoordinate ??= new CommandHandler(InsertRelCoordinateCmd, true);

        private ICommand? _InsertRelCoordinate;
        
        public void InsertRelCoordinateCmd()
        {
            var index = StoredCoordinatesSelectedIndex;

            if (index < 0)
            {
                AddRelCoordinateCmd();
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

    }
}

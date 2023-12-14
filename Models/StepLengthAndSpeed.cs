using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace XYCordReader.Models
{
    /// <summary>
    /// Nastavení kroku a rychlosti.
    /// </summary>
    public class StepLengthAndSpeed : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public StepLengthAndSpeed(ModifierKeys modifierKeys, decimal stepLength, int speed)
        {
			_ModifierKeys = modifierKeys;
            _StepLength = stepLength;
            _Speed = speed;
        }

		public string ModifierKeysName => $"{_ModifierKeys}";

        /// <summary>
        /// Název sady.
        /// </summary>
        public ModifierKeys ModifierKeys
		{
			get => _ModifierKeys;
			set
			{
				if (_ModifierKeys == value)
					return;

				_ModifierKeys = value;

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ModifierKeys)));
			}
		}

		private ModifierKeys _ModifierKeys;


		/// <summary>
		/// Počet kroků.
		/// </summary>
		public decimal StepLength
		{
			get => _StepLength;
			set
			{
				if (_StepLength == value)
					return;

				_StepLength = value;

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StepLength)));
			}
		}

		private decimal _StepLength;


		/// <summary>
		/// Rychlost.
		/// </summary>
		public int Speed
		{
			get => _Speed;
			set
			{
				if (_Speed == value)
					return;

				_Speed = value;

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Speed)));
			}
		}

		private int _Speed;

        public override string ToString() => $"{nameof(ModifierKeys)}={ModifierKeys}; {nameof(StepLength)}={StepLength}; {nameof(Speed)}={Speed}";
    }
}

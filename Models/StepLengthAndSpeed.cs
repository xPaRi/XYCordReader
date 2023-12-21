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
    public class StepLengthAndSpeed : NotifyPropertyChangedBase
    {
        public StepLengthAndSpeed(string modifierKeys, decimal stepLength, int speed)
        {
			_ModifierKeys = modifierKeys;
            _StepLength = stepLength;
            _Speed = speed;
        }

		public string ModifierKeysName => $"{_ModifierKeys}";

        /// <summary>
        /// Název sady.
        /// </summary>
        public string ModifierKeys
		{
			get => _ModifierKeys;
			set
			{
				if (_ModifierKeys == value)
					return;

				_ModifierKeys = value;

				OnPropertyChanged();
			}
		}

		private string _ModifierKeys;


		/// <summary>
		/// Počet kroků.
		/// </summary>
		public decimal StepLength
		{
			get => _StepLength;
			set => SetValue(ref _StepLength, value);
		}

		private decimal _StepLength;


		/// <summary>
		/// Rychlost.
		/// </summary>
		public int Speed
		{
			get => _Speed;
            set => SetValue(ref _Speed, value);
        }

        private int _Speed;

        public override string ToString() => $"{nameof(ModifierKeys)}={ModifierKeys}; {nameof(StepLength)}={StepLength}; {nameof(Speed)}={Speed}";
    }
}

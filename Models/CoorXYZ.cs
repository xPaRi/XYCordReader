using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYCordReader.Models
{
	/// <summary>
	/// Třída pro uchování souřadnic X, Y, Z
	/// </summary>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <param name="z"></param>
    public class CoorXYZ : NotifyPropertyChangedBase
    {
        #region Constructor

        public CoorXYZ(bool onlyPositiveValues = false)
        {
            _OnlyPositiveValues = onlyPositiveValues;

            X = 0;
            Y = 0;
            Z = 0;
        }

        public CoorXYZ(CoorXYZ coorXYZ, bool onlyPositiveValues = false)
        {
            _OnlyPositiveValues = onlyPositiveValues;

            X = coorXYZ.X;
            Y = coorXYZ.Y;
            Z = coorXYZ.Z;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Indikuje, že hodnoty budou na vstupu zarovnány na kladné hodnoty
        /// </summary>
        public bool OnlyPositiveValues
        {
            get => _OnlyPositiveValues;
            set => SetValue(ref _OnlyPositiveValues, value);
        }

		private bool _OnlyPositiveValues = false;

        /// <summary>
        /// Souřadnice X.
        /// </summary>
        public decimal X
        {
            get => _X;
            set
            {
                if (_OnlyPositiveValues && value < 0)
                {
                    value = 0;
                }

                SetValue(ref _X, value);
            }
        }

        private decimal _X;

		/// <summary>
		/// Souřadnice Y.
		/// </summary>
		public decimal Y
        {
            get => _Y;
            set
            {
                if (_OnlyPositiveValues && value < 0)
                {
                    value = 0;
                }

                SetValue(ref _Y, value);
            }
        }

        private decimal _Y;

		/// <summary>
		/// Souřadnice Z.
		/// </summary>
		public decimal Z
        {
            get => _Z;
            set
            {
                if (_OnlyPositiveValues && value < 0)
                {
                    value = 0;
                }

                SetValue(ref _Z, value);
            }
        }

        private decimal _Z;

        #endregion

        #region Methods

        /// <summary>
        /// Nastaví vše na nulu.
        /// </summary>
        public void Clear() => Set(0, 0, 0);


        /// <summary>
        /// Nastaví souřadnice podle X, Y, Z
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void Set(decimal x, decimal y, decimal z)
		{
			X = x;
			Y = y;	
			Z = z;
		}

		/// <summary>
		/// Nastaví souřadnice podle zadaného objektu.
		/// </summary>
		/// <param name="coorXYZ"></param>
		public void Set(CoorXYZ coorXYZ)
		{
			X = coorXYZ.X;
			Y = coorXYZ.Y;
			Z = coorXYZ.Z;
		}

		/// <summary>
		/// Nastaví souřadnice podle zadaného objektu a odečte od nich druhé souřadnice.
		/// </summary>
		/// <param name="coorXYZ">Základní souřadnice.</param>
		/// <param name="xyzDecrement">Odečítací souřadnice.</param>
		public void SetWithDecrement(CoorXYZ coorXYZ, CoorXYZ xyzDecrement)
		{
			X = coorXYZ.X - xyzDecrement.X;
			Y = coorXYZ.Y - xyzDecrement.Y;
			Z = coorXYZ.Z - xyzDecrement.Z;
		}

        /// <summary>
		/// Přičte hodnotu delta do příslušných souřadnic podle zadaného směru. 
        /// </summary>
        /// <param name="delta"></param>
        /// <param name="directionX"></param>
        /// <param name="directionY"></param>
        /// <param name="directionZ"></param>
        internal void SetWithDirection(decimal delta, int directionX, int directionY, int directionZ)
        {
            X += directionX * delta;
            Y += directionY * delta;
			Z += directionZ * delta;
        }

        public override string ToString() => $"[X={X}; Y={Y}; Z={Z}]";

        #endregion
    }
}

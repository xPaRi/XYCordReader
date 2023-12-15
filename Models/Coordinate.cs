using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYCordReader.Models
{
    public class Coordinate : INotifyPropertyChanged
    {
        #region PropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Absolute XYZ

        /// <summary>
        /// Absolutní souřadnice X.
        /// </summary>
        public decimal AbsX
		{
			get => _AbsX;
			set
			{
				if (value < 0)
                    value = 0;

				if (_AbsX == value)
					return;

				_AbsX = value;

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AbsX)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RelX)));
            }
		}

		private decimal _AbsX;

		/// <summary>
		/// Absolutní souřadnice Y.
		/// </summary>
		public decimal AbsY
		{
			get => _AbsY;
			set
			{
                if (value < 0)
                    value = 0;
                
				if (_AbsY == value)
					return;

				_AbsY = value;

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AbsY)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RelY)));
            }
		}

		private decimal _AbsY;

		/// <summary>
		/// Absolutní souřadnice Z.
		/// </summary>
		public decimal AbsZ
		{
			get => _AbsZ;
			set
			{
                if (value < 0)
                    value = 0;
                
				if (_AbsZ == value)
					return;

				_AbsZ = value;

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AbsZ)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RelZ)));
            }
		}

		private decimal _AbsZ;

        #endregion

        #region Relative XYZ

        /// <summary>
        /// Relativní souřadnice X.
        /// </summary>
        public decimal RelX => _AbsX - _ZeroX;

        /// <summary>
        /// Relativní souřadnice Y.
        /// </summary>
        public decimal RelY => _AbsY - _ZeroY;

        /// <summary>
        /// Relativní souřadnice Z.
        /// </summary>
        public decimal RelZ => _AbsZ - _ZeroZ;

        #endregion

        #region Zero position XYZ

        /// <summary>
        /// Absolutní souřadnice X.
        /// </summary>
        public decimal ZeroX
        {
            get => _ZeroX;
            set
            {
                if (value < 0)
                    value = 0;

                if (_ZeroX == value)
                    return;

                _ZeroX = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ZeroX)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RelX)));
            }
        }

        private decimal _ZeroX;

        /// <summary>
        /// Absolutní souřadnice Y.
        /// </summary>
        public decimal ZeroY
        {
            get => _ZeroY;
            set
            {
                if (value < 0)
                    value = 0;

                if (_ZeroY == value)
                    return;

                _ZeroY = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ZeroY)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RelY)));
            }
        }

        private decimal _ZeroY;

        /// <summary>
        /// Absolutní souřadnice Z.
        /// </summary>
        public decimal ZeroZ
        {
            get => _ZeroZ;
            set
            {
                if (value < 0)
                    value = 0;

                if (_ZeroZ == value)
                    return;

                _ZeroZ = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ZeroZ)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RelZ)));
            }
        }

        private decimal _ZeroZ;

        public void SetZeroByAbsX() => ZeroX = AbsX;

        public void SetZeroByAbsY() => ZeroY = AbsY;

        public void SetZeroByAbsZ() => ZeroZ = AbsZ;

        public void SetZeroByAbsXY() => SetZero(AbsX, AbsY, null);

        public void SetZeroByAbsXYZ() => SetZero(AbsX, AbsY, AbsZ);

        #endregion

        #region Helpers

        /// <summary>
        /// Nastaví vše na nulu.
        /// </summary>
        public void Homing()
        {
            AbsX = 0;
            AbsY = 0;
            AbsZ = 0;

            ZeroX = 0;
            ZeroY = 0;
            ZeroZ = 0;
        }

        /// <summary>
        /// Nastaví vše naráz.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void Set(decimal x, decimal y, decimal z)
		{
			AbsX = x;
			AbsY = y;
			AbsZ = z;
		}

        /// <summary>
        /// Nastaví vše naráz.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void SetZero(decimal x, decimal y, decimal? z = null)
		{
			ZeroX = x;
			ZeroY = y;
            ZeroZ = z.GetValueOrDefault(_ZeroZ);
        }

		/// <summary>
		/// Přičte příslušné souřadnice podle zadaného směru.
		/// </summary>
		/// <param name="stepLengthAndSpeed"></param>
		/// <param name="directionX"></param>
		/// <param name="directionY"></param>
		/// <param name="directionZ"></param>
		public void Add(StepLengthAndSpeed stepLengthAndSpeed, int directionX, int directionY, int directionZ)
		{
            AbsX += directionX * stepLengthAndSpeed.StepLength;
            AbsY += directionY * stepLengthAndSpeed.StepLength;
            AbsZ += directionZ * stepLengthAndSpeed.StepLength;
        }

        #endregion
    }
}

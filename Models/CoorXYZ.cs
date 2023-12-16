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
    public class CoorXYZ(decimal x, decimal y, decimal z) : NotifyPropertyChangedBase
    {
		/// <summary>
		/// Souřadnice X.
		/// </summary>
		public decimal X
		{
			get => _X;
			set => SetValue(ref _X, value);
		}

		private decimal _X = x;

		/// <summary>
		/// Souřadnice Y.
		/// </summary>
		public decimal Y
		{
			get => _Y;
			set => SetValue(ref _Y, value);
		}

		private decimal _Y = y;

		/// <summary>
		/// Souřadnice Z.
		/// </summary>
		public decimal Z
		{
			get => _Z;
			set => SetValue(ref _Z, value);
		}

		private decimal _Z = z;

        /// <summary>
        /// Nastaví souřadnice podle X, Y, Z
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void SetBy(decimal x, decimal y, decimal z)
		{
			X = x;
			Y = y;	
			Z = z;
		}

		/// <summary>
		/// Nastaví souřadnice podle zadaného objektu.
		/// </summary>
		/// <param name="coorXYZ"></param>
		public void SetBy(CoorXYZ coorXYZ)
		{
			X = coorXYZ.X;
			Y = coorXYZ.Y;
			Z = coorXYZ.Z;
		}

		/// <summary>
		/// Odečte od souřadnic zadané souřadnice.
		/// </summary>
		/// <param name="coorXYZ"></param>
		public void Decrement(CoorXYZ coorXYZ)
		{
            X -= coorXYZ.X;
            Y -= coorXYZ.Y;
            Z -= coorXYZ.Z;
        }

    }
}

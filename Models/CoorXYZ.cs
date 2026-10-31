namespace XYCordReader.Models;

/// <summary>
/// Třída pro uchování souřadnic X, Y, Z
/// </summary>
public partial class CoorXYZ : ObservableObject
{
    public CoorXYZ()
    {
    }

    public CoorXYZ(CoorXYZ coorXYZ)
    {
        X = coorXYZ.X;
        Y = coorXYZ.Y;
        Z = coorXYZ.Z;
    }

    #region Properties

    /// <summary>
    /// Souřadnice X.
    /// </summary>
    [ObservableProperty]
    public partial decimal X { get; set; }

    /// <summary>
    /// Souřadnice Y.
    /// </summary>
    [ObservableProperty]
    public partial decimal Y { get; set; }

    /// <summary>
    /// Souřadnice Z.
    /// </summary>
    [ObservableProperty]
    public partial decimal Z { get; set; }

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

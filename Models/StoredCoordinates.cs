using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYCordReader.Models
{
    public class StoredCoordinates : NotifyPropertyChangedBase
    {
        public StoredCoordinates(CoorXYZ coorXYZ)
        {
            AbsCoordinate = new CoorXYZ(coorXYZ);
            RelCoordinate = new CoorXYZ(coorXYZ);
        }

        public StoredCoordinates(CoorXYZ coorXYZ, CoorXYZ zeroCoordinate)
            :this(coorXYZ) 
        {
            SetZero(zeroCoordinate);
        }

        /// <summary>
        /// Absolutní souřadnice.
        /// </summary>
        public CoorXYZ AbsCoordinate { get; private set; }

        /// <summary>
        /// Relativní souřadnice.
        /// </summary>
        public CoorXYZ RelCoordinate { get; private set; }

        public void SetZero(CoorXYZ zeroCoordinate)
        {
            RelCoordinate.SetWithDecrement(AbsCoordinate, zeroCoordinate);
        }

        public static string GetAllStringHeader() => "absX;absY;absZ;relX;relY;relZ";

        public string GetAllString() => $"{AbsCoordinate.X:0.00};{AbsCoordinate.Y:0.00};{AbsCoordinate.Z:0.00};{RelCoordinate.X:0.00};{RelCoordinate.Y:0.00};{RelCoordinate.Z:0.00}";

        public static string GetRelativeStringHeader(bool includeZ) => $"X;Y{(includeZ ? ";Z" : string.Empty)}";

        public string GetRelativeString(bool includeZ)
        {
            if (includeZ)
                return $"{RelCoordinate.X:0.00};{RelCoordinate.Y:0.00};{RelCoordinate.Z:0.00}";

            return $"{RelCoordinate.X:0.00};{RelCoordinate.Y:0.00}";
        }

        public override string ToString() => $"StoredCoordinates Abs: {AbsCoordinate}; Rel:{RelCoordinate}";
    }
}

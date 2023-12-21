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

        public override string ToString() => $"StoredCoordinates Asb: {AbsCoordinate}; Rel:{RelCoordinate}";
    }
}

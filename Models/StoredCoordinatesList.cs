using Syncfusion.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYCordReader.Models
{
    public class StoredCoordinatesList : ObservableCollection<StoredCoordinates>
    {
        public StoredCoordinatesList(CoorXYZ coordinateZero)
        {
            this.CoordinateZero = coordinateZero;

            this.CoordinateZero.PropertyChanged += this.CoordinateZero_PropertyChanged;
        }

        private void CoordinateZero_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.ForEach(it => it.SetZero(this.CoordinateZero));
        }

        /// <summary>
        /// Odkaz na nulový bod pro výpočet relativních souřadnic.
        /// </summary>
        public CoorXYZ CoordinateZero { get; }

        public void AddCoordinates(StoredCoordinates storedCoordinates)
        {
            base.Add(storedCoordinates);
        }

    }
}

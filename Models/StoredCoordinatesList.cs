using Syncfusion.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace XYCordReader.Models
{
    public class StoredCoordinatesList : ObservableCollection<StoredCoordinates>
    {
        public StoredCoordinatesList(CoorXYZ coordinateZero)
        {
            this.CoordinateZero = coordinateZero;

            this.CoordinateZero.PropertyChanged += this.CoordinateZero_PropertyChanged;

            this.CollectionChanged += this.StoredCoordinatesList_CollectionChanged;
        }

        private void StoredCoordinatesList_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Debug.WriteLine($"StoredCoordinatesList_CollectionChanged {e.Action}");
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

        /// <summary>
        /// Uloží všechna data do souboru
        /// </summary>
        /// <param name="path"></param>
        public void ExportAll(string path)
        {
            var lines = new List<string> { StoredCoordinates.GetAllStringHeader() };
            lines.AddRange(this.Select(it => it.GetAllString()));

            File.WriteAllLines(path, lines);
        }

        /// <summary>
        /// Uloží jen relativní data do souboru
        /// </summary>
        /// <param name="path"></param>
        /// <param name="includeZ">Nastavuje, zda bude exportována souřadnice Z</param>
        public void ExportRelative(string path, bool includeZ) => File.WriteAllLines(path, this.Select(it => it.GetRelativeString(includeZ)));
    }
}

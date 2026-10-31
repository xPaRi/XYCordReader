using System.IO;

namespace XYCordReader.Models;

public class StoredCoordinatesList : ObservableCollection<StoredCoordinates>
{
    public StoredCoordinatesList(CoorXYZ coordinateZero)
    {
        this.CoordinateZero = coordinateZero;

        this.CoordinateZero.PropertyChanged += this.CoordinateZero_PropertyChanged;
    }

    private void CoordinateZero_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        foreach (var it in this)
            it.SetZero(this.CoordinateZero);
    }

    /// <summary>
    /// Odkaz na nulový bod pro výpočet relativních souřadnic.
    /// </summary>
    public CoorXYZ CoordinateZero { get; }

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

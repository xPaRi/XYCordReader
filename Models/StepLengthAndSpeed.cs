namespace XYCordReader.Models;

/// <summary>
/// Nastavení kroku a rychlosti.
/// </summary>
public partial class StepLengthAndSpeed : ObservableObject
{
    public StepLengthAndSpeed(string modifierKeys, decimal stepLength, int speed)
    {
        ModifierKeys = modifierKeys;
        StepLength = stepLength;
        Speed = speed;
    }

    public string ModifierKeysName => $"{ModifierKeys}";

    /// <summary>
    /// Název sady.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ModifierKeysName))]
    public partial string ModifierKeys { get; set; }

    /// <summary>
    /// Počet kroků.
    /// </summary>
    [ObservableProperty]
    public partial decimal StepLength { get; set; }

    /// <summary>
    /// Rychlost.
    /// </summary>
    [ObservableProperty]
    public partial int Speed { get; set; }

    public override string ToString() => $"{nameof(ModifierKeys)}={ModifierKeys}; {nameof(StepLength)}={StepLength}; {nameof(Speed)}={Speed}";
}

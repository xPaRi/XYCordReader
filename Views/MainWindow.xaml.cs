using Microsoft.Win32;
using Syncfusion.SfSkinManager;
using Syncfusion.Themes.MaterialLightBlue.WPF;
using Syncfusion.Windows.Shared;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using XYCordReader.ViewModels;

namespace XYCordReader;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : ChromelessWindow
{
    public MainWindow()
    {
        SetSf();

        InitializeComponent();

        this.KeyDown += this.MainWindow_KeyDown;

        this.Loaded += this.MainWindow_Loaded;

        this.Closed += this.MainWindow_Closed;
    }

    const string REG_POSITION = "Position";

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        CurrentDataContext?.AppRegistry.SetData(REG_POSITION, new System.Drawing.Rectangle((int)this.Left, (int)this.Top, (int)this.Width, (int)this.Height));
    }

    #region Helpers

    private MainViewModel? CurrentDataContext => this.DataContext as MainViewModel;

    #endregion

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        SettingUpDownButton(AbsX);
        SettingUpDownButton(AbsY);
        SettingUpDownButton(AbsZ);

        SettingUpDownButton(RelX, 22);
        SettingUpDownButton(RelY, 22);
        SettingUpDownButton(RelZ, 22);

        SettingUpDownButton(ZeroX);
        SettingUpDownButton(ZeroY);
        SettingUpDownButton(ZeroZ);

        var rectangle = CurrentDataContext?.AppRegistry.GetData(REG_POSITION, new System.Drawing.Rectangle((int)this.Left, (int)this.Top, (int)this.Width, (int)this.Height));

        if (rectangle != null )
        {
            this.Left = rectangle.Value.X;
            this.Top = rectangle.Value.Y;
            this.Width = rectangle.Value.Width;
            this.Height = rectangle.Value.Height;
        }
    }

    private static void SettingUpDownButton(UpDown control, double? fontSize = null)
    {
        if (fontSize.HasValue)
        {
            control.FontSize = fontSize.Value;
        }

        control.TextAlignment = TextAlignment.Right;
        control.BorderThickness = new Thickness(1);
        control.Padding = new Thickness(2);
        control.NumberDecimalDigits = 2;
        control.Margin = new Thickness(5);
        control.MinWidth = 100;

        if (control.Template.FindName("upbutton", control) is RepeatButton upButton)
            upButton.Visibility = Visibility.Collapsed;

        if (control.Template.FindName("downbutton", control) is RepeatButton downButton)
            downButton.Visibility = Visibility.Collapsed;
    }


    private void SetSf()
    {
        var themeSettings = new MaterialLightBlueThemeSettings
        {
            BodyFontSize = 14,
            FontFamily = new FontFamily("Callibri"),
            Palette = MaterialPalette.Default,
        };

        SfSkinManager.RegisterThemeSettings("MaterialLightBlue", themeSettings);

        this.TitleBarBackground = themeSettings.PrimaryBackground;
        this.TitleBarForeground = themeSettings.PrimaryForeground;
        this.TitleFontSize = themeSettings.TitleFontSize * 1.2;
    }

    private void MainWindow_KeyDown(object sender, KeyEventArgs e)
    {
        if (CurrentDataContext == null) 
            return;

        switch(e.Key)
        {
            //X
            case Key.Left:
                CurrentDataContext.XDownCommand.Execute(Keyboard.Modifiers.ToString());
                e.Handled = true;
                break;
            case Key.Right:
                CurrentDataContext.XUpCommand.Execute(Keyboard.Modifiers.ToString());
                e.Handled = true;
                break;

            //Y
            case Key.Up:
                CurrentDataContext.YUpCommand.Execute(Keyboard.Modifiers.ToString());
                e.Handled = true;
                break;
            case Key.Down:
                CurrentDataContext.YDownCommand.Execute(Keyboard.Modifiers.ToString());
                e.Handled = true;
                break;

            //Z
            case Key.PageUp:
                CurrentDataContext.ZUpCommand.Execute(Keyboard.Modifiers.ToString());
                e.Handled = true;
                break;
            case Key.PageDown:
                CurrentDataContext.ZDownCommand.Execute(Keyboard.Modifiers.ToString());
                e.Handled = true;
                break;

            case Key.Subtract when Keyboard.Modifiers == ModifierKeys.None:
                DeleteRelCoordinate_Click(sender, null);
                break;
            case Key.Add when Keyboard.Modifiers == ModifierKeys.Control:
                CurrentDataContext.InsertRelCoordinateCommand.Execute(true);
                break;
            case Key.Add when Keyboard.Modifiers==ModifierKeys.None:
                CurrentDataContext.AddRelCoordinateCommand.Execute(true);
                break;

            case Key.Multiply:
                CurrentDataContext.GotoXYCommand.Execute(Keyboard.Modifiers.ToString());
                e.Handled = true;
                break;
        }
    }

    private void DeleteRelCoordinate_Click(object sender, RoutedEventArgs? e)
    {
        if (CurrentDataContext == null)
            return;

        CurrentDataContext.DeleteRelCoordinateSelectedItemsCmd(CoordinatesList.SelectedItems);
    }

    private void ExportRelative_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentDataContext == null)
            return;

        var saveFileDialog = new SaveFileDialog()
        {
            FileName = CurrentDataContext.LastExportFile,
            Filter = "CSV UTF-8 (*.csv)|*.csv",

        };

        if (saveFileDialog.ShowDialog() == true)
        {
            CurrentDataContext.ExportRelative(saveFileDialog.FileName);
        }
    }

    private void ExportAll_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentDataContext == null)
            return;

        var saveFileDialog = new SaveFileDialog()
        {
            FileName = CurrentDataContext.LastExportFile,
            Filter = "CSV UTF-8 (*.csv)|*.csv",
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            CurrentDataContext.ExportAll(saveFileDialog.FileName);
        }
    }
}
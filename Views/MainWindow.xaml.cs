using Syncfusion.SfSkinManager;
using Syncfusion.Themes.MaterialLightBlue.WPF;
using Syncfusion.Windows.Shared;
using System.Diagnostics;
using System.Text;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using XYCordReader.ViewModels;

namespace XYCordReader
{
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

        }

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
            if (this.DataContext is not MainViewModel dataContext)
                return;

            switch(e.Key)
            {
                //X
                case Key.Left:
                    dataContext.XDown.Execute(Keyboard.Modifiers);
                    e.Handled = true;
                    break;
                case Key.Right:
                    dataContext.XUp.Execute(Keyboard.Modifiers);
                    e.Handled = true;
                    break;

                //Y
                case Key.Up:
                    dataContext.YUp.Execute(Keyboard.Modifiers);
                    e.Handled = true;
                    break;
                case Key.Down:
                    dataContext.YDown.Execute(Keyboard.Modifiers);
                    e.Handled = true;
                    break;

                //Z
                case Key.PageUp:
                    dataContext.ZUp.Execute(Keyboard.Modifiers);
                    e.Handled = true;
                    break;
                case Key.PageDown:
                    dataContext.ZDown.Execute(Keyboard.Modifiers);
                    e.Handled = true;
                    break;
            }

            Debug.WriteLine($"{Keyboard.Modifiers}; {e.Key}; {e.KeyStates}; {e.SystemKey}");
        }


    }
}
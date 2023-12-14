using System.Configuration;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;

namespace XYCordReader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            //Desetinné tečky v celé aplikaci
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mjk3NTcyNUAzMjMzMmUzMDJlMzBJNDdteFpVNVpGbFVvTGhqSlpGdzlZaUltSkNDazg1eFZJU1Q3Ri9tT0xFPQ==;Mjk3NTcyNkAzAzMjMzMzMzHcyMd3WHUZJlM FFeXB1aVQzZzdXZkwybEt6a2hjPQ==");
            Syncfusion.SfSkinManager.SfSkinManager.ApplyStylesOnApplication = true; //aby vše v aplikaci mělo stejné téma
        }
  
    }

}

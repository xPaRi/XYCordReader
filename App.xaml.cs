namespace XYCordReader;

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

        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjGyl/VkN+XU9HcVRLVGpAY1J0WGBYb1xzflBPallYT3RfQFtiSX1Sd0xmWX5bcHFWRmtfVA==");
        Syncfusion.SfSkinManager.SfSkinManager.ApplyStylesOnApplication = true; //aby vše v aplikaci mělo stejné téma
    }

}

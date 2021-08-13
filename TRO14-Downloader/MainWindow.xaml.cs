using CrashReporterDotNET;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TRO14_Downloader
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public string installationFolder = Environment.CurrentDirectory;

        //Download Links
        public string jsonDemoLink = "https://raw.githubusercontent.com/Lagger2807/TRO14-Files/main/Demo.json";
        public string jsonLiteLink = "https://github.com/Lagger2807/TRO14-Files/raw/main/Lite.json";
        public string jsonStandardLink = "https://raw.githubusercontent.com/Lagger2807/TRO14-Files/main/Standard.json";
        public string jsonOldTimesLink = "https://github.com/Lagger2807/TRO14-Files/raw/main/OldTimes.json";
        public string jsonFutureLink = "https://github.com/Lagger2807/TRO14-Files/raw/main/Future.json";

        public string packDemoLink = "https://github.com/Lagger2807/TRO14-Files/raw/main/TROP%20Demo%20.html";
        public string packLiteLink = "https://github.com/Lagger2807/TRO14-Files/raw/main/TROP%20Lite.html";
        public string packStandardLink = "https://github.com/Lagger2807/TRO14-Files/raw/main/TROP%20Standard.html";
        public string packOldTimesLink = "https://github.com/Lagger2807/TRO14-Files/raw/main/TROP%20OldTimes.html";
        public string packFutureLink = "https://github.com/Lagger2807/TRO14-Files/raw/main/TROP%20Future.html";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //first-start version file control
                FirstStartControl();

                //calling the versions.json controller
                Packs packsController = packVersionController();

                //All comments done in the "Demo" selection

                //Demo
                if (packsController.Demo != 0)
                {
                    //Change led state to ON
                    InstalledPacksCheck(0);

                    //Declaring installation folder and calling the Downloader
                    string thisUri = installationFolder + @"\Demo.json";
                    Downloader(jsonDemoLink, thisUri);

                    //Calling the Reader to create a "modpack".json controller
                    string jsonController = Reader(thisUri);
                    ModPack Pack = JsonConvert.DeserializeObject<ModPack>(jsonController);

                    //Check if the pack is up to date
                    if (Pack.Version > packsController.Demo)
                    {
                        Led_Demo.Fill = Brushes.Red;
                        MessageBox.Show("Aggiornamento Demo disponibile!");
                    }
                }

                //Lite
                if (packsController.Lite != 0)
                {
                    InstalledPacksCheck(1);

                    string thisUri = installationFolder + @"\Lite.json";
                    Downloader(jsonLiteLink, thisUri);

                    string jsonController = Reader(thisUri);
                    ModPack Pack = JsonConvert.DeserializeObject<ModPack>(jsonController);

                    if (Pack.Version > packsController.Lite)
                    {
                        Led_Lite.Fill = Brushes.Red;
                        MessageBox.Show("Aggiornamento Lite disponibile!");
                    }
                }

                //Standard
                if (packsController.Standard != 0)
                {
                    InstalledPacksCheck(2);

                    string thisUri = installationFolder + @"\Standard.json";
                    Downloader(jsonStandardLink, thisUri);

                    string jsonController = Reader(thisUri);
                    ModPack Pack = JsonConvert.DeserializeObject<ModPack>(jsonController);

                    if (Pack.Version > packsController.Standard)
                    {
                        Led_Standard.Fill = Brushes.Red;
                        MessageBox.Show("Aggiornamento Standard disponibile!");
                    }
                }

                //OldTimes
                if (packsController.OldTimes != 0)
                {
                    InstalledPacksCheck(3);

                    string thisUri = installationFolder + @"\OldTimes.json";
                    Downloader(jsonOldTimesLink, thisUri);

                    string jsonController = Reader(thisUri);
                    ModPack Pack = JsonConvert.DeserializeObject<ModPack>(jsonController);

                    if (Pack.Version > packsController.OldTimes)
                    {
                        Led_OldTimes.Fill = Brushes.Red;
                        MessageBox.Show("Aggiornamento OldTimes disponibile!");
                    }
                }

                //Future
                if (packsController.Future != 0)
                {
                    InstalledPacksCheck(4);

                    string thisUri = installationFolder + @"\Future.json";
                    Downloader(jsonFutureLink, thisUri);

                    string jsonController = Reader(thisUri);
                    ModPack Pack = JsonConvert.DeserializeObject<ModPack>(jsonController);

                    if (Pack.Version > packsController.Future)
                    {
                        Led_Future.Fill = Brushes.Red;
                        MessageBox.Show("Aggiornamento Future disponibile!");
                    }
                }
            }
            catch(Exception error)
            {
                App.SendReport(error, "Errore di inizializzazione" + error.Message);
            }
            
        }

        private void Btn_Download_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Set download root folder
                string downloadFolder = installationFolder + @"\Download";

                //Check if Demo checkbox is checked
                if (CK_Demo.IsChecked.Value)
                {
                    try
                    {
                        Downloader(packDemoLink, downloadFolder + @"\Demo.html");

                        string thisUri = installationFolder + @"\Demo.json";
                        if (!File.Exists(thisUri))
                        {
                            Downloader(jsonDemoLink, thisUri);
                        }

                        string jsonController = Reader(thisUri);
                        ModPack Pack = JsonConvert.DeserializeObject<ModPack>(jsonController);
                        Packs packsController = packVersionController();

                        packsController.Demo = Pack.Version;

                        string packsJson = JsonConvert.SerializeObject(packsController);

                        using (FileStream packsJsonFile = File.Create(installationFolder + @"\versions.json"))
                        {
                            byte[] content = new UTF8Encoding(true).GetBytes(packsJson);
                            packsJsonFile.Write(content, 0, content.Length);
                            packsJsonFile.Close();
                        }

                        Led_Demo.Fill = Brushes.Green;
                        Led_Demo.Visibility = Visibility.Visible;
                    }
                    catch (Exception error)
                    {
                        MessageBox.Show(error.Message);
                    }
                }

                //Check if Lite checkbox is checked
                if (CK_Lite.IsChecked.Value)
                {
                    try
                    {
                        Downloader(packLiteLink, downloadFolder + @"\Lite.html");

                        string thisUri = installationFolder + @"\Lite.json";
                        if (!File.Exists(thisUri))
                        {
                            Downloader(jsonLiteLink, thisUri);
                        }

                        string jsonController = Reader(thisUri);
                        ModPack Pack = JsonConvert.DeserializeObject<ModPack>(jsonController);
                        Packs packsController = packVersionController();

                        packsController.Lite = Pack.Version;

                        string packsJson = JsonConvert.SerializeObject(packsController);

                        using (FileStream packsJsonFile = File.Create(installationFolder + @"\versions.json"))
                        {
                            byte[] content = new UTF8Encoding(true).GetBytes(packsJson);
                            packsJsonFile.Write(content, 0, content.Length);
                            packsJsonFile.Close();
                        }

                        Led_Lite.Fill = Brushes.Green;
                        Led_Lite.Visibility = Visibility.Visible;
                    }
                    catch (Exception error)
                    {
                        MessageBox.Show(error.Message);
                    }
                }

                //Check if Standard checkbox is checked
                if (CK_Standard.IsChecked.Value)
                {
                    try
                    {
                        Downloader(packStandardLink, downloadFolder + @"\Standard.html");

                        string thisUri = installationFolder + @"\Standard.json";
                        if (!File.Exists(thisUri))
                        {
                            Downloader(jsonStandardLink, thisUri);
                        }

                        string jsonController = Reader(thisUri);
                        ModPack Pack = JsonConvert.DeserializeObject<ModPack>(jsonController);
                        Packs packsController = packVersionController();

                        packsController.Standard = Pack.Version;

                        string packsJson = JsonConvert.SerializeObject(packsController);

                        using (FileStream packsJsonFile = File.Create(installationFolder + @"\versions.json"))
                        {
                            byte[] content = new UTF8Encoding(true).GetBytes(packsJson);
                            packsJsonFile.Write(content, 0, content.Length);
                            packsJsonFile.Close();
                        }

                        Led_Standard.Fill = Brushes.Green;
                        Led_Standard.Visibility = Visibility.Visible;
                    }
                    catch (Exception error)
                    {
                        MessageBox.Show(error.Message);
                    }
                }

                //Check if Old Times checkbox is checked
                if (CK_OldTimes.IsChecked.Value)
                {
                    try
                    {
                        Downloader(packOldTimesLink, downloadFolder + @"\OldTimes.html");

                        string thisUri = installationFolder + @"\OldTimes.json";
                        if (!File.Exists(thisUri))
                        {
                            Downloader(jsonOldTimesLink, thisUri);
                        }

                        string jsonController = Reader(thisUri);
                        ModPack Pack = JsonConvert.DeserializeObject<ModPack>(jsonController);
                        Packs packsController = packVersionController();

                        packsController.OldTimes = Pack.Version;

                        string packsJson = JsonConvert.SerializeObject(packsController);

                        using (FileStream packsJsonFile = File.Create(installationFolder + @"\versions.json"))
                        {
                            byte[] content = new UTF8Encoding(true).GetBytes(packsJson);
                            packsJsonFile.Write(content, 0, content.Length);
                            packsJsonFile.Close();
                        }

                        Led_OldTimes.Fill = Brushes.Green;
                        Led_OldTimes.Visibility = Visibility.Visible;
                    }
                    catch (Exception error)
                    {
                        MessageBox.Show(error.Message);
                    }
                }

                //Check if Future checkbox is checked
                if (CK_Future.IsChecked.Value)
                {
                    try
                    {
                        Downloader(packFutureLink, downloadFolder + @"\Future.html");

                        string thisUri = installationFolder + @"\Future.json";
                        if (!File.Exists(thisUri))
                        {
                            Downloader(jsonFutureLink, thisUri);
                        }

                        string jsonController = Reader(thisUri);
                        ModPack Pack = JsonConvert.DeserializeObject<ModPack>(jsonController);
                        Packs packsController = packVersionController();

                        packsController.Future = Pack.Version;

                        string packsJson = JsonConvert.SerializeObject(packsController);

                        using (FileStream packsJsonFile = File.Create(installationFolder + @"\versions.json"))
                        {
                            byte[] content = new UTF8Encoding(true).GetBytes(packsJson);
                            packsJsonFile.Write(content, 0, content.Length);
                            packsJsonFile.Close();
                        }

                        Led_Future.Fill = Brushes.Green;
                        Led_Future.Visibility = Visibility.Visible;
                    }
                    catch (Exception error)
                    {
                        MessageBox.Show(error.Message);
                    }
                }

                //Open the download folder and instructions
                Process.Start(downloadFolder);
                Instructions();
            }
            catch(Exception error)
            {
                App.SendReport(error, "Download non riuscito" + error.Message);
            }
            
        }

        private void Btn_Tutorial_Click(object sender, RoutedEventArgs e)
        {
            Instructions();
        }

        private void Btn_Visualizza_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string versionJson = Reader(installationFolder + @"\versions.json");
                Packs packsController = JsonConvert.DeserializeObject<Packs>(versionJson);
                MessageBox.Show("Demo: " + packsController.Demo + "\n" +
                    "Lite: " + packsController.Lite + "\n" +
                    "Standard: " + packsController.Standard + "\n" +
                    "Old Times: " + packsController.OldTimes + "\n" +
                    "Future: " + packsController.Future);
            }
            catch(Exception error)
            {
                App.SendReport(error, "Si è riscontrato un errore nella lettura dei pacchetti installati" + error.Message);
            }
            
        }

        //Re-callable Downloader
        void Downloader(string dwLink, string uri)
        {
            using (WebClient wc = new WebClient())
            {
                wc.Headers.Add("a", "a");
                try
                {
                    wc.DownloadFile(dwLink, uri);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        //Re-callable Reader
        string Reader(string uri)
        {
            StreamReader reader = new StreamReader(uri);
            string returnString = reader.ReadToEnd();
            reader.Close();

            return returnString;
        }

        //Function with all first start procedures (check presence and create basic files/roots)
        void FirstStartControl()
        {
            if (!File.Exists(installationFolder + @"\versions.json"))
            {
                Packs packs = new Packs()
                {
                    Demo = 0,
                    Lite = 0,
                    Standard = 0,
                    OldTimes = 0,
                    Future = 0,
                };

                string packsJson = JsonConvert.SerializeObject(packs);

                using (FileStream packsJsonFile = File.Create(installationFolder + @"\versions.json"))
                {
                    byte[] content = new UTF8Encoding(true).GetBytes(packsJson);
                    packsJsonFile.Write(content, 0, content.Length);
                    packsJsonFile.Close();
                }
            }

            if(!Directory.Exists(installationFolder + @"\Download"))
            {
                Directory.CreateDirectory(installationFolder + @"\Download");
            }
        }

        //Modular function to light up the "presence led" of a pack
        void InstalledPacksCheck(int ledId)
        {
            switch(ledId)
            {
                case 0:
                    Led_Demo.Visibility = Visibility.Visible;
                    break;
                case 1:
                    Led_Lite.Visibility = Visibility.Visible;
                    break;
                case 2:
                    Led_Standard.Visibility = Visibility.Visible;
                    break;
                case 3:
                    Led_OldTimes.Visibility = Visibility.Visible;
                    break;
                case 4:
                    Led_Future.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
        }

        //Read and Deserialize versions.json for version checking
        Packs packVersionController()
        {
            string versionJson = Reader(installationFolder + @"\versions.json");
            Packs packsController = JsonConvert.DeserializeObject<Packs>(versionJson);

            return packsController;
        }

        //Standardized and re-callable Message Box
        void Instructions()
        {
            MessageBox.Show("-Importa i pacchetti trascinandoli nel launcher di ArmA III \n" +
                    "-Accetta eventuali sovrascritture e/o sottoscrizioni al workshop \n" +
                    "-Attendi la fine del download.");
        }
    }
    public class ModPack
    {
        public string Pack { get; set; }
        public float Version { get; set; }
    }

    public class Packs
    {
        public float Demo { get; set; }
        public float Lite { get; set; }
        public float Standard { get; set; }
        public float OldTimes { get; set; }
        public float Future { get; set; }
    }

    public partial class App : Application
    {
        private static ReportCrash _reportCrash;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            Application.Current.DispatcherUnhandledException += DispatcherOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
            _reportCrash = new ReportCrash("tro14.squad@gmail.com")
            {
                Silent = true
            };
            _reportCrash.RetryFailedReports();
        }

        private void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs unobservedTaskExceptionEventArgs)
        {
            SendReport(unobservedTaskExceptionEventArgs.Exception);
        }

        private void DispatcherOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs dispatcherUnhandledExceptionEventArgs)
        {
            SendReport(dispatcherUnhandledExceptionEventArgs.Exception);
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            SendReport((Exception)unhandledExceptionEventArgs.ExceptionObject);
        }

        public static void SendReport(Exception exception, string developerMessage = "")
        {
            _reportCrash.Silent = false;
            _reportCrash.Send(exception);
        }

        public static void SendReportSilently(Exception exception, string developerMessage = "")
        {
            _reportCrash.Silent = true;
            _reportCrash.Send(exception);
        }
    }
}

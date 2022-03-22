using CrashReporterDotNET;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace TRO14_Downloader
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //OPTIMIZE VULKAN DIRECTORY READING AND FILE EXISTENCE CHECK ACROSS THE CODE o(≧口≦)o

        //Global installation folder
        public string installationFolder = Environment.CurrentDirectory;

        //State variables
        public bool vulkan;

        StreamReader reader;

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

        public string vulkanAPIFiles = "https://github.com/Lagger2807/TRO14-Files/raw/main/VulkanFiles.zip";
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //Threaded first-start control of files and roots
                await Task.Run(() => FirstStartControl());

                //Threaded on opening control of download versions and updates
                await Task.Run(() => OpeningChecker());
            }
            catch (Exception error)
            {
                //Send Crash report via CrashReportDotNet API
                App.SendReport(error, "Initialization error" + error.Message);
            }
            
        }

        private async void Btn_Download_Click(object sender, RoutedEventArgs e)
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
                        await Task.Run(() =>Downloader(packDemoLink, downloadFolder + @"\Demo.html"));

                        string thisUri = installationFolder + @"\Demo.json";
                        if (!File.Exists(thisUri))
                        {
                            await Task.Run(() => Downloader(jsonDemoLink, thisUri));
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
                        await Task.Run(() => Downloader(packLiteLink, downloadFolder + @"\Lite.html"));

                        string thisUri = installationFolder + @"\Lite.json";
                        if (!File.Exists(thisUri))
                        {
                            await Task.Run(() => Downloader(jsonLiteLink, thisUri));
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
                        await Task.Run(() => Downloader(packStandardLink, downloadFolder + @"\Standard.html"));

                        string thisUri = installationFolder + @"\Standard.json";
                        if (!File.Exists(thisUri))
                        {
                            await Task.Run(() => Downloader(jsonStandardLink, thisUri));
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
                        await Task.Run(() => Downloader(packOldTimesLink, downloadFolder + @"\OldTimes.html"));

                        string thisUri = installationFolder + @"\OldTimes.json";
                        if (!File.Exists(thisUri))
                        {
                            await Task.Run(() => Downloader(jsonOldTimesLink, thisUri));
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
                        await Task.Run(() => Downloader(packFutureLink, downloadFolder + @"\Future.html"));

                        string thisUri = installationFolder + @"\Future.json";
                        if (!File.Exists(thisUri))
                        {
                            await Task.Run(() => Downloader(jsonFutureLink, thisUri));
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
                await Task.Run(() => Instructions());
            }
            catch(Exception error)
            {
                //Send Crash report via CrashReportDotNet API
                App.SendReport(error, "Download failed" + error.Message);
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
                MessageBox.Show("Demo: " + packsController.Demo + ".\n" +
                    "Lite: " + packsController.Lite + ".\n" +
                    "Standard: " + packsController.Standard + ".\n" +
                    "Old Times: " + packsController.OldTimes + ".\n" +
                    "Future: " + packsController.Future + ".",
                    "Versions");
            }
            catch(Exception error)
            {
                //Send Crash report via CrashReportDotNet API
                App.SendReport(error, "An error occurred during the installed packs check" + error.Message);
            }
            
        }

        private void Btn_Vulkan_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Set download root folder
                string downloadFolder = installationFolder + @"\Download";

                bool dialogOK = false;
                string arma3Folder;

                //Open folder selection dialog to user
                System.Windows.Forms.FolderBrowserDialog folderDlg = new System.Windows.Forms.FolderBrowserDialog();                
                folderDlg.ShowNewFolderButton = false; //Disable new folders creation

                //Show dialog to user
                System.Windows.Forms.DialogResult result = folderDlg.ShowDialog();

                //Check if a folder as been selected and assign it to a variable, else it choose the default (desktop) folder
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    arma3Folder = folderDlg.SelectedPath;
                    dialogOK = true;
                    //Call to the Downloader function
                    Downloader(vulkanAPIFiles, downloadFolder + @"\VulkanFiles.zip");
                }
                else
                {
                    //Set the ArmA3 folder as the desktop
                    arma3Folder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                    //Call to the Downloader function
                    Downloader(vulkanAPIFiles, downloadFolder + @"\VulkanFiles.zip");
                }

                //Check if the files already exist, if true it overwrites them
                if (File.Exists(arma3Folder + @"\d3d11.dll"))
                    File.Delete(arma3Folder + @"\d3d11.dll");

                if (File.Exists(arma3Folder + @"\dxgi.dll"))
                    File.Delete(arma3Folder + @"\dxgi.dll");

                //Unzip files in the ArmA3 directory
                ZipFile.ExtractToDirectory(downloadFolder + @"\VulkanFiles.zip", arma3Folder);
                
                //Check if the placeholder file is present and if the dialog has been completed
                if (File.Exists(installationFolder + @"\VulkanIsPresent.txt") && dialogOK)
                {
                    vulkan = true; //Sets the global VulkanAPI presence variable on true
                    //Show the VulkanAPI checkbox, text and led
                    CK_Vulkan.Visibility = Visibility.Visible;
                    CK_Vulkan.IsChecked = true;
                    Text_VulkanIsPresent.Visibility = Visibility.Visible;
                    Led_Vulkan.Visibility = Visibility.Visible;
                }
                else if(!File.Exists(installationFolder + @"\VulkanIsPresent.txt") && dialogOK)
                {
                    //Create the placeholder file to know if VulkanAPI is present
                    StreamWriter writer = new StreamWriter("VulkanIsPresent.txt");
                    writer.WriteLine(arma3Folder);
                    writer.Close();

                    vulkan = true; //Sets the global VulkanAPI presence variable on true
                    //Show the VulkanAPI checkbox, text and led
                    CK_Vulkan.Visibility = Visibility.Visible;
                    CK_Vulkan.IsChecked = true;
                    Text_VulkanIsPresent.Visibility = Visibility.Visible;
                    Led_Vulkan.Visibility = Visibility.Visible;
                }

                MessageBox.Show("Vulkan libraries installed successfully");
            }
            catch(Exception error)
            {
                //Send Crash report via CrashReportDotNet API
                App.SendReport(error, "Vulkan API download failed" + error.Message);
            }
        }

        private void CK_Vulkan_CheckedEvent(object sender, RoutedEventArgs e) //HERE
        {
            string arma3Folder = Reader(installationFolder + @"\VulkanIsPresent.txt", true);

            try
            {
                //If VulkanAPIs are already on the rename script do not trigger
                if (!vulkan)
                {
                    //Rename files using Move using
                    File.Move(arma3Folder + @"\d3d11.dllOFF", arma3Folder + @"\d3d11.dll");
                    File.Move(arma3Folder + @"\dxgi.dllOFF", arma3Folder + @"\dxgi.dll");
                    vulkan = true;
                }

                MessageBox.Show("Disable the Battleye™ service in the ArmA III launcher to start the game");
            }
            catch(Exception error)
            {
                //Send Crash report via CrashReportDotNet API
                App.SendReport(error, "failed to enable Vulkan API" + error.Message);
            }
        } 

        private void CK_Vulkan_UncheckedEvent(object sender, RoutedEventArgs e) //HERE
        {
            string arma3Folder = Reader(installationFolder + @"\VulkanIsPresent.txt", true);

            try
            {
                //If VulkanAPIs are already off the rename script do not trigger
                if (vulkan)
                {
                    //Rename files using Move using
                    File.Move(arma3Folder + @"\d3d11.dll", arma3Folder + @"\d3d11.dllOFF");
                    File.Move(arma3Folder + @"\dxgi.dll", arma3Folder + @"\dxgi.dllOFF");
                    vulkan = false;
                }

                MessageBox.Show("You can enable the Battleye™ service in the ArmA III launcher");
            }
            catch(Exception error)
            {
                //Send Crash report via CrashReportDotNet API
                App.SendReport(error, "failed to disable Vulkan API" + error.Message);
            }
        }

        //Function to check packs presence/update
        void OpeningChecker() //HERE
        {
            //calling the versions.json controller
            Packs packsController = packVersionController();

            //All comments done in the "Demo" selection inside the dispatcher
            this.Dispatcher.Invoke(() =>
            {
                //Optimize code (single Modpack object creation) (ToT)/

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
                        MessageBox.Show("Demo update found!");
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
                        MessageBox.Show("Lite update found!");
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
                        MessageBox.Show("Standard update found!");
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
                        MessageBox.Show("Old Times update found!");
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
                        MessageBox.Show("Future update found!");
                    }
                }

                string vulkanFile = installationFolder + @"\VulkanIsPresent.txt";

                //Check if the VulkanAPIs are installed
                if (File.Exists(vulkanFile))
                {
                    //Read the ArmA3 directory
                    string arma3Folder = Reader(vulkanFile, true);

                    //Show the VulkanAPI checkbox, text and led
                    CK_Vulkan.Visibility = Visibility.Visible;
                    Text_VulkanIsPresent.Visibility = Visibility.Visible;
                    Led_Vulkan.Visibility = Visibility.Visible;

                    if (File.Exists(arma3Folder + @"\d3d11.dll") && File.Exists(arma3Folder + @"\dxgi.dll"))
                    {
                        vulkan = true; //Sets the global VulkanAPI presence variable on true
                        CK_Vulkan.IsChecked = true;
                    }
                    else if (File.Exists(arma3Folder + @"\d3d11.dllOFF") && File.Exists(arma3Folder + @"\dxgi.dllOFF"))
                    {
                        vulkan = false; //Sets the global VulkanAPI presence variable on false
                        CK_Vulkan.IsChecked = false;
                    }
                    else
                    {
                        MessageBox.Show("Vulkan API damaged, UI enabler disabled");
                        CK_Vulkan.IsEnabled = false;
                    }
                }
            });
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

        //Re-callable Readers
        string Reader(string uri)
        {
            StreamReader reader = new StreamReader(uri);
            string returnString = reader.ReadToEnd();
            reader.Close();

            return returnString;
        }

        string Reader(string uri, bool singleLine)
        {
            reader = new StreamReader(uri);
            string returnString = reader.ReadLine();
            reader.Close();

            return returnString;
        }
        //End Readers

        //Function with all first start procedures (check presence and create basic files/roots)
        void FirstStartControl()
        {
            this.Dispatcher.Invoke(() =>
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

                if (!Directory.Exists(installationFolder + @"\Download"))
                {
                    Directory.CreateDirectory(installationFolder + @"\Download");
                }
            });
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
            MessageBox.Show("-Import all downloaded packs by dragging them into your ArmA III launcher. \n" +
                    "-Accept all the Workshop™ subscriptions. \n" +
                    "-Wait until the mods are downloaded from Steam™.", "Instructions");
        }

        private void Btn_Start_Click(object sender, RoutedEventArgs e)
        {
            if (!vulkan) { return; }

            string arma3Folder = Reader(installationFolder + @"\VulkanIsPresent.txt", true);

            Process.Start(arma3Folder + @"\arma3launcher.exe");
            this.Close();
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
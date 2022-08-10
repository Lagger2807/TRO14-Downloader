using CrashReporterDotNET;
using Newtonsoft.Json;
using SQLite;
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
using TRO14_Downloader.CustomClasses;

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
        public string dbURI = Path.Combine(Environment.CurrentDirectory, "Downloader.db");

        public bool vulkan;
        public string vulkanAPIFiles;

        //Cached objects
        DBManager dbManager = new DBManager();
        StreamReader reader;
        Instructions instructions;

        //Global constants
        public const string jsonReadURL = "https://raw.githubusercontent.com/Lagger2807/TRO14-Files/main/DB%20Inizializer.json";

        public MainWindow()
        {
            InitializeComponent();
        }

        #region Events
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
            //try
            //{
            //    //Set download root folder
            //    string downloadFolder = installationFolder + @"\Download";

            //    //Check if Demo checkbox is checked
            //    if (CK_Demo.IsChecked.Value)
            //    {
            //        try
            //        {
            //            await Task.Run(() => Downloader(packsLinks[0], downloadFolder + @"\Demo.html"));

            //            string thisUri = installationFolder + @"\Demo.json";
            //            if (!File.Exists(thisUri))
            //            {
            //                await Task.Run(() => Downloader(jsonLinks[0], thisUri));
            //            }

            //            string jsonController = Reader(thisUri);
            //            ModPack Pack = JsonConvert.DeserializeObject<ModPack>(jsonController);
            //            Packs packsController = packVersionController();

            //            packsController.Demo = Pack.Version;

            //            string packsJson = JsonConvert.SerializeObject(packsController);

            //            using (FileStream packsJsonFile = File.Create(installationFolder + @"\versions.json"))
            //            {
            //                byte[] content = new UTF8Encoding(true).GetBytes(packsJson);
            //                packsJsonFile.Write(content, 0, content.Length);
            //                packsJsonFile.Close();
            //            }

            //            Led_Demo.Fill = Brushes.Green;
            //            Led_Demo.Visibility = Visibility.Visible;
            //        }
            //        catch (Exception error)
            //        {
            //            MessageBox.Show(error.Message);
            //        }
            //    }

            //    //Check if Lite checkbox is checked
            //    if (CK_Lite.IsChecked.Value)
            //    {
            //        try
            //        {
            //            await Task.Run(() => Downloader(packsLinks[1], downloadFolder + @"\Lite.html"));

            //            string thisUri = installationFolder + @"\Lite.json";
            //            if (!File.Exists(thisUri))
            //            {
            //                await Task.Run(() => Downloader(jsonLinks[1], thisUri));
            //            }

            //            string jsonController = Reader(thisUri);
            //            ModPack Pack = JsonConvert.DeserializeObject<ModPack>(jsonController);
            //            Packs packsController = packVersionController();

            //            packsController.Lite = Pack.Version;

            //            string packsJson = JsonConvert.SerializeObject(packsController);

            //            using (FileStream packsJsonFile = File.Create(installationFolder + @"\versions.json"))
            //            {
            //                byte[] content = new UTF8Encoding(true).GetBytes(packsJson);
            //                packsJsonFile.Write(content, 0, content.Length);
            //                packsJsonFile.Close();
            //            }

            //            Led_Lite.Fill = Brushes.Green;
            //            Led_Lite.Visibility = Visibility.Visible;
            //        }
            //        catch (Exception error)
            //        {
            //            MessageBox.Show(error.Message);
            //        }
            //    }

            //    //Check if Standard checkbox is checked
            //    if (CK_Standard.IsChecked.Value)
            //    {
            //        try
            //        {
            //            await Task.Run(() => Downloader(packsLinks[2], downloadFolder + @"\Standard.html"));

            //            string thisUri = installationFolder + @"\Standard.json";
            //            if (!File.Exists(thisUri))
            //            {
            //                await Task.Run(() => Downloader(jsonLinks[2], thisUri));
            //            }

            //            string jsonController = Reader(thisUri);
            //            ModPack Pack = JsonConvert.DeserializeObject<ModPack>(jsonController);
            //            Packs packsController = packVersionController();

            //            packsController.Standard = Pack.Version;

            //            string packsJson = JsonConvert.SerializeObject(packsController);

            //            using (FileStream packsJsonFile = File.Create(installationFolder + @"\versions.json"))
            //            {
            //                byte[] content = new UTF8Encoding(true).GetBytes(packsJson);
            //                packsJsonFile.Write(content, 0, content.Length);
            //                packsJsonFile.Close();
            //            }

            //            Led_Standard.Fill = Brushes.Green;
            //            Led_Standard.Visibility = Visibility.Visible;
            //        }
            //        catch (Exception error)
            //        {
            //            MessageBox.Show(error.Message);
            //        }
            //    }

            //    //Check if Old Times checkbox is checked
            //    if (CK_OldTimes.IsChecked.Value)
            //    {
            //        try
            //        {
            //            await Task.Run(() => Downloader(packsLinks[3], downloadFolder + @"\OldTimes.html"));

            //            string thisUri = installationFolder + @"\OldTimes.json";
            //            if (!File.Exists(thisUri))
            //            {
            //                await Task.Run(() => Downloader(jsonLinks[3], thisUri));
            //            }

            //            string jsonController = Reader(thisUri);
            //            ModPack Pack = JsonConvert.DeserializeObject<ModPack>(jsonController);
            //            Packs packsController = packVersionController();

            //            packsController.OldTimes = Pack.Version;

            //            string packsJson = JsonConvert.SerializeObject(packsController);

            //            using (FileStream packsJsonFile = File.Create(installationFolder + @"\versions.json"))
            //            {
            //                byte[] content = new UTF8Encoding(true).GetBytes(packsJson);
            //                packsJsonFile.Write(content, 0, content.Length);
            //                packsJsonFile.Close();
            //            }

            //            Led_OldTimes.Fill = Brushes.Green;
            //            Led_OldTimes.Visibility = Visibility.Visible;
            //        }
            //        catch (Exception error)
            //        {
            //            MessageBox.Show(error.Message);
            //        }
            //    }

            //    //Check if Future checkbox is checked
            //    if (CK_Future.IsChecked.Value)
            //    {
            //        try
            //        {
            //            await Task.Run(() => Downloader(packsLinks[4], downloadFolder + @"\Future.html"));

            //            string thisUri = installationFolder + @"\Future.json";
            //            if (!File.Exists(thisUri))
            //            {
            //                await Task.Run(() => Downloader(jsonLinks[4], thisUri));
            //            }

            //            string jsonController = Reader(thisUri);
            //            ModPack Pack = JsonConvert.DeserializeObject<ModPack>(jsonController);
            //            Packs packsController = packVersionController();

            //            packsController.Future = Pack.Version;

            //            string packsJson = JsonConvert.SerializeObject(packsController);

            //            using (FileStream packsJsonFile = File.Create(installationFolder + @"\versions.json"))
            //            {
            //                byte[] content = new UTF8Encoding(true).GetBytes(packsJson);
            //                packsJsonFile.Write(content, 0, content.Length);
            //                packsJsonFile.Close();
            //            }

            //            Led_Future.Fill = Brushes.Green;
            //            Led_Future.Visibility = Visibility.Visible;
            //        }
            //        catch (Exception error)
            //        {
            //            MessageBox.Show(error.Message);
            //        }
            //    }

            //    //Open the download folder and instructions
            //    Process.Start(downloadFolder);
            //    instructions = new Instructions();
            //    await Task.Run(() => instructions.Show());
            //}
            //catch (Exception error)
            //{
            //    //Send Crash report via CrashReportDotNet API
            //    App.SendReport(error, "Download failed" + error.Message);
            //}
        }

        private void Btn_Tutorial_Click(object sender, RoutedEventArgs e)
        {
            instructions = new Instructions();
            instructions.Show();
        }

        private void Btn_Visualizza_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    string versionJson = Reader(installationFolder + @"\versions.json");
            //    Packs packsController = JsonConvert.DeserializeObject<Packs>(versionJson);
            //    MessageBox.Show("Demo: " + packsController.Demo + ".\n" +
            //        "Lite: " + packsController.Lite + ".\n" +
            //        "Standard: " + packsController.Standard + ".\n" +
            //        "Old Times: " + packsController.OldTimes + ".\n" +
            //        "Future: " + packsController.Future + ".",
            //        "Versions");
            //}
            //catch (Exception error)
            //{
            //    //Send Crash report via CrashReportDotNet API
            //    App.SendReport(error, "An error occurred during the installed packs check" + error.Message);
            //}

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
                else if (!File.Exists(installationFolder + @"\VulkanIsPresent.txt") && dialogOK)
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
            catch (Exception error)
            {
                //Send Crash report via CrashReportDotNet API
                App.SendReport(error, "Vulkan API download failed" + error.Message);
            }
        }

        private void Btn_Start_Click(object sender, RoutedEventArgs e)
        {
            if (!vulkan) { return; }

            string arma3Folder = Reader(installationFolder + @"\VulkanIsPresent.txt", true);

            Process.Start(arma3Folder + @"\arma3launcher.exe");
            this.Close();
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
            catch (Exception error)
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
            catch (Exception error)
            {
                //Send Crash report via CrashReportDotNet API
                App.SendReport(error, "failed to disable Vulkan API" + error.Message);
            }
        }
        #endregion

        //Re-callable Downloader
        void Downloader(string dwLink, string uri)
        {
            try
            {
                WebClient wc = new WebClient();
                wc.DownloadFile(dwLink, uri);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        #region Readers
        //Re-callable Readers (overloaded for single lines reading)
        string Reader(string uri)
        {
            reader = new StreamReader(uri);
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
        #endregion

        //Function with all first start procedures (check presence and create basic files/roots)
        void FirstStartControl()
        {
            this.Dispatcher.Invoke(() =>
            {
                if (!Directory.Exists(installationFolder + @"\Download"))
                {
                    Directory.CreateDirectory(installationFolder + @"\Download");
                }

                dbManager.InizializeDB(jsonReadURL);
            });
        }

        //Function to check packs presence/update
        //Finish DB Conversion
        void OpeningChecker()
        {
            //Create the database connection
            var db = new SQLiteConnection(dbURI);

            this.Dispatcher.Invoke(() =>
            {
                //Create a query object with all modpacks informations
                ModPacks[] modPacks = db.Query<ModPacks>("SELECT * FROM ModPacks").ToArray();
                //Starts the web client
                WebClient wc = new WebClient();

                //Iterate all modpacks and checks their versions
                for (int i = 0; i < modPacks.Length; i++)
                {
                    //Reads the version from the link saved inside the DB and deserialize it
                    string versionJson = wc.DownloadString(modPacks[i].VersionURL);
                    ModPack singleModPack = JsonConvert.DeserializeObject<ModPack>(versionJson);

                    //For every single modpack checks the version and sets the UI state, then gives notification
                    if (modPacks[i].Version < singleModPack.Version)
                    {
                        switch (modPacks[i].Name)
                        {
                            case "Demo":
                                InstalledPacksCheck(0);
                                Led_Demo.Fill = Brushes.Red;
                                break;

                            case "Lite":
                                InstalledPacksCheck(1);
                                Led_Lite.Fill = Brushes.Red;
                                break;

                            case "Standard":
                                InstalledPacksCheck(2);
                                Led_Standard.Fill = Brushes.Red;
                                break;

                            case "Old Times":
                                InstalledPacksCheck(3);
                                Led_OldTimes.Fill = Brushes.Red;
                                break;

                            case "Future":
                                InstalledPacksCheck(4);
                                Led_Future.Fill = Brushes.Red;
                                break;

                            default:
                                break;
                        }

                        MessageBox.Show(modPacks[i].Name + " update found!");
                    }
                }

                VulkanFiles[] vulkanFiles = db.Query<VulkanFiles>("SELECT * FROM VulkanFiles").ToArray();

                //Check if the VulkanAPIs are installed
                if (vulkanFiles[0].Downloaded > 0 && vulkanFiles[1].Downloaded > 0)
                {
                    //Read the ArmA3 directory
                    Paths[] paths = db.Query<Paths>("SELECT * FROM Paths WHERE Name = 'ArmaDirectory'").ToArray();
                    string arma3Folder = paths[0].PathURI;

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

        //Modular function to light up the "presence led" of a pack
        void InstalledPacksCheck(int ledId)
        {
            switch (ledId)
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
    }

    public class ModPack
    {
        public float Version { get; set; }
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
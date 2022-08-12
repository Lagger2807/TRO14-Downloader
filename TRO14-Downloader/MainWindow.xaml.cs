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

        //Global installation folders
        public string installationFolder = AppDomain.CurrentDomain.BaseDirectory;
        public string dbURI;

        //Global variables
        bool[] modPacksCheckBoxesState = new bool[] { false, false, false, false, false };

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

            dbURI = Path.Combine(installationFolder, "Downloader.db");
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
            try
            {
                //Declare check variable to see if something has been checked
                bool isSomethingSelected = false;

                //Starts SQL and Web Client connections
                var db = new SQLiteConnection(dbURI);
                WebClient webClient = new WebClient();

                //Create the paths object with the download folder location
                Paths[] downloadFolderPath = db.Query<Paths>("SELECT * FROM Paths WHERE Name = 'DownloadFolder'").ToArray();

                //For every checkbox execute all the instructions
                for (int i = 0; i < modPacksCheckBoxesState.Length; i++)
                {
                    //Check if the box is checked
                    if (modPacksCheckBoxesState[i])
                    {
                        //Set the variable on true, meaning that at leat 1 box was checked
                        isSomethingSelected = true;

                        //Check which box was selected and execute the right code (every comment in case 0)
                        switch (i)
                        {
                            case 0:
                                //Select for the right modpack (hardcoded for now) and starts a threaded downloader instance with download information
                                ModPacks[] modPacks_Demo = db.Query<ModPacks>("SELECT * FROM ModPacks WHERE Name = 'Demo'").ToArray();
                                await Task.Run(() => Downloader(modPacks_Demo[0].DownloadURL, downloadFolderPath[0].PathURI + @"\Demo.html"));

                                //Reads the GitHub json and deserialize it in a single modpack object then updates the db
                                string demoJson = webClient.DownloadString(modPacks_Demo[0].VersionURL);
                                ModPack demoModPack = JsonConvert.DeserializeObject<ModPack>(demoJson);
                                db.Query<ModPacks>("UPDATE ModPacks SET Version = '" + demoModPack.Version + "', Downloaded = 1 WHERE Name = 'Demo'");

                                //Updates the UI
                                Led_Demo.Fill = Brushes.Green;
                                Led_Demo.Visibility = Visibility.Visible;
                                break;

                            case 1:
                                ModPacks[] modPacks_Lite = db.Query<ModPacks>("SELECT * FROM ModPacks WHERE Name = 'Lite'").ToArray();
                                await Task.Run(() => Downloader(modPacks_Lite[0].DownloadURL, downloadFolderPath[0].PathURI + @"\Lite.html"));

                                string liteJson = webClient.DownloadString(modPacks_Lite[0].VersionURL);
                                ModPack liteModPack = JsonConvert.DeserializeObject<ModPack>(liteJson);
                                db.Query<ModPacks>("UPDATE ModPacks SET Version = '" + liteModPack.Version + "', Downloaded = 1 WHERE Name = 'Lite'");

                                Led_Lite.Fill = Brushes.Green;
                                Led_Lite.Visibility = Visibility.Visible;
                                break;

                            case 2:
                                ModPacks[] modPacks_Standard = db.Query<ModPacks>("SELECT * FROM ModPacks WHERE Name = 'Standard'").ToArray();
                                MessageBox.Show(downloadFolderPath[0].PathURI);
                                await Task.Run(() => Downloader(modPacks_Standard[0].DownloadURL, downloadFolderPath[0].PathURI + @"\Standard.html"));

                                string standardJson = webClient.DownloadString(modPacks_Standard[0].VersionURL);
                                ModPack standardModPack = JsonConvert.DeserializeObject<ModPack>(standardJson);
                                db.Query<ModPacks>("UPDATE ModPacks SET Version = '" + standardModPack.Version + "', Downloaded = 1 WHERE Name = 'Standard'");

                                Led_Standard.Fill = Brushes.Green;
                                Led_Standard.Visibility = Visibility.Visible;
                                break;

                            case 3:
                                ModPacks[] modPacks_OldTimes = db.Query<ModPacks>("SELECT * FROM ModPacks WHERE Name = 'Old Times'").ToArray();
                                await Task.Run(() => Downloader(modPacks_OldTimes[0].DownloadURL, downloadFolderPath[0].PathURI + @"\OldTimes.html"));

                                string oldTimeJson = webClient.DownloadString(modPacks_OldTimes[0].VersionURL);
                                ModPack oldTimeModPack = JsonConvert.DeserializeObject<ModPack>(oldTimeJson);
                                db.Query<ModPacks>("UPDATE ModPacks SET Version = '" + oldTimeModPack.Version + "', Downloaded = 1 WHERE Name = 'Old Times'");

                                Led_OldTimes.Fill = Brushes.Green;
                                Led_OldTimes.Visibility = Visibility.Visible;
                                break;

                            case 4:
                                ModPacks[] modPacks_Future = db.Query<ModPacks>("SELECT * FROM ModPacks WHERE Name = 'Future'").ToArray();
                                await Task.Run(() => Downloader(modPacks_Future[0].DownloadURL, downloadFolderPath[0].PathURI + @"\Future.html"));

                                string futureJson = webClient.DownloadString(modPacks_Future[0].VersionURL);
                                ModPack futureModPack = JsonConvert.DeserializeObject<ModPack>(futureJson);
                                db.Query<ModPacks>("UPDATE ModPacks SET Version = '" + futureModPack.Version + "', Downloaded = 1 WHERE Name = 'Future'");

                                Led_Future.Fill = Brushes.Green;
                                Led_Future.Visibility = Visibility.Visible;
                                break;

                            default:
                                break;
                        }
                    }
                }

                //Check if something was selected then execute the post download guide
                if(isSomethingSelected)
                {
                    //Open the download folder and instructions
                    Process.Start(downloadFolderPath[0].PathURI);

                    instructions = new Instructions();
                    instructions.Show();
                }
                
                //Dispose all the complex objects
                db.Dispose();
                webClient.Dispose();
            }
            catch (Exception error)
            {
                //Send Crash report via CrashReportDotNet API
                App.SendReport(error, "Download failed" + error.Message);
            }
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

        #region CheckBoxes Click events
        private void CK_Demo_Click(object sender, RoutedEventArgs e)
        {
            modPacksCheckBoxesState[0] = CK_Demo.IsChecked.Value;
        }

        private void CK_Lite_Click(object sender, RoutedEventArgs e)
        {
            modPacksCheckBoxesState[1] = CK_Lite.IsChecked.Value;
        }

        private void CK_Standard_Click(object sender, RoutedEventArgs e)
        {
            modPacksCheckBoxesState[2] = CK_Standard.IsChecked.Value;
        }

        private void CK_OldTimes_Click(object sender, RoutedEventArgs e)
        {
            modPacksCheckBoxesState[3] = CK_OldTimes.IsChecked.Value;
        }

        private void CK_Future_Click(object sender, RoutedEventArgs e)
        {
            modPacksCheckBoxesState[4] = CK_Future.IsChecked.Value;
        }
        #endregion
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

                var db = new SQLiteConnection(dbURI);

                Paths[] paths = db.Query<Paths>("SELECT * FROM Paths WHERE Name = 'ArmaDirectory'").ToArray();

                if (paths.Length <= 0)
                {
                    Paths newPath = new Paths { Name = "ArmaDirectory" };

                    //Open folder selection dialog to user
                    System.Windows.Forms.FolderBrowserDialog folderDlg = new System.Windows.Forms.FolderBrowserDialog();
                    folderDlg.ShowNewFolderButton = true; //Enables new folders creation
                    folderDlg.Description = "Select the folder where Arma3.exe is contained"; //Sets a description for the folder dialog window

                    //Show dialog to user
                    System.Windows.Forms.DialogResult result = folderDlg.ShowDialog();

                    //Check if a folder as been selected and assign it to a variable, else it chooses the default (desktop) folder
                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        newPath.PathURI = folderDlg.SelectedPath;
                    }
                    else
                    {
                        //Set the ArmA3 folder as the desktop folder
                        newPath.PathURI = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    }

                    db.Insert(newPath);
                    folderDlg.Dispose();
                }
                
                Paths[] downloadQuery = db.Query<Paths>("SELECT * FROM Paths WHERE Name = 'DownloadFolder'").ToArray();
                if(downloadQuery.Length <= 0)
                {
                    Paths downloadPath = new Paths { Name = "DownloadFolder", PathURI = Path.Combine(installationFolder, "Download") };
                    db.Insert(downloadPath);
                }

                db.Dispose();
            });
        }

        //Function to check packs presence/update
        void OpeningChecker()
        {
            this.Dispatcher.Invoke(() =>
            {
                //Create the database connection
                var db = new SQLiteConnection(dbURI);

                //Create a query object with all modpacks informations
                ModPacks[] modPacks = db.Query<ModPacks>("SELECT * FROM ModPacks").ToArray();

                //Starts the web client
                WebClient webClient = new WebClient();

                //Iterate all modpacks and checks their versions
                for (int i = 0; i < modPacks.Length; i++)
                {
                    //Reads the version from the link saved inside the DB and deserialize it
                    string versionJson = webClient.DownloadString(modPacks[i].VersionURL);
                    ModPack singleModPack = JsonConvert.DeserializeObject<ModPack>(versionJson);

                    //For every single modpack checks if it's installed and sets the UI
                    if (modPacks[i].Downloaded > 0)
                    {
                        InstalledPacksCheck(i);

                        //try parsing the text value because sqlite doesn't support floats...
                        float.TryParse(modPacks[i].Version.ToString(), out float floatVersion);

                        //Then checks if the modpack is up to date and show the alert
                        if (floatVersion < singleModPack.Version)
                        {
                            switch (modPacks[i].Name)
                            {
                                case "Demo":
                                    Led_Demo.Fill = Brushes.Red;
                                    break;

                                case "Lite":
                                    Led_Lite.Fill = Brushes.Red;
                                    break;

                                case "Standard":
                                    Led_Standard.Fill = Brushes.Red;
                                    break;

                                case "Old Times":
                                    Led_OldTimes.Fill = Brushes.Red;
                                    break;

                                case "Future":
                                    Led_Future.Fill = Brushes.Red;
                                    break;

                                default:
                                    break;
                            }

                            MessageBox.Show(modPacks[i].Name + " update found!");
                        }
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

                db.Dispose();
                webClient.Dispose();
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
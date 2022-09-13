using CrashReporterDotNET;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using TRO14_Downloader.CustomClasses;

namespace TRO14_Downloader
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Global installation folders
        public string installationFolder = AppDomain.CurrentDomain.BaseDirectory;
        public string dbURI;

        //Global variables
        bool[] modPacksCheckBoxesState = new bool[] { false, false, false, false, false };

        //Cached objects
        DBManager dbManager = new DBManager();
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
                //Random background extractor
                RandomStartupBackground();

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
                Paths[] downloadFolderQuery = db.Query<Paths>("SELECT * FROM Paths WHERE Name = 'DownloadFolder'").ToArray();
                string downloadFolderPath = downloadFolderQuery[0].PathURI;

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
                                await Task.Run(() => Downloader(modPacks_Demo[0].DownloadURL, downloadFolderPath + @"\Demo.html"));

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
                                await Task.Run(() => Downloader(modPacks_Lite[0].DownloadURL, downloadFolderPath + @"\Lite.html"));

                                string liteJson = webClient.DownloadString(modPacks_Lite[0].VersionURL);
                                ModPack liteModPack = JsonConvert.DeserializeObject<ModPack>(liteJson);
                                db.Query<ModPacks>("UPDATE ModPacks SET Version = '" + liteModPack.Version + "', Downloaded = 1 WHERE Name = 'Lite'");

                                Led_Lite.Fill = Brushes.Green;
                                Led_Lite.Visibility = Visibility.Visible;
                                break;

                            case 2:
                                ModPacks[] modPacks_Standard = db.Query<ModPacks>("SELECT * FROM ModPacks WHERE Name = 'Standard'").ToArray();
                                await Task.Run(() => Downloader(modPacks_Standard[0].DownloadURL, downloadFolderPath + @"\Standard.html"));

                                string standardJson = webClient.DownloadString(modPacks_Standard[0].VersionURL);
                                ModPack standardModPack = JsonConvert.DeserializeObject<ModPack>(standardJson);
                                db.Query<ModPacks>("UPDATE ModPacks SET Version = '" + standardModPack.Version + "', Downloaded = 1 WHERE Name = 'Standard'");

                                Led_Standard.Fill = Brushes.Green;
                                Led_Standard.Visibility = Visibility.Visible;
                                break;

                            case 3:
                                ModPacks[] modPacks_OldTimes = db.Query<ModPacks>("SELECT * FROM ModPacks WHERE Name = 'Old Times'").ToArray();
                                await Task.Run(() => Downloader(modPacks_OldTimes[0].DownloadURL, downloadFolderPath + @"\OldTimes.html"));

                                string oldTimeJson = webClient.DownloadString(modPacks_OldTimes[0].VersionURL);
                                ModPack oldTimeModPack = JsonConvert.DeserializeObject<ModPack>(oldTimeJson);
                                db.Query<ModPacks>("UPDATE ModPacks SET Version = '" + oldTimeModPack.Version + "', Downloaded = 1 WHERE Name = 'Old Times'");

                                Led_OldTimes.Fill = Brushes.Green;
                                Led_OldTimes.Visibility = Visibility.Visible;
                                break;

                            case 4:
                                ModPacks[] modPacks_Future = db.Query<ModPacks>("SELECT * FROM ModPacks WHERE Name = 'Future'").ToArray();
                                await Task.Run(() => Downloader(modPacks_Future[0].DownloadURL, downloadFolderPath + @"\Future.html"));

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
                    Process.Start(downloadFolderPath);

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
            try
            {
                var db = new SQLiteConnection(dbURI);
                ModPacks[] modPacks = db.Query<ModPacks>("SELECT * FROM ModPacks").ToArray();

                MessageBox.Show("Demo: " + modPacks[0].Version + ".\n" +
                    "Lite: " + modPacks[1].Version + ".\n" +
                    "Standard: " + modPacks[2].Version + ".\n" +
                    "Old Times: " + modPacks[3].Version + ".\n" +
                    "Future: " + modPacks[4].Version + ".", "Installed versions");

                db.Dispose();
            }
            catch (Exception error)
            {
                App.SendReport(error, "An error occurred during the version check" + error.Message);
            }
        }

        private void Btn_Vulkan_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Starts SQL connections
                var db = new SQLiteConnection(dbURI);

                //Create the paths object with the download folder location
                Paths[] armaPath = db.Query<Paths>("SELECT * FROM Paths WHERE Name = 'ArmaDirectory'").ToArray();
                string armaDirectory = armaPath[0].PathURI;

                //Create the vulkan files query
                VulkanFiles[] vulkanFiles = db.Query<VulkanFiles>("SELECT * FROM VulkanFiles").ToArray();
                
                //for each item in the query downloads in the arma folder and sets it to downloaded
                foreach(VulkanFiles item in vulkanFiles)
                {
                    Downloader(item.DownloadURL, armaDirectory + @"\" + item.Name);

                    //Checks if the files exists in the arma folder, if true sets it to downloaded
                    if (File.Exists(armaDirectory + @"\" + item.Name))
                        db.Query<VulkanFiles>("UPDATE VulkanFiles SET Downloaded = " + 1 + " WHERE Name = '" + item.Name + "'");
                }

                //Updates the UI
                CK_Vulkan.Visibility = Visibility.Visible;
                CK_Vulkan.IsChecked = true;
                CK_Vulkan.IsEnabled = true;
                Text_VulkanIsPresent.Visibility = Visibility.Visible;
                Text_VulkanIsPresent.Content = "Vulkan API Present";
                Text_VulkanIsPresent.FontWeight = FontWeights.Normal;
                Led_Vulkan.Visibility = Visibility.Visible;
                Led_Vulkan.Fill = Brushes.ForestGreen;

                //Disposes the db connection
                db.Dispose();
            }
            catch (Exception error)
            {
                //Send Crash report via CrashReportDotNet API
                App.SendReport(error, "Vulkan API download failed" + error.Message);
            }
        }

        private void Btn_Start_Click(object sender, RoutedEventArgs e)
        {
            var db = new SQLiteConnection(dbURI);
            Paths[] path = db.Query<Paths>("SELECT * FROM Paths WHERE Name = 'ArmaDirectory'").ToArray();
            db.Dispose();

            Process.Start(path[0].PathURI + @"\arma3launcher.exe");
            this.Close();
        }

        private void Btn_Info_Click(object sender, RoutedEventArgs e)
        {
            //change generic github link with wiki link
            Process.Start("www.github.com"); //Process.Start seems to be able to open web pages too... cool...
        }

        private void Btn_DLL_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Starts SQL connections
                var db = new SQLiteConnection(dbURI);

                //Create the paths object with the download folder location
                Paths[] armaPath = db.Query<Paths>("SELECT * FROM Paths").ToArray();
                string armaDirectory = armaPath[0].PathURI;

                //Create the allocators query
                AllocDLLs[] allocators = db.Query<AllocDLLs>("SELECT * FROM AllocDLLs").ToArray();

                //for each item in the query downloads in the arma folder and sets it to downloaded
                foreach (AllocDLLs item in allocators)
                {
                    Downloader(item.DownloadURL, armaDirectory + @"\Dll\" + item.Name);

                    //Checks if the files exists in the arma folder, if true sets it to downloaded
                    if (File.Exists(armaDirectory + @"\Dll\" + item.Name))
                        db.Query<VulkanFiles>("UPDATE AllocDLLs SET Downloaded = " + 1 + " WHERE Name = '" + item.Name + "'");
                }

                //Updates the UI
                Btn_Allocs.IsEnabled = false;

                //Disposes the db connection
                db.Dispose();
            }
            catch(Exception error)
            {
                //Send Crash report via CrashReportDotNet API
                App.SendReport(error, "Custom allocators download failed" + error.Message);
            }
        }

        private void Btn_Profile_Click(object sender, RoutedEventArgs e)
        {

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

        private async void CK_Vulkan_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Create the db connection
                var db = new SQLiteConnection(dbURI);

                //Gets the arma directory path
                Paths[] armaPath = db.Query<Paths>("SELECT * FROM Paths WHERE Name = 'ArmaDirectory'").ToArray();
                string armaDirectory = armaPath[0].PathURI;

                //Creates the vulkaFiles object from query
                VulkanFiles[] vulkanFiles = db.Query<VulkanFiles>("SELECT * FROM VulkanFiles").ToArray();

                bool vulkanState = false;

                foreach (VulkanFiles item in vulkanFiles)
                {
                    //Checks if files are active AND the checkbox is NOT Checked then deactives them (else it activates them)
                    //why? bc the checkbox state change on click and this execute after it so we need to check it with reverse psychology
                    if (item.Active > 0 && CK_Vulkan.IsChecked == false)
                    {
                        await Task.Run(() => File.Move(armaDirectory + @"\" + item.Name, armaDirectory + @"\" + item.Name + "OFF"));
                        db.Query<VulkanFiles>("UPDATE VulkanFiles SET Active = 0 WHERE Name = '" + item.Name + "'");

                        vulkanState = false;
                    }
                    else if (item.Active <= 0 && CK_Vulkan.IsChecked == true)
                    {
                        await Task.Run(() => File.Move(armaDirectory + @"\" + item.Name + "OFF", armaDirectory + @"\" + item.Name));
                        db.Query<VulkanFiles>("UPDATE VulkanFiles SET Active = 1 WHERE Name = '" + item.Name + "'");

                        vulkanState = true;
                    }
                }

                db.Dispose();

                //Shows a message based on the new Vulkan state
                if (vulkanState)
                    MessageBox.Show("Disable the Battleye™ service in the ArmA III launcher to start the game.", "Vulkan enabled");
                else if (!vulkanState)
                    MessageBox.Show("You can enable the Battleye™ service in the ArmA III launcher.", "Vulkan disabled");
            }
            catch (Exception error)
            {
                //Send Crash report via CrashReportDotNet API
                App.SendReport(error, "failed to change Vulkan API state" + error.Message);
            }
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
                wc.Dispose();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        #region Starting functions
        //Function that extract a random background from the "Backgrounds" folder
        void RandomStartupBackground()
        {
            string[] backgroundImages = Directory.GetFiles(installationFolder + @"\Backgrounds", "*.jpg");

            if(backgroundImages.Length <= 0) { return; }

            Random random = new Random();
            int backgroundIndex = random.Next(0, backgroundImages.Length);

            ImageSource background = new BitmapImage(new Uri(backgroundImages[backgroundIndex]));
            BackGroundXAMLImage.ImageSource = background;
        }

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

                //Read the ArmA3 directory
                Paths[] paths = db.Query<Paths>("SELECT * FROM Paths WHERE Name = 'ArmaDirectory'").ToArray();
                string arma3Folder = paths[0].PathURI;

                if (!File.Exists(arma3Folder + @"\arma3launcher.exe"))
                    Btn_Start.IsEnabled = false;

                #region ModPacks

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

                #endregion

                #region VulkanFiles

                VulkanFiles[] vulkanFiles = db.Query<VulkanFiles>("SELECT * FROM VulkanFiles").ToArray();

                bool vulkanDownloaded = true;

                //Check if the VulkanAPI files are downloaded
                foreach (VulkanFiles item in vulkanFiles)
                {
                    if (item.Downloaded <= 0)
                    {
                        vulkanDownloaded = false;
                    }
                }

                //Check if the VulkanAPIs are installed
                if (vulkanDownloaded)
                {
                    //Show the VulkanAPI checkbox, text and led while disabling the download button
                    CK_Vulkan.Visibility = Visibility.Visible;
                    Text_VulkanIsPresent.Visibility = Visibility.Visible;
                    Led_Vulkan.Visibility = Visibility.Visible;

                    bool vulkanInstalled = true;

                    //Mass check if the VulkanAPI files are correctly present in one of their states, if not, disables everything and make you re-download them
                    if (File.Exists(arma3Folder + @"\d3d11.dll") && File.Exists(arma3Folder + @"\dxgi.dll"))
                    {
                        CK_Vulkan.IsChecked = true;
                        db.Query<VulkanFiles>("UPDATE VulkanFiles SET Active = 1");
                    }
                    else if (File.Exists(arma3Folder + @"\d3d11.dllOFF") && File.Exists(arma3Folder + @"\dxgi.dllOFF"))
                    {
                        CK_Vulkan.IsChecked = false;
                        db.Query<VulkanFiles>("UPDATE VulkanFiles SET Active = 0");
                    }
                    else
                    {
                        CK_Vulkan.IsEnabled = false;
                        db.Query<VulkanFiles>("UPDATE VulkanFiles SET Active = 0");

                        vulkanInstalled = false;

                        MessageBox.Show("Vulkan API damaged, UI enabler disabled");
                    }

                    if(vulkanInstalled)
                    {
                        Btn_Vulkan.IsEnabled = false;
                    }
                    else if(!vulkanInstalled)
                    {
                        CK_Vulkan.IsEnabled = false;
                        Text_VulkanIsPresent.Content = "Vulkan API damaged!";
                        Text_VulkanIsPresent.FontWeight = FontWeights.Bold;
                        Led_Vulkan.Fill = Brushes.Red;
                    }
                }

                #endregion

                #region DLLs

                AllocDLLs[] allocators = db.Query<AllocDLLs>("SELECT * FROM AllocDLLs").ToArray();

                bool allocatorsPresent = true;

                foreach(AllocDLLs item in allocators)
                {
                    if(item.Downloaded <= 0)
                    {
                        allocatorsPresent = false;
                    }    
                }

                if(allocatorsPresent)
                {
                    Btn_Allocs.IsEnabled = false;
                }

                #endregion

                //Disposes both db and web client objects
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

        #endregion
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
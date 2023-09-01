using CrashReporterDotNET;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
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
        public string armaDirectory;
        public string downloadFolder;
        public string dbURI;

        //Global variables
        bool[] modPacksCheckBoxesState = new bool[] { false, false, false, false, false };

        //Cached objects
        DBManager dbManager = new DBManager();
        WebClient WC = new WebClient();
        Instructions Instructions;
        Profile_Instructions ProfileInstructions;

        //Global constants
        public const string jsonReadURL = "https://raw.githubusercontent.com/Lagger2807/TRO14-Files/main/DB%20Inizializer.json";

        public MainWindow()
        {
            InitializeComponent();

            dbURI = Path.Combine(installationFolder, "Downloader.db");
        }

        #region Events
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //Random background extractor
                RandomStartupBackground();

                FirstStartControl();

                OpeningChecker();
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

                List<ModPacks> modPacks = db.Query<ModPacks>("SELECT * FROM ModPacks");

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
                                ModPacks modPackDemo = modPacks.Find(
                                    delegate (ModPacks MP)
                                    {
                                        return MP.Name == "Demo";
                                    }
                                    );
                                await Task.Run(() => Downloader(modPackDemo.DownloadURL, downloadFolder + @"\Demo.html"));

                                //Reads the GitHub json and deserialize it in a single modpack object then updates the db
                                string demoJson = WC.DownloadString(modPackDemo.VersionURL);
                                ModPack demoModPack = JsonConvert.DeserializeObject<ModPack>(demoJson);
                                db.Query<ModPacks>("UPDATE ModPacks SET Version = '" + demoModPack.Version + "', Downloaded = 1 WHERE Name = 'Demo'");

                                //Updates the UI
                                Led_Demo.Fill = Brushes.Green;
                                Led_Demo.Visibility = Visibility.Visible;
                                break;

                            case 1:
                                ModPacks modPackLite = modPacks.Find(
                                    delegate (ModPacks MP)
                                    {
                                        return MP.Name == "Lite";
                                    }
                                    );
                                await Task.Run(() => Downloader(modPackLite.DownloadURL, downloadFolder + @"\Lite.html"));

                                string liteJson = WC.DownloadString(modPackLite.VersionURL);
                                ModPack liteModPack = JsonConvert.DeserializeObject<ModPack>(liteJson);
                                db.Query<ModPacks>("UPDATE ModPacks SET Version = '" + liteModPack.Version + "', Downloaded = 1 WHERE Name = 'Lite'");

                                Led_Lite.Fill = Brushes.Green;
                                Led_Lite.Visibility = Visibility.Visible;
                                break;

                            case 2:
                                ModPacks modPackStandard = modPacks.Find(
                                    delegate (ModPacks MP)
                                    {
                                        return MP.Name == "Standard";
                                    }
                                    );
                                await Task.Run(() => Downloader(modPackStandard.DownloadURL, downloadFolder + @"\Standard.html"));

                                string standardJson = WC.DownloadString(modPackStandard.VersionURL);
                                ModPack standardModPack = JsonConvert.DeserializeObject<ModPack>(standardJson);
                                db.Query<ModPacks>("UPDATE ModPacks SET Version = '" + standardModPack.Version + "', Downloaded = 1 WHERE Name = 'Standard'");

                                Led_Standard.Fill = Brushes.Green;
                                Led_Standard.Visibility = Visibility.Visible;
                                break;

                            case 3:
                                ModPacks modPackOldTimes = modPacks.Find(
                                    delegate (ModPacks MP)
                                    {
                                        return MP.Name == "Old Times";
                                    }
                                    );
                                await Task.Run(() => Downloader(modPackOldTimes.DownloadURL, downloadFolder + @"\OldTimes.html"));

                                string oldTimeJson = WC.DownloadString(modPackOldTimes.VersionURL);
                                ModPack oldTimeModPack = JsonConvert.DeserializeObject<ModPack>(oldTimeJson);
                                db.Query<ModPacks>("UPDATE ModPacks SET Version = '" + oldTimeModPack.Version + "', Downloaded = 1 WHERE Name = 'Old Times'");

                                Led_OldTimes.Fill = Brushes.Green;
                                Led_OldTimes.Visibility = Visibility.Visible;
                                break;

                            case 4:
                                ModPacks modPackFuture = modPacks.Find(
                                    delegate (ModPacks MP)
                                    {
                                        return MP.Name == "Future";
                                    }
                                    );
                                await Task.Run(() => Downloader(modPackFuture.DownloadURL, downloadFolder + @"\Future.html"));

                                string futureJson = WC.DownloadString(modPackFuture.VersionURL);
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
                    Process.Start(downloadFolder);

                    Instructions = new Instructions();
                    Instructions.Show();
                }
                
                //Dispose all the complex objects
                db.Close();
            }
            catch (Exception error)
            {
                //Send Crash report via CrashReportDotNet API
                App.SendReport(error, "Download failed" + error.Message);
            }
        }

        private void Btn_Tutorial_Click(object sender, RoutedEventArgs e)
        {
            Instructions = new Instructions();
            Instructions.Show();
        }

        private void Btn_Visualizza_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var db = new SQLiteConnection(dbURI);
                ModPacks[] modPacks = db.Query<ModPacks>("SELECT * FROM ModPacks").ToArray();

                List<ModPacks> queriedModPacks = db.Query<ModPacks>("SELECT * FROM ModPacks");

                string modPacksVersionList = "";

                foreach(ModPacks modPack in queriedModPacks)
                {
                    modPacksVersionList += modPack.Name + ": " + modPack.Version + ". \n";
                }

                MessageBox.Show(modPacksVersionList, "Installed versions");

                db.Close();
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

                List<VulkanFiles> queriedVulkanFiles = db.Query<VulkanFiles>("SELECT * FROM VulkanFiles");
                
                //for each item in the query downloads in the arma folder and sets it to downloaded
                foreach(VulkanFiles vulkanFile in queriedVulkanFiles)
                {
                    Downloader(vulkanFile.DownloadURL, armaDirectory + @"\" + vulkanFile.Name);

                    //Checks if the files exists in the arma folder, if true sets it to downloaded
                    if (File.Exists(armaDirectory + @"\" + vulkanFile.Name))
                        db.Query<VulkanFiles>("UPDATE VulkanFiles SET Downloaded = " + 1 + " WHERE Name = '" + vulkanFile.Name + "'");
                }

                //Updates the UI
                CK_Vulkan.Visibility = Visibility.Visible;
                CK_Vulkan.IsChecked = true;
                CK_Vulkan.IsEnabled = true;
                Text_Vulkan.Visibility = Visibility.Visible;
                Text_VulkanIsPresent.Visibility = Visibility.Visible;
                Text_VulkanIsPresent.Content = "Vulkan API Present";
                Text_VulkanIsPresent.FontWeight = FontWeights.Normal;
                Led_Vulkan.Visibility = Visibility.Visible;
                Led_Vulkan.Fill = Brushes.ForestGreen;

                //Disposes the db connection
                db.Close();
            }
            catch (Exception error)
            {
                //Send Crash report via CrashReportDotNet API
                App.SendReport(error, "Vulkan API download failed" + error.Message);
            }
        }

        private void Btn_Start_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(armaDirectory + @"\arma3launcher.exe");
            this.Close();
        }

        private void Btn_Info_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/Lagger2807/TRO14-Downloader/wiki/Advanced-options-info"); //Process.Start seems to be able to open web pages too... cool...
        }

        private void Btn_DLL_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Starts SQL connections
                var db = new SQLiteConnection(dbURI);

                List<AllocDLLs> queriedAllocators = db.Query<AllocDLLs>("SELECT * FROM AllocDLLs");

                //for each item in the query downloads in the arma folder and sets it to downloaded
                foreach (AllocDLLs allocator in queriedAllocators)
                {
                    Downloader(allocator.DownloadURL, armaDirectory + @"\Dll\" + allocator.Name);

                    //Checks if the files exists in the arma folder, if true sets it to downloaded
                    if (File.Exists(armaDirectory + @"\Dll\" + allocator.Name))
                        db.Query<AllocDLLs>("UPDATE AllocDLLs SET Downloaded = " + 1 + " WHERE Name = '" + allocator.Name + "'");
                }

                //Updates the UI
                Btn_Allocs.IsEnabled = false;

                //Disposes the db connection
                db.Close();
            }
            catch(Exception error)
            {
                //Send Crash report via CrashReportDotNet API
                App.SendReport(error, "Custom allocators download failed" + error.Message);
            }
        }

        private void Btn_Profile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Starts SQL connections
                var db = new SQLiteConnection(dbURI);

                List<Profiles> queriedProfiles = db.Query<Profiles>("SELECT * FROM Profiles");

                //Create the profile folder if not exists
                if(!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Arma 3 - Other Profiles\DefaultProfile\"))
                    Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Arma 3 - Other Profiles\DefaultProfile\");

                //for each item in the query downloads in the "documents/Arma 3 - other profiles" and sets it to downloaded
                foreach (Profiles profile in queriedProfiles)
                {
                    Downloader(profile.DownloadURL, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Arma 3 - Other Profiles\DefaultProfile\" + profile.Name);

                    //Checks if the files exists in the arma folder, if true sets it to downloaded
                    if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Arma 3 - Other Profiles\DefaultProfile\" + profile.Name))
                        db.Query<Profiles>("UPDATE Profiles SET Downloaded = " + 1 + " WHERE Name = '" + profile.Name + "'");
                }

                //Updates the UI
                Btn_Profile.IsEnabled = false;

                //Disposes the db connection
                db.Close();

                ProfileInstructions = new Profile_Instructions();
                ProfileInstructions.Show();
            }
            catch(Exception error)
            {
                //Send Crash report via CrashReportDotNet API
                App.SendReport(error, "Default profile download failed" + error.Message);
            }
        }

        private void Btn_ChangeDir_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var db = new SQLiteConnection(dbURI);

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
                    //Assign the new URI to the path object
                    newPath.PathURI = folderDlg.SelectedPath;

                    //Update the db element with the updated one
                    db.Update(newPath);
                }

                //Dispose the folder dialog and db object
                folderDlg.Dispose();
                db.Close();
            }
            catch (Exception error)
            {
                //Send Crash report via CrashReportDotNet API
                App.SendReport(error, "Directory change failed" + error.Message);
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
                WC.DownloadFile(dwLink, uri);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        #region Starting functions
        //Function that extracts a random background from the "Backgrounds" folder
        void RandomStartupBackground()
        {
            string[] backgroundImages = Directory.GetFiles(installationFolder + @"\Backgrounds", "*.jpg");

            if(backgroundImages.Length < 1) { return; }

            Random Random = new Random();
            int backgroundIndex = Random.Next(0, backgroundImages.Length);

            ImageSource Background = new BitmapImage(new Uri(backgroundImages[backgroundIndex]));
            BackGroundXAMLImage.ImageSource = Background;
        }

        //Function with all first start procedures (check presence and create basic files/roots)
        void FirstStartControl()
        {   
            if (!Directory.Exists(installationFolder + @"\Download"))
                Directory.CreateDirectory(installationFolder + @"\Download");

            dbManager.InizializeDB(jsonReadURL);

            var db = new SQLiteConnection(dbURI);

            List<Paths> paths = db.Query<Paths>("SELECT * FROM Paths");

            bool armaDirExists = paths.Exists(
                delegate(Paths P) 
                {
                    return P.Name == "ArmaDirectory"; 
                });
            bool downloadFolderExists = paths.Exists(
                delegate (Paths P) 
                {
                    return P.Name == "DownloadFolder"; 
                }
                );

            if (!armaDirExists)
            {
                Paths newPath = new Paths { Name = "ArmaDirectory" };

                //Open folder selection dialog to user
                System.Windows.Forms.FolderBrowserDialog folderDlg = new System.Windows.Forms.FolderBrowserDialog();
                folderDlg.ShowNewFolderButton = true; //Enables new folders creation
                folderDlg.Description = "Select the folder where Arma3.exe is contained, usually inside your default Steam games directory."; //Sets a description for the folder dialog window

                //Show dialog to user
                System.Windows.Forms.DialogResult result = folderDlg.ShowDialog();

                //Check if a folder as been selected and assign it to a variable, else it chooses the default (desktop) folder
                if (result == System.Windows.Forms.DialogResult.OK)
                    newPath.PathURI = folderDlg.SelectedPath;
                else
                    newPath.PathURI = Environment.GetFolderPath(Environment.SpecialFolder.Desktop); //Set the ArmA3 folder as the desktop folder

                armaDirectory = newPath.PathURI;

                db.Insert(newPath);
            }
            else
            {
                armaDirectory = paths.Find(
                    delegate (Paths P)
                    {
                        return P.Name == "ArmaDirectory";
                    }
                    ).PathURI;
            }

            if(!downloadFolderExists)
            {
                Paths downloadPath = new Paths { Name = "DownloadFolder", PathURI = Path.Combine(installationFolder, "Download") };
                downloadFolder = downloadPath.PathURI;
                db.Insert(downloadPath);
            }
            else
            {
                downloadFolder = paths.Find(
                delegate (Paths P)
                {
                    return P.Name == "DownloadFolder";
                }
                ).PathURI;
            }

            db.Close();
        }

        //Function to check packs presence/update
        void OpeningChecker()
        {
            //Create the database connection
            var db = new SQLiteConnection(dbURI);

            if (!File.Exists(armaDirectory + @"\arma3launcher.exe"))
                Btn_Start.IsEnabled = false;
            
            #region ModPacks
            //Put ModPacks informations in to a List
            List<ModPacks> queriedModPacks = db.Query<ModPacks>("SELECT * FROM ModPacks");

            //Cycle each ModPack
            foreach (ModPacks modPack in queriedModPacks)
            {
                //Check if the ModPack was ever downloaded
                if(modPack.Downloaded > 0)
                {
                    //Update installation status in the interface based on ModPack name
                    UpdateModpackInstallStatus(modPack.Name);

                    //Download and deserialize in to an object the Json stream
                    string versionJson = WC.DownloadString(modPack.VersionURL);
                    ModPack singleModPack = JsonConvert.DeserializeObject<ModPack>(versionJson);

                    //Parse the float version of the ModPack based on DB value
                    float.TryParse(modPack.Version.ToString(), out float floatVersion);

                    //Check if the uploaded ModPack is newer than the local one
                    if(floatVersion < singleModPack.Version)
                    {
                        //Update the update status interface based on ModPack name and displays the update notice popup
                        UpdateModpackUpdateStatus(modPack.Name);
                        MessageBox.Show(modPack.Name + " update found!");
                    }
                }
            }
            #endregion

            #region VulkanFiles
            //Put VulkanFiles informations in to a List
            List<VulkanFiles> queriedVulkanFiles = db.Query<VulkanFiles>("SELECT * FROM VulkanFiles");

            //Use a variable to create a default state and change it only based on a DB check
            bool vulkanDownloaded = true;
            foreach (VulkanFiles vulkanFile in queriedVulkanFiles)
            {
                if (vulkanFile.Downloaded < 1)
                    vulkanDownloaded = false;
            }
            //---------------------------------------------------------

            if (vulkanDownloaded)
            {
                //Update interface showing the Vulkan panel
                CK_Vulkan.Visibility = Visibility.Visible;
                Text_Vulkan.Visibility = Visibility.Visible;
                Text_VulkanIsPresent.Visibility = Visibility.Visible;
                Led_Vulkan.Visibility = Visibility.Visible;

                //Mass check if the VulkanAPI files are correctly present in one of their states, if not, disables everything and make you re-download them
                if (File.Exists(armaDirectory + @"\d3d11.dll") && File.Exists(armaDirectory + @"\dxgi.dll"))
                {
                    CK_Vulkan.IsChecked = true;
                    db.Query<VulkanFiles>("UPDATE VulkanFiles SET Active = 1");
                    Btn_Vulkan.IsEnabled = false;
                }
                else if (File.Exists(armaDirectory + @"\d3d11.dllOFF") && File.Exists(armaDirectory + @"\dxgi.dllOFF"))
                {
                    CK_Vulkan.IsChecked = false;
                    db.Query<VulkanFiles>("UPDATE VulkanFiles SET Active = 0");
                    Btn_Vulkan.IsEnabled = false;
                }
                else
                {
                    CK_Vulkan.IsEnabled = false;
                    db.Query<VulkanFiles>("UPDATE VulkanFiles SET Active = 0");

                    CK_Vulkan.IsEnabled = false;
                    Text_VulkanIsPresent.Content = "Vulkan API damaged!";
                    Text_VulkanIsPresent.FontWeight = FontWeights.Bold;
                    Led_Vulkan.Fill = Brushes.Red;

                    MessageBox.Show("Vulkan API damaged, UI enabler disabled");
                }
            }
            #endregion

            #region DLLs
            List<AllocDLLs> queriedAllocators = db.Query<AllocDLLs>("SELECT * FROM AllocDLLs");

            bool allocatorsPresent = false;

            foreach (AllocDLLs allocator in queriedAllocators)
            {
                if (allocator.Downloaded > 0)
                    allocatorsPresent = true;
            }

            if(allocatorsPresent)
                Btn_Allocs.IsEnabled = false;
            #endregion

            #region Profile files
            List<Profiles> queriedProfiles = db.Query<Profiles>("SELECT * FROM Profiles");

            bool profileFilesPresent = true;

            foreach(Profiles profile in queriedProfiles)
            {
                if (profile.Downloaded < 1)
                    profileFilesPresent = false;
            }

            if (profileFilesPresent)
                Btn_Profile.IsEnabled = false;
            #endregion

            //Close DB connection
            db.Close();
        }

        //Modular function to light up the "presence led" of a pack
        void UpdateModpackInstallStatus(string modPackName)
        {
            switch (modPackName)
            {
                case "Demo":
                    Led_Demo.Visibility = Visibility.Visible;
                    break;
                case "Lite":
                    Led_Lite.Visibility = Visibility.Visible;
                    break;
                case "Standard":
                    Led_Standard.Visibility = Visibility.Visible;
                    break;
                case "Old Times":
                    Led_OldTimes.Visibility = Visibility.Visible;
                    break;
                case "Future":
                    Led_Future.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
        }

        void UpdateModpackUpdateStatus(string modPackName)
        {
            switch (modPackName)
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
        }

        #endregion

        //Easter Egg (don't tell anyone)
        private void Img_Pepe_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("https://youtu.be/dQw4w9WgXcQ");
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
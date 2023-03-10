using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using SQLite;
using Newtonsoft.Json;
using System.Linq;

namespace TRO14_Downloader.CustomClasses
{
    class DBManager
    {
        bool dbExists = true;
        string databasePath;

        public void InizializeDB(string url)
        {
            databasePath = Path.Combine(Environment.CurrentDirectory, "Downloader.db");

            //Check if the database is present in the installation folder and assign the result to a bool variable
            dbExists = File.Exists(databasePath);

            var db = new SQLiteConnection(databasePath);

            //Based on the bool variable change the type of inizialization for the DB
            if (!dbExists)
                CreateTables(db, url);
            else
                InizializeTables(db, url);

            db.Close();
        }

        //Create the tables and execute the inizialization algorithm
        private void CreateTables(SQLiteConnection db, string url)
        {
            db.CreateTable<ModPacks>();
            db.CreateTable<Paths>();
            db.CreateTable<VulkanFiles>();
            db.CreateTable<AllocDLLs>();
            db.CreateTable<DownloadLinks>();
            db.CreateTable<Profiles>();

            InizializeTables(db, url);
        }

        private void InizializeTables(SQLiteConnection db, string url)
        {
            #region DownloadLinks table update
            //Create the web client for the web reading
            WebClient webClient = new WebClient();
            string downloadedString = webClient.DownloadString(url);
            
            //Create 2 Lists of DownloadLinks objects from the downloaded Json and from the database query
            List<DownloadLinks> downloadLinks = JsonConvert.DeserializeObject<List<DownloadLinks>>(downloadedString);
            List<DownloadLinks> queriedLinks = db.Query<DownloadLinks>("SELECT * FROM DownloadLinks");

            //Use Except and Intersect methods to divide which records i need to add and which i need to update -> reference: https://dev.to/kenakamu/c-compare-two-list-items-gec
            var toInsert = downloadLinks.Except(queriedLinks);
            var toUpdate = downloadLinks.Intersect(queriedLinks);
            var toDelete = queriedLinks.Except(downloadLinks); //Added check for old deprecated links

            foreach (var link in toInsert)
                db.Insert(link);

            foreach (var link in toUpdate)
                db.Update(link);

            //Old links are deleted from the DB
            foreach (var link in toDelete)
                db.Delete(link);
            #endregion

            //Update queriedLinks with the updated table
            queriedLinks = db.Query<DownloadLinks>("SELECT * FROM DownloadLinks");

            //Filters out in different Lists the various Types of links -> reference: https://learn.microsoft.com/it-it/dotnet/api/system.collections.generic.list-1.findall?view=net-7.0
            var queriedModPacksLinks = queriedLinks.FindAll(
                delegate(DownloadLinks DL)
                {
                    return DL.Type == "ModPacks";
                }
                );

            var queriedVulkanFilesLinks = queriedLinks.FindAll(
                delegate(DownloadLinks DL)
                {
                    return DL.Type == "VulkanFiles";
                }
                );
            
            var queriedAllocatorsLinks = queriedLinks.FindAll(
                delegate(DownloadLinks DL)
                {
                    return DL.Type == "AllocDLLs";
                }
                );

            var queriedProfilesLinks = queriedLinks.FindAll(
                delegate (DownloadLinks DL)
                {
                    return DL.Type == "Profiles";
                }
                );

            //Generate Lists objects from tables
            List<ModPacks> queriedModPacks = db.Query<ModPacks>("SELECT * FROM ModPacks");
            List<VulkanFiles> queriedVulkanFiles = db.Query<VulkanFiles>("SELECT * FROM VulkanFiles");
            List<AllocDLLs> queriedAllocators = db.Query<AllocDLLs>("SELECT * FROM AllocDLLs");
            List<Profiles> queriedProfiles = db.Query<Profiles>("SELECT * FROM Profiles");

            //All partial comments in the ModPack region
            #region ModPack Initialization
            ModPacks ModPack = new ModPacks(); //Pre-generate ModPack object

            foreach (DownloadLinks modPackLink in queriedModPacksLinks)
            {
                //Assing new attributes to the object each cycle
                ModPack.Name = modPackLink.Name;
                ModPack.VersionURL = modPackLink.VURL;
                ModPack.DownloadURL = modPackLink.URL;

                //Check if already exists inside the DB
                bool modPackExists = queriedModPacks.Exists(
                    delegate(ModPacks MP)
                    { 
                        return MP.Name == ModPack.Name;
                    }
                    );

                if(modPackExists)
                    db.Update(ModPack);
                else
                    db.Insert(ModPack);
            }
            #endregion

            #region Vulkan Initialization
            VulkanFiles VulkanFile = new VulkanFiles();

            foreach (DownloadLinks vulkanLink in queriedVulkanFilesLinks)
            {
                VulkanFile.Name = vulkanLink.Name;
                VulkanFile.DownloadURL = vulkanLink.URL;

                bool vulkanFileExists = queriedVulkanFiles.Exists(
                    delegate(VulkanFiles VF)
                    {
                        return VF.Name == VulkanFile.Name;
                    }
                    );

                if(vulkanFileExists)
                    db.Update(VulkanFile);
                else
                    db.Insert(VulkanFile);
            }
            #endregion

            #region Allocators Initialization
            AllocDLLs Allocator = new AllocDLLs();

            foreach (DownloadLinks allocatorLink in queriedAllocatorsLinks)
            {
                Allocator.Name = allocatorLink.Name;
                Allocator.DownloadURL = allocatorLink.URL;

                bool allocatorExists = queriedAllocators.Exists(
                    delegate(AllocDLLs DLL)
                    {
                        return DLL.Name == Allocator.Name;
                    }
                    );

                if(allocatorExists)
                    db.Update(Allocator);
                else
                    db.Insert(Allocator);
            }
            #endregion

            #region Profile Initialization
            Profiles Profile = new Profiles();

            foreach (DownloadLinks profileLink in queriedProfilesLinks)
            {
                Profile.Name = profileLink.Name;
                Profile.DownloadURL = profileLink.URL;

                bool profileExists = queriedProfiles.Exists(
                    delegate(Profiles P)
                    {
                        return P.Name == Profile.Name;
                    }
                    );

                if (profileExists)
                    db.Update(Profile);
                else
                    db.Insert(Profile);
            }
            #endregion
        }
    }

    public class ModPacks
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string VersionURL { get; set; }
        public string DownloadURL { get; set; }
        public int Downloaded { get; set; }
        public int Selected { get; set; }

    }

    public class VulkanFiles
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string DownloadURL { get; set; }
        public int Downloaded { get; set; }
        public int Active { get; set; }
    }

    public class Paths
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string PathURI { get; set; } 
    }

    public class DownloadLinks
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string VURL { get; set; }
        public string URL { get; set; }
    }

    public class AllocDLLs
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string DownloadURL { get; set; }
        public int Downloaded { get; set; }
    }
    
    public class Profiles
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string DownloadURL { get; set; }
        public int Downloaded { get; set; }
    }
}
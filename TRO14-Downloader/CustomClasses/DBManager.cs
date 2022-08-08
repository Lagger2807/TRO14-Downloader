using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using SQLite;
using Newtonsoft.Json;

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
        }

        //Create the tables and execute the inizialization algorithm
        private void CreateTables(SQLiteConnection db, string url)
        {
            db.CreateTable<ModPacks>();
            db.CreateTable<Paths>();
            db.CreateTable<VulkanFiles>();
            db.CreateTable<AllocDLLs>();
            db.CreateTable<DownloadLinks>();

            InizializeTables(db, url);
        }

        private void InizializeTables(SQLiteConnection db, string url)
        {
            //Create the web client for the web reading
            WebClient webClient = new WebClient();
            string downloadedString = webClient.DownloadString(url);
            
            //Deserialize the readed json inside a List and then convert it to an Array for optimization purposes
            DownloadLinks[] downloadLink = JsonConvert.DeserializeObject<List<DownloadLinks>>(downloadedString).ToArray();

            //Execute for every element found in the json: check if an element is already existing inside the DB and decide if add or update it
            for(int i = 0; i < downloadLink.Length; i++)
            {
                DownloadLinks[] recordQuery = db.Query<DownloadLinks>("SELECT * FROM DownloadLinks WHERE Type = '" + downloadLink[i].Type + "' AND Name = '" + downloadLink[i].Name + "'").ToArray();

                if (recordQuery.Length <= 0)
                    db.Insert(downloadLink[i]);
                else
                    db.Query<DownloadLinks>("UPDATE DownloadLinks SET VURL = '" + downloadLink[i].VURL + "'AND URL = '" + downloadLink[i].URL + "' WHERE Type = '" + downloadLink[i].Type + "' AND Name = '" + downloadLink[i].Name + "'"); 
            }

            //Stores all DownloadLinks table elements into an array
            DownloadLinks[] recordsInTable = db.Query<DownloadLinks>("SELECT * FROM DownloadLinks").ToArray();

            //Counts the elements and check which type are them
            for(int i = 0; i < recordsInTable.Length; i++)
            {
                switch(recordsInTable[i].Type)
                {
                    //Every case add or update elements to the DB based on if it already exist or not
                    case "ModPacks":
                        ModPacks modpack = new ModPacks { Name = recordsInTable[i].Name, VersionURL = recordsInTable[i].VURL, DownloadURL = recordsInTable[i].URL };
                        ModPacks[] modpackQuery = db.Query<ModPacks>("SELECT * FROM ModPacks WHERE Name = '" + modpack.Name + "'").ToArray();

                        if (modpackQuery.Length <= 0)
                            db.Insert(modpack);
                        else
                            db.Query<ModPacks>("UPDATE ModPacks SET VersionUrl = '" + modpack.VersionURL + "' AND DownloadURL = '" + modpack.DownloadURL + "'");
                        break;

                    case "VulkanFiles":
                        VulkanFiles vulkanFile = new VulkanFiles { Name = recordsInTable[i].Name, DownloadURL = recordsInTable[i].URL };
                        VulkanFiles[] vulkanFilesQuery = db.Query<VulkanFiles>("SELECT * FROM VulkanFiles WHERE Name = '" + vulkanFile.Name + "'").ToArray();

                        if (vulkanFilesQuery.Length <= 0)
                            db.Insert(vulkanFile);
                        else
                            db.Query<VulkanFiles>("UPDATE VulkanFiles SET DownloadURL = '" + vulkanFile.DownloadURL + "'");
                        break;

                    case "AllocDLLs":
                        AllocDLLs allocator = new AllocDLLs { Name = recordsInTable[i].Name, DownloadURL = recordsInTable[i].URL };
                        AllocDLLs[] allocatorQuery = db.Query<AllocDLLs>("SELECT * FROM AllocDLLs WHERE Name = '" + allocator.Name + "'").ToArray();

                        if (allocatorQuery.Length <= 0)
                            db.Insert(allocator);
                        else
                            db.Query<AllocDLLs>("UPDATE AllocDLLs SET DownloadURL = '" + allocator.DownloadURL + "'");
                        break;

                    default:
                        break;
                }
            }
        }
    }

    public class ModPacks
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public float Version { get; set; }
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
}

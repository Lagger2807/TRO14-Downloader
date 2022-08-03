using System;
using System.Collections.Generic;
using System.IO;
using SQLite;

namespace TRO14_Downloader.CustomClasses
{
    class DBManager
    {
        bool dbExists = true;
        string databasePath;

        public void InizializeDB()
        {
            databasePath = System.IO.Path.Combine(Environment.CurrentDirectory, "Downloader.db");

            dbExists = File.Exists(databasePath);

            var db = new SQLiteConnection(databasePath);

            if (dbExists)
                CreateTables(db);
        }

        private void CreateTables(SQLiteConnection db)
        {
            db.CreateTable<ModPacks>();
            db.CreateTable<Paths>();
            db.CreateTable<VulkanFiles>();
            db.CreateTable<AllocDLLs>();
        }

        public ModPacks[] ExecuteQueryOnModPacks(string query)
        {
            var db = new SQLiteConnection(databasePath);
            ModPacks[] queryOutput = db.Query<ModPacks>(query).ToArray();

            return queryOutput;
        }

        public void AddPack(string name, float version)
        {
            var db = new SQLiteConnection(databasePath);

            ModPacks newPack = new ModPacks { Name = name, Version = version };
            db.Insert(newPack);
        }

        public void UpdatePackVersion(string name, float newVersion)
        {
            var db = new SQLiteConnection(databasePath);

            db.Query<ModPacks>("UPDATE ModPacks SET Version = " + newVersion + " WHERE Name = " + name);
        }

        public void DeletePack(string name)
        {
            var db = new SQLiteConnection(databasePath);

            db.Query<ModPacks>("DELETE FROM ModPacks WHERE Name = " + name);
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

        public string FileName { get; set; }

        public int Active { get; set; } 
    }

    public class Paths
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string PathURI { get; set; } 
    }

    public class AllocDLLs
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string FileName { get; set; }
        public int Selected { get; set; }
    }
}

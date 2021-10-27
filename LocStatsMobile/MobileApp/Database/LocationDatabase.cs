
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SQLite;

namespace MobileApp.Database
{
    class LocationDatabase
    {
        private string dbName = "Location.db3";
        private Environment.SpecialFolder specialFolder = Environment.SpecialFolder.ApplicationData;
        private string folderPath;
        private string pathDB;
        private SQLiteConnection db;

        public LocationDatabase()
        {
            folderPath = Environment.GetFolderPath(specialFolder);
            pathDB = Path.Combine(folderPath, dbName);
            db = new SQLiteConnection(pathDB);
            db.CreateTable<LocationModel>();

            db.TableChanged += ((sender, args) =>
            {
                Console.WriteLine("TableChanged");
            });
        }

        ~LocationDatabase()
        {
            db.DropTable<LocationModel>();
            db.Close();
        }

        public void AddLocation(LocationModel lm)
        {
            db.Insert(lm);
        }

        public IEnumerable<LocationModel> GetAllLocations()
        {
            return db.Table<LocationModel>();
        }

        public void DeleteAllLocations()
        {
            db.DeleteAll<LocationModel>();
        }
    }
}
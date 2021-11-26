
using System;
using System.Collections.Generic;
using System.IO;
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
            db.CreateTable<PolyLinesModel>();

            db.TableChanged += ((sender, args) =>
            {
                Console.WriteLine("TableChanged");
            });
        }

        ~LocationDatabase()
        {
            db.DropTable<LocationModel>();
            db.DropTable<PolyLinesModel>();
            db.Close();
        }

        public void AddLocation(LocationModel lm)
        {
            db.Insert(lm);
        }

        public void AddPolyLine(PolyLinesModel plm)
        {
            db.Insert(plm);
        }

        public void AddPolyLines(PolyLinesModel[] plms)
        {
            db.InsertAll(plms);
        }

        public IEnumerable<LocationModel> GetAllLocations()
        {
            return db.Table<LocationModel>();
        }

        public IEnumerable<PolyLinesModel> GetAllPolyLines()
        {
            return db.Query<PolyLinesModel>("SELECT * FROM PolyLines ORDER BY Timestamp ASC");
        }

        public void DeleteAllLocations()
        {
            db.DeleteAll<LocationModel>();
        }

        public void DeleteAllPolyLines()
        {
            db.DeleteAll<PolyLinesModel>();
        }

        public IEnumerable<LocationModel> GetNLastLocations(int n)
        {
            return db.Query<LocationModel>($"SELECT * FROM Location ORDER BY Timestamp DESC LIMIT {n}");
        }
    }
}
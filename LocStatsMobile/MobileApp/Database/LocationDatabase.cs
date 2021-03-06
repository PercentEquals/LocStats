using System;
using System.Collections.Generic;
using System.IO;
using Android.Util;
using SQLite;

namespace MobileApp.Database
{
    class LocationDatabase
    {
        private string dbName = "Location.db3";
        private Environment.SpecialFolder specialFolder = Environment.SpecialFolder.ApplicationData;
        private SQLiteConnection db;

        public LocationDatabase()
        {
            var folderPath = Environment.GetFolderPath(specialFolder);
            var pathDb = Path.Combine(folderPath, dbName);
            db = new SQLiteConnection(pathDb);
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

        public void AddPolyLines(IEnumerable<PolyLinesModel> plms)
        {
            int res = db.InsertAll(plms);
            Log.Info("PolyLines DB", "PolyLines added" + res);
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
            int res = db.DeleteAll<PolyLinesModel>();
            Log.Info("PolyLines DB", "PolyLines deleted" + res);
        }

        public IEnumerable<LocationModel> GetNLastLocations(int n)
        {
            return db.Query<LocationModel>($"SELECT * FROM Location ORDER BY Timestamp DESC LIMIT {n}");
        }
    }
}
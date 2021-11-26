using System;
using System.Collections.Generic;
using System.Linq;
using Android.Util;
using MobileApp.Fragments;
using MobileApp.Managers;

namespace MobileApp.Database
{
    public class LocationRepo
    {
        private static LocationRepo instance;
        private static readonly object padlock = new object();
        private LocationDatabase db;
        protected static LocationRepo me;
        private FragmentLocalization fl;

        private long lastTimestamp;

        // Just multiply minutesToSave with clearBufferSize and you got time when to send data to the cloud
        private double milisecToSave = 1 * 60 * 1000;
        //private double milisecToSave = 5 * 60 * 10; //only for testing
        private int bufferSize;
        private int clearBufferSize = 4;

        static LocationRepo()
        {
            me = new LocationRepo();
        }

        public LocationRepo()
        {
            db = new LocationDatabase();
        }

        public static LocationRepo Instance
        {
            get
            {
                lock (padlock)
                {
                    return instance ??= new LocationRepo();
                }
            }
        }

        public void SetFragLocal(FragmentLocalization fl)
        {
            this.fl = fl;
        }

        public void AddLocation(long ts, double lat, double lgn)
        {
            Log.Info("Location Repo", "Inserting New Location");
            lastTimestamp = ts;
            bufferSize++;
            LocationModel lm = new LocationModel
            {
                Timestamp = ts,
                Latitude = lat,
                Longitude = lgn
            };
            me.db.AddLocation(lm);
            fl.AddPolyLinePoint((PolyLinesModel)lm);
            Log.Info("Location Repo", "Db size: " + me.GetLocationsLength());

            AddPolyLine(ts, lat, lgn);

            if (bufferSize == clearBufferSize)
            {
                Log.Info("Location Repo", "OnReachedSize event call");
                OnReachedSize();
                bufferSize = 0;
            }
        }

        public void AddPolyLine(long ts, double lat, double lgn)
        {
            Log.Info("Location Repo", "Inserting New PolyLine");
            PolyLinesModel plm = new PolyLinesModel
            {
                Timestamp = ts,
                Latitude = lat,
                Longitude = lgn
            };
            me.db.AddPolyLine(plm);
        }

        public void AddPolyLines(IEnumerable<PolyLinesModel> plms)
        {
            me.db.AddPolyLines(plms);
        }

        public IEnumerable<LocationModel> GetAllLocations()
        {
            return me.db.GetAllLocations();
        }

        public IEnumerable<PolyLinesModel> GetAllPolyLines()
        {
            return me.db.GetAllPolyLines();
        }

        public void DeleteAllLocations()
        {
            me.db.DeleteAllLocations();
        }

        public void DeleteAllPolyLines()
        {
            me.db.DeleteAllPolyLines();
        }

        public int GetLocationsLength()
        {
            return me.db.GetAllLocations().ToList().Count;
        }

        public bool HasTimePassed(long ts)
        {
            var diffMinutes = (ts - lastTimestamp);
            return diffMinutes >= milisecToSave;
        }

        public IEnumerable<LocationModel> GetBuffer()
        {
            return me.db.GetNLastLocations(clearBufferSize);
        }

        public async void OnReachedSize()
        {
            // Ready buffer to the cloud
            IEnumerable<LocationModel> bufferLocations = GetBuffer();

            // milliseconds to seconds
            foreach (LocationModel loc in bufferLocations)
            {
                loc.Timestamp /= 1000;
            }

            var result = await ConnectionManager.SendGPSData(bufferLocations);

            Log.Info("Server response", result.errorMessage);

            foreach (LocationModel loc in bufferLocations)
            {
                Console.WriteLine(new DateTime(10000 * loc.Timestamp + DateTime.UnixEpoch.Ticks, DateTimeKind.Local).ToString("r"));
                Console.WriteLine(loc.Latitude);
                Console.WriteLine(loc.Longitude);
            }
            Log.Info("Location Repo", $"Reached buffer size of {bufferSize}. Deleting");
            DeleteAllLocations();
        }
    }
}
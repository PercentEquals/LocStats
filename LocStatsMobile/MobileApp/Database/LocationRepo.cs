
using System;
using System.Collections.Generic;
using System.Linq;
using Android.Util;
using Java.Lang;
using MobileApp.Managers;

namespace MobileApp.Database
{
    public class LocationRepo
    {
        private LocationDatabase db = null;
        protected static LocationRepo me;

        private long lastTimestamp = 0;

        // Just multiply minutesToSave with clearBufferSize and you got time when to send data to the cloud
        private double milisecToSave = 5 * 60 * 1000;
        //private double milisecToSave = 5 * 60 * 10; //only for testing
        private int bufferSize = 0;
        private int clearBufferSize = 4;

        static LocationRepo()
        {
            me = new LocationRepo();
        }

        public LocationRepo()
        {
            db = new LocationDatabase();
        }

        public void AddLocation(long ts, double lat, double lon)
        {
            Log.Info("Location Repo", "Inserting New Location");
            lastTimestamp = ts;
            bufferSize++;
            LocationModel lm = new LocationModel
            {
                Timestamp = ts,
                Latitude = lat,
                Longitude = lon
            };
            me.db.AddLocation(lm);

            if (bufferSize == clearBufferSize)
            {
                OnReachedSize();
                bufferSize = 0;
            }
        }

        public IEnumerable<LocationModel> GetAllLocations()
        {
            return me.db.GetAllLocations();
        }

        public void DeleteAllLocations()
        {
            me.db.DeleteAllLocations();
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
            return me.db.GetNLastRows(clearBufferSize);
        }

        public async void OnReachedSize()
        {
            //Ready buffer to the cloud
            IEnumerable<LocationModel> bufferLocations = GetBuffer();

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
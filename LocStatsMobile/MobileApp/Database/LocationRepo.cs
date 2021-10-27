
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MobileApp.Database
{
    public class LocationRepo
    {
        private LocationDatabase db = null;
        protected static LocationRepo me;

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
            LocationModel lm = new LocationModel
            {
                Timestamp = ts,
                Latitude = lat,
                Longitude = lon
            };
            me.db.AddLocation(lm);
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
    }
}
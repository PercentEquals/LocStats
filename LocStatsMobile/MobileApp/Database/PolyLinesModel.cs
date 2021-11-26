using SQLite;
using System;

namespace MobileApp.Database
{
    [Table("PolyLines")]
    public class PolyLinesModel
    {
        public long Timestamp { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        
        public static explicit operator PolyLinesModel(LocationModel v)
        {
            return new PolyLinesModel
            {
                Timestamp = v.Timestamp,
                Latitude = v.Latitude,
                Longitude = v.Longitude
            };
        }
    }
}
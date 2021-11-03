using SQLite;

namespace MobileApp.Database
{
    [Table("Location")]
    public class LocationModel
    {
        public long Timestamp { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
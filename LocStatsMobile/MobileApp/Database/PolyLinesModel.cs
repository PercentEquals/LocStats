using SQLite;

namespace MobileApp.Database
{
    [Table("PolyLines")]
    public class PolyLinesModel
    {
        public long Timestamp { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
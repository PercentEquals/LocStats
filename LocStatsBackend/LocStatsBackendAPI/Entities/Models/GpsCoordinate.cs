using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocStatsBackendAPI.Entities.Models
{
    [Table("GPSCoordinates")]
    public class GpsCoordinate
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public long Timestamp { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
    }
}
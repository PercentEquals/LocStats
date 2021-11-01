namespace LocStatsBackendAPI.Entities.Responses
{
    public class GpsResponse
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public long Timestamp { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
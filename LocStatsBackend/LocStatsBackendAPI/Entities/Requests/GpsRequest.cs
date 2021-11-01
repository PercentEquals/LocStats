using System;
using System.Collections.Generic;
using LocStatsBackendAPI.Entities.Models;

namespace LocStatsBackendAPI.Entities.Requests
{
    public class GpsRequest
    {
        public long Timestamp { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
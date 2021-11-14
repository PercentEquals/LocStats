using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LocStatsBackendAPI.Entities.Models;
using LocStatsBackendAPI.Entities.Requests;

namespace LocStatsBackendAPI.Services.IServices
{
    public interface IGpsService
    {
        public Task<GpsCoordinate> AddCoordinates(GpsRequest gpsRequest, string userId);
        public Task<List<GpsCoordinate>> GetCoordinatesFrom(DateTime date, string userId);
        public Task<List<GpsCoordinate>> GetCoordinatesFrom(DateTime from, DateTime to, string userId);
        public Task<bool> CheckIfExists(GpsRequest gpsRequest, string userId);
    }
}
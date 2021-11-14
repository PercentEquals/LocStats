using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LocStatsBackendAPI.Data;
using LocStatsBackendAPI.Entities.Helpers;
using LocStatsBackendAPI.Entities.Models;
using LocStatsBackendAPI.Entities.Repositories.IRepositories;
using LocStatsBackendAPI.Entities.Requests;
using LocStatsBackendAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace LocStatsBackendAPI.Services
{
    public class GpsService : IGpsService
    {
        protected readonly IGpsRepository GpsRepository;
        protected readonly AppDbContext Context;

        public GpsService(IGpsRepository gpsRepository, AppDbContext context)
        {
            GpsRepository = gpsRepository;
            Context = context;
        }

        public async Task<GpsCoordinate> AddCoordinates(GpsRequest gpsRequest, string userId)
        {
            var gpsCoordinate = new GpsCoordinate()
            {
                UserId = userId,
                Latitude = gpsRequest.Latitude,
                Longitude = gpsRequest.Longitude,
                Timestamp = gpsRequest.Timestamp
            };
        
            await Context.GpsCoordinates.AddAsync(gpsCoordinate);
            await Context.SaveChangesAsync();
            
            return gpsCoordinate;
        }

        public async Task<List<GpsCoordinate>> GetCoordinatesFrom(DateTime date, string userId)
        {
            return await GetCoordinatesFrom(date, date.AddDays(1), userId);
        }

        public async Task<List<GpsCoordinate>> GetCoordinatesFrom(DateTime from, DateTime to, string userId)
        {
            var start = TimeHelper.DateTimeToUnixTimeStamp(from.Date);
            var end = TimeHelper.DateTimeToUnixTimeStamp(to.Date.AddDays(1));

            return await Context.GpsCoordinates.Where(x => x.UserId == userId && x.Timestamp >= start && x.Timestamp <= end)
                .OrderBy(x => x.Timestamp)
                .ToListAsync();
        }

        public async Task<bool> CheckIfExists(GpsRequest gpsRequest, string userId)
        {
            var found = await Context.GpsCoordinates.FirstOrDefaultAsync(x => x.Latitude - gpsRequest.Latitude <= 0.01
                                                                        && x.Longitude - gpsRequest.Longitude <= 0.01 
                                                                        && x.Timestamp == gpsRequest.Timestamp
                                                                        && x.UserId == userId);

            return found != null;
        }
    }
}
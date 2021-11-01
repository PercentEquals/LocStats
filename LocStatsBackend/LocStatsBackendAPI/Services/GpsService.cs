using System;
using System.Linq;
using System.Threading.Tasks;
using LocStatsBackendAPI.Data;
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
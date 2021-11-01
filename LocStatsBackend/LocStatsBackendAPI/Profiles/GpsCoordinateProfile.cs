using AutoMapper;
using LocStatsBackendAPI.Entities.Models;
using LocStatsBackendAPI.Entities.Responses;

namespace LocStatsBackendAPI.Profiles
{
    public class GpsCoordinateProfile : Profile
    {
        public GpsCoordinateProfile()
        {
            CreateMap<GpsCoordinate, GpsResponse>();
        }
    }
}
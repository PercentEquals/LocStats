using System.Threading.Tasks;
using LocStatsBackendAPI.Entities.Models;
using Microsoft.AspNetCore.Mvc;

namespace LocStatsBackendAPI.Entities.Repositories.IRepositories
{
    public interface IGpsRepository : IRepository<GpsCoordinate>
    {
        public Task<GpsCoordinate> GetById(int id);
        public Task<GpsCoordinate> GetByUserId(string id);
    }
}
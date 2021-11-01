using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LocStatsBackendAPI.Data;
using LocStatsBackendAPI.Entities.Models;
using LocStatsBackendAPI.Entities.Repositories.IRepositories;
using LocStatsBackendAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocStatsBackendAPI.Entities.Repositories
{
    public class GpsRepository : IGpsRepository
    {
        protected readonly AppDbContext Context;

        public Task<bool> Update(GpsCoordinate entity)
        {
            throw new System.NotSupportedException();
        }

        public Task<bool> Add(GpsCoordinate gpsCoordinate)
        {
            throw new System.NotSupportedException();
        }

        public Task<GpsCoordinate> GetById(Guid id)
        {
            throw new System.NotSupportedException();
        }

        public Task<GpsCoordinate> GetByUserId(string id)
        {
            throw new System.NotSupportedException();
        }

        public IQueryable<GpsCoordinate> GetAll()
        {
            return Context.GpsCoordinates;
        }

        public IQueryable<GpsCoordinate> Find(Expression<Func<GpsCoordinate, bool>> @where)
        {
            return Context.GpsCoordinates.AsQueryable().Where(where);
        }

        public Task<GpsCoordinate> GetById(int id)
        {
            return Context.GpsCoordinates.FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task<bool> Delete(Guid id)
        {
            throw new NotSupportedException();
        }
    }
}
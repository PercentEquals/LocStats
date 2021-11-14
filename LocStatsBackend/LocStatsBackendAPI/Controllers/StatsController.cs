using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using LocStatsBackendAPI.Data;
using LocStatsBackendAPI.Entities.Helpers;
using LocStatsBackendAPI.Entities.Models;
using LocStatsBackendAPI.Entities.Requests;
using LocStatsBackendAPI.Entities.Responses;
using LocStatsBackendAPI.Services;
using LocStatsBackendAPI.Services.IServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocStatsBackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class StatsController : ControllerBase
    {
        private readonly IGpsService _gpsService;
        private readonly IMapper _mapper;

        public StatsController(IGpsService gpsService, IMapper mapper)
        {
            _gpsService = gpsService;
            _mapper = mapper;
        }

        /// <summary>
        /// Saves GPS coordinates on the cloud
        /// </summary>
        /// <param name="from">Date from (eq. 2021-11-14)</param>
        /// <param name="to">Date to (eq. 2021-11-14)</param>
        /// <returns>Coordinates result</returns>
        /// <response code="200">Returns sent GPS coordinates</response>
        /// <response code="400">Bad request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="500">Something went wrong</response>
        [HttpGet]
        [Route("Time/{from:datetime}/{to:datetime}")]
        public async Task<IActionResult> GetTimeStats(DateTime from, DateTime to)
        {
            if (!ModelState.IsValid) 
                return new BadRequestObjectResult("Invalid payload");

            var userId = User.Claims.First(i => i.Type == "Id").Value;
            var coords = await _gpsService.GetCoordinatesFrom(from, to, userId);
            var time = StatsHelper.CalcUsageTime(coords);

            var response = new List<StatsResponse>();

            while (from.Date != to.Date)
            {
                response.Add(new StatsResponse
                {
                    Date = from.Date.ToShortDateString(),
                    Value = time.ContainsKey(from.Date) ? time[from.Date] : 0
                });

                from = from.AddDays(1);
            }

            return Ok(response);
        }
    }
}
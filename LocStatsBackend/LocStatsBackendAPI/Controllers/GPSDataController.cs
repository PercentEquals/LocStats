using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using LocStatsBackendAPI.Data;
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
    public class GPSDataController : ControllerBase
    {
        private readonly IGpsService _gpsService;
        private readonly IMapper _mapper;

        public GPSDataController(IGpsService gpsService, IMapper mapper)
        {
            _gpsService = gpsService;
            _mapper = mapper;
        }

        /// <summary>
        /// Saves GPS coordinates on the cloud
        /// </summary>
        /// <param name="gpsRequest">Timestamp, latitude and longitude</param>
        /// <returns>Coordinates result</returns>
        /// <response code="200">Returns sent GPS coordinates</response>
        /// <response code="400">Bad request</response>
        /// <response code="401">Unauthorised</response>
        /// <response code="500">Something went wrong</response>
        [HttpPost]
        [Route("Send")]
        public async Task<IActionResult> SendCoordinates([FromBody] GpsRequest gpsRequest)
        {
            if (!ModelState.IsValid) return new BadRequestObjectResult("Invalid payload");

            var userId = User.Claims.First(i => i.Type == "Id").Value;
            
            var found = await _gpsService.CheckIfExists(gpsRequest, userId);
            
            if (found) return new BadRequestObjectResult("Received duplicated data");

            var gpsCoordinate = await _gpsService.AddCoordinates(gpsRequest, userId);
            
            return Ok(_mapper.Map<GpsResponse>(gpsCoordinate));
        }
        
        /// <summary>
        /// Saves multiple GPS coordinates on the cloud
        /// </summary>
        /// <param name="gpsRequests">List of Timestamps, latitudes and longitudes</param>
        /// <returns>Coordinates result</returns>
        /// <response code="200">Returns sent GPS coordinates</response>
        /// <response code="400">Bad request</response>
        /// <response code="401">Unauthorised</response>
        /// <response code="500">Something went wrong</response>
        [HttpPost]
        [Route("SendMultiple")]
        public async Task<IActionResult> SendMultipleCoordinates([FromBody] List<GpsRequest> gpsRequests)
        {
            if (!ModelState.IsValid) return new BadRequestObjectResult("Invalid payload");
            
            foreach (var gpsCoordinate in gpsRequests)
            {
                await SendCoordinates(gpsCoordinate);
            }

            return Ok();
        }

        /// <summary>
        /// Gets GPS coordinates from specific day
        /// </summary>
        /// <param name="date">Date (eq. 2021-11-14)</param>
        /// <returns>List of GPS coordinates for that day (for user requesting this)</returns>
        /// <response code="200">Returns GPS coordinates from specific day</response>
        /// <response code="400">Bad request</response>
        /// <response code="401">Unauthorised</response>
        /// <response code="500">Something went wrong</response>
        [HttpGet]
        public async Task<IActionResult> GetCoordinatesFromSpecificDay([FromQuery] DateTime date)
        {
            var userId = User.Claims.First(i => i.Type == "Id").Value;
            var list = await _gpsService.GetCoordinatesFrom(date, userId);
            return Ok(_mapper.Map<GpsCoordinate[], List<GpsResponse>>(list.ToArray()));
        }
    }
}
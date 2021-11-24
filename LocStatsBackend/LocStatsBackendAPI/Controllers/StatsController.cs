using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using ClusteringKMeansLib;
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
        /// Returns time usage stats (in seconds) for specified time range - basically how long tracking was enabled
        /// </summary>
        /// <param name="from">Date from (eq. 2021-11-14)</param>
        /// <param name="to">Date to (eq. 2021-11-14)</param>
        /// <returns>Coordinates result</returns>
        /// <response code="200">Returns time usage stats (in seconds)</response>
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

            while (from <= to)
            {
                response.Add(new StatsResponse
                {
                    Date = from.Date.ToString("yyyy-MM-dd"),
                    Value = time.ContainsKey(from.Date) ? time[from.Date] : 0.0
                });

                from = from.AddDays(1);
            }

            return Ok(response);
        }

        /// <summary>
        /// Returns distance stats (in meters) for specified time range - basically how much meters were tracked
        /// </summary>
        /// <param name="from">Date from (eq. 2021-11-14)</param>
        /// <param name="to">Date to (eq. 2021-11-14)</param>
        /// <returns>Coordinates result</returns>
        /// <response code="200">Returns distance stats (in meters)</response>
        /// <response code="400">Bad request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="500">Something went wrong</response>
        [HttpGet]
        [Route("Distance/{from:datetime}/{to:datetime}")]
        public async Task<IActionResult> GetDistanceStats(DateTime from, DateTime to)
        {
            if (!ModelState.IsValid)
                return new BadRequestObjectResult("Invalid payload");

            var userId = User.Claims.First(i => i.Type == "Id").Value;
            var coords = await _gpsService.GetCoordinatesFrom(from, to, userId);
            var distance = StatsHelper.CalcTraveledDistance(coords);

            var response = new List<StatsResponse>();

            while (from <= to)
            {
                response.Add(new StatsResponse
                {
                    Date = from.Date.ToString("yyyy-MM-dd"),
                    Value = distance.ContainsKey(from.Date) ? distance[from.Date] : 0.0
                });

                from = from.AddDays(1);
            }

            return Ok(response);
        }

        /// <summary>
        /// Returns most frequent location (lat and long) for specified time range
        /// </summary>
        /// <param name="from">Date from (eq. 2021-11-14)</param>
        /// <param name="to">Date to (eq. 2021-11-14)</param>
        /// <param name="clusters">How many clusters to predict</param>
        /// <returns>Coordinates result</returns>
        /// <response code="200">Returns most frequent location</response>
        /// <response code="400">Bad request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="500">Something went wrong</response>
        [HttpGet]
        [Route("MostFrequentLocation/{from:datetime}/{to:datetime}/{clusters:int}")]
        public async Task<IActionResult> GetMostFrequentLocation(DateTime from, DateTime to, int clusters)
        {
            if (!ModelState.IsValid)
                return new BadRequestObjectResult("Invalid payload");
            if (clusters <= 0 || clusters >= 100)
                return new BadRequestObjectResult("Clusters size must be a non negative integer lower than 100");

            var userId = User.Claims.First(i => i.Type == "Id").Value;
            var coords = await _gpsService.GetCoordinatesFrom(from, to, userId);

            if (coords.Count <= 0)
                return new BadRequestObjectResult("Not enough coordinate data");
            if (clusters > coords.Count)
                return new BadRequestObjectResult("Clusters size was too big for the data from specified range");

            double[][] data = new double[coords.Count][];
            var i = -1;

            foreach (var coord in coords)
            {
                data[++i] = new double[] { coord.Latitude, coord.Longitude };
            }

            var k = clusters;
            var initMethod = "plusplus";
            var maxIter = 100;
            var seed = 0;
            var trials = 10;

            KMeans km = new KMeans(k, data, initMethod, maxIter, seed);
            km.Cluster(trials);

            var mostFrequentCluster = km.counts.Max();
            var mostFrequentClusterIndex = km.counts.ToList().IndexOf(mostFrequentCluster);

            var meanLocation = km.means[mostFrequentClusterIndex];

            var url = new System.Text.StringBuilder();
            url.Append($"https://commons.wikimedia.org/w/api.php?");
            url.Append($"format=json&action=query&generator=geosearch&ggsprimary=all&ggsnamespace=6&ggsradius=500");
            url.Append($"&ggscoord={ meanLocation[0] }|{ meanLocation[1] }");
            url.Append($"&ggslimit=1&prop=imageinfo&iilimit=1&iiprop=url&iiurlwidth=200&iiurlheight=200");

            var imageUrl = "";

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url.ToString());

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    var pattern = new Regex("\"url\":(.*?)\",");
                    Match m = pattern.Match(content);

                    if (m.Success)
                    {
                        imageUrl = m.Value.Substring(7);
                        imageUrl = imageUrl.Substring(0, imageUrl.Length - 2);
                    }
                }
            }

            return Ok(new GpsStatsResponse
            {
                ImageUrl = imageUrl,
                Latitude = meanLocation[0],
                Longitude = meanLocation[1],
            });
        }
    }
}
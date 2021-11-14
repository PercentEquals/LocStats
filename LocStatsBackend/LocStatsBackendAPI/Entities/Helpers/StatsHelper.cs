using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LocStatsBackendAPI.Entities.Models;

namespace LocStatsBackendAPI.Entities.Helpers
{
    public static class StatsHelper
    {
        public static Dictionary<DateTime, List<GpsCoordinate>> CategorizeDataByDate(List<GpsCoordinate> coordinates)
        {
            Dictionary<DateTime, List<GpsCoordinate>> dictionary = new();

            foreach (var coord in coordinates)
            {
                var date = TimeHelper.UnixTimeStampToDateTime(coord.Timestamp);

                if (dictionary.ContainsKey(date.Date))
                {
                    dictionary[date.Date].Add(coord);
                }
                else
                {
                    dictionary.Add(date.Date, new List<GpsCoordinate> {coord});
                }
            }

            return dictionary;
        }

        public static Dictionary<DateTime, double> CalcUsageTime(List<GpsCoordinate> coordinates)
        {
            var dictionary = CategorizeDataByDate(coordinates);
            Dictionary<DateTime, double> stats = new();

            foreach (var item in dictionary)
            {
                stats.Add(item.Key, item.Value.Last().Timestamp - item.Value.First().Timestamp);
            }

            return stats;
        }
    }
}

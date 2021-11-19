using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LocStatsBackendAPI.Entities.Responses
{
    public class StatsResponse
    {
        public string Date { get; set; }
        public double Value { get; set; }
    }
}

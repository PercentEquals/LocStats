using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LocStatsBackendAPI.Entities.Helpers
{
    public static class TimeHelper
    {
        public static bool IsSameDate(DateTime first, DateTime second)
        {
            return first.Date == second.Date;
        }

        public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            var dateTimeVal = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTimeVal = dateTimeVal.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTimeVal;
        }

        public static long DateTimeToUnixTimeStamp(DateTime date)
        {
            var dateTimeVal = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var unixDateTime = (date.ToUniversalTime() - dateTimeVal).TotalSeconds;
            return Convert.ToInt64(unixDateTime);
        }
    }
}

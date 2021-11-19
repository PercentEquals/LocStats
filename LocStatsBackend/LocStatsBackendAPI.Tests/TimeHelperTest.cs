using LocStatsBackendAPI.Entities.Helpers;
using System;
using Xunit;

namespace LocStatsBackendAPI.Tests
{
    public class TimeHelperTest
    {
        [Fact]
        public void ConversionTest()
        {
            var date = DateTime.Now;
            var date_now_unix = DateTimeOffset.Now.ToUnixTimeSeconds();
            var date_unix = TimeHelper.DateTimeToUnixTimeStamp(date);

            Assert.Equal(date.Date, TimeHelper.UnixTimeStampToDateTime(date_unix).Date);
            Assert.Equal(date.Hour, TimeHelper.UnixTimeStampToDateTime(date_unix).Hour);
            Assert.Equal(date.Minute, TimeHelper.UnixTimeStampToDateTime(date_unix).Minute);

            Assert.True(Math.Abs(date_now_unix - date_unix) <= 2);
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SunriseCalculator;
using SunriseCalculator.Enums;
using System;

namespace SunriseCalculatorTests
{
    [TestClass]
    public class UnitTests
    {
        private DateTime ReduceToMinutes(DateTime value) => new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, 0);

        private void AssertAreWithinNMinutes(DateTime expected, DateTime actual, int minutes = 1)
        {
            DateTime exp = ReduceToMinutes(expected);
            DateTime act = ReduceToMinutes(actual);
            for (int i = 1; i < minutes; i++)
            {
                try
                {
                    Assert.AreEqual(exp, act.AddMinutes(i));
                    return;
                }
                catch (Exception)
                {
                }
            }

            for (int i = 1; i < minutes; i++)
            {
                try
                {
                    Assert.AreEqual(exp, act.AddMinutes(-i));
                    return;
                }
                catch (Exception)
                {
                }
            }

            Assert.AreEqual(exp, act);
        }

        [TestMethod]
        public void SimpleTestNYC()
        {
            // A simple spot test for one known location and time.
            DateTime testDate = new DateTime(2021, 7, 8);
            TimeSpan NYCTimezoneOffset = TimeSpan.FromHours(4);
            const double NYCLat = 40.7128;
            const double NYCLong = -74.0060;
            DateTime actualSunrise = testDate.AddHours(5).AddMinutes(32);
            DateTime actualSunset = testDate.AddHours(20).AddMinutes(29);

            SunriseCalc nyc = new SunriseCalc(NYCLat, NYCLong, testDate);
            var result = nyc.GetRiseAndSet(out DateTime sunrise, out DateTime sunset);

            // The sun always rises on New York City.
            Assert.AreEqual(DiurnalResult.NormalDay, result);

            // The sunrise and sunset should be within a minute of the expected value.
            AssertAreWithinNMinutes(actualSunrise + NYCTimezoneOffset, sunrise);
            AssertAreWithinNMinutes(actualSunset + NYCTimezoneOffset, sunset);

            var riseResult = nyc.GetSunrise(out DateTime sunrise2);
            Assert.AreEqual(DiurnalResult.NormalDay, riseResult);

            // We expect both methods to return the same value.
            Assert.AreEqual(sunrise, sunrise2);

            var setResult = nyc.GetSunset(out DateTime sunset2);
            Assert.AreEqual(DiurnalResult.NormalDay, setResult);
            Assert.AreEqual(sunset, sunset2);
        }

//        // It turns out that SunDate will often produce nonsense values, so *shrug* whatever.
//#if TESTSUNDATE
//        [TestMethod]
//        public void SunDateCompare()
//        {
//            // Compare our results with the output from SunDate.cs. Note that SunDate.cs is not
//            // included in the repository, as it has an incompatible license.
//            // Since SunDate only works for today, we can only compare randomly selected locations,
//            // not randomly selected days.

//            // How many random locations to test against
//            const int testCount = 1000;

//            // This is the +/- variation we will tolerate between the output of SunDate.cs and our own.
//            const int minuteAccuracy = 3;
//            Random rng = new Random();

//            // SunDate assumes that it is only used for the current system time zone and applies
//            // the current system time zone offset to the results; this is obviously a bad idea,
//            // which we have to adjust for.
//            var tzOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);

//            for (int i = 0; i < testCount; i++)
//            {
//                // Generate a random lat & lon to compare
//                double lat = rng.NextDouble() * 2 * SunriseCalc.MaxLatitude - SunriseCalc.MaxLatitude;
//                double lon = rng.NextDouble() * 2 * SunriseCalc.MaxLongitude - SunriseCalc.MaxLongitude;

//                int[] sundateResults = SunDate.CalculateSunriseSunset(lat, lon);
//                DateTime sundateRise;
//                //DateTime sundateSet;
//                try
//                {
//                    sundateRise = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddHours(sundateResults[0] / 60).AddMinutes(sundateResults[0] % 60);
//                    //sundateSet = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddHours(sundateResults[1] / 60).AddMinutes(sundateResults[1] % 60);
//                }
//                catch (Exception e)
//                {
//                    var e2 = e;
//                    throw e;
//                }
//                SunriseCalc sunriseCalc;
//                try
//                {
//                    sunriseCalc = new SunriseCalc(lat, lon);
//                }
//                catch (Exception e)
//                {
//                    var e2 = e;
//                    throw e;
//                }
//                var dayType = sunriseCalc.GetRiseAndSet(out DateTime rise, out DateTime set);
//                if (dayType == DiurnalResult.NormalDay)
//                {
//                    try
//                    {
//                        AssertAreWithinNMinutes(sundateRise, rise + tzOffset, minuteAccuracy);
//                        //AssertAreWithinNMinutes(sundateSet, set + tzOffset, minuteAccuracy);
//                    }
//                    catch (Exception e)
//                    {
//                        var e2 = e;
//                        throw e;
//                    }
//                }
//            }
//        }
//#endif
    }
}

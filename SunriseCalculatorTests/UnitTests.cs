using Microsoft.VisualStudio.TestTools.UnitTesting;
using SunriseCalculator;
using System;

namespace SunriseCalculatorTests
{
    [TestClass]
    public class UnitTests
    {
        private DateTime ReduceToMinutes(DateTime value) => new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute + (int)Math.Round(value.Second / 60.0), 0);

        [TestMethod]
        public void SimpleTestNYC()
        {
            DateTime testDate = new DateTime(2021, 7, 9);
            TimeSpan NYCTimezoneOffset = TimeSpan.FromHours(4);
            const double NYCLat = 40.7128;
            const double NYCLong = -74.0060;
            DateTime actualSunrise = testDate.AddHours(5).AddMinutes(33);
            DateTime actualSunset = testDate.AddHours(20).AddMinutes(29);

            SunriseCalculator.SunriseCalc nyc = new SunriseCalculator.SunriseCalc(NYCLat, NYCLong, testDate);
            var result = nyc.GetRiseAndSet(out DateTime sunrise, out DateTime sunset);

            // The sun always rises on New York City.
            Assert.AreEqual(SunriseCalculator.Enums.DiurnalResult.NormalDay, result);

            // The sunrise and sunset should be within a minute of the expected value.
            Assert.AreEqual(actualSunrise + NYCTimezoneOffset, ReduceToMinutes(sunrise));
            Assert.AreEqual(actualSunset + NYCTimezoneOffset, ReduceToMinutes(sunset));

            var riseResult = nyc.GetSunrise(out DateTime sunrise2);
            Assert.AreEqual(SunriseCalculator.Enums.DiurnalResult.NormalDay, riseResult);

            // We expect both methods to return the same value.
            Assert.AreEqual(sunrise, sunrise2);

            var setResult = nyc.GetSunset(out DateTime sunset2);
            Assert.AreEqual(SunriseCalculator.Enums.DiurnalResult.NormalDay, setResult);
            Assert.AreEqual(sunset, sunset2);
        }
    }
}

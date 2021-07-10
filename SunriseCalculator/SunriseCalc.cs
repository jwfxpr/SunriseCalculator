// An implementation of Paul Schlyter's sunrise calculator in C# under an MIT license.
// Version 0.1, initial commit.

using SunriseCalculator.Classes;
using SunriseCalculator.Enums;
using System;

namespace SunriseCalculator
{
    /// <summary>
    /// Performs calculations for dawn and dusk times and related results for any position on 
    /// Earth, for any day from 1801 to 2099.
    /// </summary>
    public partial class SunriseCalc
    {
        /// <summary> Degrees to radians conversion factor. </summary>
        private const double DegRad = Math.PI / 180.0;

        private double latitude;
        private double longitude;
        private DateTime time;

        /// <summary>
        /// The maximum possible Latitude value. Latitudes above this do not exist.
        /// </summary>
        public const double MaxLatitude = 90.0;

        /// <summary>
        /// The maximum possible Longitude value. Longitudes above this wrap around to negative values.
        /// </summary>
        public const double MaxLongitude = 180.0;

        /// <summary>
        /// The minimum possible Latitude value. Latitudes below this do not exist.
        /// </summary>
        public const double MinLatitude = -90.0;

        /// <summary>
        /// The mininum possible Longitude value. Longitudes below this wrap around to positive values.
        /// </summary>
        public const double MinLongitude = -180.0;

        /// <summary>
        /// Creates a new instance of the sunrise calculator for a specified latitude, longitude,
        /// and optionally, day (default is today).
        /// </summary>
        /// <param name="latitude">The latitude, between -90.0 (the south pole,
        /// <see cref="MinLatitude"/>) and 90.0 (the north pole, <see cref="MaxLatitude"/>). Values
        /// outside this range will raise an exception.</param>
        /// <param name="longitude">The longitude, between -180.0 (the eastern side of the
        /// international dateline, <see cref="MinLongitude"/>) and and 180.0 (the western side of
        /// the international dateline, <see cref="MaxLongitude"/>). Values outside this range
        /// will be wrapped around.</param>
        /// <param name="day">Optionally specify any day for calculation, default is today. Results
        /// should be accurate for dates between 1801 and 2099.</param>
        /// <exception cref="ArgumentOutOfRangeException"><see cref="Latitude"/> must be in range <see cref="MinLatitude"/> (-90°) to <see cref="MaxLatitude"/> (90°).</exception>
        /// <exception cref="ArgumentOutOfRangeException"><see cref="Longitude"/> must be finite.</exception>
        public SunriseCalc(double latitude, double longitude, DateTime day = default)
        {
            Latitude = latitude;
            Longitude = longitude;
            Day = day;
        }

        /// <summary>
        /// The local sidereal time at the <see cref="Longitude"/>, in radians.
        /// </summary>
        /// <returns>The local sidereal time in radians, modulo 360.</returns>
        private double LocalSiderealTimeRadians => Rev2Pi(GMST0Radians(SolarPosition.LocalMeanSolarTimeEpoch) + Math.PI + Longitude * DegRad);

        /// <summary>
        /// The time (in UTC) at which the sun will be immediately above the <see cref="Longitude"/>
        /// (directly to the south in the northern hemisphere; to the north in the southern).
        /// </summary>
        private DateTime TimeSunAtLongitude => new DateTime(Day.Year, Day.Month, Day.Day, 0, 0, 0, DateTimeKind.Utc)
            .AddHours(12.0 - RadiansToHours(Rev1Pi(LocalSiderealTimeRadians - SolarPosition.RightAscensionRadians)));

        /// <summary>
        /// Sets and gets the day for which calculations are made. By default, the current date is used.
        /// </summary>
        public DateTime Day
        {
            get => time.Date; set
            {
                DateTime newTime = EpochHelper.LocalMidday(value == default ? DateTime.Today : value, Longitude);
                if (newTime != time)
                {
                    time = newTime;
                    SolarPosition = new SolarPosition(Day, Longitude);
                }
            }
        }

        /// <summary>
        /// The latitude of the geolocation to use for calculation, in degrees. North is positive,
        /// south is negative. Must be within <see cref="MinLatitude"/> (-90°) and <see cref="MaxLatitude"/> (90°).
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><see cref="Latitude"/> must be in range <see cref="MinLatitude"/> (-90°) to <see cref="MaxLatitude"/> (90°).</exception>
        public double Latitude
        {
            get => latitude; set
            {
                if (double.IsNaN(value) || (value < MinLatitude) || (value > MaxLatitude))
                    throw new ArgumentOutOfRangeException(nameof(value), value, $"{nameof(Latitude)} must be in range {MinLatitude}° to {MaxLatitude}°.");

                latitude = value;
            }
        }

        /// <summary>
        /// The longitude of the geolocation to use for calculation, in degrees. East is positive,
        /// west is negative. Values outside of <see cref="MinLongitude"/> (-180°) and
        /// <see cref="MaxLongitude"/> (180°) will be wrapped around.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><see cref="Longitude"/> must be finite.</exception>
        public double Longitude
        {
            get => longitude; set
            {
                if (double.IsNaN(value) || double.IsInfinity(value))
                    throw new ArgumentOutOfRangeException(nameof(value), value, $"{nameof(Longitude)} must be finite.");

                // Since Windows.Devices.Geolocation.BasicGeoposition can return longitudes outside -180..180, we'll constrain.
                const double revolution = 360.0;
                while (value < MinLongitude)
                    value += revolution;
                while (value > MaxLongitude)
                    value -= revolution;

                longitude = value;
                if (time != default)
                    SolarPosition = new SolarPosition(Day, longitude);
            }
        }

        /// <summary>
        /// This object holds values for the position of the sun on the day of <see cref="Day"/>.
        /// </summary>
        public SolarPosition SolarPosition { get; private set; }

        /// <summary>
        /// Given a value in radians, returns that value constrained to between -Pi and Pi radians.
        /// </summary>
        /// <param name="value">Any value in radians.</param>
        /// <returns>The value constrained to the -Pi and Pi radians range.</returns>
        private static double Rev1Pi(double value) => value % Math.PI;

        /// <summary>
        /// Given a value in radians, returns that value constrained to between 0 and 2 Pi radians.
        /// </summary>
        /// <param name="value">Any value in radians.</param>
        /// <returns>The value constrained to the 0 to 360 degree range.</returns>
        private static double Rev2Pi(double value)
        {
            double result = value % (2 * Math.PI);
            return result < 0 ? result + (2 * Math.PI) : result;
        }

        /// <summary>
        /// Given a diurnal arc in radians, returns the corresponding time of day on the day of <see cref="Day"/>.
        /// </summary>
        /// <param name="radiansOfArc">An arc value in radians.</param>
        /// <returns>The corresponding time on the specified day.</returns>
        private TimeSpan ArcRadiansToTimeSpan(double radiansOfArc) => TimeSpan.FromHours(RadiansToHours(radiansOfArc));

        /// <summary>
        /// Computes the diurnal arc that the sun traverses to reach the specified altitude.
        /// Use <see cref="RadiansToHours(double)"/> to convert result to hours.
        /// </summary>
        /// <param name="altitude">The altitude threshold, in degrees. This is the 'horizon' for the calculation.</param>
        /// <param name="arcRadians">The diurnal arc, in radians.</param>
        /// <returns>A value indicating whether the sun rises (crosses the horizon) on the specified day.</returns>
        private DiurnalResult DiurnalArcRadians(double altitude, out double arcRadians)
        {
            double cosineOfArc = (Math.Sin(altitude * DegRad) - (Math.Sin(Latitude * DegRad) * Math.Sin(SolarPosition.DeclinationRadians))) / (Math.Cos(Latitude * DegRad) * Math.Cos(SolarPosition.DeclinationRadians));
            if (cosineOfArc >= 1.0)
            {
                arcRadians = 0.0;
                return DiurnalResult.SunAlwaysBelow;
            }
            else if (cosineOfArc <= -1.0)
            {
                arcRadians = Math.PI;
                return DiurnalResult.SunAlwaysAbove;
            }
            else
            {
                arcRadians = Math.Acos(cosineOfArc);
                return DiurnalResult.NormalDay;
            }
        }

        /// <summary>
        /// Returns the Greenwich Mean Sidereal Time at 0h UT (i.e. the sidereal time at the
        /// Greenwhich meridian at 0h UT) in radians.  GMST is then the sidereal time at Greenwich at any time
        /// of the day.
        /// </summary>
        /// <param name="epochDay">The days since the beginning of the J2000 epoch.</param>
        /// <remarks>For a full explanation of this value, refer to the original comments in Paul
        /// Schlyter's code, details in the <c>README.md</c> file.</remarks>
        private double GMST0Radians(double epochDay)
        {
            return Rev2Pi(Math.PI + 356.0470 * DegRad + 282.9404 * DegRad + ((0.9856002585 * DegRad + 4.70935E-5 * DegRad) * epochDay));
        }

        /// <summary>
        /// Selects the correct horizon value in degrees.
        /// </summary>
        private double HorizonDegrees(Horizon horizon)
        {
            const double AstronomicalHorizon = -18.0;
            const double CivilHorizon = -6.0;
            const double NauticalHorizon = -12.0;
            const double NominalHorizon = -35.0 / 60.0;

            switch (horizon)
            {
                case Horizon.Normal:
                    return NominalHorizon - SolarPosition.ApparentRadiusDegrees;

                case Horizon.Civil:
                    return CivilHorizon;

                case Horizon.Nautical:
                    return NauticalHorizon;

                case Horizon.Astronomical:
                    return AstronomicalHorizon;

                default:
                    throw new NotImplementedException($"No definition found for horizon {horizon}");
            }
        }

        /// <summary>
        /// Converts a right ascension or arc value in radians to a value in hours.
        /// </summary>
        private double RadiansToHours(double radians) => 12 / Math.PI * radians;

        /// <summary>
        /// Returns a <see cref="TimeSpan"/> of the time the sun spends above the specified <see cref="Horizon"/>
        /// on the day of <see cref="Day"/>.
        /// </summary>
        /// <param name="horizon">The <see cref="Horizon"/> to use for day length calculation.</param>
        /// <returns>A <see cref="TimeSpan"/> for the specified day length.</returns>
        public TimeSpan GetDayLength(Horizon horizon = Horizon.Normal)
        {
            _ = DiurnalArcRadians(HorizonDegrees(horizon), out double arcRadians);
            TimeSpan halfDay = ArcRadiansToTimeSpan(arcRadians);

            return 2 * halfDay;
        }

        /// <summary>
        /// Returns sunrise and sunset in UTC on the day specified in <see cref="Day"/>, and a
        /// <see cref="DiurnalResult"/> indicating whether the sun rises or not.
        /// </summary>
        /// <param name="sunrise">The calculated sunrise (in UTC) for the specified day.</param>
        /// <param name="sunset">The calculated sunset for the specified day.</param>
        /// <param name="timeZone">Optionally, convert the times to the provided time zone. In not specified, times are in UTC.</param>
        /// <param name="horizon">The horizon to use for the sunrise calculation.</param>
        /// <returns>A value indicating whether the sun rises (crosses the horizon) on the specified day.</returns>
        public DiurnalResult GetRiseAndSet(out DateTime sunrise, out DateTime sunset, TimeZoneInfo timeZone = null, Horizon horizon = Horizon.Normal)
        {
            DiurnalResult result = DiurnalArcRadians(HorizonDegrees(horizon), out double arcRadians);
            TimeSpan halfDay = ArcRadiansToTimeSpan(arcRadians);

            if (timeZone == null)
            {
                sunrise = TimeSunAtLongitude - halfDay;
                sunset = TimeSunAtLongitude + halfDay;
            }
            else
            {
                sunrise = TimeZoneInfo.ConvertTimeFromUtc(TimeSunAtLongitude - halfDay, timeZone);
                sunset = TimeZoneInfo.ConvertTimeFromUtc(TimeSunAtLongitude + halfDay, timeZone);
            }

            return result;
        }

        /// <summary>
        /// Returns sunrise in UTC on the day specified in <see cref="Day"/>, and a <see cref="DiurnalResult"/>
        /// indicating whether the sun rises or not.
        /// </summary>
        /// <param name="sunrise">The calculated sunrise (in UTC) for the specified day.</param>
        /// <param name="timeZone">Optionally, convert the times to the provided time zone. In not specified, times are in UTC.</param>
        /// <param name="horizon">The horizon to use for the sunrise calculation.</param>
        /// <returns>A value indicating whether the sun rises (crosses the horizon) on the specified day.</returns>
        public DiurnalResult GetSunrise(out DateTime sunrise, TimeZoneInfo timeZone = null, Horizon horizon = Horizon.Normal)
        {
            DiurnalResult result = DiurnalArcRadians(HorizonDegrees(horizon), out double arcRadians);
            TimeSpan halfDay = ArcRadiansToTimeSpan(arcRadians);

            if (timeZone == null)
                sunrise = TimeSunAtLongitude - halfDay;
            else
                sunrise = TimeZoneInfo.ConvertTimeFromUtc(TimeSunAtLongitude - halfDay, timeZone);

            return result;
        }

        /// <summary>
        /// Returns sunset in UTC on the day specified in <see cref="Day"/>, and a <see cref="DiurnalResult"/>
        /// indicating whether the sun sets or not.
        /// </summary>
        /// <param name="sunset">The calculated sunset for the specified day.</param>
        /// <param name="timeZone">Optionally, convert the times to the provided time zone. In not specified, times are in UTC.</param>
        /// <param name="horizon">The horizon to use for the sunset calculation.</param>
        /// <returns>A value indicating whether the sun sets (crosses the horizon) on the specified day.</returns>
        public DiurnalResult GetSunset(out DateTime sunset, TimeZoneInfo timeZone = null, Horizon horizon = Horizon.Normal)
        {
            DiurnalResult result = DiurnalArcRadians(HorizonDegrees(horizon), out double arcRadians);
            TimeSpan halfDay = ArcRadiansToTimeSpan(arcRadians);

            if (timeZone == null)
                sunset = TimeSunAtLongitude + halfDay;
            else
                sunset = TimeZoneInfo.ConvertTimeFromUtc(TimeSunAtLongitude + halfDay, timeZone);

            return result;
        }
    }
}
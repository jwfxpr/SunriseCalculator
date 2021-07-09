// An implementation of Paul Schlyter's sunrise calculator in C# under an MIT license.
// Version 0.1, initial commit.

using SunriseCalculator.Classes;
using SunriseCalculator.Enums;
using System;

namespace SunriseCalculator
{
    public partial class SunriseCalc
    {
        /// <summary> Radians to degrees conversion factor. </summary>
        private const double RadDeg = 180.0 / Math.PI;

        /// <summary> Degrees to radians conversion factor. </summary>
        private const double DegRad = Math.PI / 180.0;

        private const double AstronomicalHorizon = -18.0;
        private const double CivilHorizon = -6.0;
        private const double NauticalHorizon = -12.0;
        private const double NominalHorizon = -35.0 / 60.0;
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
        /// The local sidereal time at the <see cref="Longitude"/>. This value is in degrees.
        /// </summary>
        /// <returns>The local sidereal time in degrees, modulo 360.</returns>
        private double LocalSiderealTimeDegrees => Rev360(GMST0(SolarPosition.LocalMeanSolarTimeEpoch) + 180.0 + Longitude);

        /// <summary>
        /// The time (in UTC) at which the Sun will be immediately above the <see cref="Longitude"/>
        /// (directly to the south in the northern hemisphere; to the north in the southern).
        /// </summary>
        private DateTime TimeSunAtLongitude => new DateTime(Day.Year, Day.Month, Day.Day, 0, 0, 0, DateTimeKind.Utc)
            .AddHours(12.0 - DegreesToHours(Rev180(LocalSiderealTimeDegrees - SolarPosition.RightAscensionDegrees)));

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
        /// This object holds values for the position of the Sun on the day of <see cref="Day"/>.
        /// </summary>
        public SolarPosition SolarPosition { get; private set; }

        /// <summary>
        /// Given a value in degrees, returns that value constrained to between -180 and 180 degrees.
        /// </summary>
        /// <param name="value">Any value in degrees.</param>
        /// <returns>The value constrained to the -180 to 180 degree range.</returns>
        private static double Rev180(double value) => value % 180.0;

        /// <summary>
        /// Given a value in degrees, returns that value constrained to between 0 and 360 degrees.
        /// </summary>
        /// <param name="value">Any value in degrees.</param>
        /// <returns>The value constrained to the 0 to 360 degree range.</returns>
        private static double Rev360(double value)
        {
            double result = value % 360.0;
            return result < 0 ? result + 360.0 : result;
        }

        /// <summary>
        /// Converts a right ascension or diurnal arc value in degrees to a value in hours.
        /// </summary>
        private double DegreesToHours(double degrees) => degrees / 15.0;

        /// <summary>
        /// Computes the diurnal arc that the Sun traverses to reach the specified altitude.
        /// Use <see cref="DegreesToHours(double)"/> to convert result to hours.
        /// </summary>
        /// <param name="altitude">The altitude threshold, in degrees.</param>
        /// <param name="diurnalArc">The diurnal arc in degrees.</param>
        /// <returns>A value indicating whether the Sun rises (crosses the horizon) on the specified day.</returns>
        private DiurnalResult DiurnalArc(double altitude, out double diurnalArc)
        {
            double cosineOfArc = (Math.Sin(altitude * DegRad) - (Math.Sin(Latitude * DegRad) * Math.Sin(SolarPosition.DeclinationRadians))) / (Math.Cos(Latitude * DegRad) * Math.Cos(SolarPosition.DeclinationRadians));
            if (cosineOfArc >= 1.0)
            {
                diurnalArc = 0.0;
                return DiurnalResult.SunAlwaysBelow;
            }
            else if (cosineOfArc <= -1.0)
            {
                diurnalArc = 180.0;
                return DiurnalResult.SunAlwaysAbove;
            }
            else
            {
                diurnalArc = Math.Acos(cosineOfArc) * RadDeg;
                return DiurnalResult.NormalDay;
            }
        }

        /// <summary>
        /// Given a diurnal arc in degrees, returns the corresponding time of day on the day of <see cref="Day"/>.
        /// </summary>
        /// <param name="degreesOfArc">A diurnal arc value in degrees, calculated by <see cref="DiurnalArc(double, out double)"/></param>
        /// <returns>The corresponding time on the specified day.</returns>
        private TimeSpan DiurnalArcToTimeSpan(double degreesOfArc) => TimeSpan.FromHours(DegreesToHours(degreesOfArc));

        /// <summary>
        /// Returns the Greenwich Mean Sidereal Time at 0h UT (i.e. the sidereal time at the
        /// Greenwhich meridian at 0h UT) in degrees.  GMST is then the sidereal time at Greenwich at any time
        /// of the day.
        /// </summary>
        /// <param name="epochDay">The days since the beginning of the <see cref="J2000"/> epoch.</param>
        /// <remarks>For a full explanation of this value, refer to the original comments in Paul 
        /// Schlyter's code, details in the <c>README.md</c> file.</remarks>
        private double GMST0(double epochDay) => Rev360(180.0 + 356.0470 + 282.9404 + ((0.9856002585 + 4.70935E-5) * epochDay));

        /// <summary>
        /// Selects the correct horizon value in degrees.
        /// </summary>
        private double HorizonDegrees(Horizon horizon)
        {
            switch (horizon)
            {
                case Horizon.Normal:
                    return NominalHorizon - SolarPosition.ApparentRadiusDegrees;

                case Horizon.Nominal:
                    return NominalHorizon;

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
        /// Returns a <see cref="TimeSpan"/> of the time the Sun spends above the specified <see cref="Horizon"/>
        /// on the day of <see cref="Day"/>.
        /// </summary>
        /// <param name="horizon">The <see cref="Horizon"/> to use for day length calculation.</param>
        /// <returns>A <see cref="TimeSpan"/> for the specified day length.</returns>
        public TimeSpan GetDayLength(Horizon horizon = Horizon.Normal)
        {
            double sineOfSolarDeclination = Math.Sin(SolarPosition.ObliquityOfEcliptic) * Math.Sin(Longitude * DegRad);
            double cosineOfSolarDeclination = Math.Sqrt(1.0 - sineOfSolarDeclination * sineOfSolarDeclination);
            double cosineOfArc = Math.Sin(HorizonDegrees(horizon) * DegRad - Math.Sin(Latitude * DegRad) * sineOfSolarDeclination)
                / (Math.Cos(Latitude * DegRad) * cosineOfSolarDeclination);

            if (cosineOfArc >= 1.0)
                return TimeSpan.Zero; // Sun always below horizon
            else if (cosineOfArc <= -1.0)
                return TimeSpan.FromDays(1); // Sun always above horizon
            else
                return TimeSpan.FromHours(2 * DegreesToHours(Math.Acos(cosineOfArc) * RadDeg));
        }

        /// <summary>
        /// Returns sunrise and sunset in UTC on the day specified in <see cref="Day"/>, and a 
        /// <see cref="DiurnalResult"/> indicating whether the Sun rises or not.
        /// </summary>
        /// <param name="sunrise">The calculated sunrise (in UTC) for the specified day.</param>
        /// <param name="sunset">The calculated sunset for the specified day.</param>
        /// <param name="horizon">The horizon to use for the sunrise calculation.</param>
        /// <returns>A value indicating whether the Sun rises (crosses the horizon) on the specified day.</returns>
        public DiurnalResult GetRiseAndSet(out DateTime sunrise, out DateTime sunset, Horizon horizon = Horizon.Normal)
        {
            DiurnalResult result = DiurnalArc(HorizonDegrees(horizon), out double diurnalArc);

            TimeSpan halfDay = DiurnalArcToTimeSpan(diurnalArc);
            sunrise = TimeSunAtLongitude - halfDay;
            sunset = TimeSunAtLongitude + halfDay;
            return result;
        }

        /// <summary>
        /// Returns sunrise in UTC on the day specified in <see cref="Day"/>, and a <see cref="DiurnalResult"/>
        /// indicating whether the Sun rises or not.
        /// </summary>
        /// <param name="sunrise">The calculated sunrise (in UTC) for the specified day.</param>
        /// <param name="horizon">The horizon to use for the sunrise calculation.</param>
        /// <returns>A value indicating whether the Sun rises (crosses the horizon) on the specified day.</returns>
        public DiurnalResult GetSunrise(out DateTime sunrise, Horizon horizon = Horizon.Normal)
        {
            var result = DiurnalArc(HorizonDegrees(horizon), out double diurnalArc);
            sunrise = TimeSunAtLongitude - DiurnalArcToTimeSpan(diurnalArc);
            return result;
        }

        /// <summary>
        /// Returns sunset in UTC on the day specified in <see cref="Day"/>, and a <see cref="DiurnalResult"/>
        /// indicating whether the Sun sets or not.
        /// </summary>
        /// <param name="sunset">The calculated sunset for the specified day.</param>
        /// <param name="horizon">The horizon to use for the sunset calculation.</param>
        /// <returns>A value indicating whether the Sun sets (crosses the horizon) on the specified day.</returns>
        public DiurnalResult GetSunset(out DateTime sunset, Horizon horizon = Horizon.Normal)
        {
            var result = DiurnalArc(HorizonDegrees(horizon), out double diurnalArc);
            sunset = TimeSunAtLongitude + DiurnalArcToTimeSpan(diurnalArc);
            return result;
        }
    }
}
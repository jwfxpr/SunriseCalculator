// An implementation of Paul Schlyter's sunrise calculator in C# under an MIT license.
// See SunriseCalc.cs for details.

using System;

namespace SunriseCalculator.Classes
{
    /// <summary>
    /// Performs calculations for the position of the sun in the sky from arbitrary positions on 
    /// Earth on a given day. Results are accurate for dates from 1801 to 2099.
    /// </summary>
    public class SolarPosition
    {
        /// <summary> Radians to degrees conversion factor. </summary>
        private const double RadDeg = 180.0 / Math.PI;

        /// <summary> Degrees to radians conversion factor. </summary>
        private const double DegRad = Math.PI / 180.0;

        private const double SolarRadiusAt1AUDegrees = 0.2666;

        /// <summary>
        /// This value represents the days between January 1, 2000 at 00:00:00 (the start of the 
        /// current astronomical epoch) and local midday at the specified <see cref="Longitude"/>
        /// and <see cref="Date"/>.
        /// </summary>
        public readonly double LocalMeanSolarTimeEpoch;

        /// <summary>
        /// Creates an instance for the specified day at the specified longitude. Calculations 
        /// should be accurate for days from 1801 to 2099. Dates outside this range will be 
        /// accepted, but accuracy will be reduced.
        /// </summary>
        /// <param name="day">The day for which to calculate solar position values. Dates from 1801
        /// to 2099 will be yield the most accurate results.</param>
        /// <param name="longitude">The local longitude for which to calculate values. If accuracy 
        /// is not important, this can be zero.</param>
        /// <exception cref="ArgumentOutOfRangeException"><c>longitude</c> must be in range -180.0 to 180.0.</exception>
        public SolarPosition(DateTime day, double longitude)
        {
            if (double.IsNaN(longitude) || longitude < -180 || longitude > 180)
                throw new ArgumentOutOfRangeException(nameof(longitude), longitude, $"{nameof(longitude)} must be in range -180.0 to 180.0.");

            Date = day.Date;
            Longitude = longitude;
            LocalMeanSolarTimeEpoch = EpochHelper.EpochDayLocalMidday(day, longitude);
        }

        /// <summary>
        /// Mean anomaly of the sun, in radians.
        /// </summary>
        private double MeanAnomaly => Rev2Pi(356.0470 * DegRad + (0.9856002585 * DegRad) * LocalMeanSolarTimeEpoch);
        
        /// <summary>
        /// Mean longitude of perihelion, in radians.
        /// </summary>
        /// <remarks>The Suns' mean longitude = <see cref="MeanAnomaly"/> + <see cref="MeanLongitudeOfPerihelion"/>.</remarks>
        private double MeanLongitudeOfPerihelion => (282.9404 * DegRad + (4.70935E-5 * DegRad) * LocalMeanSolarTimeEpoch);

        /// <summary>
        /// Eccentricity of Earth's orbit. This value is unitless.
        /// </summary>
        private double EarthOrbitEccentricity => 0.016709 - 1.151E-9 * LocalMeanSolarTimeEpoch;

        /// <summary>
        /// Eccentric anomaly, the angle between the direction of periapsis and the current 
        /// position of the Earth on its orbit, in radians.
        /// </summary>
        private double EccentricAnomaly => MeanAnomaly + EarthOrbitEccentricity * Math.Sin(MeanAnomaly) * (1.0 + EarthOrbitEccentricity * Math.Cos(MeanAnomaly));

        /// <summary>
        /// X coordinate in orbit, in AU.
        /// </summary>
        private double OrbitXCoordinate => Math.Cos(EccentricAnomaly) - EarthOrbitEccentricity;

        /// <summary>
        /// Y coordinate in orbit, in AU.
        /// </summary>
        private double OrbitYCoordinate => Math.Sqrt(1.0 - Square(EarthOrbitEccentricity)) * Math.Sin(EccentricAnomaly);

        /// <summary>
        /// Distance to the sun, in AU.
        /// </summary>
        public double DistanceToSun => Hypotenuse(OrbitXCoordinate, OrbitYCoordinate);

        /// <summary>
        /// The sun's true anomaly, in radians.
        /// </summary>
        private double TrueAnomaly => Math.Atan2(OrbitYCoordinate, OrbitXCoordinate);

        /// <summary>
        /// The true solar longitude, in radians.
        /// </summary>
        private double TrueSolarLongitude => Rev2Pi(TrueAnomaly + MeanLongitudeOfPerihelion);

        /// <summary>
        /// The obliquity of the ecliptic, or the inclination of Earth's axis, in radians.
        /// </summary>
        public double ObliquityOfEcliptic => (23.4393 * DegRad - (3.563E-7 * DegRad) * LocalMeanSolarTimeEpoch);

        /// <summary>
        /// The ecliptic rectangular x coordinate of the sun's position in the sky.
        /// This is the same as the <see cref="EquatorialXCoordinate"/>.
        /// </summary>
        private double EclipticRectangularXCoordinate => DistanceToSun * Math.Cos(TrueSolarLongitude);

        /// <summary>
        /// The ecliptic rectangular y coordinate of the sun's position in the sky.
        /// </summary>
        private double EclipticRectangularYCoordinate => DistanceToSun * Math.Sin(TrueSolarLongitude);

        /// <summary>
        /// The equatorial rectangular x coordinate of the sun's position in the sky.
        /// This is the same as the <see cref="EclipticRectangularXCoordinate"/>.
        /// </summary>
        private double EquatorialXCoordinate => EclipticRectangularXCoordinate;

        /// <summary>
        /// The equatorial rectangular z coordinate of the sun's position in the sky.
        /// </summary>
        private double EquatorialZCoordinate => EclipticRectangularYCoordinate * Math.Sin(ObliquityOfEcliptic);

        /// <summary>
        /// The equatorial rectangular y coordinate of the sun's position in the sky.
        /// </summary>
        private double EquatorialYCoordinate => EclipticRectangularYCoordinate * Math.Cos(ObliquityOfEcliptic);

        /// <summary>
        /// The sun's apparent radius from Earth on the given day, in degrees.
        /// </summary>
        public double ApparentRadiusDegrees => SolarRadiusAt1AUDegrees / DistanceToSun;

        /// <summary>
        /// The sun's apparent radius from Earth on the given day, in radians.
        /// </summary>
        public double ApparentRadiusRadians => (SolarRadiusAt1AUDegrees * DegRad) / DistanceToSun;

        /// <summary>
        /// Gets the date for which this instance is calculated. Calculations should be accurate
        /// for dates between 1801 and 2099.
        /// </summary>
        public readonly DateTime Date;

        /// <summary>
        /// Gets the longitude (on the surface of Earth) for which this instance is calculated. If 
        /// accuracy is not important, this can be zero.
        /// </summary>
        public readonly double Longitude;

        /// <summary>
        /// The sun's right ascension, in radians.
        /// </summary>
        public double RightAscensionRadians => Math.Atan2(EquatorialYCoordinate, EquatorialXCoordinate);

        /// <summary>
        /// The sun's right ascension, in degrees.
        /// </summary>
        public double RightAscensionDegrees => RightAscensionRadians * RadDeg;

        /// <summary>
        /// The sun's declination, in radians.
        /// </summary>
        public double DeclinationRadians => Math.Atan2(EquatorialZCoordinate, Hypotenuse(EquatorialXCoordinate, EquatorialYCoordinate));

        /// <summary>
        /// The sun's declination, in degrees.
        /// </summary>
        public double DeclinationDegrees => DeclinationRadians * RadDeg;

        /// <summary>
        /// Returns the square root of the sum of the squares of the parameters.
        /// </summary>
        private static double Hypotenuse(double x, double y) => Math.Sqrt(Square(x) + Square(y));

        /// <summary>
        /// Given a value in radians, returns that value constrained to between 0 and 2*Pi.
        /// </summary>
        /// <param name="value">Any value in radians.</param>
        /// <returns>The value constrained to the 0 to 2*Pi radians range.</returns>
        private static double Rev2Pi(double value)
        {
            const double revolution = Math.PI * 2;
            double result = value % revolution;
            return result < 0 ? result + revolution : result;
        }

        /// <summary>
        /// Returns the square of the value.
        /// </summary>
        private static double Square(double value) => value * value;
    }
}
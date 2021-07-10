// An implementation of Paul Schlyter's sunrise calculator in C# under an MIT license.
// See SunriseCalc.cs for details.

using System;

namespace SunriseCalculator.Classes
{
    /// <summary>
    /// A small helper class with functions useful for calculations in the J2000 era.
    /// </summary>
    public static class EpochHelper
    {
        private const double MaxLongitude = 180;

        /// <summary>
        /// The maximum year for which calculations are accurate.
        /// </summary>
        public const int MaxYear = 2099;

        /// <summary>
        /// The minimum year for which calculations are accurate.
        /// </summary>
        public const int MinYear = 1801;

        /// <summary>
        /// The start of the current astronomical epoch, at 00:00:00 on January 1, 2000.
        /// </summary>
        public static readonly DateTime J2000 = new DateTime(2000, 1, 1, 0, 0, 0);

        /// <summary>
        /// Returns the epoch day, or the days between the provided <see cref="DateTime"/> and <see cref="J2000"/>,
        /// the start of the current astronomical epoch.
        /// </summary>
        /// <param name="dateTime">The moment for which to calculate the epoch day.</param>
        /// <returns>The epoch day for the specified time.</returns>
        public static double DaysSinceJ2000(DateTime dateTime) => (dateTime - J2000).TotalDays;

        /// <summary>
        /// Returns the epoch day of local midday at the time and longitude specified.
        /// </summary>
        /// <param name="dateTime">A date for which to calculate the local midday. Hours, minutes, and seconds of this value are ignored.</param>
        /// <param name="longitude">The longitude at which local midday will be calculated.</param>
        /// <returns></returns>
        public static double EpochDayLocalMidday(DateTime dateTime, double longitude) => DaysSinceJ2000(dateTime.Date) + 0.5 - (longitude % MaxLongitude / 360.0);

        /// <summary>
        /// Converts an epoch day to a <see cref="DateTime"/> value.
        /// </summary>
        /// <param name="epochDays">The epoch day. See <see cref="DaysSinceJ2000(DateTime)"/> for more information.</param>
        /// <returns></returns>
        public static DateTime EpochDayToDateTime(double epochDays) => J2000.AddDays(epochDays);

        /// <summary>
        /// Returns the local midday in UTC at the longitude specified.
        /// </summary>
        /// <param name="dateTime">A date for which to calculate the local midday. Hours, minutes, and seconds of this value are ignored.</param>
        /// <param name="longitude">The longitude at which local midday will be calculated.</param>
        /// <returns></returns>
        public static DateTime LocalMidday(DateTime dateTime, double longitude) => dateTime.Date.AddDays(0.5 - (longitude % MaxLongitude / 360.0));

        /// <summary>
        /// Returns the <see cref="DateTime"/> of midday for the provided day. The returned time
        /// represents midday in UTC, ignoring longitude.
        /// </summary>
        /// <param name="day">A date for which to calculate midday. Hours, minutes and seconds are ignored.</param>
        /// <returns>UTC midday on the provided day.</returns>
        public static DateTime UTCMidday(DateTime day) => day.Date.AddHours(12);

        /// <summary>
        /// Returns the epoch day of midday for the provided day. The returned time
        /// represents midday in UTC, ignoring longitude.
        /// </summary>
        /// <param name="epochDay">A date for which to calculate midday. Hours, minutes and seconds are ignored.</param>
        /// <returns>UTC midday on the provided day.</returns>
        public static double UTCMidday(double epochDay) => Math.Floor(epochDay) + 0.5;
    }
}
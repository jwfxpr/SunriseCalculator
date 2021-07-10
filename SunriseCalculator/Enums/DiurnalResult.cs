// An implementation of Paul Schlyter's sunrise calculator in C# under an MIT license.
// See SunriseCalc.cs for details.

namespace SunriseCalculator.Enums
{
    /// <summary>
    /// Enumerates the possible cases for the results of diurnal arc calculations. This return 
    /// value indicates whether or not the sun passes the calculated horizon line on the given day.
    /// </summary>
    public enum DiurnalResult
    {
        /// <summary>
        /// The diurnal arc is normal; the sun crosses the specified altitude on the specified day.
        /// </summary>
        NormalDay,

        /// <summary>
        /// The sun remains above the specified altitude for all 24h of the specified day. The time
        /// returned is when the sun is closest to the horizon (directly to the south in the
        /// northern hemisphere; to the north in the southern); sunrise is 12 hours before
        /// midday, and sunset is 12 hours after.
        /// </summary>
        SunAlwaysAbove,

        /// <summary>
        /// The sun remains below the specified altitude for all 24h of the specified day. Sunrise
        /// and sunset are both calculated as when the sun is closest to the horizon (directly to
        /// the south in the northern hemisphere; to the north in the southern).
        /// </summary>
        SunAlwaysBelow
    }
}
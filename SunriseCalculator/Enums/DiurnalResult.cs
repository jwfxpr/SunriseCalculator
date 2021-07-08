﻿// An implementation of Paul Schlyter's sunrise calculator in C# under an MIT license.
// See SunriseCalc.cs for details.

namespace SunriseCalculator.Enums
{
    public enum DiurnalResult
    {
        /// <summary>
        /// The diurnal arc is normal; the Sun crosses the specified altitude on the specified day.
        /// </summary>
        NormalDay,

        /// <summary>
        /// The Sun remains above the specified altitude for all 24h of the specified day. The time
        /// returned is when the Sun is closest to the horizon (directly to the south in the
        /// northern hemisphere; to the north in the southern); sunrise is 12 hours before
        /// midday, and sunset is 12 hours after.
        /// </summary>
        SunAlwaysAbove,

        /// <summary>
        /// The Sun remains below the specified altitude for all 24h of the specified day. Sunrise
        /// and sunset are both calculated as when the Sun is closest to the horizon (directly to
        /// the south in the northern hemisphere; to the north in the southern).
        /// </summary>
        SunAlwaysBelow
    }
}
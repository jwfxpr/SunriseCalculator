// An implementation of Paul Schlyter's sunrise calculator in C# under an MIT license.
// See SunriseCalc.cs for details.

namespace SunriseCalculator.Enums
{
    /// <summary>
    /// Enumerates the possible horizon types for dawn and dusk calculations.
    /// </summary>
    public enum Horizon
    {
        /// <summary>
        /// Normally, sunrise/set is considered to occur when the sun's upper limb is 35 arc
        /// minutes below the horizon (this accounts for the refraction of the Earth's atmosphere).
        /// </summary>
        Normal,

        /// <summary>
        /// Civil dawn or dusk marks the start or end of the brightest of the standard
        /// twilights, followed by <see cref="Nautical"/> and <see cref="Astronomical"/> twilights.
        /// It occurs when the center of the sun passes 6° below the horizon.
        /// </summary>
        Civil,

        /// <summary>
        /// Nautical dawn or dusk marks the start or end of the intermediate brightness of
        /// twilight, between <see cref="Civil"/> and <see cref="Astronomical"/> twilights.
        /// It occurs when the center of the sun passes 12° below the horizon.
        /// </summary>
        Nautical,

        /// <summary>
        /// Astronomical dawn or dusk marks the start or end of the dimmest of the standard
        /// twilights, followed by <see cref="Nautical"/> and <see cref="Civil"/> twilights.
        /// It occurs when the center of the sun passes 18° below the horizon.
        /// </summary>
        Astronomical,
    }
}
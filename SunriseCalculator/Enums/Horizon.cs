// An implementation of Paul Schlyter's sunrise calculator in C# under an MIT license.
// See SunriseCalc.cs for details.

namespace SunriseCalculator.Enums
{
    public enum Horizon
    {
        /// <summary>
        /// Normally, sunrise/set is considered to occur when the Sun's upper limb is 35 arc
        /// minutes below the horizon (this accounts for the refraction of the Earth's atmosphere).
        /// </summary>
        Normal,

        /// <summary>
        /// Sunrise and sunset are calculated according to the center of the Sun passing the nominal
        /// horizon (at 35 arc minutes below 0 degrees), ignoring the Sun's apparent radius.
        /// </summary>
        Nominal,

        /// <summary>
        /// Civil dawn or dusk marks the start or end of the brightest of the standard
        /// twilights, followed by <see cref="Nautical"/> and <see cref="Astronomical"/> twilights.
        /// It occurs when the center of the Sun passes 6° below the horizon.
        /// </summary>
        Civil,

        /// <summary>
        /// Nautical dawn or dusk marks the start or end of the intermediate brightness of
        /// twilight, between <see cref="Civil"/> and <see cref="Astronomical"/> twilights.
        /// It occurs when the center of the Sun passes 12° below the horizon.
        /// </summary>
        Nautical,

        /// <summary>
        /// Astronomical dawn or dusk marks the start or end of the dimmest of the standard
        /// twilights, followed by <see cref="Nautical"/> and <see cref="Civil"/> twilights.
        /// It occurs when the center of the Sun passes 18° below the horizon.
        /// </summary>
        Astronomical,
    }
}
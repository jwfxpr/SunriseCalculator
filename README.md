# SunriseCalculator
An implementation of [Paul Schlyter's sunrise calculator](https://stjarnhimlen.se/comp/sunriset.c) ([wayback](https://web.archive.org/web/20210708103125/https://stjarnhimlen.se/comp/sunriset.c)) in C#, made available under an [MIT license](./LICENSE).

I have attempted to recreate the logic of the original as faithfully as possible, while still writing sensible and good quality C#. I also made it less northern hemisphere-centric as much as possible (the original assumes the Sun is always south of you).

The library is targeted to .NET Core 2.1 LTS and has no dependencies. It is intended to be as portable and interoperable with any existing C# project as possible without modification or refactoring. I currently have no plans to distribute this as a NuGet package or any other form than this repository, but if that would be helpful please don't hesitate to raise an issue.

Unit testing is done through the MSTest framework. The [`SunriseCalculatorTests`](./SunriseCalculatorTests) project targets .NET Core 2.1 LTS.

## Usage

``` C#
#using SunriseCalculator;
#using SunriseCalculator.Enums;

// Create an instance of the sunrise calculator for the desired location for today.
SunriseCalculator newYorkCityToday = new SunriseCalculator(latitude: 40.7128, longitude: -74.0060);

// Create an instance of the sunrise calculator for the desired location for tomorrow.
SunriseCalculator newYorkCityTomorrow = new SunriseCalculator(40.7128, -74.0060, DateTime.Today + TimeSpan.FromDays(1));

// Get today's sunrise and sunset times for New York City.
newYorkCityToday.GetRiseAndSet(out DateTime todaysSunset, out DateTime todaysSunrise);

// Get tomorrow's civil dawn time for New York City.
newYorkCityTomorrow.GetSunrise(out DateTime tomorrowsCivilSunrise, Horizon.Civil);

// Get the length of tomorrow's civil day in New York City.
TimeSpan tomorrowsCivilDayLength = newYorkCityTomorrow.GetDayLength(Horizon.Civil);

// Let's find out if the Sun will set or rise today in Inuvik, Canada, which is above the arctic circle.
SunriseCalculator inuvikToday = new SunriseCalculator(68.3607, -133.7230);
DiurnalResult inuvikDayType = inuvikToday.GetSunrise(out DateTime inuvikSunset);
switch (inuvikDayType)
{
    case DiurnalResult.NormalDay:
        // Today the Sun will rise and set in Inuvik.
        break;
    case DiurnalResult.SunAlwaysAbove:
        // Today the Sun will never set in Inuvik.
        break;
    case DiurnalResult.SunAlwaysBelow:
        // Today the Sun will never rise in Inuvik.
        break;
}
```

## `Horizon` types

The library has five different horizons that can be used to calculate sunrise and sunset times:

* `Horizon.Normal`: Normally, sunrise/set is considered to occur when the Sun's upper limb is 35 arc minutes below the horizon (this accounts for the refraction of the Earth's atmosphere).
* `Horizon.Nominal`: Sunrise and sunset are calculated according to the center of the Sun passing the nominal horizon (at 35 arc minutes below 0 degrees), ignoring the Sun's apparent radius.
* `Horizon.Civil`: Civil dawn or dusk marks the start or end of the brightest of the standard twilights, followed by `Nautical` and `Astronomical` twilights. It occurs when the center of the Sun passes 6° below the horizon.
* `Horizon.Nautical`: Nautical dawn or dusk marks the start or end of the intermediate brightness of twilight, between `Civil` and `Astronomical` twilights. It occurs when the center of the Sun passes 12° below the horizon.
* `Horizon.Astronomical`: Astronomical dawn or dusk marks the start or end of the dimmest of the standard twilights, followed by `Nautical` and `Civil` twilights. It occurs when the center of the Sun passes 18° below the horizon.

## `DiurnalResult` types

The methods `GetRiseAndSet(out DateTime, out Datetime)`, `GetSunrise(out Datetime)`, and `GetSunset(out Datetime)` return a `DiurnalResult` value. For all locations certain to be outside the arctic circles, this result can be safely ignored; however, for all locations at latitudes where the Sun may never set in Summer or never rise in Winter, this return value is useful.

The `Horizon` selected for the calculation will affect the `DiurnalResult`; a location close to the arctic circle may not see the Sun rise above the horizon at all (`Horizon.Normal`), but the Sun may still cross the `Horizon.Nautical` on that day.

Note that the latitudes of the arctic circles are variable, and shouldn't be hard-coded in your code.

* `DiurnalResult.NormalDay`: This is a normal day, and the Sun rises and sets above the specified `Horizon`.
* `DiurnalResult.SunAlwaysAbove`: The Sun remains above the specified `Horizon` for all 24h of the specified day. The time returned is when the Sun is closest to the horizon (directly to the south in the northern hemisphere; to the north in the southern); sunrise is 12 hours before the middle of the day, and sunset is 12 hours after, for a 24 hour day.
* `DiurnalResult.SunAlwaysBelow`: The Sun remains below the specified `Horizon` for all 24h of the specified day. Sunrise and sunset are both calculated as when the Sun is closest to the horizon (directly to the south in the northern hemisphere; to the north in the southern).

## Changelog and version history

Version 0.1: Initial release.

## Contributing

All contributions, issues, PRs, corrections, and discussions are welcome. I am new to open source distribution, and welcome all guidance and input. I have endeavoured to ensure that the code is well-documented and easy to follow for non-specialists as much as possible.

I am also not an astronomer, and my understanding or implementation of the original sunrise calculator may be partially or entirely incorrect. All mistakes are my own, and please point them out to me.
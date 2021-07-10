# SunriseCalculator
An implementation of [Paul Schlyter's sunrise calculator](https://stjarnhimlen.se/comp/sunriset.c) ([wayback](https://web.archive.org/web/20210708103125/https://stjarnhimlen.se/comp/sunriset.c)) in C#, made available under an [MIT license](./LICENSE).

I have attempted to recreate the logic of the original as faithfully as possible, while still writing sensible and good quality C#. I also made it less northern hemisphere-centric as much as possible (the original assumes the sun is always south of you).

The library is targeted to .NET Core 2.1 LTS and has no dependencies. It is intended to be as portable and interoperable with any existing C# project as possible without modification or refactoring. I currently have no plans to distribute this as a NuGet package or any other form than this repository, but if that would be helpful please don't hesitate to raise an issue.

Unit testing is done through the MSTest framework, the [`SunriseCalculatorTests`](https://github.com/jwfxpr/SunriseCalculatorTests) project is included as a submodule. Set `[fetchRecurseSubmodules](https://git-scm.com/docs/gitmodules#Documentation/gitmodules.txt-submoduleltnamegtfetchRecurseSubmodules) = false` in your `.gitmodules` to prevent the unit tests from being pulled into your code.

## Usage

For the most obvious use case, we'll get sunrise and sunset for our current place and time. This example uses [`Windows.Devices.Geolocation`](https://docs.microsoft.com/en-us/uwp/api/windows.devices.geolocation) to retrieve location information, and [`System.TimeZoneInfo`](https://docs.microsoft.com/en-us/dotnet/api/system.timezoneinfo) to get our local time zone information.

``` C#
    // Fist, get your current geolocation:
var permission = await Geolocator.RequestAccessAsync();
if (permission == GeolocationAccessStatus.Allowed)
{
    Geolocator locator = new Geolocator();
    Geoposition location = await locator.GetGeopositionAsync();
    BasicGeoposition position = location.Coordinate.Point.Position;

    // Now we can get our sunrise and sunset in our timezone
    SunriseCalc sunCalc = new SunriseCalc(position.Latitude, position.Longitude);
    sunCalc.GetRiseAndSet(out DateTime sunrise, out DateTime sunset, TimeZoneInfo.Local);

    // Done!
    Console.WriteLine($"At ({position.Latitude:0.####}°, {position.Longitude:0.####}°) on " + 
        $"{DateTime.Today:yyyy-MM-dd} sunrise is at {sunrise:HH:mm} and sunset is at {sunset:HH:mm}.");
}
```

However, we can specify any location on Earth (including extreme latitudes) and any day from 1801 to 2099. Want to populate every day of a calendar with sun rise and set times for an arbitrary location, using an arbitrary time zone? No problem!

``` C#
// Create an instance of the sunrise calculator for the desired location, for today.
SunriseCalc newYorkCity = new SunriseCalc(latitude: 40.7128, longitude: -74.0060);

// Get today's sunrise and sunset times for New York City, in UTC.
newYorkCity.GetRiseAndSet(out DateTime todaysSunriseUTC, out DateTime todaysSunsetUTC);

// Set the calculator to the next day.
newYorkCity.Day += TimeSpan.FromDays(1);
```

We can also get our results using a few different [horizon types](#horizon-types):

``` C#
// Get the astronomical dawn time. This is the offical start of twilight.
newYorkCity.GetSunrise(out DateTime astronomicalDawnUTC, Horizon.Astronomical);

// Get the length of the civil day in New York City.
TimeSpan tomorrowsCivilDayLength = newYorkCityTomorrow.GetDayLength(Horizon.Civil);

// How long will twilight last, from astronomical dawn to sunrise?
nweYorkCity.GetSunrise(out DateTime dawnUTC);
TimeSpan twilightSpan = dawnUTC - astronomicalDawnUTC;
```

The library also returns [a value](#diurnalresult-types) which indicates if the sun doesn't rise or set at all at the specified place and day. Let's find out if the sun will rise or set today in [Inuvik, Canada](https://en.wikipedia.org/wiki/Inuvik), which is about 200km north of the Arctic Circle.

``` C#
SunriseCalc inuvikToday = new SunriseCalc(68.3607, -133.7230);
DiurnalResult inuvikDayType = inuvikToday.GetSunrise(out DateTime inuvikSunset);
switch (inuvikDayType)
{
    case DiurnalResult.NormalDay:
        Console.WriteLine("Today the sun will rise and set in Inuvik.");
        break;
    case DiurnalResult.SunAlwaysAbove:
        Console.WriteLine("Today the sun will never set in Inuvik.");
        break;
    case DiurnalResult.SunAlwaysBelow:
        Console.WriteLine("Today the sun will never rise in Inuvik.");
        break;
}
```

## `Horizon` types

The library has four different horizons that can be used to calculate dawns and dusks:

* `Horizon.Normal`: Normally, sunrise/set is considered to occur when the sun's upper limb is 35 arc minutes below the horizon (this accounts for the refraction of the Earth's atmosphere).
* `Horizon.Civil`: Civil dawn or dusk marks the start or end of the brightest of the standard twilights, followed by `Nautical` and `Astronomical` twilights. It occurs when the center of the sun passes 6° below the horizon.
* `Horizon.Nautical`: Nautical dawn or dusk marks the start or end of the intermediate brightness of twilight, between `Civil` and `Astronomical` twilights. It occurs when the center of the sun passes 12° below the horizon.
* `Horizon.Astronomical`: Astronomical dawn or dusk marks the start or end of the dimmest of the standard twilights, followed by `Nautical` and `Civil` twilights. It occurs when the center of the sun passes 18° below the horizon.

## `DiurnalResult` types

The methods `GetRiseAndSet(out DateTime, out Datetime)`, `GetSunrise(out Datetime)`, and `GetSunset(out Datetime)` return a `DiurnalResult` value. For all locations certain to be outside the Arctic Circles, this result can be safely ignored; however, for locations at latitudes where the sun may never set in summer or never rise in winter, this return value is useful.

The `Horizon` selected for the calculation will affect the `DiurnalResult`; a location close to the Arctic Circle may not see the sun rise above the horizon at all (`Horizon.Normal`), but the sun may still cross the `Horizon.Nautical` on that day.

Note that the latitudes of the Arctic Circles are variable, and shouldn't be hard-coded in your code.

* `DiurnalResult.NormalDay`: This is a normal day, and the sun rises and sets above the specified `Horizon`.
* `DiurnalResult.SunAlwaysAbove`: The sun remains above the specified `Horizon` for all 24h of the specified day. The time returned is when the sun is closest to the horizon (directly to the south in the northern hemisphere; to the north in the southern); sunrise is 12 hours before the middle of the day, and sunset is 12 hours after, for a 24 hour day.
* `DiurnalResult.SunAlwaysBelow`: The sun remains below the specified `Horizon` for all 24h of the specified day. Sunrise and sunset are both calculated as when the sun is closest to the horizon (directly to the south in the northern hemisphere; to the north in the southern).

## Changelog and version history

Version 0.1
- Initial release.

Version 0.2:
- Fixed bug in `GetDayLength()` calculation.
- Added support for automatic timezone translation with a provided `TimeZoneInfo`.
- Converted all internal calculations to radians to prevent unnecessary conversions.
- Removed `Horizon.Nominal` as it just isn't good for any use case.
- Expanded and corrected documentation and examples.
- `SunriseCalculatorTests` moved to submodule to make it easier to embed the library in your project.

## Contributing

All contributions, issues, PRs, corrections, and discussions are welcome. I am new to open source distribution, and welcome all guidance and input. I have endeavoured to ensure that the code is well-documented and easy to follow for non-specialists as much as possible.

I am also not an astronomer, and my understanding or implementation of the original sunrise calculator may be partially or entirely incorrect. All mistakes are my own, and please point them out to me.
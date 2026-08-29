using System;

namespace TwelveMonthCalendar
{

internal static class CalendarStrategicDistance
{
	internal const double KilometersPerSourceMapUnit = 1.4;

	internal const double MilesPerKilometer = 0.621371192237334;

	internal const double TravelHoursPerCalendarDay = 8.0;

	internal const double WalkingMinimumMilesPerHour = 3.0;

	internal const double WalkingMaximumMilesPerHour = 4.0;

	internal const double HorseMinimumMilesPerHour = 25.0;

	internal const double HorseMaximumMilesPerHour = 30.0;

	internal const double ShipMinimumKnots = 4.0;

	internal const double ShipMaximumKnots = 5.0;

	internal const double StatuteMilesPerNauticalMile = 1.150779448;

	internal const double ShipTravelHoursPerCalendarDay = 24.0;

	internal static double CalculateStraightLineKilometers(float firstSourceX, float firstSourceY, float secondSourceX, float secondSourceY)
	{
		double num = secondSourceX - firstSourceX;
		double deltaY = secondSourceY - firstSourceY;
		return Math.Sqrt(num * num + deltaY * deltaY) * 1.4;
	}

	internal static double ToMiles(double kilometers)
	{
		return kilometers * 0.621371192237334;
	}

	internal static string FormatDualDistance(double kilometers)
	{
		return Math.Round(kilometers).ToString("N0") + " km / " + Math.Round(ToMiles(kilometers)).ToString("N0") + " mi";
	}

	internal static string FormatMappedSpan()
	{
		double widthKilometers = 2422.0;
		double heightKilometers = 2408.0;
		return "W-E: " + FormatDualDistance(widthKilometers) + "\nN-S: " + FormatDualDistance(heightKilometers);
	}

	internal static string FormatWalkingCalendarDays(double kilometers)
	{
		return FormatCalendarDayRange(kilometers, 3.0, 4.0, 8.0);
	}

	internal static string FormatHorseCalendarDays(double kilometers)
	{
		return FormatCalendarDayRange(kilometers, 25.0, 30.0, 8.0);
	}

	internal static string FormatShipCalendarDays(double kilometers)
	{
		return FormatCalendarDayRange(kilometers, 4.603117792, 5.75389724, 24.0);
	}

	private static string FormatCalendarDayRange(double kilometers, double minimumMilesPerHour, double maximumMilesPerHour, double travelHoursPerCalendarDay)
	{
		double num = ToMiles(kilometers);
		double fastestDays = num / (maximumMilesPerHour * travelHoursPerCalendarDay);
		return string.Concat(str2: (num / (minimumMilesPerHour * travelHoursPerCalendarDay)).ToString("0.0"), str0: fastestDays.ToString("0.0"), str1: "-", str3: " days");
	}
}
}

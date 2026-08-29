using System;
using System.Text.RegularExpressions;
using TaleWorlds.CampaignSystem;

namespace TwelveMonthCalendar
{

internal static class CalendarFormatter
{
	internal static string FormatMapBar(CampaignTime time)
	{
		string date = Format(time);
		if (string.IsNullOrWhiteSpace(date))
		{
			return date;
		}
		string season = CalendarSettingsState.GetSeasonName(CalendarTimeMath.GetSeason(time));
		if (!string.IsNullOrWhiteSpace(season))
		{
			return season + " " + date;
		}
		return date;
	}

	internal static string Format(CampaignTime time)
	{
		try
		{
			if (time == CampaignTime.Never || time.ToDays < 0.0)
			{
				return null;
			}
			int year = CalendarTimeMath.GetYear(time);
			int dayOfYear = CalendarTimeMath.GetDayOfYear(time);
			int month = CalendarTimeMath.GetMonth(time);
			int dayNumber = dayOfYear - CalendarTimeMath.GetMonthStart(month, CalendarTimeMath.IsLeapYear(year)) + 1;
			string dayNumberText = (CalendarSettingsState.UseOrdinalDaySuffixes ? FormatOrdinal(dayNumber) : dayNumber.ToString());
			string dayText = (CalendarSettingsState.ShowDayLabel ? $"Day {dayNumberText}" : dayNumberText);
			string yearText = (CalendarSettingsState.ShowYearLabel ? $"Year {year}" : year.ToString());
			return Regex.Replace(Regex.Replace(CalendarSettingsState.DateFormat, "\\{Season\\}", string.Empty, RegexOptions.IgnoreCase), "\\s{2,}", " ").Trim().Replace("{Month}", CalendarSettingsState.GetMonthName(month))
				.Replace("{Day}", dayText)
				.Replace("{Year}", yearText)
				.Replace("{MonthNumber}", (month + 1).ToString())
				.Replace("{DayOfYear}", (dayOfYear + 1).ToString());
		}
		catch (Exception exception)
		{
			Diagnostics.Error("Calendar date formatting failed.", exception);
			return null;
		}
	}

	internal static string FormatMapDateLine(CampaignTime time)
	{
		try
		{
			if (time == CampaignTime.Never || time.ToDays < 0.0)
			{
				return null;
			}
			int year = CalendarTimeMath.GetYear(time);
			int dayOfYear = CalendarTimeMath.GetDayOfYear(time);
			int month = CalendarTimeMath.GetMonth(time);
			int dayNumber = dayOfYear - CalendarTimeMath.GetMonthStart(month, CalendarTimeMath.IsLeapYear(year)) + 1;
			string dayText = (CalendarSettingsState.UseOrdinalDaySuffixes ? FormatOrdinal(dayNumber) : dayNumber.ToString());
			if (CalendarSettingsState.ShowDayLabel)
			{
				dayText = "Day " + dayText;
			}
			string monthText = CalendarSettingsState.GetMonthName(month);
			string yearText = (CalendarSettingsState.ShowYearLabel ? ("Year " + year) : year.ToString());
			string format = CalendarSettingsState.DateFormat;
			if (string.Equals(format, "{Day} {Month} {Year}", StringComparison.Ordinal))
			{
				return dayText + ", " + monthText;
			}
			if (string.Equals(format, "{Year} {Month} {Day}", StringComparison.Ordinal))
			{
				return yearText + ", " + monthText;
			}
			if (string.Equals(format, "{Month} {Day} {Year}", StringComparison.Ordinal))
			{
				return monthText + ", " + dayText;
			}
			return Format(time);
		}
		catch (Exception exception)
		{
			Diagnostics.Error("Map-bar calendar date-line formatting failed.", exception);
			return null;
		}
	}

	internal static string FormatMapSeasonYearLine(CampaignTime time)
	{
		try
		{
			if (time == CampaignTime.Never || time.ToDays < 0.0)
			{
				return null;
			}
			string season = CalendarSettingsState.GetSeasonName(CalendarTimeMath.GetSeason(time));
			int year = CalendarTimeMath.GetYear(time);
			string yearText = (CalendarSettingsState.ShowYearLabel ? ("Year " + year) : year.ToString());
			if (string.Equals(CalendarSettingsState.DateFormat, "{Year} {Month} {Day}", StringComparison.Ordinal))
			{
				int dayOfYear = CalendarTimeMath.GetDayOfYear(time);
				int month = CalendarTimeMath.GetMonth(time);
				int dayNumber = dayOfYear - CalendarTimeMath.GetMonthStart(month, CalendarTimeMath.IsLeapYear(year)) + 1;
				string dayText = (CalendarSettingsState.UseOrdinalDaySuffixes ? FormatOrdinal(dayNumber) : dayNumber.ToString());
				if (CalendarSettingsState.ShowDayLabel)
				{
					dayText = "Day " + dayText;
				}
				return string.IsNullOrWhiteSpace(season) ? dayText : (dayText + ", " + season);
			}
			return string.IsNullOrWhiteSpace(season) ? yearText : (yearText + ", " + season);
		}
		catch (Exception exception)
		{
			Diagnostics.Error("Map-bar season-year formatting failed.", exception);
			return null;
		}
	}

	private static string FormatOrdinal(int value)
	{
		int lastTwoDigits = value % 100;
		if (lastTwoDigits >= 11 && lastTwoDigits <= 13)
		{
			return value + "th";
		}
		return (value % 10) switch
		{
			1 => value + "st",
			2 => value + "nd",
			3 => value + "rd",
			_ => value + "th",
		};
	}
}
}

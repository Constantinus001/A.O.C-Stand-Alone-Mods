using System;
using TaleWorlds.CampaignSystem;

namespace TwelveMonthCalendar
{

internal static class CalendarTimeMath
{
	internal const int SeasonsInYear = 4;

	internal static int DaysInYear => CalendarSettingsState.CommonDaysInYear;

	internal static int LeapYearDays => DaysInYear + 1;

	internal static int NativeDaysInYear => CalendarSettingsState.NativeDaysInYear;

	internal static int CalendarPeriodsInYear => 4;

	internal static float PregnancyDurationInDays => CalendarSettingsState.PregnancyDurationInDays;

	internal static double AverageDaysInYear => NativeDaysInYear;

	internal static double NativeCampaignStartDay => 1084.0 * (double)NativeDaysInYear + (double)(NativeDaysInYear / 4);

	internal static double GregorianCampaignStartDay => NativeCampaignStartDay;

	internal static double LegacyCalendarDayOffset => 0.0;

	internal static double DaysPerSeason => AverageDaysInYear / 4.0;

	internal static float CampaignTimeMultiplier => CalendarSettingsState.CampaignTimeScale;

	internal static CampaignTime GetPregnancyDueDate(CampaignTime conceptionTime)
	{
		if (CalendarSettingsState.UseCalendarMonthPregnancy)
		{
			return AddCalendarMonths(conceptionTime, CalendarSettingsState.PregnancyDurationMonths);
		}
		return conceptionTime + CampaignTime.Days(PregnancyDurationInDays);
	}

	internal static bool LooksLikeNativeTimeBasis(CampaignTime time)
	{
		double rawDay = time.ToDays;
		if (double.IsNaN(rawDay) || double.IsInfinity(rawDay))
		{
			return false;
		}
		return Math.Abs(rawDay - NativeCampaignStartDay) < Math.Abs(rawDay - GregorianCampaignStartDay);
	}

	internal static double ToCalendarAbsoluteDays(CampaignTime time)
	{
		return ToCalendarAbsoluteDays(time.ToDays);
	}

	internal static double ToCalendarAbsoluteDays(double rawDay)
	{
		double nearestDay = Math.Round(rawDay);
		if (!(Math.Abs(rawDay - nearestDay) <= 1.0 / 256.0))
		{
			return rawDay;
		}
		return nearestDay;
	}

	internal static CampaignTime FromCalendarAbsoluteDays(double calendarDay)
	{
		return CampaignTime.Days((float)calendarDay);
	}

	internal static CampaignTime GetCampaignStartTime()
	{
		return CampaignTime.Days((float)(CalendarSettingsState.IsLegacySaveAgeCompatibility ? NativeCampaignStartDay : GregorianCampaignStartDay)) + CampaignTime.Hours(9f);
	}

	internal static int GetYearLength(int year)
	{
		return DaysInYear + (IsLeapYear(year) ? 1 : 0);
	}

	internal static bool IsFastForwardMode(CampaignTimeControlMode mode)
	{
		if (mode == CampaignTimeControlMode.UnstoppableFastForward || (uint)(mode - 4) <= 1u)
		{
			return true;
		}
		return false;
	}

	internal static bool IsLeapYear(int year)
	{
		return false;
	}

	internal static long DaysBeforeYear(int year)
	{
		return (long)year * (long)NativeDaysInYear;
	}

	internal static int GetYear(CampaignTime time)
	{
		double absoluteDays = ToCalendarAbsoluteDays(time);
		int year = (int)Math.Floor(absoluteDays / AverageDaysInYear);
		while (absoluteDays < (double)DaysBeforeYear(year))
		{
			year--;
		}
		for (; absoluteDays >= (double)DaysBeforeYear(year + 1); year++)
		{
		}
		return year;
	}

	internal static int GetDayOfYear(CampaignTime time)
	{
		int year = GetYear(time);
		return (int)((long)Math.Floor(ToCalendarAbsoluteDays(time)) - DaysBeforeYear(year));
	}

	internal static int GetMonth(CampaignTime time)
	{
		int year = GetYear(time);
		int dayOfYear = GetDayOfYear(time);
		bool leap = IsLeapYear(year);
		for (int month = CalendarPeriodsInYear - 1; month >= 0; month--)
		{
			if (dayOfYear >= GetMonthStart(month, leap))
			{
				return month;
			}
		}
		return 0;
	}

	internal static int GetMonthStart(int month, bool leapYear)
	{
		return month * 21;
	}

	internal static int GetMonthLength(int month, bool leapYear)
	{
		return 21;
	}

	internal static int GetSeasonLength(int year, int season)
	{
		return 21;
	}

	internal static CampaignTime AddCalendarMonths(CampaignTime time, int months)
	{
		int sourceYear = GetYear(time);
		int sourceMonth = GetMonth(time);
		int val = GetDayOfYear(time) - GetMonthStart(sourceMonth, IsLeapYear(sourceYear)) + 1;
		int num = sourceYear * CalendarPeriodsInYear + sourceMonth + months;
		int targetYear = num / CalendarPeriodsInYear;
		int targetMonth = num % CalendarPeriodsInYear;
		if (targetMonth < 0)
		{
			targetMonth += CalendarPeriodsInYear;
			targetYear--;
		}
		bool targetLeapYear = IsLeapYear(targetYear);
		int targetDay = Math.Min(val, GetMonthLength(targetMonth, targetLeapYear));
		long num2 = DaysBeforeYear(targetYear) + GetMonthStart(targetMonth, targetLeapYear) + targetDay - 1;
		double num3 = ToCalendarAbsoluteDays(time);
		double fractionalDay = num3 - Math.Floor(num3);
		return FromCalendarAbsoluteDays((double)num2 + fractionalDay);
	}

	internal static int GetSeason(CampaignTime time)
	{
		return GetDayOfYear(time) / 21;
	}

	internal static int GetDayOfSeason(CampaignTime time)
	{
		int year = GetYear(time);
		int dayOfYear = GetDayOfYear(time);
		int season = GetSeason(time);
		int seasonStart = GetSeasonStartDayOfYear(year, season);
		if (season == 3 && dayOfYear < seasonStart)
		{
			int previousYear = year - 1;
			return dayOfYear + GetYearLength(previousYear) - GetSeasonStartDayOfYear(previousYear, season);
		}
		return dayOfYear - seasonStart;
	}

	internal static float ElapsedYearsUntilNow(CampaignTime time)
	{
		if (CalendarSettingsState.IsLegacySaveAgeCompatibility)
		{
			return GetLegacyCompatibleElapsedYearsAt(time, CampaignTime.Now);
		}
		return (float)(ToYears(CampaignTime.Now) - ToYears(time));
	}

	internal static float GetLegacyCompatibleHeroAgeAt(CampaignTime birthDay, CampaignTime referenceTime)
	{
		return GetLegacyCompatibleElapsedYearsAt(birthDay, referenceTime);
	}

	internal static float GetLegacyCompatibleElapsedYearsAt(CampaignTime startTime, CampaignTime referenceTime)
	{
		double nowDay = referenceTime.ToDays;
		double cutoverDay = CalendarSettingsState.LegacySaveAgeCutoverDay;
		double startDay = startTime.ToDays;
		if (double.IsNaN(nowDay) || double.IsInfinity(nowDay) || double.IsNaN(cutoverDay) || double.IsInfinity(cutoverDay) || double.IsNaN(startDay) || double.IsInfinity(startDay))
		{
			return 0f;
		}
		double ageInYears = ((nowDay <= cutoverDay) ? ((nowDay - startDay) / (double)NativeDaysInYear) : ((!(startDay <= cutoverDay)) ? ((nowDay - startDay) / AverageDaysInYear) : ((cutoverDay - startDay) / (double)NativeDaysInYear + (nowDay - cutoverDay) / AverageDaysInYear)));
		if (double.IsNaN(ageInYears) || double.IsInfinity(ageInYears))
		{
			return 0f;
		}
		return (float)Math.Max(0.0, ageInYears);
	}

	internal static float RemainingYearsFromNow(CampaignTime time)
	{
		return (float)(ToYears(time) - ToYears(CampaignTime.Now));
	}

	internal static double ToYears(CampaignTime time)
	{
		int year = GetYear(time);
		double dayWithinYear = ToCalendarAbsoluteDays(time) - (double)DaysBeforeYear(year);
		return (double)year + dayWithinYear / (double)GetYearLength(year);
	}

	internal static double DurationToYears(CampaignTime duration)
	{
		return duration.ToDays / AverageDaysInYear;
	}

	internal static float ElapsedSeasonsUntilNow(CampaignTime time)
	{
		return (float)(ToSeasons(CampaignTime.Now) - ToSeasons(time));
	}

	internal static float RemainingSeasonsFromNow(CampaignTime time)
	{
		return (float)(ToSeasons(time) - ToSeasons(CampaignTime.Now));
	}

	internal static double ToSeasons(CampaignTime time)
	{
		int year = GetYear(time);
		int season = GetSeason(time);
		int seasonYear = GetSeasonYear(time, year, season);
		int seasonStart = GetSeasonStartDayOfYear(seasonYear, season);
		double dayWithinSeason = ToCalendarAbsoluteDays(time) - (double)(DaysBeforeYear(seasonYear) + seasonStart);
		return (double)(seasonYear * 4 + season) + dayWithinSeason / (double)GetSeasonLength(seasonYear, season);
	}

	internal static double DurationToSeasons(CampaignTime duration)
	{
		return duration.ToDays / DaysPerSeason;
	}

	internal static int GetSeasonYear(CampaignTime time)
	{
		int year = GetYear(time);
		return GetSeasonYear(time, year, GetSeason(time));
	}

	internal static int GetSeasonStartDayOfYear(int year, int season)
	{
		if (season < 0 || season >= 4)
		{
			throw new ArgumentOutOfRangeException("season");
		}
		return season * 21;
	}

	private static int GetSeasonYear(CampaignTime time, int year, int season)
	{
		return year;
	}
}
}

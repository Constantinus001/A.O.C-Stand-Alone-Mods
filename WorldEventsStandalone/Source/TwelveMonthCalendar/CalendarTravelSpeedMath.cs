using System;

namespace TwelveMonthCalendar
{

internal static class CalendarTravelSpeedMath
{
	internal const double CampaignHoursPerCalendarDay = 24.0;

	internal const double SourceMapUnitsPerCampaignMapUnit = 2.285;

	internal const double FootAverageMilesPerHour = 3.5;

	internal const double HorseMaximumMilesPerHour = 30.0;

	internal const float GameplayPacingMultiplier = 1.5f;

	internal const float MinimumGameSpeed = 0.1f;

	internal static float FootBaselineGameSpeed => ConvertMilesPerHourToGameSpeed(3.5);

	internal static float HorseMaximumGameSpeed => ConvertMilesPerHourToGameSpeed(30.0);

	internal static float ConvertMilesPerHourToGameSpeed(double milesPerHour)
	{
		double milesPerCampaignMapUnit = 1.9877664439672311;
		return (float)(milesPerHour * 8.0 / (24.0 * milesPerCampaignMapUnit));
	}

	internal static float CalculateEffectiveMountedRatio(int mountedTroops, int footTroops, int spareRidingHorses)
	{
		int safeMountedTroops = Math.Max(0, mountedTroops);
		int safeFootTroops = Math.Max(0, footTroops);
		int totalTroops = safeMountedTroops + safeFootTroops;
		if (totalTroops <= 0)
		{
			return 0f;
		}
		int mountedFootTroops = Math.Min(safeFootTroops, Math.Max(0, spareRidingHorses));
		return Math.Max(0f, Math.Min(1f, (float)(safeMountedTroops + mountedFootTroops) / (float)totalTroops));
	}

	internal static float CalculateCalibratedLandSpeed(float nativeFinalSpeed, float nativeBaseSpeed, float effectiveMountedRatio)
	{
		float mountedRatio = Math.Max(0f, Math.Min(1f, effectiveMountedRatio));
		float num = FootBaselineGameSpeed + (HorseMaximumGameSpeed - FootBaselineGameSpeed) * mountedRatio;
		float nativeModifier = ((nativeBaseSpeed > 0.001f) ? Math.Max(0f, nativeFinalSpeed / nativeBaseSpeed) : 1f);
		float calibratedSpeed = num * nativeModifier * 1.5f;
		return Math.Max(0.1f, Math.Min(HorseMaximumGameSpeed, calibratedSpeed));
	}
}
}

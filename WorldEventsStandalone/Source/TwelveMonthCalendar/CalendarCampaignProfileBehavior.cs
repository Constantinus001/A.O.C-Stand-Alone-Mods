using System.Globalization;
using TaleWorlds.CampaignSystem;

namespace TwelveMonthCalendar
{

internal sealed class CalendarCampaignProfileBehavior : CampaignBehaviorBase
{
	private const string SerializedProfileKey = "AgesOfCalradia.CampaignProfileV3";

	private const string LegacySerializedProfileKey = "TwelveMonthCalendar.CampaignProfileV2";

	private const string LegacyAgeCompatibilityKey = "AgesOfCalradia.LegacyNativeAgeCompatibilityV1";

	private const string LegacyAgeCutoverKey = "AgesOfCalradia.LegacyNativeAgeCutoverDayV1";

	private string _serializedCampaignProfile = string.Empty;

	private bool _legacyAgeCompatibility;

	private string _legacyAgeCutoverDay = string.Empty;

	public override void RegisterEvents()
	{
	}

	public override void SyncData(IDataStore dataStore)
	{
		if (dataStore.IsLoading)
		{
			bool legacyAgeCompatibilityWasPresent = dataStore.SyncData("AgesOfCalradia.LegacyNativeAgeCompatibilityV1", ref _legacyAgeCompatibility);
			dataStore.SyncData("AgesOfCalradia.LegacyNativeAgeCutoverDayV1", ref _legacyAgeCutoverDay);
			RestoreLoadedProfile(dataStore, legacyAgeCompatibilityWasPresent);
			return;
		}
		_legacyAgeCompatibility = CalendarSettingsState.IsLegacySaveAgeCompatibility;
		_legacyAgeCutoverDay = GetLegacyAgeCutoverPayload();
		dataStore.SyncData("AgesOfCalradia.LegacyNativeAgeCompatibilityV1", ref _legacyAgeCompatibility);
		dataStore.SyncData("AgesOfCalradia.LegacyNativeAgeCutoverDayV1", ref _legacyAgeCutoverDay);
		CalendarCampaignProfile profile = CalendarCampaignProfile.Capture();
		_serializedCampaignProfile = profile.Serialize();
		dataStore.SyncData("AgesOfCalradia.CampaignProfileV3", ref _serializedCampaignProfile);
		CrashFlightRecorder.Record("CampaignProfile", "Saved soft profile fingerprint=" + profile.Fingerprint + "; primitive payload only; no module-load marker written.");
	}

	private void RestoreLoadedProfile(IDataStore dataStore, bool legacyAgeCompatibilityWasPresent)
	{
		bool serializedProfileWasPresent = dataStore.SyncData("AgesOfCalradia.CampaignProfileV3", ref _serializedCampaignProfile);
		if (!serializedProfileWasPresent)
		{
			serializedProfileWasPresent = dataStore.SyncData("TwelveMonthCalendar.CampaignProfileV2", ref _serializedCampaignProfile);
		}
		string failure = null;
		string source;
		if (serializedProfileWasPresent && CalendarCampaignProfile.TryDeserialize(_serializedCampaignProfile, out var profile, out failure))
		{
			source = "soft saved profile";
		}
		else
		{
			profile = CalendarCampaignProfile.Capture();
			source = "active local settings captured for a legacy/no-profile save";
			if (serializedProfileWasPresent)
			{
				Diagnostics.Info("Saved calendar profile was not applied because " + failure + " Current local settings remain active; the next save will replace it with a valid soft profile.");
				CrashFlightRecorder.Record("CampaignProfile", "Soft profile validation failed: " + failure);
			}
		}
		bool legacyAgeCompatibility = (legacyAgeCompatibilityWasPresent ? _legacyAgeCompatibility : (profile.LegacyNativeAgeBasis || CalendarTimeMath.LooksLikeNativeTimeBasis(CampaignTime.Now)));
		if (legacyAgeCompatibility)
		{
			if (!TryParseCutoverDay(_legacyAgeCutoverDay, out var cutoverDay))
			{
				cutoverDay = CampaignTime.Now.ToDays;
				_legacyAgeCutoverDay = cutoverDay.ToString("R", CultureInfo.InvariantCulture);
			}
			_legacyAgeCompatibility = true;
			CalendarSettingsState.MarkLegacySaveAgeCompatibility(cutoverDay);
		}
		else
		{
			_legacyAgeCompatibility = false;
			_legacyAgeCutoverDay = string.Empty;
			CalendarSettingsState.MarkModernSaveAgeCompatibility();
		}
		string differences = profile.DescribeDifferencesFromCurrentSettings();
		CalendarSettingsState.ApplyPersistedCampaignProfile(profile);
		CalendarSettingsState.Save();
		Diagnostics.Info("Calendar " + source + "." + (legacyAgeCompatibility ? (" Legacy-save hero-age compatibility is active from campaign day " + _legacyAgeCutoverDay + ".") : " Modern-save hero-age basis is active.") + (string.IsNullOrWhiteSpace(differences) ? string.Empty : (" Differences=" + differences + ".")));
		CrashFlightRecorder.Record("CampaignProfile", "Restored " + source + "; fingerprint=" + profile.Fingerprint + "; LegacyAgeCompatibility=" + legacyAgeCompatibility + (string.IsNullOrWhiteSpace(differences) ? string.Empty : ("; Differences=" + differences)));
	}

	private static bool TryParseCutoverDay(string value, out double cutoverDay)
	{
		if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out cutoverDay) && !double.IsNaN(cutoverDay))
		{
			return !double.IsInfinity(cutoverDay);
		}
		return false;
	}

	private static string GetLegacyAgeCutoverPayload()
	{
		if (!CalendarSettingsState.IsLegacySaveAgeCompatibility)
		{
			return string.Empty;
		}
		double cutoverDay = CalendarSettingsState.LegacySaveAgeCutoverDay;
		if (double.IsNaN(cutoverDay) || double.IsInfinity(cutoverDay))
		{
			return string.Empty;
		}
		return cutoverDay.ToString("R", CultureInfo.InvariantCulture);
	}
}
}

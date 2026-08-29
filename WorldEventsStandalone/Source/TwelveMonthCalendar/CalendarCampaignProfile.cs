using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace TwelveMonthCalendar
{

public sealed class CalendarCampaignProfile
{
	public const int CurrentSchemaVersion = 5;

	private const string SerializedRootName = "CalendarCampaignProfile";

	private const int MaximumSerializedLength = 32768;

	public int SchemaVersion = 5;

	public string CalendarSystem = "Gregorian12Month";

	public bool UseLeapYears;

	public string MonthLengthSignature = string.Empty;

	public bool AutoCampaignTimeScale;

	public float CampaignTimeScale;

	public float NormalPlayTimeMultiplier;

	public float FastForwardTimeMultiplier;

	public bool UseCalendarMonthPregnancy;

	public int PregnancyDurationMonths;

	public float PregnancyDurationInDays;

	public float RenownGainMultiplier;

	public bool BalancePartyImpairment;

	public bool BalancePrisonerRecruitment;

	public bool BalanceNpcMarriage;

	public bool BalanceMapTracks;

	public bool BalanceQuestDeadlines;

	public bool AnnualBalanceEnabled = true;

	public string Fingerprint = string.Empty;

	public float LordDeathRateMultiplier = 0.2f;

	public bool LegacyNativeAgeBasis;

	public static CalendarCampaignProfile Capture()
	{
		CalendarCampaignProfile calendarCampaignProfile = new CalendarCampaignProfile();
		calendarCampaignProfile.CalendarSystem = CalendarSettingsState.CalendarSystem;
		calendarCampaignProfile.UseLeapYears = CalendarSettingsState.UseLeapYears;
		calendarCampaignProfile.MonthLengthSignature = BuildMonthLengthSignature(CalendarSettingsState.MonthLengthsSnapshot());
		calendarCampaignProfile.AutoCampaignTimeScale = CalendarSettingsState.AutoCampaignTimeScale;
		calendarCampaignProfile.CampaignTimeScale = CalendarSettingsState.CampaignTimeScale;
		calendarCampaignProfile.NormalPlayTimeMultiplier = CalendarSettingsState.NormalPlayTimeMultiplier;
		calendarCampaignProfile.FastForwardTimeMultiplier = CalendarSettingsState.FastForwardTimeMultiplier;
		calendarCampaignProfile.UseCalendarMonthPregnancy = CalendarSettingsState.UseCalendarMonthPregnancy;
		calendarCampaignProfile.PregnancyDurationMonths = CalendarSettingsState.PregnancyDurationMonths;
		calendarCampaignProfile.PregnancyDurationInDays = CalendarSettingsState.PregnancyDurationInDays;
		calendarCampaignProfile.RenownGainMultiplier = CalendarSettingsState.RenownGainMultiplier;
		calendarCampaignProfile.LordDeathRateMultiplier = CalendarSettingsState.LordDeathRateMultiplier;
		calendarCampaignProfile.BalancePartyImpairment = CalendarSettingsState.BalancePartyImpairment;
		calendarCampaignProfile.BalancePrisonerRecruitment = CalendarSettingsState.BalancePrisonerRecruitment;
		calendarCampaignProfile.BalanceNpcMarriage = CalendarSettingsState.BalanceNpcMarriage;
		calendarCampaignProfile.BalanceMapTracks = CalendarSettingsState.BalanceMapTracks;
		calendarCampaignProfile.BalanceQuestDeadlines = CalendarSettingsState.BalanceQuestDeadlines;
		calendarCampaignProfile.AnnualBalanceEnabled = CalendarSettingsState.AnnualBalanceEnabled;
		calendarCampaignProfile.LegacyNativeAgeBasis = false;
		calendarCampaignProfile.RefreshFingerprint();
		return calendarCampaignProfile;
	}

	public bool TryUpgradeLegacyProfile()
	{
		if (SchemaVersion == 5)
		{
			return true;
		}
		if (SchemaVersion != 1 && SchemaVersion != 2 && SchemaVersion != 3 && SchemaVersion != 4)
		{
			return false;
		}
		int schemaVersion = SchemaVersion;
		NormalPlayTimeMultiplier = 1f;
		if (schemaVersion <= 2)
		{
			FastForwardTimeMultiplier = CalendarSettingsState.ConvertLegacyFastForwardPacingToSpeed(FastForwardTimeMultiplier);
		}
		if (schemaVersion == 3)
		{
			FastForwardTimeMultiplier = Math.Min(4f, Math.Max(1f, FastForwardTimeMultiplier));
		}
		AnnualBalanceEnabled = true;
		SchemaVersion = 5;
		if (schemaVersion == 1)
		{
			LordDeathRateMultiplier = 0.2f;
		}
		RefreshFingerprint();
		return true;
	}

	public bool TryValidate(out string failure)
	{
		if (SchemaVersion != 5)
		{
			failure = "unsupported profile schema " + SchemaVersion + ".";
			return false;
		}
		if (!string.Equals(CalendarSystem, CalendarSettingsState.CalendarSystem, StringComparison.OrdinalIgnoreCase))
		{
			failure = "calendar system '" + CalendarSystem + "' is not supported.";
			return false;
		}
		if (!TryGetMonthLengths(out var _))
		{
			failure = "month lengths are invalid.";
			return false;
		}
		if (!IsFinite(CampaignTimeScale) || CampaignTimeScale < 0.01f || CampaignTimeScale > 1f || !IsFinite(NormalPlayTimeMultiplier) || !NearlyEqual(NormalPlayTimeMultiplier, 1f) || !IsFinite(FastForwardTimeMultiplier) || FastForwardTimeMultiplier < 1f || FastForwardTimeMultiplier > 4f || PregnancyDurationMonths < 1 || !IsFinite(PregnancyDurationInDays) || PregnancyDurationInDays < 0.1f || !IsFinite(RenownGainMultiplier) || RenownGainMultiplier < 0f || RenownGainMultiplier > 1f || !IsFinite(LordDeathRateMultiplier) || LordDeathRateMultiplier < 0f || LordDeathRateMultiplier > 1f)
		{
			failure = "one or more numeric values are outside their safe range.";
			return false;
		}
		string expectedFingerprint = BuildFingerprint();
		if (!string.IsNullOrWhiteSpace(Fingerprint) && !string.Equals(Fingerprint, expectedFingerprint, StringComparison.Ordinal))
		{
			failure = "the profile fingerprint does not match its saved values.";
			return false;
		}
		failure = null;
		return true;
	}

	public bool TryGetMonthLengths(out int[] monthLengths)
	{
		monthLengths = null;
		if (string.IsNullOrWhiteSpace(MonthLengthSignature))
		{
			return false;
		}
		string[] parts = MonthLengthSignature.Split(',');
		if (parts.Length != 12)
		{
			return false;
		}
		int[] parsed = new int[parts.Length];
		int total = 0;
		for (int index = 0; index < parts.Length; index++)
		{
			if (!int.TryParse(parts[index], NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) || value < 1 || value > 1000)
			{
				return false;
			}
			parsed[index] = value;
			total += value;
		}
		if (total != 365)
		{
			return false;
		}
		monthLengths = parsed;
		return true;
	}

	public string DescribeDifferencesFromCurrentSettings()
	{
		List<string> differences = new List<string>();
		if (UseLeapYears != CalendarSettingsState.UseLeapYears)
		{
			differences.Add("UseLeapYears");
		}
		if (!string.Equals(MonthLengthSignature, BuildMonthLengthSignature(CalendarSettingsState.MonthLengthsSnapshot()), StringComparison.Ordinal))
		{
			differences.Add("MonthLengths");
		}
		if (AutoCampaignTimeScale != CalendarSettingsState.AutoCampaignTimeScale)
		{
			differences.Add("AutoCampaignTimeScale");
		}
		if (!NearlyEqual(CampaignTimeScale, CalendarSettingsState.CampaignTimeScale))
		{
			differences.Add("CampaignTimeScale");
		}
		if (!NearlyEqual(NormalPlayTimeMultiplier, CalendarSettingsState.NormalPlayTimeMultiplier))
		{
			differences.Add("NormalPlayTimeMultiplier");
		}
		if (!NearlyEqual(FastForwardTimeMultiplier, CalendarSettingsState.FastForwardTimeMultiplier))
		{
			differences.Add("FastForwardTimeMultiplier");
		}
		if (UseCalendarMonthPregnancy != CalendarSettingsState.UseCalendarMonthPregnancy)
		{
			differences.Add("UseCalendarMonthPregnancy");
		}
		if (PregnancyDurationMonths != CalendarSettingsState.PregnancyDurationMonths)
		{
			differences.Add("PregnancyDurationMonths");
		}
		if (!NearlyEqual(PregnancyDurationInDays, CalendarSettingsState.PregnancyDurationInDays))
		{
			differences.Add("PregnancyDurationInDays");
		}
		if (!NearlyEqual(RenownGainMultiplier, CalendarSettingsState.RenownGainMultiplier))
		{
			differences.Add("RenownGainMultiplier");
		}
		if (!NearlyEqual(LordDeathRateMultiplier, CalendarSettingsState.LordDeathRateMultiplier))
		{
			differences.Add("LordDeathRateMultiplier");
		}
		if (BalancePartyImpairment != CalendarSettingsState.BalancePartyImpairment)
		{
			differences.Add("BalancePartyImpairment");
		}
		if (BalancePrisonerRecruitment != CalendarSettingsState.BalancePrisonerRecruitment)
		{
			differences.Add("BalancePrisonerRecruitment");
		}
		if (BalanceNpcMarriage != CalendarSettingsState.BalanceNpcMarriage)
		{
			differences.Add("BalanceNpcMarriage");
		}
		if (BalanceMapTracks != CalendarSettingsState.BalanceMapTracks)
		{
			differences.Add("BalanceMapTracks");
		}
		if (BalanceQuestDeadlines != CalendarSettingsState.BalanceQuestDeadlines)
		{
			differences.Add("BalanceQuestDeadlines");
		}
		if (AnnualBalanceEnabled != CalendarSettingsState.AnnualBalanceEnabled)
		{
			differences.Add("AnnualBalanceEnabled");
		}
		if (differences.Count != 0)
		{
			return string.Join(",", differences);
		}
		return string.Empty;
	}

	public void RefreshFingerprint()
	{
		Fingerprint = BuildFingerprint();
	}

	public string Serialize()
	{
		RefreshFingerprint();
		XmlDocument obj = new XmlDocument
		{
			XmlResolver = null
		};
		XmlElement root = obj.CreateElement("CalendarCampaignProfile");
		obj.AppendChild(root);
		root.SetAttribute("SchemaVersion", SchemaVersion.ToString(CultureInfo.InvariantCulture));
		root.SetAttribute("CalendarSystem", CalendarSystem ?? string.Empty);
		root.SetAttribute("UseLeapYears", UseLeapYears.ToString());
		root.SetAttribute("MonthLengthSignature", MonthLengthSignature ?? string.Empty);
		root.SetAttribute("AutoCampaignTimeScale", AutoCampaignTimeScale.ToString());
		root.SetAttribute("CampaignTimeScale", CampaignTimeScale.ToString("R", CultureInfo.InvariantCulture));
		root.SetAttribute("NormalPlayTimeMultiplier", NormalPlayTimeMultiplier.ToString("R", CultureInfo.InvariantCulture));
		root.SetAttribute("FastForwardSpeedMultiplier", FastForwardTimeMultiplier.ToString("R", CultureInfo.InvariantCulture));
		root.SetAttribute("UseCalendarMonthPregnancy", UseCalendarMonthPregnancy.ToString());
		root.SetAttribute("PregnancyDurationMonths", PregnancyDurationMonths.ToString(CultureInfo.InvariantCulture));
		root.SetAttribute("PregnancyDurationInDays", PregnancyDurationInDays.ToString("R", CultureInfo.InvariantCulture));
		root.SetAttribute("RenownGainMultiplier", RenownGainMultiplier.ToString("R", CultureInfo.InvariantCulture));
		root.SetAttribute("LordDeathRateMultiplier", LordDeathRateMultiplier.ToString("R", CultureInfo.InvariantCulture));
		root.SetAttribute("BalancePartyImpairment", BalancePartyImpairment.ToString());
		root.SetAttribute("BalancePrisonerRecruitment", BalancePrisonerRecruitment.ToString());
		root.SetAttribute("BalanceNpcMarriage", BalanceNpcMarriage.ToString());
		root.SetAttribute("BalanceMapTracks", BalanceMapTracks.ToString());
		root.SetAttribute("BalanceQuestDeadlines", BalanceQuestDeadlines.ToString());
		root.SetAttribute("AnnualBalanceEnabled", AnnualBalanceEnabled.ToString());
		root.SetAttribute("LegacyNativeAgeBasis", LegacyNativeAgeBasis.ToString());
		root.SetAttribute("Fingerprint", Fingerprint ?? string.Empty);
		return obj.OuterXml;
	}

	public static bool TryDeserialize(string serializedProfile, out CalendarCampaignProfile profile, out string failure)
	{
		profile = null;
		if (string.IsNullOrWhiteSpace(serializedProfile) || serializedProfile.Length > 32768)
		{
			failure = "the serialized profile is empty or too large.";
			return false;
		}
		try
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.XmlResolver = null;
			xmlDocument.LoadXml(serializedProfile);
			XmlElement root = xmlDocument.DocumentElement;
			if (root == null || root.Name != "CalendarCampaignProfile")
			{
				failure = "the serialized profile root is invalid.";
				return false;
			}
			CalendarCampaignProfile candidate = new CalendarCampaignProfile();
			if (!TryReadInt(root, "SchemaVersion", out candidate.SchemaVersion) || !TryReadString(root, "CalendarSystem", out candidate.CalendarSystem) || !TryReadBoolean(root, "UseLeapYears", out candidate.UseLeapYears) || !TryReadString(root, "MonthLengthSignature", out candidate.MonthLengthSignature) || !TryReadBoolean(root, "AutoCampaignTimeScale", out candidate.AutoCampaignTimeScale) || !TryReadFloat(root, "CampaignTimeScale", out candidate.CampaignTimeScale) || !TryReadFloat(root, "NormalPlayTimeMultiplier", out candidate.NormalPlayTimeMultiplier) || !TryReadBoolean(root, "UseCalendarMonthPregnancy", out candidate.UseCalendarMonthPregnancy) || !TryReadInt(root, "PregnancyDurationMonths", out candidate.PregnancyDurationMonths) || !TryReadFloat(root, "PregnancyDurationInDays", out candidate.PregnancyDurationInDays) || !TryReadFloat(root, "RenownGainMultiplier", out candidate.RenownGainMultiplier) || !TryReadBoolean(root, "BalancePartyImpairment", out candidate.BalancePartyImpairment) || !TryReadBoolean(root, "BalancePrisonerRecruitment", out candidate.BalancePrisonerRecruitment) || !TryReadBoolean(root, "BalanceNpcMarriage", out candidate.BalanceNpcMarriage) || !TryReadBoolean(root, "BalanceMapTracks", out candidate.BalanceMapTracks) || !TryReadBoolean(root, "BalanceQuestDeadlines", out candidate.BalanceQuestDeadlines) || !TryReadString(root, "Fingerprint", out candidate.Fingerprint))
			{
				failure = "the serialized profile has missing or invalid values.";
				return false;
			}
			if (candidate.SchemaVersion >= 4 && !TryReadBoolean(root, "AnnualBalanceEnabled", out candidate.AnnualBalanceEnabled))
			{
				failure = "the serialized profile has a missing or invalid annual-balance setting.";
				return false;
			}
			if (candidate.SchemaVersion >= 5 && !TryReadBoolean(root, "LegacyNativeAgeBasis", out candidate.LegacyNativeAgeBasis))
			{
				failure = "the serialized profile has a missing or invalid age-compatibility setting.";
				return false;
			}
			string fastForwardAttribute = ((candidate.SchemaVersion >= 3) ? "FastForwardSpeedMultiplier" : "FastForwardTimeMultiplier");
			if (!TryReadFloat(root, fastForwardAttribute, out candidate.FastForwardTimeMultiplier))
			{
				failure = "the serialized profile has a missing or invalid fast-forward speed.";
				return false;
			}
			if (candidate.SchemaVersion == 1)
			{
				candidate.LordDeathRateMultiplier = 0.2f;
			}
			else if (!TryReadFloat(root, "LordDeathRateMultiplier", out candidate.LordDeathRateMultiplier))
			{
				failure = "the serialized profile has a missing or invalid lord-death rate.";
				return false;
			}
			if (!candidate.TryUpgradeLegacyProfile())
			{
				failure = "unsupported profile schema " + candidate.SchemaVersion + ".";
				return false;
			}
			if (!candidate.TryValidate(out failure))
			{
				return false;
			}
			profile = candidate;
			return true;
		}
		catch (Exception exception)
		{
			failure = "the serialized profile could not be read: " + exception.GetType().Name + ".";
			return false;
		}
	}

	private string BuildFingerprint()
	{
		string payload = string.Join("|", SchemaVersion.ToString(CultureInfo.InvariantCulture), CalendarSystem ?? string.Empty, UseLeapYears.ToString(), MonthLengthSignature ?? string.Empty, AutoCampaignTimeScale.ToString(), CampaignTimeScale.ToString("R", CultureInfo.InvariantCulture), NormalPlayTimeMultiplier.ToString("R", CultureInfo.InvariantCulture), FastForwardTimeMultiplier.ToString("R", CultureInfo.InvariantCulture), UseCalendarMonthPregnancy.ToString(), PregnancyDurationMonths.ToString(CultureInfo.InvariantCulture), PregnancyDurationInDays.ToString("R", CultureInfo.InvariantCulture), RenownGainMultiplier.ToString("R", CultureInfo.InvariantCulture), LordDeathRateMultiplier.ToString("R", CultureInfo.InvariantCulture), BalancePartyImpairment.ToString(), BalancePrisonerRecruitment.ToString(), BalanceNpcMarriage.ToString(), BalanceMapTracks.ToString(), BalanceQuestDeadlines.ToString(), AnnualBalanceEnabled.ToString(), LegacyNativeAgeBasis.ToString());
		using SHA256 sha256 = SHA256.Create();
		return string.Concat(from value in sha256.ComputeHash(Encoding.UTF8.GetBytes(payload))
			select value.ToString("x2", CultureInfo.InvariantCulture));
	}

	private static bool TryReadString(XmlElement root, string name, out string value)
	{
		value = root.GetAttribute(name);
		return !string.IsNullOrWhiteSpace(value);
	}

	private static bool TryReadBoolean(XmlElement root, string name, out bool value)
	{
		return bool.TryParse(root.GetAttribute(name), out value);
	}

	private static bool TryReadFloat(XmlElement root, string name, out float value)
	{
		if (float.TryParse(root.GetAttribute(name), NumberStyles.Float, CultureInfo.InvariantCulture, out value))
		{
			return IsFinite(value);
		}
		return false;
	}

	private static bool TryReadInt(XmlElement root, string name, out int value)
	{
		return int.TryParse(root.GetAttribute(name), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
	}

	private static string BuildMonthLengthSignature(int[] values)
	{
		if (values != null)
		{
			return string.Join(",", values.Select((int value) => value.ToString(CultureInfo.InvariantCulture)));
		}
		return string.Empty;
	}

	private static bool NearlyEqual(float first, float second)
	{
		return Math.Abs(first - second) < 0.0001f;
	}

	private static bool IsFinite(float value)
	{
		if (!float.IsNaN(value))
		{
			return !float.IsInfinity(value);
		}
		return false;
	}
}
}

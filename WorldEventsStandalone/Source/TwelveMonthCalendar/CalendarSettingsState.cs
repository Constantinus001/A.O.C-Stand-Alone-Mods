using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace TwelveMonthCalendar
{

public static class CalendarSettingsState
{
	private static readonly object SyncRoot;

	private const string FixedCalendarSystem = "Gregorian12Month";

	private const int RequiredCommonDaysInYear = 365;

	private const string DefaultDateFormat = "{Month} {Day} {Year}";

	private const bool DefaultUseOrdinalDaySuffixes = true;

	private const bool DefaultUse24HourClock = true;

	internal const int DefaultNativeDaysInYear = 84;

	public const float DefaultCampaignTimeScale = 0.15f;

	public const bool DefaultClockSynchronizedLighting = true;

	public const float DefaultVisualSunriseHour = 6.25f;

	public const float DefaultVisualSunsetHour = 18.25f;

	public const float DefaultVisualLightingTransitionHours = 2f;

	public const bool DefaultStrategicMapShowLegend = true;

	public const int DefaultStrategicMapLegendWidth = 250;

	public const int DefaultStrategicMapLegendHeight = 108;

	public const int DefaultStrategicMapLegendMarginTop = 6;

	public const int DefaultStrategicMapLegendIconSize = 30;

	public const int DefaultStrategicMapLegendFontSize = 13;

	public const float DefaultStrategicMapMarkerSpacing = 52f;

	public const bool DefaultStrategicMapShowSettlementLabels = true;

	public const int DefaultStrategicMapLabelFontSize = 12;

	public const int DefaultPoliticalControlOpacityPercent = 100;

	public const int DefaultPoliticalControlBrightnessPercent = 100;

	public const bool DefaultPoliticalSolidWater = true;

	internal const float DefaultPregnancyDurationInDays = 273.75f;

	internal const int DefaultPregnancyDurationMonths = 9;

	internal const float DefaultRenownGainMultiplier = 0.5f;

	internal const float DefaultLordDeathRateMultiplier = 0.2f;

	internal const float DefaultNormalPlayTimeMultiplier = 1f;

	internal const float DefaultFastForwardTimeMultiplier = 4f;

	internal const float MinimumPacingMultiplier = 1f;

	internal const float MaximumPacingMultiplier = 4f;

	private const int MaximumConfiguredMonthLength = 1000;

	private const int MaximumConfiguredMonthNameLength = 24;

	private static readonly string[] DefaultMonthNames;

	private static readonly string[] DefaultSeasonNames;

	private static readonly int[] DefaultMonthLengths;

	private static bool _useLeapYears;

	private static bool _showDayLabel;

	private static bool _showYearLabel;

	private static bool _useOrdinalDaySuffixes;

	private static bool _use24HourClock;

	private static float _campaignTimeScale;

	private static bool _autoCampaignTimeScale;

	private static float _fastForwardTimeMultiplier;

	private static bool _clockSynchronizedLighting;

	private static float _visualSunriseHour;

	private static float _visualSunsetHour;

	private static float _visualLightingTransitionHours;

	private static bool _strategicMapShowLegend;

	private static int _strategicMapLegendWidth;

	private static int _strategicMapLegendHeight;

	private static int _strategicMapLegendMarginTop;

	private static int _strategicMapLegendIconSize;

	private static int _strategicMapLegendFontSize;

	private static float _strategicMapMarkerSpacing;

	private static bool _strategicMapShowSettlementLabels;

	private static int _strategicMapLabelFontSize;

	private static int _politicalControlOpacityPercent;

	private static int _politicalControlBrightnessPercent;

	private static bool _politicalSolidWater;

	private static string _dateFormat;

	private static int _nativeDaysInYear;

	private static float _pregnancyDurationInDays;

	private static int _pregnancyDurationMonths;

	private static bool _useCalendarMonthPregnancy;

	private static float _renownGainMultiplier;

	private static float _lordDeathRateMultiplier;

	private static bool _balancePartyImpairment;

	private static bool _balancePrisonerRecruitment;

	private static bool _balanceNpcMarriage;

	private static bool _balanceMapTracks;

	private static bool _balanceQuestDeadlines;

	private static bool _annualBalanceEnabled;

	private static bool _annualBalanceDiagnosticsEnabled;

	private static bool _campaignSessionStarted;

	private static bool _legacySaveAgeCompatibility;

	private static double _legacySaveAgeCutoverDay;

	private static readonly string[] _monthNames;

	private static readonly string[] _seasonNames;

	private static readonly int[] _monthLengths;

	private static readonly int[] _monthStarts;

	private static int _commonDaysInYear;

	public static bool ExtendedCalendarEnabled => true;

	internal static bool RefugeSystemEnabled => false;

	public static bool UseLeapYears => false;

	public static bool ShowDayLabel
	{
		get
		{
			lock (SyncRoot)
			{
				return _showDayLabel;
			}
		}
	}

	public static bool ShowYearLabel
	{
		get
		{
			lock (SyncRoot)
			{
				return _showYearLabel;
			}
		}
	}

	public static bool UseOrdinalDaySuffixes
	{
		get
		{
			lock (SyncRoot)
			{
				return _useOrdinalDaySuffixes;
			}
		}
	}

	public static bool Use24HourClock
	{
		get
		{
			lock (SyncRoot)
			{
				return _use24HourClock;
			}
		}
	}

	public static float CampaignTimeScale
	{
		get
		{
			lock (SyncRoot)
			{
				return _campaignTimeScale;
			}
		}
	}

	public static bool AutoCampaignTimeScale
	{
		get
		{
			lock (SyncRoot)
			{
				return _autoCampaignTimeScale;
			}
		}
	}

	public static float NormalPlayTimeMultiplier => 1f;

	public static float FastForwardTimeMultiplier
	{
		get
		{
			lock (SyncRoot)
			{
				return _fastForwardTimeMultiplier;
			}
		}
	}

	public static bool ClockSynchronizedLighting
	{
		get
		{
			lock (SyncRoot)
			{
				return _clockSynchronizedLighting;
			}
		}
	}

	public static float VisualSunriseHour
	{
		get
		{
			lock (SyncRoot)
			{
				return _visualSunriseHour;
			}
		}
	}

	public static float VisualSunsetHour
	{
		get
		{
			lock (SyncRoot)
			{
				return _visualSunsetHour;
			}
		}
	}

	public static float VisualLightingTransitionHours
	{
		get
		{
			lock (SyncRoot)
			{
				return _visualLightingTransitionHours;
			}
		}
	}

	public static bool StrategicMapShowLegend
	{
		get
		{
			lock (SyncRoot)
			{
				return _strategicMapShowLegend;
			}
		}
	}

	public static int StrategicMapLegendWidth
	{
		get
		{
			lock (SyncRoot)
			{
				return _strategicMapLegendWidth;
			}
		}
	}

	public static int StrategicMapLegendHeight
	{
		get
		{
			lock (SyncRoot)
			{
				return _strategicMapLegendHeight;
			}
		}
	}

	public static int StrategicMapLegendMarginTop
	{
		get
		{
			lock (SyncRoot)
			{
				return _strategicMapLegendMarginTop;
			}
		}
	}

	public static int StrategicMapLegendIconSize
	{
		get
		{
			lock (SyncRoot)
			{
				return _strategicMapLegendIconSize;
			}
		}
	}

	public static int StrategicMapLegendFontSize
	{
		get
		{
			lock (SyncRoot)
			{
				return _strategicMapLegendFontSize;
			}
		}
	}

	public static float StrategicMapMarkerSpacing
	{
		get
		{
			lock (SyncRoot)
			{
				return _strategicMapMarkerSpacing;
			}
		}
	}

	public static bool StrategicMapShowSettlementLabels
	{
		get
		{
			lock (SyncRoot)
			{
				return _strategicMapShowSettlementLabels;
			}
		}
	}

	public static int StrategicMapLabelFontSize
	{
		get
		{
			lock (SyncRoot)
			{
				return _strategicMapLabelFontSize;
			}
		}
	}

	public static int PoliticalControlOpacityPercent
	{
		get
		{
			lock (SyncRoot)
			{
				return _politicalControlOpacityPercent;
			}
		}
	}

	public static int PoliticalControlBrightnessPercent
	{
		get
		{
			lock (SyncRoot)
			{
				return _politicalControlBrightnessPercent;
			}
		}
	}

	public static bool PoliticalSolidWater
	{
		get
		{
			lock (SyncRoot)
			{
				return _politicalSolidWater;
			}
		}
	}

	internal static bool IsCampaignProfileLocked
	{
		get
		{
			lock (SyncRoot)
			{
				return _campaignSessionStarted;
			}
		}
	}

	public static string DateFormat
	{
		get
		{
			lock (SyncRoot)
			{
				return _dateFormat;
			}
		}
	}

	public static int NativeDaysInYear
	{
		get
		{
			lock (SyncRoot)
			{
				return _nativeDaysInYear;
			}
		}
	}

	public static float PregnancyDurationInDays
	{
		get
		{
			lock (SyncRoot)
			{
				return _pregnancyDurationInDays;
			}
		}
	}

	public static int PregnancyDurationMonths
	{
		get
		{
			lock (SyncRoot)
			{
				return _pregnancyDurationMonths;
			}
		}
	}

	public static bool UseCalendarMonthPregnancy
	{
		get
		{
			lock (SyncRoot)
			{
				return _useCalendarMonthPregnancy;
			}
		}
	}

	public static float RenownGainMultiplier
	{
		get
		{
			lock (SyncRoot)
			{
				return _renownGainMultiplier;
			}
		}
	}

	public static float LordDeathRateMultiplier
	{
		get
		{
			lock (SyncRoot)
			{
				return _lordDeathRateMultiplier;
			}
		}
	}

	public static bool BalancePartyImpairment
	{
		get
		{
			lock (SyncRoot)
			{
				return _balancePartyImpairment;
			}
		}
	}

	public static bool BalancePrisonerRecruitment
	{
		get
		{
			lock (SyncRoot)
			{
				return _balancePrisonerRecruitment;
			}
		}
	}

	public static bool BalanceNpcMarriage
	{
		get
		{
			lock (SyncRoot)
			{
				return _balanceNpcMarriage;
			}
		}
	}

	public static bool BalanceMapTracks
	{
		get
		{
			lock (SyncRoot)
			{
				return _balanceMapTracks;
			}
		}
	}

	public static bool BalanceQuestDeadlines
	{
		get
		{
			lock (SyncRoot)
			{
				return _balanceQuestDeadlines;
			}
		}
	}

	public static bool AnnualBalanceDiagnosticsEnabled
	{
		get
		{
			lock (SyncRoot)
			{
				return _annualBalanceDiagnosticsEnabled;
			}
		}
	}

	public static int CommonDaysInYear => 84;

	public static bool AnnualBalanceEnabled
	{
		get
		{
			lock (SyncRoot)
			{
				return _annualBalanceEnabled;
			}
		}
	}

	public static bool AnnualRateBalanceEnabled
	{
		get
		{
			if (ExtendedCalendarEnabled)
			{
				return AnnualBalanceEnabled;
			}
			return false;
		}
	}

	internal static bool IsLegacySaveAgeCompatibility
	{
		get
		{
			lock (SyncRoot)
			{
				return _legacySaveAgeCompatibility;
			}
		}
	}

	internal static double LegacySaveAgeCutoverDay
	{
		get
		{
			lock (SyncRoot)
			{
				return _legacySaveAgeCutoverDay;
			}
		}
	}

	public static string MonthNamesDelimited
	{
		get
		{
			lock (SyncRoot)
			{
				return string.Join("|", _monthNames);
			}
		}
	}

	public static string SeasonNamesDelimited
	{
		get
		{
			lock (SyncRoot)
			{
				return string.Join("|", _seasonNames);
			}
		}
	}

	public static string MonthLengthsDelimited
	{
		get
		{
			lock (SyncRoot)
			{
				return string.Join("|", _monthLengths.Select((int value) => value.ToString(CultureInfo.InvariantCulture)));
			}
		}
	}

	public static string CalendarSystem => "Gregorian12Month";

	public static string ConfigPath => Path.Combine(GetModuleRoot(), "AgesOfCalradia.settings.xml");

	private static string PreviousUserConfigPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Mount and Blade II Bannerlord", "Configs", "AgesOfCalradia", "settings.xml");

	private static string LegacyConfigPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Mount and Blade II Bannerlord", "Configs", "TwelveMonthCalendar", "settings.xml");

	public static event Action SettingsChanged;

	static CalendarSettingsState()
	{
		SyncRoot = new object();
		DefaultMonthNames = new string[12]
		{
			"January", "February", "March", "April", "May", "June", "July", "August", "September", "October",
			"November", "December"
		};
		DefaultSeasonNames = new string[4] { "Spring", "Summer", "Autumn", "Winter" };
		DefaultMonthLengths = new int[12]
		{
			31, 28, 31, 30, 31, 30, 31, 31, 30, 31,
			30, 31
		};
		_useLeapYears = true;
		_useOrdinalDaySuffixes = true;
		_use24HourClock = true;
		_campaignTimeScale = 0.15f;
		_autoCampaignTimeScale = true;
		_fastForwardTimeMultiplier = 4f;
		_clockSynchronizedLighting = true;
		_visualSunriseHour = 6.25f;
		_visualSunsetHour = 18.25f;
		_visualLightingTransitionHours = 2f;
		_strategicMapShowLegend = true;
		_strategicMapLegendWidth = 250;
		_strategicMapLegendHeight = 108;
		_strategicMapLegendMarginTop = 6;
		_strategicMapLegendIconSize = 30;
		_strategicMapLegendFontSize = 13;
		_strategicMapMarkerSpacing = 52f;
		_strategicMapShowSettlementLabels = true;
		_strategicMapLabelFontSize = 12;
		_politicalControlOpacityPercent = 100;
		_politicalControlBrightnessPercent = 100;
		_politicalSolidWater = true;
		_dateFormat = "{Month} {Day} {Year}";
		_nativeDaysInYear = 84;
		_pregnancyDurationInDays = 273.75f;
		_pregnancyDurationMonths = 9;
		_useCalendarMonthPregnancy = true;
		_renownGainMultiplier = 0.5f;
		_lordDeathRateMultiplier = 0.2f;
		_balancePartyImpairment = true;
		_balancePrisonerRecruitment = true;
		_balanceNpcMarriage = true;
		_balanceMapTracks = true;
		_balanceQuestDeadlines = true;
		_annualBalanceEnabled = true;
		_annualBalanceDiagnosticsEnabled = true;
		_legacySaveAgeCutoverDay = double.NaN;
		_monthNames = (string[])DefaultMonthNames.Clone();
		_seasonNames = (string[])DefaultSeasonNames.Clone();
		_monthLengths = (int[])DefaultMonthLengths.Clone();
		_monthStarts = new int[12];
		RebuildMonthCache();
	}

	public static string GetMonthName(int month)
	{
		lock (SyncRoot)
		{
			if (month < 0 || month >= _seasonNames.Length)
			{
				throw new ArgumentOutOfRangeException("month");
			}
			return _seasonNames[month];
		}
	}

	public static string GetSeasonName(int season)
	{
		lock (SyncRoot)
		{
			return _seasonNames[season];
		}
	}

	public static int GetMonthLength(int month)
	{
		lock (SyncRoot)
		{
			if (month < 0 || month >= 4)
			{
				throw new ArgumentOutOfRangeException("month");
			}
			return 21;
		}
	}

	public static int GetMonthStart(int month)
	{
		lock (SyncRoot)
		{
			if (month < 0 || month >= 4)
			{
				throw new ArgumentOutOfRangeException("month");
			}
			return month * 21;
		}
	}

	public static string[] MonthNamesSnapshot()
	{
		lock (SyncRoot)
		{
			return (string[])_monthNames.Clone();
		}
	}

	public static string[] SeasonNamesSnapshot()
	{
		lock (SyncRoot)
		{
			return (string[])_seasonNames.Clone();
		}
	}

	public static int[] MonthLengthsSnapshot()
	{
		lock (SyncRoot)
		{
			return (int[])_monthLengths.Clone();
		}
	}

	internal static void MarkLegacySaveAgeCompatibility(double cutoverDay)
	{
		lock (SyncRoot)
		{
			_legacySaveAgeCompatibility = true;
			if (!double.IsNaN(cutoverDay) && !double.IsInfinity(cutoverDay))
			{
				_legacySaveAgeCutoverDay = cutoverDay;
			}
		}
	}

	internal static void MarkModernSaveAgeCompatibility()
	{
		lock (SyncRoot)
		{
			_legacySaveAgeCompatibility = false;
			_legacySaveAgeCutoverDay = double.NaN;
		}
	}

	public static bool TryParseMonthNamesDelimited(string input, out string[] values, out string failure)
	{
		if (!TryParseDelimitedNames(input, 12, "month", out values, out failure))
		{
			return false;
		}
		return true;
	}

	public static bool TryParseSeasonNamesDelimited(string input, out string[] values, out string failure)
	{
		if (!TryParseDelimitedNames(input, 4, "season", out values, out failure))
		{
			return false;
		}
		return true;
	}

	public static bool TryParseMonthLengthsDelimited(string input, out int[] values, out string failure)
	{
		values = null;
		if (string.IsNullOrWhiteSpace(input))
		{
			failure = "Enter twelve positive month lengths separated by |.";
			return false;
		}
		string[] parts = input.Replace(',', '|').Split('|');
		if (parts.Length != 12)
		{
			failure = "Enter exactly twelve month lengths separated by |.";
			return false;
		}
		int[] parsed = new int[parts.Length];
		int total = 0;
		for (int index = 0; index < parts.Length; index++)
		{
			if (!int.TryParse(parts[index].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) || value < 1 || value > 1000)
			{
				failure = "Each month length must be a whole number from 1 to " + 1000 + ".";
				return false;
			}
			parsed[index] = value;
			total += value;
		}
		if (total != 365)
		{
			failure = "Month lengths must total exactly " + 365 + " days.";
			return false;
		}
		values = parsed;
		failure = null;
		return true;
	}

	public static bool TryApplyMonthNamesDelimited(string input, out string failure)
	{
		if (!TryParseMonthNamesDelimited(input, out var values, out failure))
		{
			return false;
		}
		Apply(CalendarSystem, UseLeapYears, ShowDayLabel, ShowYearLabel, CampaignTimeScale, DateFormat, values);
		Save();
		return true;
	}

	public static bool TryApplySeasonNamesDelimited(string input, out string failure)
	{
		if (!TryParseSeasonNamesDelimited(input, out var values, out failure))
		{
			return false;
		}
		Apply(CalendarSystem, UseLeapYears, ShowDayLabel, ShowYearLabel, CampaignTimeScale, DateFormat, null, null, values);
		Save();
		return true;
	}

	public static bool TryApplyMonthLengthsDelimited(string input, out string failure)
	{
		if (IsCampaignProfileLocked)
		{
			failure = "Month lengths are locked by the active campaign profile.";
			return false;
		}
		if (!TryParseMonthLengthsDelimited(input, out var values, out failure))
		{
			return false;
		}
		Apply(CalendarSystem, UseLeapYears, ShowDayLabel, ShowYearLabel, CampaignTimeScale, DateFormat, null, values);
		Save();
		return true;
	}

	public static void Apply(string calendarSystem, bool useLeapYears, bool showDayLabel, bool showYearLabel, float campaignTimeScale, string dateFormat, string[] monthNames = null, int[] monthLengths = null, string[] seasonNames = null, int? nativeDaysInYear = null, float? pregnancyDurationInDays = null, int? pregnancyDurationMonths = null, bool? useCalendarMonthPregnancy = null, bool? autoCampaignTimeScale = null, float? renownGainMultiplier = null, float? lordDeathRateMultiplier = null, bool? useOrdinalDaySuffixes = null, bool? use24HourClock = null, bool? balancePartyImpairment = null, bool? balancePrisonerRecruitment = null, bool? balanceNpcMarriage = null, bool? balanceMapTracks = null, bool? balanceQuestDeadlines = null, bool? annualBalanceEnabled = null, bool? annualBalanceDiagnosticsEnabled = null, float? normalPlayTimeMultiplier = null, float? fastForwardTimeMultiplier = null, bool? clockSynchronizedLighting = null, float? visualSunriseHour = null, float? visualSunsetHour = null, float? visualLightingTransitionHours = null, bool? strategicMapShowLegend = null, int? strategicMapLegendWidth = null, int? strategicMapLegendHeight = null, int? strategicMapLegendMarginTop = null, int? strategicMapLegendIconSize = null, int? strategicMapLegendFontSize = null, float? strategicMapMarkerSpacing = null, bool? strategicMapShowSettlementLabels = null, int? strategicMapLabelFontSize = null, int? politicalControlOpacityPercent = null, int? politicalControlBrightnessPercent = null, bool? politicalSolidWater = null)
	{
		lock (SyncRoot)
		{
			if (_campaignSessionStarted && useLeapYears != _useLeapYears)
			{
				Diagnostics.Info("Leap-year setting change ignored after campaign session start; restart the campaign to apply it.");
			}
			else
			{
				_useLeapYears = useLeapYears;
			}
			if (!string.IsNullOrWhiteSpace(calendarSystem) && !string.Equals(calendarSystem, "Gregorian12Month", StringComparison.OrdinalIgnoreCase))
			{
				Diagnostics.Info("Ignoring legacy calendar-system setting '" + calendarSystem + "'; Ages of Calradia is always Gregorian12Month.");
			}
			_showDayLabel = showDayLabel;
			_showYearLabel = showYearLabel;
			_useOrdinalDaySuffixes = useOrdinalDaySuffixes ?? _useOrdinalDaySuffixes;
			_use24HourClock = use24HourClock ?? _use24HourClock;
			_dateFormat = NormalizeDateFormat(dateFormat);
			ApplyMonthNames(monthNames);
			ApplySeasonNames(seasonNames);
			ApplyMonthLengths(monthLengths);
			int requestedNativeDaysInYear = nativeDaysInYear ?? _nativeDaysInYear;
			if (requestedNativeDaysInYear != 84)
			{
				Diagnostics.Info("Ignoring NativeDaysInYear=" + requestedNativeDaysInYear + "; annual balance is calibrated to Bannerlord's native 84-day year.");
			}
			_nativeDaysInYear = 84;
			float requestedPregnancyDays = pregnancyDurationInDays ?? _pregnancyDurationInDays;
			float normalizedPregnancyDays = (IsFinite(requestedPregnancyDays) ? Math.Max(0.1f, Math.Min(10000f, requestedPregnancyDays)) : 273.75f);
			ApplyCampaignStartSetting(ref _pregnancyDurationInDays, normalizedPregnancyDays, "PregnancyDurationDays");
			_pregnancyDurationMonths = Math.Max(1, pregnancyDurationMonths ?? _pregnancyDurationMonths);
			_useCalendarMonthPregnancy = useCalendarMonthPregnancy ?? _useCalendarMonthPregnancy;
			float requestedRenownMultiplier = renownGainMultiplier ?? _renownGainMultiplier;
			_renownGainMultiplier = (IsFinite(requestedRenownMultiplier) ? Math.Max(0f, Math.Min(1f, requestedRenownMultiplier)) : 0.5f);
			float requestedLordDeathRateMultiplier = lordDeathRateMultiplier ?? _lordDeathRateMultiplier;
			_lordDeathRateMultiplier = (IsFinite(requestedLordDeathRateMultiplier) ? Math.Max(0f, Math.Min(1f, requestedLordDeathRateMultiplier)) : 0.2f);
			_balancePartyImpairment = balancePartyImpairment ?? _balancePartyImpairment;
			_balancePrisonerRecruitment = balancePrisonerRecruitment ?? _balancePrisonerRecruitment;
			_balanceNpcMarriage = balanceNpcMarriage ?? _balanceNpcMarriage;
			_balanceMapTracks = balanceMapTracks ?? _balanceMapTracks;
			_balanceQuestDeadlines = balanceQuestDeadlines ?? _balanceQuestDeadlines;
			_annualBalanceEnabled = annualBalanceEnabled ?? _annualBalanceEnabled;
			_annualBalanceDiagnosticsEnabled = annualBalanceDiagnosticsEnabled ?? _annualBalanceDiagnosticsEnabled;
			bool num = autoCampaignTimeScale ?? _autoCampaignTimeScale;
			_autoCampaignTimeScale = num;
			_campaignTimeScale = (num ? 0.15f : (IsFinite(campaignTimeScale) ? Math.Max(0.01f, Math.Min(1f, campaignTimeScale)) : 0.15f));
			_fastForwardTimeMultiplier = NormalizePacingMultiplier(fastForwardTimeMultiplier ?? _fastForwardTimeMultiplier, 4f);
			_clockSynchronizedLighting = clockSynchronizedLighting ?? _clockSynchronizedLighting;
			_visualSunriseHour = NormalizeLightingHour(visualSunriseHour ?? _visualSunriseHour, 6.25f);
			_visualSunsetHour = NormalizeLightingHour(visualSunsetHour ?? _visualSunsetHour, 18.25f);
			if (Math.Abs(_visualSunsetHour - _visualSunriseHour) < 0.25f)
			{
				_visualSunriseHour = 6.25f;
				_visualSunsetHour = 18.25f;
			}
			_visualLightingTransitionHours = NormalizeLightingTransition(visualLightingTransitionHours ?? _visualLightingTransitionHours);
			_strategicMapShowLegend = strategicMapShowLegend ?? _strategicMapShowLegend;
			_strategicMapLegendWidth = ClampInt(strategicMapLegendWidth ?? _strategicMapLegendWidth, 180, 400, 250);
			_strategicMapLegendHeight = ClampInt(strategicMapLegendHeight ?? _strategicMapLegendHeight, 60, 220, 108);
			_strategicMapLegendMarginTop = ClampInt(strategicMapLegendMarginTop ?? _strategicMapLegendMarginTop, 0, 80, 6);
			_strategicMapLegendIconSize = ClampInt(strategicMapLegendIconSize ?? _strategicMapLegendIconSize, 16, 64, 30);
			_strategicMapLegendFontSize = ClampInt(strategicMapLegendFontSize ?? _strategicMapLegendFontSize, 8, 24, 13);
			_strategicMapMarkerSpacing = ClampFloat(strategicMapMarkerSpacing ?? _strategicMapMarkerSpacing, 52f, 200f, 52f);
			_strategicMapShowSettlementLabels = strategicMapShowSettlementLabels ?? _strategicMapShowSettlementLabels;
			_strategicMapLabelFontSize = ClampInt(strategicMapLabelFontSize ?? _strategicMapLabelFontSize, 8, 24, 12);
			_politicalControlOpacityPercent = ClampInt(politicalControlOpacityPercent ?? _politicalControlOpacityPercent, 10, 100, 100);
			_politicalControlBrightnessPercent = ClampInt(politicalControlBrightnessPercent ?? _politicalControlBrightnessPercent, 25, 125, 100);
			_politicalSolidWater = politicalSolidWater ?? _politicalSolidWater;
		}
		Diagnostics.Info(string.Format("Settings applied. CalendarSystem={0}; LeapYears={1}; ShowDayLabel={2}; ShowYearLabel={3}; OrdinalDays={4}; Clock24Hour={5}; TimeScale={6:F6}; NormalPace=fixed; FastForwardSpeed={7:F0}; LightingSync={8}; VisualSunrise={9:F2}; VisualSunset={10:F2}; LightingTransition={11:F2}; LordDeathRate={12:F3}; DateFormat={13}", "Gregorian12Month", UseLeapYears, ShowDayLabel, ShowYearLabel, UseOrdinalDaySuffixes, Use24HourClock, CampaignTimeScale, FastForwardTimeMultiplier, ClockSynchronizedLighting, VisualSunriseHour, VisualSunsetHour, VisualLightingTransitionHours, LordDeathRateMultiplier, DateFormat));
		NotifySettingsChanged();
	}

	public static void ResetToDefaults()
	{
		Apply("Gregorian12Month", useLeapYears: true, showDayLabel: false, showYearLabel: false, 0.15f, "{Month} {Day} {Year}", (string[])DefaultMonthNames.Clone(), (int[])DefaultMonthLengths.Clone(), (string[])DefaultSeasonNames.Clone(), 84, 273.75f, 9, true, true, 0.5f, 0.2f, true, true, true, true, true, true, true, true, true, 1f, 4f, true, 6.25f, 18.25f, 2f, true, 250, 108, 6, 30, 13, 52f, true, 12, 100, 100, true);
		Save();
		Diagnostics.Info("Calendar settings reset to defaults.");
	}

	internal static void ResetCalendarCategory()
	{
		lock (SyncRoot)
		{
			if (!_campaignSessionStarted)
			{
				_useLeapYears = true;
				Array.Copy(DefaultMonthLengths, _monthLengths, DefaultMonthLengths.Length);
				RebuildMonthCache();
			}
			Array.Copy(DefaultMonthNames, _monthNames, DefaultMonthNames.Length);
			Array.Copy(DefaultSeasonNames, _seasonNames, DefaultSeasonNames.Length);
		}
		Save();
		NotifySettingsChanged();
		Diagnostics.Info("Calendar category settings reset to defaults.");
	}

	internal static void MarkCampaignSessionStarted()
	{
		lock (SyncRoot)
		{
			_campaignSessionStarted = true;
		}
	}

	internal static void BeginCampaignSession()
	{
		lock (SyncRoot)
		{
			_campaignSessionStarted = false;
			_legacySaveAgeCompatibility = false;
			_legacySaveAgeCutoverDay = double.NaN;
		}
	}

	internal static void ApplyPersistedCampaignProfile(CalendarCampaignProfile profile)
	{
		if (profile == null)
		{
			return;
		}
		if (!profile.TryValidate(out var failure) || !profile.TryGetMonthLengths(out var profileMonthLengths))
		{
			Diagnostics.Info("Saved campaign profile restore was skipped because " + (failure ?? "its month lengths are invalid.") + ".");
			return;
		}
		lock (SyncRoot)
		{
			_useLeapYears = profile.UseLeapYears;
			for (int index = 0; index < _monthLengths.Length; index++)
			{
				_monthLengths[index] = profileMonthLengths[index];
			}
			RebuildMonthCache();
			_autoCampaignTimeScale = profile.AutoCampaignTimeScale;
			_campaignTimeScale = (_autoCampaignTimeScale ? 0.15f : Math.Max(0.01f, Math.Min(1f, profile.CampaignTimeScale)));
			_fastForwardTimeMultiplier = NormalizePacingMultiplier(profile.FastForwardTimeMultiplier, 4f);
			_useCalendarMonthPregnancy = profile.UseCalendarMonthPregnancy;
			_pregnancyDurationMonths = Math.Max(1, profile.PregnancyDurationMonths);
			_pregnancyDurationInDays = Math.Max(0.1f, Math.Min(10000f, profile.PregnancyDurationInDays));
			_renownGainMultiplier = Math.Max(0f, Math.Min(1f, profile.RenownGainMultiplier));
			_lordDeathRateMultiplier = Math.Max(0f, Math.Min(1f, profile.LordDeathRateMultiplier));
			_balancePartyImpairment = profile.BalancePartyImpairment;
			_balancePrisonerRecruitment = profile.BalancePrisonerRecruitment;
			_balanceNpcMarriage = profile.BalanceNpcMarriage;
			_balanceMapTracks = profile.BalanceMapTracks;
			_balanceQuestDeadlines = profile.BalanceQuestDeadlines;
			_annualBalanceEnabled = profile.AnnualBalanceEnabled;
		}
		Diagnostics.Info("Persisted campaign profile applied. Fingerprint=" + profile.Fingerprint + "; TimeScale=" + CampaignTimeScale.ToString("F6", CultureInfo.InvariantCulture) + "; NormalPace=fixed; FastForwardSpeed=" + FastForwardTimeMultiplier.ToString("F0", CultureInfo.InvariantCulture) + "; LordDeathRate=" + LordDeathRateMultiplier.ToString("F3", CultureInfo.InvariantCulture) + ".");
		NotifySettingsChanged();
	}

	internal static bool IsGameplaySettingLocked(string settingName)
	{
		if (!IsCampaignProfileLocked || string.IsNullOrWhiteSpace(settingName))
		{
			return false;
		}
		_ = settingName == "Use Leap Years";
		return false;
	}

	public static void Load()
	{
		try
		{
			string settingsPath = ConfigPath;
			bool migratedLegacyPath = false;
			if (!File.Exists(settingsPath) && File.Exists(PreviousUserConfigPath))
			{
				settingsPath = PreviousUserConfigPath;
				migratedLegacyPath = true;
			}
			else if (!File.Exists(settingsPath) && File.Exists(LegacyConfigPath))
			{
				settingsPath = LegacyConfigPath;
				migratedLegacyPath = true;
			}
			if (!File.Exists(settingsPath))
			{
				Diagnostics.Info("No standalone settings file found; using defaults.");
				Save();
				return;
			}
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(settingsPath);
			XmlElement root = xmlDocument.DocumentElement;
			if (root == null || (root.Name != "AgesOfCalradia" && root.Name != "TwelveMonthCalendar"))
			{
				throw new InvalidDataException("The standalone settings file has an invalid root element.");
			}
			Apply(ReadAttribute(root, "CalendarSystem", "Gregorian12Month"), ReadBoolean(root, "UseLeapYears", fallback: true), ReadBoolean(root, "ShowDayLabel", fallback: false), ReadBoolean(root, "ShowYearLabel", fallback: false), ReadFloat(root, "CampaignTimeScale", 0.15f), ReadAttribute(root, "DateFormat", "{Month} {Day} {Year}"), ReadMonthNames(root), ReadMonthLengths(root), ReadSeasonNames(root), ReadInt(root, "NativeDaysInYear", 84), ReadFloat(root, "PregnancyDurationDays", 273.75f), ReadInt(root, "PregnancyDurationMonths", 9), ReadBoolean(root, "UseCalendarMonthPregnancy", fallback: true), ReadBoolean(root, "AutoCampaignTimeScale", fallback: true), ReadFloat(root, "RenownGainMultiplier", 0.5f), ReadFloat(root, "LordDeathRateMultiplier", 0.2f), ReadBoolean(root, "UseOrdinalDaySuffixes", fallback: true), ReadBoolean(root, "Use24HourClock", fallback: true), ReadBoolean(root, "BalancePartyImpairment", fallback: true), ReadBoolean(root, "BalancePrisonerRecruitment", fallback: true), ReadBoolean(root, "BalanceNpcMarriage", fallback: true), ReadBoolean(root, "BalanceMapTracks", fallback: true), ReadBoolean(root, "BalanceQuestDeadlines", fallback: true), ReadBoolean(root, "AnnualBalanceEnabled", fallback: true), ReadBoolean(root, "AnnualBalanceDiagnosticsEnabled", fallback: true), null, ReadFastForwardSpeed(root), ReadBoolean(root, "ClockSynchronizedLighting", fallback: true), ReadFloat(root, "VisualSunriseHour", 6.25f), ReadFloat(root, "VisualSunsetHour", 18.25f), ReadFloat(root, "VisualLightingTransitionHours", 2f), ReadBoolean(root, "StrategicMapShowLegend", fallback: true), ReadInt(root, "StrategicMapLegendWidth", 250), ReadInt(root, "StrategicMapLegendHeight", 108), ReadInt(root, "StrategicMapLegendMarginTop", 6), ReadInt(root, "StrategicMapLegendIconSize", 30), ReadInt(root, "StrategicMapLegendFontSize", 13), ReadFloat(root, "StrategicMapMarkerSpacing", 52f), ReadBoolean(root, "StrategicMapShowSettlementLabels", fallback: true), ReadInt(root, "StrategicMapLabelFontSize", 12), ReadInt(root, "PoliticalControlOpacityPercent", 100), ReadInt(root, "PoliticalControlBrightnessPercent", 100), ReadBoolean(root, "PoliticalSolidWater", fallback: true));
			Diagnostics.Info(string.Format("Standalone settings loaded from {0}.{1}", settingsPath, migratedLegacyPath ? " Values will be copied to the legacy internal settings path" : string.Empty));
			Save();
		}
		catch (Exception exception)
		{
			Diagnostics.Error("Standalone settings could not be loaded; defaults remain active.", exception);
		}
	}

	public static void Save()
	{
		try
		{
			string directory = Path.GetDirectoryName(ConfigPath);
			if (!string.IsNullOrEmpty(directory))
			{
				Directory.CreateDirectory(directory);
			}
			XmlDocument document = new XmlDocument();
			XmlElement root = document.CreateElement("AgesOfCalradia");
			document.AppendChild(root);
			root.SetAttribute("UseLeapYears", UseLeapYears.ToString());
			root.SetAttribute("ShowDayLabel", ShowDayLabel.ToString());
			root.SetAttribute("ShowYearLabel", ShowYearLabel.ToString());
			root.SetAttribute("UseOrdinalDaySuffixes", UseOrdinalDaySuffixes.ToString());
			root.SetAttribute("Use24HourClock", Use24HourClock.ToString());
			root.SetAttribute("CampaignTimeScale", CampaignTimeScale.ToString("R", CultureInfo.InvariantCulture));
			root.SetAttribute("AutoCampaignTimeScale", AutoCampaignTimeScale.ToString());
			root.SetAttribute("FastForwardSpeedMultiplier", FastForwardTimeMultiplier.ToString("R", CultureInfo.InvariantCulture));
			root.SetAttribute("ClockSynchronizedLighting", ClockSynchronizedLighting.ToString());
			root.SetAttribute("VisualSunriseHour", VisualSunriseHour.ToString("R", CultureInfo.InvariantCulture));
			root.SetAttribute("VisualSunsetHour", VisualSunsetHour.ToString("R", CultureInfo.InvariantCulture));
			root.SetAttribute("VisualLightingTransitionHours", VisualLightingTransitionHours.ToString("R", CultureInfo.InvariantCulture));
			root.SetAttribute("StrategicMapShowLegend", StrategicMapShowLegend.ToString());
			root.SetAttribute("StrategicMapLegendWidth", StrategicMapLegendWidth.ToString(CultureInfo.InvariantCulture));
			root.SetAttribute("StrategicMapLegendHeight", StrategicMapLegendHeight.ToString(CultureInfo.InvariantCulture));
			root.SetAttribute("StrategicMapLegendMarginTop", StrategicMapLegendMarginTop.ToString(CultureInfo.InvariantCulture));
			root.SetAttribute("StrategicMapLegendIconSize", StrategicMapLegendIconSize.ToString(CultureInfo.InvariantCulture));
			root.SetAttribute("StrategicMapLegendFontSize", StrategicMapLegendFontSize.ToString(CultureInfo.InvariantCulture));
			root.SetAttribute("StrategicMapMarkerSpacing", StrategicMapMarkerSpacing.ToString("R", CultureInfo.InvariantCulture));
			root.SetAttribute("StrategicMapShowSettlementLabels", StrategicMapShowSettlementLabels.ToString());
			root.SetAttribute("StrategicMapLabelFontSize", StrategicMapLabelFontSize.ToString(CultureInfo.InvariantCulture));
			root.SetAttribute("PoliticalControlOpacityPercent", PoliticalControlOpacityPercent.ToString(CultureInfo.InvariantCulture));
			root.SetAttribute("PoliticalControlBrightnessPercent", PoliticalControlBrightnessPercent.ToString(CultureInfo.InvariantCulture));
			root.SetAttribute("PoliticalSolidWater", PoliticalSolidWater.ToString());
			root.SetAttribute("DateFormat", DateFormat);
			root.SetAttribute("NativeDaysInYear", NativeDaysInYear.ToString(CultureInfo.InvariantCulture));
			root.SetAttribute("PregnancyDurationDays", PregnancyDurationInDays.ToString("R", CultureInfo.InvariantCulture));
			root.SetAttribute("PregnancyDurationMonths", PregnancyDurationMonths.ToString(CultureInfo.InvariantCulture));
			root.SetAttribute("UseCalendarMonthPregnancy", UseCalendarMonthPregnancy.ToString());
			root.SetAttribute("RenownGainMultiplier", RenownGainMultiplier.ToString("R", CultureInfo.InvariantCulture));
			root.SetAttribute("LordDeathRateMultiplier", LordDeathRateMultiplier.ToString("R", CultureInfo.InvariantCulture));
			root.SetAttribute("BalancePartyImpairment", BalancePartyImpairment.ToString());
			root.SetAttribute("BalancePrisonerRecruitment", BalancePrisonerRecruitment.ToString());
			root.SetAttribute("BalanceNpcMarriage", BalanceNpcMarriage.ToString());
			root.SetAttribute("BalanceMapTracks", BalanceMapTracks.ToString());
			root.SetAttribute("BalanceQuestDeadlines", BalanceQuestDeadlines.ToString());
			root.SetAttribute("AnnualBalanceEnabled", AnnualBalanceEnabled.ToString());
			root.SetAttribute("AnnualBalanceDiagnosticsEnabled", AnnualBalanceDiagnosticsEnabled.ToString());
			string[] monthNames = MonthNamesSnapshot();
			string[] seasonNames = SeasonNamesSnapshot();
			int[] monthLengths = MonthLengthsSnapshot();
			for (int j = 0; j < 12; j++)
			{
				root.SetAttribute($"Month{j + 1}Name", monthNames[j]);
				root.SetAttribute($"Month{j + 1}Days", monthLengths[j].ToString(CultureInfo.InvariantCulture));
			}
			for (int i = 0; i < seasonNames.Length; i++)
			{
				root.SetAttribute($"Season{i + 1}Name", seasonNames[i]);
			}
			string temporaryPath2 = ConfigPath + ".tmp";
			document.Save(temporaryPath2);
			if (File.Exists(ConfigPath))
			{
				File.Replace(temporaryPath2, ConfigPath, null);
			}
			else
			{
				File.Move(temporaryPath2, ConfigPath);
			}
			Diagnostics.Info($"Standalone settings saved to {ConfigPath}.");
		}
		catch (Exception exception)
		{
			try
			{
				string temporaryPath = ConfigPath + ".tmp";
				if (File.Exists(temporaryPath))
				{
					File.Delete(temporaryPath);
				}
			}
			catch
			{
			}
			Diagnostics.Error("Standalone settings could not be saved.", exception);
		}
	}

	private static string GetModuleRoot()
	{
		string directoryName = Path.GetDirectoryName(typeof(CalendarSettingsState).Assembly.Location);
		if (string.IsNullOrEmpty(directoryName))
		{
			throw new InvalidOperationException("The module assembly location is unavailable.");
		}
		return (Directory.GetParent(directoryName)?.Parent ?? throw new InvalidOperationException("The module directory could not be resolved.")).FullName;
	}

	private static string ReadAttribute(XmlElement root, string name, string fallback)
	{
		string value = root.GetAttribute(name);
		if (!string.IsNullOrWhiteSpace(value))
		{
			return value;
		}
		return fallback;
	}

	private static void NotifySettingsChanged()
	{
		Action changed = CalendarSettingsState.SettingsChanged;
		if (changed == null)
		{
			return;
		}
		try
		{
			changed();
		}
		catch (Exception exception)
		{
			Diagnostics.Error("A settings synchronization listener failed.", exception);
		}
	}

	private static void ApplyCampaignStartSetting(ref bool currentValue, bool? requestedValue, string name)
	{
		if (requestedValue.HasValue && requestedValue.Value != currentValue)
		{
			if (_campaignSessionStarted)
			{
				Diagnostics.Info(name + " change ignored after campaign session start; restart the campaign to apply it.");
			}
			else
			{
				currentValue = requestedValue.Value;
			}
		}
	}

	private static void ApplyCampaignStartSetting(ref float currentValue, float requestedValue, string name)
	{
		if (!(Math.Abs(requestedValue - currentValue) < 0.0001f))
		{
			if (_campaignSessionStarted)
			{
				Diagnostics.Info(name + " change ignored after campaign session start; this save's campaign profile owns it.");
			}
			else
			{
				currentValue = requestedValue;
			}
		}
	}

	private static void ApplyCampaignStartSetting(ref int currentValue, int requestedValue, string name)
	{
		if (requestedValue != currentValue)
		{
			if (_campaignSessionStarted)
			{
				Diagnostics.Info(name + " change ignored after campaign session start; this save's campaign profile owns it.");
			}
			else
			{
				currentValue = requestedValue;
			}
		}
	}

	private static bool ReadBoolean(XmlElement root, string name, bool fallback)
	{
		if (!bool.TryParse(root.GetAttribute(name), out var value))
		{
			return fallback;
		}
		return value;
	}

	private static float ReadFloat(XmlElement root, string name, float fallback)
	{
		if (!float.TryParse(root.GetAttribute(name), NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
		{
			return fallback;
		}
		if (!IsFinite(value))
		{
			return fallback;
		}
		return value;
	}

	private static float ReadFastForwardSpeed(XmlElement root)
	{
		if (root.HasAttribute("FastForwardSpeedMultiplier"))
		{
			return ReadFloat(root, "FastForwardSpeedMultiplier", 4f);
		}
		if (root.HasAttribute("FastForwardTimeMultiplier"))
		{
			return ConvertLegacyFastForwardPacingToSpeed(ReadFloat(root, "FastForwardTimeMultiplier", 1f));
		}
		return 4f;
	}

	private static float NormalizeLightingHour(float value, float fallback)
	{
		if (!IsFinite(value))
		{
			return fallback;
		}
		float normalized = value % 24f;
		if (normalized < 0f)
		{
			normalized += 24f;
		}
		return normalized;
	}

	private static float NormalizeLightingTransition(float value)
	{
		if (!IsFinite(value))
		{
			return 2f;
		}
		return Math.Max(0.25f, Math.Min(4f, value));
	}

	private static int ClampInt(int value, int minimum, int maximum, int fallback)
	{
		if (value >= minimum && value <= maximum)
		{
			return value;
		}
		return fallback;
	}

	private static float ClampFloat(float value, float minimum, float maximum, float fallback)
	{
		if (IsFinite(value) && !(value < minimum) && !(value > maximum))
		{
			return value;
		}
		return fallback;
	}

	private static int ReadInt(XmlElement root, string name, int fallback)
	{
		if (!int.TryParse(root.GetAttribute(name), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
		{
			return fallback;
		}
		return value;
	}

	private static string[] ReadMonthNames(XmlElement root)
	{
		string[] values = (string[])DefaultMonthNames.Clone();
		for (int i = 0; i < values.Length; i++)
		{
			values[i] = ReadAttribute(root, $"Month{i + 1}Name", values[i]);
		}
		return values;
	}

	private static bool TryParseDelimitedNames(string input, int expectedCount, string label, out string[] values, out string failure)
	{
		values = null;
		if (string.IsNullOrWhiteSpace(input))
		{
			failure = "Enter " + expectedCount + " " + label + " names separated by |.";
			return false;
		}
		string[] parts = input.Split('|');
		if (parts.Length != expectedCount)
		{
			failure = "Enter exactly " + expectedCount + " " + label + " names separated by |.";
			return false;
		}
		string[] parsed = new string[parts.Length];
		for (int index = 0; index < parts.Length; index++)
		{
			string value = ((parts[index] == null) ? string.Empty : parts[index].Trim());
			if (string.IsNullOrWhiteSpace(value))
			{
				failure = "Every " + label + " name must contain text.";
				return false;
			}
			if (value.Length > 24)
			{
				failure = label + " names are limited to " + 24 + " characters.";
				return false;
			}
			parsed[index] = value;
		}
		values = parsed;
		failure = null;
		return true;
	}

	private static string[] ReadSeasonNames(XmlElement root)
	{
		string[] values = (string[])DefaultSeasonNames.Clone();
		for (int i = 0; i < values.Length; i++)
		{
			values[i] = ReadAttribute(root, $"Season{i + 1}Name", values[i]);
		}
		return values;
	}

	private static int[] ReadMonthLengths(XmlElement root)
	{
		int[] values = (int[])DefaultMonthLengths.Clone();
		for (int i = 0; i < values.Length; i++)
		{
			values[i] = Math.Max(1, Math.Min(1000, ReadInt(root, $"Month{i + 1}Days", values[i])));
		}
		return values;
	}

	private static void ApplyMonthNames(string[] values)
	{
		if (values == null || values.Length != _monthNames.Length)
		{
			return;
		}
		for (int i = 0; i < _monthNames.Length; i++)
		{
			if (!string.IsNullOrWhiteSpace(values[i]))
			{
				string name = values[i].Trim();
				_monthNames[i] = ((name.Length > 24) ? name.Substring(0, 24) : name);
			}
		}
	}

	private static void ApplySeasonNames(string[] values)
	{
		if (values == null || values.Length != _seasonNames.Length)
		{
			return;
		}
		for (int i = 0; i < _seasonNames.Length; i++)
		{
			if (!string.IsNullOrWhiteSpace(values[i]))
			{
				string name = values[i].Trim();
				_seasonNames[i] = ((name.Length > 24) ? name.Substring(0, 24) : name);
			}
		}
	}

	private static void ApplyMonthLengths(int[] values)
	{
		if (values == null || values.Length != _monthLengths.Length)
		{
			return;
		}
		int totalDays = 0;
		for (int k = 0; k < values.Length; k++)
		{
			totalDays += Math.Max(1, Math.Min(1000, values[k]));
		}
		if (totalDays != 365)
		{
			Diagnostics.Info("Ignoring custom month lengths totaling " + totalDays + " days; Ages of Calradia requires exactly " + 365 + " common-year days.");
			return;
		}
		if (_campaignSessionStarted)
		{
			for (int j = 0; j < values.Length; j++)
			{
				if (Math.Max(1, Math.Min(1000, values[j])) != _monthLengths[j])
				{
					Diagnostics.Info("Custom month-length change ignored after campaign session start; restart the campaign to apply it.");
					return;
				}
			}
		}
		for (int i = 0; i < _monthLengths.Length; i++)
		{
			_monthLengths[i] = Math.Max(1, Math.Min(1000, values[i]));
		}
		RebuildMonthCache();
	}

	private static void RebuildMonthCache()
	{
		int runningTotal = 0;
		for (int i = 0; i < _monthLengths.Length; i++)
		{
			_monthStarts[i] = runningTotal;
			runningTotal += _monthLengths[i];
		}
		_commonDaysInYear = runningTotal;
	}

	private static bool IsFinite(float value)
	{
		if (!float.IsNaN(value))
		{
			return !float.IsInfinity(value);
		}
		return false;
	}

	private static string NormalizeDateFormat(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return "{Month} {Day} {Year}";
		}
		string normalized = value;
		string[] array = new string[6] { "Month", "Season", "Day", "Year", "MonthNumber", "DayOfYear" };
		foreach (string token in array)
		{
			normalized = Regex.Replace(normalized, "\\{" + token + "\\}", "{" + token + "}", RegexOptions.IgnoreCase);
		}
		return normalized;
	}

	private static float NormalizePacingMultiplier(float value, float fallback)
	{
		if (!IsFinite(value))
		{
			return fallback;
		}
		return Math.Max(1f, Math.Min(4f, value));
	}

	internal static float ConvertLegacyFastForwardPacingToSpeed(float legacyPacing)
	{
		if (!IsFinite(legacyPacing))
		{
			return 4f;
		}
		return NormalizePacingMultiplier(Math.Max(0.1f, Math.Min(10f, legacyPacing)) * 4f, 4f);
	}
}
}

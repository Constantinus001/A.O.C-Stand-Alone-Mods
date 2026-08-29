using System;
using System.Collections.Generic;
using System.Text;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Library;

namespace TwelveMonthCalendar
{

internal sealed class CalendarWorldLedgerVM : ViewModel
{
	private const float StrategicMapViewportWidth = 741f;

	private const float StrategicMapViewportHeight = 427f;

	private const float StrategicMapMinimumZoom = 1f;

	private const float StrategicMapDefaultZoom = 1f;

	private const float StrategicMapMaximumZoom = 5f;

	private const float StrategicMapZoomStep = 0.25f;

	private const int StrategicPartyLegendMinimumHeight = 202;

	private const int StrategicPartyLegendBaseHeight = 112;

	private const int FirstCalendarMonth = 0;

	private const int LastCalendarMonth = 3;

	private const int CalendarFutureMonths = 4;

	private const int CalendarFuturePeriodDays = 21;

	private const int AnnualImportantEventCapacity = 40;

	private const float StrategicMarkerMinimumSeparation = 52f;

	private const float StrategicMarkerEdgePadding = 20f;

	private string _title = "World Events";

	private string _monthTitle = string.Empty;

	private string _monthSummaryTitle = string.Empty;

	private string _monthSummaryText = string.Empty;

	private string _yearSummaryTitle = string.Empty;

	private string _yearSummaryText = string.Empty;

	private string _notesText = "No world events have been recorded yet.";

	private string _notesTitle = "NOTES";

	private string _strategicText = string.Empty;

	private string _characterStoryTitle = string.Empty;

	private string _characterStorySubtitle = string.Empty;

	private string _characterStoryText = string.Empty;

	private string _characterMilestoneText = string.Empty;

	private string _characterKillsText = "0";

	private string _characterKnockoutsText = "0";

	private string _characterRecordNote = string.Empty;

	private CharacterImageIdentifierVM _characterPortrait = new CharacterImageIdentifierVM(null);

	private BannerImageIdentifierVM _characterClanBanner = new BannerImageIdentifierVM(null, nineGrid: true);

	private string _companionsText = string.Empty;

	private bool _hasCompanionRows;

	private string _diplomacyText = string.Empty;

	private string _foreignOfficePeaceText = "0";

	private string _foreignOfficeWarText = "0";

	private string _foreignOfficeIncomeText = "0";

	private string _foreignOfficeRealmText = string.Empty;

	private bool _isDiplomacyRelationsPage = true;

	private bool _isKingdomFinancesPage;

	private string _kingdomFinanceRealmText = string.Empty;

	private string _kingdomTreasuryText = "0";

	private string _kingdomIncomeText = "0 / day";

	private string _kingdomExpensesText = "0 / day";

	private string _kingdomNetText = "0 / day";

	private string _kingdomFinanceStatusText = string.Empty;

	private string _warStatisticsText = string.Empty;

	private string _warActiveWarsText = "0";

	private string _warTroopsFieldedText = "0";

	private string _warLossesText = "0";

	private string _warDiplomacyRecordText = string.Empty;

	private string _warPlayerStatusText = string.Empty;

	private string _warStatisticsFootnote = string.Empty;

	private bool _hasWarStatisticsRows;

	private bool _isStrategicMap;

	private string _strategicMapMode = "Political";

	private bool _isCalendarVisible = true;

	private bool _isCalendarSectionVisible = true;

	private bool _isSummariesVisible;

	private bool _isSavedSummariesPage = true;

	private bool _isMarriagesPage;

	private string _marriagePlayerName = string.Empty;

	private string _marriagePlayerDetails = string.Empty;

	private string _marriagePlayerStatus = string.Empty;

	private string _marriageStatusText = string.Empty;

	private string _marriageSortMode = "All";

	private string _marriageNameSearchText = string.Empty;

	private CharacterImageIdentifierVM _marriagePlayerPortrait = new CharacterImageIdentifierVM(null);

	private bool _isStorySectionVisible;

	private bool _isCharacterStoryVisible;

	private bool _isCompanionsVisible;

	private bool _isDiplomacyVisible;

	private bool _isStrategicSectionVisible;

	private bool _isWarStatisticsVisible;

	private bool _isPointerOverStrategicMap;

	private readonly MBBindingList<CalendarWorldLedgerTabVM> _tabs = new MBBindingList<CalendarWorldLedgerTabVM>();

	private readonly MBBindingList<CalendarWorldCalendarDayVM> _days = new MBBindingList<CalendarWorldCalendarDayVM>();

	private readonly MBBindingList<CalendarWorldCalendarMonthVM> _calendarMonths = new MBBindingList<CalendarWorldCalendarMonthVM>();

	private readonly MBBindingList<CalendarDiplomacyRelationVM> _diplomacyRelations = new MBBindingList<CalendarDiplomacyRelationVM>();

	private readonly MBBindingList<CalendarKingdomFinanceRowVM> _kingdomFinanceRows = new MBBindingList<CalendarKingdomFinanceRowVM>();

	private readonly MBBindingList<CalendarCompanionRecordVM> _companionRows = new MBBindingList<CalendarCompanionRecordVM>();

	private readonly MBBindingList<CalendarWarStatisticsRowVM> _warStatisticsRows = new MBBindingList<CalendarWarStatisticsRowVM>();

	private readonly MBBindingList<CalendarWorldSavedSummaryVM> _savedSummaries = new MBBindingList<CalendarWorldSavedSummaryVM>();

	private readonly MBBindingList<CalendarMarriageCandidateVM> _marriageCandidates = new MBBindingList<CalendarMarriageCandidateVM>();

	private readonly MBBindingList<CalendarWorldStrategicProvinceVM> _strategicProvinces = new MBBindingList<CalendarWorldStrategicProvinceVM>();

	private readonly MBBindingList<CalendarWorldStrategicProvinceVM> _strategicContestedProvinces = new MBBindingList<CalendarWorldStrategicProvinceVM>();

	private readonly MBBindingList<CalendarWorldStrategicMarkerVM> _strategicMarkers = new MBBindingList<CalendarWorldStrategicMarkerVM>();

	private readonly MBBindingList<CalendarStrategicKingdomSummaryVM> _strategicKingdomRows = new MBBindingList<CalendarStrategicKingdomSummaryVM>();

	private readonly MBBindingList<CalendarWorldStrategicVillageVM> _selectedStrategicVillages = new MBBindingList<CalendarWorldStrategicVillageVM>();

	private readonly MBBindingList<CalendarWorldStrategicVillageVM> _trackedStrategicSettlements = new MBBindingList<CalendarWorldStrategicVillageVM>();

	private readonly CalendarWorldStrategicProvinceVM[] _fixedStrategicProvinces = new CalendarWorldStrategicProvinceVM[133];

	private readonly CalendarWorldStrategicProvinceVM[] _fixedStrategicContestedProvinces = new CalendarWorldStrategicProvinceVM[133];

	private readonly Action _close;

	private string _selectedFilter = "All";

	private string _selectedStrategicSettlementId = string.Empty;

	private string _strategicDistanceOriginSettlementId = string.Empty;

	private string _strategicDistanceDestinationSettlementId = string.Empty;

	private long _selectedCalendarDay = long.MinValue;

	private int _displayCalendarYear = int.MinValue;

	private int _displayCalendarMonth = int.MinValue;

	private float _strategicMapZoom = 1f;

	private static readonly Dictionary<string, Vec2> TownReferenceAnchors = new Dictionary<string, Vec2>(StringComparer.Ordinal)
	{
		{
			"town_N1",
			new Vec2(290f, 470.5f)
		},
		{
			"town_N2",
			new Vec2(938.5f, 394.5f)
		},
		{
			"town_N3",
			new Vec2(1349f, 331.5f)
		},
		{
			"town_N4",
			new Vec2(1674f, 439.5f)
		},
		{
			"town_S1",
			new Vec2(930f, 712.5f)
		},
		{
			"town_S2",
			new Vec2(1342f, 512f)
		},
		{
			"town_S3",
			new Vec2(1005.5f, 773f)
		},
		{
			"town_S4",
			new Vec2(1192f, 694f)
		},
		{
			"town_S5",
			new Vec2(1411f, 662.5f)
		},
		{
			"town_S6",
			new Vec2(1125f, 625.5f)
		},
		{
			"town_S7",
			new Vec2(731f, 629.5f)
		},
		{
			"town_V1",
			new Vec2(518f, 1106.5f)
		},
		{
			"town_V2",
			new Vec2(477f, 907.5f)
		},
		{
			"town_V3",
			new Vec2(371f, 913.5f)
		},
		{
			"town_V5",
			new Vec2(273f, 999.5f)
		},
		{
			"town_V6",
			new Vec2(383f, 1078.5f)
		},
		{
			"town_V7",
			new Vec2(499f, 1176.5f)
		},
		{
			"town_V8",
			new Vec2(377f, 750.5f)
		},
		{
			"town_V9",
			new Vec2(450.5f, 804.5f)
		},
		{
			"town_B1",
			new Vec2(663f, 1014.5f)
		},
		{
			"town_B2",
			new Vec2(618f, 909f)
		},
		{
			"town_B3",
			new Vec2(692f, 787.5f)
		},
		{
			"town_B4",
			new Vec2(752f, 955.5f)
		},
		{
			"town_B5",
			new Vec2(549f, 952.5f)
		},
		{
			"town_EN1",
			new Vec2(913f, 931f)
		},
		{
			"town_EN2",
			new Vec2(1075f, 915f)
		},
		{
			"town_EN3",
			new Vec2(1169.5f, 1026.5f)
		},
		{
			"town_EN4",
			new Vec2(1203f, 861.5f)
		},
		{
			"town_EN5",
			new Vec2(1371f, 990.5f)
		},
		{
			"town_EN6",
			new Vec2(1396f, 891.5f)
		},
		{
			"town_EW1",
			new Vec2(677f, 1108.5f)
		},
		{
			"town_EW2",
			new Vec2(919f, 1274f)
		},
		{
			"town_EW3",
			new Vec2(836.5f, 1224.5f)
		},
		{
			"town_EW4",
			new Vec2(680f, 1235.5f)
		},
		{
			"town_EW5",
			new Vec2(975.5f, 1206.5f)
		},
		{
			"town_EW6",
			new Vec2(853f, 1153f)
		},
		{
			"town_ES1",
			new Vec2(1454f, 1373.5f)
		},
		{
			"town_ES2",
			new Vec2(1257.5f, 1341.5f)
		},
		{
			"town_ES3",
			new Vec2(1096f, 1336f)
		},
		{
			"town_ES4",
			new Vec2(1173f, 1211.5f)
		},
		{
			"town_ES5",
			new Vec2(1498f, 1262.5f)
		},
		{
			"town_ES6",
			new Vec2(1342f, 1129.5f)
		},
		{
			"town_ES7",
			new Vec2(1469.5f, 1050.5f)
		},
		{
			"town_K1",
			new Vec2(1670f, 735.5f)
		},
		{
			"town_K2",
			new Vec2(1713f, 1051.5f)
		},
		{
			"town_K3",
			new Vec2(1583.5f, 861.5f)
		},
		{
			"town_K4",
			new Vec2(1709.5f, 904.5f)
		},
		{
			"town_K5",
			new Vec2(1592.5f, 1029.5f)
		},
		{
			"town_K6",
			new Vec2(1690f, 1166.5f)
		},
		{
			"town_A1",
			new Vec2(636f, 1421.5f)
		},
		{
			"town_A2",
			new Vec2(1571f, 1449f)
		},
		{
			"town_A3",
			new Vec2(1049f, 1702.5f)
		},
		{
			"town_A4",
			new Vec2(1375f, 1581f)
		},
		{
			"town_A5",
			new Vec2(1230f, 1741.5f)
		},
		{
			"town_A6",
			new Vec2(842f, 1537f)
		},
		{
			"town_A7",
			new Vec2(794.5f, 1708.5f)
		},
		{
			"town_A8",
			new Vec2(1134f, 1636.5f)
		}
	};

	private static readonly Dictionary<string, Vec2> CastleReferenceAnchors = new Dictionary<string, Vec2>(StringComparer.Ordinal)
	{
		{
			"castle_N1",
			new Vec2(1522.5f, 564f)
		},
		{
			"castle_N2",
			new Vec2(798.5f, 345f)
		},
		{
			"castle_N5",
			new Vec2(1141.5f, 290.5f)
		},
		{
			"castle_N7",
			new Vec2(278.5f, 380f)
		},
		{
			"castle_N8",
			new Vec2(1579.5f, 370.5f)
		},
		{
			"castle_N9",
			new Vec2(1698.5f, 537.5f)
		},
		{
			"castle_S1",
			new Vec2(662f, 607.5f)
		},
		{
			"castle_S2",
			new Vec2(944.5f, 819f)
		},
		{
			"castle_S3",
			new Vec2(827.5f, 830.5f)
		},
		{
			"castle_S4",
			new Vec2(870.5f, 748f)
		},
		{
			"castle_S5",
			new Vec2(1078.5f, 671f)
		},
		{
			"castle_S6",
			new Vec2(1279.5f, 616.5f)
		},
		{
			"castle_S7",
			new Vec2(1449.5f, 738f)
		},
		{
			"castle_S8",
			new Vec2(1420.5f, 607.5f)
		},
		{
			"castle_V1",
			new Vec2(432.5f, 1128.5f)
		},
		{
			"castle_V2",
			new Vec2(341.5f, 1021.5f)
		},
		{
			"castle_V3",
			new Vec2(293.5f, 862.5f)
		},
		{
			"castle_V4",
			new Vec2(501.5f, 826.5f)
		},
		{
			"castle_V5",
			new Vec2(423.5f, 726.5f)
		},
		{
			"castle_V6",
			new Vec2(404.5f, 873.5f)
		},
		{
			"castle_V7",
			new Vec2(469.5f, 1056.5f)
		},
		{
			"castle_V8",
			new Vec2(481.5f, 970f)
		},
		{
			"castle_B1",
			new Vec2(542.5f, 1044.5f)
		},
		{
			"castle_B4",
			new Vec2(736f, 868.5f)
		},
		{
			"castle_B5",
			new Vec2(745.5f, 1030.5f)
		},
		{
			"castle_B6",
			new Vec2(624.5f, 866.5f)
		},
		{
			"castle_B7",
			new Vec2(568.5f, 809.5f)
		},
		{
			"castle_B8",
			new Vec2(795.5f, 898.5f)
		},
		{
			"castle_EN1",
			new Vec2(1093.5f, 1076f)
		},
		{
			"castle_EN2",
			new Vec2(1439.5f, 793.5f)
		},
		{
			"castle_EN3",
			new Vec2(1032.5f, 995f)
		},
		{
			"castle_EN4",
			new Vec2(1325.5f, 999f)
		},
		{
			"castle_EN6",
			new Vec2(946.5f, 1074f)
		},
		{
			"castle_EN7",
			new Vec2(1498f, 794.5f)
		},
		{
			"castle_EN8",
			new Vec2(1157.5f, 1128.5f)
		},
		{
			"castle_EN9",
			new Vec2(874.5f, 973.5f)
		},
		{
			"castle_EW1",
			new Vec2(592.5f, 1297.5f)
		},
		{
			"castle_EW2",
			new Vec2(931.5f, 1218f)
		},
		{
			"castle_EW3",
			new Vec2(990.5f, 1326f)
		},
		{
			"castle_EW4",
			new Vec2(597.5f, 1166.5f)
		},
		{
			"castle_EW5",
			new Vec2(601.5f, 1096.5f)
		},
		{
			"castle_EW6",
			new Vec2(743.5f, 1114.5f)
		},
		{
			"castle_EW7",
			new Vec2(789.5f, 1329f)
		},
		{
			"castle_EW8",
			new Vec2(842.5f, 1009.5f)
		},
		{
			"castle_ES1",
			new Vec2(1514.5f, 1412.5f)
		},
		{
			"castle_ES2",
			new Vec2(1468.5f, 1150.5f)
		},
		{
			"castle_ES3",
			new Vec2(1362.5f, 1089.5f)
		},
		{
			"castle_ES4",
			new Vec2(1369.5f, 1377.5f)
		},
		{
			"castle_ES5",
			new Vec2(1322.5f, 1254f)
		},
		{
			"castle_ES6",
			new Vec2(1121.5f, 1218f)
		},
		{
			"castle_ES7",
			new Vec2(1470.5f, 1002f)
		},
		{
			"castle_ES8",
			new Vec2(1243.5f, 1108f)
		},
		{
			"castle_K1",
			new Vec2(1629.5f, 879.5f)
		},
		{
			"castle_K2",
			new Vec2(1589.5f, 1179f)
		},
		{
			"castle_K3",
			new Vec2(1655.5f, 963f)
		},
		{
			"castle_K5",
			new Vec2(1706.5f, 844.5f)
		},
		{
			"castle_K6",
			new Vec2(1653.5f, 651.5f)
		},
		{
			"castle_K7",
			new Vec2(1618.5f, 1052.5f)
		},
		{
			"castle_K8",
			new Vec2(1646.5f, 1224.5f)
		},
		{
			"castle_K9",
			new Vec2(1570.5f, 715.5f)
		},
		{
			"castle_A2",
			new Vec2(1414.5f, 1644.5f)
		},
		{
			"castle_A3",
			new Vec2(806.5f, 1610.5f)
		},
		{
			"castle_A4",
			new Vec2(994.5f, 1635.5f)
		},
		{
			"castle_A6",
			new Vec2(1563.5f, 1383.5f)
		},
		{
			"castle_A7",
			new Vec2(706f, 1501f)
		},
		{
			"castle_A8",
			new Vec2(1496.5f, 1533.5f)
		}
	};

	private static readonly Dictionary<string, Vec2> ProjectedCastleReferenceAnchors = new Dictionary<string, Vec2>(StringComparer.Ordinal)
	{
		{
			"castle_A1",
			new Vec2(572.6f, 1506.8f)
		},
		{
			"castle_A5",
			new Vec2(1147f, 1686.2f)
		},
		{
			"castle_A9",
			new Vec2(721f, 1628.6f)
		},
		{
			"castle_B2",
			new Vec2(594.9f, 1015.3f)
		},
		{
			"castle_B3",
			new Vec2(547.1f, 857.3f)
		},
		{
			"castle_EN5",
			new Vec2(1204.5f, 929.4f)
		},
		{
			"castle_K4",
			new Vec2(1613.1f, 815.8f)
		},
		{
			"castle_N3",
			new Vec2(931.1f, 167.1f)
		},
		{
			"castle_N4",
			new Vec2(1405.2f, 181f)
		},
		{
			"castle_N6",
			new Vec2(1612.5f, 274.1f)
		}
	};

	private static readonly string[] FixedStrategicProvincePropertyNames = new string[266]
	{
		"StrategicProvince001", "StrategicContestedProvince001", "StrategicProvince002", "StrategicContestedProvince002", "StrategicProvince003", "StrategicContestedProvince003", "StrategicProvince004", "StrategicContestedProvince004", "StrategicProvince005", "StrategicContestedProvince005",
		"StrategicProvince006", "StrategicContestedProvince006", "StrategicProvince007", "StrategicContestedProvince007", "StrategicProvince008", "StrategicContestedProvince008", "StrategicProvince009", "StrategicContestedProvince009", "StrategicProvince010", "StrategicContestedProvince010",
		"StrategicProvince011", "StrategicContestedProvince011", "StrategicProvince012", "StrategicContestedProvince012", "StrategicProvince013", "StrategicContestedProvince013", "StrategicProvince014", "StrategicContestedProvince014", "StrategicProvince015", "StrategicContestedProvince015",
		"StrategicProvince016", "StrategicContestedProvince016", "StrategicProvince017", "StrategicContestedProvince017", "StrategicProvince018", "StrategicContestedProvince018", "StrategicProvince019", "StrategicContestedProvince019", "StrategicProvince020", "StrategicContestedProvince020",
		"StrategicProvince021", "StrategicContestedProvince021", "StrategicProvince022", "StrategicContestedProvince022", "StrategicProvince023", "StrategicContestedProvince023", "StrategicProvince024", "StrategicContestedProvince024", "StrategicProvince025", "StrategicContestedProvince025",
		"StrategicProvince026", "StrategicContestedProvince026", "StrategicProvince027", "StrategicContestedProvince027", "StrategicProvince028", "StrategicContestedProvince028", "StrategicProvince029", "StrategicContestedProvince029", "StrategicProvince030", "StrategicContestedProvince030",
		"StrategicProvince031", "StrategicContestedProvince031", "StrategicProvince032", "StrategicContestedProvince032", "StrategicProvince033", "StrategicContestedProvince033", "StrategicProvince034", "StrategicContestedProvince034", "StrategicProvince035", "StrategicContestedProvince035",
		"StrategicProvince036", "StrategicContestedProvince036", "StrategicProvince037", "StrategicContestedProvince037", "StrategicProvince038", "StrategicContestedProvince038", "StrategicProvince039", "StrategicContestedProvince039", "StrategicProvince040", "StrategicContestedProvince040",
		"StrategicProvince041", "StrategicContestedProvince041", "StrategicProvince042", "StrategicContestedProvince042", "StrategicProvince043", "StrategicContestedProvince043", "StrategicProvince044", "StrategicContestedProvince044", "StrategicProvince045", "StrategicContestedProvince045",
		"StrategicProvince046", "StrategicContestedProvince046", "StrategicProvince047", "StrategicContestedProvince047", "StrategicProvince048", "StrategicContestedProvince048", "StrategicProvince049", "StrategicContestedProvince049", "StrategicProvince050", "StrategicContestedProvince050",
		"StrategicProvince051", "StrategicContestedProvince051", "StrategicProvince052", "StrategicContestedProvince052", "StrategicProvince053", "StrategicContestedProvince053", "StrategicProvince054", "StrategicContestedProvince054", "StrategicProvince055", "StrategicContestedProvince055",
		"StrategicProvince056", "StrategicContestedProvince056", "StrategicProvince057", "StrategicContestedProvince057", "StrategicProvince058", "StrategicContestedProvince058", "StrategicProvince059", "StrategicContestedProvince059", "StrategicProvince060", "StrategicContestedProvince060",
		"StrategicProvince061", "StrategicContestedProvince061", "StrategicProvince062", "StrategicContestedProvince062", "StrategicProvince063", "StrategicContestedProvince063", "StrategicProvince064", "StrategicContestedProvince064", "StrategicProvince065", "StrategicContestedProvince065",
		"StrategicProvince066", "StrategicContestedProvince066", "StrategicProvince067", "StrategicContestedProvince067", "StrategicProvince068", "StrategicContestedProvince068", "StrategicProvince069", "StrategicContestedProvince069", "StrategicProvince070", "StrategicContestedProvince070",
		"StrategicProvince071", "StrategicContestedProvince071", "StrategicProvince072", "StrategicContestedProvince072", "StrategicProvince073", "StrategicContestedProvince073", "StrategicProvince074", "StrategicContestedProvince074", "StrategicProvince075", "StrategicContestedProvince075",
		"StrategicProvince076", "StrategicContestedProvince076", "StrategicProvince077", "StrategicContestedProvince077", "StrategicProvince078", "StrategicContestedProvince078", "StrategicProvince079", "StrategicContestedProvince079", "StrategicProvince080", "StrategicContestedProvince080",
		"StrategicProvince081", "StrategicContestedProvince081", "StrategicProvince082", "StrategicContestedProvince082", "StrategicProvince083", "StrategicContestedProvince083", "StrategicProvince084", "StrategicContestedProvince084", "StrategicProvince085", "StrategicContestedProvince085",
		"StrategicProvince086", "StrategicContestedProvince086", "StrategicProvince087", "StrategicContestedProvince087", "StrategicProvince088", "StrategicContestedProvince088", "StrategicProvince089", "StrategicContestedProvince089", "StrategicProvince090", "StrategicContestedProvince090",
		"StrategicProvince091", "StrategicContestedProvince091", "StrategicProvince092", "StrategicContestedProvince092", "StrategicProvince093", "StrategicContestedProvince093", "StrategicProvince094", "StrategicContestedProvince094", "StrategicProvince095", "StrategicContestedProvince095",
		"StrategicProvince096", "StrategicContestedProvince096", "StrategicProvince097", "StrategicContestedProvince097", "StrategicProvince098", "StrategicContestedProvince098", "StrategicProvince099", "StrategicContestedProvince099", "StrategicProvince100", "StrategicContestedProvince100",
		"StrategicProvince101", "StrategicContestedProvince101", "StrategicProvince102", "StrategicContestedProvince102", "StrategicProvince103", "StrategicContestedProvince103", "StrategicProvince104", "StrategicContestedProvince104", "StrategicProvince105", "StrategicContestedProvince105",
		"StrategicProvince106", "StrategicContestedProvince106", "StrategicProvince107", "StrategicContestedProvince107", "StrategicProvince108", "StrategicContestedProvince108", "StrategicProvince109", "StrategicContestedProvince109", "StrategicProvince110", "StrategicContestedProvince110",
		"StrategicProvince111", "StrategicContestedProvince111", "StrategicProvince112", "StrategicContestedProvince112", "StrategicProvince113", "StrategicContestedProvince113", "StrategicProvince114", "StrategicContestedProvince114", "StrategicProvince115", "StrategicContestedProvince115",
		"StrategicProvince116", "StrategicContestedProvince116", "StrategicProvince117", "StrategicContestedProvince117", "StrategicProvince118", "StrategicContestedProvince118", "StrategicProvince119", "StrategicContestedProvince119", "StrategicProvince120", "StrategicContestedProvince120",
		"StrategicProvince121", "StrategicContestedProvince121", "StrategicProvince122", "StrategicContestedProvince122", "StrategicProvince123", "StrategicContestedProvince123", "StrategicProvince124", "StrategicContestedProvince124", "StrategicProvince125", "StrategicContestedProvince125",
		"StrategicProvince126", "StrategicContestedProvince126", "StrategicProvince127", "StrategicContestedProvince127", "StrategicProvince128", "StrategicContestedProvince128", "StrategicProvince129", "StrategicContestedProvince129", "StrategicProvince130", "StrategicContestedProvince130",
		"StrategicProvince131", "StrategicContestedProvince131", "StrategicProvince132", "StrategicContestedProvince132", "StrategicProvince133", "StrategicContestedProvince133"
	};

	private const float StrategicKingdomClusterLinkDistance = 340f;

	private readonly MBBindingList<CalendarStrategicKingdomLabelVM> _strategicKingdomLabels = new MBBindingList<CalendarStrategicKingdomLabelVM>();

	private readonly MBBindingList<CalendarStrategicFriendlyArmyVM> _strategicFriendlyArmies = new MBBindingList<CalendarStrategicFriendlyArmyVM>();

	[DataSourceProperty]
	public string Title => _title;

	[DataSourceProperty]
	public string MonthTitle
	{
		get
		{
			return _monthTitle;
		}
		private set
		{
			if (!(_monthTitle == value))
			{
				_monthTitle = value ?? string.Empty;
				OnPropertyChangedWithValue(_monthTitle, "MonthTitle");
			}
		}
	}

	[DataSourceProperty]
	public string MonthSummaryTitle
	{
		get
		{
			return _monthSummaryTitle;
		}
		private set
		{
			if (!(_monthSummaryTitle == value))
			{
				_monthSummaryTitle = value ?? string.Empty;
				OnPropertyChangedWithValue(_monthSummaryTitle, "MonthSummaryTitle");
			}
		}
	}

	[DataSourceProperty]
	public string MonthSummaryText
	{
		get
		{
			return _monthSummaryText;
		}
		private set
		{
			if (!(_monthSummaryText == value))
			{
				_monthSummaryText = value ?? string.Empty;
				OnPropertyChangedWithValue(_monthSummaryText, "MonthSummaryText");
			}
		}
	}

	[DataSourceProperty]
	public string YearSummaryTitle
	{
		get
		{
			return _yearSummaryTitle;
		}
		private set
		{
			if (!(_yearSummaryTitle == value))
			{
				_yearSummaryTitle = value ?? string.Empty;
				OnPropertyChangedWithValue(_yearSummaryTitle, "YearSummaryTitle");
			}
		}
	}

	[DataSourceProperty]
	public string YearSummaryText
	{
		get
		{
			return _yearSummaryText;
		}
		private set
		{
			if (!(_yearSummaryText == value))
			{
				_yearSummaryText = value ?? string.Empty;
				OnPropertyChangedWithValue(_yearSummaryText, "YearSummaryText");
			}
		}
	}

	[DataSourceProperty]
	public string NotesText
	{
		get
		{
			return _notesText;
		}
		private set
		{
			if (!(_notesText == value))
			{
				_notesText = value ?? string.Empty;
				OnPropertyChangedWithValue(_notesText, "NotesText");
			}
		}
	}

	[DataSourceProperty]
	public string NotesTitle
	{
		get
		{
			return _notesTitle;
		}
		private set
		{
			if (!(_notesTitle == value))
			{
				_notesTitle = value ?? string.Empty;
				OnPropertyChangedWithValue(_notesTitle, "NotesTitle");
			}
		}
	}

	[DataSourceProperty]
	public string StrategicText
	{
		get
		{
			return _strategicText;
		}
		private set
		{
			if (!(_strategicText == value))
			{
				_strategicText = value ?? string.Empty;
				OnPropertyChangedWithValue(_strategicText, "StrategicText");
			}
		}
	}

	[DataSourceProperty]
	public string CharacterStoryTitle
	{
		get
		{
			return _characterStoryTitle;
		}
		private set
		{
			if (!(_characterStoryTitle == value))
			{
				_characterStoryTitle = value ?? string.Empty;
				OnPropertyChangedWithValue(_characterStoryTitle, "CharacterStoryTitle");
			}
		}
	}

	[DataSourceProperty]
	public string CharacterStorySubtitle
	{
		get
		{
			return _characterStorySubtitle;
		}
		private set
		{
			if (!(_characterStorySubtitle == value))
			{
				_characterStorySubtitle = value ?? string.Empty;
				OnPropertyChangedWithValue(_characterStorySubtitle, "CharacterStorySubtitle");
			}
		}
	}

	[DataSourceProperty]
	public string CharacterStoryText
	{
		get
		{
			return _characterStoryText;
		}
		private set
		{
			if (!(_characterStoryText == value))
			{
				_characterStoryText = value ?? string.Empty;
				OnPropertyChangedWithValue(_characterStoryText, "CharacterStoryText");
			}
		}
	}

	[DataSourceProperty]
	public string CharacterMilestoneText
	{
		get
		{
			return _characterMilestoneText;
		}
		private set
		{
			if (!(_characterMilestoneText == value))
			{
				_characterMilestoneText = value ?? string.Empty;
				OnPropertyChangedWithValue(_characterMilestoneText, "CharacterMilestoneText");
			}
		}
	}

	[DataSourceProperty]
	public string CharacterKillsText
	{
		get
		{
			return _characterKillsText;
		}
		private set
		{
			if (!(_characterKillsText == value))
			{
				_characterKillsText = value ?? "0";
				OnPropertyChangedWithValue(_characterKillsText, "CharacterKillsText");
			}
		}
	}

	[DataSourceProperty]
	public string CharacterKnockoutsText
	{
		get
		{
			return _characterKnockoutsText;
		}
		private set
		{
			if (!(_characterKnockoutsText == value))
			{
				_characterKnockoutsText = value ?? "0";
				OnPropertyChangedWithValue(_characterKnockoutsText, "CharacterKnockoutsText");
			}
		}
	}

	[DataSourceProperty]
	public string CharacterRecordNote
	{
		get
		{
			return _characterRecordNote;
		}
		private set
		{
			if (!(_characterRecordNote == value))
			{
				_characterRecordNote = value ?? string.Empty;
				OnPropertyChangedWithValue(_characterRecordNote, "CharacterRecordNote");
			}
		}
	}

	[DataSourceProperty]
	public CharacterImageIdentifierVM CharacterPortrait
	{
		get
		{
			return _characterPortrait;
		}
		private set
		{
			if (_characterPortrait != value)
			{
				_characterPortrait = value ?? new CharacterImageIdentifierVM(null);
				OnPropertyChangedWithValue(_characterPortrait, "CharacterPortrait");
			}
		}
	}

	[DataSourceProperty]
	public BannerImageIdentifierVM CharacterClanBanner
	{
		get
		{
			return _characterClanBanner;
		}
		private set
		{
			if (_characterClanBanner != value)
			{
				_characterClanBanner = value ?? new BannerImageIdentifierVM(null, nineGrid: true);
				OnPropertyChangedWithValue(_characterClanBanner, "CharacterClanBanner");
			}
		}
	}

	[DataSourceProperty]
	public string CompanionsText
	{
		get
		{
			return _companionsText;
		}
		private set
		{
			if (!(_companionsText == value))
			{
				_companionsText = value ?? string.Empty;
				OnPropertyChangedWithValue(_companionsText, "CompanionsText");
			}
		}
	}

	[DataSourceProperty]
	public bool HasCompanionRows
	{
		get
		{
			return _hasCompanionRows;
		}
		private set
		{
			if (_hasCompanionRows != value)
			{
				_hasCompanionRows = value;
				OnPropertyChangedWithValue(value, "HasCompanionRows");
				OnPropertyChangedWithValue(!value, "ShowCompanionEmptyState");
			}
		}
	}

	[DataSourceProperty]
	public bool ShowCompanionEmptyState => !_hasCompanionRows;

	[DataSourceProperty]
	public MBBindingList<CalendarCompanionRecordVM> CompanionRows => _companionRows;

	[DataSourceProperty]
	public string DiplomacyText
	{
		get
		{
			return _diplomacyText;
		}
		private set
		{
			if (!(_diplomacyText == value))
			{
				_diplomacyText = value ?? string.Empty;
				OnPropertyChangedWithValue(_diplomacyText, "DiplomacyText");
			}
		}
	}

	[DataSourceProperty]
	public string ForeignOfficePeaceText
	{
		get
		{
			return _foreignOfficePeaceText;
		}
		private set
		{
			if (!(_foreignOfficePeaceText == value))
			{
				_foreignOfficePeaceText = value ?? "0";
				OnPropertyChangedWithValue(_foreignOfficePeaceText, "ForeignOfficePeaceText");
			}
		}
	}

	[DataSourceProperty]
	public string ForeignOfficeWarText
	{
		get
		{
			return _foreignOfficeWarText;
		}
		private set
		{
			if (!(_foreignOfficeWarText == value))
			{
				_foreignOfficeWarText = value ?? "0";
				OnPropertyChangedWithValue(_foreignOfficeWarText, "ForeignOfficeWarText");
			}
		}
	}

	[DataSourceProperty]
	public string ForeignOfficeIncomeText
	{
		get
		{
			return _foreignOfficeIncomeText;
		}
		private set
		{
			if (!(_foreignOfficeIncomeText == value))
			{
				_foreignOfficeIncomeText = value ?? "0";
				OnPropertyChangedWithValue(_foreignOfficeIncomeText, "ForeignOfficeIncomeText");
			}
		}
	}

	[DataSourceProperty]
	public string ForeignOfficeRealmText
	{
		get
		{
			return _foreignOfficeRealmText;
		}
		private set
		{
			if (!(_foreignOfficeRealmText == value))
			{
				_foreignOfficeRealmText = value ?? string.Empty;
				OnPropertyChangedWithValue(_foreignOfficeRealmText, "ForeignOfficeRealmText");
			}
		}
	}

	[DataSourceProperty]
	public bool IsDiplomacyRelationsPage
	{
		get
		{
			return _isDiplomacyRelationsPage;
		}
		private set
		{
			if (_isDiplomacyRelationsPage != value)
			{
				_isDiplomacyRelationsPage = value;
				OnPropertyChangedWithValue(value, "IsDiplomacyRelationsPage");
			}
		}
	}

	[DataSourceProperty]
	public bool IsKingdomFinancesPage
	{
		get
		{
			return _isKingdomFinancesPage;
		}
		private set
		{
			if (_isKingdomFinancesPage != value)
			{
				_isKingdomFinancesPage = value;
				OnPropertyChangedWithValue(value, "IsKingdomFinancesPage");
				NotifyKingdomFinanceRowStateChanged();
			}
		}
	}

	[DataSourceProperty]
	public string KingdomFinanceRealmText
	{
		get
		{
			return _kingdomFinanceRealmText;
		}
		private set
		{
			if (!(_kingdomFinanceRealmText == value))
			{
				_kingdomFinanceRealmText = value ?? string.Empty;
				OnPropertyChangedWithValue(_kingdomFinanceRealmText, "KingdomFinanceRealmText");
			}
		}
	}

	[DataSourceProperty]
	public string KingdomTreasuryText
	{
		get
		{
			return _kingdomTreasuryText;
		}
		private set
		{
			if (!(_kingdomTreasuryText == value))
			{
				_kingdomTreasuryText = value ?? "0";
				OnPropertyChangedWithValue(_kingdomTreasuryText, "KingdomTreasuryText");
			}
		}
	}

	[DataSourceProperty]
	public string KingdomIncomeText
	{
		get
		{
			return _kingdomIncomeText;
		}
		private set
		{
			if (!(_kingdomIncomeText == value))
			{
				_kingdomIncomeText = value ?? "0 / day";
				OnPropertyChangedWithValue(_kingdomIncomeText, "KingdomIncomeText");
			}
		}
	}

	[DataSourceProperty]
	public string KingdomExpensesText
	{
		get
		{
			return _kingdomExpensesText;
		}
		private set
		{
			if (!(_kingdomExpensesText == value))
			{
				_kingdomExpensesText = value ?? "0 / day";
				OnPropertyChangedWithValue(_kingdomExpensesText, "KingdomExpensesText");
			}
		}
	}

	[DataSourceProperty]
	public string KingdomNetText
	{
		get
		{
			return _kingdomNetText;
		}
		private set
		{
			if (!(_kingdomNetText == value))
			{
				_kingdomNetText = value ?? "0 / day";
				OnPropertyChangedWithValue(_kingdomNetText, "KingdomNetText");
			}
		}
	}

	[DataSourceProperty]
	public string KingdomFinanceStatusText
	{
		get
		{
			return _kingdomFinanceStatusText;
		}
		private set
		{
			if (!(_kingdomFinanceStatusText == value))
			{
				_kingdomFinanceStatusText = value ?? string.Empty;
				OnPropertyChangedWithValue(_kingdomFinanceStatusText, "KingdomFinanceStatusText");
			}
		}
	}

	[DataSourceProperty]
	public string WarStatisticsText
	{
		get
		{
			return _warStatisticsText;
		}
		private set
		{
			if (!(_warStatisticsText == value))
			{
				_warStatisticsText = value ?? string.Empty;
				OnPropertyChangedWithValue(_warStatisticsText, "WarStatisticsText");
			}
		}
	}

	[DataSourceProperty]
	public string WarActiveWarsText
	{
		get
		{
			return _warActiveWarsText;
		}
		private set
		{
			if (!(_warActiveWarsText == value))
			{
				_warActiveWarsText = value ?? string.Empty;
				OnPropertyChangedWithValue(_warActiveWarsText, "WarActiveWarsText");
			}
		}
	}

	[DataSourceProperty]
	public string WarTroopsFieldedText
	{
		get
		{
			return _warTroopsFieldedText;
		}
		private set
		{
			if (!(_warTroopsFieldedText == value))
			{
				_warTroopsFieldedText = value ?? string.Empty;
				OnPropertyChangedWithValue(_warTroopsFieldedText, "WarTroopsFieldedText");
			}
		}
	}

	[DataSourceProperty]
	public string WarLossesText
	{
		get
		{
			return _warLossesText;
		}
		private set
		{
			if (!(_warLossesText == value))
			{
				_warLossesText = value ?? string.Empty;
				OnPropertyChangedWithValue(_warLossesText, "WarLossesText");
			}
		}
	}

	[DataSourceProperty]
	public string WarDiplomacyRecordText
	{
		get
		{
			return _warDiplomacyRecordText;
		}
		private set
		{
			if (!(_warDiplomacyRecordText == value))
			{
				_warDiplomacyRecordText = value ?? string.Empty;
				OnPropertyChangedWithValue(_warDiplomacyRecordText, "WarDiplomacyRecordText");
			}
		}
	}

	[DataSourceProperty]
	public string WarPlayerStatusText
	{
		get
		{
			return _warPlayerStatusText;
		}
		private set
		{
			if (!(_warPlayerStatusText == value))
			{
				_warPlayerStatusText = value ?? string.Empty;
				OnPropertyChangedWithValue(_warPlayerStatusText, "WarPlayerStatusText");
			}
		}
	}

	[DataSourceProperty]
	public string WarStatisticsFootnote
	{
		get
		{
			return _warStatisticsFootnote;
		}
		private set
		{
			if (!(_warStatisticsFootnote == value))
			{
				_warStatisticsFootnote = value ?? string.Empty;
				OnPropertyChangedWithValue(_warStatisticsFootnote, "WarStatisticsFootnote");
			}
		}
	}

	[DataSourceProperty]
	public bool HasWarStatisticsRows
	{
		get
		{
			return _hasWarStatisticsRows;
		}
		private set
		{
			if (_hasWarStatisticsRows != value)
			{
				_hasWarStatisticsRows = value;
				OnPropertyChangedWithValue(value, "HasWarStatisticsRows");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<CalendarWarStatisticsRowVM> WarStatisticsRows => _warStatisticsRows;

	[DataSourceProperty]
	public bool IsStrategicMap
	{
		get
		{
			return _isStrategicMap;
		}
		private set
		{
			if (_isStrategicMap != value)
			{
				_isStrategicMap = value;
				OnPropertyChangedWithValue(value, "IsStrategicMap");
			}
		}
	}

	[DataSourceProperty]
	public bool IsPoliticalMapMode => string.Equals(_strategicMapMode, "Political", StringComparison.Ordinal);

	[DataSourceProperty]
	public bool IsCultureMapMode => string.Equals(_strategicMapMode, "Culture", StringComparison.Ordinal);

	[DataSourceProperty]
	public bool IsReligionMapMode => string.Equals(_strategicMapMode, "Religion", StringComparison.Ordinal);

	[DataSourceProperty]
	public bool IsPopulationMapMode => string.Equals(_strategicMapMode, "Population", StringComparison.Ordinal);

	[DataSourceProperty]
	public string StrategicMapModeTitle => _strategicMapMode.ToUpperInvariant() + " MAP";

	[DataSourceProperty]
	public string StrategicMapModeLegendText
	{
		get
		{
			if (IsCultureMapMode)
			{
				return "Province colour = local culture";
			}
			if (IsReligionMapMode)
			{
				return "Province colour = dominant religion";
			}
			if (IsPopulationMapMode)
			{
				return "Population: blue = low, red = high";
			}
			return "Province colour = current owner";
		}
	}

	[DataSourceProperty]
	public bool IsCalendarVisible
	{
		get
		{
			return _isCalendarVisible;
		}
		private set
		{
			if (_isCalendarVisible != value)
			{
				_isCalendarVisible = value;
				OnPropertyChangedWithValue(value, "IsCalendarVisible");
			}
		}
	}

	[DataSourceProperty]
	public bool IsCalendarSectionVisible
	{
		get
		{
			return _isCalendarSectionVisible;
		}
		private set
		{
			if (_isCalendarSectionVisible != value)
			{
				_isCalendarSectionVisible = value;
				OnPropertyChangedWithValue(value, "IsCalendarSectionVisible");
			}
		}
	}

	[DataSourceProperty]
	public bool IsSummariesVisible
	{
		get
		{
			return _isSummariesVisible;
		}
		private set
		{
			if (_isSummariesVisible != value)
			{
				_isSummariesVisible = value;
				OnPropertyChangedWithValue(value, "IsSummariesVisible");
			}
		}
	}

	[DataSourceProperty]
	public bool IsSavedSummariesPage
	{
		get
		{
			return _isSavedSummariesPage;
		}
		private set
		{
			if (_isSavedSummariesPage != value)
			{
				_isSavedSummariesPage = value;
				OnPropertyChangedWithValue(value, "IsSavedSummariesPage");
			}
		}
	}

	[DataSourceProperty]
	public bool IsMarriagesPage
	{
		get
		{
			return _isMarriagesPage;
		}
		private set
		{
			if (_isMarriagesPage != value)
			{
				_isMarriagesPage = value;
				OnPropertyChangedWithValue(value, "IsMarriagesPage");
				OnPropertyChangedWithValue(IsRealmLedgerVisible, "IsRealmLedgerVisible");
			}
		}
	}

	[DataSourceProperty]
	public string MarriagePlayerName
	{
		get
		{
			return _marriagePlayerName;
		}
		private set
		{
			if (!(_marriagePlayerName == value))
			{
				_marriagePlayerName = value ?? string.Empty;
				OnPropertyChangedWithValue(_marriagePlayerName, "MarriagePlayerName");
			}
		}
	}

	[DataSourceProperty]
	public string MarriagePlayerDetails
	{
		get
		{
			return _marriagePlayerDetails;
		}
		private set
		{
			if (!(_marriagePlayerDetails == value))
			{
				_marriagePlayerDetails = value ?? string.Empty;
				OnPropertyChangedWithValue(_marriagePlayerDetails, "MarriagePlayerDetails");
			}
		}
	}

	[DataSourceProperty]
	public string MarriagePlayerStatus
	{
		get
		{
			return _marriagePlayerStatus;
		}
		private set
		{
			if (!(_marriagePlayerStatus == value))
			{
				_marriagePlayerStatus = value ?? string.Empty;
				OnPropertyChangedWithValue(_marriagePlayerStatus, "MarriagePlayerStatus");
			}
		}
	}

	[DataSourceProperty]
	public string MarriageStatusText
	{
		get
		{
			return _marriageStatusText;
		}
		private set
		{
			if (!(_marriageStatusText == value))
			{
				_marriageStatusText = value ?? string.Empty;
				OnPropertyChangedWithValue(_marriageStatusText, "MarriageStatusText");
			}
		}
	}

	[DataSourceProperty]
	public string MarriageNameSearchText
	{
		get
		{
			return _marriageNameSearchText;
		}
		set
		{
			string normalized = value ?? string.Empty;
			if (!string.Equals(_marriageNameSearchText, normalized, StringComparison.Ordinal))
			{
				_marriageNameSearchText = normalized;
				OnPropertyChangedWithValue(_marriageNameSearchText, "MarriageNameSearchText");
				RefreshMarriages();
			}
		}
	}

	[DataSourceProperty]
	public bool IsMarriageSortName => string.Equals(_marriageSortMode, "Name", StringComparison.Ordinal);

	[DataSourceProperty]
	public bool IsMarriageSortAge => string.Equals(_marriageSortMode, "Age", StringComparison.Ordinal);

	[DataSourceProperty]
	public bool IsMarriageSortKingdom => string.Equals(_marriageSortMode, "Kingdom", StringComparison.Ordinal);

	[DataSourceProperty]
	public bool IsMarriageSortAll => string.Equals(_marriageSortMode, "All", StringComparison.Ordinal);

	[DataSourceProperty]
	public CharacterImageIdentifierVM MarriagePlayerPortrait
	{
		get
		{
			return _marriagePlayerPortrait;
		}
		private set
		{
			if (_marriagePlayerPortrait != value)
			{
				_marriagePlayerPortrait = value ?? new CharacterImageIdentifierVM(null);
				OnPropertyChangedWithValue(_marriagePlayerPortrait, "MarriagePlayerPortrait");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<CalendarMarriageCandidateVM> MarriageCandidates => _marriageCandidates;

	[DataSourceProperty]
	public bool HasMarriageCandidates => _marriageCandidates.Count > 0;

	[DataSourceProperty]
	public bool ShowMarriageEmptyState => _marriageCandidates.Count == 0;

	[DataSourceProperty]
	public bool IsStorySectionVisible
	{
		get
		{
			return _isStorySectionVisible;
		}
		private set
		{
			if (_isStorySectionVisible != value)
			{
				_isStorySectionVisible = value;
				OnPropertyChangedWithValue(value, "IsStorySectionVisible");
			}
		}
	}

	[DataSourceProperty]
	public bool IsCharacterStoryVisible
	{
		get
		{
			return _isCharacterStoryVisible;
		}
		private set
		{
			if (_isCharacterStoryVisible != value)
			{
				_isCharacterStoryVisible = value;
				OnPropertyChangedWithValue(value, "IsCharacterStoryVisible");
			}
		}
	}

	[DataSourceProperty]
	public bool IsCompanionsVisible
	{
		get
		{
			return _isCompanionsVisible;
		}
		private set
		{
			if (_isCompanionsVisible != value)
			{
				_isCompanionsVisible = value;
				OnPropertyChangedWithValue(value, "IsCompanionsVisible");
			}
		}
	}

	[DataSourceProperty]
	public bool IsDiplomacyVisible
	{
		get
		{
			return _isDiplomacyVisible;
		}
		private set
		{
			if (_isDiplomacyVisible != value)
			{
				_isDiplomacyVisible = value;
				OnPropertyChangedWithValue(value, "IsDiplomacyVisible");
				OnPropertyChangedWithValue(IsRealmLedgerVisible, "IsRealmLedgerVisible");
			}
		}
	}

	[DataSourceProperty]
	public bool IsRealmLedgerVisible
	{
		get
		{
			if (_isDiplomacyVisible)
			{
				return !_isMarriagesPage;
			}
			return false;
		}
	}

	[DataSourceProperty]
	public bool IsStrategicSectionVisible
	{
		get
		{
			return _isStrategicSectionVisible;
		}
		private set
		{
			if (_isStrategicSectionVisible != value)
			{
				_isStrategicSectionVisible = value;
				OnPropertyChangedWithValue(value, "IsStrategicSectionVisible");
			}
		}
	}

	[DataSourceProperty]
	public bool IsWarStatisticsVisible
	{
		get
		{
			return _isWarStatisticsVisible;
		}
		private set
		{
			if (_isWarStatisticsVisible != value)
			{
				_isWarStatisticsVisible = value;
				OnPropertyChangedWithValue(value, "IsWarStatisticsVisible");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<CalendarWorldLedgerTabVM> Tabs => _tabs;

	[DataSourceProperty]
	public MBBindingList<CalendarWorldCalendarDayVM> Days => _days;

	[DataSourceProperty]
	public bool HasSelectedCalendarDay => _selectedCalendarDay != long.MinValue;

	[DataSourceProperty]
	public bool CanPreviousCalendarMonth => CanMoveDisplayedCalendarMonth(-1);

	[DataSourceProperty]
	public bool CanNextCalendarMonth => CanMoveDisplayedCalendarMonth(1);

	[DataSourceProperty]
	public MBBindingList<CalendarWorldCalendarMonthVM> CalendarMonths => _calendarMonths;

	[DataSourceProperty]
	public MBBindingList<CalendarDiplomacyRelationVM> DiplomacyRelations => _diplomacyRelations;

	[DataSourceProperty]
	public MBBindingList<CalendarKingdomFinanceRowVM> KingdomFinanceRows => _kingdomFinanceRows;

	[DataSourceProperty]
	public bool HasKingdomFinanceRows => _kingdomFinanceRows.Count > 0;

	[DataSourceProperty]
	public bool ShowKingdomFinanceEmptyState => _kingdomFinanceRows.Count == 0;

	[DataSourceProperty]
	public bool IsKingdomFinanceLedgerVisible
	{
		get
		{
			if (_isKingdomFinancesPage)
			{
				return _kingdomFinanceRows.Count > 0;
			}
			return false;
		}
	}

	[DataSourceProperty]
	public bool IsKingdomFinanceEmptyVisible
	{
		get
		{
			if (_isKingdomFinancesPage)
			{
				return _kingdomFinanceRows.Count == 0;
			}
			return false;
		}
	}

	[DataSourceProperty]
	public MBBindingList<CalendarWorldSavedSummaryVM> SavedSummaries => _savedSummaries;

	[DataSourceProperty]
	public MBBindingList<CalendarWorldStrategicProvinceVM> StrategicProvinces => _strategicProvinces;

	[DataSourceProperty]
	public MBBindingList<CalendarWorldStrategicProvinceVM> StrategicContestedProvinces => _strategicContestedProvinces;

	[DataSourceProperty]
	public MBBindingList<CalendarWorldStrategicMarkerVM> StrategicMarkers => _strategicMarkers;

	[DataSourceProperty]
	public MBBindingList<CalendarStrategicKingdomSummaryVM> StrategicKingdomRows => _strategicKingdomRows;

	[DataSourceProperty]
	public MBBindingList<CalendarWorldStrategicVillageVM> SelectedStrategicVillages => _selectedStrategicVillages;

	[DataSourceProperty]
	public MBBindingList<CalendarWorldStrategicVillageVM> TrackedStrategicSettlements => _trackedStrategicSettlements;

	[DataSourceProperty]
	public bool HasTrackedStrategicSettlements => _trackedStrategicSettlements.Count > 0;

	[DataSourceProperty]
	public bool ShowTrackedStrategicSettlements
	{
		get
		{
			if (HasTrackedStrategicSettlements)
			{
				return !HasSelectedStrategicSettlement;
			}
			return false;
		}
	}

	[DataSourceProperty]
	public bool ShowStrategicMapLegend => CalendarSettingsState.StrategicMapShowLegend;

	[DataSourceProperty]
	public int StrategicMapLegendWidth => CalendarSettingsState.StrategicMapLegendWidth;

	[DataSourceProperty]
	public int StrategicMapLegendHeight => Math.Max(202, CalendarSettingsState.StrategicMapLegendHeight);

	[DataSourceProperty]
	public int StrategicMapLegendMarginTop => CalendarSettingsState.StrategicMapLegendMarginTop;

	[DataSourceProperty]
	public int StrategicMapLegendContentTop => StrategicMapLegendMarginTop + 34;

	[DataSourceProperty]
	public int StrategicMapLegendSeparatorTop => StrategicMapLegendContentTop + StrategicMapLegendHeight + 2;

	[DataSourceProperty]
	public int StrategicMapLegendIconSize => CalendarSettingsState.StrategicMapLegendIconSize;

	[DataSourceProperty]
	public int StrategicMapLegendFontSize => CalendarSettingsState.StrategicMapLegendFontSize;

	[DataSourceProperty]
	public int StrategicMapLegendContentWidth => Math.Max(160, CalendarSettingsState.StrategicMapLegendWidth - 12);

	private static float StrategicMapFitScale => Math.Max(0.4283237f, 0.24825582f);

	private float StrategicMapScale => StrategicMapFitScale * _strategicMapZoom;

	[DataSourceProperty]
	public float MapCanvasWidth => 1730f * StrategicMapScale;

	[DataSourceProperty]
	public float MapCanvasHeight => 1720f * StrategicMapScale;

	[DataSourceProperty]
	public string MapZoomText => _strategicMapZoom.ToString("0.00x");

	[DataSourceProperty]
	public bool CanZoomIn => _strategicMapZoom < 5f;

	[DataSourceProperty]
	public bool CanZoomOut => _strategicMapZoom > 1f;

	[DataSourceProperty]
	public bool HasSelectedStrategicSettlement => GetSelectedStrategicSettlement() != null;

	[DataSourceProperty]
	public bool ShowStrategicKingdomRows => !HasSelectedStrategicSettlement;

	[DataSourceProperty]
	public bool ShowStrategicSettlementDetails => HasSelectedStrategicSettlement;

	[DataSourceProperty]
	public string StrategicMappedSpanText => CalendarStrategicDistance.FormatMappedSpan();

	[DataSourceProperty]
	public bool HasStrategicDistanceOrigin => GetStrategicDistanceOriginSettlement() != null;

	[DataSourceProperty]
	public bool HasStrategicDistanceMeasurement
	{
		get
		{
			Settlement origin = GetStrategicDistanceOriginSettlement();
			Settlement destination = GetStrategicDistanceDestinationSettlement();
			if (origin != null && destination != null)
			{
				return origin != destination;
			}
			return false;
		}
	}

	[DataSourceProperty]
	public string StrategicDistanceActionText
	{
		get
		{
			Settlement selected = GetSelectedStrategicSettlement();
			Settlement origin = GetStrategicDistanceOriginSettlement();
			if (selected == null || selected != origin)
			{
				return "MEASURE FROM HERE";
			}
			return "CLEAR DISTANCE ORIGIN";
		}
	}

	[DataSourceProperty]
	public string StrategicDistanceText => BuildStrategicDistanceText();

	[DataSourceProperty]
	public bool IsSelectedStrategicSettlementTracked
	{
		get
		{
			Settlement selected = GetSelectedStrategicSettlement();
			if (selected != null && Campaign.Current != null && Campaign.Current.VisualTrackerManager != null)
			{
				return Campaign.Current.VisualTrackerManager.CheckTracked(selected);
			}
			return false;
		}
	}

	[DataSourceProperty]
	public string TrackSelectedSettlementText
	{
		get
		{
			if (!IsSelectedStrategicSettlementTracked)
			{
				return "Track Settlement";
			}
			return "Untrack Settlement";
		}
	}

	[DataSourceProperty]
	public int StrategicSummaryScrollerHeight
	{
		get
		{
			int legendExpansion = StrategicMapLegendHeight - 112;
			return Math.Max(180, (HasSelectedStrategicSettlement ? 348 : 390) - legendExpansion);
		}
	}

	[DataSourceProperty]
	public int StrategicSummaryScrollerMarginBottom
	{
		get
		{
			if (!HasSelectedStrategicSettlement)
			{
				return 60;
			}
			return 102;
		}
	}

	[DataSourceProperty]
	public int StrategicSummaryContentTop => StrategicMapLegendSeparatorTop + 29;

	private static string CalendarPeriodSummaryLabel => "SEASON SUMMARY";

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince001 => _fixedStrategicProvinces[0];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince001 => _fixedStrategicContestedProvinces[0];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince002 => _fixedStrategicProvinces[1];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince002 => _fixedStrategicContestedProvinces[1];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince003 => _fixedStrategicProvinces[2];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince003 => _fixedStrategicContestedProvinces[2];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince004 => _fixedStrategicProvinces[3];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince004 => _fixedStrategicContestedProvinces[3];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince005 => _fixedStrategicProvinces[4];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince005 => _fixedStrategicContestedProvinces[4];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince006 => _fixedStrategicProvinces[5];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince006 => _fixedStrategicContestedProvinces[5];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince007 => _fixedStrategicProvinces[6];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince007 => _fixedStrategicContestedProvinces[6];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince008 => _fixedStrategicProvinces[7];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince008 => _fixedStrategicContestedProvinces[7];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince009 => _fixedStrategicProvinces[8];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince009 => _fixedStrategicContestedProvinces[8];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince010 => _fixedStrategicProvinces[9];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince010 => _fixedStrategicContestedProvinces[9];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince011 => _fixedStrategicProvinces[10];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince011 => _fixedStrategicContestedProvinces[10];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince012 => _fixedStrategicProvinces[11];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince012 => _fixedStrategicContestedProvinces[11];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince013 => _fixedStrategicProvinces[12];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince013 => _fixedStrategicContestedProvinces[12];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince014 => _fixedStrategicProvinces[13];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince014 => _fixedStrategicContestedProvinces[13];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince015 => _fixedStrategicProvinces[14];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince015 => _fixedStrategicContestedProvinces[14];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince016 => _fixedStrategicProvinces[15];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince016 => _fixedStrategicContestedProvinces[15];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince017 => _fixedStrategicProvinces[16];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince017 => _fixedStrategicContestedProvinces[16];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince018 => _fixedStrategicProvinces[17];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince018 => _fixedStrategicContestedProvinces[17];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince019 => _fixedStrategicProvinces[18];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince019 => _fixedStrategicContestedProvinces[18];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince020 => _fixedStrategicProvinces[19];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince020 => _fixedStrategicContestedProvinces[19];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince021 => _fixedStrategicProvinces[20];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince021 => _fixedStrategicContestedProvinces[20];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince022 => _fixedStrategicProvinces[21];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince022 => _fixedStrategicContestedProvinces[21];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince023 => _fixedStrategicProvinces[22];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince023 => _fixedStrategicContestedProvinces[22];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince024 => _fixedStrategicProvinces[23];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince024 => _fixedStrategicContestedProvinces[23];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince025 => _fixedStrategicProvinces[24];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince025 => _fixedStrategicContestedProvinces[24];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince026 => _fixedStrategicProvinces[25];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince026 => _fixedStrategicContestedProvinces[25];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince027 => _fixedStrategicProvinces[26];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince027 => _fixedStrategicContestedProvinces[26];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince028 => _fixedStrategicProvinces[27];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince028 => _fixedStrategicContestedProvinces[27];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince029 => _fixedStrategicProvinces[28];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince029 => _fixedStrategicContestedProvinces[28];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince030 => _fixedStrategicProvinces[29];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince030 => _fixedStrategicContestedProvinces[29];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince031 => _fixedStrategicProvinces[30];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince031 => _fixedStrategicContestedProvinces[30];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince032 => _fixedStrategicProvinces[31];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince032 => _fixedStrategicContestedProvinces[31];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince033 => _fixedStrategicProvinces[32];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince033 => _fixedStrategicContestedProvinces[32];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince034 => _fixedStrategicProvinces[33];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince034 => _fixedStrategicContestedProvinces[33];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince035 => _fixedStrategicProvinces[34];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince035 => _fixedStrategicContestedProvinces[34];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince036 => _fixedStrategicProvinces[35];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince036 => _fixedStrategicContestedProvinces[35];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince037 => _fixedStrategicProvinces[36];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince037 => _fixedStrategicContestedProvinces[36];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince038 => _fixedStrategicProvinces[37];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince038 => _fixedStrategicContestedProvinces[37];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince039 => _fixedStrategicProvinces[38];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince039 => _fixedStrategicContestedProvinces[38];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince040 => _fixedStrategicProvinces[39];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince040 => _fixedStrategicContestedProvinces[39];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince041 => _fixedStrategicProvinces[40];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince041 => _fixedStrategicContestedProvinces[40];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince042 => _fixedStrategicProvinces[41];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince042 => _fixedStrategicContestedProvinces[41];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince043 => _fixedStrategicProvinces[42];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince043 => _fixedStrategicContestedProvinces[42];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince044 => _fixedStrategicProvinces[43];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince044 => _fixedStrategicContestedProvinces[43];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince045 => _fixedStrategicProvinces[44];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince045 => _fixedStrategicContestedProvinces[44];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince046 => _fixedStrategicProvinces[45];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince046 => _fixedStrategicContestedProvinces[45];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince047 => _fixedStrategicProvinces[46];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince047 => _fixedStrategicContestedProvinces[46];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince048 => _fixedStrategicProvinces[47];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince048 => _fixedStrategicContestedProvinces[47];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince049 => _fixedStrategicProvinces[48];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince049 => _fixedStrategicContestedProvinces[48];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince050 => _fixedStrategicProvinces[49];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince050 => _fixedStrategicContestedProvinces[49];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince051 => _fixedStrategicProvinces[50];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince051 => _fixedStrategicContestedProvinces[50];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince052 => _fixedStrategicProvinces[51];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince052 => _fixedStrategicContestedProvinces[51];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince053 => _fixedStrategicProvinces[52];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince053 => _fixedStrategicContestedProvinces[52];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince054 => _fixedStrategicProvinces[53];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince054 => _fixedStrategicContestedProvinces[53];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince055 => _fixedStrategicProvinces[54];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince055 => _fixedStrategicContestedProvinces[54];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince056 => _fixedStrategicProvinces[55];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince056 => _fixedStrategicContestedProvinces[55];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince057 => _fixedStrategicProvinces[56];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince057 => _fixedStrategicContestedProvinces[56];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince058 => _fixedStrategicProvinces[57];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince058 => _fixedStrategicContestedProvinces[57];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince059 => _fixedStrategicProvinces[58];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince059 => _fixedStrategicContestedProvinces[58];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince060 => _fixedStrategicProvinces[59];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince060 => _fixedStrategicContestedProvinces[59];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince061 => _fixedStrategicProvinces[60];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince061 => _fixedStrategicContestedProvinces[60];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince062 => _fixedStrategicProvinces[61];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince062 => _fixedStrategicContestedProvinces[61];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince063 => _fixedStrategicProvinces[62];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince063 => _fixedStrategicContestedProvinces[62];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince064 => _fixedStrategicProvinces[63];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince064 => _fixedStrategicContestedProvinces[63];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince065 => _fixedStrategicProvinces[64];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince065 => _fixedStrategicContestedProvinces[64];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince066 => _fixedStrategicProvinces[65];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince066 => _fixedStrategicContestedProvinces[65];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince067 => _fixedStrategicProvinces[66];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince067 => _fixedStrategicContestedProvinces[66];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince068 => _fixedStrategicProvinces[67];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince068 => _fixedStrategicContestedProvinces[67];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince069 => _fixedStrategicProvinces[68];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince069 => _fixedStrategicContestedProvinces[68];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince070 => _fixedStrategicProvinces[69];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince070 => _fixedStrategicContestedProvinces[69];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince071 => _fixedStrategicProvinces[70];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince071 => _fixedStrategicContestedProvinces[70];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince072 => _fixedStrategicProvinces[71];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince072 => _fixedStrategicContestedProvinces[71];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince073 => _fixedStrategicProvinces[72];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince073 => _fixedStrategicContestedProvinces[72];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince074 => _fixedStrategicProvinces[73];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince074 => _fixedStrategicContestedProvinces[73];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince075 => _fixedStrategicProvinces[74];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince075 => _fixedStrategicContestedProvinces[74];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince076 => _fixedStrategicProvinces[75];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince076 => _fixedStrategicContestedProvinces[75];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince077 => _fixedStrategicProvinces[76];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince077 => _fixedStrategicContestedProvinces[76];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince078 => _fixedStrategicProvinces[77];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince078 => _fixedStrategicContestedProvinces[77];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince079 => _fixedStrategicProvinces[78];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince079 => _fixedStrategicContestedProvinces[78];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince080 => _fixedStrategicProvinces[79];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince080 => _fixedStrategicContestedProvinces[79];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince081 => _fixedStrategicProvinces[80];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince081 => _fixedStrategicContestedProvinces[80];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince082 => _fixedStrategicProvinces[81];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince082 => _fixedStrategicContestedProvinces[81];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince083 => _fixedStrategicProvinces[82];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince083 => _fixedStrategicContestedProvinces[82];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince084 => _fixedStrategicProvinces[83];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince084 => _fixedStrategicContestedProvinces[83];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince085 => _fixedStrategicProvinces[84];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince085 => _fixedStrategicContestedProvinces[84];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince086 => _fixedStrategicProvinces[85];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince086 => _fixedStrategicContestedProvinces[85];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince087 => _fixedStrategicProvinces[86];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince087 => _fixedStrategicContestedProvinces[86];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince088 => _fixedStrategicProvinces[87];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince088 => _fixedStrategicContestedProvinces[87];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince089 => _fixedStrategicProvinces[88];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince089 => _fixedStrategicContestedProvinces[88];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince090 => _fixedStrategicProvinces[89];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince090 => _fixedStrategicContestedProvinces[89];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince091 => _fixedStrategicProvinces[90];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince091 => _fixedStrategicContestedProvinces[90];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince092 => _fixedStrategicProvinces[91];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince092 => _fixedStrategicContestedProvinces[91];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince093 => _fixedStrategicProvinces[92];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince093 => _fixedStrategicContestedProvinces[92];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince094 => _fixedStrategicProvinces[93];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince094 => _fixedStrategicContestedProvinces[93];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince095 => _fixedStrategicProvinces[94];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince095 => _fixedStrategicContestedProvinces[94];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince096 => _fixedStrategicProvinces[95];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince096 => _fixedStrategicContestedProvinces[95];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince097 => _fixedStrategicProvinces[96];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince097 => _fixedStrategicContestedProvinces[96];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince098 => _fixedStrategicProvinces[97];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince098 => _fixedStrategicContestedProvinces[97];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince099 => _fixedStrategicProvinces[98];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince099 => _fixedStrategicContestedProvinces[98];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince100 => _fixedStrategicProvinces[99];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince100 => _fixedStrategicContestedProvinces[99];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince101 => _fixedStrategicProvinces[100];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince101 => _fixedStrategicContestedProvinces[100];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince102 => _fixedStrategicProvinces[101];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince102 => _fixedStrategicContestedProvinces[101];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince103 => _fixedStrategicProvinces[102];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince103 => _fixedStrategicContestedProvinces[102];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince104 => _fixedStrategicProvinces[103];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince104 => _fixedStrategicContestedProvinces[103];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince105 => _fixedStrategicProvinces[104];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince105 => _fixedStrategicContestedProvinces[104];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince106 => _fixedStrategicProvinces[105];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince106 => _fixedStrategicContestedProvinces[105];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince107 => _fixedStrategicProvinces[106];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince107 => _fixedStrategicContestedProvinces[106];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince108 => _fixedStrategicProvinces[107];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince108 => _fixedStrategicContestedProvinces[107];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince109 => _fixedStrategicProvinces[108];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince109 => _fixedStrategicContestedProvinces[108];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince110 => _fixedStrategicProvinces[109];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince110 => _fixedStrategicContestedProvinces[109];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince111 => _fixedStrategicProvinces[110];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince111 => _fixedStrategicContestedProvinces[110];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince112 => _fixedStrategicProvinces[111];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince112 => _fixedStrategicContestedProvinces[111];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince113 => _fixedStrategicProvinces[112];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince113 => _fixedStrategicContestedProvinces[112];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince114 => _fixedStrategicProvinces[113];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince114 => _fixedStrategicContestedProvinces[113];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince115 => _fixedStrategicProvinces[114];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince115 => _fixedStrategicContestedProvinces[114];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince116 => _fixedStrategicProvinces[115];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince116 => _fixedStrategicContestedProvinces[115];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince117 => _fixedStrategicProvinces[116];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince117 => _fixedStrategicContestedProvinces[116];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince118 => _fixedStrategicProvinces[117];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince118 => _fixedStrategicContestedProvinces[117];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince119 => _fixedStrategicProvinces[118];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince119 => _fixedStrategicContestedProvinces[118];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince120 => _fixedStrategicProvinces[119];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince120 => _fixedStrategicContestedProvinces[119];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince121 => _fixedStrategicProvinces[120];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince121 => _fixedStrategicContestedProvinces[120];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince122 => _fixedStrategicProvinces[121];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince122 => _fixedStrategicContestedProvinces[121];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince123 => _fixedStrategicProvinces[122];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince123 => _fixedStrategicContestedProvinces[122];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince124 => _fixedStrategicProvinces[123];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince124 => _fixedStrategicContestedProvinces[123];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince125 => _fixedStrategicProvinces[124];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince125 => _fixedStrategicContestedProvinces[124];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince126 => _fixedStrategicProvinces[125];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince126 => _fixedStrategicContestedProvinces[125];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince127 => _fixedStrategicProvinces[126];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince127 => _fixedStrategicContestedProvinces[126];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince128 => _fixedStrategicProvinces[127];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince128 => _fixedStrategicContestedProvinces[127];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince129 => _fixedStrategicProvinces[128];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince129 => _fixedStrategicContestedProvinces[128];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince130 => _fixedStrategicProvinces[129];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince130 => _fixedStrategicContestedProvinces[129];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince131 => _fixedStrategicProvinces[130];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince131 => _fixedStrategicContestedProvinces[130];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince132 => _fixedStrategicProvinces[131];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince132 => _fixedStrategicContestedProvinces[131];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicProvince133 => _fixedStrategicProvinces[132];

	[DataSourceProperty]
	public CalendarWorldStrategicProvinceVM StrategicContestedProvince133 => _fixedStrategicContestedProvinces[132];

	[DataSourceProperty]
	public MBBindingList<CalendarStrategicKingdomLabelVM> StrategicKingdomLabels => _strategicKingdomLabels;

	[DataSourceProperty]
	public MBBindingList<CalendarStrategicFriendlyArmyVM> StrategicFriendlyArmies => _strategicFriendlyArmies;

	internal CalendarWorldLedgerVM(Action close)
	{
		_close = close;
		AddTab("Calendar", "Realm Chronicle");
		AddTab("Character", "My Story");
		AddTab("Diplomacy", "Realm Affairs");
		AddTab("Strategic", "Military Affairs");
		SelectTab(_tabs[0]);
	}

	public void ExecuteClose()
	{
		if (_close != null)
		{
			_close();
		}
	}

	public void ExecuteRefresh()
	{
		RefreshCalendar();
	}

	public void ExecuteShowCharacterStory()
	{
		Diagnostics.Info("World Events page tab selected: My Story / Personal Chronicle.");
		IsCharacterStoryVisible = true;
		IsCompanionsVisible = false;
		RefreshCharacterStory();
	}

	public void ExecuteShowCalendarPage()
	{
		Diagnostics.Info("World Events page tab selected: Realm Chronicle / Calendar.");
		IsCalendarVisible = true;
		IsSummariesVisible = false;
		IsSavedSummariesPage = false;
	}

	public void ExecuteShowSavedSummaries()
	{
		Diagnostics.Info("World Events page tab selected: Realm Chronicle / Summaries.");
		IsCalendarVisible = false;
		IsSummariesVisible = true;
		IsSavedSummariesPage = true;
		IsMarriagesPage = false;
	}

	public void ExecuteShowMarriagesPage()
	{
		Diagnostics.Info("World Events page tab selected: Realm Affairs / Marriages.");
		IsSavedSummariesPage = false;
		IsDiplomacyRelationsPage = false;
		IsKingdomFinancesPage = false;
		IsMarriagesPage = true;
		OnPropertyChangedWithValue(IsRealmLedgerVisible, "IsRealmLedgerVisible");
		RefreshMarriages();
	}

	public void ExecuteSortMarriagesByName()
	{
		SetMarriageSortMode("Name");
	}

	public void ExecuteSortMarriagesByAge()
	{
		SetMarriageSortMode("Age");
	}

	public void ExecuteSortMarriagesByKingdom()
	{
		SetMarriageSortMode("Kingdom");
	}

	public void ExecuteSortMarriagesByAll()
	{
		SetMarriageSortMode("All");
	}

	private void SetMarriageSortMode(string mode)
	{
		if (!string.Equals(_marriageSortMode, mode, StringComparison.Ordinal))
		{
			_marriageSortMode = mode;
			OnPropertyChangedWithValue(IsMarriageSortName, "IsMarriageSortName");
			OnPropertyChangedWithValue(IsMarriageSortAge, "IsMarriageSortAge");
			OnPropertyChangedWithValue(IsMarriageSortKingdom, "IsMarriageSortKingdom");
			OnPropertyChangedWithValue(IsMarriageSortAll, "IsMarriageSortAll");
			RefreshMarriages();
		}
	}

	public void ExecuteShowCompanionsPage()
	{
		Diagnostics.Info("World Events page tab selected: My Story / Companions.");
		IsCharacterStoryVisible = false;
		IsCompanionsVisible = true;
		RefreshCompanions();
	}

	public void ExecuteShowDiplomacyRelations()
	{
		Diagnostics.Info("World Events page tab selected: Realm Affairs / Diplomacy.");
		IsDiplomacyRelationsPage = true;
		IsKingdomFinancesPage = false;
		IsMarriagesPage = false;
		OnPropertyChangedWithValue(IsRealmLedgerVisible, "IsRealmLedgerVisible");
		RefreshDiplomacy();
	}

	public void ExecuteShowKingdomFinances()
	{
		Diagnostics.Info("World Events page tab selected: Realm Affairs / Kingdom Finances.");
		IsDiplomacyRelationsPage = false;
		IsKingdomFinancesPage = true;
		IsMarriagesPage = false;
		OnPropertyChangedWithValue(IsRealmLedgerVisible, "IsRealmLedgerVisible");
		RefreshDiplomacy();
	}

	public void ExecuteShowStrategicMapPage()
	{
		Diagnostics.Info("World Events page tab selected: Military Affairs / Strategic Map.");
		IsStrategicMap = true;
		IsWarStatisticsVisible = false;
		BuildStrategicMapLayers();
		RefreshStrategicKingdomRows();
		RefreshSelectedStrategicVillages();
		RefreshTrackedStrategicSettlements();
		StrategicText = BuildStrategicPanelText();
		NotifyStrategicSettlementSelectionChanged();
	}

	public void ExecuteShowPoliticalMapMode()
	{
		SetStrategicMapMode("Political");
	}

	public void ExecuteShowCultureMapMode()
	{
		SetStrategicMapMode("Culture");
	}

	public void ExecuteShowReligionMapMode()
	{
		SetStrategicMapMode("Religion");
	}

	public void ExecuteShowPopulationMapMode()
	{
		SetStrategicMapMode("Population");
	}

	private void SetStrategicMapMode(string mode)
	{
		if (!string.Equals(_strategicMapMode, mode, StringComparison.Ordinal))
		{
			_strategicMapMode = mode;
			OnPropertyChangedWithValue(IsPoliticalMapMode, "IsPoliticalMapMode");
			OnPropertyChangedWithValue(IsCultureMapMode, "IsCultureMapMode");
			OnPropertyChangedWithValue(IsReligionMapMode, "IsReligionMapMode");
			OnPropertyChangedWithValue(IsPopulationMapMode, "IsPopulationMapMode");
			OnPropertyChangedWithValue(StrategicMapModeTitle, "StrategicMapModeTitle");
			OnPropertyChangedWithValue(StrategicMapModeLegendText, "StrategicMapModeLegendText");
			if (IsStrategicMap)
			{
				BuildStrategicMapLayers();
			}
			Diagnostics.Info("Strategic map mode selected: " + _strategicMapMode + ".");
		}
	}

	internal void RefreshStrategicDemographicMap()
	{
		if (IsStrategicMap && !IsPoliticalMapMode)
		{
			BuildStrategicMapLayers();
		}
	}

	public void ExecuteShowStrategicWarStatistics()
	{
		Diagnostics.Info("World Events page tab selected: Military Affairs / War Statistics.");
		IsStrategicMap = false;
		IsWarStatisticsVisible = true;
		_isPointerOverStrategicMap = false;
		RefreshWarStatistics();
	}

	internal void RefreshWorldState()
	{
		RefreshCalendar();
	}

	public void ExecuteZoomIn()
	{
		SetStrategicMapZoom(_strategicMapZoom + 0.25f);
	}

	public void ExecuteZoomOut()
	{
		SetStrategicMapZoom(_strategicMapZoom - 0.25f);
	}

	public void ExecuteResetMapView()
	{
		SetStrategicMapZoom(1f);
	}

	public void ExecuteTrackSelectedSettlement()
	{
		Settlement selected = GetSelectedStrategicSettlement();
		if (selected != null && Campaign.Current != null && Campaign.Current.VisualTrackerManager != null)
		{
			ToggleSettlementTracking(selected, "settlement");
			RefreshTrackedStrategicSettlements();
			NotifyStrategicSettlementSelectionChanged();
			StrategicText = BuildStrategicPanelText();
		}
	}

	public void ExecuteSetStrategicDistanceOrigin()
	{
		Settlement selected = GetSelectedStrategicSettlement();
		if (selected != null)
		{
			Settlement currentOrigin = GetStrategicDistanceOriginSettlement();
			if (selected == currentOrigin)
			{
				_strategicDistanceOriginSettlementId = string.Empty;
				_strategicDistanceDestinationSettlementId = string.Empty;
				Diagnostics.Info("Strategic distance survey cleared.");
			}
			else
			{
				_strategicDistanceOriginSettlementId = selected.StringId ?? string.Empty;
				_strategicDistanceDestinationSettlementId = string.Empty;
				Diagnostics.Info("Strategic distance origin set: " + _strategicDistanceOriginSettlementId + " (" + selected.Name?.ToString() + ").");
			}
			NotifyStrategicDistanceChanged();
		}
	}

	public void ExecuteShowKingdomSummary()
	{
		_selectedStrategicSettlementId = string.Empty;
		foreach (CalendarWorldStrategicMarkerVM strategicMarker in _strategicMarkers)
		{
			strategicMarker.IsSelected = false;
		}
		RefreshSelectedStrategicVillages();
		RefreshStrategicKingdomRows();
		StrategicText = BuildStrategicPanelText();
		NotifyStrategicSettlementSelectionChanged();
		Diagnostics.Info("Strategic settlement details closed; Kingdom Summary restored.");
	}

	public void ExecuteStrategicMapHoverBegin()
	{
		_isPointerOverStrategicMap = true;
	}

	public void ExecuteStrategicMapHoverEnd()
	{
		_isPointerOverStrategicMap = false;
	}

	internal void AdjustStrategicMapZoomFromMouseWheel(float delta)
	{
		if (IsStrategicMap && _isPointerOverStrategicMap && !(Math.Abs(delta) < 0.001f))
		{
			SetStrategicMapZoom(_strategicMapZoom + ((delta > 0f) ? 0.25f : (-0.25f)));
		}
	}

	private void AddTab(string filter, string label)
	{
		_tabs.Add(new CalendarWorldLedgerTabVM(filter, label, SelectTab));
	}

	private void SelectTab(CalendarWorldLedgerTabVM tab)
	{
		if (tab == null)
		{
			return;
		}
		Diagnostics.Info("World Events main tab selected: " + tab.Filter + ".");
		_selectedFilter = "All";
		IsStrategicSectionVisible = string.Equals(tab.Filter, "Strategic", StringComparison.Ordinal);
		IsStrategicMap = IsStrategicSectionVisible;
		IsCalendarSectionVisible = string.Equals(tab.Filter, "Calendar", StringComparison.Ordinal);
		IsCalendarVisible = IsCalendarSectionVisible;
		IsSummariesVisible = false;
		IsSavedSummariesPage = false;
		IsMarriagesPage = false;
		OnPropertyChangedWithValue(IsRealmLedgerVisible, "IsRealmLedgerVisible");
		IsStorySectionVisible = string.Equals(tab.Filter, "Character", StringComparison.Ordinal);
		IsCharacterStoryVisible = IsStorySectionVisible;
		IsCompanionsVisible = false;
		IsDiplomacyVisible = string.Equals(tab.Filter, "Diplomacy", StringComparison.Ordinal);
		IsWarStatisticsVisible = false;
		if (IsDiplomacyVisible)
		{
			IsDiplomacyRelationsPage = false;
			IsKingdomFinancesPage = true;
		}
		if (!IsStrategicMap)
		{
			_isPointerOverStrategicMap = false;
		}
		foreach (CalendarWorldLedgerTabVM tab2 in _tabs)
		{
			tab2.IsSelected = tab2 == tab;
		}
		OnPropertyChangedWithValue(IsRealmLedgerVisible, "IsRealmLedgerVisible");
		RefreshCalendar();
	}

	private void RefreshCalendar()
	{
		RefreshCalendarNotes();
		RefreshCharacterStory();
		RefreshCompanions();
		RefreshMarriages();
		RefreshDiplomacy();
		RefreshWarStatistics();
		BuildStrategicMapLayers();
		RefreshStrategicKingdomRows();
		RefreshSelectedStrategicVillages();
		RefreshTrackedStrategicSettlements();
		StrategicText = BuildStrategicPanelText();
		NotifyStrategicSettlementSelectionChanged();
		BuildCalendarHistory();
	}

	private void RefreshCharacterStory()
	{
		Hero hero = Hero.MainHero;
		if (hero == null)
		{
			CharacterStoryTitle = "CHARACTER CHRONICLE";
			CharacterStorySubtitle = "No active campaign hero";
			CharacterStoryText = "Load a campaign to view the current character's story.";
			CharacterMilestoneText = "No later milestones have been recorded yet.";
			CharacterKillsText = "0";
			CharacterKnockoutsText = "0";
			CharacterRecordNote = "No combat record is available.";
			CharacterPortrait = new CharacterImageIdentifierVM(null);
			CharacterClanBanner = new BannerImageIdentifierVM(null, nineGrid: true);
			return;
		}
		string culture = ((hero.Culture == null || hero.Culture.Name == null) ? "Unknown culture" : hero.Culture.Name.ToString());
		string clan = ((hero.Clan == null || hero.Clan.Name == null) ? "No clan" : hero.Clan.Name.ToString());
		CharacterStoryTitle = ((hero.Name == null) ? "CHARACTER CHRONICLE" : hero.Name.ToString().ToUpperInvariant());
		CharacterStorySubtitle = culture + "  •  " + clan + "  •  Age " + hero.Age.ToString("0");
		CharacterPortrait = new CharacterImageIdentifierVM(CharacterCode.CreateFrom(hero.CharacterObject));
		CharacterClanBanner = new BannerImageIdentifierVM((hero.Clan == null) ? null : hero.Clan.Banner, nineGrid: true);
		CalendarWorldLedgerBehavior.RepairStandaloneInitialCharacterFacts(hero);
		string origin = CalendarWorldLedgerBehavior.GetCharacterOriginStory();
		string story = (string.IsNullOrWhiteSpace(origin) ? "This campaign predates the origin recorder, so Bannerlord no longer exposes the exact choices made during character creation.\n\nThe next character created with Ages of Calradia enabled will have every final narrative choice preserved here." : origin);
		string milestones = CalendarWorldLedgerBehavior.GetCharacterMilestoneStory(hero);
		CalendarWorldLedgerBehavior.GetCombatStatistics(hero, out var kills, out var knockouts);
		CharacterStoryText = story;
		CharacterMilestoneText = (string.IsNullOrWhiteSpace(milestones) ? "No later milestones have been recorded yet." : milestones);
		CharacterKillsText = kills.ToString("N0");
		CharacterKnockoutsText = knockouts.ToString("N0");
		CharacterRecordNote = "Combat records begin when this Ages of Calradia version is installed.";
	}

	private void RefreshWarStatistics()
	{
		CalendarWarStatisticsSnapshot snapshot = CalendarWorldLedgerBehavior.GetWarStatisticsSnapshot();
		WarActiveWarsText = snapshot.Wars.Count.ToString("N0");
		WarTroopsFieldedText = snapshot.TotalTroopsFielded.ToString("N0");
		WarLossesText = snapshot.TotalLosses.ToString("N0");
		WarDiplomacyRecordText = "LIVE BANNERLORD DIPLOMACY  •  " + snapshot.Wars.Count.ToString("N0") + " active kingdom wars";
		WarStatisticsFootnote = "Strength uses Bannerlord's native faction-strength value; losses use its current-war casualty record. No custom ship totals or war score are shown.";
		WarPlayerStatusText = snapshot.PlayerStatus;
		_warStatisticsRows.Clear();
		for (int index = 0; index < snapshot.Wars.Count; index++)
		{
			_warStatisticsRows.Add(new CalendarWarStatisticsRowVM(snapshot.Wars[index], index, ConcludeWar));
		}
		HasWarStatisticsRows = _warStatisticsRows.Count > 0;
		WarStatisticsText = (HasWarStatisticsRows ? string.Empty : "No kingdom wars are active. Calradia is presently at peace.");
	}

	private void ConcludeWar(CalendarWarStatisticsRecord record, bool surrender)
	{
		if (record != null)
		{
			if (CalendarWorldLedgerBehavior.TryConcludeWar(record.LeftKingdom, record.RightKingdom, surrender, out var message))
			{
				InformationManager.DisplayMessage(new InformationMessage(message));
				RefreshCalendar();
			}
			else
			{
				WarStatisticsText = message;
			}
		}
	}

	private void RefreshCompanions()
	{
		_companionRows.Clear();
		List<Hero> companions = new List<Hero>();
		foreach (Hero hero in Hero.AllAliveHeroes)
		{
			if (hero != null && hero.IsPlayerCompanion)
			{
				companions.Add(hero);
			}
		}
		companions.Sort(delegate(Hero left, Hero right)
		{
			string strA = ((left == null || left.Name == null) ? string.Empty : left.Name.ToString());
			string strB = ((right == null || right.Name == null) ? string.Empty : right.Name.ToString());
			return string.Compare(strA, strB, StringComparison.Ordinal);
		});
		if (companions.Count == 0)
		{
			CompanionsText = "No companions currently serve your clan. Recruit wanderers to build a company whose histories and combat records can be followed here.";
			HasCompanionRows = false;
			return;
		}
		for (int index = 0; index < companions.Count; index++)
		{
			_companionRows.Add(new CalendarCompanionRecordVM(companions[index], index));
		}
		CompanionsText = string.Empty;
		HasCompanionRows = _companionRows.Count > 0;
	}

	private void RefreshMarriages()
	{
		_marriageCandidates.Clear();
		Hero player = Hero.MainHero;
		if (player == null || player.CharacterObject == null || Campaign.Current == null)
		{
			MarriagePlayerName = "NO ACTIVE CHARACTER";
			MarriagePlayerDetails = "Load a campaign to open the marriage ledger.";
			MarriagePlayerStatus = string.Empty;
			MarriagePlayerPortrait = new CharacterImageIdentifierVM(null);
			MarriageStatusText = "No marriage candidates are available outside a campaign.";
			NotifyMarriageCandidateStateChanged();
			return;
		}
		MarriagePlayerName = ((player.Name == null) ? "YOUR CHARACTER" : player.Name.ToString().ToUpperInvariant());
		string clanName = ((player.Clan == null || player.Clan.Name == null) ? "No clan" : player.Clan.Name.ToString());
		string kingdomName = ((player.Clan == null || player.Clan.Kingdom == null || player.Clan.Kingdom.Name == null) ? "Independent" : player.Clan.Kingdom.Name.ToString());
		MarriagePlayerDetails = clanName + "  •  " + kingdomName + "  •  Age " + player.Age.ToString("0");
		MarriagePlayerStatus = ((player.Spouse == null) ? "UNMARRIED  •  Messengers arrange audiences; Bannerlord's native courtship and family approval decide the match." : ("MARRIED TO " + ((player.Spouse.Name == null) ? "an unnamed spouse" : player.Spouse.Name.ToString().ToUpperInvariant())));
		MarriagePlayerPortrait = new CharacterImageIdentifierVM(CharacterCode.CreateFrom(player.CharacterObject));
		List<Hero> candidates = new List<Hero>();
		MarriageModel marriageModel = ((Campaign.Current.Models == null) ? null : Campaign.Current.Models.MarriageModel);
		foreach (Hero candidate in Hero.AllAliveHeroes)
		{
			if (candidate == null || candidate == player || !candidate.IsAlive || !candidate.IsActive || !candidate.IsLord || candidate.IsFemale == player.IsFemale || candidate.CharacterObject == null || candidate.Spouse != null || candidate.IsMinorFactionHero)
			{
				continue;
			}
			int minimumAge = ((marriageModel == null) ? 18 : (candidate.IsFemale ? marriageModel.MinimumMarriageAgeFemale : marriageModel.MinimumMarriageAgeMale));
			if (!(candidate.Age < (float)minimumAge))
			{
				string candidateName = ((candidate.Name == null) ? string.Empty : candidate.Name.ToString());
				if (string.IsNullOrWhiteSpace(_marriageNameSearchText) || candidateName.IndexOf(_marriageNameSearchText.Trim(), StringComparison.OrdinalIgnoreCase) >= 0)
				{
					candidates.Add(candidate);
				}
			}
		}
		candidates.Sort((Hero left, Hero right) => CompareMarriageCandidates(left, right, _marriageSortMode));
		for (int index = 0; index < candidates.Count; index++)
		{
			_marriageCandidates.Add(new CalendarMarriageCandidateVM(player, candidates[index], index, SendMarriageMessenger));
		}
		MarriageStatusText = ((_marriageCandidates.Count != 0) ? (_marriageCandidates.Count.ToString("N0") + " unmarried " + (player.IsFemale ? "men" : "women") + " listed  •  Sorted by " + MarriageSortDescription(_marriageSortMode) + (string.IsNullOrWhiteSpace(_marriageNameSearchText) ? string.Empty : ("  •  Name contains ‘" + _marriageNameSearchText.Trim() + "’"))) : (string.IsNullOrWhiteSpace(_marriageNameSearchText) ? "No unmarried opposite-sex adult nobles are currently available." : ("No eligible marriage candidates match ‘" + _marriageNameSearchText.Trim() + "’.")));
		NotifyMarriageCandidateStateChanged();
	}

	private static int CompareMarriageCandidates(Hero left, Hero right, string mode)
	{
		if (string.Equals(mode, "Age", StringComparison.Ordinal))
		{
			int age2 = left.Age.CompareTo(right.Age);
			if (age2 == 0)
			{
				return CompareMarriageCandidateNames(left, right);
			}
			return age2;
		}
		if (string.Equals(mode, "Kingdom", StringComparison.Ordinal))
		{
			int kingdom2 = string.Compare(MarriageCandidateKingdom(left), MarriageCandidateKingdom(right), StringComparison.Ordinal);
			if (kingdom2 == 0)
			{
				return CompareMarriageCandidateNames(left, right);
			}
			return kingdom2;
		}
		if (string.Equals(mode, "All", StringComparison.Ordinal))
		{
			int kingdom = string.Compare(MarriageCandidateKingdom(left), MarriageCandidateKingdom(right), StringComparison.Ordinal);
			if (kingdom != 0)
			{
				return kingdom;
			}
			int age = left.Age.CompareTo(right.Age);
			if (age == 0)
			{
				return CompareMarriageCandidateNames(left, right);
			}
			return age;
		}
		return CompareMarriageCandidateNames(left, right);
	}

	private static int CompareMarriageCandidateNames(Hero left, Hero right)
	{
		string strA = ((left == null || left.Name == null) ? string.Empty : left.Name.ToString());
		string rightName = ((right == null || right.Name == null) ? string.Empty : right.Name.ToString());
		return string.Compare(strA, rightName, StringComparison.Ordinal);
	}

	private static string MarriageCandidateKingdom(Hero hero)
	{
		if (hero != null && hero.Clan != null && hero.Clan.Kingdom != null && !(hero.Clan.Kingdom.Name == null))
		{
			return hero.Clan.Kingdom.Name.ToString();
		}
		return "Independent";
	}

	private static string MarriageSortDescription(string mode)
	{
		if (string.Equals(mode, "Name", StringComparison.Ordinal))
		{
			return "name";
		}
		if (string.Equals(mode, "Age", StringComparison.Ordinal))
		{
			return "age (youngest first)";
		}
		if (string.Equals(mode, "Kingdom", StringComparison.Ordinal))
		{
			return "kingdom, then name";
		}
		return "kingdom, age, then name";
	}

	private static bool IsMarriageProspectCompatible(Hero player, Hero candidate)
	{
		if (player == null || candidate == null || player.Spouse != null || candidate.Spouse != null || Campaign.Current == null || Campaign.Current.Models == null || Campaign.Current.Models.MarriageModel == null)
		{
			return false;
		}
		try
		{
			return Campaign.Current.Models.MarriageModel.IsCoupleSuitableForMarriage(player, candidate) && !FactionManager.IsAtWarAgainstFaction(player.MapFaction, candidate.MapFaction);
		}
		catch (Exception exception)
		{
			Diagnostics.Error("Marriage prospect compatibility check failed.", exception);
			return false;
		}
	}

	private void NotifyMarriageCandidateStateChanged()
	{
		OnPropertyChangedWithValue(HasMarriageCandidates, "HasMarriageCandidates");
		OnPropertyChangedWithValue(ShowMarriageEmptyState, "ShowMarriageEmptyState");
	}

	private void SendMarriageMessenger(Hero candidate, string purpose)
	{
		Hero target = ((!string.Equals(purpose, "ClanLeader", StringComparison.Ordinal)) ? candidate : ((candidate == null || candidate.Clan == null) ? null : candidate.Clan.Leader));
		string audienceName = ((target == null || target.Name == null) ? "that court" : target.Name.ToString());
		if (candidate == null || target == null || target.CharacterObject == null || !target.IsAlive || target.IsPrisoner)
		{
			MarriageStatusText = "That court cannot receive a messenger at present.";
			return;
		}
		if (!IsMarriageProspectCompatible(Hero.MainHero, candidate))
		{
			MarriageStatusText = "Bannerlord's native marriage rules do not currently permit this match.";
			return;
		}
		if (CharacterObject.PlayerCharacter == null || PartyBase.MainParty == null || Campaign.Current == null)
		{
			MarriageStatusText = "A campaign party is required before a messenger can be dispatched.";
			return;
		}
		int daysRemaining = CalendarWorldLedgerBehavior.GetMarriageMessengerDaysRemaining(target, purpose);
		if (daysRemaining < 0)
		{
			if (CalendarWorldLedgerBehavior.DispatchMarriageMessenger(target, purpose, out var travelDays))
			{
				RefreshMarriages();
				MarriageStatusText = "A messenger has departed for " + audienceName + ". The campaign route should take " + travelDays.ToString("N0") + ((travelDays == 1) ? " day." : " days.");
				Diagnostics.Info("Marriage messenger dispatched to " + audienceName + "; purpose=" + purpose + "; campaign-distance ETA=" + travelDays.ToString("N0") + " days.");
			}
			return;
		}
		if (daysRemaining > 0)
		{
			MarriageStatusText = "Your messenger is still travelling to " + audienceName + ". Expected arrival in " + daysRemaining.ToString("N0") + ((daysRemaining == 1) ? " day." : " days.");
			return;
		}
		try
		{
			if (_close != null)
			{
				_close();
			}
			CampaignMapConversation.OpenConversation(new ConversationCharacterData(CharacterObject.PlayerCharacter, PartyBase.MainParty), new ConversationCharacterData(target.CharacterObject));
			CalendarWorldLedgerBehavior.ConsumeArrivedMarriageMessenger(target, purpose);
			Diagnostics.Info("Marriage audience opened with " + audienceName + "; purpose=" + purpose + ".");
		}
		catch (Exception exception)
		{
			Diagnostics.Error("Marriage messenger could not open the arranged audience.", exception);
			InformationManager.DisplayMessage(new InformationMessage("The arranged audience with " + audienceName + " could not be opened."));
		}
	}

	private void RefreshDiplomacy()
	{
		_diplomacyRelations.Clear();
		_kingdomFinanceRows.Clear();
		Kingdom playerKingdom = ((Clan.PlayerClan == null) ? null : Clan.PlayerClan.Kingdom);
		List<Kingdom> kingdoms = new List<Kingdom>(Kingdom.All);
		kingdoms.Sort(delegate(Kingdom left, Kingdom right)
		{
			if (left == playerKingdom && right != playerKingdom)
			{
				return -1;
			}
			if (right == playerKingdom && left != playerKingdom)
			{
				return 1;
			}
			bool flag = playerKingdom != null && left != null && playerKingdom.IsAtWarWith(left);
			bool flag2 = playerKingdom != null && right != null && playerKingdom.IsAtWarWith(right);
			if (flag != flag2)
			{
				if (!flag)
				{
					return 1;
				}
				return -1;
			}
			string strA = ((left == null || left.Name == null) ? string.Empty : left.Name.ToString());
			string strB = ((right == null || right.Name == null) ? string.Empty : right.Name.ToString());
			return string.Compare(strA, strB, StringComparison.Ordinal);
		});
		int activeWarCount = 0;
		HashSet<Kingdom> kingdomsAtWar = new HashSet<Kingdom>();
		int observedDailyIncome = 0;
		int rowIndex = 0;
		foreach (Kingdom kingdom in kingdoms)
		{
			if (kingdom != null && !kingdom.IsEliminated)
			{
				int dailyIncome = CalculateKingdomDailyIncome(kingdom);
				observedDailyIncome += dailyIncome;
				_diplomacyRelations.Add(new CalendarDiplomacyRelationVM(playerKingdom, kingdom, dailyIncome, rowIndex++, SendMessenger));
			}
		}
		for (int first = 0; first < kingdoms.Count; first++)
		{
			Kingdom left2 = kingdoms[first];
			if (left2 == null || left2.IsEliminated)
			{
				continue;
			}
			for (int second = first + 1; second < kingdoms.Count; second++)
			{
				Kingdom right2 = kingdoms[second];
				if (right2 != null && !right2.IsEliminated && left2.IsAtWarWith(right2))
				{
					activeWarCount++;
					kingdomsAtWar.Add(left2);
					kingdomsAtWar.Add(right2);
				}
			}
		}
		ForeignOfficePeaceText = activeWarCount.ToString("N0");
		ForeignOfficeWarText = kingdomsAtWar.Count.ToString("N0");
		ForeignOfficeIncomeText = observedDailyIncome.ToString("N0") + " / day";
		ForeignOfficeRealmText = ((playerKingdom == null) ? "INDEPENDENT CLAN  •  Global wars remain visible; bilateral peace clocks appear after you join or found a realm." : ("YOUR REALM: " + ((playerKingdom.Name == null) ? "Unnamed kingdom" : playerKingdom.Name.ToString()) + "  •  Foreign courts observed: " + Math.Max(0, _diplomacyRelations.Count - 1).ToString("N0")));
		DiplomacyText = ((_diplomacyRelations.Count == 0) ? "No active foreign kingdoms could be found." : string.Empty);
		RefreshKingdomFinances(playerKingdom);
	}

	private void RefreshKingdomFinances(Kingdom playerKingdom)
	{
		if (playerKingdom == null || Campaign.Current == null || Campaign.Current.Models == null || Campaign.Current.Models.ClanFinanceModel == null)
		{
			KingdomFinanceRealmText = "KINGDOM FINANCES";
			KingdomTreasuryText = "0";
			KingdomIncomeText = "0 / day";
			KingdomExpensesText = "0 / day";
			KingdomNetText = "0 / day";
			KingdomFinanceStatusText = "Join or found a kingdom to open its treasury ledger.";
			NotifyKingdomFinanceRowStateChanged();
			return;
		}
		int totalTreasury = 0;
		int totalIncome = 0;
		int totalExpenses = 0;
		int totalNet = 0;
		int index = 0;
		List<Clan> list = new List<Clan>(playerKingdom.Clans);
		list.Sort(delegate(Clan left, Clan right)
		{
			if (left == Clan.PlayerClan && right != Clan.PlayerClan)
			{
				return -1;
			}
			if (right == Clan.PlayerClan && left != Clan.PlayerClan)
			{
				return 1;
			}
			string strA = ((left == null || left.Name == null) ? string.Empty : left.Name.ToString());
			string strB = ((right == null || right.Name == null) ? string.Empty : right.Name.ToString());
			return string.Compare(strA, strB, StringComparison.Ordinal);
		});
		foreach (Clan clan in list)
		{
			if (clan != null && !clan.IsEliminated)
			{
				int income = (int)Math.Round(Campaign.Current.Models.ClanFinanceModel.CalculateClanIncome(clan).ResultNumber);
				int expenses = Math.Abs((int)Math.Round(Campaign.Current.Models.ClanFinanceModel.CalculateClanExpenses(clan).ResultNumber));
				int net = (int)Math.Round(Campaign.Current.Models.ClanFinanceModel.CalculateClanGoldChange(clan).ResultNumber);
				totalTreasury += clan.Gold;
				totalIncome += income;
				totalExpenses += expenses;
				totalNet += net;
				_kingdomFinanceRows.Add(new CalendarKingdomFinanceRowVM(clan, income, expenses, net, index++));
			}
		}
		string realmName = ((playerKingdom.Name == null) ? "Unnamed kingdom" : playerKingdom.Name.ToString());
		KingdomFinanceRealmText = realmName.ToUpperInvariant() + "  •  " + _kingdomFinanceRows.Count.ToString("N0") + " clans in the royal ledger";
		KingdomTreasuryText = totalTreasury.ToString("N0") + " denars";
		KingdomIncomeText = "+" + totalIncome.ToString("N0") + " / day";
		KingdomExpensesText = "-" + totalExpenses.ToString("N0") + " / day";
		KingdomNetText = ((totalNet >= 0) ? "+" : string.Empty) + totalNet.ToString("N0") + " / day";
		KingdomFinanceStatusText = ((_kingdomFinanceRows.Count == 0) ? "No active clans could be found in this kingdom." : string.Empty);
		NotifyKingdomFinanceRowStateChanged();
	}

	private void NotifyKingdomFinanceRowStateChanged()
	{
		OnPropertyChangedWithValue(HasKingdomFinanceRows, "HasKingdomFinanceRows");
		OnPropertyChangedWithValue(ShowKingdomFinanceEmptyState, "ShowKingdomFinanceEmptyState");
		OnPropertyChangedWithValue(IsKingdomFinanceLedgerVisible, "IsKingdomFinanceLedgerVisible");
		OnPropertyChangedWithValue(IsKingdomFinanceEmptyVisible, "IsKingdomFinanceEmptyVisible");
	}

	private static int CalculateKingdomDailyIncome(Kingdom kingdom)
	{
		if (kingdom == null || Campaign.Current == null || Campaign.Current.Models == null || Campaign.Current.Models.ClanFinanceModel == null)
		{
			return 0;
		}
		float total = 0f;
		foreach (Clan clan in kingdom.Clans)
		{
			if (clan != null && !clan.IsEliminated)
			{
				total += Campaign.Current.Models.ClanFinanceModel.CalculateClanIncome(clan).ResultNumber;
			}
		}
		return (int)Math.Round(total);
	}

	private void SendMessenger(Kingdom targetKingdom)
	{
		Hero ruler = targetKingdom?.Leader;
		if (ruler == null || ruler.CharacterObject == null || !ruler.IsAlive || ruler.IsPrisoner)
		{
			DiplomacyText = "That court cannot receive a messenger at present.";
			return;
		}
		if (CharacterObject.PlayerCharacter == null || PartyBase.MainParty == null || Campaign.Current == null)
		{
			DiplomacyText = "A campaign party is required before a messenger can be dispatched.";
			return;
		}
		int daysRemaining = CalendarWorldLedgerBehavior.GetMessengerDaysRemaining(targetKingdom);
		if (daysRemaining < 0)
		{
			if (CalendarWorldLedgerBehavior.DispatchMessenger(targetKingdom, out var travelDays))
			{
				RefreshDiplomacy();
				DiplomacyText = "A messenger has departed for " + ((ruler.Name == null) ? "the foreign court" : ruler.Name.ToString()) + ". The campaign route is expected to take " + travelDays.ToString("N0") + ((travelDays == 1) ? " day." : " days.");
				Diagnostics.Info("Diplomacy messenger dispatched to " + ((ruler.Name == null) ? "an unnamed ruler" : ruler.Name.ToString()) + "; campaign-distance ETA=" + travelDays.ToString("N0") + " days.");
			}
			return;
		}
		if (daysRemaining > 0)
		{
			DiplomacyText = "Your messenger is still travelling. Expected arrival in " + daysRemaining.ToString("N0") + ((daysRemaining == 1) ? " day." : " days.");
			return;
		}
		try
		{
			Diagnostics.Info("Diplomacy audience opened after messenger arrival for " + ((ruler.Name == null) ? "an unnamed ruler" : ruler.Name.ToString()) + ".");
			if (_close != null)
			{
				_close();
			}
			CampaignMapConversation.OpenConversation(new ConversationCharacterData(CharacterObject.PlayerCharacter, PartyBase.MainParty), new ConversationCharacterData(ruler.CharacterObject));
			CalendarWorldLedgerBehavior.ConsumeArrivedMessenger(targetKingdom);
		}
		catch (Exception exception)
		{
			Diagnostics.Error("Diplomacy could not open the remote conversation.", exception);
			InformationManager.DisplayMessage(new InformationMessage("The audience could not be opened with " + ((ruler.Name == null) ? "that ruler" : ruler.Name.ToString()) + "."));
		}
	}

	private void BuildStrategicMapLayers()
	{
		CalendarStrategicSettlementReference.CaptureNativeSnapshot();
		Dictionary<string, IFaction> ownersBySettlementId = new Dictionary<string, IFaction>(StringComparer.Ordinal);
		Dictionary<string, IFaction> besiegersBySettlementId = new Dictionary<string, IFaction>(StringComparer.Ordinal);
		List<StrategicSettlementPoint> markerPoints = new List<StrategicSettlementPoint>();
		foreach (Settlement settlement in Settlement.All)
		{
			if ((settlement != null && settlement.Village != null) || settlement == null || settlement.Town == null)
			{
				continue;
			}
			IFaction currentOwner = CalendarWorldLedgerBehavior.GetLiveSettlementFaction(settlement);
			if (currentOwner != null && !string.IsNullOrEmpty(settlement.StringId))
			{
				ownersBySettlementId[settlement.StringId] = currentOwner;
				IFaction besieger = GetBesiegerFaction(settlement);
				if (besieger != null)
				{
					besiegersBySettlementId[settlement.StringId] = besieger;
				}
				Vec2 sourcePosition = ProjectSettlementToReferenceMap(settlement);
				markerPoints.Add(new StrategicSettlementPoint(settlement, sourcePosition.x, sourcePosition.y, currentOwner, besieger));
			}
		}
		ResolveStrategicMarkerSpacing(markerPoints);
		BuildStrategicProvinces(ownersBySettlementId, markerPoints);
		BuildStrategicContestedProvinces(besiegersBySettlementId);
		BuildStrategicKingdomLabels(markerPoints);
		BuildStrategicMarkers(markerPoints);
		RefreshStrategicFriendlyArmies();
	}

	private static void ResolveStrategicMarkerSpacing(List<StrategicSettlementPoint> points)
	{
		if (points == null || points.Count < 2)
		{
			return;
		}
		List<StrategicSettlementPoint> list = new List<StrategicSettlementPoint>(points);
		list.Sort(delegate(StrategicSettlementPoint left, StrategicSettlementPoint right)
		{
			bool flag = left != null && left.Settlement != null && left.Settlement.IsTown;
			bool flag2 = right != null && right.Settlement != null && right.Settlement.IsTown;
			if (flag != flag2)
			{
				if (!flag)
				{
					return 1;
				}
				return -1;
			}
			string strA = ((left == null || left.Settlement == null) ? string.Empty : left.Settlement.StringId);
			string strB = ((right == null || right.Settlement == null) ? string.Empty : right.Settlement.StringId);
			return string.Compare(strA, strB, StringComparison.Ordinal);
		});
		List<StrategicSettlementPoint> placed = new List<StrategicSettlementPoint>();
		float minimumSeparation = Math.Max(52f, CalendarSettingsState.StrategicMapMarkerSpacing);
		foreach (StrategicSettlementPoint point in list)
		{
			if (point == null)
			{
				continue;
			}
			point.ResetDisplayPosition();
			float candidateX = point.SourceX;
			float candidateY = point.SourceY;
			if (!IsStrategicMarkerPositionClear(candidateX, candidateY, placed))
			{
				float pushX = 0f;
				float pushY = 0f;
				foreach (StrategicSettlementPoint occupied in placed)
				{
					float deltaX = candidateX - occupied.DisplayX;
					float deltaY = candidateY - occupied.DisplayY;
					float distance = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
					if (!(distance >= minimumSeparation))
					{
						if (distance < 0.01f)
						{
							double num = (double)StableMarkerDirection(point) * (Math.PI / 6.0);
							deltaX = (float)Math.Cos(num);
							deltaY = (float)Math.Sin(num);
							distance = 1f;
						}
						float requiredPush = minimumSeparation - distance + 4f;
						pushX += deltaX / distance * requiredPush;
						pushY += deltaY / distance * requiredPush;
					}
				}
				candidateX = ClampStrategicMarkerX(point.SourceX + pushX);
				candidateY = ClampStrategicMarkerY(point.SourceY + pushY);
				if (!IsStrategicMarkerPositionClear(candidateX, candidateY, placed))
				{
					bool placedInSpiral = false;
					int startDirection = StableMarkerDirection(point);
					for (int ring = 1; ring <= 5; ring++)
					{
						if (placedInSpiral)
						{
							break;
						}
						float radius = 24f + (float)ring * 20f;
						for (int step = 0; step < 12; step++)
						{
							double radians = (double)((startDirection + step) % 12) * (Math.PI / 6.0);
							float spiralX = ClampStrategicMarkerX(point.SourceX + (float)Math.Cos(radians) * radius);
							float spiralY = ClampStrategicMarkerY(point.SourceY + (float)Math.Sin(radians) * radius);
							if (IsStrategicMarkerPositionClear(spiralX, spiralY, placed))
							{
								candidateX = spiralX;
								candidateY = spiralY;
								placedInSpiral = true;
								break;
							}
						}
					}
				}
			}
			point.SetDisplayPosition(candidateX, candidateY);
			placed.Add(point);
		}
	}

	private static bool IsStrategicMarkerPositionClear(float x, float y, List<StrategicSettlementPoint> placed)
	{
		foreach (StrategicSettlementPoint occupied in placed)
		{
			float num = x - occupied.DisplayX;
			float deltaY = y - occupied.DisplayY;
			float num2 = num * num + deltaY * deltaY;
			float minimumSeparation = Math.Max(52f, CalendarSettingsState.StrategicMapMarkerSpacing);
			if (num2 < minimumSeparation * minimumSeparation)
			{
				return false;
			}
		}
		return true;
	}

	private static int StableMarkerDirection(StrategicSettlementPoint point)
	{
		string id = ((point == null || point.Settlement == null) ? string.Empty : point.Settlement.StringId);
		int hash = 17;
		for (int index = 0; index < id.Length; index++)
		{
			hash = hash * 31 + id[index];
		}
		return (hash & 0x7FFFFFFF) % 12;
	}

	private static float ClampStrategicMarkerX(float x)
	{
		return Math.Max(20f, Math.Min(1710f, x));
	}

	private static float ClampStrategicMarkerY(float y)
	{
		return Math.Max(20f, Math.Min(1700f, y));
	}

	private static IFaction GetBesiegerFaction(Settlement settlement)
	{
		if (settlement == null || !settlement.IsUnderSiege || settlement.SiegeEvent == null || settlement.SiegeEvent.BesiegerCamp == null)
		{
			return null;
		}
		return settlement.SiegeEvent.BesiegerCamp.MapFaction;
	}

	internal static bool TryGetReferenceAnchor(string settlementId, out Vec2 referenceAnchor)
	{
		if (!string.IsNullOrEmpty(settlementId) && TownReferenceAnchors.TryGetValue(settlementId, out referenceAnchor))
		{
			return true;
		}
		if (!string.IsNullOrEmpty(settlementId) && CastleReferenceAnchors.TryGetValue(settlementId, out referenceAnchor))
		{
			return true;
		}
		if (!string.IsNullOrEmpty(settlementId) && ProjectedCastleReferenceAnchors.TryGetValue(settlementId, out referenceAnchor))
		{
			return true;
		}
		referenceAnchor = new Vec2(0f, 0f);
		return false;
	}

	internal static Vec2 ProjectSettlementToReferenceMap(Settlement settlement)
	{
		if (settlement != null && TryGetReferenceAnchor(settlement.StringId, out var referenceAnchor))
		{
			return new Vec2(referenceAnchor.x - 80f, referenceAnchor.y - 90f);
		}
		Vec2 vec = ProjectNativeSettlementPosition(settlement?.GetPosition2D ?? new Vec2(0f, 0f));
		float sourceX = vec.x - 80f;
		float sourceY = vec.y - 90f;
		sourceX = Math.Max(0f, Math.Min(1730f, sourceX));
		sourceY = Math.Max(0f, Math.Min(1720f, sourceY));
		return new Vec2(sourceX, sourceY);
	}

	private static Vec2 ProjectNativeSettlementPosition(Vec2 campaignPosition)
	{
		if (!TryGetCampaignToReferenceProjection(out var xCoefficients, out var yCoefficients))
		{
			return new Vec2(campaignPosition.x * 2.23f + 20f, campaignPosition.y * -2.34f + 1985f);
		}
		return new Vec2((float)((double)campaignPosition.x * xCoefficients[0] + (double)campaignPosition.y * xCoefficients[1] + xCoefficients[2]), (float)((double)campaignPosition.x * yCoefficients[0] + (double)campaignPosition.y * yCoefficients[1] + yCoefficients[2]));
	}

	internal static bool TryGetCampaignToReferenceProjection(out double[] xCoefficients, out double[] yCoefficients)
	{
		double[,] normal = new double[3, 3];
		double[] nativeX = new double[3];
		double[] nativeY = new double[3];
		int samples = 0;
		foreach (Settlement candidate in Settlement.All)
		{
			if (candidate == null || candidate.Village != null || !TryGetReferenceAnchor(candidate.StringId, out var anchor))
			{
				continue;
			}
			Vec2 position = candidate.GetPosition2D;
			double[] basis = new double[3] { position.x, position.y, 1.0 };
			for (int row = 0; row < 3; row++)
			{
				for (int column = 0; column < 3; column++)
				{
					normal[row, column] += basis[row] * basis[column];
				}
				nativeX[row] += basis[row] * (double)anchor.x;
				nativeY[row] += basis[row] * (double)anchor.y;
			}
			samples++;
		}
		if (samples < 3 || !TrySolveThreeByThree(normal, nativeX, out xCoefficients) || !TrySolveThreeByThree(normal, nativeY, out yCoefficients))
		{
			xCoefficients = null;
			yCoefficients = null;
			return false;
		}
		return true;
	}

	internal static Vec2 ProjectCampaignPositionToStrategicMap(Vec2 campaignPosition)
	{
		Vec2 projected = ProjectNativeSettlementPosition(campaignPosition);
		return new Vec2(Math.Max(0f, Math.Min(1730f, projected.x - 80f)), Math.Max(0f, Math.Min(1720f, projected.y - 90f)));
	}

	private static bool TrySolveThreeByThree(double[,] matrix, double[] values, out double[] result)
	{
		result = null;
		double[,] augmented = new double[3, 4];
		for (int row3 = 0; row3 < 3; row3++)
		{
			for (int column = 0; column < 3; column++)
			{
				augmented[row3, column] = matrix[row3, column];
			}
			augmented[row3, 3] = values[row3];
		}
		for (int pivot = 0; pivot < 3; pivot++)
		{
			int bestRow = pivot;
			for (int row2 = pivot + 1; row2 < 3; row2++)
			{
				if (Math.Abs(augmented[row2, pivot]) > Math.Abs(augmented[bestRow, pivot]))
				{
					bestRow = row2;
				}
			}
			if (Math.Abs(augmented[bestRow, pivot]) < 1E-06)
			{
				return false;
			}
			if (bestRow != pivot)
			{
				for (int column4 = pivot; column4 < 4; column4++)
				{
					double swap = augmented[pivot, column4];
					augmented[pivot, column4] = augmented[bestRow, column4];
					augmented[bestRow, column4] = swap;
				}
			}
			double divisor = augmented[pivot, pivot];
			for (int column3 = pivot; column3 < 4; column3++)
			{
				augmented[pivot, column3] /= divisor;
			}
			for (int row = 0; row < 3; row++)
			{
				if (row != pivot)
				{
					double factor = augmented[row, pivot];
					for (int column2 = pivot; column2 < 4; column2++)
					{
						augmented[row, column2] -= factor * augmented[pivot, column2];
					}
				}
			}
		}
		result = new double[3]
		{
			augmented[0, 3],
			augmented[1, 3],
			augmented[2, 3]
		};
		return true;
	}

	private void BuildStrategicMarkers(List<StrategicSettlementPoint> points)
	{
		_strategicMarkers.Clear();
		float scaleX = StrategicMapScale;
		float scaleY = StrategicMapScale;
		foreach (StrategicSettlementPoint point in points)
		{
			bool isTown = point.Settlement != null && point.Settlement.IsTown;
			int markerSize = 20;
			int iconSize = (isTown ? 18 : 17);
			float markerHalfSize = (float)markerSize / 2f;
			_strategicMarkers.Add(new CalendarWorldStrategicMarkerVM((int)Math.Round(point.DisplayX * scaleX - markerHalfSize), (int)Math.Round(point.DisplayY * scaleY - markerHalfSize), ToUiColor(point.Owner.Color), point.Settlement, isTown, markerSize, iconSize, showLabel: true, point.IsUnderSiege, string.Equals(point.Settlement.StringId, _selectedStrategicSettlementId, StringComparison.Ordinal), SelectStrategicSettlement));
		}
	}

	private void SelectStrategicSettlement(CalendarWorldStrategicMarkerVM marker)
	{
		if (marker == null || marker.Settlement == null)
		{
			return;
		}
		string previousSelection = _selectedStrategicSettlementId;
		string previousDistanceDestination = _strategicDistanceDestinationSettlementId;
		Diagnostics.Info("Strategic settlement selection started: " + (marker.Settlement.StringId ?? "<no-id>") + " (" + marker.Settlement.Name?.ToString() + ").");
		try
		{
			_selectedStrategicSettlementId = marker.Settlement.StringId ?? string.Empty;
			Settlement distanceOrigin = GetStrategicDistanceOriginSettlement();
			_strategicDistanceDestinationSettlementId = ((distanceOrigin != null && distanceOrigin != marker.Settlement) ? _selectedStrategicSettlementId : string.Empty);
			foreach (CalendarWorldStrategicMarkerVM strategicMarker in _strategicMarkers)
			{
				strategicMarker.IsSelected = strategicMarker == marker;
			}
			RefreshSelectedStrategicVillages();
			Diagnostics.Info("Strategic settlement villages prepared: settlement=" + _selectedStrategicSettlementId + "; count=" + _selectedStrategicVillages.Count + ".");
			StrategicText = BuildStrategicPanelText();
			NotifyStrategicSettlementSelectionChanged();
			Diagnostics.Info("Strategic settlement selection completed: " + _selectedStrategicSettlementId + ".");
		}
		catch (Exception exception)
		{
			_selectedStrategicSettlementId = previousSelection;
			_strategicDistanceDestinationSettlementId = previousDistanceDestination;
			foreach (CalendarWorldStrategicMarkerVM entry in _strategicMarkers)
			{
				entry.IsSelected = entry.Settlement != null && string.Equals(entry.Settlement.StringId, previousSelection, StringComparison.Ordinal);
			}
			RefreshSelectedStrategicVillages();
			StrategicText = BuildStrategicPanelText();
			NotifyStrategicSettlementSelectionChanged();
			Diagnostics.Error("Strategic settlement selection failed and was rolled back.", exception);
		}
	}

	private void RefreshSelectedStrategicVillages()
	{
		_selectedStrategicVillages.Clear();
		Settlement selected = GetSelectedStrategicSettlement();
		if (selected == null || selected.BoundVillages == null)
		{
			return;
		}
		List<Village> villages = new List<Village>();
		foreach (Village village2 in selected.BoundVillages)
		{
			if (village2 != null && village2.Settlement != null)
			{
				villages.Add(village2);
			}
		}
		villages.Sort((Village left, Village right) => string.Compare(left.Settlement.Name.ToString(), right.Settlement.Name.ToString(), StringComparison.CurrentCultureIgnoreCase));
		foreach (Village village in villages)
		{
			_selectedStrategicVillages.Add(new CalendarWorldStrategicVillageVM(village.Settlement, ToggleStrategicVillageTracking));
		}
	}

	private void RefreshStrategicKingdomRows()
	{
		_strategicKingdomRows.Clear();
		if (Campaign.Current == null)
		{
			return;
		}
		List<Kingdom> kingdoms = new List<Kingdom>();
		foreach (Kingdom kingdom2 in Kingdom.All)
		{
			if (kingdom2 != null && !kingdom2.IsEliminated)
			{
				kingdoms.Add(kingdom2);
			}
		}
		kingdoms.Sort((Kingdom left, Kingdom right) => string.Compare((left.Name == null) ? string.Empty : left.Name.ToString(), (right.Name == null) ? string.Empty : right.Name.ToString(), StringComparison.CurrentCultureIgnoreCase));
		for (int index = 0; index < kingdoms.Count; index++)
		{
			Kingdom kingdom = kingdoms[index];
			long wealth = 0L;
			int clanCount = 0;
			foreach (Clan clan in Clan.All)
			{
				if (clan != null && clan.Kingdom == kingdom)
				{
					clanCount++;
					wealth += clan.Gold;
				}
			}
			int fieldedTroops = 0;
			foreach (MobileParty party in MobileParty.All)
			{
				if (party != null && party.MapFaction == kingdom && party.MemberRoster != null)
				{
					fieldedTroops += party.MemberRoster.TotalManCount;
				}
			}
			int townCount = 0;
			int castleCount = 0;
			foreach (Settlement settlement in Settlement.All)
			{
				if (settlement != null && settlement.Town != null && CalendarWorldLedgerBehavior.GetLiveSettlementFaction(settlement) == kingdom)
				{
					if (settlement.IsTown)
					{
						townCount++;
					}
					else if (settlement.IsCastle)
					{
						castleCount++;
					}
				}
			}
			_strategicKingdomRows.Add(new CalendarStrategicKingdomSummaryVM(kingdom, wealth, fieldedTroops, clanCount, townCount, castleCount, index));
		}
	}

	private void ToggleStrategicVillageTracking(CalendarWorldStrategicVillageVM village)
	{
		if (village != null && village.Settlement != null)
		{
			ToggleSettlementTracking(village.Settlement, "village");
			village.RefreshTracking();
			RefreshTrackedStrategicSettlements();
		}
	}

	private void ToggleTrackedStrategicSettlement(CalendarWorldStrategicVillageVM item)
	{
		if (item != null && item.Settlement != null)
		{
			ToggleSettlementTracking(item.Settlement, "settlement-list item");
			RefreshSelectedStrategicVillages();
			RefreshTrackedStrategicSettlements();
			NotifyStrategicSettlementSelectionChanged();
			StrategicText = BuildStrategicPanelText();
		}
	}

	private void RefreshTrackedStrategicSettlements()
	{
		_trackedStrategicSettlements.Clear();
		if (Campaign.Current == null || Campaign.Current.VisualTrackerManager == null)
		{
			OnPropertyChangedWithValue(value: false, "HasTrackedStrategicSettlements");
			OnPropertyChangedWithValue(value: false, "ShowTrackedStrategicSettlements");
			return;
		}
		List<Settlement> tracked = new List<Settlement>();
		foreach (Settlement settlement2 in Settlement.All)
		{
			if (settlement2 != null && (settlement2.Town != null || settlement2.Village != null) && Campaign.Current.VisualTrackerManager.CheckTracked(settlement2))
			{
				tracked.Add(settlement2);
			}
		}
		tracked.Sort((Settlement left, Settlement right) => string.Compare(left.Name.ToString(), right.Name.ToString(), StringComparison.CurrentCultureIgnoreCase));
		foreach (Settlement settlement in tracked)
		{
			_trackedStrategicSettlements.Add(new CalendarWorldStrategicVillageVM(settlement, ToggleTrackedStrategicSettlement));
		}
		OnPropertyChangedWithValue(HasTrackedStrategicSettlements, "HasTrackedStrategicSettlements");
		OnPropertyChangedWithValue(ShowTrackedStrategicSettlements, "ShowTrackedStrategicSettlements");
		Diagnostics.Info("Strategic tracked-settlement list refreshed: count=" + _trackedStrategicSettlements.Count + ".");
	}

	private static void ToggleSettlementTracking(Settlement settlement, string kind)
	{
		if (settlement != null && Campaign.Current != null && Campaign.Current.VisualTrackerManager != null)
		{
			if (Campaign.Current.VisualTrackerManager.CheckTracked(settlement))
			{
				Campaign.Current.VisualTrackerManager.RemoveTrackedObject(settlement);
				Diagnostics.Info("Strategic " + kind + " tracking: untracked " + settlement.StringId + " (" + settlement.Name?.ToString() + ").");
			}
			else
			{
				Campaign.Current.VisualTrackerManager.RegisterObject(settlement);
				Diagnostics.Info("Strategic " + kind + " tracking: tracked " + settlement.StringId + " (" + settlement.Name?.ToString() + ").");
			}
		}
	}

	private Settlement GetSelectedStrategicSettlement()
	{
		return GetSettlementById(_selectedStrategicSettlementId);
	}

	private Settlement GetStrategicDistanceOriginSettlement()
	{
		return GetSettlementById(_strategicDistanceOriginSettlementId);
	}

	private Settlement GetStrategicDistanceDestinationSettlement()
	{
		return GetSettlementById(_strategicDistanceDestinationSettlementId);
	}

	private static Settlement GetSettlementById(string settlementId)
	{
		if (string.IsNullOrEmpty(settlementId))
		{
			return null;
		}
		foreach (Settlement settlement in Settlement.All)
		{
			if (settlement != null && string.Equals(settlement.StringId, settlementId, StringComparison.Ordinal))
			{
				return settlement;
			}
		}
		return null;
	}

	private void NotifyStrategicSettlementSelectionChanged()
	{
		OnPropertyChangedWithValue(HasSelectedStrategicSettlement, "HasSelectedStrategicSettlement");
		OnPropertyChangedWithValue(ShowStrategicKingdomRows, "ShowStrategicKingdomRows");
		OnPropertyChangedWithValue(ShowStrategicSettlementDetails, "ShowStrategicSettlementDetails");
		OnPropertyChangedWithValue(IsSelectedStrategicSettlementTracked, "IsSelectedStrategicSettlementTracked");
		OnPropertyChangedWithValue(TrackSelectedSettlementText, "TrackSelectedSettlementText");
		OnPropertyChangedWithValue(StrategicSummaryScrollerHeight, "StrategicSummaryScrollerHeight");
		OnPropertyChangedWithValue(StrategicSummaryScrollerMarginBottom, "StrategicSummaryScrollerMarginBottom");
		OnPropertyChangedWithValue(StrategicSummaryContentTop, "StrategicSummaryContentTop");
		OnPropertyChangedWithValue(ShowTrackedStrategicSettlements, "ShowTrackedStrategicSettlements");
		NotifyStrategicDistanceChanged();
	}

	private void NotifyStrategicDistanceChanged()
	{
		OnPropertyChangedWithValue(HasStrategicDistanceOrigin, "HasStrategicDistanceOrigin");
		OnPropertyChangedWithValue(HasStrategicDistanceMeasurement, "HasStrategicDistanceMeasurement");
		OnPropertyChangedWithValue(StrategicDistanceActionText, "StrategicDistanceActionText");
		OnPropertyChangedWithValue(StrategicDistanceText, "StrategicDistanceText");
	}

	private void BuildStrategicProvinces(Dictionary<string, IFaction> ownersBySettlementId, List<StrategicSettlementPoint> markerPoints)
	{
		_strategicProvinces.Clear();
		Dictionary<string, uint> ownerColorsBySettlementId = new Dictionary<string, uint>(StringComparer.Ordinal);
		foreach (KeyValuePair<string, IFaction> entry2 in ownersBySettlementId)
		{
			if (!string.IsNullOrEmpty(entry2.Key) && entry2.Value != null)
			{
				ownerColorsBySettlementId[entry2.Key] = NormalizeFactionColor(entry2.Value.Color);
			}
		}
		Dictionary<string, StrategicDemographicProvince> demographicProvinces;
		bool hasDemographicData = StrategicDemographicMapBridge.TryGetSnapshot(out demographicProvinces);
		Dictionary<string, uint> displayColorsBySettlementId = new Dictionary<string, uint>(ownerColorsBySettlementId, StringComparer.Ordinal);
		if (!IsPoliticalMapMode && hasDemographicData)
		{
			foreach (KeyValuePair<string, StrategicDemographicProvince> entry in demographicProvinces)
			{
				uint displayColor2 = (IsCultureMapMode ? StrategicDemographicMapBridge.ResolveCultureColor(entry.Value) : (IsReligionMapMode ? StrategicDemographicMapBridge.ResolveReligionColor(entry.Value) : StrategicDemographicMapBridge.ResolvePopulationColor(entry.Value)));
				displayColorsBySettlementId[entry.Key] = displayColor2;
			}
		}
		float scaleX = StrategicMapScale;
		float scaleY = StrategicMapScale;
		for (int index = 0; index < CalendarStrategicMapLayout.Provinces.Length; index++)
		{
			CalendarStrategicProvinceDefinition province = CalendarStrategicMapLayout.Provinces[index];
			IFaction owner = null;
			string settlementId;
			bool num = CalendarStrategicMapLayout.TryGetSettlementId(province.SpriteName, out settlementId) && ownersBySettlementId.TryGetValue(settlementId, out owner);
			uint displayColor = 0u;
			bool hasDisplayColor = num && displayColorsBySettlementId.TryGetValue(settlementId, out displayColor);
			CalendarWorldStrategicProvinceVM layer = new CalendarWorldStrategicProvinceVM(province.SpriteName, (int)Math.Round(((float)province.X - 80f) * scaleX), (int)Math.Round(((float)province.Y - 90f) * scaleY), Math.Max(1, (int)Math.Ceiling((float)province.Width * scaleX)), Math.Max(1, (int)Math.Ceiling((float)province.Height * scaleY)), hasDisplayColor ? ToProvinceColor(displayColor) : "#00000000");
			_strategicProvinces.Add(layer);
			_fixedStrategicProvinces[index] = layer;
			NotifyFixedStrategicProvince(index, contested: false);
		}
		CalendarStrategicCampaignAtlasTextureProvider.UpdateMapState(displayColorsBySettlementId, markerPoints);
	}

	private void BuildStrategicContestedProvinces(Dictionary<string, IFaction> besiegersBySettlementId)
	{
		_strategicContestedProvinces.Clear();
		float scaleX = StrategicMapScale;
		float scaleY = StrategicMapScale;
		for (int index = 0; index < CalendarStrategicMapLayout.Provinces.Length; index++)
		{
			CalendarStrategicProvinceDefinition province = CalendarStrategicMapLayout.Provinces[index];
			CalendarWorldStrategicProvinceVM layer = new CalendarWorldStrategicProvinceVM(province.SpriteName, (int)Math.Round(((float)province.X - 80f) * scaleX), (int)Math.Round(((float)province.Y - 90f) * scaleY), Math.Max(1, (int)Math.Ceiling((float)province.Width * scaleX)), Math.Max(1, (int)Math.Ceiling((float)province.Height * scaleY)), "#00000000");
			_fixedStrategicContestedProvinces[index] = layer;
			NotifyFixedStrategicProvince(index, contested: true);
		}
	}

	private void SetStrategicMapZoom(float value)
	{
		float zoom = Math.Max(1f, Math.Min(5f, value));
		if (!(Math.Abs(_strategicMapZoom - zoom) < 0.001f))
		{
			_strategicMapZoom = zoom;
			OnPropertyChangedWithValue(MapCanvasWidth, "MapCanvasWidth");
			OnPropertyChangedWithValue(MapCanvasHeight, "MapCanvasHeight");
			OnPropertyChangedWithValue(MapZoomText, "MapZoomText");
			OnPropertyChangedWithValue(CanZoomIn, "CanZoomIn");
			OnPropertyChangedWithValue(CanZoomOut, "CanZoomOut");
			BuildStrategicMapLayers();
		}
	}

	private static string ToUiColor(uint color)
	{
		uint argb = NormalizeFactionColor(color);
		return "#" + Color.UIntToColorString(argb);
	}

	private static string ToProvinceColor(uint color)
	{
		string rgba = Color.UIntToColorString(NormalizeFactionColor(color));
		return "#" + rgba.Substring(0, 6) + "FF";
	}

	private static uint NormalizeFactionColor(uint color)
	{
		if ((color & 0xFF000000u) != 0)
		{
			return color;
		}
		return color | 0xFF000000u;
	}

	private string BuildStrategicPanelText()
	{
		if (string.IsNullOrEmpty(_selectedStrategicSettlementId))
		{
			return BuildKingdomStrengthText();
		}
		Settlement selected = GetSelectedStrategicSettlement();
		if (selected == null || selected.Town == null)
		{
			_selectedStrategicSettlementId = string.Empty;
			return BuildKingdomStrengthText();
		}
		IFaction owner = CalendarWorldLedgerBehavior.GetLiveSettlementFaction(selected);
		IFaction besieger = GetBesiegerFaction(selected);
		StringBuilder text = new StringBuilder();
		text.Append(selected.Name).AppendLine();
		text.Append(selected.IsTown ? "Town" : "Castle").AppendLine();
		text.Append("Kingdom: ").Append((owner == null) ? "Unknown" : owner.Name.ToString()).AppendLine();
		Clan owningClan = selected.OwnerClan;
		text.Append("Owning clan: ").Append((owningClan == null) ? "None" : owningClan.Name.ToString()).AppendLine();
		if (owningClan != null && owningClan.Leader != null)
		{
			text.Append("Clan leader: ").Append(owningClan.Leader.Name.ToString()).AppendLine();
		}
		if (owner != null)
		{
			text.Append("Map colour: ").Append(ToProvinceColor(owner.Color)).AppendLine();
		}
		if (besieger != null)
		{
			text.Append("Status: UNDER SIEGE").AppendLine();
			text.Append("Besieging faction: ").Append(besieger.Name.ToString()).AppendLine();
		}
		text.Append("Tracking: ").Append(IsSelectedStrategicSettlementTracked ? "Yes" : "No").AppendLine();
		text.Append("Bound villages: ").Append((selected.BoundVillages != null) ? selected.BoundVillages.Count : 0).AppendLine();
		if (!CanPlayerInspectSettlement(selected, owner))
		{
			text.AppendLine();
			text.Append("Detailed settlement information is available only for settlements you own or for the faction you serve.");
			return text.ToString();
		}
		Town town = selected.Town;
		text.AppendLine();
		text.Append("Prosperity: ").Append(Math.Round(town.Prosperity)).AppendLine();
		text.Append("Loyalty: ").Append(town.Loyalty.ToString("0.0")).AppendLine();
		text.Append("Security: ").Append(town.Security.ToString("0.0")).AppendLine();
		text.Append("Militia: ").Append(Math.Round(town.Militia)).AppendLine();
		text.Append("Food stocks: ").Append(town.FoodStocks.ToString("0.0"));
		return text.ToString();
	}

	private string BuildStrategicDistanceText()
	{
		Settlement origin = GetStrategicDistanceOriginSettlement();
		if (origin == null)
		{
			return "Select a settlement, choose MEASURE FROM HERE, then select any other town or castle.";
		}
		Settlement destination = GetStrategicDistanceDestinationSettlement();
		if (destination == null || origin == destination)
		{
			return "Origin: " + origin.Name?.ToString() + "\nSelect another town or castle to measure.";
		}
		Vec2 originPoint = ProjectSettlementToReferenceMap(origin);
		Vec2 destinationPoint = ProjectSettlementToReferenceMap(destination);
		double kilometers = CalendarStrategicDistance.CalculateStraightLineKilometers(originPoint.x, originPoint.y, destinationPoint.x, destinationPoint.y);
		return origin.Name?.ToString() + " to " + destination.Name?.ToString() + "\nApprox. straight-line: " + CalendarStrategicDistance.FormatDualDistance(kilometers) + "\nWalking (3-4 mph): " + CalendarStrategicDistance.FormatWalkingCalendarDays(kilometers) + "\nHorse (25-30 mph): " + CalendarStrategicDistance.FormatHorseCalendarDays(kilometers) + "\nShip (4-5 knots): " + CalendarStrategicDistance.FormatShipCalendarDays(kilometers) + "\nLand assumes 8 moving hours; ships sail 24 hours per calendar day.\nLand routes and coastwise sailing will be longer.";
	}

	private static string BuildKingdomStrengthText()
	{
		StringBuilder text = new StringBuilder("KINGDOM SUMMARY\n\n");
		foreach (Kingdom kingdom in Kingdom.All)
		{
			if (kingdom == null)
			{
				continue;
			}
			long wealth = 0L;
			foreach (Clan clan2 in Clan.All)
			{
				if (clan2 != null && clan2.Kingdom == kingdom)
				{
					wealth += clan2.Gold;
				}
			}
			int fieldedTroops = 0;
			foreach (MobileParty party in MobileParty.All)
			{
				if (party != null && party.MapFaction == kingdom && party.MemberRoster != null)
				{
					fieldedTroops += party.MemberRoster.TotalManCount;
				}
			}
			int clanCount = 0;
			foreach (Clan clan in Clan.All)
			{
				if (clan != null && clan.Kingdom == kingdom)
				{
					clanCount++;
				}
			}
			int townCount = 0;
			int castleCount = 0;
			foreach (Settlement settlement in Settlement.All)
			{
				if (settlement != null && settlement.Town != null && CalendarWorldLedgerBehavior.GetLiveSettlementFaction(settlement) == kingdom)
				{
					if (settlement.IsTown)
					{
						townCount++;
					}
					else if (settlement.IsCastle)
					{
						castleCount++;
					}
				}
			}
			text.Append(kingdom.Name).AppendLine();
			text.Append("  Leader: ").Append((kingdom.Leader == null) ? "Unknown" : kingdom.Leader.Name.ToString()).Append(" | Ruling clan: ")
				.Append((kingdom.RulingClan == null) ? "Unknown" : kingdom.RulingClan.Name.ToString())
				.AppendLine();
			text.Append("  Wealth: ").Append(wealth.ToString("N0")).Append(" | Fielded: ")
				.Append(fieldedTroops.ToString("N0"))
				.AppendLine();
			text.Append("  Clans: ").Append(clanCount).Append(" | Towns: ")
				.Append(townCount)
				.Append(" | Castles: ")
				.Append(castleCount)
				.AppendLine();
		}
		text.AppendLine();
		text.Append("Select a town or castle to see its kingdom and owning clan.");
		return text.ToString();
	}

	private static bool CanPlayerInspectSettlement(Settlement settlement, IFaction owner)
	{
		if (settlement == null || owner == null || Clan.PlayerClan == null)
		{
			return false;
		}
		return (Clan.PlayerClan.MapFaction ?? Clan.PlayerClan) == owner;
	}

	private static string BuildStrategicText()
	{
		StringBuilder text = new StringBuilder("LIVE SETTLEMENT OWNERS\n\n");
		int shown = 0;
		foreach (Settlement settlement in Settlement.All)
		{
			if (settlement != null && settlement.Town != null)
			{
				if (shown++ >= 26)
				{
					break;
				}
				IFaction owner = CalendarWorldLedgerBehavior.GetLiveSettlementFaction(settlement);
				text.Append(settlement.Name).Append(" — ").Append((owner == null) ? "Unknown" : owner.Name.ToString())
					.AppendLine();
			}
		}
		return text.ToString();
	}

	private void BuildCalendarHistory()
	{
		_days.Clear();
		_calendarMonths.Clear();
		CampaignTime now = CampaignTime.Now;
		int currentYear = CalendarTimeMath.GetYear(now);
		int currentMonth = CalendarTimeMath.GetMonth(now);
		long today = (long)Math.Floor(CalendarTimeMath.ToCalendarAbsoluteDays(now));
		Dictionary<long, List<QuestBase>> questDueByDay = GetActiveQuestDeadlines(today);
		long futureCalendarEnd = today + 84;
		foreach (long dueDay in questDueByDay.Keys)
		{
			futureCalendarEnd = Math.Max(futureCalendarEnd, dueDay);
		}
		int lastVisibleYear = CalendarTimeMath.GetYear(CalendarTimeMath.FromCalendarAbsoluteDays(futureCalendarEnd));
		int lastVisibleMonth = CalendarTimeMath.GetMonth(CalendarTimeMath.FromCalendarAbsoluteDays(futureCalendarEnd));
		long firstRecordedDay = CalendarWorldLedgerBehavior.GetFirstRecordedDay(today);
		int firstYear = CalendarTimeMath.GetYear(CalendarTimeMath.FromCalendarAbsoluteDays(firstRecordedDay));
		int firstMonth = CalendarTimeMath.GetMonth(CalendarTimeMath.FromCalendarAbsoluteDays(firstRecordedDay));
		if (_displayCalendarYear == int.MinValue || _displayCalendarMonth == int.MinValue)
		{
			_displayCalendarYear = currentYear;
			_displayCalendarMonth = currentMonth;
		}
		ClampDisplayedCalendarMonth(firstRecordedDay, futureCalendarEnd);
		MonthTitle = "Campaign Calendar";
		for (int year = firstYear; year <= lastVisibleYear; year++)
		{
			int num = ((year == firstYear) ? firstMonth : 0);
			int endMonth = ((year == lastVisibleYear) ? lastVisibleMonth : 3);
			for (int month = num; month <= endMonth; month++)
			{
				bool leapYear = CalendarTimeMath.IsLeapYear(year);
				int monthLength = CalendarTimeMath.GetMonthLength(month, leapYear);
				long monthStart = CalendarTimeMath.DaysBeforeYear(year) + CalendarTimeMath.GetMonthStart(month, leapYear);
				MBBindingList<CalendarWorldCalendarDayVM> monthDays = new MBBindingList<CalendarWorldCalendarDayVM>();
				int firstWeekday = (int)((monthStart % 7 + 7) % 7);
				for (int cell = 0; cell < 42; cell++)
				{
					int dayOfMonth = cell - firstWeekday + 1;
					if (dayOfMonth < 1 || dayOfMonth > monthLength)
					{
						monthDays.Add(CalendarWorldCalendarDayVM.Empty());
						continue;
					}
					long absoluteDay = monthStart + dayOfMonth - 1;
					questDueByDay.TryGetValue(absoluteDay, out var dueQuests);
					monthDays.Add(new CalendarWorldCalendarDayVM(dayOfMonth.ToString(), BuildCalendarDaySummary(absoluteDay, dueQuests), absoluteDay, absoluteDay == today, isSelectable: true, absoluteDay == _selectedCalendarDay, dueQuests != null && dueQuests.Count > 0, SelectCalendarDay));
				}
				int eventCount = CalendarWorldLedgerBehavior.CountRecordedEntries(monthStart, monthStart + monthLength);
				int questCount = CountQuestDeadlines(questDueByDay, monthStart, monthStart + monthLength);
				bool isCurrentMonth = year == currentYear && month == currentMonth;
				CalendarWorldCalendarMonthVM monthViewModel = new CalendarWorldCalendarMonthVM(GetCalendarPeriodName(month) + " " + year, BuildMonthEventCountText(eventCount, questCount), monthDays, monthStart, monthStart + monthLength, isCurrentMonth, SelectCalendarMonth);
				_calendarMonths.Add(monthViewModel);
				if (isCurrentMonth)
				{
					SetMonthSummary(monthViewModel);
				}
			}
		}
		BuildSavedSummaries(firstYear, firstMonth, currentYear, currentMonth);
		BuildMonthGrid();
	}

	public void ExecutePreviousCalendarMonth()
	{
		MoveDisplayedCalendarMonth(-1);
	}

	public void ExecuteNextCalendarMonth()
	{
		MoveDisplayedCalendarMonth(1);
	}

	private void MoveDisplayedCalendarMonth(int direction)
	{
		if (!CanMoveDisplayedCalendarMonth(direction))
		{
			Diagnostics.Info("Calendar month navigation ignored: direction=" + direction.ToString("N0") + "; displayed=" + _displayCalendarYear.ToString("N0") + "/" + _displayCalendarMonth.ToString("N0") + ".");
			return;
		}
		if (_selectedCalendarDay != long.MinValue)
		{
			_selectedCalendarDay = long.MinValue;
			OnPropertyChangedWithValue(HasSelectedCalendarDay, "HasSelectedCalendarDay");
			RefreshCalendarNotes();
		}
		_displayCalendarMonth += direction;
		if (_displayCalendarMonth < 0)
		{
			_displayCalendarMonth = 3;
			_displayCalendarYear--;
		}
		else if (_displayCalendarMonth > 3)
		{
			_displayCalendarMonth = 0;
			_displayCalendarYear++;
		}
		BuildMonthGrid();
		OnPropertyChangedWithValue(CanPreviousCalendarMonth, "CanPreviousCalendarMonth");
		OnPropertyChangedWithValue(CanNextCalendarMonth, "CanNextCalendarMonth");
		Diagnostics.Info("Calendar month displayed: year=" + _displayCalendarYear.ToString("N0") + "; month=" + _displayCalendarMonth.ToString("N0") + ".");
	}

	private bool CanMoveDisplayedCalendarMonth(int direction)
	{
		if (direction == 0 || Campaign.Current == null)
		{
			return false;
		}
		int year = _displayCalendarYear;
		int month = _displayCalendarMonth + direction;
		if (month < 0)
		{
			month = 3;
			year--;
		}
		if (month > 3)
		{
			month = 0;
			year++;
		}
		bool leapYear = CalendarTimeMath.IsLeapYear(year);
		long candidateStart = CalendarTimeMath.DaysBeforeYear(year) + CalendarTimeMath.GetMonthStart(month, leapYear);
		long num = (long)Math.Floor(CalendarTimeMath.ToCalendarAbsoluteDays(CampaignTime.Now));
		long firstRecordedDay = CalendarWorldLedgerBehavior.GetFirstRecordedDay(num);
		int firstCalendarYear = CalendarTimeMath.GetYear(CalendarTimeMath.FromCalendarAbsoluteDays(firstRecordedDay));
		int firstCalendarMonth = CalendarTimeMath.GetMonth(CalendarTimeMath.FromCalendarAbsoluteDays(firstRecordedDay));
		bool firstCalendarLeapYear = CalendarTimeMath.IsLeapYear(firstCalendarYear);
		long firstVisibleMonthStart = CalendarTimeMath.DaysBeforeYear(firstCalendarYear) + CalendarTimeMath.GetMonthStart(firstCalendarMonth, firstCalendarLeapYear);
		long lastVisibleDay = GetCalendarFutureEndDay(num);
		if (candidateStart >= firstVisibleMonthStart)
		{
			return candidateStart <= lastVisibleDay;
		}
		return false;
	}

	private void ClampDisplayedCalendarMonth(long firstRecordedDay, long lastVisibleDay)
	{
		int firstCalendarYear = CalendarTimeMath.GetYear(CalendarTimeMath.FromCalendarAbsoluteDays(firstRecordedDay));
		int firstCalendarMonth = CalendarTimeMath.GetMonth(CalendarTimeMath.FromCalendarAbsoluteDays(firstRecordedDay));
		bool firstCalendarLeapYear = CalendarTimeMath.IsLeapYear(firstCalendarYear);
		long firstVisibleMonthStart = CalendarTimeMath.DaysBeforeYear(firstCalendarYear) + CalendarTimeMath.GetMonthStart(firstCalendarMonth, firstCalendarLeapYear);
		bool leapYear = CalendarTimeMath.IsLeapYear(_displayCalendarYear);
		long displayedStart = CalendarTimeMath.DaysBeforeYear(_displayCalendarYear) + CalendarTimeMath.GetMonthStart(_displayCalendarMonth, leapYear);
		if (displayedStart < firstVisibleMonthStart)
		{
			_displayCalendarYear = firstCalendarYear;
			_displayCalendarMonth = firstCalendarMonth;
		}
		else if (displayedStart > lastVisibleDay)
		{
			_displayCalendarYear = CalendarTimeMath.GetYear(CalendarTimeMath.FromCalendarAbsoluteDays(lastVisibleDay));
			_displayCalendarMonth = CalendarTimeMath.GetMonth(CalendarTimeMath.FromCalendarAbsoluteDays(lastVisibleDay));
		}
	}

	private static long GetCalendarFutureEndDay(long today)
	{
		long result = today + 84;
		foreach (long dueDay in GetActiveQuestDeadlines(today).Keys)
		{
			result = Math.Max(result, dueDay);
		}
		return result;
	}

	private void SelectCalendarMonth(CalendarWorldCalendarMonthVM month)
	{
		if (month != null)
		{
			SetMonthSummary(month);
		}
	}

	private void SelectCalendarDay(CalendarWorldCalendarDayVM day)
	{
		if (day == null || !day.IsSelectable)
		{
			return;
		}
		_selectedCalendarDay = day.AbsoluteDay;
		foreach (CalendarWorldCalendarDayVM entry in _days)
		{
			entry.IsSelected = entry.IsSelectable && entry.AbsoluteDay == _selectedCalendarDay;
		}
		OnPropertyChangedWithValue(HasSelectedCalendarDay, "HasSelectedCalendarDay");
		RefreshCalendarNotes();
		Diagnostics.Info("Calendar history day selected: absoluteDay=" + _selectedCalendarDay + ".");
	}

	private void RefreshCalendarNotes()
	{
		long today = ((Campaign.Current == null) ? long.MinValue : ((long)Math.Floor(CalendarTimeMath.ToCalendarAbsoluteDays(CampaignTime.Now))));
		if (_selectedCalendarDay != long.MinValue)
		{
			NotesTitle = CalendarFormatter.Format(CalendarTimeMath.FromCalendarAbsoluteDays(_selectedCalendarDay)).ToUpperInvariant();
			string eventText = ((_selectedCalendarDay <= today) ? CalendarWorldLedgerBehavior.GetImportantEventsText(_selectedCalendarDay, _selectedCalendarDay + 1, int.MaxValue, includeCounts: true) : "No saved world events yet.");
			string questText = GetQuestDeadlineText(_selectedCalendarDay);
			NotesText = (string.IsNullOrEmpty(questText) ? eventText : ("QUEST DEADLINES\n" + questText + "\n\n" + eventText));
		}
		else
		{
			_selectedCalendarDay = long.MinValue;
			NotesTitle = "NOTES";
			NotesText = CalendarWorldLedgerBehavior.GetRecentEntriesText(_selectedFilter);
		}
	}

	private void SetMonthSummary(CalendarWorldCalendarMonthVM month)
	{
		SetMonthSummary(month.Title, month.StartDay, month.EndDay);
	}

	private void SetMonthSummary(string monthTitle, long monthStart, long monthEnd)
	{
		int monthEvents = CalendarWorldLedgerBehavior.CountRecordedEntries(monthStart, monthEnd);
		int questCount = CountQuestDeadlines(GetActiveQuestDeadlines((long)Math.Floor(CalendarTimeMath.ToCalendarAbsoluteDays(CampaignTime.Now))), monthStart, monthEnd);
		int monthCapacity = Math.Max(1, (int)(monthEnd - monthStart));
		int selectedMonthEvents = Math.Min(monthCapacity, monthEvents);
		MonthSummaryTitle = monthTitle + " " + CalendarPeriodSummaryLabel + " (" + selectedMonthEvents + "/" + monthCapacity + "; " + questCount + " quest deadlines)";
		string eventText = CalendarWorldLedgerBehavior.GetImportantEventsText(monthStart, monthEnd, monthCapacity, includeCounts: true);
		string questText = GetQuestDeadlineText(monthStart, monthEnd);
		MonthSummaryText = (string.IsNullOrEmpty(questText) ? eventText : ("QUEST DEADLINES\n" + questText + "\n\n" + eventText));
		int year = CalendarTimeMath.GetYear(CalendarTimeMath.FromCalendarAbsoluteDays(monthStart));
		YearSummaryText = BuildYearImportantSummary(year, 0, 3, out var importantEventCount, out var _);
		YearSummaryTitle = year + " YEARLY SUMMARY (" + importantEventCount + "/" + 40 + ")";
	}

	private void BuildSavedSummaries(int firstYear, int firstMonth, int currentYear, int currentMonth)
	{
		Dictionary<string, bool> expandedByTitle = new Dictionary<string, bool>(StringComparer.Ordinal);
		for (int index3 = 0; index3 < _savedSummaries.Count; index3++)
		{
			CalendarWorldSavedSummaryVM existing = _savedSummaries[index3];
			if (existing != null && existing.IsExpanded)
			{
				expandedByTitle[GetSavedSummaryIdentity(existing.Title)] = true;
			}
		}
		List<CalendarWorldSavedSummaryVM> refreshed = new List<CalendarWorldSavedSummaryVM>();
		for (int year = currentYear; year >= firstYear; year--)
		{
			int startMonth = ((year == firstYear) ? firstMonth : 0);
			int endMonth = ((year == currentYear) ? currentMonth : 3);
			int yearImportantCount;
			int monthsWithEvents;
			string yearText = BuildYearImportantSummary(year, startMonth, endMonth, out yearImportantCount, out monthsWithEvents);
			int yearBodyHeight = ((yearImportantCount == 0) ? 86 : Math.Max(150, 70 + yearImportantCount * 20 + monthsWithEvents * 28));
			string yearTitle = year + " YEARLY SUMMARY — " + yearImportantCount + "/" + 40 + " important events";
			refreshed.Add(new CalendarWorldSavedSummaryVM(yearTitle, yearText, yearBodyHeight, expandedByTitle.ContainsKey(GetSavedSummaryIdentity(yearTitle))));
			for (int month = endMonth; month >= startMonth; month--)
			{
				bool leapYear = CalendarTimeMath.IsLeapYear(year);
				int monthLength = CalendarTimeMath.GetMonthLength(month, leapYear);
				long num = CalendarTimeMath.DaysBeforeYear(year) + CalendarTimeMath.GetMonthStart(month, leapYear);
				int eventCount = CalendarWorldLedgerBehavior.CountRecordedEntries(num, num + monthLength);
				string monthText = CalendarWorldLedgerBehavior.GetImportantEventsText(num, num + monthLength, monthLength, includeCounts: true);
				int monthBodyHeight = ((eventCount == 0) ? 86 : Math.Max(130, 70 + Math.Min(monthLength, eventCount) * 22));
				string monthTitle = GetCalendarPeriodName(month) + " " + year + " " + CalendarPeriodSummaryLabel + " — " + Math.Min(monthLength, eventCount) + "/" + monthLength + " important events";
				refreshed.Add(new CalendarWorldSavedSummaryVM(monthTitle, monthText, monthBodyHeight, expandedByTitle.ContainsKey(GetSavedSummaryIdentity(monthTitle))));
			}
		}
		bool changed = refreshed.Count != _savedSummaries.Count;
		int index2 = 0;
		while (!changed && index2 < refreshed.Count)
		{
			CalendarWorldSavedSummaryVM current = _savedSummaries[index2];
			CalendarWorldSavedSummaryVM next = refreshed[index2];
			changed = current == null || !string.Equals(current.Title, next.Title, StringComparison.Ordinal) || !string.Equals(current.SummaryText, next.SummaryText, StringComparison.Ordinal) || current.BodyHeight != next.BodyHeight;
			index2++;
		}
		if (changed)
		{
			_savedSummaries.Clear();
			for (int index = 0; index < refreshed.Count; index++)
			{
				_savedSummaries.Add(refreshed[index]);
			}
		}
	}

	private static string GetSavedSummaryIdentity(string title)
	{
		if (string.IsNullOrEmpty(title))
		{
			return string.Empty;
		}
		int countSeparator = title.IndexOf(" — ", StringComparison.Ordinal);
		if (countSeparator >= 0)
		{
			return title.Substring(0, countSeparator);
		}
		return title;
	}

	private static string BuildYearImportantSummary(int year, int startMonth, int endMonth, out int importantEventCount, out int monthsWithEvents)
	{
		StringBuilder text = new StringBuilder();
		importantEventCount = 0;
		monthsWithEvents = 0;
		startMonth = Math.Max(0, Math.Min(3, startMonth));
		endMonth = Math.Max(0, Math.Min(3, endMonth));
		if (startMonth > endMonth)
		{
			return "No events were recorded for this year.";
		}
		for (int month = startMonth; month <= endMonth; month++)
		{
			bool leapYear = CalendarTimeMath.IsLeapYear(year);
			int monthLength = CalendarTimeMath.GetMonthLength(month, leapYear);
			long monthStart = CalendarTimeMath.DaysBeforeYear(year) + CalendarTimeMath.GetMonthStart(month, leapYear);
			int monthEventCount = CalendarWorldLedgerBehavior.CountRecordedEntries(monthStart, monthStart + monthLength);
			if (monthEventCount > 0)
			{
				monthsWithEvents++;
				int selectedCount = Math.Min(10, monthEventCount);
				importantEventCount += selectedCount;
				if (text.Length > 0)
				{
					text.AppendLine().AppendLine();
				}
				text.Append(GetCalendarPeriodName(month).ToUpperInvariant()).Append(" — ").Append(selectedCount)
					.Append(" important events")
					.AppendLine();
				text.Append(CalendarWorldLedgerBehavior.GetImportantEventsText(monthStart, monthStart + monthLength, 10, includeCounts: false));
			}
		}
		if (text.Length != 0)
		{
			return text.ToString();
		}
		return "No events were recorded for this year.";
	}

	private void BuildMonthGrid()
	{
		_days.Clear();
		CampaignTime now = CampaignTime.Now;
		int year = _displayCalendarYear;
		int month = _displayCalendarMonth;
		bool leapYear = CalendarTimeMath.IsLeapYear(year);
		int monthLength = CalendarTimeMath.GetMonthLength(month, leapYear);
		long monthStart = CalendarTimeMath.DaysBeforeYear(year) + CalendarTimeMath.GetMonthStart(month, leapYear);
		int firstWeekday = (int)((monthStart % 7 + 7) % 7);
		long today = (long)Math.Floor(CalendarTimeMath.ToCalendarAbsoluteDays(now));
		Dictionary<long, List<QuestBase>> questDueByDay = GetActiveQuestDeadlines(today);
		MonthTitle = GetCalendarPeriodName(month) + " " + year;
		for (int cell = 0; cell < 42; cell++)
		{
			int dayOfMonth = cell - firstWeekday + 1;
			CalendarWorldCalendarDayVM cellValue;
			if (dayOfMonth < 1 || dayOfMonth > monthLength)
			{
				cellValue = CalendarWorldCalendarDayVM.Empty();
			}
			else
			{
				long absoluteDay = monthStart + dayOfMonth - 1;
				questDueByDay.TryGetValue(absoluteDay, out var dueQuests);
				cellValue = new CalendarWorldCalendarDayVM(dayOfMonth.ToString(), BuildCalendarDaySummary(absoluteDay, dueQuests), absoluteDay, absoluteDay == today, isSelectable: true, absoluteDay == _selectedCalendarDay, dueQuests != null && dueQuests.Count > 0, SelectCalendarDay);
			}
			_days.Add(cellValue);
		}
		SetMonthSummary(MonthTitle, monthStart, monthStart + monthLength);
	}

	private static Dictionary<long, List<QuestBase>> GetActiveQuestDeadlines(long today)
	{
		Dictionary<long, List<QuestBase>> result = new Dictionary<long, List<QuestBase>>();
		if (Campaign.Current == null || Campaign.Current.QuestManager == null)
		{
			return result;
		}
		foreach (QuestBase quest in Campaign.Current.QuestManager.Quests)
		{
			if (quest == null || !quest.IsOngoing || quest.IsRemainingTimeHidden)
			{
				continue;
			}
			double dueDays = CalendarTimeMath.ToCalendarAbsoluteDays(quest.QuestDueTime);
			if (double.IsNaN(dueDays) || double.IsInfinity(dueDays))
			{
				continue;
			}
			long dueDay = (long)Math.Floor(dueDays);
			if (dueDay >= today - 1 && dueDay <= today + 3650)
			{
				if (!result.TryGetValue(dueDay, out var dueQuests))
				{
					dueQuests = new List<QuestBase>();
					result.Add(dueDay, dueQuests);
				}
				dueQuests.Add(quest);
			}
		}
		return result;
	}

	private static int CountQuestDeadlines(Dictionary<long, List<QuestBase>> dueByDay, long startDay, long endDay)
	{
		int count = 0;
		foreach (KeyValuePair<long, List<QuestBase>> entry in dueByDay)
		{
			if (entry.Key >= startDay && entry.Key < endDay)
			{
				count += entry.Value.Count;
			}
		}
		return count;
	}

	private static string BuildMonthEventCountText(int eventCount, int questCount)
	{
		string events = ((eventCount == 1) ? "1 saved event" : (eventCount + " saved events"));
		if (questCount != 0)
		{
			return events + "; " + ((questCount == 1) ? "1 quest due" : (questCount + " quests due"));
		}
		return events;
	}

	private static string GetCalendarPeriodName(int period)
	{
		return CalendarSettingsState.GetSeasonName(period);
	}

	private static string BuildCalendarDaySummary(long absoluteDay, List<QuestBase> dueQuests)
	{
		if (dueQuests != null && dueQuests.Count > 0)
		{
			if (dueQuests.Count != 1)
			{
				return dueQuests.Count + " QUESTS DUE";
			}
			return "QUEST DUE";
		}
		return CalendarWorldLedgerBehavior.GetDaySummary(absoluteDay, "All");
	}

	private static string GetQuestDeadlineText(long day)
	{
		return GetQuestDeadlineText(day, day + 1);
	}

	private static string GetQuestDeadlineText(long startDay, long endDay)
	{
		Dictionary<long, List<QuestBase>> activeQuestDeadlines = GetActiveQuestDeadlines((long)Math.Floor(CalendarTimeMath.ToCalendarAbsoluteDays(CampaignTime.Now)));
		StringBuilder text = new StringBuilder();
		foreach (KeyValuePair<long, List<QuestBase>> entry in activeQuestDeadlines)
		{
			if (entry.Key < startDay || entry.Key >= endDay)
			{
				continue;
			}
			foreach (QuestBase quest in entry.Value)
			{
				if (text.Length > 0)
				{
					text.AppendLine();
				}
				text.Append(CalendarFormatter.Format(CalendarTimeMath.FromCalendarAbsoluteDays(entry.Key))).Append(" — ");
				text.Append((quest.Title == null) ? "Unnamed quest" : quest.Title.ToString());
			}
		}
		return text.ToString();
	}

	private void NotifyFixedStrategicProvince(int index, bool contested)
	{
		OnPropertyChangedWithValue(contested ? _fixedStrategicContestedProvinces[index] : _fixedStrategicProvinces[index], FixedStrategicProvincePropertyNames[index * 2 + (contested ? 1 : 0)]);
	}

	private void BuildStrategicKingdomLabels(List<StrategicSettlementPoint> points)
	{
		_strategicKingdomLabels.Clear();
		Dictionary<Kingdom, List<StrategicSettlementPoint>> holdingsByKingdom = new Dictionary<Kingdom, List<StrategicSettlementPoint>>();
		foreach (StrategicSettlementPoint point2 in points)
		{
			Kingdom kingdom = ((point2 == null || point2.Settlement == null || point2.Settlement.OwnerClan == null) ? null : point2.Settlement.OwnerClan.Kingdom);
			if (kingdom != null && !kingdom.IsEliminated)
			{
				if (!holdingsByKingdom.TryGetValue(kingdom, out var holdings))
				{
					holdings = new List<StrategicSettlementPoint>();
					holdingsByKingdom.Add(kingdom, holdings);
				}
				holdings.Add(point2);
			}
		}
		List<Vec2> occupied = new List<Vec2>();
		List<KeyValuePair<Kingdom, List<StrategicSettlementPoint>>> list = new List<KeyValuePair<Kingdom, List<StrategicSettlementPoint>>>(holdingsByKingdom);
		list.Sort(delegate(KeyValuePair<Kingdom, List<StrategicSettlementPoint>> left, KeyValuePair<Kingdom, List<StrategicSettlementPoint>> right)
		{
			int num = right.Value.Count.CompareTo(left.Value.Count);
			return (num == 0) ? string.CompareOrdinal(left.Key.StringId, right.Key.StringId) : num;
		});
		foreach (KeyValuePair<Kingdom, List<StrategicSettlementPoint>> entry in list)
		{
			List<StrategicSettlementPoint> cluster = FindDominantKingdomCluster(entry.Value);
			if (cluster.Count == 0)
			{
				continue;
			}
			float sourceX = 0f;
			float sourceY = 0f;
			foreach (StrategicSettlementPoint point in cluster)
			{
				sourceX += point.SourceX;
				sourceY += point.SourceY;
			}
			sourceX /= (float)cluster.Count;
			sourceY /= (float)cluster.Count;
			float x = sourceX * StrategicMapScale;
			float y = sourceY * StrategicMapScale;
			ResolveStrategicLabelOverlap(ref x, ref y, occupied);
			occupied.Add(new Vec2(x, y));
			_strategicKingdomLabels.Add(new CalendarStrategicKingdomLabelVM(entry.Key.Name.ToString().ToUpperInvariant(), (int)Math.Round(x - 150f), (int)Math.Round(y - 20f), cluster.Count));
		}
	}

	private static List<StrategicSettlementPoint> FindDominantKingdomCluster(List<StrategicSettlementPoint> holdings)
	{
		List<StrategicSettlementPoint> dominant = new List<StrategicSettlementPoint>();
		HashSet<StrategicSettlementPoint> remaining = new HashSet<StrategicSettlementPoint>(holdings);
		float linkSquared = 115600f;
		while (remaining.Count > 0)
		{
			StrategicSettlementPoint seed = null;
			using (HashSet<StrategicSettlementPoint>.Enumerator enumerator = remaining.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					seed = enumerator.Current;
				}
			}
			List<StrategicSettlementPoint> cluster = new List<StrategicSettlementPoint>();
			Queue<StrategicSettlementPoint> frontier = new Queue<StrategicSettlementPoint>();
			remaining.Remove(seed);
			frontier.Enqueue(seed);
			while (frontier.Count > 0)
			{
				StrategicSettlementPoint current = frontier.Dequeue();
				cluster.Add(current);
				List<StrategicSettlementPoint> connected = new List<StrategicSettlementPoint>();
				foreach (StrategicSettlementPoint candidate2 in remaining)
				{
					float num = current.SourceX - candidate2.SourceX;
					float dy = current.SourceY - candidate2.SourceY;
					if (num * num + dy * dy <= linkSquared)
					{
						connected.Add(candidate2);
					}
				}
				foreach (StrategicSettlementPoint candidate in connected)
				{
					remaining.Remove(candidate);
					frontier.Enqueue(candidate);
				}
			}
			if (cluster.Count > dominant.Count || (cluster.Count == dominant.Count && string.CompareOrdinal(GetClusterKey(cluster), GetClusterKey(dominant)) < 0))
			{
				dominant = cluster;
			}
		}
		return dominant;
	}

	private static string GetClusterKey(List<StrategicSettlementPoint> cluster)
	{
		string key = null;
		foreach (StrategicSettlementPoint point in cluster)
		{
			string id = ((point == null || point.Settlement == null) ? string.Empty : point.Settlement.StringId);
			if (key == null || string.CompareOrdinal(id, key) < 0)
			{
				key = id;
			}
		}
		return key ?? string.Empty;
	}

	private static void ResolveStrategicLabelOverlap(ref float x, ref float y, List<Vec2> occupied)
	{
		for (int attempt = 0; attempt < 8; attempt++)
		{
			bool overlaps = false;
			foreach (Vec2 position in occupied)
			{
				if (Math.Abs(position.x - x) < 230f && Math.Abs(position.y - y) < 44f)
				{
					overlaps = true;
					break;
				}
			}
			if (!overlaps)
			{
				break;
			}
			y += ((attempt % 2 == 0) ? 46f : (-92f));
		}
	}

	internal void RefreshStrategicFriendlyArmies()
	{
		_strategicFriendlyArmies.Clear();
		IFaction playerFaction = ((Clan.PlayerClan == null) ? null : (Clan.PlayerClan.MapFaction ?? Clan.PlayerClan));
		if (playerFaction == null)
		{
			return;
		}
		List<MobileParty> visibleParties = new List<MobileParty>();
		foreach (MobileParty party2 in MobileParty.All)
		{
			if (party2 != null && party2.IsActive)
			{
				bool num = party2 == MobileParty.MainParty;
				bool isClanParty2 = Clan.PlayerClan != null && party2.ActualClan == Clan.PlayerClan;
				bool isFriendlyArmyLeader = party2.Army != null && party2.Army.LeaderParty == party2 && IsFriendlyArmyFaction(playerFaction, party2.MapFaction);
				if (num || isClanParty2 || isFriendlyArmyLeader)
				{
					visibleParties.Add(party2);
				}
			}
		}
		visibleParties.Sort(delegate(MobileParty left, MobileParty right)
		{
			string strA = ((left == null) ? string.Empty : left.StringId);
			string strB = ((right == null) ? string.Empty : right.StringId);
			return string.CompareOrdinal(strA, strB);
		});
		foreach (MobileParty party in visibleParties)
		{
			CampaignVec2 position = party.Position;
			Vec2 source = ProjectCampaignPositionToStrategicMap(new Vec2(position.X, position.Y));
			bool num2 = party == MobileParty.MainParty;
			bool isClanParty = Clan.PlayerClan != null && party.ActualClan == Clan.PlayerClan;
			string partyName = (num2 ? "Your Party" : party.Name.ToString());
			string glyph = (num2 ? "P" : (isClanParty ? "C" : "A"));
			string markerColor = (num2 ? "#72B7FFFF" : (isClanParty ? "#7DE8B0FF" : "#65E89AFF"));
			_strategicFriendlyArmies.Add(new CalendarStrategicFriendlyArmyVM((int)Math.Round(source.x * StrategicMapScale - 10f), (int)Math.Round(source.y * StrategicMapScale - 15f), partyName, glyph, markerColor));
		}
	}

	private static bool IsFriendlyArmyFaction(IFaction playerFaction, IFaction candidateFaction)
	{
		if (candidateFaction != null)
		{
			if (playerFaction != candidateFaction)
			{
				return DiplomacyHelper.HasAllianceWithFaction(playerFaction, candidateFaction);
			}
			return true;
		}
		return false;
	}
}
}

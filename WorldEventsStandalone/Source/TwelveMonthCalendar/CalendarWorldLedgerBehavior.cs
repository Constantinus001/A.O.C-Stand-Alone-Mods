using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace TwelveMonthCalendar
{

internal sealed class CalendarWorldLedgerBehavior : CampaignBehaviorBase
{
	private sealed class ImportantLedgerEntry
	{
		internal long Day { get; private set; }

		internal string Category { get; private set; }

		internal string Message { get; private set; }

		internal int Importance { get; private set; }

		internal int Sequence { get; private set; }

		internal ImportantLedgerEntry(long day, string category, string message, int importance, int sequence)
		{
			Day = day;
			Category = category ?? string.Empty;
			Message = message ?? string.Empty;
			Importance = importance;
			Sequence = sequence;
		}
	}

	private const string LedgerKey = "AgesOfCalradia.WorldLedgerV1";

	private const string SettlementOwnershipKey = "AgesOfCalradia.SettlementOwnershipV1";

	private const string CharacterOriginStoryKey = "AgesOfCalradia.CharacterOriginStoryV1";

	private const string CharacterMilestonesKey = "AgesOfCalradia.CharacterMilestonesV1";

	private const string CharacterMilestoneStateKey = "AgesOfCalradia.CharacterMilestoneStateV2";

	private const string CombatStatisticsKey = "AgesOfCalradia.HeroCombatStatisticsV1";

	private const string WarScoreKey = "AgesOfCalradia.WarScoreV1";

	private const string NavalWarLossesKey = "AgesOfCalradia.NavalWarLossesV1";

	private const string WarOccupationKey = "AgesOfCalradia.WarOccupationsV1";

	private const string MessengerTravelKey = "AgesOfCalradia.MessengerTravelV1";

	private const string MarriageMessengerTravelKey = "AgesOfCalradia.MarriageMessengerTravelV1";

	private List<string> _entries = new List<string>();

	private List<string> _settlementOwners = new List<string>();

	private string _characterOriginStory = string.Empty;

	private List<string> _characterMilestones = new List<string>();

	private string _characterMilestoneState = string.Empty;

	private List<string> _combatStatistics = new List<string>();

	private List<string> _warScores = new List<string>();

	private List<string> _navalWarLosses = new List<string>();

	private List<string> _warOccupations = new List<string>();

	private List<string> _messengerTravels = new List<string>();

	private List<string> _marriageMessengerTravels = new List<string>();

	private static CalendarWorldLedgerBehavior _active;

	private static int _ownershipRevision;

	internal static int OwnershipRevision => _ownershipRevision;

	public override void RegisterEvents()
	{
		_active = this;
		CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
		CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
		CampaignEvents.WarDeclared.AddNonSerializedListener(this, OnWarDeclared);
		CampaignEvents.MakePeace.AddNonSerializedListener(this, OnPeaceMade);
		CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener(this, OnSettlementOwnerChanged);
		CampaignEvents.MapEventEnded.AddNonSerializedListener(this, OnMapEventEndedForWarScore);
		CampaignEvents.OnShipDestroyedEvent.AddNonSerializedListener(this, OnShipDestroyed);
		CampaignEvents.OnGivenBirthEvent.AddNonSerializedListener(this, OnGivenBirth);
		CampaignEvents.HeroKilledEvent.AddNonSerializedListener(this, OnHeroKilled);
		CampaignEvents.OnCharacterCreationIsOverEvent.AddNonSerializedListener(this, OnCharacterCreationIsOver);
	}

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData("AgesOfCalradia.WorldLedgerV1", ref _entries);
		dataStore.SyncData("AgesOfCalradia.SettlementOwnershipV1", ref _settlementOwners);
		dataStore.SyncData("AgesOfCalradia.CharacterOriginStoryV1", ref _characterOriginStory);
		dataStore.SyncData("AgesOfCalradia.CharacterMilestonesV1", ref _characterMilestones);
		dataStore.SyncData("AgesOfCalradia.CharacterMilestoneStateV2", ref _characterMilestoneState);
		dataStore.SyncData("AgesOfCalradia.HeroCombatStatisticsV1", ref _combatStatistics);
		dataStore.SyncData("AgesOfCalradia.WarScoreV1", ref _warScores);
		dataStore.SyncData("AgesOfCalradia.NavalWarLossesV1", ref _navalWarLosses);
		dataStore.SyncData("AgesOfCalradia.WarOccupationsV1", ref _warOccupations);
		dataStore.SyncData("AgesOfCalradia.MessengerTravelV1", ref _messengerTravels);
		dataStore.SyncData("AgesOfCalradia.MarriageMessengerTravelV1", ref _marriageMessengerTravels);
		_entries = _entries ?? new List<string>();
		_settlementOwners = _settlementOwners ?? new List<string>();
		_characterOriginStory = _characterOriginStory ?? string.Empty;
		_characterMilestones = _characterMilestones ?? new List<string>();
		_characterMilestoneState = _characterMilestoneState ?? string.Empty;
		_combatStatistics = _combatStatistics ?? new List<string>();
		_warScores = _warScores ?? new List<string>();
		_navalWarLosses = _navalWarLosses ?? new List<string>();
		_warOccupations = _warOccupations ?? new List<string>();
		_messengerTravels = _messengerTravels ?? new List<string>();
		_marriageMessengerTravels = _marriageMessengerTravels ?? new List<string>();
	}

	internal static int GetMarriageMessengerDaysRemaining(Hero targetHero, string purpose)
	{
		if (_active == null || targetHero == null || string.IsNullOrEmpty(targetHero.StringId) || string.IsNullOrEmpty(purpose))
		{
			return -1;
		}
		if (!_active.TryGetMarriageMessengerArrivalDay(purpose, targetHero.StringId, out var arrivalDay))
		{
			return -1;
		}
		return Math.Max(0, (int)Math.Ceiling(arrivalDay - CampaignTime.Now.ToDays));
	}

	internal static bool DispatchMarriageMessenger(Hero targetHero, string purpose, out int travelDays)
	{
		travelDays = 0;
		if (_active == null || targetHero == null || string.IsNullOrEmpty(targetHero.StringId) || string.IsNullOrEmpty(purpose))
		{
			return false;
		}
		if (_active.TryGetMarriageMessengerArrivalDay(purpose, targetHero.StringId, out var _))
		{
			return false;
		}
		travelDays = CalculateMessengerTravelDays(targetHero, (targetHero.Clan == null) ? null : targetHero.Clan.Kingdom);
		_active._marriageMessengerTravels.Add(purpose + "\t" + targetHero.StringId + "\t" + (CampaignTime.Now.ToDays + (double)travelDays).ToString("R", CultureInfo.InvariantCulture) + "\t0");
		return true;
	}

	internal static bool ConsumeArrivedMarriageMessenger(Hero targetHero, string purpose)
	{
		if (_active == null || targetHero == null || string.IsNullOrEmpty(targetHero.StringId) || string.IsNullOrEmpty(purpose))
		{
			return false;
		}
		for (int i = 0; i < _active._marriageMessengerTravels.Count; i++)
		{
			string[] array = _active._marriageMessengerTravels[i].Split('\t');
			if (array.Length == 4 && string.Equals(array[0], purpose, StringComparison.Ordinal) && string.Equals(array[1], targetHero.StringId, StringComparison.Ordinal))
			{
				if (!double.TryParse(array[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var result) || CampaignTime.Now.ToDays < result)
				{
					return false;
				}
				_active._marriageMessengerTravels.RemoveAt(i);
				return true;
			}
		}
		return false;
	}

	private bool TryGetMarriageMessengerArrivalDay(string purpose, string heroId, out double arrivalDay)
	{
		arrivalDay = 0.0;
		foreach (string marriageMessengerTravel in _marriageMessengerTravels)
		{
			string[] array = marriageMessengerTravel.Split('\t');
			if (array.Length == 4 && string.Equals(array[0], purpose, StringComparison.Ordinal) && string.Equals(array[1], heroId, StringComparison.Ordinal))
			{
				return double.TryParse(array[2], NumberStyles.Float, CultureInfo.InvariantCulture, out arrivalDay);
			}
		}
		return false;
	}

	internal static int GetMessengerDaysRemaining(Kingdom targetKingdom)
	{
		if (_active == null || targetKingdom == null || string.IsNullOrEmpty(targetKingdom.StringId))
		{
			return -1;
		}
		if (!_active.TryGetMessengerArrivalDay(targetKingdom.StringId, out var arrivalDay))
		{
			return -1;
		}
		return Math.Max(0, (int)Math.Ceiling(arrivalDay - CampaignTime.Now.ToDays));
	}

	internal static bool DispatchMessenger(Kingdom targetKingdom, out int travelDays)
	{
		travelDays = 0;
		if (_active == null || targetKingdom == null || string.IsNullOrEmpty(targetKingdom.StringId))
		{
			return false;
		}
		if (_active.TryGetMessengerArrivalDay(targetKingdom.StringId, out var _))
		{
			return false;
		}
		travelDays = CalculateMessengerTravelDays(targetKingdom.Leader, targetKingdom);
		_active._messengerTravels.Add(targetKingdom.StringId + "\t" + (CampaignTime.Now.ToDays + (double)travelDays).ToString("R", CultureInfo.InvariantCulture));
		return true;
	}

	private static int CalculateMessengerTravelDays(Hero ruler, Kingdom targetKingdom)
	{
		Campaign current = Campaign.Current;
		MobileParty mainParty = MobileParty.MainParty;
		if (current == null || current.Models == null || current.Models.MapDistanceModel == null || mainParty == null || ruler == null)
		{
			return 1;
		}
		float num = -1f;
		try
		{
			float landRatio;
			if (ruler.PartyBelongedTo != null)
			{
				num = current.Models.MapDistanceModel.GetDistance(mainParty, ruler.PartyBelongedTo, MobileParty.NavigationType.All, out landRatio);
			}
			else
			{
				Settlement settlement = ruler.CurrentSettlement ?? ruler.LastKnownClosestSettlement ?? targetKingdom?.FactionMidSettlement;
				if (settlement != null)
				{
					num = current.Models.MapDistanceModel.GetDistance(mainParty, settlement, isTargetingPort: false, MobileParty.NavigationType.All, out landRatio);
				}
			}
		}
		catch (Exception exception)
		{
			Diagnostics.Error("Messenger route-distance calculation failed; using map-position fallback.", exception);
		}
		if (num < 0f || float.IsNaN(num) || float.IsInfinity(num) || num >= 100000000f)
		{
			Vec2 v = ((ruler.PartyBelongedTo != null) ? ruler.PartyBelongedTo.GetPosition2D : (((ruler.CurrentSettlement ?? ruler.LastKnownClosestSettlement ?? targetKingdom?.FactionMidSettlement) == null) ? mainParty.GetPosition2D : (ruler.CurrentSettlement ?? ruler.LastKnownClosestSettlement ?? targetKingdom?.FactionMidSettlement).GetPosition2D));
			num = mainParty.GetPosition2D.Distance(v);
		}
		float num2 = Math.Max(1f, current.EstimatedAverageLordPartySpeed) * (float)CampaignTime.HoursInDay;
		return Math.Max(1, (int)Math.Ceiling(num / num2));
	}

	internal static bool ConsumeArrivedMessenger(Kingdom targetKingdom)
	{
		if (_active == null || targetKingdom == null || string.IsNullOrEmpty(targetKingdom.StringId))
		{
			return false;
		}
		for (int i = 0; i < _active._messengerTravels.Count; i++)
		{
			string[] array = _active._messengerTravels[i].Split('\t');
			if (array.Length == 2 && string.Equals(array[0], targetKingdom.StringId, StringComparison.Ordinal))
			{
				if (!double.TryParse(array[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var result) || CampaignTime.Now.ToDays < result)
				{
					return false;
				}
				_active._messengerTravels.RemoveAt(i);
				return true;
			}
		}
		return false;
	}

	private bool TryGetMessengerArrivalDay(string kingdomId, out double arrivalDay)
	{
		arrivalDay = 0.0;
		foreach (string messengerTravel in _messengerTravels)
		{
			string[] array = messengerTravel.Split('\t');
			if (array.Length == 2 && string.Equals(array[0], kingdomId, StringComparison.Ordinal))
			{
				return double.TryParse(array[1], NumberStyles.Float, CultureInfo.InvariantCulture, out arrivalDay);
			}
		}
		return false;
	}

	internal static string GetCharacterOriginStory()
	{
		if (_active != null)
		{
			return _active._characterOriginStory;
		}
		return string.Empty;
	}

	internal static string GetCharacterMilestoneStory(Hero hero)
	{
		if (_active == null || hero == null || string.IsNullOrEmpty(hero.StringId) || _active._characterMilestones == null)
		{
			return string.Empty;
		}
		List<string> list = new List<string>();
		foreach (string characterMilestone in _active._characterMilestones)
		{
			string[] array = characterMilestone.Split(new char[1] { '\t' }, 3);
			if (array.Length == 3 && string.Equals(array[0], hero.StringId, StringComparison.Ordinal) && double.TryParse(array[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				string text = CalendarFormatter.Format(CampaignTime.Days((float)result));
				list.Add("• " + (string.IsNullOrWhiteSpace(text) ? "Recorded" : text) + " — " + array[2]);
			}
		}
		if (list.Count > 40)
		{
			list.RemoveRange(0, list.Count - 40);
		}
		return string.Join("\n\n", list.ToArray());
	}

	internal static void RepairStandaloneInitialCharacterFacts(Hero hero)
	{
		if (_active == null || hero == null || string.IsNullOrEmpty(hero.StringId) || _active._characterMilestones == null)
		{
			return;
		}
		string cultureName = ((hero.Culture == null) ? "unknown" : NameOf(hero.Culture));
		string clanName = ((hero.Clan == null) ? string.Empty : NameOf(hero.Clan));
		if (string.Equals(clanName, "Playerland", StringComparison.OrdinalIgnoreCase))
		{
			clanName = "Your Clan";
		}
		for (int i = 0; i < _active._characterMilestones.Count; i++)
		{
			string[] fields = _active._characterMilestones[i].Split(new char[1] { '\t' }, 3);
			if (fields.Length != 3 || !string.Equals(fields[0], hero.StringId, StringComparison.Ordinal))
			{
				continue;
			}
			string repaired = fields[2];
			if (repaired.StartsWith("Culture recorded as ", StringComparison.Ordinal))
			{
				repaired = "Culture recorded as " + cultureName + ".";
			}
			else if (!string.IsNullOrEmpty(clanName) && repaired.StartsWith("Clan ", StringComparison.Ordinal))
			{
				int tierMarker = repaired.IndexOf(" stands at tier ", StringComparison.Ordinal);
				if (tierMarker >= 0)
				{
					repaired = "Clan " + clanName + repaired.Substring(tierMarker);
				}
				else if (repaired.EndsWith(" is independent.", StringComparison.Ordinal))
				{
					repaired = "Clan " + clanName + " is independent.";
				}
				else if (repaired.IndexOf(" serves ", StringComparison.Ordinal) >= 0 && hero.Clan != null && hero.Clan.Kingdom != null)
				{
					repaired = "Clan " + clanName + " serves " + NameOf(hero.Clan.Kingdom) + ".";
				}
			}
			if (!string.Equals(repaired, fields[2], StringComparison.Ordinal))
			{
				_active._characterMilestones[i] = fields[0] + "\t" + fields[1] + "\t" + repaired;
			}
		}
	}

	internal static void RecordCombatResult(Hero hero, bool wasKilled)
	{
		if (_active != null && hero != null && !string.IsNullOrEmpty(hero.StringId))
		{
			_active.UpdateCombatStatistics(hero.StringId, wasKilled);
		}
	}

	internal static void GetCombatStatistics(Hero hero, out int kills, out int knockouts)
	{
		kills = 0;
		knockouts = 0;
		if (_active == null || hero == null || string.IsNullOrEmpty(hero.StringId))
		{
			return;
		}
		foreach (string combatStatistic in _active._combatStatistics)
		{
			string[] array = combatStatistic.Split('\t');
			if (array.Length == 3 && string.Equals(array[0], hero.StringId, StringComparison.Ordinal))
			{
				int.TryParse(array[1], out kills);
				int.TryParse(array[2], out knockouts);
				break;
			}
		}
	}

	private void UpdateCombatStatistics(string heroId, bool wasKilled)
	{
		for (int i = 0; i < _combatStatistics.Count; i++)
		{
			string[] array = _combatStatistics[i].Split('\t');
			if (array.Length == 3 && string.Equals(array[0], heroId, StringComparison.Ordinal))
			{
				int.TryParse(array[1], out var result);
				int.TryParse(array[2], out var result2);
				_combatStatistics[i] = heroId + "\t" + (wasKilled ? (result + 1) : result) + "\t" + (wasKilled ? result2 : (result2 + 1));
				return;
			}
		}
		_combatStatistics.Add(heroId + "\t" + (wasKilled ? 1 : 0) + "\t" + ((!wasKilled) ? 1 : 0));
	}

	internal static string GetWarStatisticsText()
	{
		CalendarWorldLedgerBehavior active = _active;
		int num = 0;
		int num2 = 0;
		if (active != null && active._entries != null)
		{
			foreach (string entry in active._entries)
			{
				string[] array = entry.Split(new char[1] { '\t' }, 3);
				if (array.Length >= 2)
				{
					if (string.Equals(array[1], "War", StringComparison.Ordinal))
					{
						num++;
					}
					if (string.Equals(array[1], "Peace", StringComparison.Ordinal))
					{
						num2++;
					}
				}
			}
		}
		int num3 = 0;
		List<IFaction> list = new List<IFaction>();
		List<Kingdom> list2 = new List<Kingdom>(Kingdom.All);
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < list2.Count; i++)
		{
			Kingdom kingdom = list2[i];
			if (kingdom == null || kingdom.IsEliminated)
			{
				continue;
			}
			for (int j = i + 1; j < list2.Count; j++)
			{
				Kingdom kingdom2 = list2[j];
				if (kingdom2 != null && !kingdom2.IsEliminated && kingdom.IsAtWarWith(kingdom2))
				{
					num3++;
					StanceLink stanceWith = kingdom.GetStanceWith(kingdom2);
					stringBuilder.Append(NameOf(kingdom)).Append("  vs  ").Append(NameOf(kingdom2))
						.AppendLine();
					stringBuilder.Append("  Troops fielded: ").Append(GetKingdomTroopStrength(kingdom).ToString("N0")).Append(" / ")
						.Append(GetKingdomTroopStrength(kingdom2).ToString("N0"))
						.AppendLine();
					stringBuilder.Append("  War losses: ").Append((stanceWith?.GetCasualties(kingdom) ?? 0).ToString("N0")).Append(" / ")
						.Append((stanceWith?.GetCasualties(kingdom2) ?? 0).ToString("N0"))
						.AppendLine()
						.AppendLine();
				}
			}
		}
		IFaction faction = ((Hero.MainHero == null) ? null : Hero.MainHero.MapFaction);
		if (faction != null)
		{
			foreach (Kingdom item in list2)
			{
				if (item != null && !item.IsEliminated && item != faction && faction.IsAtWarWith(item))
				{
					list.Add(item);
				}
			}
		}
		StringBuilder stringBuilder2 = new StringBuilder();
		stringBuilder2.Append("ACTIVE KINGDOM WARS: ").Append(num3).AppendLine();
		stringBuilder2.Append("WAR DECLARATIONS RECORDED: ").Append(num).AppendLine();
		stringBuilder2.Append("PEACE AGREEMENTS RECORDED: ").Append(num2).AppendLine()
			.AppendLine();
		stringBuilder2.Append("YOUR CURRENT WARS").AppendLine();
		if (list.Count == 0)
		{
			stringBuilder2.Append("Your faction is currently at peace.");
		}
		else
		{
			foreach (IFaction item2 in list)
			{
				stringBuilder2.Append("• ").Append(NameOf(item2)).AppendLine();
			}
		}
		stringBuilder2.AppendLine().AppendLine().Append("ACTIVE WARFRONTS")
			.AppendLine()
			.AppendLine();
		if (num3 == 0)
		{
			stringBuilder2.Append("No kingdom wars are active.");
		}
		else
		{
			stringBuilder2.Append(stringBuilder);
		}
		stringBuilder2.AppendLine().AppendLine().Append("Troops fielded includes all current kingdom forces: field parties, caravans, garrisons, and militia. War losses are Bannerlord's native casualty totals for the current war; diplomatic totals are recorded by Ages of Calradia in this campaign.");
		return stringBuilder2.ToString();
	}

	internal static CalendarWarStatisticsSnapshot GetWarStatisticsSnapshot()
	{
		CalendarWarStatisticsSnapshot calendarWarStatisticsSnapshot = new CalendarWarStatisticsSnapshot();
		CalendarWorldLedgerBehavior active = _active;
		if (active != null && active._entries != null)
		{
			foreach (string entry in active._entries)
			{
				string[] array = entry.Split(new char[1] { '\t' }, 3);
				if (array.Length >= 2)
				{
					if (string.Equals(array[1], "War", StringComparison.Ordinal))
					{
						calendarWarStatisticsSnapshot.WarDeclarations++;
					}
					if (string.Equals(array[1], "Peace", StringComparison.Ordinal))
					{
						calendarWarStatisticsSnapshot.PeaceAgreements++;
					}
				}
			}
		}
		List<Kingdom> list = new List<Kingdom>(Kingdom.All);
		HashSet<Kingdom> hashSet = new HashSet<Kingdom>();
		IFaction faction = ((Hero.MainHero == null) ? null : Hero.MainHero.MapFaction);
		List<string> list2 = new List<string>();
		for (int i = 0; i < list.Count; i++)
		{
			Kingdom kingdom = list[i];
			if (kingdom == null || kingdom.IsEliminated)
			{
				continue;
			}
			for (int j = i + 1; j < list.Count; j++)
			{
				Kingdom kingdom2 = list[j];
				if (kingdom2 != null && !kingdom2.IsEliminated && kingdom.IsAtWarWith(kingdom2))
				{
					StanceLink stanceWith = kingdom.GetStanceWith(kingdom2);
					int kingdomTroopStrength = GetKingdomTroopStrength(kingdom);
					int kingdomTroopStrength2 = GetKingdomTroopStrength(kingdom2);
					int kingdomShipStrength = GetKingdomShipStrength(kingdom);
					int kingdomShipStrength2 = GetKingdomShipStrength(kingdom2);
					int num = stanceWith?.GetCasualties(kingdom) ?? 0;
					int num2 = stanceWith?.GetCasualties(kingdom2) ?? 0;
					int navalWarLosses = GetNavalWarLosses(kingdom, kingdom2);
					int navalWarLosses2 = GetNavalWarLosses(kingdom2, kingdom);
					bool isPlayerWar = kingdom == faction || kingdom2 == faction;
					calendarWarStatisticsSnapshot.Wars.Add(new CalendarWarStatisticsRecord(kingdom, kingdom2, NameOf(kingdom), NameOf(kingdom2), kingdomTroopStrength, kingdomTroopStrength2, kingdomShipStrength, kingdomShipStrength2, num, num2, navalWarLosses, navalWarLosses2, GetWarScore(kingdom, kingdom2), isPlayerWar));
					hashSet.Add(kingdom);
					hashSet.Add(kingdom2);
					calendarWarStatisticsSnapshot.TotalLosses += num + num2;
				}
			}
		}
		foreach (Kingdom item in hashSet)
		{
			calendarWarStatisticsSnapshot.TotalTroopsFielded += GetKingdomTroopStrength(item);
		}
		if (faction != null)
		{
			foreach (Kingdom item2 in list)
			{
				if (item2 != null && !item2.IsEliminated && item2 != faction && faction.IsAtWarWith(item2))
				{
					list2.Add(NameOf(item2));
				}
			}
		}
		list2.Sort(StringComparer.Ordinal);
		bool flag = Clan.PlayerClan != null && Clan.PlayerClan.Kingdom != null;
		calendarWarStatisticsSnapshot.PlayerStatus = ((list2.Count != 0) ? ("At war with " + string.Join(", ", list2.ToArray()) + ".") : (flag ? "Your kingdom is currently at peace." : ("Independent clan  •  " + calendarWarStatisticsSnapshot.Wars.Count.ToString("N0") + " kingdom warfronts are active across Calradia.")));
		calendarWarStatisticsSnapshot.Wars.Sort(delegate(CalendarWarStatisticsRecord left, CalendarWarStatisticsRecord right)
		{
			int num3 = right.IsPlayerWar.CompareTo(left.IsPlayerWar);
			if (num3 != 0)
			{
				return num3;
			}
			int num4 = right.TotalLosses.CompareTo(left.TotalLosses);
			if (num4 != 0)
			{
				return num4;
			}
			int num5 = string.Compare(left.LeftName, right.LeftName, StringComparison.Ordinal);
			return (num5 != 0) ? num5 : string.Compare(left.RightName, right.RightName, StringComparison.Ordinal);
		});
		return calendarWarStatisticsSnapshot;
	}

	internal static string GetDiplomacyText()
	{
		Kingdom kingdom = ((Clan.PlayerClan == null) ? null : Clan.PlayerClan.Kingdom);
		if (kingdom == null)
		{
			return "You do not currently belong to a kingdom. Join or found a kingdom to view its foreign relations.\n\nAges of Calradia diplomacy will use Bannerlord's native kingdom decisions for war and peace, while its own save-safe campaign behavior stores custom treaties and cooldowns.";
		}
		List<Kingdom> list = new List<Kingdom>(Kingdom.All);
		list.Sort(delegate(Kingdom left, Kingdom right)
		{
			string strA = ((left == null || left.Name == null) ? string.Empty : left.Name.ToString());
			string strB = ((right == null || right.Name == null) ? string.Empty : right.Name.ToString());
			return string.Compare(strA, strB, StringComparison.Ordinal);
		});
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("YOUR REALM: ").Append(NameOf(kingdom)).AppendLine()
			.AppendLine();
		foreach (Kingdom item in list)
		{
			if (item == null || item.IsEliminated || item == kingdom)
			{
				continue;
			}
			StanceLink stanceWith = kingdom.GetStanceWith(item);
			bool flag = kingdom.IsAtWarWith(item);
			stringBuilder.Append(NameOf(item)).Append(" — ").Append(flag ? "AT WAR" : "AT PEACE");
			if (stanceWith != null)
			{
				int dailyTributeToPay = stanceWith.GetDailyTributeToPay(kingdom);
				if (dailyTributeToPay > 0)
				{
					stringBuilder.Append("  |  Tribute owed: ").Append(dailyTributeToPay.ToString("N0")).Append("/day");
				}
				else if (dailyTributeToPay < 0)
				{
					stringBuilder.Append("  |  Tribute received: ").Append((-dailyTributeToPay).ToString("N0")).Append("/day");
				}
				if (flag)
				{
					stringBuilder.Append("  |  War losses: ").Append(stanceWith.GetCasualties(item).ToString("N0")).Append(" inflicted / ")
						.Append(stanceWith.GetCasualties(kingdom).ToString("N0"))
						.Append(" suffered");
				}
			}
			stringBuilder.AppendLine().AppendLine();
		}
		stringBuilder.Append("This is a live report from Bannerlord's current diplomatic stances. Proposed treaties and policy actions will be added through Ages of Calradia's own campaign behavior, without requiring the separate Diplomacy mod.");
		return stringBuilder.ToString();
	}

	private static int GetKingdomTroopStrength(Kingdom kingdom)
	{
		if (kingdom == null)
		{
			return 0;
		}
		return Math.Max(0, (int)Math.Round(kingdom.CurrentTotalStrength));
	}

	private static int GetKingdomShipStrength(Kingdom kingdom)
	{
		return 0;
	}

	private void OnCharacterCreationIsOver()
	{
		string text = CharacterOriginStoryCapture.ConsumeSnapshot();
		if (string.IsNullOrWhiteSpace(text))
		{
			Diagnostics.Info("Character origin recorder found no narrative selections to save.");
			return;
		}
		_characterOriginStory = text;
		Diagnostics.Info("Character origin recorder saved the player's final creation choices.");
	}

	internal void Record(string category, string message)
	{
		if (!string.IsNullOrWhiteSpace(category) && !string.IsNullOrWhiteSpace(message))
		{
			string item = CampaignTime.Now.ToDays.ToString("R", CultureInfo.InvariantCulture) + "\t" + category.Trim() + "\t" + message.Trim();
			_entries.Add(item);
		}
	}

	internal static long GetFirstRecordedDay(long fallbackDay)
	{
		CalendarWorldLedgerBehavior active = _active;
		if (active == null || active._entries == null || active._entries.Count == 0)
		{
			return fallbackDay;
		}
		long num = long.MaxValue;
		foreach (string entry in active._entries)
		{
			string[] array = entry.Split(new char[1] { '\t' }, 3);
			if (array.Length == 3 && double.TryParse(array[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				long num2 = ToCalendarDay(result);
				if (num2 < num)
				{
					num = num2;
				}
			}
		}
		if (num != long.MaxValue)
		{
			return num;
		}
		return fallbackDay;
	}

	internal static int CountRecordedEntries(long firstDayInclusive, long firstDayExclusive)
	{
		CalendarWorldLedgerBehavior active = _active;
		if (active == null || active._entries == null || firstDayExclusive <= firstDayInclusive)
		{
			return 0;
		}
		int num = 0;
		foreach (string entry in active._entries)
		{
			string[] array = entry.Split(new char[1] { '\t' }, 3);
			if (array.Length == 3 && double.TryParse(array[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				long num2 = ToCalendarDay(result);
				if (num2 >= firstDayInclusive && num2 < firstDayExclusive)
				{
					num++;
				}
			}
		}
		return num;
	}

	internal static string GetRecordedEntriesText(long firstDayInclusive, long firstDayExclusive, int visibleLimit)
	{
		CalendarWorldLedgerBehavior active = _active;
		if (active == null || active._entries == null || firstDayExclusive <= firstDayInclusive)
		{
			return "No events were recorded for this month.";
		}
		List<string> list = new List<string>();
		foreach (string entry in active._entries)
		{
			string[] array = entry.Split(new char[1] { '\t' }, 3);
			if (array.Length == 3 && double.TryParse(array[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				long num = ToCalendarDay(result);
				if (num >= firstDayInclusive && num < firstDayExclusive)
				{
					list.Add("[" + array[1] + "] " + array[2]);
				}
			}
		}
		if (list.Count == 0)
		{
			return "No events were recorded for this month.";
		}
		StringBuilder stringBuilder = new StringBuilder();
		int num2 = Math.Min(Math.Max(1, visibleLimit), list.Count);
		for (int i = 0; i < num2; i++)
		{
			if (i > 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.Append("• ").Append(list[i]);
		}
		if (list.Count > num2)
		{
			stringBuilder.AppendLine().Append("+ ").Append(list.Count - num2)
				.Append(" more saved events.");
		}
		return stringBuilder.ToString();
	}

	internal static string GetRecordedSummaryText(long firstDayInclusive, long firstDayExclusive)
	{
		CalendarWorldLedgerBehavior active = _active;
		if (active == null || active._entries == null || firstDayExclusive <= firstDayInclusive)
		{
			return "No events were recorded.";
		}
		Dictionary<string, int> dictionary = new Dictionary<string, int>(StringComparer.Ordinal);
		List<string> list = new List<string>();
		int num = 0;
		foreach (string entry in active._entries)
		{
			string[] array = entry.Split(new char[1] { '\t' }, 3);
			if (array.Length != 3 || !double.TryParse(array[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				continue;
			}
			long num2 = ToCalendarDay(result);
			if (num2 >= firstDayInclusive && num2 < firstDayExclusive)
			{
				num++;
				dictionary.TryGetValue(array[1], out var value);
				dictionary[array[1]] = value + 1;
				if (list.Count < 2)
				{
					list.Add("• " + array[2]);
				}
			}
		}
		if (num == 0)
		{
			return "No events were recorded.";
		}
		StringBuilder stringBuilder = new StringBuilder("Recorded: ").Append(num);
		foreach (KeyValuePair<string, int> item in dictionary)
		{
			stringBuilder.Append(" | ").Append(item.Key).Append(": ")
				.Append(item.Value);
		}
		foreach (string item2 in list)
		{
			stringBuilder.AppendLine().Append(item2);
		}
		return stringBuilder.ToString();
	}

	internal static string GetImportantEventsText(long firstDayInclusive, long firstDayExclusive, int limit, bool includeCounts)
	{
		CalendarWorldLedgerBehavior active = _active;
		if (active == null || active._entries == null || firstDayExclusive <= firstDayInclusive)
		{
			return "No events were recorded.";
		}
		List<ImportantLedgerEntry> list = new List<ImportantLedgerEntry>();
		Dictionary<string, int> dictionary = new Dictionary<string, int>(StringComparer.Ordinal);
		int num = 0;
		foreach (string entry in active._entries)
		{
			string[] array = entry.Split(new char[1] { '\t' }, 3);
			if (array.Length == 3 && double.TryParse(array[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				long num2 = ToCalendarDay(result);
				if (num2 >= firstDayInclusive && num2 < firstDayExclusive)
				{
					dictionary.TryGetValue(array[1], out var value);
					dictionary[array[1]] = value + 1;
					list.Add(new ImportantLedgerEntry(num2, array[1], array[2], GetImportanceScore(array[1]), num++));
				}
			}
		}
		if (list.Count == 0)
		{
			return "No events were recorded.";
		}
		list.Sort(delegate(ImportantLedgerEntry left, ImportantLedgerEntry right)
		{
			int num4 = right.Importance.CompareTo(left.Importance);
			if (num4 != 0)
			{
				return num4;
			}
			int num5 = right.Day.CompareTo(left.Day);
			return (num5 != 0) ? num5 : right.Sequence.CompareTo(left.Sequence);
		});
		StringBuilder stringBuilder = new StringBuilder();
		if (includeCounts)
		{
			stringBuilder.Append("Recorded: ").Append(list.Count);
			foreach (KeyValuePair<string, int> item in dictionary)
			{
				stringBuilder.Append(" | ").Append(item.Key).Append(": ")
					.Append(item.Value);
			}
		}
		int num3 = Math.Min(Math.Max(1, limit), list.Count);
		for (int i = 0; i < num3; i++)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.AppendLine();
			}
			ImportantLedgerEntry importantLedgerEntry = list[i];
			stringBuilder.Append("• [").Append(importantLedgerEntry.Category).Append("] ")
				.Append(importantLedgerEntry.Message);
		}
		return stringBuilder.ToString();
	}

	private static int GetImportanceScore(string category)
	{
		if (string.Equals(category, "War", StringComparison.Ordinal))
		{
			return 100;
		}
		if (string.Equals(category, "Settlement", StringComparison.Ordinal))
		{
			return 95;
		}
		if (string.Equals(category, "Peace", StringComparison.Ordinal))
		{
			return 90;
		}
		if (string.Equals(category, "Death", StringComparison.Ordinal))
		{
			return 80;
		}
		if (string.Equals(category, "Birth", StringComparison.Ordinal))
		{
			return 50;
		}
		return 25;
	}

	internal static string GetRecentEntriesText(string filter)
	{
		CalendarWorldLedgerBehavior active = _active;
		if (active == null || active._entries == null || active._entries.Count == 0)
		{
			return "No world events have been recorded yet.";
		}
		StringBuilder stringBuilder = new StringBuilder();
		int num = Math.Max(0, active._entries.Count - 40);
		bool flag = string.Equals(filter, "ByDay", StringComparison.Ordinal);
		int num2 = int.MinValue;
		for (int num3 = active._entries.Count - 1; num3 >= num; num3--)
		{
			string[] array = active._entries[num3].Split(new char[1] { '\t' }, 3);
			if (array.Length == 3)
			{
				if (!flag && !MatchesFilter(array[1], filter))
				{
					continue;
				}
				if (flag)
				{
					if (!double.TryParse(array[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
					{
						continue;
					}
					int num4 = (int)ToCalendarDay(result);
					if (num4 != num2)
					{
						if (stringBuilder.Length > 0)
						{
							stringBuilder.AppendLine();
						}
						stringBuilder.Append(CalendarFormatter.Format(CalendarTimeMath.FromCalendarAbsoluteDays(num4))).AppendLine();
						num2 = num4;
					}
				}
				stringBuilder.Append('[').Append(array[1]).Append("] ")
					.Append(array[2]);
			}
			else
			{
				stringBuilder.Append(active._entries[num3]);
			}
			if (num3 > num)
			{
				stringBuilder.AppendLine().AppendLine();
			}
		}
		if (stringBuilder.Length != 0)
		{
			return stringBuilder.ToString();
		}
		return "No matching world events have been recorded yet.";
	}

	internal static string GetTrackedSettlementOwnersText()
	{
		CalendarWorldLedgerBehavior active = _active;
		if (active == null)
		{
			return "Settlement ownership tracker is not active yet.";
		}
		active.CaptureSettlementOwnership(recordChanges: false);
		List<string> list = new List<string>(active._settlementOwners);
		list.Sort(StringComparer.OrdinalIgnoreCase);
		StringBuilder stringBuilder = new StringBuilder("TRACKED SETTLEMENT OWNERS (" + list.Count + ")\n\n");
		int num = 0;
		foreach (string item in list)
		{
			if (num >= 24)
			{
				break;
			}
			string[] array = item.Split(new char[1] { '\t' }, 4);
			if (array.Length >= 4)
			{
				num++;
				stringBuilder.Append(array[1]).Append(" — ").Append(array[3])
					.AppendLine();
			}
		}
		int num2 = list.Count - num;
		if (num2 > 0)
		{
			stringBuilder.AppendLine().Append("+ ").Append(num2)
				.Append(" more settlements tracked.");
		}
		return stringBuilder.ToString();
	}

	internal static string GetTrackedOwnerFactionId(string settlementId)
	{
		CalendarWorldLedgerBehavior active = _active;
		if (active == null || string.IsNullOrEmpty(settlementId))
		{
			return string.Empty;
		}
		active.CaptureSettlementOwnership(recordChanges: false);
		foreach (string settlementOwner in active._settlementOwners)
		{
			string[] array = settlementOwner.Split(new char[1] { '\t' }, 4);
			if (array.Length >= 3 && string.Equals(array[0], settlementId, StringComparison.Ordinal))
			{
				return array[2] ?? string.Empty;
			}
		}
		return string.Empty;
	}

	private static bool MatchesFilter(string category, string filter)
	{
		if (string.IsNullOrEmpty(filter) || string.Equals(filter, "All", StringComparison.Ordinal))
		{
			return true;
		}
		if (string.Equals(filter, "ByDay", StringComparison.Ordinal))
		{
			return true;
		}
		if (string.Equals(filter, "Diplomacy", StringComparison.Ordinal))
		{
			if (!string.Equals(category, "War", StringComparison.Ordinal))
			{
				return string.Equals(category, "Peace", StringComparison.Ordinal);
			}
			return true;
		}
		if (string.Equals(filter, "Settlements", StringComparison.Ordinal))
		{
			return string.Equals(category, "Settlement", StringComparison.Ordinal);
		}
		if (string.Equals(filter, "People", StringComparison.Ordinal))
		{
			if (!string.Equals(category, "Birth", StringComparison.Ordinal))
			{
				return string.Equals(category, "Death", StringComparison.Ordinal);
			}
			return true;
		}
		return false;
	}

	internal static string GetDaySummary(long absoluteDay, string filter)
	{
		CalendarWorldLedgerBehavior active = _active;
		if (active == null || active._entries == null)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		int num = active._entries.Count - 1;
		while (num >= 0 && stringBuilder.Length < 34)
		{
			string[] array = active._entries[num].Split(new char[1] { '\t' }, 3);
			if (array.Length == 3 && double.TryParse(array[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var result) && ToCalendarDay(result) == absoluteDay && MatchesFilter(array[1], filter))
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append('•').Append(' ').Append(array[1]);
			}
			num--;
		}
		return stringBuilder.ToString();
	}

	private static long ToCalendarDay(double rawDay)
	{
		return (long)Math.Floor(CalendarTimeMath.ToCalendarAbsoluteDays(rawDay));
	}

	private void OnDailyTick()
	{
		CaptureSettlementOwnership(recordChanges: true);
		KeepOccupiedFiefsUnassignedUntilPeace();
		ReconcileActiveWars();
		NotifyArrivedMarriageMessengers();
		CaptureCharacterMilestones(recordCurrentFacts: false);
		_ownershipRevision++;
	}

	private void NotifyArrivedMarriageMessengers()
	{
		for (int i = 0; i < _marriageMessengerTravels.Count; i++)
		{
			string[] array = _marriageMessengerTravels[i].Split('\t');
			if (array.Length == 4 && !string.Equals(array[3], "1", StringComparison.Ordinal) && double.TryParse(array[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var result) && !(CampaignTime.Now.ToDays < result))
			{
				Hero hero = ((Campaign.Current == null || Campaign.Current.CampaignObjectManager == null) ? null : Campaign.Current.CampaignObjectManager.Find<Hero>(array[1]));
				string text = ((hero == null || hero.Name == null) ? "the requested court" : hero.Name.ToString());
				InformationManager.DisplayMessage(new InformationMessage((string.Equals(array[0], "ClanLeader", StringComparison.Ordinal) ? "A clan-leader audience is ready with " : "A marriage introduction is ready with ") + text + ". Open World Events > Summaries > Marriages."));
				_marriageMessengerTravels[i] = array[0] + "\t" + array[1] + "\t" + array[2] + "\t1";
			}
		}
	}

	private void OnSessionLaunched(CampaignGameStarter starter)
	{
		ReconcileActiveWars();
		CaptureCharacterMilestones(recordCurrentFacts: true);
	}

	private void CaptureCharacterMilestones(bool recordCurrentFacts)
	{
		Hero mainHero = Hero.MainHero;
		if (mainHero == null || string.IsNullOrEmpty(mainHero.StringId))
		{
			return;
		}
		string text = ((mainHero.Spouse == null || string.IsNullOrEmpty(mainHero.Spouse.StringId)) ? string.Empty : mainHero.Spouse.StringId);
		string text2 = ((mainHero.Clan == null || string.IsNullOrEmpty(mainHero.Clan.StringId)) ? string.Empty : mainHero.Clan.StringId);
		int num = ((mainHero.Clan != null) ? mainHero.Clan.Tier : 0);
		string[] array = (_characterMilestoneState ?? string.Empty).Split('\t');
		bool flag = array.Length == 4 && string.Equals(array[0], mainHero.StringId, StringComparison.Ordinal);
		string b = (flag ? array[1] : string.Empty);
		int result = 0;
		if (flag)
		{
			int.TryParse(array[2], out result);
		}
		string text3 = (flag ? array[3] : string.Empty);
		if (!flag && recordCurrentFacts)
		{
			AddCharacterMilestone(mainHero, "Chronicle opened at age " + mainHero.Age.ToString("0") + ".");
			AddCharacterMilestone(mainHero, "Culture recorded as " + ((mainHero.Culture == null) ? "unknown" : NameOf(mainHero.Culture)) + ".");
			if (mainHero.Spouse != null)
			{
				AddCharacterMilestone(mainHero, "Recorded as married to " + NameOf(mainHero.Spouse) + ".");
			}
			else
			{
				AddCharacterMilestone(mainHero, "Recorded as unmarried.");
			}
			if (mainHero.Clan != null)
			{
				string clanName = NameOf(mainHero.Clan);
				if (string.Equals(clanName, "Playerland", StringComparison.OrdinalIgnoreCase))
				{
					clanName = "Your Clan";
				}
				AddCharacterMilestone(mainHero, "Clan " + clanName + " stands at tier " + num + ".");
				AddCharacterMilestone(mainHero, (mainHero.Clan.Kingdom == null) ? ("Clan " + clanName + " is independent.") : ("Clan " + clanName + " serves " + NameOf(mainHero.Clan.Kingdom) + "."));
			}
		}
		else if (flag)
		{
			if (!string.IsNullOrEmpty(text) && !string.Equals(text, b, StringComparison.Ordinal))
			{
				AddCharacterMilestone(mainHero, "Married " + NameOf(mainHero.Spouse) + ".");
			}
			if (!string.IsNullOrEmpty(text2) && !string.IsNullOrEmpty(text3) && !string.Equals(text2, text3, StringComparison.Ordinal))
			{
				AddCharacterMilestone(mainHero, "Joined Clan " + NameOf(mainHero.Clan) + ".");
			}
			if (mainHero.Clan != null && num > result)
			{
				AddCharacterMilestone(mainHero, "Clan " + NameOf(mainHero.Clan) + " rose to tier " + num + ".");
			}
		}
		_characterMilestoneState = mainHero.StringId + "\t" + text + "\t" + num.ToString(CultureInfo.InvariantCulture) + "\t" + text2;
	}

	private void AddCharacterMilestone(Hero hero, string message)
	{
		if (hero == null || string.IsNullOrEmpty(hero.StringId) || string.IsNullOrWhiteSpace(message))
		{
			return;
		}
		string text = message.Trim();
		foreach (string characterMilestone in _characterMilestones)
		{
			string[] array = characterMilestone.Split(new char[1] { '\t' }, 3);
			if (array.Length == 3 && string.Equals(array[0], hero.StringId, StringComparison.Ordinal) && string.Equals(array[2], text, StringComparison.Ordinal))
			{
				return;
			}
		}
		string item = hero.StringId + "\t" + CampaignTime.Now.ToDays.ToString("R", CultureInfo.InvariantCulture) + "\t" + text;
		_characterMilestones.Add(item);
	}

	private void ReconcileActiveWars()
	{
		List<Kingdom> list = new List<Kingdom>(Kingdom.All);
		for (int i = 0; i < list.Count; i++)
		{
			Kingdom kingdom = list[i];
			if (kingdom == null || kingdom.IsEliminated)
			{
				continue;
			}
			for (int j = i + 1; j < list.Count; j++)
			{
				Kingdom kingdom2 = list[j];
				if (kingdom2 != null && !kingdom2.IsEliminated && kingdom.IsAtWarWith(kingdom2) && !HasWarScoreRecord(kingdom, kingdom2))
				{
					SetWarScore(kingdom, kingdom2, 0);
				}
			}
		}
	}

	private void OnWarDeclared(IFaction first, IFaction second, DeclareWarAction.DeclareWarDetail detail)
	{
		Record("War", NameOf(first) + " declared war on " + NameOf(second) + ".");
		Kingdom kingdom = AsKingdom(first);
		Kingdom kingdom2 = AsKingdom(second);
		if (kingdom != null && kingdom2 != null)
		{
			RemoveNavalWarLosses(kingdom, kingdom2);
			SetWarScore(kingdom, kingdom2, 0);
		}
	}

	private void OnPeaceMade(IFaction first, IFaction second, MakePeaceAction.MakePeaceDetail detail)
	{
		Kingdom kingdom = AsKingdom(first);
		Kingdom kingdom2 = AsKingdom(second);
		if (kingdom != null && kingdom2 != null)
		{
			ResolveOccupiedFiefs(kingdom, kingdom2, Math.Abs(GetWarScore(kingdom, kingdom2)) >= 100);
			RemoveWarScore(kingdom, kingdom2);
			RemoveNavalWarLosses(kingdom, kingdom2);
		}
		Record("Peace", NameOf(first) + " made peace with " + NameOf(second) + ".");
	}

	private void OnSettlementOwnerChanged(Settlement settlement, bool openToClaim, Hero newOwner, Hero oldOwner, Hero capturerHero, ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail)
	{
		if (detail == ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail.BySiege && settlement != null && settlement.IsFortification && newOwner != null && oldOwner != null)
		{
			Kingdom kingdom = AsKingdom(newOwner.MapFaction);
			Kingdom kingdom2 = AsKingdom(oldOwner.MapFaction);
			if (kingdom != null && kingdom2 != null && kingdom.IsAtWarWith(kingdom2))
			{
				RegisterWarOccupation(kingdom, kingdom2, settlement, oldOwner.Clan);
				AddWarScore(kingdom, kingdom2, settlement.IsTown ? 25 : 15, (settlement.IsTown ? "Town captured: " : "Castle captured: ") + NameOf(settlement));
				if (settlement.Town != null)
				{
					settlement.Town.IsOwnerUnassigned = false;
				}
			}
		}
		UpdateSettlementOwner(settlement, recordChanges: false);
		Record("Settlement", NameOf(settlement) + " changed owner to " + NameOf(newOwner) + ".");
	}

	private void OnMapEventEndedForWarScore(MapEvent mapEvent)
	{
		if (mapEvent != null && mapEvent.HasWinner && (mapEvent.IsFieldBattle || mapEvent.IsSallyOut || mapEvent.IsSiegeOutside || mapEvent.IsBlockade || mapEvent.IsBlockadeSallyOut))
		{
			MapEventSide winner = mapEvent.Winner;
			MapEventSide mapEventSide = ((winner == mapEvent.AttackerSide) ? mapEvent.DefenderSide : mapEvent.AttackerSide);
			Kingdom kingdom = AsKingdom(winner?.MapFaction);
			Kingdom kingdom2 = AsKingdom(mapEventSide?.MapFaction);
			if (kingdom != null && kingdom2 != null && kingdom != kingdom2 && kingdom.IsAtWarWith(kingdom2))
			{
				int num = ((mapEventSide != null) ? Math.Max(0, mapEventSide.HealthyTroopCountAtMapEventStart) : 0);
				int points = Math.Max(3, Math.Min(10, 3 + num / 200));
				AddWarScore(kingdom, kingdom2, points, "Battle won against " + NameOf(kingdom2) + " (" + points.ToString("N0") + " war score)");
			}
		}
	}

	private void OnShipDestroyed(PartyBase owner, Ship ship, DestroyShipAction.ShipDestroyDetail detail)
	{
		if (detail == DestroyShipAction.ShipDestroyDetail.ApplyDefault && owner != null && owner.IsMobile && owner.MapEvent != null && owner.MapEventSide != null)
		{
			MapEventSide obj = ((owner.MapEventSide == owner.MapEvent.AttackerSide) ? owner.MapEvent.DefenderSide : owner.MapEvent.AttackerSide);
			Kingdom kingdom = AsKingdom(owner.MapFaction);
			Kingdom kingdom2 = AsKingdom(obj?.MapFaction);
			if (kingdom != null && kingdom2 != null && kingdom != kingdom2 && kingdom.IsAtWarWith(kingdom2))
			{
				AddNavalWarLoss(kingdom, kingdom2);
			}
		}
	}

	internal static int GetWarScore(Kingdom first, Kingdom second)
	{
		if (_active != null)
		{
			return _active.ReadWarScore(first, second);
		}
		return 0;
	}

	internal static int GetNavalWarLosses(Kingdom kingdom, Kingdom opponent)
	{
		if (_active != null)
		{
			return _active.ReadNavalWarLosses(kingdom, opponent);
		}
		return 0;
	}

	internal static bool TryConcludeWar(Kingdom first, Kingdom second, bool surrender, out string message)
	{
		message = string.Empty;
		if (_active == null || first == null || second == null || !first.IsAtWarWith(second))
		{
			message = "That war is no longer active.";
			return false;
		}
		Kingdom kingdom = ((Clan.PlayerClan == null) ? null : Clan.PlayerClan.Kingdom);
		if (kingdom == null || (kingdom != first && kingdom != second))
		{
			message = "Only a war involving your kingdom can be concluded here.";
			return false;
		}
		int num3 = Math.Abs(_active.ReadWarScore(first, second));
		int num2 = (surrender ? 100 : 50);
		if (num3 < num2)
		{
			message = num2.ToString("N0") + "% war score is required.";
			return false;
		}
		message = (surrender ? "Surrender enforced. Current conquests will remain with the victorious kingdom." : "White peace concluded. Every occupied fief will return to its pre-war owner.");
		MakePeaceAction.Apply(first, second);
		return true;
	}

	private int ReadWarScore(Kingdom first, Kingdom second)
	{
		string warKey = GetWarKey(first, second);
		if (string.IsNullOrEmpty(warKey))
		{
			return 0;
		}
		foreach (string warScore in _warScores)
		{
			string[] array = warScore.Split(new char[1] { '\t' }, 2);
			if (array.Length == 2 && string.Equals(array[0], warKey, StringComparison.Ordinal) && int.TryParse(array[1], out var result))
			{
				return IsFirstInWarKey(first, second) ? result : (-result);
			}
		}
		return 0;
	}

	private void SetWarScore(Kingdom first, Kingdom second, int scoreFromFirstPerspective)
	{
		string warKey = GetWarKey(first, second);
		if (string.IsNullOrEmpty(warKey))
		{
			return;
		}
		int val = (IsFirstInWarKey(first, second) ? scoreFromFirstPerspective : (-scoreFromFirstPerspective));
		string text = warKey + "\t" + Math.Max(-100, Math.Min(100, val));
		for (int i = 0; i < _warScores.Count; i++)
		{
			if (_warScores[i].StartsWith(warKey + "\t", StringComparison.Ordinal))
			{
				_warScores[i] = text;
				return;
			}
		}
		_warScores.Add(text);
	}

	private bool HasWarScoreRecord(Kingdom first, Kingdom second)
	{
		string warKey = GetWarKey(first, second);
		if (string.IsNullOrEmpty(warKey))
		{
			return false;
		}
		foreach (string warScore in _warScores)
		{
			if (warScore.StartsWith(warKey + "\t", StringComparison.Ordinal))
			{
				return true;
			}
		}
		return false;
	}

	private void AddWarScore(Kingdom winner, Kingdom loser, int points, string reason)
	{
		int scoreFromFirstPerspective = Math.Max(-100, Math.Min(100, ReadWarScore(winner, loser) + Math.Max(0, points)));
		SetWarScore(winner, loser, scoreFromFirstPerspective);
		Record("War Score", NameOf(winner) + " gained " + points.ToString("N0") + " war score. " + reason + ". Current score: " + scoreFromFirstPerspective.ToString("+0;-0;0") + ".");
	}

	private void RemoveWarScore(Kingdom first, Kingdom second)
	{
		string key = GetWarKey(first, second);
		_warScores.RemoveAll((string record) => record.StartsWith(key + "\t", StringComparison.Ordinal));
	}

	private int ReadNavalWarLosses(Kingdom kingdom, Kingdom opponent)
	{
		string warKey = GetWarKey(kingdom, opponent);
		string text = ((kingdom == null) ? string.Empty : (kingdom.StringId ?? string.Empty));
		if (string.IsNullOrEmpty(warKey) || string.IsNullOrEmpty(text))
		{
			return 0;
		}
		string value = warKey + "\t" + text + "\t";
		foreach (string navalWarLoss in _navalWarLosses)
		{
			if (navalWarLoss.StartsWith(value, StringComparison.Ordinal))
			{
				string[] array = navalWarLoss.Split(new char[1] { '\t' }, 3);
				int result;
				return (array.Length == 3 && int.TryParse(array[2], out result)) ? Math.Max(0, result) : 0;
			}
		}
		return 0;
	}

	private void AddNavalWarLoss(Kingdom kingdom, Kingdom opponent)
	{
		string warKey = GetWarKey(kingdom, opponent);
		string text = ((kingdom == null) ? string.Empty : (kingdom.StringId ?? string.Empty));
		if (string.IsNullOrEmpty(warKey) || string.IsNullOrEmpty(text))
		{
			return;
		}
		string text2 = warKey + "\t" + text + "\t";
		string text3 = text2 + (ReadNavalWarLosses(kingdom, opponent) + 1).ToString(CultureInfo.InvariantCulture);
		for (int i = 0; i < _navalWarLosses.Count; i++)
		{
			if (_navalWarLosses[i].StartsWith(text2, StringComparison.Ordinal))
			{
				_navalWarLosses[i] = text3;
				return;
			}
		}
		_navalWarLosses.Add(text3);
	}

	private void RemoveNavalWarLosses(Kingdom first, Kingdom second)
	{
		string key = GetWarKey(first, second);
		if (!string.IsNullOrEmpty(key))
		{
			_navalWarLosses.RemoveAll((string record) => record.StartsWith(key + "\t", StringComparison.Ordinal));
		}
	}

	private void RegisterWarOccupation(Kingdom victor, Kingdom defeated, Settlement settlement, Clan originalClan)
	{
		string warKey = GetWarKey(victor, defeated);
		if (string.IsNullOrEmpty(warKey) || settlement == null)
		{
			return;
		}
		string text = warKey + "\t" + (settlement.StringId ?? string.Empty) + "\t";
		foreach (string warOccupation in _warOccupations)
		{
			if (warOccupation.StartsWith(text, StringComparison.Ordinal))
			{
				return;
			}
		}
		_warOccupations.Add(text + ((originalClan == null) ? string.Empty : (originalClan.StringId ?? string.Empty)) + "\t" + (defeated.StringId ?? string.Empty));
	}

	private void KeepOccupiedFiefsUnassignedUntilPeace()
	{
		foreach (string warOccupation in _warOccupations)
		{
			string[] array = warOccupation.Split(new char[1] { '\t' }, 4);
			if (array.Length == 4 && TryGetWarKingdoms(array[0], out var first, out var second) && first != null && second != null && first.IsAtWarWith(second))
			{
				Settlement settlement = FindSettlement(array[1]);
				if (settlement != null && settlement.Town != null)
				{
					settlement.Town.IsOwnerUnassigned = false;
				}
			}
		}
	}

	private void ResolveOccupiedFiefs(Kingdom first, Kingdom second, bool surrender)
	{
		string warKey = GetWarKey(first, second);
		List<string> list = new List<string>();
		foreach (string warOccupation in _warOccupations)
		{
			string[] array = warOccupation.Split(new char[1] { '\t' }, 4);
			if (array.Length != 4 || !string.Equals(array[0], warKey, StringComparison.Ordinal))
			{
				continue;
			}
			list.Add(warOccupation);
			Settlement settlement = FindSettlement(array[1]);
			if (settlement == null || settlement.Town == null)
			{
				continue;
			}
			Clan clan = FindClan(array[2]);
			Kingdom kingdom = FindKingdom(array[3]);
			Kingdom kingdom2 = ((settlement.OwnerClan == null) ? null : settlement.OwnerClan.Kingdom);
			if (!surrender || kingdom2 == kingdom)
			{
				Clan clan2 = ((clan != null && !clan.IsEliminated) ? clan : kingdom?.RulingClan);
				if (clan2 != null && clan2.Leader != null)
				{
					ChangeOwnerOfSettlementAction.ApplyByDefault(clan2.Leader, settlement);
				}
				settlement.Town.IsOwnerUnassigned = false;
			}
			else
			{
				settlement.Town.IsOwnerUnassigned = true;
			}
		}
		foreach (string item in list)
		{
			_warOccupations.Remove(item);
		}
		Diagnostics.Info("War settlement resolved for " + NameOf(first) + " and " + NameOf(second) + "; outcome=" + (surrender ? "surrender" : "white peace") + "; fiefs=" + list.Count.ToString("N0") + ".");
	}

	private static string GetWarKey(Kingdom first, Kingdom second)
	{
		if (first == null || second == null || string.IsNullOrEmpty(first.StringId) || string.IsNullOrEmpty(second.StringId))
		{
			return string.Empty;
		}
		if (string.CompareOrdinal(first.StringId, second.StringId) > 0)
		{
			return second.StringId + "|" + first.StringId;
		}
		return first.StringId + "|" + second.StringId;
	}

	private static bool IsFirstInWarKey(Kingdom first, Kingdom second)
	{
		if (first != null && second != null)
		{
			return string.CompareOrdinal(first.StringId, second.StringId) <= 0;
		}
		return false;
	}

	private static Kingdom AsKingdom(IFaction faction)
	{
		if (faction is Kingdom result)
		{
			return result;
		}
		if (faction is Clan clan)
		{
			return clan.Kingdom;
		}
		return null;
	}

	private static Settlement FindSettlement(string id)
	{
		foreach (Settlement item in Settlement.All)
		{
			if (item != null && string.Equals(item.StringId, id, StringComparison.Ordinal))
			{
				return item;
			}
		}
		return null;
	}

	private static Clan FindClan(string id)
	{
		foreach (Clan item in Clan.All)
		{
			if (item != null && string.Equals(item.StringId, id, StringComparison.Ordinal))
			{
				return item;
			}
		}
		return null;
	}

	private static Kingdom FindKingdom(string id)
	{
		foreach (Kingdom item in Kingdom.All)
		{
			if (item != null && string.Equals(item.StringId, id, StringComparison.Ordinal))
			{
				return item;
			}
		}
		return null;
	}

	private static bool TryGetWarKingdoms(string key, out Kingdom first, out Kingdom second)
	{
		first = null;
		second = null;
		string[] array = (key ?? string.Empty).Split('|');
		if (array.Length != 2)
		{
			return false;
		}
		first = FindKingdom(array[0]);
		second = FindKingdom(array[1]);
		if (first != null)
		{
			return second != null;
		}
		return false;
	}

	private void CaptureSettlementOwnership(bool recordChanges)
	{
		foreach (Settlement item in Settlement.All)
		{
			if (item != null && item.Town != null)
			{
				UpdateSettlementOwner(item, recordChanges);
			}
		}
	}

	internal static IFaction GetLiveSettlementFaction(Settlement settlement)
	{
		if (settlement == null)
		{
			return null;
		}
		IFaction faction = ((settlement.Town == null) ? null : settlement.Town.MapFaction);
		if (faction != null)
		{
			return faction;
		}
		faction = settlement.MapFaction;
		if (faction != null)
		{
			return faction;
		}
		Clan ownerClan = settlement.OwnerClan;
		object obj;
		if (ownerClan != null)
		{
			obj = ownerClan.MapFaction;
			if (obj == null)
			{
				return ownerClan;
			}
		}
		else
		{
			obj = null;
		}
		return (IFaction)obj;
	}

	private void UpdateSettlementOwner(Settlement settlement, bool recordChanges)
	{
		IFaction liveSettlementFaction = GetLiveSettlementFaction(settlement);
		if (settlement == null || liveSettlementFaction == null)
		{
			return;
		}
		string text = settlement.StringId ?? string.Empty;
		string text2 = NameOf(settlement);
		string text3 = liveSettlementFaction.StringId ?? string.Empty;
		string text4 = NameOf(liveSettlementFaction);
		string text5 = text + "\t" + text2 + "\t" + text3 + "\t" + text4;
		int num = -1;
		string a = null;
		for (int i = 0; i < _settlementOwners.Count; i++)
		{
			string[] array = _settlementOwners[i].Split(new char[1] { '\t' }, 4);
			if (array.Length != 0 && string.Equals(array[0], text, StringComparison.Ordinal))
			{
				num = i;
				a = ((array.Length > 2) ? array[2] : string.Empty);
				break;
			}
		}
		if (num < 0)
		{
			_settlementOwners.Add(text5);
			_ownershipRevision++;
			return;
		}
		if (string.Equals(a, text3, StringComparison.Ordinal))
		{
			_settlementOwners[num] = text5;
			return;
		}
		_settlementOwners[num] = text5;
		_ownershipRevision++;
		if (recordChanges)
		{
			Record("Settlement", text2 + " is now controlled by " + text4 + ".");
		}
	}

	private void OnGivenBirth(Hero mother, List<Hero> children, int numberOfChildren)
	{
		Record("Birth", NameOf(mother) + " gave birth to " + Math.Max(1, numberOfChildren) + " child" + ((numberOfChildren == 1) ? "." : "ren."));
	}

	private void OnHeroKilled(Hero victim, Hero killer, KillCharacterAction.KillCharacterActionDetail detail, bool showNotification)
	{
		Record("Death", NameOf(victim) + " died.");
	}

	private static string NameOf(object value)
	{
		if (value == null)
		{
			return "Unknown";
		}
		PropertyInfo property = value.GetType().GetProperty("Name");
		object obj = ((property == null) ? null : property.GetValue(value, null));
		if (obj != null)
		{
			return obj.ToString();
		}
		return value.ToString();
	}
}
}

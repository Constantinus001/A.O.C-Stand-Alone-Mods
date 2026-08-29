using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Library;

namespace TwelveMonthCalendar
{

internal sealed class CalendarMarriageCandidateVM : ViewModel
{
	private readonly Hero _candidate;

	private readonly Hero _clanLeader;

	private readonly bool _isCompatible;

	private readonly bool _canPursueMatch;

	private readonly Action<Hero, string> _sendMessenger;

	[DataSourceProperty]
	public string Name
	{
		get
		{
			if (_candidate != null && !(_candidate.Name == null))
			{
				return _candidate.Name.ToString();
			}
			return "Unknown noble";
		}
	}

	[DataSourceProperty]
	public string GenderText
	{
		get
		{
			if (_candidate == null || !_candidate.IsFemale)
			{
				return "MAN";
			}
			return "WOMAN";
		}
	}

	[DataSourceProperty]
	public string DetailsText
	{
		get
		{
			string clan = ((_candidate == null || _candidate.Clan == null || _candidate.Clan.Name == null) ? "No clan" : _candidate.Clan.Name.ToString());
			string kingdom = ((_candidate == null || _candidate.Clan == null || _candidate.Clan.Kingdom == null || _candidate.Clan.Kingdom.Name == null) ? "Independent" : _candidate.Clan.Kingdom.Name.ToString());
			return clan + "  •  " + kingdom + "  •  Age " + ((_candidate == null) ? "?" : _candidate.Age.ToString("0"));
		}
	}

	[DataSourceProperty]
	public string ClanLeaderText => "Clan leader: " + ((_clanLeader == null || _clanLeader.Name == null) ? "None" : _clanLeader.Name.ToString());

	[DataSourceProperty]
	public string EligibilityText
	{
		get
		{
			if (_isCompatible)
			{
				return "NATIVE COURTSHIP AVAILABLE";
			}
			if (_candidate != null && (_candidate.PartyBelongedTo == null || (_candidate.PartyBelongedTo.MapEvent == null && _candidate.PartyBelongedTo.Army == null)))
			{
				return "MESSENGER AVAILABLE • COURTSHIP CHECKED AT AUDIENCE";
			}
			return "CAMPAIGNING • MESSENGER CAN STILL TRAVEL";
		}
	}

	[DataSourceProperty]
	public string EligibilityColor
	{
		get
		{
			if (!_isCompatible)
			{
				return "#C9A55EFF";
			}
			return "#87BD82FF";
		}
	}

	[DataSourceProperty]
	public CharacterImageIdentifierVM Portrait { get; private set; }

	[DataSourceProperty]
	public bool IsEven { get; private set; }

	[DataSourceProperty]
	public bool CanContactCandidate
	{
		get
		{
			if (_canPursueMatch)
			{
				return IsAvailable(_candidate);
			}
			return false;
		}
	}

	[DataSourceProperty]
	public bool CanContactClanLeader
	{
		get
		{
			if (_canPursueMatch)
			{
				return IsAvailable(_clanLeader);
			}
			return false;
		}
	}

	[DataSourceProperty]
	public string CandidateButtonText => MessengerButtonText(_candidate, "Candidate", "SEND TO CANDIDATE");

	[DataSourceProperty]
	public string ClanLeaderButtonText => MessengerButtonText(_clanLeader, "ClanLeader", "SEND TO CLAN LEADER");

	internal CalendarMarriageCandidateVM(Hero player, Hero candidate, int index, Action<Hero, string> sendMessenger)
	{
		_candidate = candidate;
		_clanLeader = ((candidate == null || candidate.Clan == null) ? null : candidate.Clan.Leader);
		_sendMessenger = sendMessenger;
		_isCompatible = player != null && candidate != null && player.Spouse == null && candidate.Spouse == null && Campaign.Current != null && Campaign.Current.Models != null && Campaign.Current.Models.MarriageModel != null && Campaign.Current.Models.MarriageModel.IsCoupleSuitableForMarriage(player, candidate) && !FactionManager.IsAtWarAgainstFaction(player.MapFaction, candidate.MapFaction);
		_canPursueMatch = player != null && candidate != null && player.Spouse == null && candidate.Spouse == null && !FactionManager.IsAtWarAgainstFaction(player.MapFaction, candidate.MapFaction);
		IsEven = index % 2 == 0;
		Portrait = ((candidate == null || candidate.CharacterObject == null) ? new CharacterImageIdentifierVM(null) : new CharacterImageIdentifierVM(CharacterCode.CreateFrom(candidate.CharacterObject)));
	}

	private static bool IsAvailable(Hero hero)
	{
		if (hero != null && hero.IsAlive && !hero.IsPrisoner)
		{
			return hero.CharacterObject != null;
		}
		return false;
	}

	private static string MessengerButtonText(Hero target, string purpose, string idleText)
	{
		int days = CalendarWorldLedgerBehavior.GetMarriageMessengerDaysRemaining(target, purpose);
		if (days < 0)
		{
			return idleText;
		}
		if (days == 0)
		{
			return "OPEN ARRANGED AUDIENCE";
		}
		return "ARRIVES IN " + days.ToString("N0") + ((days == 1) ? " DAY" : " DAYS");
	}

	public void ExecuteContactCandidate()
	{
		if (CanContactCandidate && _sendMessenger != null)
		{
			_sendMessenger(_candidate, "Candidate");
		}
	}

	public void ExecuteContactClanLeader()
	{
		if (CanContactClanLeader && _sendMessenger != null)
		{
			_sendMessenger(_clanLeader, "ClanLeader");
		}
	}
}
}

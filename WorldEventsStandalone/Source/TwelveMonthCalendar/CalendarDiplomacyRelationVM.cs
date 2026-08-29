using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Library;

namespace TwelveMonthCalendar
{

internal sealed class CalendarDiplomacyRelationVM : ViewModel
{
	private readonly Kingdom _playerKingdom;

	private readonly Kingdom _otherKingdom;

	private readonly Action<Kingdom> _sendMessenger;

	private readonly BannerImageIdentifierVM _banner;

	[DataSourceProperty]
	public BannerImageIdentifierVM Banner => _banner;

	[DataSourceProperty]
	public string Name
	{
		get
		{
			if (_otherKingdom != null && !(_otherKingdom.Name == null))
			{
				return _otherKingdom.Name.ToString();
			}
			return "Unknown kingdom";
		}
	}

	[DataSourceProperty]
	public bool IsAtWar
	{
		get
		{
			if (_playerKingdom != null && _otherKingdom != null)
			{
				return _playerKingdom.IsAtWarWith(_otherKingdom);
			}
			return false;
		}
	}

	[DataSourceProperty]
	public bool IsEven { get; private set; }

	[DataSourceProperty]
	public string DailyIncomeText { get; private set; }

	[DataSourceProperty]
	public string DailyIncomeColor { get; private set; }

	[DataSourceProperty]
	public string RelationColor
	{
		get
		{
			if (!IsAtWar)
			{
				return "#4B9C2FFF";
			}
			return "#A63D31FF";
		}
	}

	[DataSourceProperty]
	public string RulerText
	{
		get
		{
			Hero ruler = ((_otherKingdom == null) ? null : _otherKingdom.Leader);
			return "Ruler: " + ((ruler == null || ruler.Name == null) ? "Unknown" : ruler.Name.ToString());
		}
	}

	[DataSourceProperty]
	public string StatusText
	{
		get
		{
			if (_otherKingdom != _playerKingdom)
			{
				if (_playerKingdom != null)
				{
					if (!IsAtWar)
					{
						return "AT PEACE";
					}
					return "AT WAR";
				}
				return "FOREIGN KINGDOM";
			}
			return "YOUR KINGDOM";
		}
	}

	[DataSourceProperty]
	public string ActiveWarsText
	{
		get
		{
			if (_otherKingdom == null)
			{
				return "No war information";
			}
			List<string> enemies = new List<string>();
			foreach (Kingdom kingdom in Kingdom.All)
			{
				if (kingdom != null && !kingdom.IsEliminated && kingdom != _otherKingdom && _otherKingdom.IsAtWarWith(kingdom))
				{
					enemies.Add((kingdom.Name == null) ? "Unknown kingdom" : kingdom.Name.ToString());
				}
			}
			enemies.Sort(StringComparer.Ordinal);
			if (enemies.Count != 0)
			{
				return "At war with: " + string.Join(", ", enemies.ToArray());
			}
			return "No active wars";
		}
	}

	[DataSourceProperty]
	public string PeaceDurationText
	{
		get
		{
			if (_otherKingdom == _playerKingdom)
			{
				return "Seat of your realm";
			}
			if (_playerKingdom == null || _otherKingdom == null)
			{
				return ActiveWarsText;
			}
			StanceLink stance = _playerKingdom.GetStanceWith(_otherKingdom);
			if (stance == null)
			{
				return "No relation record";
			}
			CampaignTime date = (IsAtWar ? stance.WarStartDate : stance.PeaceDeclarationDate);
			int days = ((!(date.ToDays <= 0.0)) ? Math.Max(0, (int)Math.Floor(date.ElapsedDaysUntilNow)) : 0);
			if (!IsAtWar)
			{
				return "Peace held for " + days.ToString("N0") + ((days == 1) ? " day" : " days");
			}
			return "War for " + days.ToString("N0") + ((days == 1) ? " day" : " days");
		}
	}

	[DataSourceProperty]
	public string TributeText
	{
		get
		{
			if (_playerKingdom == null || _otherKingdom == null)
			{
				return string.Empty;
			}
			int tribute = _playerKingdom.GetStanceWith(_otherKingdom)?.GetDailyTributeToPay(_playerKingdom) ?? 0;
			if (tribute > 0)
			{
				return "Tribute owed: " + tribute.ToString("N0") + "/day";
			}
			if (tribute < 0)
			{
				return "Tribute received: " + (-tribute).ToString("N0") + "/day";
			}
			return "No current tribute";
		}
	}

	[DataSourceProperty]
	public bool CanSendMessenger
	{
		get
		{
			Hero ruler = ((_otherKingdom == null) ? null : _otherKingdom.Leader);
			if (_otherKingdom != _playerKingdom && ruler != null && ruler != Hero.MainHero && ruler.IsAlive && !ruler.IsPrisoner && ruler.CharacterObject != null)
			{
				return !_otherKingdom.IsEliminated;
			}
			return false;
		}
	}

	[DataSourceProperty]
	public string MessengerButtonText
	{
		get
		{
			int daysRemaining = CalendarWorldLedgerBehavior.GetMessengerDaysRemaining(_otherKingdom);
			if (daysRemaining < 0)
			{
				return "SEND MESSENGER";
			}
			if (daysRemaining == 0)
			{
				return "OPEN AUDIENCE";
			}
			return "ARRIVES IN " + daysRemaining.ToString("N0") + ((daysRemaining == 1) ? " DAY" : " DAYS");
		}
	}

	internal CalendarDiplomacyRelationVM(Kingdom playerKingdom, Kingdom otherKingdom, int dailyIncome, int index, Action<Kingdom> sendMessenger)
	{
		_playerKingdom = playerKingdom;
		_otherKingdom = otherKingdom;
		_sendMessenger = sendMessenger;
		_banner = new BannerImageIdentifierVM(otherKingdom?.Banner, nineGrid: true);
		DailyIncomeText = ((dailyIncome >= 0) ? "+" : string.Empty) + dailyIncome.ToString("N0") + " denars / day";
		DailyIncomeColor = ((dailyIncome >= 0) ? "#8CC47CFF" : "#D36C5CFF");
		IsEven = index % 2 == 0;
	}

	public void ExecuteSendMessenger()
	{
		if (CanSendMessenger && _sendMessenger != null)
		{
			_sendMessenger(_otherKingdom);
		}
	}
}
}

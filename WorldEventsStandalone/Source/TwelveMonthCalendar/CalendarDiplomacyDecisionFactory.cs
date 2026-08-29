using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Election;

namespace TwelveMonthCalendar
{

internal static class CalendarDiplomacyDecisionFactory
{
	internal static KingdomDecision Create(Clan proposer, Kingdom targetKingdom, bool makePeace)
	{
		if (proposer == null || targetKingdom == null)
		{
			return null;
		}
		if (!makePeace)
		{
			return new DeclareWarDecision(proposer, targetKingdom);
		}
		int durationInDays = 0;
		int dailyTributeToPay = 0;
		if (Campaign.Current != null && Campaign.Current.Models != null && Campaign.Current.Models.DiplomacyModel != null && targetKingdom.Leader != null && targetKingdom.Leader.Clan != null)
		{
			dailyTributeToPay = Campaign.Current.Models.DiplomacyModel.GetDailyTributeToPay(proposer, targetKingdom.Leader.Clan, out durationInDays);
			dailyTributeToPay = 10 * (dailyTributeToPay / 10);
		}
		return new MakePeaceKingdomDecision(proposer, targetKingdom, dailyTributeToPay, durationInDays);
	}

	internal static bool HasPendingDecision(Kingdom playerKingdom, Kingdom targetKingdom, bool makePeace)
	{
		if (playerKingdom == null || targetKingdom == null)
		{
			return false;
		}
		foreach (KingdomDecision pending in playerKingdom.UnresolvedDecisions)
		{
			if (pending != null && !pending.ShouldBeCancelled())
			{
				MakePeaceKingdomDecision peace = pending as MakePeaceKingdomDecision;
				if (makePeace && peace != null && peace.FactionToMakePeaceWith == targetKingdom)
				{
					return true;
				}
				DeclareWarDecision war = pending as DeclareWarDecision;
				if (!makePeace && war != null && war.FactionToDeclareWarOn == targetKingdom)
				{
					return true;
				}
			}
		}
		return false;
	}
}
}

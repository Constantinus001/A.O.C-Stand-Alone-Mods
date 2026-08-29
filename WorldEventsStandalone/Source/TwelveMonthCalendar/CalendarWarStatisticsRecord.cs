using System;
using TaleWorlds.CampaignSystem;

namespace TwelveMonthCalendar
{

internal sealed class CalendarWarStatisticsRecord
{
	internal string LeftName { get; private set; }

	internal string RightName { get; private set; }

	internal Kingdom LeftKingdom { get; private set; }

	internal Kingdom RightKingdom { get; private set; }

	internal int LeftTroops { get; private set; }

	internal int RightTroops { get; private set; }

	internal int LeftShips { get; private set; }

	internal int RightShips { get; private set; }

	internal int LeftLosses { get; private set; }

	internal int RightLosses { get; private set; }

	internal int LeftShipLosses { get; private set; }

	internal int RightShipLosses { get; private set; }

	internal int WarScore { get; private set; }

	internal bool IsPlayerWar { get; private set; }

	internal int TotalLosses => LeftLosses + RightLosses;

	internal CalendarWarStatisticsRecord(Kingdom leftKingdom, Kingdom rightKingdom, string leftName, string rightName, int leftTroops, int rightTroops, int leftShips, int rightShips, int leftLosses, int rightLosses, int leftShipLosses, int rightShipLosses, int warScore, bool isPlayerWar)
	{
		LeftKingdom = leftKingdom;
		RightKingdom = rightKingdom;
		LeftName = leftName ?? "Unknown";
		RightName = rightName ?? "Unknown";
		LeftTroops = Math.Max(0, leftTroops);
		RightTroops = Math.Max(0, rightTroops);
		LeftShips = Math.Max(0, leftShips);
		RightShips = Math.Max(0, rightShips);
		LeftLosses = Math.Max(0, leftLosses);
		RightLosses = Math.Max(0, rightLosses);
		LeftShipLosses = Math.Max(0, leftShipLosses);
		RightShipLosses = Math.Max(0, rightShipLosses);
		WarScore = Math.Max(-100, Math.Min(100, warScore));
		IsPlayerWar = isPlayerWar;
	}
}
}

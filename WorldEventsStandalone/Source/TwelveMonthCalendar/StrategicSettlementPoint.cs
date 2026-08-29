using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;

namespace TwelveMonthCalendar
{

internal sealed class StrategicSettlementPoint
{
	internal Settlement Settlement { get; private set; }

	internal float SourceX { get; private set; }

	internal float SourceY { get; private set; }

	internal float DisplayX { get; private set; }

	internal float DisplayY { get; private set; }

	internal IFaction Owner { get; private set; }

	internal IFaction Besieger { get; private set; }

	internal bool IsUnderSiege => Besieger != null;

	internal StrategicSettlementPoint(Settlement settlement, float sourceX, float sourceY, IFaction owner, IFaction besieger)
	{
		Settlement = settlement;
		SourceX = sourceX;
		SourceY = sourceY;
		DisplayX = sourceX;
		DisplayY = sourceY;
		Owner = owner;
		Besieger = besieger;
	}

	internal void ResetDisplayPosition()
	{
		DisplayX = SourceX;
		DisplayY = SourceY;
	}

	internal void SetDisplayPosition(float x, float y)
	{
		DisplayX = x;
		DisplayY = y;
	}
}
}

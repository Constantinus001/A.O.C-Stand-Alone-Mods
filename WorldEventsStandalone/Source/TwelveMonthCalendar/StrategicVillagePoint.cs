namespace TwelveMonthCalendar
{

internal sealed class StrategicVillagePoint
{
	internal string SettlementId { get; private set; }

	internal float SourceX { get; private set; }

	internal float SourceY { get; private set; }

	internal StrategicVillagePoint(string settlementId, float sourceX, float sourceY)
	{
		SettlementId = settlementId ?? string.Empty;
		SourceX = sourceX;
		SourceY = sourceY;
	}
}
}

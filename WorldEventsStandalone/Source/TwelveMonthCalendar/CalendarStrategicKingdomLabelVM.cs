using TaleWorlds.Library;

namespace TwelveMonthCalendar
{

internal sealed class CalendarStrategicKingdomLabelVM : ViewModel
{
	[DataSourceProperty]
	public string Name { get; private set; }

	[DataSourceProperty]
	public int X { get; private set; }

	[DataSourceProperty]
	public int Y { get; private set; }

	[DataSourceProperty]
	public int HoldingCount { get; private set; }

	[DataSourceProperty]
	public string GoldColor => "#FFD66FFF";

	internal CalendarStrategicKingdomLabelVM(string name, int x, int y, int holdingCount)
	{
		Name = name ?? string.Empty;
		X = x;
		Y = y;
		HoldingCount = holdingCount;
	}
}
}

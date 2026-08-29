using TaleWorlds.Library;

namespace TwelveMonthCalendar
{

internal sealed class CalendarStrategicFriendlyArmyVM : ViewModel
{
	[DataSourceProperty]
	public int X { get; private set; }

	[DataSourceProperty]
	public int Y { get; private set; }

	[DataSourceProperty]
	public string Name { get; private set; }

	[DataSourceProperty]
	public string Glyph { get; private set; }

	[DataSourceProperty]
	public string MarkerColor { get; private set; }

	[DataSourceProperty]
	public string GoldColor => "#FFD66FFF";

	[DataSourceProperty]
	public string ShadowColor => "#100C08D8";

	internal CalendarStrategicFriendlyArmyVM(int x, int y, string name, string glyph, string markerColor)
	{
		X = x;
		Y = y;
		Name = name ?? "Friendly Party";
		Glyph = glyph ?? "A";
		MarkerColor = markerColor ?? "#65E89AFF";
	}
}
}

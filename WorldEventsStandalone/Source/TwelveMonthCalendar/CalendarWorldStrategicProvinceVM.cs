using TaleWorlds.Library;

namespace TwelveMonthCalendar
{

internal sealed class CalendarWorldStrategicProvinceVM : ViewModel
{
	private readonly string _spriteName;

	private readonly int _x;

	private readonly int _y;

	private readonly int _width;

	private readonly int _height;

	private readonly string _color;

	[DataSourceProperty]
	public string SpriteName => _spriteName;

	[DataSourceProperty]
	public int X => _x;

	[DataSourceProperty]
	public int Y => _y;

	[DataSourceProperty]
	public int Width => _width;

	[DataSourceProperty]
	public int Height => _height;

	[DataSourceProperty]
	public string Color => _color;

	internal CalendarWorldStrategicProvinceVM(string spriteName, int x, int y, int width, int height, string color)
	{
		_spriteName = spriteName ?? string.Empty;
		_x = x;
		_y = y;
		_width = width;
		_height = height;
		_color = color ?? "#FFFFFFFF";
	}
}
}

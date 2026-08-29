namespace TwelveMonthCalendar
{

internal sealed class CalendarStrategicProvinceDefinition
{
	internal string SpriteName { get; private set; }

	internal int X { get; private set; }

	internal int Y { get; private set; }

	internal int Width { get; private set; }

	internal int Height { get; private set; }

	internal float CenterX { get; private set; }

	internal float CenterY { get; private set; }

	internal CalendarStrategicProvinceDefinition(string spriteName, int x, int y, int width, int height, float centerX, float centerY)
	{
		SpriteName = spriteName;
		X = x;
		Y = y;
		Width = width;
		Height = height;
		CenterX = centerX;
		CenterY = centerY;
	}
}
}

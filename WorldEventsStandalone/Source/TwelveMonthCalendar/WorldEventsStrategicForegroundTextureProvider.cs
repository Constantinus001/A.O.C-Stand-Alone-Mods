namespace TwelveMonthCalendar
{

public sealed class WorldEventsStrategicForegroundTextureProvider : WorldEventsSkinTextureProvider
{
	protected override string AssetName => "page_cabinet_strategic_v1";

	protected override bool UseStrategicForegroundMask => true;
}
}

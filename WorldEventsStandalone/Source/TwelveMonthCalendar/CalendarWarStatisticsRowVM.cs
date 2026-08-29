using System;
using TaleWorlds.Library;

namespace TwelveMonthCalendar
{

internal sealed class CalendarWarStatisticsRowVM : ViewModel
{
	private readonly CalendarWarStatisticsRecord _record;

	private readonly Action<CalendarWarStatisticsRecord, bool> _concludeWar;

	[DataSourceProperty]
	public string LeftName { get; private set; }

	[DataSourceProperty]
	public string RightName { get; private set; }

	[DataSourceProperty]
	public string LeftTroopsText { get; private set; }

	[DataSourceProperty]
	public string RightTroopsText { get; private set; }

	[DataSourceProperty]
	public string LeftLossesText { get; private set; }

	[DataSourceProperty]
	public string RightLossesText { get; private set; }

	[DataSourceProperty]
	public string ConflictTotalText { get; private set; }

	[DataSourceProperty]
	public string WarScoreText { get; private set; }

	[DataSourceProperty]
	public string WarScoreStatusText { get; private set; }

	[DataSourceProperty]
	public int LeftAdvantageWidth { get; private set; }

	[DataSourceProperty]
	public int RightAdvantageWidth { get; private set; }

	[DataSourceProperty]
	public string ResolutionText { get; private set; }

	[DataSourceProperty]
	public bool CanResolveWar { get; private set; }

	[DataSourceProperty]
	public bool IsPlayerWar { get; private set; }

	[DataSourceProperty]
	public bool IsEven { get; private set; }

	internal CalendarWarStatisticsRowVM(CalendarWarStatisticsRecord record, int index, Action<CalendarWarStatisticsRecord, bool> concludeWar)
	{
		_record = record;
		_concludeWar = concludeWar;
		LeftName = record.LeftName.ToUpperInvariant();
		RightName = record.RightName.ToUpperInvariant();
		LeftTroopsText = record.LeftTroops.ToString("N0") + " vanilla faction strength";
		RightTroopsText = record.RightTroops.ToString("N0") + " vanilla faction strength";
		LeftLossesText = record.LeftLosses.ToString("N0") + " current-war casualties";
		RightLossesText = record.RightLosses.ToString("N0") + " current-war casualties";
		ConflictTotalText = record.TotalLosses.ToString("N0") + " native recorded casualties";
		WarScoreText = "BANNERLORD WAR RECORD";
		LeftAdvantageWidth = 0;
		RightAdvantageWidth = 0;
		WarScoreStatusText = "NO VANILLA WAR-SCORE PERCENTAGE";
		ResolutionText = string.Empty;
		CanResolveWar = false;
		IsPlayerWar = record.IsPlayerWar;
		IsEven = index % 2 == 0;
	}

	public void ExecuteResolveWar()
	{
		if (CanResolveWar && _concludeWar != null)
		{
			_concludeWar(_record, Math.Abs(_record.WarScore) >= 100);
		}
	}
}
}

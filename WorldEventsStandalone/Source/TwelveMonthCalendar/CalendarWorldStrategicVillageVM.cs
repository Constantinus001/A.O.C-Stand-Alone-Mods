using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace TwelveMonthCalendar
{

internal sealed class CalendarWorldStrategicVillageVM : ViewModel
{
	private readonly Settlement _settlement;

	private readonly Action<CalendarWorldStrategicVillageVM> _toggleTracking;

	internal Settlement Settlement => _settlement;

	[DataSourceProperty]
	public string Name
	{
		get
		{
			if (_settlement != null)
			{
				return _settlement.Name.ToString();
			}
			return string.Empty;
		}
	}

	[DataSourceProperty]
	public bool IsTracked
	{
		get
		{
			if (_settlement != null && Campaign.Current != null && Campaign.Current.VisualTrackerManager != null)
			{
				return Campaign.Current.VisualTrackerManager.CheckTracked(_settlement);
			}
			return false;
		}
	}

	[DataSourceProperty]
	public string TrackText
	{
		get
		{
			if (!IsTracked)
			{
				return "Track";
			}
			return "Untrack";
		}
	}

	internal CalendarWorldStrategicVillageVM(Settlement settlement, Action<CalendarWorldStrategicVillageVM> toggleTracking)
	{
		_settlement = settlement;
		_toggleTracking = toggleTracking;
	}

	public void ExecuteToggleTrack()
	{
		if (_toggleTracking != null)
		{
			_toggleTracking(this);
		}
	}

	internal void RefreshTracking()
	{
		OnPropertyChangedWithValue(IsTracked, "IsTracked");
		OnPropertyChangedWithValue(TrackText, "TrackText");
	}
}
}

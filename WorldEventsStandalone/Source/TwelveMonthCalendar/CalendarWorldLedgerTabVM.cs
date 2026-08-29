using System;
using TaleWorlds.Library;

namespace TwelveMonthCalendar
{

internal sealed class CalendarWorldLedgerTabVM : ViewModel
{
	private readonly Action<CalendarWorldLedgerTabVM> _select;

	private readonly string _baseLabel;

	private bool _isSelected;

	private string _label;

	internal string Filter { get; private set; }

	[DataSourceProperty]
	public string Label
	{
		get
		{
			return _label;
		}
		private set
		{
			if (!(_label == value))
			{
				_label = value;
				OnPropertyChangedWithValue(value, "Label");
			}
		}
	}

	[DataSourceProperty]
	public int TabWidth => 332;

	[DataSourceProperty]
	public string SelectedTabTextureProvider => Filter switch
	{
		"Character" => "WorldEventsExactSelectedStoryTextureProvider",
		"Diplomacy" => "WorldEventsExactSelectedRealmTextureProvider",
		"Strategic" => "WorldEventsExactSelectedStrategicTextureProvider",
		_ => "WorldEventsExactSelectedCalendarTextureProvider",
	};

	[DataSourceProperty]
	public string InactiveTabTextureProvider => Filter switch
	{
		"Character" => "WorldEventsExactInactiveStoryTextureProvider",
		"Diplomacy" => "WorldEventsExactInactiveRealmTextureProvider",
		"Strategic" => "WorldEventsExactInactiveStrategicTextureProvider",
		_ => "WorldEventsExactInactiveCalendarTextureProvider",
	};

	[DataSourceProperty]
	public string SelectedTabGoldFrameProvider => Filter switch
	{
		"Character" => "WorldEventsGoldFrameStoryTextureProvider",
		"Companions" => "WorldEventsGoldFrameCompanionsTextureProvider",
		"Diplomacy" => "WorldEventsGoldFrameDiplomacyTextureProvider",
		"Wars" => "WorldEventsGoldFrameWarTextureProvider",
		"Summaries" => "WorldEventsGoldFrameSummariesTextureProvider",
		"Strategic" => "WorldEventsGoldFrameMapTextureProvider",
		_ => "WorldEventsGoldFrameCalendarTextureProvider",
	};

	[DataSourceProperty]
	public string SkinIconProvider => Filter switch
	{
		"Character" => "WorldEventsStoryIconTextureProvider",
		"Companions" => "WorldEventsCompanionsIconTextureProvider",
		"Diplomacy" => "WorldEventsDiplomacyIconTextureProvider",
		"Wars" => "WorldEventsWarIconTextureProvider",
		"Summaries" => "WorldEventsSummariesIconTextureProvider",
		"Strategic" => "WorldEventsMapIconTextureProvider",
		_ => "WorldEventsCalendarIconTextureProvider",
	};

	[DataSourceProperty]
	public string SelectedTabForegroundProvider => Filter switch
	{
		"Character" => "WorldEventsForegroundStoryTextureProvider",
		"Companions" => "WorldEventsForegroundCompanionsTextureProvider",
		"Diplomacy" => "WorldEventsForegroundDiplomacyTextureProvider",
		"Wars" => "WorldEventsForegroundWarTextureProvider",
		"Summaries" => "WorldEventsForegroundSummariesTextureProvider",
		"Strategic" => "WorldEventsForegroundMapTextureProvider",
		_ => "WorldEventsForegroundCalendarTextureProvider",
	};

	[DataSourceProperty]
	public bool IsSelected
	{
		get
		{
			return _isSelected;
		}
		set
		{
			if (_isSelected != value)
			{
				_isSelected = value;
				Label = _baseLabel;
				OnPropertyChangedWithValue(value, "IsSelected");
			}
		}
	}

	internal CalendarWorldLedgerTabVM(string filter, string label, Action<CalendarWorldLedgerTabVM> select)
	{
		Filter = filter;
		_baseLabel = label;
		_label = label;
		_select = select;
	}

	public void ExecuteSelect()
	{
		if (_select != null)
		{
			_select(this);
		}
	}
}
}

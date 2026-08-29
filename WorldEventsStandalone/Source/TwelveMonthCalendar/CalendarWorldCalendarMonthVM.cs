using System;
using TaleWorlds.Library;

namespace TwelveMonthCalendar
{

internal sealed class CalendarWorldCalendarMonthVM : ViewModel
{
	private readonly string _title;

	private readonly string _eventCountText;

	private readonly MBBindingList<CalendarWorldCalendarDayVM> _days;

	private readonly long _startDay;

	private readonly long _endDay;

	private readonly Action<CalendarWorldCalendarMonthVM> _select;

	private bool _isExpanded;

	[DataSourceProperty]
	public string Title => _title;

	[DataSourceProperty]
	public string EventCountText => _eventCountText;

	[DataSourceProperty]
	public MBBindingList<CalendarWorldCalendarDayVM> Days => _days;

	[DataSourceProperty]
	public bool IsExpanded
	{
		get
		{
			return _isExpanded;
		}
		private set
		{
			if (_isExpanded != value)
			{
				_isExpanded = value;
				OnPropertyChangedWithValue(value, "IsExpanded");
				OnPropertyChangedWithValue(_isExpanded ? "-" : "+", "ExpandGlyph");
			}
		}
	}

	[DataSourceProperty]
	public string ExpandGlyph
	{
		get
		{
			if (!_isExpanded)
			{
				return "+";
			}
			return "-";
		}
	}

	internal long StartDay => _startDay;

	internal long EndDay => _endDay;

	internal CalendarWorldCalendarMonthVM(string title, string eventCountText, MBBindingList<CalendarWorldCalendarDayVM> days, long startDay, long endDay, bool isExpanded, Action<CalendarWorldCalendarMonthVM> select)
	{
		_title = title;
		_eventCountText = eventCountText;
		_days = days;
		_startDay = startDay;
		_endDay = endDay;
		_isExpanded = isExpanded;
		_select = select;
	}

	public void ExecuteToggle()
	{
		IsExpanded = !IsExpanded;
		if (_select != null)
		{
			_select(this);
		}
	}
}
}

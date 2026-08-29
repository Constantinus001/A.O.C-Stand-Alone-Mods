using System;
using TaleWorlds.Library;

namespace TwelveMonthCalendar
{

internal sealed class CalendarWorldCalendarDayVM : ViewModel
{
	private readonly string _dayNumber;

	private readonly string _eventSummary;

	private readonly long _absoluteDay;

	private readonly bool _isToday;

	private readonly bool _isSelectable;

	private readonly bool _hasQuestDue;

	private readonly Action<CalendarWorldCalendarDayVM> _select;

	private bool _isSelected;

	internal long AbsoluteDay => _absoluteDay;

	[DataSourceProperty]
	public string DayNumber => _dayNumber;

	[DataSourceProperty]
	public string EventSummary => _eventSummary;

	[DataSourceProperty]
	public bool IsSelectable => _isSelectable;

	[DataSourceProperty]
	public bool HasQuestDue => _hasQuestDue;

	[DataSourceProperty]
	public bool IsToday => _isToday;

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
				OnPropertyChangedWithValue(value, "IsSelected");
				OnPropertyChangedWithValue(BackgroundColor, "BackgroundColor");
			}
		}
	}

	[DataSourceProperty]
	public string BackgroundColor
	{
		get
		{
			if (_isSelected)
			{
				return "#75562EAA";
			}
			return "#FFFFFF00";
		}
	}

	internal CalendarWorldCalendarDayVM(string dayNumber, string eventSummary, long absoluteDay, bool isToday, bool isSelectable, bool isSelected, bool hasQuestDue, Action<CalendarWorldCalendarDayVM> select)
	{
		_dayNumber = dayNumber;
		_eventSummary = eventSummary;
		_absoluteDay = absoluteDay;
		_isToday = isToday;
		_isSelectable = isSelectable;
		_isSelected = isSelected;
		_hasQuestDue = hasQuestDue;
		_select = select;
	}

	internal static CalendarWorldCalendarDayVM Empty()
	{
		return new CalendarWorldCalendarDayVM(string.Empty, string.Empty, long.MinValue, isToday: false, isSelectable: false, isSelected: false, hasQuestDue: false, null);
	}

	public void ExecuteSelect()
	{
		if (_isSelectable && _select != null)
		{
			_select(this);
		}
	}
}
}

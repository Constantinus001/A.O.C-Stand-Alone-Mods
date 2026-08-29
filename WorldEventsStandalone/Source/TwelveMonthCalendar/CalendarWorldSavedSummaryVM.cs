using TaleWorlds.Library;

namespace TwelveMonthCalendar
{

internal sealed class CalendarWorldSavedSummaryVM : ViewModel
{
	private readonly string _title;

	private readonly string _summaryText;

	private readonly int _bodyHeight;

	private bool _isExpanded;

	[DataSourceProperty]
	public string Title => _title;

	[DataSourceProperty]
	public string SummaryText => _summaryText;

	[DataSourceProperty]
	public int BodyHeight => _bodyHeight;

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
				OnPropertyChangedWithValue(ActionText, "ActionText");
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

	[DataSourceProperty]
	public string ActionText
	{
		get
		{
			if (!_isExpanded)
			{
				return "OPEN RECORD  »";
			}
			return "CLOSE RECORD  «";
		}
	}

	internal CalendarWorldSavedSummaryVM(string title, string summaryText, int bodyHeight, bool isExpanded)
	{
		_title = title ?? string.Empty;
		_summaryText = summaryText ?? string.Empty;
		_bodyHeight = bodyHeight;
		_isExpanded = isExpanded;
	}

	public void ExecuteToggle()
	{
		IsExpanded = !IsExpanded;
	}
}
}

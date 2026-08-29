using System;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace TwelveMonthCalendar
{

internal sealed class CalendarWorldStrategicMarkerVM : ViewModel
{
	private readonly int _x;

	private readonly int _y;

	private readonly string _color;

	private readonly bool _isTown;

	private readonly int _size;

	private readonly int _iconSize;

	private readonly bool _showLabel;

	private readonly Settlement _settlement;

	private readonly Action<CalendarWorldStrategicMarkerVM> _select;

	private readonly bool _isUnderSiege;

	private bool _isSelected;

	internal Settlement Settlement => _settlement;

	[DataSourceProperty]
	public int X => _x;

	[DataSourceProperty]
	public int Y => _y;

	[DataSourceProperty]
	public string Color => _color;

	[DataSourceProperty]
	public string Label
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
	public bool ShowLabel => _showLabel;

	[DataSourceProperty]
	public bool IsTown => _isTown;

	[DataSourceProperty]
	public bool IsCastle => !_isTown;

	[DataSourceProperty]
	public bool IsUnderSiege => _isUnderSiege;

	[DataSourceProperty]
	public int Size => _size;

	[DataSourceProperty]
	public int IconSize => _iconSize;

	[DataSourceProperty]
	public string BorderColor
	{
		get
		{
			if (!_isSelected)
			{
				return "#100D0BFF";
			}
			return "#FFE3A3FF";
		}
	}

	[DataSourceProperty]
	public string GlowColor
	{
		get
		{
			if (!_isSelected)
			{
				return "#00000000";
			}
			return "#FFE3A3A8";
		}
	}

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
				OnPropertyChangedWithValue(BorderColor, "BorderColor");
				OnPropertyChangedWithValue(GlowColor, "GlowColor");
			}
		}
	}

	internal CalendarWorldStrategicMarkerVM(int x, int y, string color, Settlement settlement, bool isTown, int size, int iconSize, bool showLabel, bool isUnderSiege, bool isSelected, Action<CalendarWorldStrategicMarkerVM> select)
	{
		_x = x;
		_y = y;
		_color = color;
		_settlement = settlement;
		_isTown = isTown;
		_size = size;
		_iconSize = iconSize;
		_showLabel = showLabel && isTown;
		_isUnderSiege = isUnderSiege;
		_isSelected = isSelected;
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

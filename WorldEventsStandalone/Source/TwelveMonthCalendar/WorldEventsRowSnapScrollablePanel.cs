using System;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace TwelveMonthCalendar
{

public sealed class WorldEventsRowSnapScrollablePanel : ScrollablePanel
{
	private float _rowStride = 1f;

	private float _wheelTarget;

	private bool _hasWheelTarget;

	private bool _resetOnShow;

	private bool _wasVisible;

	private float _previousInnerHeight;

	[Editor(false)]
	public float RowStride
	{
		get
		{
			return _rowStride;
		}
		set
		{
			_rowStride = Math.Max(1f, value);
		}
	}

	[Editor(false)]
	public bool ResetOnShow
	{
		get
		{
			return _resetOnShow;
		}
		set
		{
			_resetOnShow = value;
		}
	}

	public WorldEventsRowSnapScrollablePanel(UIContext context)
		: base(context)
	{
	}

	protected override void OnUpdate(float dt)
	{
		base.OnUpdate(dt);
		float innerHeight = ((base.InnerPanel == null) ? 0f : base.InnerPanel.Size.Y);
		bool contentBecameReady = _previousInnerHeight <= 0.5f && innerHeight > 0.5f;
		if (_resetOnShow && base.IsVisible && (!_wasVisible || contentBecameReady))
		{
			float minimum = ((base.VerticalScrollbar == null) ? 0f : base.VerticalScrollbar.MinValue);
			if (base.VerticalScrollbar != null)
			{
				base.VerticalScrollbar.SetValueForced(minimum);
			}
			if (base.InnerPanel != null)
			{
				base.InnerPanel.ScaledPositionYOffset = 0f - minimum;
			}
			_wheelTarget = minimum;
			_hasWheelTarget = true;
		}
		_wasVisible = base.IsVisible;
		_previousInnerHeight = innerHeight;
	}

	protected override bool OnPreviewMouseScroll()
	{
		return true;
	}

	protected override void OnMouseScroll()
	{
		if (base.VerticalScrollbar != null && base.EventManager.DeltaMouseScroll != 0f)
		{
			float current = base.VerticalScrollbar.ValueFloat;
			if (!_hasWheelTarget || Math.Abs(current - _wheelTarget) > _rowStride)
			{
				_wheelTarget = (float)Math.Round(current / _rowStride) * _rowStride;
			}
			float direction = ((base.EventManager.DeltaMouseScroll < 0f) ? 1f : (-1f));
			_wheelTarget = Math.Max(base.VerticalScrollbar.MinValue, Math.Min(base.VerticalScrollbar.MaxValue, _wheelTarget + direction * _rowStride));
			_hasWheelTarget = true;
			SetVerticalScrollTarget(_wheelTarget, 0.1f);
		}
	}
}
}

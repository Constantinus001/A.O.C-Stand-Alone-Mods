using System;
using TaleWorlds.GauntletUI;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets;

namespace TwelveMonthCalendar
{

public sealed class StrategicMapZoomScrollablePanel : EncyclopediaTroopScrollablePanel
{
	private bool _wheelDiagnosticLogged;

	private float _previousCanvasWidth;

	private float _previousCanvasHeight;

	public StrategicMapZoomScrollablePanel(UIContext context)
		: base(context)
	{
	}

	protected override bool OnPreviewMouseScroll()
	{
		if (!_wheelDiagnosticLogged)
		{
			_wheelDiagnosticLogged = true;
			Diagnostics.Info("Strategic map wheel diagnostic: scroll panel rejected wheel input; screen-level zoom remains active.");
		}
		return false;
	}

	protected override void OnLateUpdate(float dt)
	{
		if (base.InnerPanel == null || base.ClipRect == null)
		{
			base.OnLateUpdate(dt);
			return;
		}
		float canvasWidth = base.InnerPanel.Size.X;
		float canvasHeight = base.InnerPanel.Size.Y;
		bool initializeViewport = _previousCanvasWidth <= 0f && _previousCanvasHeight <= 0f && canvasWidth > 0f && canvasHeight > 0f && base.ClipRect.Size.X > 0f && base.ClipRect.Size.Y > 0f;
		bool widthChanged = _previousCanvasWidth > 0f && Math.Abs(canvasWidth - _previousCanvasWidth) > 0.5f;
		bool heightChanged = _previousCanvasHeight > 0f && Math.Abs(canvasHeight - _previousCanvasHeight) > 0.5f;
		if (initializeViewport)
		{
			if (base.HorizontalScrollbar != null)
			{
				float horizontalOverflow = Math.Max(0f, canvasWidth - base.ClipRect.Size.X);
				base.HorizontalScrollbar.MaxValue = Math.Max(1f, horizontalOverflow);
				base.HorizontalScrollbar.SetValueForced(horizontalOverflow * 0.5f);
			}
			if (base.VerticalScrollbar != null)
			{
				float verticalOverflow = Math.Max(0f, canvasHeight - base.ClipRect.Size.Y);
				base.VerticalScrollbar.MaxValue = Math.Max(1f, verticalOverflow);
				base.VerticalScrollbar.SetValueForced(verticalOverflow * 0.5f);
			}
		}
		if (widthChanged && base.HorizontalScrollbar != null)
		{
			float centerRatio2 = Clamp01((base.HorizontalScrollbar.ValueFloat + base.ClipRect.Size.X * 0.5f) / _previousCanvasWidth);
			float newMaximum2 = Math.Max(1f, canvasWidth - base.ClipRect.Size.X);
			float target2 = Math.Max(0f, Math.Min(newMaximum2, centerRatio2 * canvasWidth - base.ClipRect.Size.X * 0.5f));
			base.HorizontalScrollbar.MaxValue = newMaximum2;
			base.HorizontalScrollbar.SetValueForced(target2);
		}
		if (heightChanged && base.VerticalScrollbar != null)
		{
			float centerRatio = Clamp01((base.VerticalScrollbar.ValueFloat + base.ClipRect.Size.Y * 0.5f) / _previousCanvasHeight);
			float newMaximum = Math.Max(1f, canvasHeight - base.ClipRect.Size.Y);
			float target = Math.Max(0f, Math.Min(newMaximum, centerRatio * canvasHeight - base.ClipRect.Size.Y * 0.5f));
			base.VerticalScrollbar.MaxValue = newMaximum;
			base.VerticalScrollbar.SetValueForced(target);
		}
		base.OnLateUpdate(dt);
		if (widthChanged || heightChanged)
		{
			Diagnostics.Info("Strategic map zoom diagnostic: viewport center preserved; canvas=" + _previousCanvasWidth.ToString("0") + "x" + _previousCanvasHeight.ToString("0") + " -> " + canvasWidth.ToString("0") + "x" + canvasHeight.ToString("0") + ".");
		}
		else if (initializeViewport)
		{
			Diagnostics.Info("Strategic map viewport diagnostic: initial view centered; canvas=" + canvasWidth.ToString("0") + "x" + canvasHeight.ToString("0") + "; clip=" + base.ClipRect.Size.X.ToString("0") + "x" + base.ClipRect.Size.Y.ToString("0") + ".");
		}
		_previousCanvasWidth = canvasWidth;
		_previousCanvasHeight = canvasHeight;
	}

	private static float Clamp01(float value)
	{
		return Math.Max(0f, Math.Min(1f, value));
	}
}
}

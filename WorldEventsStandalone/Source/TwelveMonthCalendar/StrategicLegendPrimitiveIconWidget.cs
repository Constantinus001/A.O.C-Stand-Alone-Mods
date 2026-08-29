using System;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace TwelveMonthCalendar
{

public sealed class StrategicLegendPrimitiveIconWidget : Widget
{
	private const float DesignSize = 30f;

	private static readonly float[,] TownParts = new float[15, 4]
	{
		{ 3f, 10f, 12f, 16f },
		{ 4f, 8f, 10f, 3f },
		{ 6f, 5f, 6f, 3f },
		{ 5f, 11f, 8f, 13f },
		{ 5f, 9f, 8f, 2f },
		{ 7f, 7f, 4f, 2f },
		{ 7f, 13f, 2f, 8f },
		{ 9f, 18f, 3f, 6f },
		{ 14f, 14f, 13f, 12f },
		{ 16f, 11f, 9f, 3f },
		{ 18f, 8f, 5f, 3f },
		{ 16f, 15f, 9f, 9f },
		{ 17f, 13f, 7f, 2f },
		{ 19f, 10f, 3f, 3f },
		{ 20f, 19f, 3f, 5f }
	};

	private static readonly float[,] CastleParts = new float[6, 4]
	{
		{ 3f, 5f, 24f, 22f },
		{ 5f, 7f, 20f, 18f },
		{ 9f, 5f, 4f, 5f },
		{ 17f, 5f, 4f, 5f },
		{ 7f, 10f, 3f, 11f },
		{ 12f, 18f, 6f, 7f }
	};

	private bool _diagnosticLogged;

	public string IconKind { get; set; }

	public StrategicLegendPrimitiveIconWidget(UIContext context)
		: base(context)
	{
	}

	protected override void OnUpdate(float dt)
	{
		base.OnUpdate(dt);
		float iconSize = ((base.SuggestedWidth > 0f) ? base.SuggestedWidth : 30f);
		float[,] parts = (string.Equals(IconKind, "Castle", StringComparison.OrdinalIgnoreCase) ? CastleParts : TownParts);
		int partCount = Math.Min(base.ChildCount, parts.GetLength(0));
		for (int index = 0; index < partCount; index++)
		{
			Widget child = GetChild(index);
			child.SuggestedWidth = parts[index, 2] * iconSize / 30f;
			child.SuggestedHeight = parts[index, 3] * iconSize / 30f;
			child.MarginLeft = parts[index, 0] * iconSize / 30f;
			child.MarginTop = parts[index, 1] * iconSize / 30f;
		}
		if (!_diagnosticLogged)
		{
			_diagnosticLogged = true;
			Diagnostics.Info("Strategic legend primitive diagnostic: kind=" + (IconKind ?? "Town") + "; children=" + base.ChildCount + "; configuredParts=" + parts.GetLength(0) + "; iconSize=" + iconSize.ToString("0.0") + ".");
		}
	}
}
}

using System;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;
using TaleWorlds.TwoDimension;

namespace TwelveMonthCalendar
{

public sealed class StrategicLegendDrawWidget : Widget
{
	private const float TownDesignSize = 36f;

	private const float CastleDesignSize = 30f;

	private static readonly float[,] CastleOutline = new float[1, 4] { { 3f, 4f, 24f, 23f } };

	private static readonly float[,] CastleFill = new float[1, 4] { { 5f, 8f, 20f, 17f } };

	private static readonly float[,] CastleCutouts = new float[2, 4]
	{
		{ 8f, 4f, 4f, 6f },
		{ 17f, 4f, 4f, 6f }
	};

	private static readonly float[,] CastleHighlight = new float[1, 4] { { 6f, 10f, 3f, 11f } };

	private static readonly float[,] CastleDetail = new float[1, 4] { { 12f, 18f, 6f, 7f } };

	private bool _diagnosticLogged;

	public string IconKind { get; set; }

	public StrategicLegendDrawWidget(UIContext context)
		: base(context)
	{
	}

	protected override void OnRender(TwoDimensionContext context, TwoDimensionDrawContext drawContext)
	{
		base.OnRender(context, drawContext);
		Sprite primitive = base.Context.SpriteData.GetSprite("BlankWhiteSquare");
		if (primitive != null && !(base.Size.X <= 0f) && !(base.Size.Y <= 0f))
		{
			SimpleMaterial outline = CreateMaterial(drawContext, 24, 17, 12);
			SimpleMaterial fill = CreateMaterial(drawContext, 183, 136, 68);
			SimpleMaterial highlight = CreateMaterial(drawContext, 222, 180, 99);
			SimpleMaterial detail = CreateMaterial(drawContext, 49, 32, 19);
			outline.Texture = primitive.Texture;
			fill.Texture = primitive.Texture;
			highlight.Texture = primitive.Texture;
			detail.Texture = primitive.Texture;
			bool isCastle = string.Equals(IconKind, "Castle", StringComparison.OrdinalIgnoreCase);
			float scale = Math.Min(base.Size.X, base.Size.Y) / (isCastle ? 30f : 36f);
			if (isCastle)
			{
				DrawParts(drawContext, primitive, outline, CastleOutline, scale);
				DrawParts(drawContext, primitive, fill, CastleFill, scale);
				DrawParts(drawContext, primitive, outline, CastleCutouts, scale);
				DrawParts(drawContext, primitive, highlight, CastleHighlight, scale);
				DrawParts(drawContext, primitive, detail, CastleDetail, scale);
			}
			else
			{
				DrawTownMarker(drawContext, primitive, outline, fill, highlight, detail, scale);
			}
			if (!_diagnosticLogged)
			{
				_diagnosticLogged = true;
				Diagnostics.Info("Strategic legend draw diagnostic: kind=" + (IconKind ?? "Town") + "; primitiveValid=" + (primitive.Texture != null && primitive.Texture.IsValid) + "; size=" + base.Size.X.ToString("0") + "x" + base.Size.Y.ToString("0") + ".");
			}
		}
	}

	private void DrawTownMarker(TwoDimensionDrawContext drawContext, Sprite primitive, SimpleMaterial outline, SimpleMaterial fill, SimpleMaterial highlight, SimpleMaterial detail, float scale)
	{
		DrawRectangle(drawContext, primitive, outline, 2f, 17f, 15f, 18f, scale);
		DrawRectangle(drawContext, primitive, outline, 18f, 22f, 15f, 13f, scale);
		DrawFilledTriangle(drawContext, primitive, outline, 9f, 0f, 0f, 17f, 19f, 17f, scale);
		DrawFilledTriangle(drawContext, primitive, outline, 25f, 8f, 16f, 22f, 35f, 22f, scale);
		DrawRectangle(drawContext, primitive, fill, 4f, 18f, 11f, 15f, scale);
		DrawRectangle(drawContext, primitive, fill, 20f, 23f, 11f, 10f, scale);
		DrawFilledTriangle(drawContext, primitive, fill, 9f, 4f, 3f, 16f, 16f, 16f, scale);
		DrawFilledTriangle(drawContext, primitive, fill, 25f, 12f, 19f, 21f, 32f, 21f, scale);
		DrawRectangle(drawContext, primitive, highlight, 4f, 19f, 3f, 13f, scale);
		DrawRectangle(drawContext, primitive, highlight, 20f, 24f, 3f, 9f, scale);
		DrawRectangle(drawContext, primitive, detail, 7f, 27f, 4f, 8f, scale);
		DrawRectangle(drawContext, primitive, detail, 26f, 28f, 4f, 7f, scale);
	}

	private void DrawFilledTriangle(TwoDimensionDrawContext drawContext, Sprite primitive, SimpleMaterial material, float apexX, float apexY, float leftX, float baseY, float rightX, float ignoredRightY, float scale)
	{
		int num = (int)Math.Floor(apexY);
		int lastRow = (int)Math.Ceiling(baseY);
		float height = Math.Max(1f, baseY - apexY);
		for (int row = num; row <= lastRow; row++)
		{
			float progress = Math.Max(0f, Math.Min(1f, ((float)row - apexY) / height));
			float rowLeft = apexX + (leftX - apexX) * progress;
			float rowRight = apexX + (rightX - apexX) * progress;
			DrawRectangle(drawContext, primitive, material, rowLeft, row, Math.Max(1f, rowRight - rowLeft), 1.15f, scale);
		}
	}

	private void DrawRectangle(TwoDimensionDrawContext drawContext, Sprite primitive, SimpleMaterial material, float left, float top, float width, float height, float scale)
	{
		Rectangle2D rectangle = AreaRect;
		rectangle.SetVisualOffset(left * scale, top * scale);
		rectangle.SetVisualScale(width * scale / base.Size.X, height * scale / base.Size.Y);
		rectangle.ValidateVisuals();
		drawContext.DrawSprite(primitive, material, in rectangle, base._scaleToUse);
	}

	private SimpleMaterial CreateMaterial(TwoDimensionDrawContext drawContext, byte red, byte green, byte blue)
	{
		SimpleMaterial simpleMaterial = drawContext.CreateSimpleMaterial();
		simpleMaterial.Color = new Color((float)(int)red / 255f, (float)(int)green / 255f, (float)(int)blue / 255f);
		simpleMaterial.ColorFactor = 1f;
		simpleMaterial.AlphaFactor = base.Context.ContextAlpha;
		return simpleMaterial;
	}

	private void DrawParts(TwoDimensionDrawContext drawContext, Sprite primitive, SimpleMaterial material, float[,] parts, float scale)
	{
		for (int index = 0; index < parts.GetLength(0); index++)
		{
			float left = parts[index, 0] * scale;
			float top = parts[index, 1] * scale;
			float width = parts[index, 2] * scale;
			float height = parts[index, 3] * scale;
			Rectangle2D rectangle = AreaRect;
			rectangle.SetVisualOffset(left, top);
			rectangle.SetVisualScale(width / base.Size.X, height / base.Size.Y);
			rectangle.ValidateVisuals();
			drawContext.DrawSprite(primitive, material, in rectangle, base._scaleToUse);
		}
	}
}
}

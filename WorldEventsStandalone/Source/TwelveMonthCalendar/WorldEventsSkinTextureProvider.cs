using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI;
using TaleWorlds.TwoDimension;

namespace TwelveMonthCalendar
{

public abstract class WorldEventsSkinTextureProvider : TextureProvider
{
	private static bool _hasReportedLayoutStatus;

	private TaleWorlds.Engine.Texture _engineTexture;

	private TaleWorlds.TwoDimension.Texture _renderTexture;

	protected abstract string AssetName { get; }

	protected virtual bool UseStrategicForegroundMask => false;

	protected override TaleWorlds.TwoDimension.Texture OnGetTextureForRender(TwoDimensionContext context, string name)
	{
		if (_renderTexture != null && _renderTexture.IsValid)
		{
			return _renderTexture;
		}
		try
		{
			string path = System.IO.Path.Combine(GetModuleRoot(), "GUI", "CustomUI", "WorldEventsSkin", AssetName + ".png");
			if (!File.Exists(path))
			{
				Diagnostics.Info("World Events skin asset is unavailable: " + AssetName + ".");
				return null;
			}
			byte[] bytes = File.ReadAllBytes(path);
			string renderAssetName = (UseStrategicForegroundMask ? (AssetName + "_foreground_mask") : AssetName);
			int width;
			int height;
			byte[] renderBytes = PrepareRawTexture(bytes, renderAssetName, out width, out height);
			_engineTexture = TaleWorlds.Engine.Texture.CreateFromByteArray(renderBytes, width, height);
			if (_engineTexture == null || _engineTexture.IsReleased)
			{
				Diagnostics.Info("World Events skin asset was rejected by the renderer: provider=" + GetType().Name + "; asset=" + AssetName + ".");
				return null;
			}
			_engineTexture.Name = "aoc_world_events_" + renderAssetName;
			_renderTexture = new TaleWorlds.TwoDimension.Texture(new EngineTexture(_engineTexture));
			Diagnostics.Info("World Events skin asset loaded: provider=" + GetType().Name + "; asset=" + AssetName + "; sourceBytes=" + bytes.Length + "; rawBytes=" + renderBytes.Length + "; rawSize=" + width + "x" + height + "; checkerboard=removed; decoder=raw-bgra; path=" + path + ".");
			return _renderTexture;
		}
		catch (Exception exception)
		{
			Diagnostics.Error("World Events skin asset could not be loaded: " + AssetName + ".", exception);
			return null;
		}
	}

	public override void Clear(bool clearNextFrame)
	{
		base.Clear(clearNextFrame);
		_renderTexture = null;
		if (_engineTexture != null && !_engineTexture.IsReleased)
		{
			_engineTexture.ReleaseAfterNumberOfFrames((!clearNextFrame) ? 1 : 3);
		}
		_engineTexture = null;
	}

	internal static void ReportLayoutStatus()
	{
		if (_hasReportedLayoutStatus)
		{
			return;
		}
		_hasReportedLayoutStatus = true;
		try
		{
			string moduleRoot = GetModuleRoot();
			string layoutPath = System.IO.Path.Combine(moduleRoot, "GUI", "Prefabs", "WorldCalendar", "WorldCalendar.xml");
			string referencePath = System.IO.Path.Combine(moduleRoot, "GUI", "CustomUI", "world_events_shell_v3.png");
			string spriteDataPath = System.IO.Path.Combine(moduleRoot, "GUI", "Ages Of CalradiaSpriteData.xml");
			FileInfo layout = new FileInfo(layoutPath);
			FileInfo reference = new FileInfo(referencePath);
			FileInfo fileInfo = new FileInfo(spriteDataPath);
			bool layoutUsesActiveShell = layout.Exists && File.ReadAllText(layoutPath).Contains("Sprite=\"world_events_shell_current_tabs_alpha\"");
			bool activeShellRegistered = fileInfo.Exists && File.ReadAllText(spriteDataPath).Contains("<Name>world_events_shell_current_tabs_alpha</Name>");
			bool layoutUsesSelectedTab = layout.Exists && File.ReadAllText(layoutPath).Contains("Sprite=\"world_events_tab_selected_gold\"");
			bool selectedTabRegistered = fileInfo.Exists && File.ReadAllText(spriteDataPath).Contains("<Name>world_events_tab_selected_gold</Name>");
			Diagnostics.Info("World Events UI diagnostics: layoutExists=" + layout.Exists + "; layoutBytes=" + (layout.Exists ? layout.Length.ToString() : "0") + "; referenceExists=" + reference.Exists + "; referenceBytes=" + (reference.Exists ? reference.Length.ToString() : "0") + "; activeShellSprite=world_events_shell_current_tabs_alpha; layoutUsesActiveShell=" + layoutUsesActiveShell + "; activeShellRegistered=" + activeShellRegistered + "; selectedTabSprite=world_events_tab_selected_gold; layoutUsesSelectedTab=" + layoutUsesSelectedTab + "; selectedTabRegistered=" + selectedTabRegistered + ".");
		}
		catch (Exception exception)
		{
			Diagnostics.Error("World Events UI diagnostics could not inspect its layout assets.", exception);
		}
	}

	internal static string GetModuleRoot()
	{
		string assemblyDirectory = System.IO.Path.GetDirectoryName(typeof(WorldEventsSkinTextureProvider).Assembly.Location);
		return ((string.IsNullOrEmpty(assemblyDirectory) ? null : Directory.GetParent(assemblyDirectory))?.Parent ?? throw new InvalidOperationException("World Events module directory could not be resolved.")).FullName;
	}

	internal static byte[] PrepareRawTexture(byte[] pngBytes, string assetName, out int width, out int height)
	{
		using MemoryStream input = new MemoryStream(pngBytes, writable: false);
		using Bitmap source = new Bitmap(input);
		using Bitmap bitmap = new Bitmap((!string.IsNullOrEmpty(assetName) && assetName.StartsWith("icon_", StringComparison.Ordinal)) ? 32 : source.Width, (!string.IsNullOrEmpty(assetName) && assetName.StartsWith("icon_", StringComparison.Ordinal)) ? 32 : ((!string.IsNullOrEmpty(assetName) && assetName.StartsWith("tab_", StringComparison.Ordinal)) ? Math.Min(137, source.Height) : source.Height), PixelFormat.Format32bppArgb);
		bool isIcon = !string.IsNullOrEmpty(assetName) && assetName.StartsWith("icon_", StringComparison.Ordinal);
		bool isStrategicForeground = string.Equals(assetName, "page_cabinet_strategic_v1_foreground_mask", StringComparison.Ordinal);
		using (Graphics graphics = Graphics.FromImage(bitmap))
		{
			graphics.Clear(Color.Transparent);
			if (isIcon)
			{
				graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
				graphics.DrawImage(source, new Rectangle(0, 0, 32, 32), 0, 0, source.Width, source.Height, GraphicsUnit.Pixel);
			}
			else
			{
				graphics.DrawImageUnscaled(source, 0, 0);
			}
		}
		Rectangle bounds = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
		BitmapData data = bitmap.LockBits(bounds, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
		try
		{
			int absoluteStride = Math.Abs(data.Stride);
			byte[] lockedPixels = new byte[absoluteStride * bitmap.Height];
			byte[] pixels = new byte[bitmap.Width * bitmap.Height * 4];
			Marshal.Copy(data.Scan0, lockedPixels, 0, lockedPixels.Length);
			for (int y = 0; y < bitmap.Height; y++)
			{
				int lockedRowOffset = ((data.Stride >= 0) ? (y * absoluteStride) : ((bitmap.Height - 1 - y) * absoluteStride));
				int rowOffset = y * bitmap.Width * 4;
				Buffer.BlockCopy(lockedPixels, lockedRowOffset, pixels, rowOffset, bitmap.Width * 4);
				for (int x = 0; x < bitmap.Width; x++)
				{
					int pixelOffset = rowOffset + x * 4;
					byte blue = pixels[pixelOffset];
					byte green = pixels[pixelOffset + 1];
					byte red = pixels[pixelOffset + 2];
					byte minimum = Math.Min(red, Math.Min(green, blue));
					byte maximum = Math.Max(red, Math.Max(green, blue));
					if (minimum >= (isIcon ? 190 : 218) && maximum - minimum <= (isIcon ? 24 : 18))
					{
						pixels[pixelOffset + 3] = 0;
					}
					if (isStrategicForeground)
					{
						bool num = x >= 42 && x <= 895 && y >= 56 && y <= 539;
						bool zoomConsoleArtwork = x >= 624 && x <= 874 && y >= 493 && y <= 557;
						bool legendAperture = x >= 934 && x <= 1217 && y >= 61 && y <= 249;
						bool ledgerAperture = x >= 934 && x <= 1217 && y >= 282 && y <= 529;
						if ((num && !zoomConsoleArtwork) || legendAperture || ledgerAperture)
						{
							pixels[pixelOffset + 3] = 0;
						}
					}
					pixels[pixelOffset] = red;
					pixels[pixelOffset + 2] = blue;
				}
			}
			width = bitmap.Width;
			height = bitmap.Height;
			return pixels;
		}
		finally
		{
			bitmap.UnlockBits(data);
		}
	}
}
}

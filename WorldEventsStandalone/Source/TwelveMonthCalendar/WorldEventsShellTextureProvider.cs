using System;
using System.IO;
using System.Security.Cryptography;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI;
using TaleWorlds.TwoDimension;

namespace TwelveMonthCalendar
{

public sealed class WorldEventsShellTextureProvider : TextureProvider
{
	private const string ShellFileName = "world_events_shell_v3.png";

	private TaleWorlds.Engine.Texture _engineTexture;

	private TaleWorlds.TwoDimension.Texture _renderTexture;

	protected override TaleWorlds.TwoDimension.Texture OnGetTextureForRender(TwoDimensionContext context, string name)
	{
		if (_renderTexture != null && _renderTexture.IsValid)
		{
			return _renderTexture;
		}
		try
		{
			string path = System.IO.Path.Combine(WorldEventsSkinTextureProvider.GetModuleRoot(), "GUI", "CustomUI", "world_events_shell_v3.png");
			if (!File.Exists(path))
			{
				Diagnostics.Info("World Events shell is unavailable: path=" + path + ".");
				return null;
			}
			byte[] bytes = File.ReadAllBytes(path);
			int width;
			int height;
			byte[] rawBytes = WorldEventsSkinTextureProvider.PrepareRawTexture(bytes, null, out width, out height);
			TaleWorlds.Engine.Texture replacement = TaleWorlds.Engine.Texture.CreateFromByteArray(rawBytes, width, height);
			if (replacement == null || replacement.IsReleased)
			{
				Diagnostics.Info("World Events shell was rejected by the renderer: path=" + path + ".");
				return null;
			}
			replacement.Name = "ages_of_calradia_world_events_shell_v3";
			_engineTexture = replacement;
			_renderTexture = new TaleWorlds.TwoDimension.Texture(new EngineTexture(replacement));
			Diagnostics.Info("World Events shell loaded: provider=WorldEventsShellTextureProvider; request=" + (name ?? "<null>") + "; png=" + replacement.Width + "x" + replacement.Height + "; sourceBytes=" + bytes.Length + "; rawBytes=" + rawBytes.Length + "; decoder=raw-bgra; sourceSha256=" + ComputeSha256(bytes) + "; colorChannels=source; path=" + path + ".");
			return _renderTexture;
		}
		catch (Exception exception)
		{
			Diagnostics.Error("World Events shell could not be loaded.", exception);
			return null;
		}
	}

	public override void Clear(bool clearNextFrame)
	{
		base.Clear(clearNextFrame);
		_renderTexture = null;
		if (_engineTexture != null && !_engineTexture.IsReleased)
		{
			_engineTexture.ReleaseAfterNumberOfFrames(clearNextFrame ? 3 : 0);
		}
		_engineTexture = null;
	}

	private static string ComputeSha256(byte[] bytes)
	{
		using SHA256 sha256 = SHA256.Create();
		return BitConverter.ToString(sha256.ComputeHash(bytes)).Replace("-", string.Empty).ToLowerInvariant();
	}
}
}

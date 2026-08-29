using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI;
using TaleWorlds.TwoDimension;

namespace TwelveMonthCalendar
{

public sealed class CalendarStrategicCampaignAtlasTextureProvider : TextureProvider
{
	private sealed class StrategicVillageSnapshot : IEquatable<StrategicVillageSnapshot>
	{
		internal string SettlementId { get; private set; }

		internal float X { get; private set; }

		internal float Y { get; private set; }

		internal StrategicVillageSnapshot(string settlementId, float x, float y)
		{
			SettlementId = settlementId ?? string.Empty;
			X = x;
			Y = y;
		}

		public bool Equals(StrategicVillageSnapshot other)
		{
			if (other != null && string.Equals(SettlementId, other.SettlementId, StringComparison.Ordinal) && Math.Abs(X - other.X) < 0.001f)
			{
				return Math.Abs(Y - other.Y) < 0.001f;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as StrategicVillageSnapshot);
		}

		public override int GetHashCode()
		{
			return SettlementId.GetHashCode();
		}
	}

	private sealed class StrategicMarkerSnapshot : IEquatable<StrategicMarkerSnapshot>
	{
		internal string SettlementId { get; private set; }

		internal string DisplayName { get; private set; }

		internal float X { get; private set; }

		internal float Y { get; private set; }

		internal float AnchorX { get; private set; }

		internal float AnchorY { get; private set; }

		internal bool IsTown { get; private set; }

		internal bool IsUnderSiege { get; private set; }

		internal uint BesiegerColor { get; private set; }

		internal uint OwnerColor { get; private set; }

		internal StrategicMarkerSnapshot(string settlementId, string displayName, float x, float y, float anchorX, float anchorY, bool isTown, bool isUnderSiege, uint besiegerColor, uint ownerColor)
		{
			SettlementId = settlementId ?? string.Empty;
			DisplayName = displayName ?? string.Empty;
			X = x;
			Y = y;
			AnchorX = anchorX;
			AnchorY = anchorY;
			IsTown = isTown;
			IsUnderSiege = isUnderSiege;
			BesiegerColor = besiegerColor;
			OwnerColor = ownerColor;
		}

		public bool Equals(StrategicMarkerSnapshot other)
		{
			if (other != null && string.Equals(SettlementId, other.SettlementId, StringComparison.Ordinal) && string.Equals(DisplayName, other.DisplayName, StringComparison.Ordinal) && Math.Abs(X - other.X) < 0.001f && Math.Abs(Y - other.Y) < 0.001f && Math.Abs(AnchorX - other.AnchorX) < 0.001f && Math.Abs(AnchorY - other.AnchorY) < 0.001f && IsTown == other.IsTown && IsUnderSiege == other.IsUnderSiege && BesiegerColor == other.BesiegerColor)
			{
				return OwnerColor == other.OwnerColor;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as StrategicMarkerSnapshot);
		}

		public override int GetHashCode()
		{
			return SettlementId.GetHashCode();
		}
	}

	private const int TerritoryCount = 133;

	private const byte SyntheticTerritoryBorder = byte.MaxValue;

	private const uint UnmappedTerritoryColor = 4283060826u;

	private const uint UnmappedLandColor = 4284174663u;

	private static readonly object OwnerColorSync = new object();

	private static readonly object AssetSync = new object();

	private static readonly Dictionary<string, uint> OwnerColorsBySettlementId = new Dictionary<string, uint>(StringComparer.Ordinal);

	private static readonly List<StrategicMarkerSnapshot> SettlementMarkers = new List<StrategicMarkerSnapshot>();

	private static byte[] _baseBgra;

	private static byte[] _atlasTemplateBgra;

	private static byte[] _atlasLabelBgra;

	private static byte[] _territoryIndices;

	private static PointF[] _territoryCenters;

	private static string[] _settlementIds;

	private static int _mapWidth;

	private static int _mapHeight;

	private static int _ownerColorRevision;

	private static bool _assetsLoaded;

	private static bool _assetLoadAttempted;

	private static bool _assetFailureLogged;

	private int _renderedRevision = -1;

	private TaleWorlds.Engine.Texture _engineTexture;

	private TaleWorlds.TwoDimension.Texture _renderTexture;

	internal static void UpdateMapState(IDictionary<string, uint> ownerColorsBySettlementId, IEnumerable<StrategicSettlementPoint> markerPoints)
	{
		if (ownerColorsBySettlementId == null)
		{
			return;
		}
		List<StrategicMarkerSnapshot> incomingMarkers = CreateMarkerSnapshot(markerPoints);
		lock (OwnerColorSync)
		{
			if (HasSameOwnerColors(ownerColorsBySettlementId) && HaveSameMarkers(SettlementMarkers, incomingMarkers))
			{
				return;
			}
			OwnerColorsBySettlementId.Clear();
			foreach (KeyValuePair<string, uint> entry in ownerColorsBySettlementId)
			{
				if (!string.IsNullOrEmpty(entry.Key))
				{
					OwnerColorsBySettlementId[entry.Key] = NormalizeColor(entry.Value);
				}
			}
			SettlementMarkers.Clear();
			SettlementMarkers.AddRange(incomingMarkers);
			_ownerColorRevision++;
		}
	}

	protected override TaleWorlds.TwoDimension.Texture OnGetTextureForRender(TwoDimensionContext context, string name)
	{
		if (!TryEnsureAssets())
		{
			return _renderTexture;
		}
		int revision;
		uint[] ownerColorSnapshot;
		List<StrategicMarkerSnapshot> markerSnapshot;
		lock (OwnerColorSync)
		{
			revision = _ownerColorRevision;
			ownerColorSnapshot = new uint[133];
			for (int index = 0; index < 133; index++)
			{
				if (OwnerColorsBySettlementId.TryGetValue(_settlementIds[index], out var color))
				{
					ownerColorSnapshot[index] = color;
				}
			}
			markerSnapshot = new List<StrategicMarkerSnapshot>(SettlementMarkers);
		}
		if (_renderTexture != null && _renderTexture.IsValid && revision == _renderedRevision)
		{
			return _renderTexture;
		}
		try
		{
			TaleWorlds.Engine.Texture replacement = TaleWorlds.Engine.Texture.CreateFromMemory(BuildMapPng(ownerColorSnapshot, markerSnapshot));
			if (replacement == null || replacement.IsReleased)
			{
				return _renderTexture;
			}
			TaleWorlds.Engine.Texture previous = _engineTexture;
			_engineTexture = replacement;
			_renderTexture = new TaleWorlds.TwoDimension.Texture(new EngineTexture(replacement));
			_renderedRevision = revision;
			Diagnostics.Info("Strategic texture diagnostic: provider=CalendarStrategicCampaignAtlasTextureProvider; request=" + (name ?? "<null>") + "; revision=" + revision + "; png=" + _mapWidth + "x" + _mapHeight + "; markers=" + markerSnapshot.Count + "; engineTextureReleased=" + replacement.IsReleased + ".");
			ReleaseTextureAfterFrameBudget(previous, 3);
			return _renderTexture;
		}
		catch (Exception exception)
		{
			Diagnostics.Error("Strategic map texture composition failed; the previous map texture remains active.", exception);
			return _renderTexture;
		}
	}

	public override void Clear(bool clearNextFrame)
	{
		base.Clear(clearNextFrame);
		_renderTexture = null;
		_renderedRevision = -1;
		ReleaseTextureAfterFrameBudget(_engineTexture, clearNextFrame ? 1 : 0);
		_engineTexture = null;
	}

	private static uint NormalizeColor(uint color)
	{
		if (color != 0 && (color & 0xFF000000u) != 0)
		{
			return color;
		}
		if (color != 0)
		{
			return color | 0xFF000000u;
		}
		return 0u;
	}

	private static bool HasSameOwnerColors(IDictionary<string, uint> incoming)
	{
		if (OwnerColorsBySettlementId.Count != incoming.Count)
		{
			return false;
		}
		foreach (KeyValuePair<string, uint> entry in incoming)
		{
			if (!OwnerColorsBySettlementId.TryGetValue(entry.Key, out var previous) || previous != NormalizeColor(entry.Value))
			{
				return false;
			}
		}
		return true;
	}

	private static List<StrategicMarkerSnapshot> CreateMarkerSnapshot(IEnumerable<StrategicSettlementPoint> points)
	{
		List<StrategicMarkerSnapshot> markers = new List<StrategicMarkerSnapshot>();
		if (points == null)
		{
			return markers;
		}
		foreach (StrategicSettlementPoint point in points)
		{
			if (point != null && point.Settlement != null && !string.IsNullOrEmpty(point.Settlement.StringId))
			{
				markers.Add(new StrategicMarkerSnapshot(point.Settlement.StringId, (point.Settlement.Name == null) ? point.Settlement.StringId : point.Settlement.Name.ToString(), point.DisplayX, point.DisplayY, point.SourceX, point.SourceY, point.Settlement.IsTown, point.IsUnderSiege, (point.Besieger != null) ? NormalizeColor(point.Besieger.Color) : 0u, (point.Owner != null) ? NormalizeColor(point.Owner.Color) : 0u));
			}
		}
		markers.Sort((StrategicMarkerSnapshot left, StrategicMarkerSnapshot right) => string.Compare(left.SettlementId, right.SettlementId, StringComparison.Ordinal));
		return markers;
	}

	private static List<StrategicVillageSnapshot> CreateVillageSnapshot(IEnumerable<StrategicVillagePoint> points)
	{
		List<StrategicVillageSnapshot> villages = new List<StrategicVillageSnapshot>();
		if (points == null)
		{
			return villages;
		}
		foreach (StrategicVillagePoint point in points)
		{
			if (point != null && !string.IsNullOrEmpty(point.SettlementId))
			{
				villages.Add(new StrategicVillageSnapshot(point.SettlementId, point.SourceX, point.SourceY));
			}
		}
		villages.Sort((StrategicVillageSnapshot left, StrategicVillageSnapshot right) => string.Compare(left.SettlementId, right.SettlementId, StringComparison.Ordinal));
		return villages;
	}

	private static bool HaveSameMarkers(IList<StrategicMarkerSnapshot> left, IList<StrategicMarkerSnapshot> right)
	{
		if (left == null || right == null || left.Count != right.Count)
		{
			return false;
		}
		for (int index = 0; index < left.Count; index++)
		{
			if (!left[index].Equals(right[index]))
			{
				return false;
			}
		}
		return true;
	}

	private static bool HaveSameVillages(IList<StrategicVillageSnapshot> left, IList<StrategicVillageSnapshot> right)
	{
		if (left == null || right == null || left.Count != right.Count)
		{
			return false;
		}
		for (int index = 0; index < left.Count; index++)
		{
			if (!left[index].Equals(right[index]))
			{
				return false;
			}
		}
		return true;
	}

	private static bool TryEnsureAssets()
	{
		lock (AssetSync)
		{
			if (_assetsLoaded)
			{
				return true;
			}
			if (_assetLoadAttempted)
			{
				return false;
			}
			_assetLoadAttempted = true;
			try
			{
				string mapDirectory = System.IO.Path.Combine(GetModuleRoot(), "GUI", "SpriteParts", "ui_world_calendar");
				string baseMapPath = System.IO.Path.Combine(mapDirectory, "strategic_map_atlas.png");
				string customSkinPath = System.IO.Path.Combine(mapDirectory, "custom_map_skin.png");
				string imagePath = System.IO.Path.Combine(mapDirectory, "strategic_province_index.png");
				_atlasTemplateBgra = ReadBgra(baseMapPath, out var baseWidth, out var baseHeight);
				int indexWidth;
				int indexHeight;
				byte[] indexBgra = ReadBgra(imagePath, out indexWidth, out indexHeight);
				if (baseWidth != indexWidth || baseHeight != indexHeight)
				{
					throw new InvalidOperationException("Strategic-map base and original province-index texture dimensions differ: " + baseWidth + "x" + baseHeight + " versus " + indexWidth + "x" + indexHeight + ".");
				}
				_mapWidth = baseWidth;
				_mapHeight = baseHeight;
				_baseBgra = _atlasTemplateBgra;
				if (File.Exists(customSkinPath))
				{
					byte[] customSkin = ReadScaledBgra(customSkinPath, _mapWidth, _mapHeight);
					_baseBgra = MergeCustomSkin(_atlasTemplateBgra, customSkin);
					Diagnostics.Info("Strategic map loaded custom_map_skin.png.");
				}
				_atlasLabelBgra = ReadBgra(System.IO.Path.Combine(mapDirectory, "strategic_city_labels.png"), out var labelWidth, out var labelHeight);
				if (labelWidth != _mapWidth || labelHeight != _mapHeight)
				{
					throw new InvalidOperationException("Strategic-map atlas labels dimensions differ from the base map.");
				}
				PrepareAtlasLabels(_atlasLabelBgra);
				_settlementIds = BuildSettlementIdsFromProvinceLayout();
				_territoryIndices = new byte[checked(_mapWidth * _mapHeight)];
				int index = 0;
				int offset = 0;
				while (index < _territoryIndices.Length)
				{
					byte territory = indexBgra[offset + 2];
					_territoryIndices[index] = territory;
					if (_baseBgra[offset + 3] == 0)
					{
						if (territory == 0 || territory > 133)
						{
							throw new InvalidOperationException("Strategic-map original province index contains an invalid land id.");
						}
					}
					else if (indexBgra[offset + 3] != 0)
					{
						throw new InvalidOperationException("Strategic-map settlement-territory index overwrites an opaque base-map border or water pixel.");
					}
					index++;
					offset += 4;
				}
				_territoryIndices = ExpandTerritoriesAcrossPaleLand(_territoryIndices, _baseBgra, _mapWidth, _mapHeight);
				_territoryCenters = CalculateTerritoryCenters(_territoryIndices, _mapWidth, _mapHeight);
				_assetsLoaded = true;
				Diagnostics.Info("Strategic map composer loaded a " + _mapWidth + "x" + _mapHeight + " original-province index. Owner colours are now rendered as a single texture.");
				return true;
			}
			catch (Exception exception)
			{
				if (!_assetFailureLogged)
				{
					_assetFailureLogged = true;
					Diagnostics.Error("Strategic map composer could not load its local map assets.", exception);
				}
				return false;
			}
		}
	}

	private static string[] BuildSettlementIdsFromProvinceLayout()
	{
		string[] settlementIds = new string[133];
		for (int ordinal = 1; ordinal <= 133; ordinal++)
		{
			string spriteName = "strategic_province_" + ordinal.ToString("D3");
			if (!CalendarStrategicMapLayout.TryGetSettlementId(spriteName, out var settlementId) || string.IsNullOrEmpty(settlementId))
			{
				throw new InvalidOperationException("Strategic-map original province layout has an invalid settlement binding: " + spriteName + ".");
			}
			settlementIds[ordinal - 1] = settlementId;
		}
		return settlementIds;
	}

	private static string GetModuleRoot()
	{
		string directoryName = System.IO.Path.GetDirectoryName(typeof(CalendarStrategicCampaignAtlasTextureProvider).Assembly.Location);
		if (string.IsNullOrEmpty(directoryName))
		{
			throw new InvalidOperationException("The Strategic Map assembly location is unavailable.");
		}
		return (Directory.GetParent(directoryName)?.Parent ?? throw new InvalidOperationException("The Strategic Map module directory could not be resolved.")).FullName;
	}

	private static PointF[] CalculateTerritoryCenters(byte[] territoryIndices, int width, int height)
	{
		long[] totalX = new long[133];
		long[] totalY = new long[133];
		int[] pixelCounts = new int[133];
		for (int y = 0; y < height; y++)
		{
			int rowOffset = y * width;
			for (int x = 0; x < width; x++)
			{
				byte territory = territoryIndices[rowOffset + x];
				if (territory >= 1 && territory <= 133)
				{
					int index = territory - 1;
					totalX[index] += x;
					totalY[index] += y;
					pixelCounts[index]++;
				}
			}
		}
		PointF[] centers = new PointF[133];
		for (int index2 = 0; index2 < 133; index2++)
		{
			if (pixelCounts[index2] == 0)
			{
				throw new InvalidOperationException("Strategic-map settlement-territory index contains an empty province.");
			}
			centers[index2] = new PointF((float)totalX[index2] / (float)pixelCounts[index2], (float)totalY[index2] / (float)pixelCounts[index2]);
		}
		return centers;
	}

	private static byte[] ExpandTerritoriesAcrossPaleLand(byte[] territoryIndices, byte[] baseBgra, int width, int height)
	{
		byte[] expanded = new byte[territoryIndices.Length];
		Buffer.BlockCopy(territoryIndices, 0, expanded, 0, territoryIndices.Length);
		Queue<int> frontier = new Queue<int>();
		for (int index = 0; index < expanded.Length; index++)
		{
			if (expanded[index] >= 1 && expanded[index] <= 133)
			{
				frontier.Enqueue(index);
			}
		}
		while (frontier.Count > 0)
		{
			int current = frontier.Dequeue();
			byte territory = expanded[current];
			int x = current % width;
			int y = current / width;
			ExpandTerritoryNeighbour(current - 1, x > 0, territory, expanded, baseBgra, frontier);
			ExpandTerritoryNeighbour(current + 1, x < width - 1, territory, expanded, baseBgra, frontier);
			ExpandTerritoryNeighbour(current - width, y > 0, territory, expanded, baseBgra, frontier);
			ExpandTerritoryNeighbour(current + width, y < height - 1, territory, expanded, baseBgra, frontier);
		}
		return expanded;
	}

	private static void ExpandTerritoryNeighbour(int neighbour, bool isInsideMap, byte territory, byte[] expanded, byte[] baseBgra, Queue<int> frontier)
	{
		if (isInsideMap && expanded[neighbour] == 0 && IsPaleLandPixel(baseBgra, neighbour * 4))
		{
			expanded[neighbour] = territory;
			frontier.Enqueue(neighbour);
		}
	}

	private static bool IsPaleLandPixel(byte[] bgra, int offset)
	{
		if (bgra == null || offset < 0 || offset + 3 >= bgra.Length || bgra[offset + 3] == 0)
		{
			return false;
		}
		byte blue = bgra[offset];
		byte green = bgra[offset + 1];
		byte red = bgra[offset + 2];
		if (red >= 115 && green >= 115 && blue >= 110 && Math.Abs(red - green) <= 55)
		{
			return Math.Abs(green - blue) <= 55;
		}
		return false;
	}

	private static byte[] BuildMapPng(uint[] ownerColors, IList<StrategicMarkerSnapshot> markers)
	{
		byte[] composedBgra = new byte[_baseBgra.Length];
		Buffer.BlockCopy(_baseBgra, 0, composedBgra, 0, _baseBgra.Length);
		uint[] resolvedProvinceColors = ResolveProvinceOwnerColors(ownerColors, markers);
		uint[] contestedProvinceColors = ResolveContestedProvinceColors(markers, resolvedProvinceColors);
		uint[] contestedDefenderColors = ResolveContestedDefenderColors(markers, resolvedProvinceColors);
		int pixel = 0;
		int offset = 0;
		while (pixel < _territoryIndices.Length)
		{
			byte territory = _territoryIndices[pixel];
			switch (territory)
			{
			case 0:
				if (IsPaleLandPixel(_baseBgra, offset))
				{
					composedBgra[offset] = 71;
					composedBgra[offset + 1] = 81;
					composedBgra[offset + 2] = 91;
					composedBgra[offset + 3] = byte.MaxValue;
				}
				break;
			case byte.MaxValue:
				composedBgra[offset] = 9;
				composedBgra[offset + 1] = 12;
				composedBgra[offset + 2] = 16;
				composedBgra[offset + 3] = byte.MaxValue;
				break;
			default:
			{
				uint ownerColor = resolvedProvinceColors[territory - 1];
				if (ownerColor == 0)
				{
					ownerColor = 4283060826u;
				}
				uint contestedColor = contestedProvinceColors[territory - 1];
				if (contestedColor != 0)
				{
					uint defenderColor = contestedDefenderColors[territory - 1];
					if (defenderColor != 0)
					{
						ownerColor = defenderColor;
					}
					int num = pixel % _mapWidth;
					int y = pixel / _mapWidth;
					ownerColor = (((num + y) % 24 < 7) ? contestedColor : ownerColor);
				}
				ownerColor = ApplyAtlasShading(ownerColor, pixel % _mapWidth, pixel / _mapWidth);
				composedBgra[offset] = (byte)(ownerColor & 0xFFu);
				composedBgra[offset + 1] = (byte)((ownerColor >> 8) & 0xFFu);
				composedBgra[offset + 2] = (byte)((ownerColor >> 16) & 0xFFu);
				composedBgra[offset + 3] = (byte)((ownerColor >> 24) & 0xFFu);
				break;
			}
			}
			pixel++;
			offset += 4;
		}
		using Bitmap composedMap = new Bitmap(_mapWidth, _mapHeight, PixelFormat.Format32bppArgb);
		CopyBgraToBitmap(composedBgra, composedMap);
		DrawAtlasLabels(composedMap);
		DrawSettlementMarkers(composedMap, markers);
		using MemoryStream stream = new MemoryStream();
		composedMap.Save(stream, ImageFormat.Png);
		return stream.ToArray();
	}

	private static uint[] ResolveProvinceOwnerColors(uint[] ownerColors, IList<StrategicMarkerSnapshot> markers)
	{
		uint[] resolved = new uint[133];
		if (ownerColors != null)
		{
			Array.Copy(ownerColors, resolved, Math.Min(ownerColors.Length, resolved.Length));
		}
		if (markers != null)
		{
			HashSet<int> townTerritories = new HashSet<int>();
			foreach (StrategicMarkerSnapshot marker2 in markers)
			{
				if (marker2 != null && marker2.IsTown && marker2.OwnerColor != 0)
				{
					int territory2 = FindTerritoryAtMarkerAnchor(marker2.AnchorX, marker2.AnchorY);
					if (territory2 > 0)
					{
						resolved[territory2 - 1] = marker2.OwnerColor;
						townTerritories.Add(territory2);
					}
				}
			}
			{
				foreach (StrategicMarkerSnapshot marker in markers)
				{
					if (marker != null && !marker.IsTown && marker.OwnerColor != 0)
					{
						int territory = FindTerritoryAtMarkerAnchor(marker.AnchorX, marker.AnchorY);
						if (territory > 0 && !townTerritories.Contains(territory))
						{
							resolved[territory - 1] = marker.OwnerColor;
						}
					}
				}
				return resolved;
			}
		}
		return resolved;
	}

	private static uint[] ResolveContestedDefenderColors(IList<StrategicMarkerSnapshot> markers, uint[] provinceOwnerColors)
	{
		uint[] resolved = new uint[133];
		if (markers == null || markers.Count == 0)
		{
			return resolved;
		}
		foreach (StrategicMarkerSnapshot marker4 in markers)
		{
			if (marker4 == null || marker4.IsTown || marker4.OwnerColor == 0)
			{
				continue;
			}
			int territory2 = FindTerritoryAtMarkerAnchor(marker4.AnchorX, marker4.AnchorY);
			if (territory2 <= 0)
			{
				territory2 = FindAuthoredTerritory(marker4.SettlementId);
			}
			if (territory2 > 0 && provinceOwnerColors != null && territory2 <= provinceOwnerColors.Length)
			{
				uint provinceOwnerColor = provinceOwnerColors[territory2 - 1];
				if (provinceOwnerColor != 0 && provinceOwnerColor != marker4.OwnerColor)
				{
					resolved[territory2 - 1] = provinceOwnerColor;
				}
			}
		}
		HashSet<string> dynamicallyBoundSieges = new HashSet<string>(StringComparer.Ordinal);
		foreach (StrategicMarkerSnapshot marker3 in markers)
		{
			if (marker3 == null || !marker3.IsUnderSiege || marker3.BesiegerColor == 0)
			{
				continue;
			}
			int territory = FindTerritoryAtMarkerAnchor(marker3.AnchorX, marker3.AnchorY);
			if (territory > 0)
			{
				resolved[territory - 1] = marker3.OwnerColor;
				if (!string.IsNullOrEmpty(marker3.SettlementId))
				{
					dynamicallyBoundSieges.Add(marker3.SettlementId);
				}
			}
		}
		Dictionary<string, StrategicMarkerSnapshot> bySettlementId = new Dictionary<string, StrategicMarkerSnapshot>(StringComparer.Ordinal);
		foreach (StrategicMarkerSnapshot marker2 in markers)
		{
			if (marker2 != null && !string.IsNullOrEmpty(marker2.SettlementId))
			{
				bySettlementId[marker2.SettlementId] = marker2;
			}
		}
		HashSet<string> fallbackBoundSieges = new HashSet<string>(StringComparer.Ordinal);
		for (int province = 0; province < 133; province++)
		{
			if (bySettlementId.TryGetValue(_settlementIds[province], out var marker) && marker.IsUnderSiege && marker.BesiegerColor != 0 && !dynamicallyBoundSieges.Contains(marker.SettlementId) && !fallbackBoundSieges.Contains(marker.SettlementId))
			{
				resolved[province] = marker.OwnerColor;
				fallbackBoundSieges.Add(marker.SettlementId);
			}
		}
		return resolved;
	}

	private static bool TryResolveUnmappedBlackLandColor(int pixel, IList<StrategicMarkerSnapshot> markers, out uint color)
	{
		color = 0u;
		if (markers == null || markers.Count == 0 || _baseBgra == null)
		{
			return false;
		}
		int offset = pixel * 4;
		if (offset + 3 >= _baseBgra.Length || _baseBgra[offset + 3] == 0)
		{
			return false;
		}
		byte blue = _baseBgra[offset];
		byte green = _baseBgra[offset + 1];
		if (_baseBgra[offset + 2] > 38 || green > 38 || blue > 38)
		{
			return false;
		}
		int x = pixel % _mapWidth;
		int y = pixel / _mapWidth;
		if (x < 4 || y < 4 || x >= _mapWidth - 4 || y >= _mapHeight - 4)
		{
			return false;
		}
		bool waterNearby = false;
		int firstTerritory = 0;
		for (int sampleY = y - 3; sampleY <= y + 3; sampleY++)
		{
			for (int sampleX = x - 3; sampleX <= x + 3; sampleX++)
			{
				int sampleOffset = (sampleY * _mapWidth + sampleX) * 4;
				if (_baseBgra[sampleOffset] > _baseBgra[sampleOffset + 2] + 24)
				{
					waterNearby = true;
				}
				byte territory = _territoryIndices[sampleY * _mapWidth + sampleX];
				if (territory >= 1 && territory <= 133)
				{
					if (firstTerritory == 0)
					{
						firstTerritory = territory;
					}
					else if (firstTerritory != territory)
					{
						return false;
					}
				}
			}
		}
		if (waterNearby)
		{
			return false;
		}
		StrategicMarkerSnapshot nearest = null;
		float bestDistance = float.MaxValue;
		foreach (StrategicMarkerSnapshot marker in markers)
		{
			if (marker != null && marker.OwnerColor != 0)
			{
				float num = marker.AnchorX - (float)x;
				float deltaY = marker.AnchorY - (float)y;
				float distance = num * num + deltaY * deltaY;
				if (distance < bestDistance)
				{
					bestDistance = distance;
					nearest = marker;
				}
			}
		}
		if (nearest == null)
		{
			return false;
		}
		color = nearest.OwnerColor;
		return true;
	}

	private static uint[] ResolveContestedProvinceColors(IList<StrategicMarkerSnapshot> markers, uint[] provinceOwnerColors)
	{
		uint[] resolved = new uint[133];
		if (markers == null || markers.Count == 0)
		{
			return resolved;
		}
		foreach (StrategicMarkerSnapshot marker4 in markers)
		{
			if (marker4 == null || marker4.IsTown || marker4.OwnerColor == 0)
			{
				continue;
			}
			int territory2 = FindTerritoryAtMarkerAnchor(marker4.AnchorX, marker4.AnchorY);
			if (territory2 <= 0)
			{
				territory2 = FindAuthoredTerritory(marker4.SettlementId);
			}
			if (territory2 > 0 && provinceOwnerColors != null && territory2 <= provinceOwnerColors.Length)
			{
				uint provinceOwnerColor = provinceOwnerColors[territory2 - 1];
				if (provinceOwnerColor != 0 && provinceOwnerColor != marker4.OwnerColor)
				{
					resolved[territory2 - 1] = marker4.OwnerColor;
				}
			}
		}
		HashSet<string> dynamicallyBoundSieges = new HashSet<string>(StringComparer.Ordinal);
		foreach (StrategicMarkerSnapshot marker3 in markers)
		{
			if (marker3 == null || !marker3.IsUnderSiege || marker3.BesiegerColor == 0)
			{
				continue;
			}
			int territory = FindTerritoryAtMarkerAnchor(marker3.AnchorX, marker3.AnchorY);
			if (territory > 0)
			{
				resolved[territory - 1] = marker3.BesiegerColor;
				if (!string.IsNullOrEmpty(marker3.SettlementId))
				{
					dynamicallyBoundSieges.Add(marker3.SettlementId);
				}
			}
		}
		Dictionary<string, StrategicMarkerSnapshot> bySettlementId = new Dictionary<string, StrategicMarkerSnapshot>(StringComparer.Ordinal);
		foreach (StrategicMarkerSnapshot marker2 in markers)
		{
			if (marker2 != null && !string.IsNullOrEmpty(marker2.SettlementId))
			{
				bySettlementId[marker2.SettlementId] = marker2;
			}
		}
		HashSet<string> fallbackBoundSieges = new HashSet<string>(StringComparer.Ordinal);
		for (int province = 0; province < 133; province++)
		{
			if (bySettlementId.TryGetValue(_settlementIds[province], out var marker) && marker.IsUnderSiege && marker.BesiegerColor != 0 && !dynamicallyBoundSieges.Contains(marker.SettlementId) && !fallbackBoundSieges.Contains(marker.SettlementId))
			{
				resolved[province] = marker.BesiegerColor;
				fallbackBoundSieges.Add(marker.SettlementId);
			}
		}
		return resolved;
	}

	private static int FindAuthoredTerritory(string settlementId)
	{
		if (string.IsNullOrEmpty(settlementId) || _settlementIds == null)
		{
			return 0;
		}
		for (int index = 0; index < _settlementIds.Length; index++)
		{
			if (string.Equals(_settlementIds[index], settlementId, StringComparison.Ordinal))
			{
				return index + 1;
			}
		}
		return 0;
	}

	private static int FindTerritoryAtMarkerAnchor(float anchorX, float anchorY)
	{
		if (_territoryIndices == null || _mapWidth <= 0 || _mapHeight <= 0)
		{
			return 0;
		}
		int centerX = Math.Max(0, Math.Min(_mapWidth - 1, (int)Math.Round(anchorX)));
		int centerY = Math.Max(0, Math.Min(_mapHeight - 1, (int)Math.Round(anchorY)));
		double[] scores = new double[133];
		for (int y = Math.Max(0, centerY - 18); y <= Math.Min(_mapHeight - 1, centerY + 18); y++)
		{
			int deltaY = y - centerY;
			for (int x = Math.Max(0, centerX - 18); x <= Math.Min(_mapWidth - 1, centerX + 18); x++)
			{
				int num = x - centerX;
				int distanceSquared = num * num + deltaY * deltaY;
				if (distanceSquared <= 324)
				{
					byte territory = _territoryIndices[y * _mapWidth + x];
					if (territory >= 1 && territory <= 133)
					{
						scores[territory - 1] += 1.0 / (1.0 + (double)distanceSquared);
					}
				}
			}
		}
		int bestTerritory = 0;
		double bestScore = 0.0;
		for (int index = 0; index < scores.Length; index++)
		{
			if (!(scores[index] <= bestScore))
			{
				bestScore = scores[index];
				bestTerritory = index + 1;
			}
		}
		return bestTerritory;
	}

	private static uint ApplyAtlasShading(uint color, int x, int y)
	{
		double shade = 0.91 + 0.055 * Math.Sin((double)x * 0.034 + (double)y * 0.012) + 0.035 * Math.Sin((double)y * 0.049 - (double)x * 0.009);
		int red = Math.Max(0, Math.Min(255, (int)((double)((color >> 16) & 0xFFu) * shade)));
		int green = Math.Max(0, Math.Min(255, (int)((double)((color >> 8) & 0xFFu) * shade)));
		int blue = Math.Max(0, Math.Min(255, (int)((double)(color & 0xFFu) * shade)));
		return (color & 0xFF000000u) | (uint)(red << 16) | (uint)(green << 8) | (uint)blue;
	}

	private static void PrepareAtlasLabels(byte[] labels)
	{
		if (labels == null)
		{
			return;
		}
		for (int offset = 0; offset < labels.Length; offset += 4)
		{
			byte blue = labels[offset];
			byte green = labels[offset + 1];
			byte red = labels[offset + 2];
			if (labels[offset + 3] != 0 && !((float)(int)red <= (float)(int)green * 1.35f) && !((float)(int)red <= (float)(int)blue * 1.35f))
			{
				labels[offset + 3] = 0;
			}
		}
	}

	private static void DrawAtlasLabels(Bitmap map)
	{
		if (map == null || _atlasLabelBgra == null)
		{
			return;
		}
		using Bitmap labels = new Bitmap(_mapWidth, _mapHeight, PixelFormat.Format32bppArgb);
		using Graphics graphics = Graphics.FromImage(map);
		CopyBgraToBitmap(_atlasLabelBgra, labels);
		graphics.CompositingMode = CompositingMode.SourceOver;
		graphics.DrawImageUnscaled(labels, 0, 0);
	}

	private static void DrawVillageMarkers(Bitmap map, IList<StrategicVillageSnapshot> villages)
	{
		if (map == null || villages == null || villages.Count == 0)
		{
			return;
		}
		using Graphics graphics = Graphics.FromImage(map);
		using SolidBrush villageDot = new SolidBrush(Color.FromArgb(255, 18, 15, 12));
		using Pen villageEdge = new Pen(Color.FromArgb(220, 210, 176, 100), 0.8f);
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		foreach (StrategicVillageSnapshot village in villages)
		{
			int x = (int)Math.Round(village.X);
			int y = (int)Math.Round(village.Y);
			Rectangle dot = new Rectangle(x - 2, y - 2, 4, 4);
			graphics.FillEllipse(villageDot, dot);
			graphics.DrawEllipse(villageEdge, dot);
		}
	}

	private static void DrawSettlementMarkers(Bitmap map, IList<StrategicMarkerSnapshot> markers)
	{
		if (map == null || markers == null || markers.Count == 0)
		{
			return;
		}
		using Graphics graphics = Graphics.FromImage(map);
		using SolidBrush markerFill = new SolidBrush(Color.FromArgb(255, 183, 136, 68));
		using SolidBrush markerHighlight = new SolidBrush(Color.FromArgb(255, 222, 180, 99));
		using SolidBrush markerDetail = new SolidBrush(Color.FromArgb(255, 49, 32, 19));
		using Pen markerOutline = new Pen(Color.FromArgb(255, 24, 17, 12), 2.5f);
		using SolidBrush siegeFill = new SolidBrush(Color.FromArgb(255, 185, 54, 43));
		using Pen siegeOutline = new Pen(Color.FromArgb(255, 58, 15, 12), 1.8f);
		using Pen siegeGlyph = new Pen(Color.FromArgb(255, 247, 221, 170), 1.8f);
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
		foreach (StrategicMarkerSnapshot marker in markers)
		{
			int centerX = (int)Math.Round(marker.X);
			int centerY = (int)Math.Round(marker.Y);
			if (marker.IsTown)
			{
				DrawTownMarker(graphics, centerX, centerY, markerFill, markerHighlight, markerDetail, markerOutline);
			}
			else
			{
				DrawCastleMarker(graphics, centerX, centerY, markerFill, markerHighlight, markerDetail, markerOutline);
			}
			if (marker.IsUnderSiege)
			{
				DrawSiegeBadge(graphics, centerX, centerY, siegeFill, siegeOutline, siegeGlyph);
			}
		}
	}

	private static void DrawTownLabels(Bitmap map, IList<StrategicMarkerSnapshot> markers)
	{
		if (map == null || markers == null || markers.Count == 0 || !CalendarSettingsState.StrategicMapShowSettlementLabels)
		{
			return;
		}
		List<StrategicMarkerSnapshot> orderedMarkers = new List<StrategicMarkerSnapshot>();
		foreach (StrategicMarkerSnapshot marker2 in markers)
		{
			if (marker2 != null && marker2.IsTown && !string.IsNullOrEmpty(marker2.DisplayName))
			{
				orderedMarkers.Add(marker2);
			}
		}
		orderedMarkers.Sort(delegate(StrategicMarkerSnapshot left, StrategicMarkerSnapshot right)
		{
			int num = left.DisplayName.Length.CompareTo(right.DisplayName.Length);
			return (num == 0) ? string.Compare(left.SettlementId, right.SettlementId, StringComparison.Ordinal) : num;
		});
		using Graphics graphics = Graphics.FromImage(map);
		using System.Drawing.Font font = new System.Drawing.Font(FontFamily.GenericSerif, CalendarSettingsState.StrategicMapLabelFontSize, System.Drawing.FontStyle.Bold, GraphicsUnit.Pixel);
		using SolidBrush shadow = new SolidBrush(Color.FromArgb(230, 20, 18, 15));
		using SolidBrush text = new SolidBrush(Color.FromArgb(255, 244, 232, 204));
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
		List<RectangleF> occupiedLabels = new List<RectangleF>();
		foreach (StrategicMarkerSnapshot marker in orderedMarkers)
		{
			SizeF size = graphics.MeasureString(marker.DisplayName, font);
			PointF[] offsets = new PointF[4]
			{
				new PointF(0f - size.Width / 2f, -34f),
				new PointF(0f - size.Width / 2f, 18f),
				new PointF(20f, 0f - size.Height / 2f),
				new PointF(-20f - size.Width, 0f - size.Height / 2f)
			};
			RectangleF chosen = RectangleF.Empty;
			for (int index = 0; index < offsets.Length; index++)
			{
				RectangleF candidate = new RectangleF(marker.X + offsets[index].X, marker.Y + offsets[index].Y, size.Width, size.Height);
				if (candidate.Left < 2f || candidate.Top < 2f || candidate.Right > (float)map.Width - 2f || candidate.Bottom > (float)map.Height - 2f)
				{
					continue;
				}
				bool overlaps = false;
				foreach (RectangleF occupied in occupiedLabels)
				{
					if (candidate.IntersectsWith(occupied))
					{
						overlaps = true;
						break;
					}
				}
				if (!overlaps)
				{
					chosen = candidate;
					break;
				}
			}
			if (!chosen.IsEmpty)
			{
				occupiedLabels.Add(chosen);
				graphics.DrawString(marker.DisplayName, font, shadow, chosen.X + 1f, chosen.Y + 1f);
				graphics.DrawString(marker.DisplayName, font, text, chosen.X, chosen.Y);
			}
		}
	}

	private static void DrawSiegeBadge(Graphics graphics, int centerX, int centerY, System.Drawing.Brush fill, Pen outline, Pen glyph)
	{
		Point[] badge = new Point[4]
		{
			new Point(centerX + 12, centerY - 20),
			new Point(centerX + 20, centerY - 12),
			new Point(centerX + 12, centerY - 4),
			new Point(centerX + 4, centerY - 12)
		};
		graphics.FillPolygon(fill, badge);
		graphics.DrawPolygon(outline, badge);
		graphics.DrawLine(glyph, centerX + 8, centerY - 16, centerX + 16, centerY - 8);
		graphics.DrawLine(glyph, centerX + 16, centerY - 16, centerX + 8, centerY - 8);
	}

	private static void DrawTownMarker(Graphics graphics, int centerX, int centerY, System.Drawing.Brush fill, System.Drawing.Brush highlight, System.Drawing.Brush detail, Pen outline)
	{
		Rectangle tallBody = new Rectangle(centerX - 15, centerY - 2, 15, 18);
		Point[] tallRoof = new Point[3]
		{
			new Point(centerX - 17, centerY - 2),
			new Point(centerX - 8, centerY - 19),
			new Point(centerX + 2, centerY - 2)
		};
		graphics.FillRectangle(fill, tallBody);
		graphics.FillPolygon(fill, tallRoof);
		graphics.DrawRectangle(outline, tallBody);
		graphics.DrawPolygon(outline, tallRoof);
		graphics.FillRectangle(highlight, centerX - 13, centerY, 3, 13);
		graphics.FillRectangle(detail, centerX - 10, centerY + 8, 4, 8);
		Rectangle smallBody = new Rectangle(centerX + 1, centerY + 3, 15, 13);
		Point[] smallRoof = new Point[3]
		{
			new Point(centerX - 1, centerY + 3),
			new Point(centerX + 8, centerY - 11),
			new Point(centerX + 18, centerY + 3)
		};
		graphics.FillRectangle(fill, smallBody);
		graphics.FillPolygon(fill, smallRoof);
		graphics.DrawRectangle(outline, smallBody);
		graphics.DrawPolygon(outline, smallRoof);
		graphics.FillRectangle(highlight, centerX + 3, centerY + 5, 3, 9);
		graphics.FillRectangle(detail, centerX + 9, centerY + 9, 4, 7);
	}

	private static void DrawCastleMarker(Graphics graphics, int centerX, int centerY, System.Drawing.Brush fill, System.Drawing.Brush highlight, System.Drawing.Brush detail, Pen outline)
	{
		Point[] wall = new Point[12]
		{
			new Point(centerX - 16, centerY - 14),
			new Point(centerX - 10, centerY - 14),
			new Point(centerX - 10, centerY - 8),
			new Point(centerX - 4, centerY - 8),
			new Point(centerX - 4, centerY - 14),
			new Point(centerX + 2, centerY - 14),
			new Point(centerX + 2, centerY - 8),
			new Point(centerX + 8, centerY - 8),
			new Point(centerX + 8, centerY - 14),
			new Point(centerX + 15, centerY - 14),
			new Point(centerX + 15, centerY + 16),
			new Point(centerX - 16, centerY + 16)
		};
		graphics.FillPolygon(fill, wall);
		graphics.DrawPolygon(outline, wall);
		graphics.FillRectangle(highlight, centerX - 13, centerY - 5, 4, 17);
		graphics.FillEllipse(detail, centerX - 5, centerY + 3, 10, 10);
		graphics.FillRectangle(detail, centerX - 5, centerY + 8, 10, 8);
	}

	private static byte[] ReadScaledBgra(string imagePath, int width, int height)
	{
		using Bitmap source = new Bitmap(imagePath);
		using Bitmap scaled = new Bitmap(width, height, PixelFormat.Format32bppArgb);
		using Graphics graphics = Graphics.FromImage(scaled);
		graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
		graphics.DrawImage(source, new Rectangle(0, 0, width, height));
		return ReadBgraFromBitmap(scaled);
	}

	private static byte[] MergeCustomSkin(byte[] template, byte[] custom)
	{
		if (template == null || custom == null || template.Length != custom.Length)
		{
			throw new InvalidOperationException("Custom strategic-map skin does not match the atlas template.");
		}
		byte[] merged = new byte[template.Length];
		for (int offset = 0; offset < template.Length; offset += 4)
		{
			byte templateBlue = template[offset];
			byte templateGreen = template[offset + 1];
			byte templateRed = template[offset + 2];
			byte templateAlpha = template[offset + 3];
			if (templateAlpha == 0)
			{
				merged[offset + 3] = 0;
			}
			else if (templateRed < 28 && templateGreen < 28 && templateBlue < 28)
			{
				merged[offset] = templateBlue;
				merged[offset + 1] = templateGreen;
				merged[offset + 2] = templateRed;
				merged[offset + 3] = templateAlpha;
			}
			else
			{
				merged[offset] = custom[offset];
				merged[offset + 1] = custom[offset + 1];
				merged[offset + 2] = custom[offset + 2];
				merged[offset + 3] = byte.MaxValue;
			}
		}
		return merged;
	}

	private static byte[] ReadBgra(string imagePath, out int width, out int height)
	{
		if (!File.Exists(imagePath))
		{
			throw new FileNotFoundException("Strategic map texture is missing.", imagePath);
		}
		using Bitmap source = new Bitmap(imagePath);
		using Bitmap normalized = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
		using (Graphics graphics = Graphics.FromImage(normalized))
		{
			graphics.DrawImageUnscaled(source, 0, 0);
		}
		width = normalized.Width;
		height = normalized.Height;
		return ReadBgraFromBitmap(normalized);
	}

	private static byte[] ReadBgraFromBitmap(Bitmap bitmap)
	{
		Rectangle rectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
		BitmapData bitmapData = bitmap.LockBits(rectangle, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
		try
		{
			int rowBytes;
			byte[] pixels;
			checked
			{
				rowBytes = bitmap.Width * 4;
				pixels = new byte[rowBytes * bitmap.Height];
			}
			for (int y = 0; y < bitmap.Height; y++)
			{
				Marshal.Copy(IntPtr.Add(bitmapData.Scan0, y * bitmapData.Stride), pixels, y * rowBytes, rowBytes);
			}
			return pixels;
		}
		finally
		{
			bitmap.UnlockBits(bitmapData);
		}
	}

	private static void CopyBgraToBitmap(byte[] pixels, Bitmap bitmap)
	{
		Rectangle rectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
		BitmapData bitmapData = bitmap.LockBits(rectangle, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
		try
		{
			int rowBytes = checked(bitmap.Width * 4);
			for (int y = 0; y < bitmap.Height; y++)
			{
				Marshal.Copy(pixels, y * rowBytes, IntPtr.Add(bitmapData.Scan0, y * bitmapData.Stride), rowBytes);
			}
		}
		finally
		{
			bitmap.UnlockBits(bitmapData);
		}
	}

	private static void ReleaseTextureAfterFrameBudget(TaleWorlds.Engine.Texture texture, int frameBudget)
	{
		if (texture == null || texture.IsReleased)
		{
			return;
		}
		try
		{
			if (frameBudget <= 0)
			{
				texture.Release();
			}
			else
			{
				texture.ReleaseAfterNumberOfFrames(frameBudget);
			}
		}
		catch
		{
		}
	}
}
}

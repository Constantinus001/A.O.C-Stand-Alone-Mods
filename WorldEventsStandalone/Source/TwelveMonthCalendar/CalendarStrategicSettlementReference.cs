using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace TwelveMonthCalendar
{

internal static class CalendarStrategicSettlementReference
{
	private static readonly object Sync = new object();

	private static string _lastSnapshot;

	internal static void CaptureNativeSnapshot()
	{
		lock (Sync)
		{
			try
			{
				List<string> rows = new List<string>();
				foreach (Settlement settlement in Settlement.All)
				{
					if (settlement != null && !string.IsNullOrEmpty(settlement.StringId))
					{
						Vec2 native = settlement.GetPosition2D;
						Vec2 reference = CalendarWorldLedgerVM.ProjectSettlementToReferenceMap(settlement);
						rows.Add(string.Join(",", Csv(settlement.StringId), Csv(GetSettlementKind(settlement)), Csv((settlement.Name == null) ? string.Empty : settlement.Name.ToString()), native.x.ToString("R", CultureInfo.InvariantCulture), native.y.ToString("R", CultureInfo.InvariantCulture), reference.x.ToString("R", CultureInfo.InvariantCulture), reference.y.ToString("R", CultureInfo.InvariantCulture)));
					}
				}
				rows.Sort(StringComparer.Ordinal);
				StringBuilder snapshot = new StringBuilder();
				snapshot.AppendLine("SettlementId,Type,Name,NativeX,NativeY,ReferenceX,ReferenceY");
				foreach (string row in rows)
				{
					snapshot.AppendLine(row);
				}
				string content = snapshot.ToString();
				if (!string.Equals(content, _lastSnapshot, StringComparison.Ordinal))
				{
					string assemblyDirectory = Path.GetDirectoryName(typeof(CalendarStrategicSettlementReference).Assembly.Location);
					string text = Path.Combine(((string.IsNullOrEmpty(assemblyDirectory) ? null : Directory.GetParent(assemblyDirectory))?.Parent ?? throw new InvalidOperationException("The calendar module directory could not be resolved.")).FullName, "ModuleData");
					Directory.CreateDirectory(text);
					string path = Path.Combine(text, "strategic_settlements_native.csv");
					if (File.Exists(path) && string.Equals(File.ReadAllText(path), content, StringComparison.Ordinal))
					{
						_lastSnapshot = content;
						return;
					}
					File.WriteAllText(path, content, Encoding.UTF8);
					_lastSnapshot = content;
					Diagnostics.Info("Wrote independent native strategic settlement reference: " + rows.Count + " settlements.");
				}
			}
			catch (Exception exception)
			{
				Diagnostics.Error("Could not write the independent native strategic settlement reference.", exception);
			}
		}
	}

	private static string GetSettlementKind(Settlement settlement)
	{
		if (settlement == null)
		{
			return "Unknown";
		}
		if (settlement.Village != null)
		{
			return "Village";
		}
		if (settlement.Town != null)
		{
			if (!settlement.IsTown)
			{
				return "Castle";
			}
			return "Town";
		}
		return "Other";
	}

	private static string Csv(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return "\"\"";
		}
		return "\"" + value.Replace("\"", "\"\"") + "\"";
	}
}
}

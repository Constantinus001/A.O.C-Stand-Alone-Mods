using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace TwelveMonthCalendar
{

internal static class StrategicDemographicMapBridge
{
	private const string PopulationServiceTypeName = "AgesOfCalradiaReligions.PopulationService, AgesOfCalradiaReligions";

	private static bool _failureLogged;

	private static bool _serviceResolutionAttempted;

	private static Type _resolvedServiceType;

	internal static int GetRevision()
	{
		try
		{
			Type service = ResolveServiceType();
			PropertyInfo property = ((service == null) ? null : service.GetProperty("StrategicMapRevision", BindingFlags.Static | BindingFlags.Public));
			return (!(property == null)) ? Convert.ToInt32(property.GetValue(null, null), CultureInfo.InvariantCulture) : 0;
		}
		catch (Exception exception)
		{
			LogFailureOnce("Demographic map revision could not be read; World Events will retain its previous map colours.", exception);
			return 0;
		}
	}

	internal static bool TryGetSnapshot(out Dictionary<string, StrategicDemographicProvince> provinces)
	{
		provinces = new Dictionary<string, StrategicDemographicProvince>(StringComparer.Ordinal);
		try
		{
			Type service = ResolveServiceType();
			MethodInfo method = ((service == null) ? null : service.GetMethod("GetStrategicMapSnapshotPayload", BindingFlags.Static | BindingFlags.Public));
			string payload = ((method == null) ? string.Empty : (method.Invoke(null, null) as string));
			if (string.IsNullOrWhiteSpace(payload))
			{
				return false;
			}
			string[] lines = payload.Split(new char[1] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
			if (lines.Length == 0 || !string.Equals(lines[0].Trim(), "AOCMAP1", StringComparison.Ordinal))
			{
				return false;
			}
			for (int index = 1; index < lines.Length; index++)
			{
				string[] fields = lines[index].Split('|');
				if (fields.Length != 5 || !long.TryParse(fields[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var population) || !float.TryParse(fields[4], NumberStyles.Float, CultureInfo.InvariantCulture, out var happiness))
				{
					return false;
				}
				string settlementId = Uri.UnescapeDataString(fields[0]);
				provinces[settlementId] = new StrategicDemographicProvince(settlementId, Math.Max(0L, population), Uri.UnescapeDataString(fields[2]), Uri.UnescapeDataString(fields[3]), Math.Max(0f, Math.Min(100f, happiness)));
			}
			return provinces.Count > 0;
		}
		catch (Exception exception)
		{
			LogFailureOnce("Demographic map snapshot could not be read; Political mode remains available.", exception);
			provinces.Clear();
			return false;
		}
	}

	internal static uint ResolveCultureColor(StrategicDemographicProvince province)
	{
		string culture = ((province == null) ? string.Empty : province.CultureId).ToLowerInvariant();
		string settlement = ((province == null) ? string.Empty : province.SettlementId);
		if (culture.Contains("aserai") || settlement.Contains("_A"))
		{
			return 4291201832u;
		}
		if (culture.Contains("battania") || settlement.Contains("_B"))
		{
			return 4283791922u;
		}
		if (culture.Contains("khuzait") || settlement.Contains("_K"))
		{
			return 4283145105u;
		}
		if (culture.Contains("vlandia") || settlement.Contains("_V"))
		{
			return 4288496698u;
		}
		if (culture.Contains("nord") || settlement.Contains("_N"))
		{
			return 4280568691u;
		}
		if (culture.Contains("sturgia") || settlement.Contains("_S"))
		{
			return 4281954459u;
		}
		if (culture.Contains("empire") || settlement.Contains("_E"))
		{
			return 4286010766u;
		}
		return 4285228637u;
	}

	internal static uint ResolveReligionColor(StrategicDemographicProvince province)
	{
		return ((province == null) ? string.Empty : province.DominantFaithId) switch
		{
			"asharim" => 4288363826u,
			"valeronism" => 4285944715u,
			"mazirism" => 4291066390u,
			"isharan_way" => 4291864623u,
			"kok_orun_way" => 4283079308u,
			"caerwydd" => 4283594290u,
			"veyrhold" => 4280831867u,
			"calradic_old_faith" => 4286932030u,
			_ => 4285097052u,
		};
	}

	internal static uint ResolvePopulationColor(StrategicDemographicProvince province)
	{
		long population = province?.Population ?? 0;
		if (population < 150000)
		{
			return 4280694861u;
		}
		if (population < 300000)
		{
			return 4281819771u;
		}
		if (population < 450000)
		{
			return 4283665288u;
		}
		if (population < 650000)
		{
			return 4287800671u;
		}
		if (population < 900000)
		{
			return 4291928135u;
		}
		return 4292172351u;
	}

	private static void LogFailureOnce(string message, Exception exception)
	{
		if (!_failureLogged)
		{
			_failureLogged = true;
			Diagnostics.Error(message, exception);
		}
	}

	private static Type ResolveServiceType()
	{
		if (_serviceResolutionAttempted)
		{
			return _resolvedServiceType;
		}
		_serviceResolutionAttempted = true;
		_resolvedServiceType = Type.GetType("AgesOfCalradiaReligions.PopulationService, AgesOfCalradiaReligions", throwOnError: false);
		return _resolvedServiceType;
	}
}
}

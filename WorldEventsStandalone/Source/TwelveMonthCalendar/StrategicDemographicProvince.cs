namespace TwelveMonthCalendar
{

internal sealed class StrategicDemographicProvince
{
	internal string SettlementId { get; private set; }

	internal long Population { get; private set; }

	internal string CultureId { get; private set; }

	internal string DominantFaithId { get; private set; }

	internal float Happiness { get; private set; }

	internal StrategicDemographicProvince(string settlementId, long population, string cultureId, string dominantFaithId, float happiness)
	{
		SettlementId = settlementId ?? string.Empty;
		Population = population;
		CultureId = cultureId ?? string.Empty;
		DominantFaithId = dominantFaithId ?? string.Empty;
		Happiness = happiness;
	}
}
}

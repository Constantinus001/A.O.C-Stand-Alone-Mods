using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace TwelveMonthCalendar
{

internal sealed class CalendarKingdomFinanceRowVM : ViewModel
{
	[DataSourceProperty]
	public string Name { get; private set; }

	[DataSourceProperty]
	public string LeaderText { get; private set; }

	[DataSourceProperty]
	public string TreasuryText { get; private set; }

	[DataSourceProperty]
	public string IncomeText { get; private set; }

	[DataSourceProperty]
	public string ExpensesText { get; private set; }

	[DataSourceProperty]
	public string NetText { get; private set; }

	[DataSourceProperty]
	public string NetColor { get; private set; }

	[DataSourceProperty]
	public string FiefsText { get; private set; }

	[DataSourceProperty]
	public bool IsPlayerClan { get; private set; }

	[DataSourceProperty]
	public bool IsEven { get; private set; }

	internal CalendarKingdomFinanceRowVM(Clan clan, int income, int expenses, int net, int index)
	{
		string clanName = ((clan == null || clan.Name == null) ? "Unknown clan" : clan.Name.ToString());
		Name = ((clan == Clan.PlayerClan && string.Equals(clanName, "Playerland", StringComparison.OrdinalIgnoreCase)) ? "Your Clan" : clanName);
		Hero leader = clan?.Leader;
		LeaderText = "Leader: " + ((leader == null || leader.Name == null) ? "Unknown" : leader.Name.ToString());
		TreasuryText = (clan?.Gold ?? 0).ToString("N0") + " denars";
		IncomeText = "+" + income.ToString("N0") + " / day";
		ExpensesText = "-" + expenses.ToString("N0") + " / day";
		NetText = ((net >= 0) ? "+" : string.Empty) + net.ToString("N0") + " / day";
		NetColor = ((net >= 0) ? "#88C98AFF" : "#D27C6FFF");
		FiefsText = (clan?.Fiefs.Count ?? 0).ToString("N0") + " fiefs";
		IsPlayerClan = clan == Clan.PlayerClan;
		IsEven = index % 2 == 0;
	}
}
}

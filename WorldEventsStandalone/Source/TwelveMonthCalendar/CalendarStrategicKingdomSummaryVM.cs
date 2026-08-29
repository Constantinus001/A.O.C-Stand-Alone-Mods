using TaleWorlds.CampaignSystem;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Library;

namespace TwelveMonthCalendar
{

internal sealed class CalendarStrategicKingdomSummaryVM : ViewModel
{
	private readonly BannerImageIdentifierVM _banner;

	[DataSourceProperty]
	public BannerImageIdentifierVM Banner => _banner;

	[DataSourceProperty]
	public string Name { get; private set; }

	[DataSourceProperty]
	public string LeaderText { get; private set; }

	[DataSourceProperty]
	public string RulingClanText { get; private set; }

	[DataSourceProperty]
	public string StrengthText { get; private set; }

	[DataSourceProperty]
	public string HoldingsText { get; private set; }

	[DataSourceProperty]
	public bool IsEven { get; private set; }

	internal CalendarStrategicKingdomSummaryVM(Kingdom kingdom, long wealth, int fieldedTroops, int clanCount, int townCount, int castleCount, int index)
	{
		_banner = new BannerImageIdentifierVM(kingdom?.Banner, nineGrid: true);
		Name = ((kingdom == null || kingdom.Name == null) ? "Unknown kingdom" : kingdom.Name.ToString());
		Hero leader = kingdom?.Leader;
		Clan rulingClan = kingdom?.RulingClan;
		LeaderText = "Leader: " + ((leader == null || leader.Name == null) ? "Unknown" : leader.Name.ToString());
		RulingClanText = "Ruling clan: " + ((rulingClan == null || rulingClan.Name == null) ? "Unknown" : rulingClan.Name.ToString());
		StrengthText = "Wealth " + wealth.ToString("N0") + "  •  Fielded " + fieldedTroops.ToString("N0");
		HoldingsText = "Clans " + clanCount.ToString("N0") + "  •  Towns " + townCount.ToString("N0") + "  •  Castles " + castleCount.ToString("N0");
		IsEven = index % 2 == 0;
	}
}
}

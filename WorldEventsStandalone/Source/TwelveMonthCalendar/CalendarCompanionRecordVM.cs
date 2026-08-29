using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Library;

namespace TwelveMonthCalendar
{

internal sealed class CalendarCompanionRecordVM : ViewModel
{
	[DataSourceProperty]
	public string Name { get; private set; }

	[DataSourceProperty]
	public string Subtitle { get; private set; }

	[DataSourceProperty]
	public string BackgroundText { get; private set; }

	[DataSourceProperty]
	public string KillsText { get; private set; }

	[DataSourceProperty]
	public string KnockoutsText { get; private set; }

	[DataSourceProperty]
	public CharacterImageIdentifierVM Portrait { get; private set; }

	[DataSourceProperty]
	public bool IsEven { get; private set; }

	internal CalendarCompanionRecordVM(Hero hero, int index)
	{
		string culture = ((hero == null || hero.Culture == null || hero.Culture.Name == null) ? "Unknown culture" : hero.Culture.Name.ToString());
		Name = ((hero == null || hero.Name == null) ? "UNNAMED COMPANION" : hero.Name.ToString().ToUpperInvariant());
		Subtitle = culture + "  •  Age " + ((hero == null) ? "?" : hero.Age.ToString("0"));
		string background = ((hero == null || hero.EncyclopediaText == null) ? string.Empty : hero.EncyclopediaText.ToString());
		BackgroundText = (string.IsNullOrWhiteSpace(background) ? "No encyclopedia backstory is available for this companion." : background.Trim());
		CalendarWorldLedgerBehavior.GetCombatStatistics(hero, out var kills, out var knockouts);
		KillsText = kills.ToString("N0");
		KnockoutsText = knockouts.ToString("N0");
		Portrait = ((hero == null || hero.CharacterObject == null) ? new CharacterImageIdentifierVM(null) : new CharacterImageIdentifierVM(CharacterCode.CreateFrom(hero.CharacterObject)));
		IsEven = index % 2 == 0;
	}
}
}

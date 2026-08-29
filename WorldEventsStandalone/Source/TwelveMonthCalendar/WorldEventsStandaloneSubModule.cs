using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;

namespace TwelveMonthCalendar
{

public sealed class WorldEventsStandaloneSubModule : MBSubModuleBase
{
	private bool _campaignCalendarBasisInitialized;

	protected override void OnSubModuleLoad()
	{
		base.OnSubModuleLoad();
		Diagnostics.Initialize();
		CalendarSettingsState.Load();
		Diagnostics.Info("World Events Standalone loaded. Press F10 on the campaign map to open it.");
	}

	protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
	{
		base.OnGameStart(game, gameStarterObject);
		CampaignGameStarter campaignStarter = gameStarterObject as CampaignGameStarter;
		if (game.GameType is Campaign && campaignStarter != null)
		{
			_campaignCalendarBasisInitialized = false;
			campaignStarter.AddBehavior(new CalendarWorldLedgerBehavior());
			Diagnostics.Info("World Events ledger behavior registered for the campaign.");
		}
	}

	public override void OnMissionBehaviorInitialize(Mission mission)
	{
		base.OnMissionBehaviorInitialize(mission);
		if (mission != null && mission.GetMissionBehavior<CalendarHeroCombatStatisticsMissionBehavior>() == null)
		{
			mission.AddMissionBehavior(new CalendarHeroCombatStatisticsMissionBehavior());
		}
	}

	protected override void OnBeforeInitialModuleScreenSetAsRoot()
	{
		base.OnBeforeInitialModuleScreenSetAsRoot();
		try
		{
			TextureProviderFactory.RefreshProviderTypes();
			Diagnostics.Info("World Events texture providers registered with Gauntlet.");
		}
		catch (Exception exception)
		{
			Diagnostics.Error("Gauntlet could not register the World Events texture providers; the game will continue without opening the overlay.", exception);
		}
	}

	protected override void OnApplicationTick(float dt)
	{
		base.OnApplicationTick(dt);
		if (Campaign.Current == null)
		{
			StandaloneWorldEventsScreen.CloseIfOpen();
			_campaignCalendarBasisInitialized = false;
			return;
		}
		EnsureCampaignCalendarBasis();
		if (Input.IsKeyReleased(InputKey.F10))
		{
			StandaloneWorldEventsScreen.Toggle();
		}
	}

	private void EnsureCampaignCalendarBasis()
	{
		if (!_campaignCalendarBasisInitialized)
		{
			_campaignCalendarBasisInitialized = true;
			CalendarSettingsState.MarkModernSaveAgeCompatibility();
			Diagnostics.Info("World Events is using Bannerlord's native 84-day calendar: four 21-day seasons per year.");
		}
	}
}
}

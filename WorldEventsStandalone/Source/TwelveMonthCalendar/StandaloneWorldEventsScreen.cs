using System;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.ScreenSystem;

namespace TwelveMonthCalendar
{

internal sealed class StandaloneWorldEventsScreen : GlobalLayer
{
	private static StandaloneWorldEventsScreen _active;

	private readonly GauntletLayer _layer;

	private readonly CalendarWorldLedgerVM _dataSource;

	private int _lastOwnershipRevision = -1;

	private int _lastDemographicRevision = -1;

	private float _friendlyArmyRefreshElapsed;

	private StandaloneWorldEventsScreen()
	{
		_layer = new GauntletLayer("WorldEventsStandalone", 240);
		_dataSource = new CalendarWorldLedgerVM(Close);
		_layer.LoadMovie("WorldEventsStandalone", _dataSource);
		base.Layer = _layer;
		base.Layer.IsFocusLayer = true;
		base.Layer.InputRestrictions.SetInputRestrictions();
	}

	internal static void Toggle()
	{
		if (_active != null)
		{
			_active.Close();
			return;
		}
		try
		{
			ScreenManager.AddGlobalLayer(_active = new StandaloneWorldEventsScreen(), isFocusable: true);
			Diagnostics.Info("World Events opened with refreshed campaign data.");
		}
		catch (Exception exception)
		{
			_active = null;
			Diagnostics.Error("World Events could not be initialized; the campaign will continue normally.", exception);
		}
	}

	internal static void CloseIfOpen()
	{
		if (_active != null)
		{
			_active.Close();
		}
	}

	protected override void OnTick(float dt)
	{
		base.OnTick(dt);
		if (_layer.Input.IsHotKeyReleased("Exit") || _layer.Input.IsKeyReleased(InputKey.Escape))
		{
			Close();
			return;
		}
		if (_dataSource.IsStrategicMap && Input.IsMouseScrollChanged)
		{
			_dataSource.AdjustStrategicMapZoomFromMouseWheel(Input.DeltaMouseScroll);
		}
		if (_dataSource.IsStrategicMap)
		{
			if (Input.IsKeyPressed(InputKey.D1))
			{
				_dataSource.ExecuteShowPoliticalMapMode();
			}
			else if (Input.IsKeyPressed(InputKey.D2))
			{
				_dataSource.ExecuteShowCultureMapMode();
			}
			else if (Input.IsKeyPressed(InputKey.D3))
			{
				_dataSource.ExecuteShowReligionMapMode();
			}
			else if (Input.IsKeyPressed(InputKey.D4))
			{
				_dataSource.ExecuteShowPopulationMapMode();
			}
			_friendlyArmyRefreshElapsed += dt;
			if (_friendlyArmyRefreshElapsed >= 1f)
			{
				_friendlyArmyRefreshElapsed = 0f;
				_dataSource.RefreshStrategicFriendlyArmies();
				int demographicRevision = StrategicDemographicMapBridge.GetRevision();
				if (demographicRevision != _lastDemographicRevision)
				{
					_lastDemographicRevision = demographicRevision;
					_dataSource.RefreshStrategicDemographicMap();
				}
			}
		}
		else
		{
			_friendlyArmyRefreshElapsed = 0f;
		}
		int ownershipRevision = CalendarWorldLedgerBehavior.OwnershipRevision;
		if (ownershipRevision != _lastOwnershipRevision)
		{
			_lastOwnershipRevision = ownershipRevision;
			_dataSource.RefreshWorldState();
		}
	}

	private void Close()
	{
		if (_active != this)
		{
			return;
		}
		try
		{
			_dataSource.RefreshWorldState();
		}
		catch (Exception exception)
		{
			Diagnostics.Error("World Events close-time refresh failed; the overlay will still close.", exception);
		}
		finally
		{
			ScreenManager.RemoveGlobalLayer(this);
			_layer.InputRestrictions.ResetInputRestrictions();
			_dataSource.OnFinalize();
			_active = null;
		}
	}
}
}

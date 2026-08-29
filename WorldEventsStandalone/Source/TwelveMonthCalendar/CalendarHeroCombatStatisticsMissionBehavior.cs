using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace TwelveMonthCalendar
{

internal sealed class CalendarHeroCombatStatisticsMissionBehavior : MissionLogic
{
	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
	{
		base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);
		if (affectedAgent != null && affectedAgent.IsHuman && affectorAgent != null && (agentState == AgentState.Killed || agentState == AgentState.Unconscious))
		{
			Hero hero = ((!(affectorAgent.Character is CharacterObject characterObject)) ? null : characterObject.HeroObject);
			if (hero != null && (hero == Hero.MainHero || hero.IsPlayerCompanion))
			{
				CalendarWorldLedgerBehavior.RecordCombatResult(hero, agentState == AgentState.Killed);
			}
		}
	}
}
}

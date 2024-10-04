using SandBox.GameComponents;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Marching;

public class MarchingAgentStatCalculateModel : SandboxAgentStatCalculateModel
{
    private readonly AgentStatCalculateModel _previousModel;
    
    public MarchingAgentStatCalculateModel(AgentStatCalculateModel previousModel)
    {
        _previousModel = previousModel;
    }
    
    public override void InitializeAgentStats(Agent agent, Equipment spawnEquipment, AgentDrivenProperties agentDrivenProperties,
        AgentBuildData agentBuildData)
    {
        _previousModel.InitializeAgentStats(agent, spawnEquipment, agentDrivenProperties, agentBuildData);
        DoMarching(agent);
    }

    public override void UpdateAgentStats(Agent agent, AgentDrivenProperties agentDrivenProperties)
    {
        _previousModel.UpdateAgentStats(agent, agentDrivenProperties);
        DoMarching(agent);
    }

    public static bool IsMarching(Agent agent)
    {
        if (agent.IsMount)
        {
            if (!MarchMissionBehavior.MarchingFormations.Contains(agent.RiderAgent?.Formation))
                return false;
        }
        else if (!MarchMissionBehavior.MarchingFormations.Contains(agent.Formation))
            return false;
        
        return true;
    }
    public static void DoMarching(Agent agent)
    {
        if (!IsMarching(agent))
        {
            return;
        }

        float speed = MarchGlobalConfig.Instance.MarchingSpeed;
        if (!agent.IsMount)
        {
            agent.SetAgentDrivenPropertyValueFromConsole(DrivenProperty.MaxSpeedMultiplier, speed);
            agent.SetAgentDrivenPropertyValueFromConsole(DrivenProperty.CombatMaxSpeedMultiplier, speed);
            agent.UpdateCustomDrivenProperties();
            return;
        }

        agent.SetAgentDrivenPropertyValueFromConsole(DrivenProperty.MountSpeed, speed + 2.25f);
        agent.UpdateCustomDrivenProperties();
    }
}

public class CustomMarchingAgentStatCalculateModel : CustomBattleAgentStatCalculateModel
{
    private readonly AgentStatCalculateModel _previousModel;

    public CustomMarchingAgentStatCalculateModel(AgentStatCalculateModel previousModel)
    {
        _previousModel = previousModel;
    }
    
    public override void InitializeAgentStats(Agent agent, Equipment spawnEquipment, AgentDrivenProperties agentDrivenProperties,
        AgentBuildData agentBuildData)
    {
        _previousModel.InitializeAgentStats(agent, spawnEquipment, agentDrivenProperties, agentBuildData);
        MarchingAgentStatCalculateModel.DoMarching(agent);
    }

    public override void UpdateAgentStats(Agent agent, AgentDrivenProperties agentDrivenProperties)
    {
        _previousModel.UpdateAgentStats(agent, agentDrivenProperties);
        MarchingAgentStatCalculateModel.DoMarching(agent);
    }
}
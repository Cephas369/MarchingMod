using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace Marching;
public class MarchMissionBehavior : MissionBehavior
{
    public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;
    private InputKey _marchKey;
    private OrderController _orderController;
    public static List<Formation> MarchingFormations;

    private void AddMarchingFormation(Formation formation)
    {
        if (!MarchingFormations.Contains(formation))
        {
            MarchingFormations.Add(formation);
        }
    }
    public MarchMissionBehavior()
    {
        MarchingFormations = new();
        _marchKey = (InputKey)Enum.Parse(typeof(InputKey), MarchGlobalConfig.Instance.MarchingHotKey.SelectedValue);
    }

    public override void OnMissionTick(float dt)
    {
        base.OnMissionTick(dt);
        if (_marchKey.IsReleased())
        {
            if (Mission.Current == null || Agent.Main == null || Agent.Main.Health <= 0 || !Agent.Main.Team.IsPlayerGeneral) 
                return;
            
            _orderController = Agent.Main.Team.PlayerOrderController;
            if (!Mission.Current.IsOrderMenuOpen || !_orderController.SelectedFormations.Any())
            {
                MBInformationManager.AddQuickInformation(new TextObject("{=no_formations_selected}No formations selected to march!"));
                return;
            }

            if (_orderController.SelectedFormations.Count == 1)
            {
                if (MarchingFormations.Contains(_orderController.SelectedFormations[0]))
                {
                    TextObject textObject = new TextObject("{=dismiss_single_formation}{FORMATION}, dismiss march!");
                    textObject.SetTextVariable("FORMATION", _orderController.SelectedFormations[0].GetFormationName());
                    MBInformationManager.AddQuickInformation(textObject);
                    MarchingFormations.Remove(_orderController.SelectedFormations[0]);
                }
                else
                {
                    TextObject textObject = new TextObject("{=march_single_formation}{FORMATION}, march!");
                    textObject.SetTextVariable("FORMATION", _orderController.SelectedFormations[0].GetFormationName());
                    MBInformationManager.AddQuickInformation(textObject);
                    AddMarchingFormation(_orderController.SelectedFormations[0]);
                }
            }
            else if (_orderController.SelectedFormations.Count != Mission.PlayerTeam.FormationsIncludingEmpty.Count(f => _orderController.IsFormationSelectable(f)))
            {
                if (!_orderController.SelectedFormations.All(f => MarchingFormations.Contains(f)))
                {
                    TextObject textObject = new TextObject("{=march_multiple_formations}{FORMATIONS}, march!");
                    textObject.SetTextVariable("FORMATIONS", string.Join(", ", _orderController.SelectedFormations.Select(f => f.GetFormationName())));
                
                    MBInformationManager.AddQuickInformation(textObject);
                    foreach (var formation in _orderController.SelectedFormations)
                    {
                        AddMarchingFormation(formation);
                    }
                }
                else
                {
                    TextObject textObject = new TextObject("{=dismiss_multiple_formations}{FORMATIONS}, dismiss march!");
                    textObject.SetTextVariable("FORMATIONS", string.Join(", ", _orderController.SelectedFormations.Select(f => f.GetFormationName())));
                
                    MBInformationManager.AddQuickInformation(textObject);
                    foreach (var formation in _orderController.SelectedFormations)
                    {
                        MarchingFormations.Remove(formation);
                    }
                }
            }
            else if(!MarchingFormations.Any())
            {
                MBInformationManager.AddQuickInformation(new TextObject("{=everyone_march}Everyone! March!"));
                MarchingFormations.Clear();
                foreach (var formation in _orderController.SelectedFormations)
                {
                    AddMarchingFormation(formation);
                }
            }
            else
            {
                MBInformationManager.AddQuickInformation(new TextObject("{everyone_dismiss}Everyone! Dismiss march!"));
                MarchingFormations.Clear();
            }
            
            OnMarch();
        }
    }

    public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, in MissionWeapon affectorWeapon, in Blow blow,
        in AttackCollisionData attackCollisionData)
    {
        base.OnAgentHit(affectedAgent, affectorAgent, in affectorWeapon, in blow, in attackCollisionData);
        if (affectedAgent.IsMainAgent && affectedAgent.Health <= 0)
        {
            MarchingFormations.Clear();
            OnMarch();
        }
    }
    
    private void OnMarch()
    {
        foreach (var agent in Mission.Teams.Player.ActiveAgents)
        {
            agent.UpdateAgentProperties();
            if (agent.HasMount)
            {
                agent.MountAgent.UpdateAgentProperties();
            }
        }
    }
}
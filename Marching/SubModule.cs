using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;


namespace Marching;

[HarmonyPatch(typeof(Agent), "WalkMode", MethodType.Getter)]
public static class WalkModePatch
{
    public static void Postfix(ref bool __result, Agent __instance)
    {
        if (MarchingAgentStatCalculateModel.IsMarching(__instance))
        {
            __result = true;
        }
    }
}
public class SubModule : MBSubModuleBase
{
    protected override void OnSubModuleLoad()
    {
        base.OnSubModuleLoad();
    }

    protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
    {
        base.OnGameStart(game, gameStarterObject);
        
        if (MarchGlobalConfig.Instance.ArtemisSupport == true)
        {
            new Harmony("com.marching").PatchAll();
        }
        
        if (gameStarterObject is CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddModel(new MarchingAgentStatCalculateModel(campaignGameStarter.GetExistingModel<AgentStatCalculateModel>()));
            return;
        }
        
        gameStarterObject.AddModel(new CustomMarchingAgentStatCalculateModel(gameStarterObject.GetExistingModel<AgentStatCalculateModel>()));
    }

    public override void OnMissionBehaviorInitialize(Mission mission)
    {
        base.OnMissionBehaviorInitialize(mission);
        mission.AddMissionBehavior(new MarchMissionBehavior());
    }
}

internal static class Helper
{
    public static TBaseModel GetExistingModel<TBaseModel>(this IGameStarter campaignGameStarter) where TBaseModel : GameModel
    {
        return (TBaseModel)campaignGameStarter.Models.Last(model => model.GetType().IsSubclassOf(typeof(TBaseModel)));
    }
    
    public static string GetFormationName(this Formation formation)
    {
        string className = GameTexts.FindAllTextVariations("str_troop_group_name").ElementAt((int)formation.RepresentativeClass).ToString();
        return className + " " + (formation.Index + 1);
    }
}
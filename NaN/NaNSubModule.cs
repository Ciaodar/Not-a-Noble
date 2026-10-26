using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using NaN.Config;
using NaN.Behaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace NaN
{
    public class NaNSubModule : MBSubModuleBase
    {
        private bool _isLoaded = false;

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            TryActivateOptionalMcmBridge();
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();

            if (!_isLoaded)
            {
                NanSettingsManager.Initialize();
                
                try
                {
                    Harmony harmony = new Harmony("com.ciaodar.notanoble");
                    harmony.PatchAll();
                    TaleWorlds.Library.Debug.Print("[NaN] Harmony patches applied successfully.");
                }
                catch (Exception ex)
                {
                    TaleWorlds.Library.Debug.Print("[NaN] Error applying Harmony patches: " + ex.Message);
                }

                _isLoaded = true;
            }
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarter)
        {
            base.OnGameStart(game, gameStarter);
            
            if (game.GameType is Campaign)
            {
                var campaignStarter = gameStarter as CampaignGameStarter;
                if (campaignStarter != null)
                {
                    campaignStarter.AddBehavior(new NanDialogueBehavior());
                    campaignStarter.AddBehavior(new NanQuestBehavior());
                }
            }
        }

        private void TryActivateOptionalMcmBridge()
        {
            try
            {
                var mcmAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "MCMv5");

                if (mcmAssembly != null)
                {
                    Assembly bridgeAssembly = Assembly.Load("NaN.MCMBridge");
                    Type bridgeType = bridgeAssembly.GetType("NaN.MCMBridge.BridgeBootstrap");
                    MethodInfo registerMethod = bridgeType?.GetMethod("TryRegister", BindingFlags.Static | BindingFlags.Public);
                    registerMethod?.Invoke(null, null);
                }
            }
            catch (Exception ex)
            {
                TaleWorlds.Library.Debug.Print("[NaN] MCM Bridge could not be loaded: " + ex.Message);
            }
        }
    }
}

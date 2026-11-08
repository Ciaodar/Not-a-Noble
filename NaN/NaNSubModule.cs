using System;
using System.IO;
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

            TryActivateOptionalMcmBridge();

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

        private static void TryActivateOptionalMcmBridge()
        {
            try
            {
                Assembly.Load("MCMv5");

                string assemblyLocation = typeof(NaNSubModule).Assembly.Location;
                if (string.IsNullOrWhiteSpace(assemblyLocation))
                    return;

                string binDir = Path.GetDirectoryName(assemblyLocation);
                if (string.IsNullOrWhiteSpace(binDir))
                    return;

                string bridgePath = Path.Combine(binDir, "NaN.MCMBridge.dll");
                if (!File.Exists(bridgePath))
                    return;

                Assembly bridgeAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => string.Equals(a.GetName().Name, "NaN.MCMBridge", StringComparison.OrdinalIgnoreCase))
                    ?? Assembly.LoadFrom(bridgePath);

                Type bootstrapType = bridgeAssembly.GetType("NaN.MCMBridge.BridgeBootstrap");
                MethodInfo tryRegister = bootstrapType?.GetMethod("TryRegister", BindingFlags.Static | BindingFlags.Public);
                tryRegister?.Invoke(null, null);
            }
            catch (Exception ex)
            {
                TaleWorlds.Library.Debug.Print("[NaN] MCM Bridge could not be loaded: " + ex.Message);
            }
        }
    }
}

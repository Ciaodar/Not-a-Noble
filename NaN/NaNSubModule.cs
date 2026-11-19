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
        private static bool _mcmResolverRegistered;
        private static bool _mcmLoadHookRegistered;
        private static bool _isActivatingMcmBridge;

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

        private static Assembly FindLoadedAssembly(string shortName)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => string.Equals(a.GetName().Name, shortName, StringComparison.OrdinalIgnoreCase));
        }

        private static void EnsureMcmAssemblyResolver()
        {
            if (_mcmResolverRegistered)
                return;

            AppDomain.CurrentDomain.AssemblyResolve += OnMcmAssemblyResolve;
            _mcmResolverRegistered = true;
        }

        private static Assembly OnMcmAssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                if (args == null || string.IsNullOrWhiteSpace(args.Name))
                    return null;

                string requestedName = new AssemblyName(args.Name).Name;
                if (string.Equals(requestedName, "MCMv5", StringComparison.OrdinalIgnoreCase))
                    return FindLoadedAssembly("MCMv5");
            }
            catch
            {
            }

            return null;
        }

        private static void EnsureMcmAssemblyLoadHook()
        {
            if (_mcmLoadHookRegistered)
                return;

            AppDomain.CurrentDomain.AssemblyLoad += OnMcmAssemblyLoaded;
            _mcmLoadHookRegistered = true;
        }

        private static void OnMcmAssemblyLoaded(object sender, AssemblyLoadEventArgs args)
        {
            try
            {
                if (args?.LoadedAssembly == null)
                    return;

                if (string.Equals(args.LoadedAssembly.GetName().Name, "MCMv5", StringComparison.OrdinalIgnoreCase))
                {
                    AppDomain.CurrentDomain.AssemblyLoad -= OnMcmAssemblyLoaded;
                    _mcmLoadHookRegistered = false;
                    TryActivateOptionalMcmBridge();
                }
            }
            catch
            {
            }
        }

        private static void TryActivateOptionalMcmBridge()
        {
            if (_isActivatingMcmBridge)
                return;

            _isActivatingMcmBridge = true;
            try
            {
                Assembly mcmAssembly = FindLoadedAssembly("MCMv5");
                if (mcmAssembly == null)
                {
                    EnsureMcmAssemblyLoadHook();
                    try
                    {
                        mcmAssembly = Assembly.Load("MCMv5");
                    }
                    catch
                    {
                        return;
                    }
                }

                if (mcmAssembly == null)
                    return;

                EnsureMcmAssemblyResolver();

                string assemblyLocation = typeof(NaNSubModule).Assembly.Location;
                if (string.IsNullOrWhiteSpace(assemblyLocation))
                    return;

                string binDir = Path.GetDirectoryName(assemblyLocation);
                if (string.IsNullOrWhiteSpace(binDir))
                    return;

                string bridgePath = Path.Combine(binDir, "NaN.MCMBridge.dll");
                if (!File.Exists(bridgePath))
                    return;

                Assembly bridgeAssembly = FindLoadedAssembly("NaN.MCMBridge") ?? Assembly.LoadFrom(bridgePath);

                Type bootstrapType = bridgeAssembly.GetType("NaN.MCMBridge.BridgeBootstrap");
                MethodInfo tryRegister = bootstrapType?.GetMethod("TryRegister", BindingFlags.Static | BindingFlags.Public);
                tryRegister?.Invoke(null, null);
            }
            catch (Exception ex)
            {
                TaleWorlds.Library.Debug.Print("[NaN] MCM Bridge could not be loaded: " + ex.Message);
            }
            finally
            {
                _isActivatingMcmBridge = false;
            }
        }
    }
}

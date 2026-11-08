using System;
using System.ComponentModel;
using NaN.Config;
using NaN.MCMBridge.Config;

namespace NaN.MCMBridge
{
    public static class BridgeBootstrap
    {
        private static NanMcmSettings? _subscribedInstance;

        public static void TryRegister()
        {
            try
            {
                NanSettingsManager.RegisterExternalSettingsProvider(BuildModelFromMcm);

                var instance = NanMcmSettings.Instance;
                if (instance != null && !ReferenceEquals(_subscribedInstance, instance))
                {
                    if (_subscribedInstance != null)
                    {
                        _subscribedInstance.PropertyChanged -= OnSettingsPropertyChanged;
                    }

                    instance.PropertyChanged += OnSettingsPropertyChanged;
                    _subscribedInstance = instance;
                }
            }
            catch (Exception ex)
            {
                TaleWorlds.Library.Debug.Print("[NaN.MCMBridge] Error during MCM registration: " + ex.Message);
            }
        }

        private static void OnSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NanSettingsManager.Reload();
        }

        private static NanJsonModel BuildModelFromMcm()
        {
            var mcm = NanMcmSettings.Instance ?? new NanMcmSettings();
            return mcm.ToJsonModel();
        }
    }
}

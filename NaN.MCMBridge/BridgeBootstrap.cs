using System;
using System.ComponentModel;
using NaN.Config;
using NaN.MCMBridge.Config;

namespace NaN.MCMBridge
{
    public static class BridgeBootstrap
    {
        public static void TryRegister()
        {
            try
            {
                NanSettingsManager.RegisterExternalSettingsProvider(() =>
                {
                    if (NanMcmSettings.Instance != null)
                    {
                        return NanMcmSettings.Instance.ToJsonModel();
                    }
                    return null;
                });

                if (NanMcmSettings.Instance != null)
                {
                    NanMcmSettings.Instance.PropertyChanged += OnSettingsPropertyChanged;
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
    }
}

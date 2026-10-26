using HarmonyLib;
using NaN.Config;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace NaN.Patches
{
    [HarmonyPatch(typeof(Clan), "AddRenown")]
    public static class ClanAddRenownPatch
    {
        public static bool BypassCap = false;

        [HarmonyPriority(Priority.Last)]
        public static bool Prefix(Clan __instance, ref float value, bool shouldNotify)
        {
            if (!NanSettingsManager.IsModEnabled || BypassCap) return true;

            // Definitive Cheat Detection:
            // Vanilla Bannerlord never awards massive amounts of renown in a single action.
            // If the value being added is exceptionally large (e.g., >= 150 at once), 
            // it is definitively a cheat/console command.
            if (value >= 150f)
            {
                return true; 
            }

            if (__instance == Clan.PlayerClan && Clan.PlayerClan.Tier < 6)
            {
                    int nextTier = Clan.PlayerClan.Tier + 1;
                    float nextTierRenown = Campaign.Current.Models.ClanTierModel.GetRequiredRenownForTier(nextTier);
                    
                    if (Clan.PlayerClan.Renown + value >= nextTierRenown)
                    {
                        // Cap renown exactly before the threshold
                        float allowedGain = (nextTierRenown - 1f) - Clan.PlayerClan.Renown;
                        
                        if (allowedGain <= 0)
                        {
                            value = 0f;

                            var message = new TaleWorlds.Localization.TextObject("{=nan_renown_locked}You cannot gain more renown! Check the Quests tab to see how to ascend to {TIER_NAME}.");
                            message.SetTextVariable("TIER_NAME", nextTier.ToString());
                            
                            // Top-middle toast notification
                            TaleWorlds.Core.MBInformationManager.AddQuickInformation(message);
                            
                            // Bottom-left chat log
                            TaleWorlds.Library.InformationManager.DisplayMessage(new TaleWorlds.Library.InformationMessage(message.ToString(), TaleWorlds.Library.Colors.Red));
                            
                            return false; // Skip original method because gain is zero
                        }
                        else
                        {
                            value = allowedGain;
                            // Allow original method to run with capped value
                        }
                }
            }

            return true;
        }
    }
}

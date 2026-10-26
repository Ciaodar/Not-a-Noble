using System.Collections.Generic;
using System.Linq;
using NaN.Quests;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace NaN
{
    public static class ConsoleCommands
    {
        [TaleWorlds.Library.CommandLineFunctionality.CommandLineArgumentFunction("complete_quest", "nan")]
        public static string CompleteActiveAscensionQuest(List<string> args)
        {
            if (Campaign.Current == null)
            {
                return "Campaign is not loaded.";
            }

            var activeQuest = Campaign.Current.QuestManager.Quests.FirstOrDefault(q => q is NanAscensionQuestBase) as NanAscensionQuestBase;

            if (activeQuest != null)
            {
                activeQuest.AscendToNextTier();
                return $"Successfully completed and ascended quest: {activeQuest.Title}";
            }

            return "No active Ascension quest found.";
        }
        
        [TaleWorlds.Library.CommandLineFunctionality.CommandLineArgumentFunction("add_renown_to_cap", "nan")]
        public static string AddRenownToCap(List<string> args)
        {
            if (Campaign.Current == null) return "Campaign not loaded.";
            
            float nextTierRenown = Campaign.Current.Models.ClanTierModel.GetRequiredRenownForTier(Clan.PlayerClan.Tier + 1);
            float allowedGain = (nextTierRenown - 1f) - Clan.PlayerClan.Renown;

            if (allowedGain > 0)
            {
                Clan.PlayerClan.AddRenown(allowedGain);
                return $"Added {allowedGain} renown. You are now exactly at the cap before Tier {Clan.PlayerClan.Tier + 1}.";
            }
            return "You are already at the renown cap.";
        }
    }
}

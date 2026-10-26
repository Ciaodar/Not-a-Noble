using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace NaN.Quests
{
    public abstract class NanAscensionQuestBase : QuestBase
    {
        protected NanAscensionQuestBase(string questId, Hero questGiver, CampaignTime duration, int rewardGold) 
            : base(questId, questGiver, duration, rewardGold)
        {
        }

        // Native quests require some text implementation
        public override TextObject Title => new TextObject("{=nan_quest_title_fallback}Ascension Quest");
        public override bool IsRemainingTimeHidden => true;
        public override string SpecialQuestType => "AscensionQuest";

        protected override void InitializeQuestOnGameLoad()
        {
            SetDialogs();
        }

        protected override void SetDialogs()
        {
            // Dialogs specific to this quest can be added here if necessary
        }

        public void AscendToNextTier()
        {
            int nextTier = Clan.PlayerClan.Tier + 1;
            float nextTierRenown = Campaign.Current.Models.ClanTierModel.GetRequiredRenownForTier(nextTier);
            float neededRenown = System.Math.Max(1f, nextTierRenown - Clan.PlayerClan.Renown);

            Patches.ClanAddRenownPatch.BypassCap = true;
            Clan.PlayerClan.AddRenown(neededRenown, true);
            Patches.ClanAddRenownPatch.BypassCap = false;
            
            CompleteQuestWithSuccess();
            
            MBInformationManager.AddQuickInformation(new TextObject("{=nan_ascension_success}Your clan has been recognized and ascended to the next tier!"));
        }
    }
}

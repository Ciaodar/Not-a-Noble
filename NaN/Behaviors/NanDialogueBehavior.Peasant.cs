using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace NaN.Behaviors
{
    public partial class NanDialogueBehavior
    {
        private void AddPeasantDialogues(CampaignGameStarter starter)
        {
            // 1. Generic Greeting Replacement ("So, then. What is it?")
            starter.AddDialogLine("nan_peasant_greeting", "lord_start", "hero_main_options", 
                "{=nan_peasant_greeting}Why are you taking my precious time, peasant?! I hope its important.", 
                ConditionPeasantTier01, null, 150);

            // 2. Hook: Block Diplomacy (Tier 0 ONLY) ("There is something I'd like to discuss")
            starter.AddDialogLine("nan_peasant_reject_diplomacy", "lord_politics_request", "hero_main_options", 
                "{=nan_peasant_reject_diplomacy}There's no way, I'm planning to discuss something with a filthy peasant. Know your place!", 
                ConditionPeasantTier0Only, null, 150);

            // 3. Hook: Block Quests (Tier 0 ONLY) ("I heard you may need some help")
            starter.AddDialogLine("nan_peasant_reject_issue", "issue_offer", "hero_main_options", 
                "{=nan_peasant_reject_issue}I have no intention of giving this task to a peasant of unknown origin.", 
                ConditionPeasantTier0Only, null, 150);

            // 4. First Introduction Replacement ("Its an honor to meet you good sir.")
            starter.AddDialogLine("nan_peasant_intro", "lord_introduction", "lord_start", 
                "{=nan_peasant_intro}My men also does not recognize you or your clans name. Which means that you are not even a Bannerman.", 
                ConditionPeasantTier01, null, 150);

            // 5. Demands Replacement ("Eh? What do you want?")
            starter.AddDialogLine("nan_peasant_demand", "lord_predemand", "hero_main_options", 
                "{=nan_peasant_demand}Demands...? Are you okay? You must've hit your head somewhere. A peasant demands something from me, eh?", 
                ConditionPeasantTier01, null, 150);

            // 6. Follow-up Replacement ("Is there anything else?")
            starter.AddDialogLine("nan_peasant_pretalk", "lord_pretalk", "hero_main_options", 
                "{=nan_peasant_pretalk}I hope thats all. I'm not willing to spend one more second with a peasant after all!", 
                ConditionPeasantTier01, null, 150);
            starter.AddDialogLine("nan_peasant_ask_again", "lord_talk_ask_something_again", "lord_talk_ask_something_2", 
                "{=nan_peasant_pretalk}I hope thats all. I'm not willing to spend one more second with a peasant after all!", 
                ConditionPeasantTier01, null, 150);

            // 7. Farewell Replacement ("Farewell, then.")
            starter.AddDialogLine("nan_peasant_leave", "hero_leave", "close_window", 
                "{=nan_peasant_leave}Yes, off with you. Frankly, you should have vanished from my sight long before this.", 
                ConditionPeasantTier01, null, 150);
        }

        private bool ConditionPeasantTier01()
        {
            if (Hero.MainHero == null || Hero.OneToOneConversationHero == null) return false;
            if (!Hero.OneToOneConversationHero.IsLord) return false;
            if (Hero.OneToOneConversationHero.Clan == Clan.PlayerClan) return false;
            if (Hero.OneToOneConversationHero.IsPrisoner || Hero.MainHero.IsPrisoner) return false;
            if (HeroHelper.WillLordAttack()) return false;
            if (Hero.OneToOneConversationHero.MapFaction != null && Hero.MainHero.MapFaction != null &&
                Hero.OneToOneConversationHero.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction)) return false;

            if (Hero.OneToOneConversationHero.GetRelation(Hero.MainHero) > NaN.Config.NanSettingsManager.PeasantDialogueRelationThreshold) return false;

            int playerTier = Clan.PlayerClan.Tier;
            return playerTier <= 1; // Tier 0 (Peasant) or Tier 1 (Freebooter)
        }

        private bool ConditionPeasantTier0Only()
        {
            if (Hero.MainHero == null || Hero.OneToOneConversationHero == null) return false;
            if (!Hero.OneToOneConversationHero.IsLord) return false;
            if (Hero.OneToOneConversationHero.Clan == Clan.PlayerClan) return false;
            if (Hero.OneToOneConversationHero.IsPrisoner || Hero.MainHero.IsPrisoner) return false;
            if (HeroHelper.WillLordAttack()) return false;
            if (Hero.OneToOneConversationHero.MapFaction != null && Hero.MainHero.MapFaction != null &&
                Hero.OneToOneConversationHero.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction)) return false;

            if (Hero.OneToOneConversationHero.GetRelation(Hero.MainHero) > NaN.Config.NanSettingsManager.PeasantDialogueRelationThreshold) return false;

            int playerTier = Clan.PlayerClan.Tier;
            return playerTier == 0; // Tier 0 ONLY (Peasant)
        }
    }
}

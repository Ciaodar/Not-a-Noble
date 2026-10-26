using System;
using NaN.Config;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation;

namespace NaN.Behaviors
{
    public partial class NanDialogueBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, AddDialogues);
        }

        public override void SyncData(IDataStore dataStore)
        {
            // No data to sync for now
        }

        private void AddDialogues(CampaignGameStarter starter)
        {
            // Strict dialogue is temporarily disabled as per user request.
            // AddPersuasionDialogues and AddChieftainDialogues still remain if they don't break the game.

            AddPersuasionDialogues(starter);
            AddChieftainDialogues(starter);
            AddPeasantDialogues(starter);
        }

        private bool conversation_nan_refusal_on_condition()
        {
            if (!NanSettingsManager.IsModEnabled || !NanSettingsManager.StrictDialogueEnabled)
                return false;

            if (Hero.OneToOneConversationHero != null)
            {
                var lord = Hero.OneToOneConversationHero;
                
                // Example condition: Lord is Tier 4+, Player is Tier 0
                if (lord.Clan != null && lord.Clan.Tier >= 4 && Clan.PlayerClan.Tier == 0)
                {
                    // Optionally, skip companions or family members
                    if (!lord.IsPlayerCompanion && !lord.IsFriend(Hero.MainHero))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        internal static void SetSpecialLine(ConversationSentence sentence)
        {
            try
            {
                var field = typeof(ConversationSentence).GetField("_flags", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null && sentence != null)
                {
                    uint currentFlags = (uint)field.GetValue(sentence);
                    field.SetValue(sentence, currentFlags | 4u); // 4 = SpecialLine (adds quest exclamation mark & color)
                }
            }
            catch (Exception ex)
            {
                TaleWorlds.Library.Debug.Print("[NaN] Error setting SpecialLine flag: " + ex.Message);
            }
        }
    }
}

using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using NaN.Quests;

namespace NaN.Behaviors
{
    public class NanQuestBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.RenownGained.AddNonSerializedListener(this, OnRenownGained);
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
        }

        public override void SyncData(IDataStore dataStore)
        {
            // The active quest instances are saved by the QuestManager automatically 
            // as long as we register them in our SaveDefiner.
        }

        private void OnGameLoaded(CampaignGameStarter starter)
        {
            CheckTierAscension(Clan.PlayerClan);
        }

        private void OnDailyTick()
        {
            CheckTierAscension(Clan.PlayerClan);
        }

        private void OnRenownGained(Hero hero, int gainedRenown, bool doNotNotify)
        {
            if (hero == Hero.MainHero && hero.Clan != null)
            {
                CheckTierAscension(hero.Clan);
            }
        }

        private void CheckTierAscension(Clan clan)
        {
            if (clan.Tier >= 6) return;

            float renownThreshold = Campaign.Current.Models.ClanTierModel.GetRequiredRenownForTier(clan.Tier + 1);
            
            
            // Floating point precision can cause renown to be slightly under 49 (e.g. 48.99999)
            if (clan.Renown >= (renownThreshold - 1.05f))
            {
                TriggerAscensionQuest(clan.Tier);
            }
        }

        private void TriggerAscensionQuest(int currentTier)
        {
            if (Campaign.Current.QuestManager.Quests.Any(q => q is NanAscensionQuestBase))
            {
                return;
            }

            NanAscensionQuestBase newQuest = null;

            switch (currentTier)
            {
                case 0:
                    newQuest = new NanFreebooterQuest();
                    break;
                case 1:
                    newQuest = new NanBannerlordQuest();
                    break;
                case 2:
                    newQuest = new NanChieftainQuest();
                    break;
                case 3:
                    newQuest = new NanCountQuest();
                    break;
                case 4:
                    newQuest = new NanDukeQuest();
                    break;
                case 5:
                    newQuest = new NanSovereignQuest();
                    break;
            }

            if (newQuest != null)
            {
                newQuest.StartQuest();
            }
        }
    }
}

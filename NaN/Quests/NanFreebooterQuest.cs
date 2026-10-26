using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace NaN.Quests
{
    public class NanFreebooterQuest : NanAscensionQuestBase
    {
        [SaveableField(1)]
        private List<string> _completedVillageIds;

        [SaveableField(2)]
        private JournalLog _villageTaskLog;
        [SaveableField(3)]
        private JournalLog _relationTaskLog;

        public override TextObject Title => new TextObject("{=nan_freebooter_title}Path to a Freebooter");

        public NanFreebooterQuest() : base("nan_freebooter_quest", null, CampaignTime.Never, 0)
        {
            _completedVillageIds = new List<string>();
            SetDialogs();
            InitializeQuestOnCreation();
        }

        protected override void OnStartQuest()
        {
            int requiredQuests = NaN.Config.NanSettingsManager.FreebooterRequiredQuests;
            var log = new TextObject("{=nan_freebooter_init_log}To prove yourself as a Freebooter, you must complete the quests of {REQUIRED_QUESTS} different villages and reach a total relation of {REQUIRED_RELATION}+ with the villagers.");
            log.SetTextVariable("REQUIRED_QUESTS", requiredQuests);
            log.SetTextVariable("REQUIRED_RELATION", NaN.Config.NanSettingsManager.FreebooterRequiredRelation);
            
            var villageTaskName = new TextObject("{=nan_freebooter_task_villages}Completed Different Village Quests");
            _villageTaskLog = AddDiscreteLog(log, villageTaskName, _completedVillageIds.Count, requiredQuests, null, false);

            var relationTaskName = new TextObject("{=nan_freebooter_task_relation}Total Relation with Villagers");
            _relationTaskLog = AddDiscreteLog(new TextObject(""), relationTaskName, GetTotalVillagerRelation(), NaN.Config.NanSettingsManager.FreebooterRequiredRelation, null, false);
        }

        protected override void InitializeQuestOnGameLoad()
        {
            base.InitializeQuestOnGameLoad();
            _completedVillageIds = _completedVillageIds ?? new List<string>();
        }

        protected override void RegisterEvents()
        {
            CampaignEvents.OnQuestCompletedEvent.AddNonSerializedListener(this, OnQuestCompleted);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
        }

        private void OnQuestCompleted(QuestBase quest, QuestCompleteDetails detail)
        {
            if (quest.QuestGiver != null && quest.QuestGiver.CurrentSettlement != null && quest.QuestGiver.CurrentSettlement.IsVillage)
            {
                if (detail == QuestCompleteDetails.Success)
                {
                    string villageId = quest.QuestGiver.CurrentSettlement.StringId;
                    if (!_completedVillageIds.Contains(villageId))
                    {
                        _completedVillageIds.Add(villageId);
                        
                        if (_villageTaskLog != null)
                        {
                            _villageTaskLog.UpdateCurrentProgress(_completedVillageIds.Count);
                        }
                        
                        int requiredQuests = NaN.Config.NanSettingsManager.FreebooterRequiredQuests;
                        var log = new TextObject("{=nan_freebooter_log}You have successfully helped {VILLAGE_NAME}. ({CURRENT}/{REQUIRED})");
                        log.SetTextVariable("VILLAGE_NAME", quest.QuestGiver.CurrentSettlement.Name);
                        log.SetTextVariable("CURRENT", _completedVillageIds.Count);
                        log.SetTextVariable("REQUIRED", requiredQuests);
                        AddLog(log, false);
                        
                        CheckCompletion();
                    }
                }
            }
        }

        private void OnDailyTick()
        {
            UpdateQuestProgress();
            CheckCompletion();
        }

        private void UpdateQuestProgress()
        {
            if (_relationTaskLog != null)
            {
                int currentRelation = GetTotalVillagerRelation();
                int requiredRelation = NaN.Config.NanSettingsManager.FreebooterRequiredRelation;
                _relationTaskLog.UpdateCurrentProgress(currentRelation > requiredRelation ? requiredRelation : currentRelation);
            }
        }

        private void CheckCompletion()
        {
            if (_completedVillageIds.Count >= NaN.Config.NanSettingsManager.FreebooterRequiredQuests && GetTotalVillagerRelation() >= NaN.Config.NanSettingsManager.FreebooterRequiredRelation)
            {
                AscendToNextTier();
            }
        }

        private int GetTotalVillagerRelation()
        {
            int total = 0;
            foreach (var hero in Hero.AllAliveHeroes)
            {
                if (hero.IsNotable && hero.CurrentSettlement != null && hero.CurrentSettlement.IsVillage)
                {
                    int relation = hero.GetRelation(Hero.MainHero);
                    if (relation > 0)
                    {
                        total += relation;
                    }
                }
            }
            return total;
        }
    }
}

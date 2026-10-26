using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace NaN.Quests
{
    public class NanCountQuest : NanAscensionQuestBase
    {
        [SaveableField(1)]
        private JournalLog _countTaskLog;

        public override TextObject Title => new TextObject("{=nan_count_title}Path to a Count");

        public NanCountQuest() : base("nan_count_quest", null, CampaignTime.Never, 0)
        {
            SetDialogs();
            InitializeQuestOnCreation();
        }

        protected override void OnStartQuest()
        {
            var log = new TextObject("{=nan_count_init_log}To prove yourself as a Count, you must either conquer a castle or successfully defend your own castle against a siege.");
            
            var taskName = new TextObject("{=nan_count_task}Conquered or Defended Castle");
            _countTaskLog = AddDiscreteLog(log, taskName, 0, 1, null, false);
        }

        protected override void RegisterEvents()
        {
            CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener(this, OnSettlementOwnerChanged);
            CampaignEvents.MapEventEnded.AddNonSerializedListener(this, OnMapEventEnded);
        }

        private void OnSettlementOwnerChanged(Settlement settlement, bool openToClaim, Hero newOwner, Hero oldOwner, Hero capturerHero, ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail)
        {
            if (settlement.IsCastle && capturerHero == Hero.MainHero)
            {
                if (_countTaskLog != null) _countTaskLog.UpdateCurrentProgress(1);
                
                AddLog(new TextObject("{=nan_count_capture_log}You have raised your banners over a conquered castle. Your might is undeniable."), false);
                AscendToNextTier();
            }
        }

        private void OnMapEventEnded(MapEvent mapEvent)
        {
            if (mapEvent.IsSiegeAssault && mapEvent.MapEventSettlement != null && mapEvent.MapEventSettlement.IsCastle)
            {
                if (mapEvent.WinningSide == mapEvent.DefenderSide.MissionSide && mapEvent.DefenderSide.LeaderParty != null && mapEvent.DefenderSide.LeaderParty.LeaderHero == Hero.MainHero)
                {
                    if (_countTaskLog != null) _countTaskLog.UpdateCurrentProgress(1);
                    
                    AddLog(new TextObject("{=nan_count_defend_log}The castle walls held strong under your command. You have proven yourself a capable defender of the realm."), false);
                    AscendToNextTier();
                }
            }
        }
    }
}

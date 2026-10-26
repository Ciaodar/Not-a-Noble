using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace NaN.Quests
{
    public class NanSovereignQuest : NanAscensionQuestBase
    {
        [SaveableField(1)]
        private JournalLog _townTaskLog;
        [SaveableField(2)]
        private JournalLog _castleTaskLog;

        public override TextObject Title => new TextObject("{=nan_sovereign_title}Path to a Sovereign");

        public NanSovereignQuest() : base("nan_sovereign_quest", null, CampaignTime.Never, 0)
        {
            SetDialogs();
            InitializeQuestOnCreation();
        }

        protected override void OnStartQuest()
        {
            int reqTowns = NaN.Config.NanSettingsManager.SovereignRequiredTowns;
            int reqCastles = NaN.Config.NanSettingsManager.SovereignRequiredCastles;

            var log = new TextObject("{=nan_sovereign_init_log}To be crowned as the High Emperor (Sovereign) of Calradia, you must forge a kingdom holding at least {REQUIRED_TOWNS} great towns and {REQUIRED_CASTLES} castles.");
            log.SetTextVariable("REQUIRED_TOWNS", reqTowns);
            log.SetTextVariable("REQUIRED_CASTLES", reqCastles);
            
            var townTask = new TextObject("{=nan_sovereign_task_town}Owned Towns");
            _townTaskLog = AddDiscreteLog(log, townTask, GetTownCount(), reqTowns, null, false);
            
            var castleTask = new TextObject("{=nan_sovereign_task_castle}Owned Castles");
            _castleTaskLog = AddDiscreteLog(new TextObject(""), castleTask, GetCastleCount(), reqCastles, null, false);
        }

        protected override void RegisterEvents()
        {
            CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener(this, OnSettlementOwnerChanged);
            CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener(this, OnClanChangedKingdom);
        }

        private void OnSettlementOwnerChanged(Settlement settlement, bool openToClaim, Hero newOwner, Hero oldOwner, Hero capturerHero, ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail)
        {
            CheckCompletion();
        }

        private void OnClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, ChangeKingdomAction.ChangeKingdomActionDetail detail, bool showNotification)
        {
            if (clan == Clan.PlayerClan)
            {
                CheckCompletion();
            }
        }

        private int GetTownCount()
        {
            return (Clan.PlayerClan.Kingdom != null && Clan.PlayerClan.Kingdom.Leader == Hero.MainHero) ? Clan.PlayerClan.Settlements.Count(s => s.IsTown) : 0;
        }

        private int GetCastleCount()
        {
            return (Clan.PlayerClan.Kingdom != null && Clan.PlayerClan.Kingdom.Leader == Hero.MainHero) ? Clan.PlayerClan.Settlements.Count(s => s.IsCastle) : 0;
        }

        private void CheckCompletion()
        {
            int townCount = GetTownCount();
            int castleCount = GetCastleCount();
            int reqTowns = NaN.Config.NanSettingsManager.SovereignRequiredTowns;
            int reqCastles = NaN.Config.NanSettingsManager.SovereignRequiredCastles;

            if (_townTaskLog != null) _townTaskLog.UpdateCurrentProgress(townCount > reqTowns ? reqTowns : townCount);
            if (_castleTaskLog != null) _castleTaskLog.UpdateCurrentProgress(castleCount > reqCastles ? reqCastles : castleCount);

            if (townCount >= reqTowns && castleCount >= reqCastles)
            {
                var log = new TextObject("{=nan_sovereign_log}From peasant to Emperor, you have forged a mighty kingdom with {REQUIRED_TOWNS} great towns and {REQUIRED_CASTLES} unyielding castles. All of Calradia will know your name!");
                log.SetTextVariable("REQUIRED_TOWNS", reqTowns);
                log.SetTextVariable("REQUIRED_CASTLES", reqCastles);
                AddLog(log, false);
                AscendToNextTier();
            }
        }
    }
}

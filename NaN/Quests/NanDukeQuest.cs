using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace NaN.Quests
{
    public class NanDukeQuest : NanAscensionQuestBase
    {
        [SaveableField(1)]
        private List<string> _capturedRulers;

        [SaveableField(2)]
        private JournalLog _dukeTaskLog;

        public override TextObject Title => new TextObject("{=nan_duke_title}Path to a Duke");

        public NanDukeQuest() : base("nan_duke_quest", null, CampaignTime.Never, 0)
        {
            _capturedRulers = new List<string>();
            SetDialogs();
            InitializeQuestOnCreation();
        }

        protected override void OnStartQuest()
        {
            var log = new TextObject("{=nan_duke_init_log}To declare your might as a Duke across the realm, you must capture the sovereign of 2 different kingdoms in battle.");
            
            var taskName = new TextObject("{=nan_duke_task}Captured Sovereigns");
            _dukeTaskLog = AddDiscreteLog(log, taskName, _capturedRulers.Count, 2, null, false);
        }

        protected override void InitializeQuestOnGameLoad()
        {
            base.InitializeQuestOnGameLoad();
            _capturedRulers = _capturedRulers ?? new List<string>();
        }

        protected override void RegisterEvents()
        {
            CampaignEvents.HeroPrisonerTaken.AddNonSerializedListener(this, OnHeroPrisonerTaken);
        }

        private void OnHeroPrisonerTaken(PartyBase captor, Hero prisoner)
        {
            if (captor == PartyBase.MainParty && prisoner.Clan != null && prisoner.Clan.Kingdom != null)
            {
                if (prisoner.Clan.Kingdom.Leader == prisoner && prisoner.Clan.Kingdom != Clan.PlayerClan.Kingdom)
                {
                    if (!_capturedRulers.Contains(prisoner.StringId))
                    {
                        _capturedRulers.Add(prisoner.StringId);
                        
                        if (_dukeTaskLog != null)
                        {
                            _dukeTaskLog.UpdateCurrentProgress(_capturedRulers.Count);
                        }
                        
                        var log = new TextObject("{=nan_duke_log}The sovereign {RULER_NAME} of {KINGDOM_NAME} now kneels before you. The realm trembles. ({CURRENT}/2)");
                        log.SetTextVariable("RULER_NAME", prisoner.Name);
                        log.SetTextVariable("KINGDOM_NAME", prisoner.Clan.Kingdom.Name);
                        log.SetTextVariable("CURRENT", _capturedRulers.Count);
                        AddLog(log, false);
                        
                        if (_capturedRulers.Count >= 2)
                        {
                            AscendToNextTier();
                        }
                    }
                }
            }
        }
    }
}

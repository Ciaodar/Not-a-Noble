using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace NaN.Quests
{
    public class NanChieftainQuest : NanAscensionQuestBase
    {
        [SaveableField(1)]
        private List<string> _defeatedLords;
        [SaveableField(2)]
        private int _releasePoints;

        [SaveableField(3)]
        private JournalLog _captureTaskLog;
        [SaveableField(4)]
        private JournalLog _releaseTaskLog;

        [SaveableField(6)]
        private List<string> _failedPersuasionLords;

        public override TextObject Title => new TextObject("{=nan_chieftain_title}Path to a Chieftain");

        public NanChieftainQuest() : base("nan_chieftain_quest", null, CampaignTime.Never, 0)
        {
            _defeatedLords = new List<string>();
            _failedPersuasionLords = new List<string>();
            _releasePoints = 0;
            SetDialogs();
            InitializeQuestOnCreation();
        }

        protected override void OnStartQuest()
        {
            int reqCaptures = NaN.Config.NanSettingsManager.ChieftainRequiredCaptures;
            int reqReleases = NaN.Config.NanSettingsManager.ChieftainRequiredReleases;

            var log = new TextObject("{=nan_chieftain_init_log}You must defeat and capture {REQUIRED_CAPTURES} different lords on the battlefield. You must also release {REQUIRED_RELEASES} high-ranking noble in exchange for their recognition of your nobility.");
            log.SetTextVariable("REQUIRED_CAPTURES", reqCaptures);
            log.SetTextVariable("REQUIRED_RELEASES", reqReleases);
            
            var captureTask = new TextObject("{=nan_chieftain_task_capture}Captured Lords");
            _captureTaskLog = AddDiscreteLog(log, captureTask, _defeatedLords.Count, reqCaptures, null, false);
            
            var releaseTask = new TextObject("{=nan_chieftain_task_release}Released Nobles");
            _releaseTaskLog = AddDiscreteLog(new TextObject(""), releaseTask, _releasePoints, reqReleases, null, false);
        }

        protected override void InitializeQuestOnGameLoad()
        {
            base.InitializeQuestOnGameLoad();
            _defeatedLords = _defeatedLords ?? new List<string>();
            _failedPersuasionLords = _failedPersuasionLords ?? new List<string>();
        }

        public void AddFailedPersuasion(Hero lord)
        {
            if (lord != null && !_failedPersuasionLords.Contains(lord.StringId))
            {
                _failedPersuasionLords.Add(lord.StringId);
            }
        }

        public bool HasFailedPersuasion(Hero lord)
        {
            return lord != null && _failedPersuasionLords.Contains(lord.StringId);
        }

        protected override void RegisterEvents()
        {
            CampaignEvents.HeroPrisonerTaken.AddNonSerializedListener(this, OnHeroPrisonerTaken);
        }

        private void OnHeroPrisonerTaken(PartyBase captor, Hero prisoner)
        {
            if (captor == PartyBase.MainParty && prisoner.IsLord)
            {
                if (!_defeatedLords.Contains(prisoner.StringId))
                {
                    _defeatedLords.Add(prisoner.StringId);
                    
                    if (_captureTaskLog != null)
                    {
                        _captureTaskLog.UpdateCurrentProgress(_defeatedLords.Count);
                    }
                    
                    int reqCaptures = NaN.Config.NanSettingsManager.ChieftainRequiredCaptures;
                    var log = new TextObject("{=nan_chieftain_capture_log}You have defeated and captured {LORD_NAME}. ({CURRENT}/{REQUIRED})");
                    log.SetTextVariable("LORD_NAME", prisoner.Name);
                    log.SetTextVariable("CURRENT", _defeatedLords.Count);
                    log.SetTextVariable("REQUIRED", reqCaptures);
                    AddLog(log, false);
                    
                    CheckCompletion();
                }
            }
        }

        public void AddReleasePoints(int points)
        {
            _releasePoints += points;
            
            if (_releaseTaskLog != null)
            {
                _releaseTaskLog.UpdateCurrentProgress(_releasePoints);
            }
            
            int reqReleases = NaN.Config.NanSettingsManager.ChieftainRequiredReleases;
            var log = new TextObject("{=nan_chieftain_release_log}You released a noble and gained their recognition. Points: {CURRENT}/{TARGET}");
            log.SetTextVariable("CURRENT", _releasePoints);
            log.SetTextVariable("TARGET", reqReleases);
            AddLog(log, false);
            
            CheckCompletion();
        }

        [SaveableField(5)]
        private bool _hasShownRelationHintLog;

        public void AddRelationHintLog()
        {
            if (!_hasShownRelationHintLog)
            {
                var text = new TextObject("{=nan_chieftain_hint_relation}Perhaps you should try this offer on prisoners with whom you have a better relationship (Relation >= 7).");
                AddLog(text, false);
                _hasShownRelationHintLog = true;
            }
        }

        private void CheckCompletion()
        {
            int reqCaptures = NaN.Config.NanSettingsManager.ChieftainRequiredCaptures;
            int reqReleases = NaN.Config.NanSettingsManager.ChieftainRequiredReleases;
            if (_defeatedLords.Count >= reqCaptures && _releasePoints >= reqReleases)
            {
                AscendToNextTier();
            }
        }
    }
}

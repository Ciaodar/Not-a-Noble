using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace NaN.Quests
{
    public class NanBannerlordQuest : NanAscensionQuestBase
    {
        [SaveableField(1)]
        private List<string> _persuadedLords;
        [SaveableField(2)]
        private Dictionary<string, CampaignTime> _failedPersuasions;

        [SaveableField(3)]
        private JournalLog _lordTaskLog;

        public override TextObject Title => new TextObject("{=nan_bannerlord_title}Path to a Bannerman");

        public NanBannerlordQuest() : base("nan_bannerlord_quest", null, CampaignTime.Never, 0)
        {
            _persuadedLords = new List<string>();
            _failedPersuasions = new Dictionary<string, CampaignTime>();
            SetDialogs();
            InitializeQuestOnCreation();
        }

        protected override void OnStartQuest()
        {
            int requiredPersuasions = NaN.Config.NanSettingsManager.BannerlordRequiredPersuasions;
            var log = new TextObject("{=nan_bannerlord_init_log}To ascend to nobility as a Bannerman, you must persuade {REQUIRED_PERSUASIONS} different lords to recognize you as a noble.");
            log.SetTextVariable("REQUIRED_PERSUASIONS", requiredPersuasions);
            
            var taskName = new TextObject("{=nan_bannerlord_task}Persuaded Lords");
            _lordTaskLog = AddDiscreteLog(log, taskName, _persuadedLords.Count, requiredPersuasions, null, false);
        }

        protected override void InitializeQuestOnGameLoad()
        {
            base.InitializeQuestOnGameLoad();
            _persuadedLords = _persuadedLords ?? new List<string>();
            _failedPersuasions = _failedPersuasions ?? new Dictionary<string, CampaignTime>();
        }

        protected override void RegisterEvents()
        {
            // Will add persuasion dialog events here
        }

        public bool IsLordPersuaded(Hero lord)
        {
            return _persuadedLords.Contains(lord.StringId);
        }

        public bool IsLordOnCooldown(Hero lord)
        {
            if (_failedPersuasions.TryGetValue(lord.StringId, out CampaignTime expiryTime))
            {
                if (CampaignTime.Now < expiryTime)
                {
                    return true;
                }
                else
                {
                    _failedPersuasions.Remove(lord.StringId);
                    return false;
                }
            }
            return false;
        }

        public void ApplyPersuasionResult(Hero lord, bool isSuccess)
        {
            if (isSuccess)
            {
                if (!_persuadedLords.Contains(lord.StringId))
                {
                    _persuadedLords.Add(lord.StringId);
                    
                    if (_lordTaskLog != null)
                    {
                        _lordTaskLog.UpdateCurrentProgress(_persuadedLords.Count);
                    }
                    
                    int requiredPersuasions = NaN.Config.NanSettingsManager.BannerlordRequiredPersuasions;
                    var log = new TextObject("{=nan_bannerlord_log}{LORD_NAME} has acknowledged your nobility. ({CURRENT}/{REQUIRED})");
                    log.SetTextVariable("LORD_NAME", lord.Name);
                    log.SetTextVariable("CURRENT", _persuadedLords.Count);
                    log.SetTextVariable("REQUIRED", requiredPersuasions);
                    AddLog(log, false);
                    
                    if (_persuadedLords.Count >= requiredPersuasions)
                    {
                        AscendToNextTier();
                    }
                }
            }
            else
            {
                OnPersuasionFailed(lord);
            }
        }

        public void OnPersuasionFailed(Hero lord)
        {
            _failedPersuasions[lord.StringId] = CampaignTime.Now + CampaignTime.Days(3);
        }
    }
}

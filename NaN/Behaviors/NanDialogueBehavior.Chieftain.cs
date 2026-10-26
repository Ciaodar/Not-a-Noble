using System;
using System.Collections.Generic;
using System.Linq;
using NaN.Config;
using NaN.Quests;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Conversation.Persuasion;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace NaN.Behaviors
{
    public partial class NanDialogueBehavior
    {
        private const float ChieftainMaxScore = 3f;
        private const int ChieftainMaxAttempts = 4;
        private int _chieftainSuccessfulRoundsThisAttempt = 0;
        private int _chieftainTotalAttemptsThisPersuasion = 0;
        private int _chieftainCurrentPage = 0;

        private List<List<PersuasionOptionArgs>> _chieftainPagesOfOptions;
        private PersuasionTask _chieftainCurrentTask;

        private void AddChieftainDialogues(CampaignGameStarter starter)
        {
            var offerLine = starter.AddPlayerLine("nan_chieftain_release_offer", "hero_main_options", "nan_chieftain_release_eval", 
                "{=nan_chieftain_offer}I will release you on one condition. Recognize me and my clan as a Chieftain, and you will have your freedom.",
                NanChieftainOfferCondition, null, 200);
            SetSpecialLine(offerLine);

            starter.AddDialogLine("nan_chieftain_release_eval", "nan_chieftain_release_eval", "nan_chieftain_persuasion_options",
                "{=nan_chieftain_eval}You dare bargain with me? Let's hear what you have to say.",
                null, StartChieftainPersuasion, 100);

            starter.AddDialogLine("nan_chieftain_task_line", "nan_chieftain_persuasion_options", "nan_chieftain_persuasion_options",
                "{=!}{NAN_CHIEFTAIN_PERSUASION_TASK_LINE}", ShowChieftainTaskLineCondition, null, 100);

            starter.AddPlayerLine("nan_chieftain_option_1", "nan_chieftain_persuasion_options", "nan_chieftain_reaction",
                "{=!}{NAN_CHIEFTAIN_PERSUASION_OPTION_1}",
                () => SetupChieftainPersuasionOptionText(0, "NAN_CHIEFTAIN_PERSUASION_OPTION_1"),
                () => BlockChieftainCurrentOption(0), 100, ChieftainOption1Clickable, SetupChieftainPersuasionOption1);
                
            starter.AddPlayerLine("nan_chieftain_option_2", "nan_chieftain_persuasion_options", "nan_chieftain_reaction",
                "{=!}{NAN_CHIEFTAIN_PERSUASION_OPTION_2}",
                () => SetupChieftainPersuasionOptionText(1, "NAN_CHIEFTAIN_PERSUASION_OPTION_2"),
                () => BlockChieftainCurrentOption(1), 100, ChieftainOption2Clickable, SetupChieftainPersuasionOption2);
                
            starter.AddPlayerLine("nan_chieftain_option_3", "nan_chieftain_persuasion_options", "nan_chieftain_reaction",
                "{=!}{NAN_CHIEFTAIN_PERSUASION_OPTION_3}",
                () => SetupChieftainPersuasionOptionText(2, "NAN_CHIEFTAIN_PERSUASION_OPTION_3"),
                () => BlockChieftainCurrentOption(2), 100, ChieftainOption3Clickable, SetupChieftainPersuasionOption3);

            starter.AddDialogLine("nan_chieftain_reaction", "nan_chieftain_reaction", "nan_chieftain_result",
                "{=!}{NAN_CHIEFTAIN_PERSUASION_REACTION}", ChieftainReactionCondition, ChieftainReactionConsequence, 100);

            // Total Success
            starter.AddDialogLine("nan_chieftain_result_total_success", "nan_chieftain_result", "nan_chieftain_release_accept_end",
                "{=nan_chieftain_accept}Your mercy... I didn't expect this. Very well, you have proven your honor. I will speak of your name with respect.",
                ChieftainTotalSuccessCondition, null, 100);

            starter.AddDialogLine("nan_chieftain_release_accept_end", "nan_chieftain_release_accept_end", "close_window",
                "{=nan_chieftain_accept_end}I am at your mercy, {?PLAYER.GENDER}my lady{?}my lord{\\?}!",
                null, NanChieftainReleaseConsequence, 100);

            // Total Fail
            starter.AddDialogLine("nan_chieftain_result_total_fail", "nan_chieftain_result", "nan_chieftain_release_refuse_end",
                "{=nan_chieftain_refuse}You will die before you ever become a Chieftain. Eventually, I will hold your head in my hands. Get lost, you lower-class scum!",
                ChieftainTotalFailCondition, null, 100);

            starter.AddDialogLine("nan_chieftain_release_refuse_end", "nan_chieftain_release_refuse_end", "close_window",
                "{=nan_chieftain_refuse_end}I will kill you, you will die!",
                null, NanChieftainRefuseConsequence, 100);

            // Round Continue
            starter.AddDialogLine("nan_chieftain_result_round_continue", "nan_chieftain_result", "nan_chieftain_persuasion_options",
                "{=nan_chieftain_round_continue}I'm still listening. Go on.",
                ChieftainRoundContinueCondition, null, 100);
        }

        private NanChieftainQuest GetActiveChieftainQuest()
        {
            return Campaign.Current.QuestManager.Quests.FirstOrDefault(q => q is NanChieftainQuest) as NanChieftainQuest;
        }

        private bool NanChieftainOfferCondition()
        {
            var quest = GetActiveChieftainQuest();
            if (quest == null || Hero.OneToOneConversationHero == null) return false;

            Hero lord = Hero.OneToOneConversationHero;

            if (quest.HasFailedPersuasion(lord)) return false;
            if (!lord.IsPrisoner || lord.PartyBelongedToAsPrisoner != PartyBase.MainParty) return false;
            if (lord.Clan == null || lord.Clan.Tier < 4) return false;

            return true;
        }

        private void StartChieftainPersuasion()
        {
            _chieftainSuccessfulRoundsThisAttempt = 0;
            _chieftainTotalAttemptsThisPersuasion = 0;
            _chieftainCurrentPage = 0;
            InitializeChieftainOptions();
            BuildChieftainTaskForCurrentPage();
            PersuasionDifficulty diff = (PersuasionDifficulty)Math.Max(0, Math.Min(5, NanSettingsManager.ChieftainPersuasionDifficulty));
            ConversationManager.StartPersuasion(ChieftainMaxScore, 1f, 0f, 2f, 2f, 0f, diff);
        }

        private PersuasionArgumentStrength GetChieftainStrengthForTraits(Hero target, bool isHard, SkillObject skill, params TraitObject[] traits)
        {
            int matchCount = 0;
            foreach (var t in traits)
            {
                if (t == DefaultTraits.Honor)
                {
                    if (target.GetTraitLevel(t) < 1) matchCount++;
                    if (Hero.MainHero.GetTraitLevel(t) >= 1) matchCount++; // Player needs High Honor
                }
                else if (t == DefaultTraits.Mercy)
                {
                    if (target.GetTraitLevel(t) >= 1) matchCount++; 
                    if (Hero.MainHero.GetTraitLevel(t) >= 1) matchCount++; // Player needs High Mercy
                }
                else
                {
                    if (target.GetTraitLevel(t) >= 1) matchCount++;
                    if (Hero.MainHero.GetTraitLevel(t) >= 1) matchCount++;
                }
            }
            if (skill != null && Hero.MainHero.GetSkillValue(skill) >= 100) matchCount++;
            if (skill != null && Hero.MainHero.GetSkillValue(skill) >= 200) matchCount++;

            if (isHard)
            {
                if (matchCount >= 5) return PersuasionArgumentStrength.Easy;
                if (matchCount >= 3) return PersuasionArgumentStrength.Normal;
                if (matchCount >= 1) return PersuasionArgumentStrength.Hard;
                return PersuasionArgumentStrength.ExtremelyHard;
            }
            else
            {
                if (traits.Length >= 2)
                {
                    if (matchCount >= 4) return PersuasionArgumentStrength.Easy;
                    if (matchCount >= 2) return PersuasionArgumentStrength.Normal;
                    return PersuasionArgumentStrength.Hard;
                }
                else
                {
                    if (matchCount >= 3) return PersuasionArgumentStrength.Easy;
                    if (matchCount >= 1) return PersuasionArgumentStrength.Normal;
                    return PersuasionArgumentStrength.Hard;
                }
            }
        }

        private void InitializeChieftainOptions()
        {
            Hero target = Hero.OneToOneConversationHero;
            _chieftainPagesOfOptions = new List<List<PersuasionOptionArgs>>();
            var rnd = new Random();

            for (int page = 0; page < 2; page++)
            {
                var pageOptions = new List<PersuasionOptionArgs>();
                bool isLastPage = (page == 1);
                
                int numMoves = isLastPage ? 0 : rnd.Next(1, 3);
                List<bool> moveFlags = new List<bool> { false, false, false };
                for (int i = 0; i < numMoves; i++) moveFlags[i] = true;
                moveFlags = moveFlags.OrderBy(x => rnd.Next()).ToList();

                if (page == 0)
                {
                    var s1 = GetChieftainStrengthForTraits(target, true, DefaultSkills.Medicine, DefaultTraits.Calculating, DefaultTraits.Honor, DefaultTraits.Valor);
                    pageOptions.Add(new PersuasionOptionArgs(DefaultSkills.Medicine, DefaultTraits.Mercy, TraitEffect.Positive, s1, true,
                        new TextObject("{=nan_chief_p1_hard}I could execute you right now. Instead, I offer life. A wise lord knows the value of my mercy."), null, false, moveFlags[0]));

                    var s2 = GetChieftainStrengthForTraits(target, false, DefaultSkills.Trade, DefaultTraits.Calculating, DefaultTraits.Generosity);
                    pageOptions.Add(new PersuasionOptionArgs(DefaultSkills.Trade, DefaultTraits.Calculating, TraitEffect.Positive, s2, false,
                        new TextObject("{=nan_chief_p1_n1}Think of your clan. An alliance with me is more profitable than dying in a dungeon."), null, false, moveFlags[1]));

                    var s3 = GetChieftainStrengthForTraits(target, false, DefaultSkills.Charm, DefaultTraits.Honor);
                    pageOptions.Add(new PersuasionOptionArgs(DefaultSkills.Charm, DefaultTraits.Honor, TraitEffect.Positive, s3, false,
                        new TextObject("{=nan_chief_p1_n2}Honor dictates that a life spared must be repaid with respect."), null, false, moveFlags[2]));
                }
                else
                {
                    var s1 = GetChieftainStrengthForTraits(target, true, DefaultSkills.Leadership, DefaultTraits.Calculating, DefaultTraits.Honor, DefaultTraits.Generosity);
                    pageOptions.Add(new PersuasionOptionArgs(DefaultSkills.Leadership, DefaultTraits.Honor, TraitEffect.Positive, s1, true,
                        new TextObject("{=nan_chief_p2_hard}Release means we are equals. Tell your kin that I am a chieftain worthy of their respect."), null, false, moveFlags[0]));

                    var s2 = GetChieftainStrengthForTraits(target, false, DefaultSkills.Tactics, DefaultTraits.Calculating, DefaultTraits.Honor);
                    pageOptions.Add(new PersuasionOptionArgs(DefaultSkills.Tactics, DefaultTraits.Calculating, TraitEffect.Positive, s2, false,
                        new TextObject("{=nan_chief_p2_n1}Don't let foolish pride blind you. A rational lord recognizes a new power when he sees it."), null, false, moveFlags[1]));

                    var s3 = GetChieftainStrengthForTraits(target, false, DefaultSkills.Roguery, DefaultTraits.Calculating);
                    pageOptions.Add(new PersuasionOptionArgs(DefaultSkills.Roguery, DefaultTraits.Calculating, TraitEffect.Positive, s3, false,
                        new TextObject("{=nan_chief_p2_n2}I have your life in the palm of my hand. Validate me, and you keep it."), null, false, moveFlags[2]));
                }

                _chieftainPagesOfOptions.Add(pageOptions.OrderBy(x => rnd.Next()).ToList());
            }
        }

        private void BuildChieftainTaskForCurrentPage()
        {
            _chieftainCurrentTask = new PersuasionTask(0);
            if (_chieftainCurrentPage < _chieftainPagesOfOptions.Count)
            {
                foreach (var opt in _chieftainPagesOfOptions[_chieftainCurrentPage])
                {
                    _chieftainCurrentTask.AddOptionToTask(opt);
                }
            }
        }

        private PersuasionTask GetCurrentChieftainTask() => _chieftainCurrentTask;

        private bool ShowChieftainTaskLineCondition()
        {
            MBTextManager.SetTextVariable("NAN_CHIEFTAIN_PERSUASION_TASK_LINE", TextObject.GetEmpty());
            return !ConversationManager.GetPersuasionProgressSatisfied() && !IsChieftainPersuasionFailed();
        }

        private bool IsChieftainPersuasionFailed()
        {
            return _chieftainTotalAttemptsThisPersuasion >= ChieftainMaxAttempts && !ConversationManager.GetPersuasionProgressSatisfied();
        }

        private bool SetupChieftainPersuasionOptionText(int optionIndex, string variableName)
        {
            PersuasionTask task = GetCurrentChieftainTask();
            if (task.Options.Count <= optionIndex)
                return false;

            MBTextManager.SetTextVariable(variableName, task.Options[optionIndex].Line);
            return true;
        }

        private void BlockChieftainCurrentOption(int optionIndex)
        {
            PersuasionTask task = GetCurrentChieftainTask();
            if (task.Options.Count > optionIndex)
                task.Options[optionIndex].BlockTheOption(true);
        }

        private PersuasionOptionArgs SetupChieftainPersuasionOption1() => GetCurrentChieftainTask().Options[0];
        private PersuasionOptionArgs SetupChieftainPersuasionOption2() => GetCurrentChieftainTask().Options[1];
        private PersuasionOptionArgs SetupChieftainPersuasionOption3() => GetCurrentChieftainTask().Options[2];

        private bool ChieftainOption1Clickable(out TextObject hintText) => ChieftainOptionClickable(0, out hintText);
        private bool ChieftainOption2Clickable(out TextObject hintText) => ChieftainOptionClickable(1, out hintText);
        private bool ChieftainOption3Clickable(out TextObject hintText) => ChieftainOptionClickable(2, out hintText);

        private bool ChieftainOptionClickable(int optionIndex, out TextObject hintText)
        {
            hintText = TextObject.GetEmpty();
            PersuasionTask task = GetCurrentChieftainTask();
            return task.Options.Count > optionIndex && !task.Options[optionIndex].IsBlocked;
        }

        private void ChieftainReactionConsequence()
        {
            var chosen = ConversationManager.GetPersuasionChosenOptions().Last();
            float successChance, critSuccessChance;
            Campaign.Current.Models.PersuasionModel.GetEffectChances(chosen.Item1, out successChance, out critSuccessChance, 0f);

            PersuasionTask task = GetCurrentChieftainTask();
            task.ApplyEffects(successChance, critSuccessChance);

            if (chosen.Item2 == PersuasionOptionResult.Success || chosen.Item2 == PersuasionOptionResult.CriticalSuccess)
                _chieftainSuccessfulRoundsThisAttempt++;

            _chieftainTotalAttemptsThisPersuasion++;

            if (chosen.Item1.CanMoveToTheNextReservation)
            {
                _chieftainCurrentPage++;
                BuildChieftainTaskForCurrentPage();
            }
            else
            {
                foreach (var option in task.Options)
                {
                    if (option == chosen.Item1) option.BlockTheOption(true);
                }
            }
        }

        private bool ChieftainReactionCondition()
        {
            if (!ConversationManager.GetPersuasionChosenOptions().Any()) return false;
            PersuasionOptionResult result = ConversationManager.GetPersuasionChosenOptions().Last().Item2;
            
            if (result == PersuasionOptionResult.Success || result == PersuasionOptionResult.CriticalSuccess)
            {
                MBTextManager.SetTextVariable("NAN_CHIEFTAIN_PERSUASION_REACTION", new TextObject("{=nan_chief_react_success}You make a fair point. Proceed."));
            }
            else
            {
                MBTextManager.SetTextVariable("NAN_CHIEFTAIN_PERSUASION_REACTION", new TextObject("{=nan_chief_react_fail}I am not convinced by such words."));
            }
            return true;
        }

        private bool ChieftainTotalSuccessCondition() => ConversationManager.GetPersuasionProgressSatisfied();
        private bool ChieftainTotalFailCondition() => IsChieftainPersuasionFailed();
        
        private bool ChieftainRoundContinueCondition()
        {
            return !ChieftainTotalSuccessCondition() && !ChieftainTotalFailCondition();
        }

        private void NanChieftainRefuseConsequence()
        {
            ConversationManager.EndPersuasion();
            var quest = GetActiveChieftainQuest();
            if (quest != null)
            {
                quest.AddFailedPersuasion(Hero.OneToOneConversationHero);
                quest.AddRelationHintLog(); // Keeps the original fail logic
            }
        }

        private void NanChieftainReleaseConsequence()
        {
            ConversationManager.EndPersuasion();
            var quest = GetActiveChieftainQuest();
            var lord = Hero.OneToOneConversationHero;

            if (quest != null && lord != null)
            {
                int points = 1;
                if (lord.Clan != null)
                {
                    if (lord.Clan.Tier == 5) points = 2;
                    else if (lord.Clan.Tier >= 6) points = 3;
                }

                quest.AddReleasePoints(points);
                EndCaptivityAction.ApplyByReleasedByChoice(lord);
            }
        }
    }
}

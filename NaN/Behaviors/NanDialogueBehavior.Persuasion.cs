using System;
using System.Collections.Generic;
using System.Linq;
using NaN.Quests;
using NaN.Config;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Conversation.Persuasion;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace NaN.Behaviors
{
    public partial class NanDialogueBehavior
    {
        private const float MaxScore = 3f;
        private const int MaxAttempts = 4;
        private int _successfulRoundsThisAttempt = 0;
        private int _totalAttemptsThisPersuasion = 0;
        private int _currentPage = 0;

        private List<List<PersuasionOptionArgs>> _pagesOfOptions;
        private PersuasionTask _currentTask;

        private void AddPersuasionDialogues(CampaignGameStarter starter)
        {
            var askLine = starter.AddPlayerLine("nan_bannerlord_ask", "hero_main_options", "nan_bannerlord_response_eval", 
                "{=nan_bannerlord_ask}My Lord, I believe I have proven myself. I ask that you recognize me and my clan as Nobility.",
                NanBannerlordAskCondition, null, 200);
            SetSpecialLine(askLine);

            starter.AddDialogLine("nan_bannerlord_cooldown", "nan_bannerlord_response_eval", "lord_pretalk",
                "{=nan_bannerlord_cooldown}I advise you not to anger me. We do not consider every vagabond a noble. Leave me be!",
                NanBannerlordCooldownCondition, null, 100);

            starter.AddDialogLine("nan_bannerlord_start", "nan_bannerlord_response_eval", "nan_bannerlord_persuasion_options",
                "{=nan_bannerlord_start}Very well. Tell me, what makes you worthy of this honor?",
                NanBannerlordStartCondition, StartPersuasion, 100);

            starter.AddDialogLine("nan_bannerlord_task_line", "nan_bannerlord_persuasion_options", "nan_bannerlord_persuasion_options",
                "{=!}{NAN_PERSUASION_TASK_LINE}", ShowCurrentTaskLineCondition, null, 100);

            starter.AddPlayerLine("nan_bannerlord_option_1", "nan_bannerlord_persuasion_options", "nan_bannerlord_reaction",
                "{=!}{NAN_PERSUASION_OPTION_1}",
                () => SetupPersuasionOptionText(0, "NAN_PERSUASION_OPTION_1"),
                () => BlockCurrentOption(0), 100, Option1Clickable, SetupPersuasionOption1);
                
            starter.AddPlayerLine("nan_bannerlord_option_2", "nan_bannerlord_persuasion_options", "nan_bannerlord_reaction",
                "{=!}{NAN_PERSUASION_OPTION_2}",
                () => SetupPersuasionOptionText(1, "NAN_PERSUASION_OPTION_2"),
                () => BlockCurrentOption(1), 100, Option2Clickable, SetupPersuasionOption2);
                
            starter.AddPlayerLine("nan_bannerlord_option_3", "nan_bannerlord_persuasion_options", "nan_bannerlord_reaction",
                "{=!}{NAN_PERSUASION_OPTION_3}",
                () => SetupPersuasionOptionText(2, "NAN_PERSUASION_OPTION_3"),
                () => BlockCurrentOption(2), 100, Option3Clickable, SetupPersuasionOption3);

            starter.AddDialogLine("nan_bannerlord_reaction", "nan_bannerlord_reaction", "nan_bannerlord_result",
                "{=!}{NAN_PERSUASION_REACTION}", ReactionCondition, ReactionConsequence, 100);

            // Total Success
            starter.AddDialogLine("nan_bannerlord_result_total_success", "nan_bannerlord_result", "lord_pretalk",
                "{=nan_bannerlord_total_success}I suppose you're right. You have proven your worth. I will consider you positively when the time comes.",
                TotalSuccessCondition, OnTotalSuccess, 100);

            // Total Fail
            starter.AddDialogLine("nan_bannerlord_result_total_fail", "nan_bannerlord_result", "lord_pretalk",
                "{=nan_bannerlord_total_fail}This is nonsense. Do not waste my time with this again.",
                TotalFailCondition, OnTotalFail, 100);

            // Round Continue
            starter.AddDialogLine("nan_bannerlord_result_round_continue", "nan_bannerlord_result", "nan_bannerlord_persuasion_options",
                "{=nan_bannerlord_round_continue}Is that all? Keep on. I'm still considering.",
                RoundContinueCondition, null, 100);
        }

        private bool ReactionCondition()
        {
            if (!ConversationManager.GetPersuasionChosenOptions().Any()) return false;
            PersuasionOptionResult result = ConversationManager.GetPersuasionChosenOptions().Last().Item2;
            
            if (result == PersuasionOptionResult.Success || result == PersuasionOptionResult.CriticalSuccess)
            {
                MBTextManager.SetTextVariable("NAN_PERSUASION_REACTION", new TextObject("{=nan_bannerlord_reaction_success}Fair point. You may really be eligible."));
            }
            else
            {
                MBTextManager.SetTextVariable("NAN_PERSUASION_REACTION", new TextObject("{=nan_bannerlord_reaction_fail}I don't think so. Not noble manners at all."));
            }
            return true;
        }

        private NanBannerlordQuest GetActiveBannerlordQuest()
        {
            return Campaign.Current.QuestManager.Quests.FirstOrDefault(q => q is NanBannerlordQuest) as NanBannerlordQuest;
        }

        private bool NanBannerlordAskCondition()
        {
            var quest = GetActiveBannerlordQuest();
            if (quest == null || Hero.OneToOneConversationHero == null || !Hero.OneToOneConversationHero.IsLord || Hero.OneToOneConversationHero.Clan == Clan.PlayerClan) return false;
            if (Hero.OneToOneConversationHero.IsPrisoner || Hero.MainHero.IsPrisoner) return false;
            if (Hero.OneToOneConversationHero.MapFaction != null && Hero.MainHero.MapFaction != null &&
                Hero.OneToOneConversationHero.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction)) return false;

            if (quest.IsLordPersuaded(Hero.OneToOneConversationHero)) return false; // Already persuaded

            return true;
        }

        private bool NanBannerlordCooldownCondition()
        {
            var quest = GetActiveBannerlordQuest();
            return quest != null && quest.IsLordOnCooldown(Hero.OneToOneConversationHero);
        }

        private bool NanBannerlordStartCondition()
        {
            return !NanBannerlordCooldownCondition();
        }

        private void StartPersuasion()
        {
            _successfulRoundsThisAttempt = 0;
            _totalAttemptsThisPersuasion = 0;
            _currentPage = 0;
            InitializeOptions();
            BuildTaskForCurrentPage();
            PersuasionDifficulty diff = (PersuasionDifficulty)Math.Max(0, Math.Min(5, NanSettingsManager.BannerlordPersuasionDifficulty));
            ConversationManager.StartPersuasion(MaxScore, 1f, 0f, 2f, 2f, 0f, diff);
        }

        private PersuasionArgumentStrength GetStrengthForTraits(Hero target, bool isHard, SkillObject skill, params TraitObject[] traits)
        {
            int matchCount = 0;
            foreach (var t in traits)
            {
                if (target.GetTraitLevel(t) > 0) matchCount++;
                if (Hero.MainHero.GetTraitLevel(t) > 0) matchCount++;
            }
            if (skill != null && Hero.MainHero.GetSkillValue(skill) >= 100) matchCount++;
            if (skill != null && Hero.MainHero.GetSkillValue(skill) >= 200) matchCount++;

            // Since matchCount can now be double (up to 6 for Hard, 4 for Normal) + up to 2 for skill
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

        private void InitializeOptions()
        {
            Hero target = Hero.OneToOneConversationHero;
            _pagesOfOptions = new List<List<PersuasionOptionArgs>>();
            var rnd = new Random();

            for (int page = 0; page < 3; page++)
            {
                var pageOptions = new List<PersuasionOptionArgs>();
                bool isLastPage = (page == 2);
                
                // Determine which options get MoveToNextReservation (1 or 2 options, only if not last page)
                int numMoves = isLastPage ? 0 : rnd.Next(1, 3);
                List<bool> moveFlags = new List<bool> { false, false, false };
                for (int i = 0; i < numMoves; i++) moveFlags[i] = true;
                moveFlags = moveFlags.OrderBy(x => rnd.Next()).ToList();

                if (page == 0)
                {
                    var s1 = GetStrengthForTraits(target, true, DefaultSkills.Leadership, DefaultTraits.Valor, DefaultTraits.Calculating, DefaultTraits.Honor);
                    pageOptions.Add(new PersuasionOptionArgs(DefaultSkills.Leadership, DefaultTraits.Valor, TraitEffect.Positive, s1, true,
                        new TextObject("{=nan_persuasion_p1_hard}I have bled on the field, calculated every maneuver, and kept my honor intact. A true lord respects these above all."), null, false, moveFlags[0]));

                    var s2 = GetStrengthForTraits(target, false, DefaultSkills.Trade, DefaultTraits.Calculating, DefaultTraits.Generosity);
                    pageOptions.Add(new PersuasionOptionArgs(DefaultSkills.Trade, DefaultTraits.Calculating, TraitEffect.Positive, s2, false,
                        new TextObject("{=nan_persuasion_p1_n1}For me, both gold and word have a standard. The reputation of an honest merchant crosses borders."), null, false, moveFlags[1]));

                    var s3 = GetStrengthForTraits(target, false, DefaultSkills.Charm, DefaultTraits.Honor);
                    pageOptions.Add(new PersuasionOptionArgs(DefaultSkills.Charm, DefaultTraits.Honor, TraitEffect.Positive, s3, false,
                        new TextObject("{=nan_persuasion_p1_n2}My word is my bond. My name is spoken with reverence in the surrounding villages."), null, false, moveFlags[2]));
                }
                else if (page == 1)
                {
                    var s1 = GetStrengthForTraits(target, true, DefaultSkills.Medicine, DefaultTraits.Mercy, DefaultTraits.Generosity, DefaultTraits.Honor);
                    pageOptions.Add(new PersuasionOptionArgs(DefaultSkills.Medicine, DefaultTraits.Mercy, TraitEffect.Positive, s1, true,
                        new TextObject("{=nan_persuasion_p2_hard}A true ruler protects the weak, feeds the hungry, and keeps his oaths. This is what makes me worthy."), null, false, moveFlags[0]));

                    var s2 = GetStrengthForTraits(target, false, DefaultSkills.Athletics, DefaultTraits.Valor, DefaultTraits.Honor);
                    pageOptions.Add(new PersuasionOptionArgs(DefaultSkills.Athletics, DefaultTraits.Valor, TraitEffect.Positive, s2, false,
                        new TextObject("{=nan_persuasion_p2_n1}The one who will bend my wrist in a fair fight has not yet been born."), null, false, moveFlags[1]));

                    var s3 = GetStrengthForTraits(target, false, DefaultSkills.Tactics, DefaultTraits.Calculating);
                    pageOptions.Add(new PersuasionOptionArgs(DefaultSkills.Tactics, DefaultTraits.Calculating, TraitEffect.Positive, s3, false,
                        new TextObject("{=nan_persuasion_p2_n2}War is a board, and I always play my pieces for victory. No trap surprises me."), null, false, moveFlags[2]));
                }
                else
                {
                    var s1 = GetStrengthForTraits(target, true, DefaultSkills.Tactics, DefaultTraits.Valor, DefaultTraits.Calculating, DefaultTraits.Generosity);
                    pageOptions.Add(new PersuasionOptionArgs(DefaultSkills.Tactics, DefaultTraits.Valor, TraitEffect.Positive, s1, true,
                        new TextObject("{=nan_persuasion_p3_hard}I bring courage to the line, wisdom to the council, and spoils to my followers. What else does nobility require?"), null, false, moveFlags[0]));

                    var s2 = GetStrengthForTraits(target, false, DefaultSkills.Steward, DefaultTraits.Mercy, DefaultTraits.Calculating);
                    pageOptions.Add(new PersuasionOptionArgs(DefaultSkills.Steward, DefaultTraits.Calculating, TraitEffect.Positive, s2, false,
                        new TextObject("{=nan_persuasion_p3_n1}The land belongs not only to the one who sows it, but to the one who protects it."), null, false, moveFlags[1]));

                    var s3 = GetStrengthForTraits(target, false, DefaultSkills.Roguery, DefaultTraits.Valor);
                    pageOptions.Add(new PersuasionOptionArgs(DefaultSkills.Roguery, DefaultTraits.Valor, TraitEffect.Positive, s3, false,
                        new TextObject("{=nan_persuasion_p3_n2}I do not hesitate to walk in the shadows and dirty my hands when necessary. I am my enemy's nightmare."), null, false, moveFlags[2]));
                }

                _pagesOfOptions.Add(pageOptions.OrderBy(x => rnd.Next()).ToList());
            }
        }

        private void BuildTaskForCurrentPage()
        {
            _currentTask = new PersuasionTask(0);
            if (_currentPage < _pagesOfOptions.Count)
            {
                foreach (var opt in _pagesOfOptions[_currentPage])
                {
                    _currentTask.AddOptionToTask(opt);
                }
            }
        }

        private PersuasionTask GetCurrentPersuasionTask() => _currentTask;

        private bool ShowCurrentTaskLineCondition()
        {
            MBTextManager.SetTextVariable("NAN_PERSUASION_TASK_LINE", TextObject.GetEmpty());
            return !ConversationManager.GetPersuasionProgressSatisfied() && !IsPersuasionFailed();
        }

        private bool IsPersuasionFailed()
        {
            return _totalAttemptsThisPersuasion >= MaxAttempts && !ConversationManager.GetPersuasionProgressSatisfied();
        }

        private bool SetupPersuasionOptionText(int optionIndex, string variableName)
        {
            PersuasionTask task = GetCurrentPersuasionTask();
            if (task.Options.Count <= optionIndex)
                return false;

            MBTextManager.SetTextVariable(variableName, task.Options[optionIndex].Line);
            return true;
        }

        private void BlockCurrentOption(int optionIndex)
        {
            PersuasionTask task = GetCurrentPersuasionTask();
            if (task.Options.Count > optionIndex)
                task.Options[optionIndex].BlockTheOption(true);
        }

        private PersuasionOptionArgs SetupPersuasionOption1() => GetCurrentPersuasionTask().Options[0];
        private PersuasionOptionArgs SetupPersuasionOption2() => GetCurrentPersuasionTask().Options[1];
        private PersuasionOptionArgs SetupPersuasionOption3() => GetCurrentPersuasionTask().Options[2];

        private bool Option1Clickable(out TextObject hintText) => OptionClickable(0, out hintText);
        private bool Option2Clickable(out TextObject hintText) => OptionClickable(1, out hintText);
        private bool Option3Clickable(out TextObject hintText) => OptionClickable(2, out hintText);

        private bool OptionClickable(int optionIndex, out TextObject hintText)
        {
            hintText = TextObject.GetEmpty();
            PersuasionTask task = GetCurrentPersuasionTask();
            return task.Options.Count > optionIndex && !task.Options[optionIndex].IsBlocked;
        }

        private void ReactionConsequence()
        {
            var chosen = ConversationManager.GetPersuasionChosenOptions().Last();
            float successChance, critSuccessChance;
            Campaign.Current.Models.PersuasionModel.GetEffectChances(chosen.Item1, out successChance, out critSuccessChance, 0f);

            PersuasionTask task = GetCurrentPersuasionTask();
            task.ApplyEffects(successChance, critSuccessChance);

            if (chosen.Item2 == PersuasionOptionResult.Success || chosen.Item2 == PersuasionOptionResult.CriticalSuccess)
                _successfulRoundsThisAttempt++;

            _totalAttemptsThisPersuasion++;

            if (chosen.Item1.CanMoveToTheNextReservation)
            {
                _currentPage++;
                BuildTaskForCurrentPage();
            }
            else
            {
                // Stay on current page, options remain the same (the chosen one is automatically blocked by ConversationManager or our logic)
                foreach (var option in task.Options)
                {
                    if (option == chosen.Item1) option.BlockTheOption(true);
                }
            }
        }

        private bool TotalSuccessCondition() => ConversationManager.GetPersuasionProgressSatisfied();
        private bool TotalFailCondition() => IsPersuasionFailed();
        
        private bool RoundContinueCondition()
        {
            return !TotalSuccessCondition() && !TotalFailCondition();
        }

        private void OnTotalSuccess()
        {
            ConversationManager.EndPersuasion();
            GetActiveBannerlordQuest()?.ApplyPersuasionResult(Hero.OneToOneConversationHero, true);
        }

        private void OnTotalFail()
        {
            ConversationManager.EndPersuasion();
            GetActiveBannerlordQuest()?.ApplyPersuasionResult(Hero.OneToOneConversationHero, false);
        }
    }
}

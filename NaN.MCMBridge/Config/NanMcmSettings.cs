using System;
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;
using NaN.Config;

namespace NaN.MCMBridge.Config
{
    public class NanMcmSettings : AttributeGlobalSettings<NanMcmSettings>
    {
        public override string Id => "NaN_v1";
        public override string DisplayName => "Not-a-Noble";
        public override string FolderName => "Not-a-Noble";
        public override string FormatType => "json2";

        [SettingPropertyBool("Mod Enabled", Order = 0, RequireRestart = false, HintText = "Enable or disable the main features of the mod.")]
        [SettingPropertyGroup("General")]
        public bool IsModEnabled { get; set; } = true;

        [SettingPropertyBool("Strict Dialogue", Order = 1, RequireRestart = false, HintText = "If enabled, high tier nobles will refuse to speak with you if your clan tier is too low.")]
        [SettingPropertyGroup("General")]
        public bool StrictDialogueEnabled { get; set; } = true;

        [SettingPropertyInteger("Peasant Dialogue Relation Threshold", -100, 100, "0", Order = 2, RequireRestart = false, HintText = "If your relation with a noble is higher than this value, they will not use dismissive 'peasant' farewells. Set to 10 by default.")]
        [SettingPropertyGroup("General")]
        public int PeasantDialogueRelationThreshold { get; set; } = 10;

        [SettingPropertyInteger("Freebooter: Required Relation", 5, 100, "0", Order = 1, RequireRestart = false, HintText = "Total relation with villagers required to complete Freebooter quest.")]
        [SettingPropertyGroup("Quest Difficulty")]
        public int FreebooterRequiredRelation { get; set; } = 30;

        [SettingPropertyInteger("Freebooter: Required Quests", 1, 10, "0", Order = 2, RequireRestart = false, HintText = "Number of different village quests you must complete.")]
        [SettingPropertyGroup("Quest Difficulty")]
        public int FreebooterRequiredQuests { get; set; } = 4;

        [SettingPropertyDropdown("Bannerlord: Persuasion Difficulty", Order = 3, RequireRestart = false, HintText = "Base difficulty for Bannerlord persuasion checks.")]
        [SettingPropertyGroup("Quest Difficulty")]
        public MCM.Common.Dropdown<string> BannerlordPersuasionDifficultyDropdown { get; set; } = new MCM.Common.Dropdown<string>(new string[] { "Very Easy", "Easy", "Medium", "Hard", "Very Hard", "Ultra Hard" }, 3);
        
        [SettingPropertyDropdown("Chieftain: Persuasion Difficulty", Order = 4, RequireRestart = false, HintText = "Base difficulty for Chieftain persuasion checks.")]
        [SettingPropertyGroup("Quest Difficulty")]
        public MCM.Common.Dropdown<string> ChieftainPersuasionDifficultyDropdown { get; set; } = new MCM.Common.Dropdown<string>(new string[] { "Very Easy", "Easy", "Medium", "Hard", "Very Hard", "Ultra Hard" }, 3);

        [SettingPropertyInteger("Bannerlord: Required Persuasions", 1, 10, "0", Order = 2, RequireRestart = false, HintText = "Number of lords you must persuade to complete Bannerlord quest.")]
        [SettingPropertyGroup("Quest Difficulty")]
        public int BannerlordRequiredPersuasions { get; set; } = 3;

        [SettingPropertyInteger("Chieftain: Required Captures", 1, 10, "0", Order = 3, RequireRestart = false, HintText = "Number of hostile lords you must capture to complete Chieftain quest.")]
        [SettingPropertyGroup("Quest Difficulty")]
        public int ChieftainRequiredCaptures { get; set; } = 2;

        [SettingPropertyInteger("Chieftain: Required Releases", 1, 10, "0", Order = 4, RequireRestart = false, HintText = "Number of hostile lords you must release after battle to complete Chieftain quest.")]
        [SettingPropertyGroup("Quest Difficulty")]
        public int ChieftainRequiredReleases { get; set; } = 2;

        [SettingPropertyInteger("Count: Required Army Defeats", 1, 5, "0", Order = 5, RequireRestart = false, HintText = "Number of enemy armies you must defeat to complete Count quest.")]
        [SettingPropertyGroup("Quest Difficulty")]
        public int CountRequiredArmyDefeats { get; set; } = 1;

        [SettingPropertyInteger("Count: Required Towns", 1, 5, "0", Order = 6, RequireRestart = false, HintText = "Number of towns you must hold to complete Count quest.")]
        [SettingPropertyGroup("Quest Difficulty")]
        public int CountRequiredTowns { get; set; } = 1;

        [SettingPropertyInteger("Sovereign: Required Towns", 1, 10, "0", Order = 7, RequireRestart = false, HintText = "Number of towns you must hold to complete Sovereign quest.")]
        [SettingPropertyGroup("Quest Difficulty")]
        public int SovereignRequiredTowns { get; set; } = 3;

        [SettingPropertyInteger("Sovereign: Required Castles", 1, 20, "0", Order = 8, RequireRestart = false, HintText = "Number of castles you must hold to complete Sovereign quest.")]
        [SettingPropertyGroup("Quest Difficulty")]
        public int SovereignRequiredCastles { get; set; } = 5;

        public NanJsonModel ToJsonModel()
        {
            return new NanJsonModel
            {
                IsModEnabled = this.IsModEnabled,
                StrictDialogueEnabled = this.StrictDialogueEnabled,
                PeasantDialogueRelationThreshold = this.PeasantDialogueRelationThreshold,
                FreebooterRequiredRelation = this.FreebooterRequiredRelation,
                FreebooterRequiredQuests = this.FreebooterRequiredQuests,
                BannerlordPersuasionDifficulty = this.BannerlordPersuasionDifficultyDropdown.SelectedIndex,
                ChieftainPersuasionDifficulty = this.ChieftainPersuasionDifficultyDropdown.SelectedIndex,
                BannerlordRequiredPersuasions = this.BannerlordRequiredPersuasions,
                ChieftainRequiredCaptures = this.ChieftainRequiredCaptures,
                ChieftainRequiredReleases = this.ChieftainRequiredReleases,
                CountRequiredArmyDefeats = this.CountRequiredArmyDefeats,
                CountRequiredTowns = this.CountRequiredTowns,
                SovereignRequiredTowns = this.SovereignRequiredTowns,
                SovereignRequiredCastles = this.SovereignRequiredCastles
            };
        }
    }
}

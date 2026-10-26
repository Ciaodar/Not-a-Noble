using System;
using Newtonsoft.Json;

namespace NaN.Config
{
    public class NanJsonModel
    {
        [JsonProperty("IsModEnabled")]
        public bool IsModEnabled { get; set; } = true;

        [JsonProperty("StrictDialogueEnabled")]
        public bool StrictDialogueEnabled { get; set; } = true;

        [JsonProperty("PeasantDialogueRelationThreshold")]
        public int PeasantDialogueRelationThreshold { get; set; } = 10;

        [JsonProperty("FreebooterRequiredRelation")]
        public int FreebooterRequiredRelation { get; set; } = 30;

        [JsonProperty("FreebooterRequiredQuests")]
        public int FreebooterRequiredQuests { get; set; } = 4;

        [JsonProperty("BannerlordPersuasionDifficulty")]
        public int BannerlordPersuasionDifficulty { get; set; } = 3; // 0=VeryEasy to 5=UltraHard

        [JsonProperty("ChieftainPersuasionDifficulty")]
        public int ChieftainPersuasionDifficulty { get; set; } = 3;

        [JsonProperty("BannerlordRequiredPersuasions")]
        public int BannerlordRequiredPersuasions { get; set; } = 3;

        [JsonProperty("ChieftainRequiredCaptures")]
        public int ChieftainRequiredCaptures { get; set; } = 2;

        [JsonProperty("ChieftainRequiredReleases")]
        public int ChieftainRequiredReleases { get; set; } = 2;

        [JsonProperty("CountRequiredArmyDefeats")]
        public int CountRequiredArmyDefeats { get; set; } = 1;

        [JsonProperty("CountRequiredTowns")]
        public int CountRequiredTowns { get; set; } = 1;

        [JsonProperty("SovereignRequiredTowns")]
        public int SovereignRequiredTowns { get; set; } = 3;

        [JsonProperty("SovereignRequiredCastles")]
        public int SovereignRequiredCastles { get; set; } = 5;
    }
}

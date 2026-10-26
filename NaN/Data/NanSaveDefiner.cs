using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;
using NaN.Quests;

namespace NaN.Data
{
    public class NanSaveDefiner : SaveableTypeDefiner
    {
        public NanSaveDefiner() : base(8451200) // Random unique namespace ID
        {
        }

        protected override void DefineClassTypes()
        {
            AddClassDefinition(typeof(NanFreebooterQuest), 1);
            AddClassDefinition(typeof(NanBannerlordQuest), 2);
            AddClassDefinition(typeof(NanChieftainQuest), 3);
            AddClassDefinition(typeof(NanCountQuest), 4);
            AddClassDefinition(typeof(NanDukeQuest), 5);
            AddClassDefinition(typeof(NanSovereignQuest), 6);
        }

        protected override void DefineContainerDefinitions()
        {
            ConstructContainerDefinition(typeof(List<string>));
            ConstructContainerDefinition(typeof(Dictionary<string, CampaignTime>));
        }
    }
}

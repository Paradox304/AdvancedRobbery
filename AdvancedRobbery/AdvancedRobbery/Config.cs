using Rocket.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedRobbery
{
    public class Config : IRocketPluginConfiguration
    {
        public int CommandCooldown { get; set; }
        public bool GroupMembersGetCooldown { get; set; }
        public float Radius { get; set; }
        public bool AutoDropItem { get; set; }
        public int HowManyItems { get; set; }
        public bool ShouldVictimBeSurrendered { get; set; }
        public int BaseChance { get; set; }
        public int WitnessDecreaseChance { get; set; }
        public int GroupMemberIncreaseChance { get; set; }
        public int MinimumChance { get; set; }
        public int MaximumChance { get; set; }

        public void LoadDefaults()
        {
            CommandCooldown = 10;
            GroupMembersGetCooldown = true;
            Radius = 10;
            AutoDropItem = true;
            HowManyItems = 2;
            ShouldVictimBeSurrendered = true;
            BaseChance = 50;
            WitnessDecreaseChance = 5;
            GroupMemberIncreaseChance = 5;
            MinimumChance = 30;
            MaximumChance = 70;
        }
    }
}

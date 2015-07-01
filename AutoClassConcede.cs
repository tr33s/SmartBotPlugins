using System;
using System.Collections.Generic;
using SmartBot.Plugins.API;

namespace SmartBot.Plugins
{
    [Serializable]
    public class bPluginDataContainer : PluginDataContainer
    {
        public Dictionary<string, bool> ConcedeDictionary;

        public bPluginDataContainer()
        {
            Name = "AutoClassConcede";
        }

        public bool Druid { get; set; }
        public bool Hunter { get; set; }
        public bool Mage { get; set; }
        public bool Paladin { get; set; }
        public bool Priest { get; set; }
        public bool Rogue { get; set; }
        public bool Shaman { get; set; }
        public bool Warlock { get; set; }
        public bool Warrior { get; set; }
    }

    public class bPlugin : Plugin
    {
        public override void OnPluginCreated()
        {
            var data = DataContainer as bPluginDataContainer;

            if (data == null || !data.Enabled)
            {
                //Bot.Log("RETURN");
                return;
            }

            data.ConcedeDictionary = new Dictionary<string, bool>
            {
                { "druid", data.Druid },
                { "hunter", data.Hunter },
                { "mage", data.Mage },
                { "paladin", data.Paladin },
                { "priest", data.Priest },
                { "rogue", data.Rogue },
                { "shaman", data.Shaman },
                { "warlock", data.Warlock },
                { "warrior", data.Warrior }
            };
        }

        // OnTurnBegin because Bot.CurrentBoard is null in OnGameBegin()
        public override void OnTurnBegin()
        {
            var data = DataContainer as bPluginDataContainer;
            var enemyClass = Bot.CurrentBoard.EnemyClass.ToString().ToLower();
            var badModes = new List<Bot.Mode> { Bot.Mode.Arena, Bot.Mode.ArenaAuto };

            if (data == null || !data.Enabled || badModes.Contains(Bot.CurrentMode()))
            {
                //Bot.Log("RETURN");
                return;
            }

            if (!data.ConcedeDictionary.ContainsKey(enemyClass))
            {
                //Bot.Log("RETURN2");
                return;
            }

            if (!data.ConcedeDictionary[enemyClass])
            {
                //Bot.Log(enemyClass);
                return;
            }

            Bot.Log("[PLUGIN] -> Concede against " + enemyClass);
            Bot.Concede();
        }
    }
}
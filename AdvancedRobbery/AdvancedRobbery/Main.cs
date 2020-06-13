using Rocket.API.Collections;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedRobbery
{
    public class Main : RocketPlugin<Config>
    {
        protected override void Load()
        {
            Instance = this;

            Logger.Log("AdvancedRobbery plugin has been loaded");
            Logger.Log("Version: 1.0");
            Logger.Log("Made by Paradox");
        }

        protected override void Unload()
        {
            Logger.Log("AdvancedRobbery plugin has been unloaded");
        }

        public override TranslationList DefaultTranslations => new TranslationList()
        {
            { "Cooldown", "This command is on cooldown" },
            { "PlayerNotFound", "Can't find player by the name you inputted" },
            { "VictimInGroup", "{0} is in your group" },
            { "VictimNotNearby", "{0} is not near you" },
            { "PlayerNotSurrender", "{0} has not surrendered yet" },
            { "RobberyFailed", "Robbery failed" },
            { "RobberySuccessfull", "Robbery successful" },
            { "NoItemsInInventory", "{0} didn't have any items in his inventory" },
            { "NotEnoughItems", "{0} didnt' have enough items to drop in his inventory" }

        };

        public void DropItems(Player victim, UnturnedPlayer robber)
        {
            List<DropItem> dropItems = new List<DropItem>();
            for (byte page = 0; page < PlayerInventory.PAGES; page++)
            {
                if (page == PlayerInventory.AREA)
                    continue;
                int itemCount = victim.inventory.getItemCount(page);
                for (byte index = 0; index < itemCount; index++)
                {
                    dropItems.Add(new DropItem(page, index, victim.inventory.getItem(page, index).item.id));
                }
            }

            if (dropItems.Count == 0)
            {
                UnturnedChat.Say(robber.CSteamID, Translate("NoItemsInInventory", victim.name));
                return;
            }

            if (dropItems.Count < Main.Instance.Configuration.Instance.HowManyItems)
            {
                
                UnturnedChat.Say(robber.CSteamID, Translate("NotEnoughItems", victim.name));
                foreach (DropItem item in dropItems)
                {
                    victim.inventory.removeItem(item.page, item.index);
                    ItemManager.dropItem(new Item(item.id, true), UnturnedPlayer.FromPlayer(victim).Position, true, true, true);
                }
                return;
            }

            Random random = new Random();
            for (int b = 0; b < Main.Instance.Configuration.Instance.HowManyItems; b++)
            {
                int randomNum = random.Next(0, dropItems.Count + 1);
                victim.inventory.removeItem(dropItems[randomNum].page, dropItems[randomNum].index);
                ItemManager.dropItem(new Item(dropItems[randomNum].id, true), UnturnedPlayer.FromPlayer(victim).Position, true, true, true);
            }
        }

        public static Main Instance { get; set; }
        public Dictionary<CSteamID, DateTime> cooldowns = new Dictionary<CSteamID, DateTime>();
    }
}

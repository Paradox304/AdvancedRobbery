using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedRobbery.Commands
{
    public class RobCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "rob";

        public string Help => "Rob players by using this command";

        public string Syntax => "/rob (player name)";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer robber = caller as UnturnedPlayer;

            // Check Syntax
            if (command.Length > 1 || command.Length == 0)
            {
                UnturnedChat.Say(caller, $"Correct Usage: {Syntax}");
                return;
            }

            // Check if player has a cooldown on the command
            if (Main.Instance.cooldowns.TryGetValue(robber.CSteamID, out DateTime expireDate))
            {
                if (DateTime.Now < expireDate)
                {
                    UnturnedChat.Say(caller, Main.Instance.Translate("Cooldown"));
                    return;
                } else
                {
                    Main.Instance.cooldowns.Remove(robber.CSteamID);
                }
            }

            // Check if player he's trying to rob is not in his group and is near him and is on the server
            if (!PlayerTool.tryGetSteamID(command[0], out CSteamID steamID))
            {
                UnturnedChat.Say(caller, Main.Instance.Translate("PlayerNotFound"));
                return;
            }
            Player victim = PlayerTool.getPlayer(steamID);
            List<Player> nearbyPlayers = new List<Player>();
            PlayerTool.getPlayersInRadius(robber.Position, Main.Instance.Configuration.Instance.Radius, nearbyPlayers);
            if (nearbyPlayers.Count == 0 || !nearbyPlayers.Contains(victim))
            {
                UnturnedChat.Say(caller, Main.Instance.Translate("VictimNotNearby", victim.name));
                return;
            }

            if (robber.Player.quests.groupID.ToString() != "0")
            {
                if (victim.quests.groupID == robber.Player.quests.groupID)
                {
                    UnturnedChat.Say(caller, Main.Instance.Translate("VictimInGroup", victim.name));
                    return;
                }
            }

            // Check if victim has his hands up
            if (Main.Instance.Configuration.Instance.ShouldVictimBeSurrendered)
            {
                if (victim.animator.gesture != EPlayerGesture.SURRENDER_START)
                {
                    UnturnedChat.Say(caller, Main.Instance.Translate("PlayerNotSurrender", victim.name));
                    return;
                }
            }

            // Get all of the group members involved in the robbery and all the witnesses
            List<Player> groupMembersNearby = new List<Player>();
            List<Player> witnessesNearby = new List<Player>();
            foreach (Player nearbyPlayer in nearbyPlayers)
            {
                if (nearbyPlayer == victim)
                    continue;
                if (nearbyPlayer == robber.Player)
                    continue;
                if (nearbyPlayer.quests.groupID == robber.Player.quests.groupID)
                {
                    groupMembersNearby.Add(nearbyPlayer);
                } else
                {
                    witnessesNearby.Add(nearbyPlayer);
                }
            }

            // Calculate the chance
            int chance = Main.Instance.Configuration.Instance.BaseChance;
            // First remove the witnesses chance and make sure minimum chance is there
            for (int b = 0; b < witnessesNearby.Count; b++)
            {
                int updatedChance = chance - Main.Instance.Configuration.Instance.WitnessDecreaseChance;
                if (updatedChance < Main.Instance.Configuration.Instance.MinimumChance)
                    break;
                chance = updatedChance;
            }
            // Then add the group members chance and make sure maximum chance is there
            for (int b = 0; b < groupMembersNearby.Count; b++)
            {
                int updatedChance = chance + Main.Instance.Configuration.Instance.GroupMemberIncreaseChance;
                if (updatedChance > Main.Instance.Configuration.Instance.MaximumChance)
                    break;
                chance = updatedChance;
            }
            UnturnedChat.Say(caller, chance.ToString());

            // Check if rob is successfull or not
            if (chance >= UnityEngine.Random.Range(1, 101))
            {
                if (Main.Instance.Configuration.Instance.AutoDropItem)
                {
                    Logger.Log("DropItems called");

                    Main.Instance.DropItems(victim, robber);
                }
                UnturnedChat.Say(caller, Main.Instance.Translate("RobberySuccessfull"));
                Main.Instance.cooldowns.Add(robber.CSteamID, DateTime.Now.AddSeconds(Main.Instance.Configuration.Instance.CommandCooldown));
                if (Main.Instance.Configuration.Instance.GroupMembersGetCooldown)
                {
                    if (groupMembersNearby.Count == 0)
                        return;
                    foreach (Player groupMember in groupMembersNearby)
                    {
                        if (Main.Instance.cooldowns.ContainsKey(groupMember.channel.owner.playerID.steamID))
                            continue;
                        Main.Instance.cooldowns.Add(groupMember.channel.owner.playerID.steamID, DateTime.Now.AddSeconds(Main.Instance.Configuration.Instance.CommandCooldown));
                    }
                }
            } else
            {
                UnturnedChat.Say(caller, Main.Instance.Translate("RobberyFailed"));
                Main.Instance.cooldowns.Add(robber.CSteamID, DateTime.Now.AddSeconds(Main.Instance.Configuration.Instance.CommandCooldown));
                if (Main.Instance.Configuration.Instance.GroupMembersGetCooldown)
                {
                    if (groupMembersNearby.Count == 0)
                        return;
                    foreach (Player groupMember in groupMembersNearby)
                    {
                        if (Main.Instance.cooldowns.ContainsKey(groupMember.channel.owner.playerID.steamID))
                            continue;
                        Main.Instance.cooldowns.Add(groupMember.channel.owner.playerID.steamID, DateTime.Now.AddSeconds(Main.Instance.Configuration.Instance.CommandCooldown));
                    }
                }
            }
        }
    }
}

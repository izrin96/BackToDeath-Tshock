using BackToDeath_TShock.DbManager;
using BackToDeath_TShock.Extensions;
using Microsoft.Xna.Framework;
using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace BackToDeath_TShock
{
    [ApiVersion(2, 1)]
    public class BackToDeath : TerrariaPlugin
    {
        public static IDbConnection Db { get; set; }
        public static HomeManager Homes { get; set; }

        public override string Name => "BackToDeath";

        public override Version Version => new Version(1, 1, 1);

        public override string Author => "izrin96";

        public override string Description => "To go back to the death point";

        public override string UpdateURL => base.UpdateURL;

        public BackToDeath(Main game) : base(game)
        {

        }

        public override void Initialize()
        {
            TShockAPI.Commands.ChatCommands.RemoveAll(a => a.Name == "home");
            Commands.ChatCommands.Add(new Command(cmdBack, "back", "b"));
            Commands.ChatCommands.Add(new Command(cmdHome, "home"));
            Commands.ChatCommands.Add(new Command(cmdDelHome, "delhome"));
            Commands.ChatCommands.Add(new Command(cmdSetHome, "sethome"));

            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
            ServerApi.Hooks.GamePostInitialize.Register(this, OnPostInitialize);
            ServerApi.Hooks.NetGetData.Register(this, OnGetData);
        }

        private void OnInitialize(EventArgs e)
        {
            Db = new SqliteConnection(
                "uri=file://" + Path.Combine(TShock.SavePath, "essentials.sqlite") + ",Version=3");
        }

        private void OnPostInitialize(EventArgs e)
        {
            Homes = new HomeManager(Db);
        }

        private List<string> everyTeleportCommands = new List<string>
        {
            "tp", "tppos", "tpnpc", "warp", "spawn", "home"
        };

        private void OnPlayerCommand(PlayerCommandEventArgs e)
        {
            if (e.Handled || e.Player == null)
            {
                return;
            }

            TSPlayer player = e.Player;

            if (everyTeleportCommands.Contains(e.CommandName))
            {
                player.GetPlayerInfo().pushLastLocation(player.TPlayer.position);
            }
        }

        private void cmdBack(CommandArgs e)
        {
            TSPlayer player = e.Player;
            PlayerInfo data = player.GetPlayerInfo();

            if (!player.GetPlayerInfo().canBack)
            {
                e.Player.SendErrorMessage("Tak boleh back lagi!");
                return;
            }

            Vector2 vector = data.popLastLocation();

            player.Teleport(vector.X, vector.Y);
            e.Player.SendSuccessMessage("Teleport to your last location");
        }

        private static async void cmdDelHome(CommandArgs e)
        {
            if (e.Parameters.Count > 1)
            {
                e.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}delhome <home name>", TShock.Config.CommandSpecifier);
                return;
            }

            string homeName = e.Parameters.Count == 1 ? e.Parameters[0] : "default";
            Home home = await BackToDeath.Homes.GetAsync(e.Player, homeName);
            if (home != null)
            {
                if (await BackToDeath.Homes.DeleteAsync(e.Player, homeName))
                {
                    e.Player.SendSuccessMessage("Deleted your home '{0}'.", homeName);
                }
                else
                {
                    e.Player.SendErrorMessage("Could not delete home, check logs for more details.");
                }
            }
            else
            {
                e.Player.SendErrorMessage("Invalid home '{0}'!", homeName);
            }
        }

        private static async void cmdSetHome(CommandArgs e)
        {
            if (e.Parameters.Count > 1)
            {
                e.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}sethome <home name>", TShock.Config.CommandSpecifier);
                return;
            }

            string homeName = e.Parameters.Count == 1 ? e.Parameters[0] : "default";
            if (await BackToDeath.Homes.GetAsync(e.Player, homeName) != null)
            {
                if (await BackToDeath.Homes.UpdateAsync(e.Player, homeName, e.Player.X, e.Player.Y))
                {
                    e.Player.SendSuccessMessage("Updated your home '{0}'.", homeName);
                }
                else
                {
                    e.Player.SendErrorMessage("Could not update home, check logs for more details.");
                }
                return;
            }

            if (await BackToDeath.Homes.AddAsync(e.Player, homeName, e.Player.X, e.Player.Y))
            {
                e.Player.SendSuccessMessage("Set your home '{0}'.", homeName);
            }
            else
            {
                e.Player.SendErrorMessage("Could not set home, check logs for more details.");
            }
        }

        private async void cmdHome(CommandArgs e)
        {
            if (e.Parameters.Count > 1)
            {
                e.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}home <home name>", TShock.Config.CommandSpecifier);
                return;
            }

            if (Regex.Match(e.Message, @"^\w+ -l(?:ist)?$").Success)
            {
                List<Home> homes = await BackToDeath.Homes.GetAllAsync(e.Player);
                e.Player.SendInfoMessage(homes.Count == 0 ? "You have no homes set." : "List of homes: {0}", string.Join(", ", homes.Select(h => h.Name)));
            }
            else
            {
                string homeName = e.Parameters.Count == 1 ? e.Parameters[0] : "default";
                Home home = await BackToDeath.Homes.GetAsync(e.Player, homeName);
                if (home != null)
                {
                    e.Player.Teleport(home.X, home.Y);
                    e.Player.SendSuccessMessage("Teleported you to your home '{0}'.", homeName);
                }
                else
                {
                    e.Player.SendErrorMessage("Invalid home '{0}'!", homeName);
                }
            }
        }

        private void OnGetData(GetDataEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }

            TSPlayer tsplayer = TShock.Players[e.Msg.whoAmI];
            if (tsplayer == null)
            {
                return;
            }

            PlayerInfo playerInfo = tsplayer.GetPlayerInfo();

            switch (e.MsgID)
            {
                case PacketTypes.PlayerDeathV2:
                    playerInfo.pushLastLocation(tsplayer.TPlayer.position);
                    break;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}

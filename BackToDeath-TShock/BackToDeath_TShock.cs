using BackToDeath_TShock.DbManager;
using BackToDeath_TShock.Extensions;
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

namespace BackToDeath_TShock
{
    [ApiVersion(2, 1)]
    public class BackToDeath : TerrariaPlugin
    {
        public static IDbConnection Db { get; set; }
        public static HomeManager Homes { get; set; }

        public override string Name => "BackToDeath";

        public override Version Version => new Version(1, 0, 0);

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

        private void cmdBack(CommandArgs args)
        {
            TSPlayer player = args.Player;
            PlayerInfo data = player.GetPlayerInfo();

            if (!player.GetPlayerInfo().isDeathYet)
            {
                args.Player.SendErrorMessage("Hang belum mampus!");
                return;
            }

            player.Teleport(data.lastLocation.X, data.lastLocation.Y);
        }

        public static async void cmdDelHome(CommandArgs e)
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

        public static async void cmdSetHome(CommandArgs e)
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

        private async void cmdHome(CommandArgs args)
        {
            if (args.Parameters.Count > 1)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}home <home name>", TShock.Config.CommandSpecifier);
                return;
            }

            if (Regex.Match(args.Message, @"^\w+ -l(?:ist)?$").Success)
            {
                List<Home> homes = await BackToDeath.Homes.GetAllAsync(args.Player);
                args.Player.SendInfoMessage(homes.Count == 0 ? "You have no homes set." : "List of homes: {0}", string.Join(", ", homes.Select(h => h.Name)));
            }
            else
            {
                string homeName = args.Parameters.Count == 1 ? args.Parameters[0] : "default";
                Home home = await BackToDeath.Homes.GetAsync(args.Player, homeName);
                if (home != null)
                {
                    args.Player.Teleport(home.X, home.Y);
                    args.Player.SendSuccessMessage("Teleported you to your home '{0}'.", homeName);
                }
                else
                {
                    args.Player.SendErrorMessage("Invalid home '{0}'!", homeName);
                }
            }
        }

        private void OnGetData(GetDataEventArgs args)
        {
            if (args.Handled)
            {
                return;
            }

            TSPlayer tsplayer = TShock.Players[args.Msg.whoAmI];
            if (tsplayer == null)
            {
                return;
            }

            PlayerInfo playerInfo = tsplayer.GetPlayerInfo();

            switch (args.MsgID)
            {
                case PacketTypes.PlayerDeathV2:
                    playerInfo.lastLocation = tsplayer.TPlayer.position;
                    playerInfo.isDeathYet = true;
                    break;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}

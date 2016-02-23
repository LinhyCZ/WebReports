using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace MOTDgd
{
    class CommandAd : IRocketCommand
    {
        public AllowedCaller AllowedCaller
        {
            get { return AllowedCaller.Player; }
        }

        public string Name
        {
            get
            {
                return "ad";
            }
        }

        public string Help
        {
            get
            {
                return "Generates link to advertisement page. After completion gives player reward.";
            }
        }

        public List<string> Aliases
        {
            get
            {
                return new List<string>() {};
            }
        }

        public string Syntax
        {
            get
            {
                return "";
            }
        }

        public List<string> Permissions
        {
            get
            {
                return new List<string>() { "ad" };
            }
        }

        public void Execute(IRocketPlayer caller, string[] command)
        {

            UnturnedPlayer player = (UnturnedPlayer)caller;
            if (player == null)
            {
                Logger.Log("This command cannot be called from the console.");
                return;
            }

            if (!Main.OnCooldown(player) && Main.Connected == true)
            {
                var shorten = Main.ShortenUrl("http://motdgd.com/motd/?user=" + Main.Instance.Configuration.Instance.User_ID + "&gm=minecraft&clt_user=" + player.CSteamID + "&srv_id=" + Main.Server_ID);
                if (shorten != "")
                {
                    UnturnedChat.Say(player, "For getting reward go to: " + shorten);
                }
                else
                {
                UnturnedChat.Say(player, "There was error with shortening URL. Contact your server administrator.");
                }
            }
            else if (Main.OnCooldown(player))
            {
                UnturnedChat.Say(player, "You are on cooldown.");
            }
            else if (Main.Connected == false)
            {
                UnturnedChat.Say(player, "There was error while connecting to HUB. Try again later.");
            }
            else
            {
                UnturnedChat.Say(player, "Error while processing your request.");
            }
        }
    }

    class CommandCooldown : IRocketCommand
    {
        public AllowedCaller AllowedCaller
        {
            get { return AllowedCaller.Player; }
        }

        public string Name
        {
            get
            {
                return "cooldown";
            }
        }

        public string Help
        {
            get
            {
                return "Tells player how much time is left before cooldown expiry.";
            }
        }

        public List<string> Aliases
        {
            get
            {
                return new List<string>() {};
            }
        }

        public string Syntax
        {
            get
            {
                return "";
            }
        }

        public List<string> Permissions
        {
            get
            {
                return new List<string>() { "cooldown" };
            }
        }

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;
            if (player == null)
            {
                Logger.Log("This command cannot be called from the console.");
                return;
            }

            var data = Main.CooldownTime(player);
            if (data != "")
            {
                UnturnedChat.Say(player, "You are on a cooldown for " + data);
            }
            else
            {
                UnturnedChat.Say(player, "You are not on a cooldown");
            }
        }
    }

    class CommandClearCooldownAll : IRocketCommand
    {
        public AllowedCaller AllowedCaller
        {
            get { return AllowedCaller.Both; }
        }

        public string Name
        {
            get
            {
                return "clearallcooldown";
            }
        }

        public string Help
        {
            get
            {
                return "Clears all cooldowns on the server.";
            }
        }

        public List<string> Aliases
        {
            get
            {
                return new List<string>() { };
            }
        }

        public string Syntax
        {
            get
            {
                return "";
            }
        }

        public List<string> Permissions
        {
            get
            {
                return new List<string>() { "clearall" };
            }
        }

        public void Execute(IRocketPlayer caller, string[] command)
        {
            Main.Cooldown.Clear();
            Logger.Log("Cooldown list cleared.");
        }
    }

    //Fix 
    class CommandClearCooldownPlayer : IRocketCommand
    {
        public AllowedCaller AllowedCaller
        {
            get { return AllowedCaller.Both; }
        }

        public string Name
        {
            get
            {
                return "clearcooldown";
            }
        }

        public string Help
        {
            get
            {
                return "Clears cooldown for player.";
            }
        }

        public List<string> Aliases
        {
            get
            {
                return new List<string>() { };
            }
        }

        public string Syntax
        {
            get
            {
                return "<player>";
            }
        }

        public List<string> Permissions
        {
            get
            {
                return new List<string>() { "clear" };
            }
        }

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;
            if (command.Length == 1)
            {
                UnturnedPlayer remPlayer = UnturnedPlayer.FromName(command[0]);
                if (remPlayer != null)
                {
                    Main.Cooldown.Remove(remPlayer.CSteamID);
                    Logger.Log("Cleared cooldown for player " + remPlayer.DisplayName);
                }
                Logger.LogWarning("Tried to clear cooldown for " + command[0] + " but he is not on the server!");
            }
            else
            {
                if(caller == null)
                {
                    Logger.Log("Wrong syntax of command");
                }
                else
                {
                    UnturnedChat.Say(player, "Wrong syntax of command");
                }
            }
        }
    }
}

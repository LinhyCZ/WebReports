using Quobject.SocketIoClientDotNet.Client;
using Rocket.Core.Plugins;
using Rocket.API;
using Rocket.Core.Logging;
using System.IO;
using System;
using Rocket.Unturned;
using Rocket.Unturned.Player;
using Rocket.Unturned.Chat;
using System.Net;
using System.Xml;
using System.Web;
using Steamworks;
using System.Collections.Generic;
using System.Timers;

namespace MOTDgd
{

    public class MOTDgdConfiguration : IRocketPluginConfiguration
    {
        public int User_ID;
        public bool AdvancedLogging;
        public string GiveItems;
        public int CooldownTime;
        public string bitlyName;
        public string bitlyAPIKey;

        public void LoadDefaults()
        {
            User_ID = 0;
            AdvancedLogging = false;
            GiveItems = " ";
            CooldownTime = 15;
            bitlyName = " ";
            bitlyAPIKey = " ";
        }

    }

    //Přidat podporu pro Uconomy a translation listy
    public class Main : RocketPlugin<MOTDgdConfiguration>
    {
        //Setting up variables
        public static int Server_ID;
        public static bool Connected;
        public static Dictionary<CSteamID, long> Cooldown = new Dictionary<CSteamID, long>();
        private Timer cooldownTimer;
        public static string User_ID;
        public static Main Instance;

        protected override void Load()
        {
            Instance = this;
            //Creating socket connection
            var socket = IO.Socket("http://mcnode.motdgd.com:8080");
            Logger.Log("Connecting to HUB");

            //Logging in to node
            socket.On("connect", () =>
            {
                Logger.Log("Connected to HUB");
                socket.Emit("login", new Object[0]);
                Connected = true;
            });

            //Reading Server ID
            socket.On("login_response", (arguments) =>
            {
                string login_data = arguments + "";
                int.TryParse(login_data, out Server_ID);
                Logger.Log("Received ID " + Server_ID + " from the HUB");
            });

            //Getting names of people that completed Advertisement
            socket.On("complete_response", (arguments) =>
            {
                string resp_data = arguments + "";
                UnturnedPlayer currentPlayer = getPlayer(resp_data);
                if (currentPlayer != null)
                {
                    if (Configuration.Instance.AdvancedLogging == true)
                    {
                        if (!OnCooldown(currentPlayer))
                        {
                            Logger.Log("User " + currentPlayer.DisplayName + " completed advertisement.");
                        }
                        else
                        {
                            Logger.Log("User " + currentPlayer.DisplayName + " completed advertisement, but is on cooldown");
                        }
                    }

                    if (!OnCooldown(currentPlayer))
                    {
                        GiveReward(currentPlayer);
                        var CooldownTime = CurrentTime.Millis + (Configuration.Instance.CooldownTime * 60 * 1000);
                        Cooldown.Add(currentPlayer.CSteamID, CooldownTime);
                    }
                    else
                    {
                        UnturnedChat.Say(currentPlayer, "You already received reward and now are on cooldown!");
                    }

                }
                else
                {
                    Logger.LogWarning("Player with CSteamID " + resp_data + " completed advertisement but is not on the server.");
                }
            });

            //Disconnecting from node
            socket.On("disconnect", () =>
            {
                Logger.LogWarning("Disconnected");
                Server_ID = 0;
                Connected = false;
            });

            //Telling player about rewards
            U.Events.OnPlayerConnected += (UnturnedPlayer player) =>
            {
                if (Connected == true && !OnCooldown(player))
                {
                    var shorten = ShortenUrl("http://motdgd.com/motd/?user=" + Configuration.Instance.User_ID + "&gm=minecraft&clt_user=" + player.CSteamID + "&srv_id=" + Server_ID);
                    if (shorten != "") {
                        UnturnedChat.Say(player, "For getting reward go to: " + shorten);
                    }
                    else
                    {
                        UnturnedChat.Say(player, "There was error with shortening URL. Contact your server administrator.");
                    }
                }
            };

            //Timer checking Cooldown players
            cooldownTimer = new System.Timers.Timer();
            cooldownTimer.Elapsed += new ElapsedEventHandler(timerFunc);
            cooldownTimer.Interval = 2000;
            cooldownTimer.Enabled = true;
        }


        //Converting URL to shorter vesion
        public static string ShortenUrl(string url) {
            string shortUrl = string.Empty;
            string statusTxt = string.Empty;

            using (WebClient wb = new WebClient())
            {
                string data = string.Format("http://api.bitly.com/v3/shorten/?login={0}&apiKey={1}&longUrl={2}&format={3}",
                Main.Instance.Configuration.Instance.bitlyName,
                Main.Instance.Configuration.Instance.bitlyAPIKey,
                HttpUtility.UrlEncode(url),
                "xml");

                //http://api.bitly.com/v3/shorten/?login=linhycz&apiKey=R_c3c1e8f0c9264a3ea072786226e37be5&longUrl=http://www.google.com&format=xml

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(wb.DownloadString(data));

                statusTxt = xmlDoc.GetElementsByTagName("status_txt")[0].InnerText;
                shortUrl = xmlDoc.GetElementsByTagName("url")[0].InnerText;
                if (statusTxt == "INVALID_APIKEY" || statusTxt == "MISSING_ARG_ACCESS_TOKEN")
                {
                    Logger.LogError("Your bitly API key is INVALID");
                    return "";
                }
                else if (statusTxt == "MISSING_ARG_LOGIN" || statusTxt == "INVALID_LOGIN")
                {
                    Logger.LogError("Your bitly username is INVALID");
                    return "";
                }
                else if (statusTxt == "OK")
                {
                    return shortUrl;
                }
                else
                {
                    Logger.LogError("Received error message from bitly. Error message: " + statusTxt);
                    return "";
                }
            }
        }

        //Get player variable from received CSteamID
        public UnturnedPlayer getPlayer(string id)
        {
            CSteamID new_ID = (CSteamID)UInt64.Parse(id);
            UnturnedPlayer player = UnturnedPlayer.FromCSteamID(new_ID);
            return player;
        }

        //Give Reward
        public void GiveReward (UnturnedPlayer player)
        {
            string[] itemList = Configuration.Instance.GiveItems.Split(';');
            foreach (string Item in itemList) {
                if (!Item.ToLower().Contains("heal") && !Item.ToLower().Contains("hp") && !Item.ToLower().Contains("experience") && !Item.ToLower().Contains("xp") && !Item.ToLower().Contains("vehicle") && !Item.ToLower().Contains("v"))
                {
                    //Give items
                    string[] details = Item.Split(' ');
                    if (details.Length == 2)
                    {
                        ushort id = 0;
                        byte count = 0;
                        bool checkID = ushort.TryParse(details[0], out id);
                        bool checkCount = byte.TryParse(details[1], out count);
                        if (checkID && checkCount)
                        {
                            player.GiveItem(id, count);
                        }
                        else
                        {
                            Logger.LogError("Error giving items to " + player.DisplayName + "! Error in " + Item);
                        }
                    }
                    else
                    {
                        Logger.LogError("Error giving items ot " + player.DisplayName + "! Error in " + Item);
                    }
                }
                else if (Item.ToLower().Contains("heal") || Item.ToLower().Contains("hp"))
                {
                    //Heal (how much HP to heal)
                    string[] details = Item.Split(' ');
                    if (details.Length == 2)
                    {
                        byte amount;
                        bool checkAmount = byte.TryParse(details[1], out amount);
                        if (checkAmount)
                        {
                            player.Heal(amount);
                        }
                        else
                        {
                            Logger.LogError("Error healing " + player.DisplayName + "! Error in " + Item);
                        }
                    }
                    else
                    {
                        Logger.LogError("Error healing " + player.DisplayName + "! Error in " + Item);
                    }
                }
                else if (Item.ToLower().Contains("experience") || Item.ToLower().Contains("xp"))
                {
                    string[] details = Item.Split(' ');
                    if (details.Length == 2)
                    {
                        uint experience;
                        bool checkXP = uint.TryParse(details[1], out experience);
                        if (checkXP)
                        {
                            player.Experience = player.Experience + experience;
                        }
                        else
                        {
                            Logger.LogError("Error giving xp to " + player.DisplayName + "! Error in " + Item);
                        }
                    }
                    else
                    {
                        Logger.LogError("Error giving xp to " + player.DisplayName + "! Error in " + Item);
                    }
                }
                else if (Item.ToLower().Contains("vehicle") || Item.ToLower().Contains("v"))
                {
                    string[] details = Item.Split(' ');
                    if (details.Length == 2)
                    {
                        ushort vehicleID;
                        bool checkID = ushort.TryParse(details[1], out vehicleID);
                        if (checkID)
                        {
                            bool check = player.GiveVehicle(vehicleID);
                            if (!check)
                            {
                                Logger.LogError("Error giving vehicle to " + player.DisplayName);
                            }
                        }
                        else
                        {
                            Logger.LogError("Error giving vehicle to " + player.DisplayName + "! Error in " + Item);
                        }
                    }
                    else
                    {
                        Logger.LogError("Error giving vehicle to " + player.DisplayName + "! Error in " + Item);
                    }
                } 
                else 
                {
                    Logger.LogError("Error giving items to " + player.DisplayName + "! Format of configuration is incorrect!");
                }
            }
            UnturnedChat.Say(player, "You got your reward! Now you are on cooldown for " + Configuration.Instance.CooldownTime + " minutes.");
        }

        private void timerFunc(object sender, EventArgs e)
        {
            RemoveCooldownLoop();
        }

        //Loop checking cooldown list and removing players after cooldown expiry 
        public void RemoveCooldownLoop()
        {
            foreach (var pair in Cooldown)
            {
                var key = pair.Key;
                var value = pair.Value;
                var currentTime = CurrentTime.Millis;

                if (value <= currentTime)
                {
                    Cooldown.Remove(key);
                    UnturnedPlayer player = UnturnedPlayer.FromCSteamID(key);
                    UnturnedChat.Say(player, "Your cooldown now expired!");
                }
            }
        }

        //Find if in Cooldown
        public static bool OnCooldown(UnturnedPlayer player)
        {
            if (Cooldown.ContainsKey(player.CSteamID))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //Return time in Millis since 1.1.1970
        static class CurrentTime
        {
            private static readonly DateTime Jan1St1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            public static long Millis { get { return (long)((DateTime.UtcNow - Jan1St1970).TotalMilliseconds); } }
        }

        //Return cooldown time
        public static string CooldownTime(UnturnedPlayer player)
        {
            foreach (var pair in Cooldown)
            {
                var key = pair.Key;
                var value = pair.Value;
                var currentTime = CurrentTime.Millis;

                if (key == player.CSteamID)
                {
                    var milTime = value - currentTime;
                    double time = milTime / 1000;
                    
                    var minutes = Math.Truncate(time / 60);
                    var seconds = time - (minutes * 60);
                    
                    return minutes + " minutes and " + seconds + " seconds";
                };
            }
            return "";
        }
    }
}

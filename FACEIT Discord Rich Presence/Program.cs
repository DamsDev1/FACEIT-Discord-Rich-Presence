using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using DiscordRPC;
using DiscordRPC.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Runtime.InteropServices;


namespace FACEIT_Discord_Rich_Presence
{
    public class FACEIT
    {
        public string discordClientID { get; set; }
        public string faceitPlayerID { get; set; }
        public string faceitAPIKey { get; set; }
        public bool showURLButton { get; set; }
        public string msgWaiting { get; set; }
        public string msgRanked { get; set; }
        public string msgMap { get; set; }
        public string msgScore { get; set; }
        public string msgSearching { get; set; }
        public string msgShowURLButton { get; set; }
        public string msgLeave { get; set; }

        public string msgLaunchWaiting { get; set; }

        public string msgVoting { get; set; }


        public string msgConfiguring { get; set; }
    }

    internal class Program
    {
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        static void Main(string[] args)
        {
            // deserialize JSON directly from a file
            using (StreamReader file = File.OpenText("config.json"))
            {
                var handle = GetConsoleWindow();

                ShowWindow(handle, SW_HIDE);

                JsonSerializer serializer = new JsonSerializer();
                FACEIT faceit = (FACEIT)serializer.Deserialize(file, typeof(FACEIT));

                bool inMatch = false;
                string state = faceit.msgWaiting;
                string matchID = "0";



                DiscordRPC.DiscordRpcClient client = new DiscordRPC.DiscordRpcClient(faceit.discordClientID);
                client.Logger = new ConsoleLogger() { Level = LogLevel.Warning };
                //Subscribe to events
                client.OnReady += (sender, e) =>
                {
                    //Console.WriteLine("Received Ready from user {0}", e.User.Username);
                };

                client.OnPresenceUpdate += (sender, e) =>
                {
                    //Console.WriteLine("Received Update! {0}", e.Presence);
                };

                //Connect to the RPC
                client.Initialize();

                //Set the rich presence
                //Call this as many times as you want and anywhere in your code.
                client.SetPresence(new RichPresence()
                {
                    Details = faceit.msgWaiting,
                    State = faceit.msgRanked,
                    Timestamps = Timestamps.Now,
                    Assets = new Assets()
                    {
                        LargeImageKey = "searching",
                        LargeImageText = faceit.msgSearching,
                    }
                });
                state = faceit.msgWaiting;
                DateTime timestamp_tmp = DateTime.UtcNow;
                async void checkPlayer()
                {
                    if (inMatch == false)
                    {
                        WebRequest webRequest = WebRequest.Create("https://api.faceit.com/match/v1/matches/groupByState?userId=" + faceit.faceitPlayerID);
                        HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
                        if (response.StatusDescription == "OK")
                        {
                            Stream dataStream = response.GetResponseStream();
                            StreamReader reader = new StreamReader(dataStream);
                            string responseFromServer = reader.ReadToEnd();
                            // Display the content.
                            dynamic data = JObject.Parse(responseFromServer);
                            if (data != null && data.payload != null)
                            {
                                if (data.payload.ONGOING != null && data.payload.ONGOING[0] != null)
                                {
                                    matchID = data.payload.ONGOING[0].id;
                                    inMatch = true;
                                    checkMatch();
                                }
                                else if (data.payload.READY != null && data.payload.READY[0] != null)
                                {
                                    matchID = data.payload.READY[0].id;
                                    inMatch = true;
                                    checkMatch();
                                }
                                else if (data.payload.CONFIGURING != null && data.payload.CONFIGURING[0] != null)
                                {
                                    matchID = data.payload.CONFIGURING[0].id;
                                    inMatch = true;
                                    checkMatch();
                                }
                                else if (data.payload.VOTING != null && data.payload.VOTING[0] != null)
                                {
                                    matchID = data.payload.VOTING[0].id;
                                    inMatch = true;
                                    checkMatch();
                                }

                            }
                            else
                            {
                                if (state != faceit.msgWaiting)
                                {
                                    state = faceit.msgWaiting;
                                    client.SetPresence(new RichPresence()
                                    {
                                        Details = faceit.msgWaiting,
                                        State = faceit.msgRanked,
                                        Timestamps = Timestamps.Now,
                                        Assets = new Assets()
                                        {
                                            LargeImageKey = "searching",
                                            LargeImageText = faceit.msgSearching,
                                        }
                                    });
                                }
                            }
                        }
                        await Task.Delay(60000);
                        checkPlayer();
                    }
                }
                async void checkMatch()
                {
                    if (inMatch && matchID != "0")
                    {
                        WebRequest myWebRequest = WebRequest.Create("https://open.faceit.com/data/v4/matches/" + matchID);
                        var myHttpWebRequest = (HttpWebRequest)myWebRequest;
                        myHttpWebRequest.PreAuthenticate = true;
                        myHttpWebRequest.Headers.Add("Authorization", "Bearer " + faceit.faceitAPIKey);
                        HttpWebResponse response = (HttpWebResponse)myWebRequest.GetResponse();
                        if (response.StatusDescription == "OK")
                        {
                            Stream dataStream = response.GetResponseStream();
                            StreamReader reader = new StreamReader(dataStream);
                            string responseFromServer = reader.ReadToEnd();
                            // Display the content.
                            dynamic data = JObject.Parse(responseFromServer);

                            if (data != null && (data.status == "ONGOING" || data.status == "READY" || data.status == "VOTING" || data.status == "CONFIGURING"))
                            {
                                if (data.status == "READY" || data.status == "ONGOING" || data.status == "CONFIGURING")
                                {
                                    // Getting name of the current map
                                    String nameMap = "";
                                    foreach (dynamic map in data.voting.map.entities)
                                    {
                                        if (map.class_name == data.voting.map.pick[0])
                                        {
                                            nameMap = map.name;
                                        }
                                    }

                                    // Getting starting date of match
                                    String dateStart = data.started_at;
                                    DateTime dateStartTime = UnixTimeStampToDateTime(Convert.ToDouble(data.started_at));
                                    dateStartTime = dateStartTime.ToUniversalTime();

                                    // Match started
                                    if (data.status == "ONGOING")
                                    {
                                        // Getting current score
                                        if (data.results != null && data.results.score != null && data.results.score.faction1 != null && data.results.score.faction2 != null)
                                        {
                                            String faction1 = data.results.score.faction1;
                                            String faction2 = data.results.score.faction2;
                                            client.SetPresence(new RichPresence()
                                            {
                                                Details = faceit.msgScore + ": " + faction1 + " - " + faction2,
                                                State = faceit.msgMap + ": " + nameMap,
                                                Timestamps = new Timestamps(dateStartTime),
                                                Buttons = (faceit.showURLButton) ? new DiscordRPC.Button[] { new DiscordRPC.Button() { Label = faceit.msgShowURLButton, Url = "https://www.faceit.com/en/csgo/room/" + data.match_id } } : null,
                                                Assets = new Assets()
                                                {
                                                    LargeImageKey = data.voting.map.pick[0],
                                                    LargeImageText = nameMap,
                                                    SmallImageKey = "competitive",
                                                    SmallImageText = faceit.msgRanked
                                                }
                                            });
                                            state = faceit.msgScore + ": " + faction1 + " - " + faction2;
                                        }
                                    }

                                    // Match waiting all players
                                    else if (data.status == "READY")
                                    {
                                        if (state != faceit.msgLaunchWaiting)
                                        {
                                            timestamp_tmp = DateTime.UtcNow;
                                        }
                                        client.SetPresence(new RichPresence()
                                        {
                                            Details = faceit.msgLaunchWaiting,
                                            State = faceit.msgMap + ": " + nameMap,
                                            Timestamps = new Timestamps(timestamp_tmp),
                                            Buttons = (faceit.showURLButton) ? new DiscordRPC.Button[] { new DiscordRPC.Button() { Label = faceit.msgShowURLButton, Url = "https://www.faceit.com/en/csgo/room/" + data.match_id } } : null,
                                            Assets = new Assets()
                                            {
                                                LargeImageKey = data.voting.map.pick[0],
                                                LargeImageText = nameMap,
                                                SmallImageKey = "competitive",
                                                SmallImageText = faceit.msgRanked
                                            }
                                        });
                                        state = faceit.msgLaunchWaiting;
                                    }

                                    // Match configuring map
                                    else
                                    {
                                        if (state != faceit.msgConfiguring)
                                        {
                                            timestamp_tmp = DateTime.UtcNow;
                                        }
                                        client.SetPresence(new RichPresence()
                                        {
                                            Details = faceit.msgConfiguring,
                                            State = faceit.msgMap + ": " + nameMap,
                                            Timestamps = new Timestamps(timestamp_tmp),
                                            Buttons = (faceit.showURLButton) ? new DiscordRPC.Button[] { new DiscordRPC.Button() { Label = faceit.msgShowURLButton, Url = "https://www.faceit.com/en/csgo/room/" + data.match_id } } : null,
                                            Assets = new Assets()
                                            {
                                                LargeImageKey = data.voting.map.pick[0],
                                                LargeImageText = nameMap,
                                                SmallImageKey = "competitive",
                                                SmallImageText = faceit.msgRanked
                                            }
                                        });
                                        state = faceit.msgConfiguring;
                                    }
                                }
                                else
                                {
                                    if (state != faceit.msgVoting)
                                    {
                                        timestamp_tmp = DateTime.UtcNow;
                                    }
                                    // Match voting map
                                    client.SetPresence(new RichPresence()
                                    {
                                        Details = faceit.msgVoting,
                                        State = null,
                                        Timestamps = new Timestamps(timestamp_tmp),
                                        Buttons = (faceit.showURLButton) ? new DiscordRPC.Button[] { new DiscordRPC.Button() { Label = faceit.msgShowURLButton, Url = "https://www.faceit.com/en/csgo/room/" + data.match_id } } : null,
                                        Assets = new Assets()
                                        {
                                            LargeImageKey = "competitive",
                                            LargeImageText = faceit.msgVoting,
                                            SmallImageKey = null,
                                            SmallImageText = null
                                        }
                                    });
                                    state = faceit.msgVoting;
                                }
                            }
                            else
                            {
                                inMatch = false;
                                matchID = "0";
                                state = "";
                                checkPlayer();
                            }
                        }
                        await Task.Delay(15000);
                        checkMatch();
                    }

                }
                checkPlayer();

                // Systray
                NotifyIcon trayIcon = new NotifyIcon();
                trayIcon.Text = "FACEIT Presence";
                trayIcon.Icon = new Icon(SystemIcons.Application, 40, 40);

                ContextMenu trayMenu = new ContextMenu();

                trayMenu.MenuItems.Add(faceit.msgLeave, leave);

                trayIcon.ContextMenu = trayMenu;
                trayIcon.Visible = true;
                Application.Run();
                Console.ReadKey(true);
            }
        }

        private static void leave(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
    }
}

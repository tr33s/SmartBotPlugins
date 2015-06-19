using System;
using System.IO;
using System.Net;
using System.Text;
using SmartBot.Plugins.API;

namespace SmartBot.Plugins
{
    [Serializable]
    public class bPluginDataContainer : PluginDataContainer
    {
        public bPluginDataContainer()
        {
            Name = "PushBullet";
        }

        public bool ArenaEnd { get; set; }
        public bool BotStopped { get; set; }
        public bool Defeat { get; set; }
        //public bool GameEnd { get; set; }
        public bool Victory { get; set; }
        public bool QuestsCompleted { get; set; }
        public string ApiKey { get; set; }
    }

    public class bPlugin : Plugin
    {
        public bPluginDataContainer Data;

        private bool Enabled
        {
            get { return Data != null && Data.Enabled; }
        }

        //Bot starting event
        public override void OnStarted()
        {
            Data = DataContainer as bPluginDataContainer;

            if (Data == null || !Enabled)
            {
                //Bot.Log("Disabled");
                return;
            }

            if (string.IsNullOrWhiteSpace(Data.ApiKey))
            {
                Bot.Log("[PushBullet] Please enter ApiKey");
                return;
            }

            PushBullet.ApiKey = Data.ApiKey;
            PushBullet.SendNote("SB Started", "Started @ " + string.Format("{0:HH:mm:ss tt}", DateTime.Now));
        }

        public override void OnDefeat()
        {
            if (!Enabled || !Data.Defeat)
            {
                return;
            }

            SendGameEnd("Defeat");
        }

        public override void OnVictory()
        {
            if (!Enabled || !Data.Victory)
            {
                return;
            }

            SendGameEnd("Victory");
        }

        public override void OnAllQuestsCompleted()
        {
            if (!Enabled || !Data.QuestsCompleted)
            {
                return;
            }

            PushBullet.SendNote("SB Quest Finished", "All Quests Completed!");
        }

        public static void SendGameEnd(string mode)
        {
            var data = Bot.GetPlayerDatas();
            var rank = data.GetRank();
            var stars = data.GetStars();

            var lostConcedes = Statistics.ConcededTotal - Statistics.Conceded;

            var totalGames = (double) (Statistics.Wins + Statistics.Losses);
            var winRatio = Math.Round(Statistics.Wins / totalGames * 100);

            var winsPerHour = Math.Round(Statistics.Wins / Statistics.ElapsedTime.TotalHours);

            var text =
                string.Format(
                    "[\"Current Rank: {0}\", \"Current Stars: {1}\", \"Won: {2}\", \"Lost: {3}\", \"Ratio: {4}%\", \"Wins/Hr: {5}\"]",
                    rank, stars, Statistics.Wins, Statistics.Losses, winRatio, winsPerHour);
            //Bot.Log(text);
            PushBullet.SendList("SB: " + mode, text);
        }
    }

    internal static class PushBullet
    {
        public static string ApiKey { get; set; }

        public static void SendNote(string title, string body)
        {
            var data =
                Encoding.ASCII.GetBytes(
                    string.Format("{{ \"type\": \"{0}\", \"title\": \"{1}\", \"body\": \"{2}\" }}", "note", title, body));

            data.GenerateAndSendRequest();
        }

        public static void SendList(string title, string items)
        {
            var data =
                Encoding.ASCII.GetBytes(
                    string.Format("{{ \"type\": \"{0}\", \"title\": \"{1}\", \"items\": {2} }}", "list", title, items));
            data.GenerateAndSendRequest();
        }

        private static void GenerateAndSendRequest(this byte[] data)
        {
            var request = WebRequest.Create("https://api.pushbullet.com/v2/pushes") as HttpWebRequest;

            if (request == null)
            {
                // Bot.Log("Request failed");
                return;
            }

            request.Method = "POST";
            request.ContentType = "application/json";
            request.Credentials = new NetworkCredential(ApiKey, "");
            request.ContentLength = data.Length;

            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(data, 0, data.Length);
                requestStream.Close();
            }

            using (var response = request.GetResponse() as HttpWebResponse)
            {
                if (response == null)
                {
                    return;
                }

                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var responseJson = reader.ReadToEnd();
                }
            }
        }
    }
}
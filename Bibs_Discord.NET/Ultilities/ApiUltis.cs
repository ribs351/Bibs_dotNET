using Discord;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Bibs_Discord_dotNET.Ultilities
{
    public static class ApiUltis
    {
        static string json;
        static JObject jsonParsed;

        static ApiUltis()
        {
            StreamReader sr;

            //Try to read configuration file
            try
            {
                sr = new StreamReader("appsettings.json");
                json = sr.ReadToEnd();
                jsonParsed = JObject.Parse(json);
            }
            catch (FileNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("appsettings.json not found! Exiting...");
                Environment.Exit(0);
            }
        }
        public static string ReturnSavedValue(string obj)
        {
            var valueToRetrieve = jsonParsed[obj];
            return (string)valueToRetrieve;
        }
        public static async Task<Embed> CreateBasicEmbed(string title, string description, Color color)
        {
            var embed = await Task.Run(() => (new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(description)
                .WithColor(color)
                .WithCurrentTimestamp()
                .Build()));
            return embed;
        }
        public static async Task<Embed> CreateErrorEmbed(string error)
        {
            var embed = await Task.Run(() => new EmbedBuilder()
                .WithDescription($"{error}")
                .WithColor(Color.DarkRed)
                .WithCurrentTimestamp()
                .Build());
            return embed;
        }
        public static async Task<string> HttpRequestAndReturnJson(HttpWebRequest request)
        {
            string httpResponse = null;

            try
            {
                //Create request to specified url
                using (HttpWebResponse httpWebResponse = (HttpWebResponse)(await request.GetResponseAsync()))
                {
                    //Process the response
                    using (Stream stream = httpWebResponse.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream))
                        httpResponse += await reader.ReadToEndAsync();
                }
            }
            catch (Exception e)
            {
                return await Task.FromException<string>(e);
            }
            //And if no errors occur, return the http response
            return await Task.FromResult(httpResponse);
        }
        public static bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }


    }
}

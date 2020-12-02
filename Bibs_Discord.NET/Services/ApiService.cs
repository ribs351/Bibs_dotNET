using Bibs_Discord_dotNET.Ultilities;
using Discord;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Bibs_Discord_dotNET.Services
{
    public sealed class ApiService
    {
        readonly string steamDevKey;

        public ApiService()
        {
            steamDevKey = ApiUltis.ReturnSavedValue("steamAPIkey");
        }
        public async Task<Embed> GetSteamInfoAsync(string userId)
        {
            string steamID64;

            //Variables
            string steamName, avatarUrl, profileUrl, countryCode, steamUserLevel;
            int onlineStatusGet, profileVisibilityGet;

            //Not the best way to verify if user is inputing the vanityURL or the SteamID, but it works
            //Verify if steam ID contains only numbers and is less than 17 digits long (steamID64 length)
            if (!ApiUltis.IsDigitsOnly(userId) && userId.Length < 17)
            {
                //If not, get steam id 64 based on user input
                steamID64 = await GetSteamId64(userId);
                if (steamID64 == "User not found")
                {
                    return await ApiUltis.CreateErrorEmbed("**User not found!** Please check your SteamID and try again.");
                }           
            }
            else
            {
                //If it is digits only, then assume the user input is the steam 64 id of a steam profile
                steamID64 = userId;
            }

            steamUserLevel = await GetSteamLevel(steamID64);

            try
            {
                //Create web request, requesting player profile info                
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key=" + steamDevKey + "&steamids=" + steamID64);

                string httpResponse = await ApiUltis.HttpRequestAndReturnJson(request);

                //Parse the json from httpResponse
                JObject profileJsonResponse = JObject.Parse(httpResponse);

                //Give values to the variables
                try
                {
                    steamName = (string)profileJsonResponse["response"]["players"][0]["personaname"];
                    avatarUrl = (string)profileJsonResponse["response"]["players"][0]["avatarfull"];
                    onlineStatusGet = (int)profileJsonResponse["response"]["players"][0]["personastate"];
                    profileUrl = (string)profileJsonResponse["response"]["players"][0]["profileurl"];
                    countryCode = (string)profileJsonResponse["response"]["players"][0]["loccountrycode"];
                    profileVisibilityGet = (int)profileJsonResponse["response"]["players"][0]["communityvisibilitystate"];
                }
                catch (Exception)
                {
                    return await ApiUltis.CreateErrorEmbed("**User not found!** Please check your SteamID and try again.");
                }

                //Online Status Switch
                string onlineStatus = null;
                switch (onlineStatusGet)
                {
                    case 0:
                        onlineStatus = "Offline";
                        break;
                    case 1:
                        onlineStatus = "Online";
                        break;
                    case 2:
                        onlineStatus = "Busy";
                        break;
                    case 3:
                        onlineStatus = "Away";
                        break;
                    case 4:
                        onlineStatus = "Snooze";
                        break;
                    case 5:
                        onlineStatus = "Looking to Trade";
                        break;
                    case 6:
                        onlineStatus = "Looking to Play";
                        break;
                }

                //Profile Visibility Switch
                string profileVisibility = null;
                switch (profileVisibilityGet)
                {
                    case 1:
                        profileVisibility = "Private";
                        break;
                    case 2:
                        profileVisibility = "Friends Only";
                        break;
                    case 3:
                        profileVisibility = "Public";
                        break;
                }


                if (countryCode == null)
                    countryCode = "Not found";

                var embed = new EmbedBuilder();
                embed.WithTitle(steamName + " Steam Info")
                    .WithDescription("\n**Steam Name:** " + steamName + "\n**Steam Level:** " + steamUserLevel  + "\n**Steam ID64:** " + steamID64 + "\n**Status:** " + onlineStatus + "\n**Profile Privacy:** " + profileVisibility + "\n**Country:** " + countryCode + "\n\n" + profileUrl)
                    .WithThumbnailUrl(avatarUrl)
                    .WithColor(Color.Blue)
                    .WithCurrentTimestamp();

                return embed.Build();
            }
            catch (WebException)
            {
                return await ApiUltis.CreateErrorEmbed("**An error ocurred**");
            }
        }

        //Retrieve steam id 64 based on userId.
        //Used to retrieve a valid steamId64 based on a vanity url.
        async Task<string> GetSteamId64(string userId)
        {
            string userIdResolved;

            //Create request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://api.steampowered.com/ISteamUser/ResolveVanityURL/v0001/?key=" + steamDevKey + "&vanityurl=" + userId);

            string httpResponse = await ApiUltis.HttpRequestAndReturnJson(request);

            //Save steamResponse in a string and then retrieve user steamId64
            try
            {
                JObject jsonParsed = JObject.Parse(httpResponse);
                userIdResolved = jsonParsed["response"]["steamid"].ToString();

                return userIdResolved;
            }
            catch (Exception)
            {
                return "User not found";
            }
        }

        //Retrieve steam level based on userId.
        //Used to retrieve the level of an account based on a valid steamId64.
        async Task<string> GetSteamLevel(string userId)
        {
            //Create a webRequest to steam api endpoint
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://api.steampowered.com/IPlayerService/GetSteamLevel/v1/?key=" + steamDevKey + "&steamid=" + userId);

            string httpResponse = await ApiUltis.HttpRequestAndReturnJson(request);

            //Save steamResponse in a string and then retrieve user level
            try
            {
                //Parse the json from httpResponse
                JObject jsonParsed = JObject.Parse(httpResponse);

                string userLevel = jsonParsed["response"]["player_level"].ToString();

                return userLevel;
            }
            catch (Exception)
            {
                return "Not found";
            }
        }
    }
}

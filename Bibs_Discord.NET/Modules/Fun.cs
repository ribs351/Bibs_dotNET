using Bibs_Infrastructure;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Bibs_Discord.NET.Modules
{
    public class Fun : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<Fun> _logger;

        public static bool NNN = false;

        public Fun(ILogger<Fun> logger)
        {
            _logger = logger;

        }
        [Command("birthday", RunMode = RunMode.Async)]
        [Summary("Posts the rats birthday mixtape lyrics")]
        public async Task Birthday([Remainder] string user = null)
        {
            if (user == null)

            {
                var builder = new EmbedBuilder()
                .WithTitle("It's your birthday today!")
                .WithDescription("Rats! Rats!\nWe are the rats!\nCelebrating yet another birthday bash!\nMICHAEL! It's your birthday today!\nCake n icecream is on its way\nMICHAEL! Such a good boy this year!\nOpen up your gifts while we all cheer!")
                .WithColor(new Color(33, 176, 252))
                .WithUrl("https://www.youtube.com/watch?v=vdVnnMOTe3Q")
                .WithCurrentTimestamp();
                var embed = builder.Build();
                await Context.Channel.TriggerTypingAsync();
                await Context.Channel.SendMessageAsync(null, false, embed);
            }
            else
            {
                var builder = new EmbedBuilder()
                .WithTitle($"It's {user.ToUpper()}'s birthday today")
                .WithDescription($"Rats! Rats!\nWe are the rats!\nCelebrating yet another birthday bash!\n{user.ToUpper()}! It's your birthday today!\nCake n icecream is on its way\n{user.ToUpper()}! Such a good boy this year!\nOpen up your gifts while we all cheer!")
                .WithColor(new Color(33, 176, 252))
                .WithUrl("https://www.youtube.com/watch?v=vdVnnMOTe3Q")
                .WithCurrentTimestamp();
                var embed = builder.Build();
                await Context.Channel.TriggerTypingAsync();
                await Context.Channel.SendMessageAsync(null, false, embed);
            }
        }

        [Command("meme", RunMode = RunMode.Async)]
        [Alias("reddit")]
        [Summary("Get a random post from a subreddit, default is r/memes")]
        public async Task Meme(string subreddit = null)
        {
            var client = new HttpClient();
            var result = await client.GetStringAsync($"https://reddit.com/r/{subreddit ?? "memes"}/random.json?limit=1");
            if (!result.StartsWith("["))
            {
                await Context.Channel.TriggerTypingAsync();
                await Context.Channel.SendMessageAsync("This subreddit doesn't exist!");
                return;
            }
            JArray arr = JArray.Parse(result);
            JObject post = JObject.Parse(arr[0]["data"]["children"][0]["data"].ToString());

            if (post["over_18"].ToString() == "True" && !(Context.Channel as ITextChannel).IsNsfw)
            {
                await ReplyAsync("L-Lewd!");
                return;
            }

            var builder = new EmbedBuilder()
                .WithImageUrl(post["url"].ToString())
                .WithColor(new Color(33, 176, 252))
                .WithTitle(post["title"].ToString())
                .WithUrl("https://reddit.com" + post["permalink"].ToString())
                .WithFooter($"🗨️ {post["num_comments"]} ⬆️ {post["ups"]}");
            await Context.Channel.TriggerTypingAsync();
            await ReplyAsync(embed: builder.Build());
        }
        [Command("md5", RunMode = RunMode.Async)]
        [Alias("hash")]
        [Summary("Turn user input into an MD5 hash")]
        public async Task MD5([Remainder] string input)
        {
            StringBuilder hash = new StringBuilder();
            MD5CryptoServiceProvider md5provider = new MD5CryptoServiceProvider();
            byte[] bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(input));

            for (int i = 0; i < bytes.Length; i++)
            {
                hash.Append(bytes[i].ToString("x2"));
            }
            await Context.Channel.TriggerTypingAsync();
            await ReplyAsync($"Your MD5 hash is: {hash.ToString()}");
        }
        [Command("mods")]
        [Summary("Promote Ribs's stellaris mods")]
        public async Task Mods()
        {
            var builder = new EmbedBuilder()
                .WithTitle("Rib's stellaris mods")
                .WithDescription("Go sub to these:")
                .WithColor(new Color(33, 176, 252))
                .AddField("The Tactician: ", "https://steamcommunity.com/sharedfiles/filedetails/?id=2165565232")
                .AddField("El Presidente: ", "TBA")
                .AddField("The Kingpin: ", "TBA");

            var embed = builder.Build();
            await Context.Channel.TriggerTypingAsync();
            await Context.Channel.SendMessageAsync(null, false, embed);
        }
        [Command("nukes")]
        [Summary("Gives you a random 6 digit number")]
        public async Task Nukes()
        {
            Random rnd = new Random();
            int codes = rnd.Next(1, 334763);

            if (NNN == true)
            {
                await Context.Channel.TriggerTypingAsync();
                await ReplyAsync("You can't use this now, No Nut November is in effect!");
                return;
            }
            else
            {
                if (!(Context.Channel as ITextChannel).IsNsfw)
                {
                    await ReplyAsync("You can't leak military secrets here!");
                    return;
                }

                var builder = new EmbedBuilder()
                   .WithColor(new Color(33, 176, 252))
                   .WithTitle($"Your nuke code is {codes}")
                   .WithUrl("https://nhentai.net/g/" + codes.ToString());
                await Context.Channel.TriggerTypingAsync();
                await ReplyAsync(embed: builder.Build());
            }
        }
        [Command("nnn")]
        [Summary("Activate No Nut November")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task NoNutNovember() 
        {
            NNN = !NNN;
            await Context.Channel.TriggerTypingAsync();
            await ReplyAsync($"Warning: No Nut November is now: {NNN.ToString()}!");
                
        }
    }
}

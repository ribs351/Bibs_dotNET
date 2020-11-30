﻿using Bibs_Discord_dotNET.Commons;
using Bibs_Infrastructure;
using Bibs_Discord_dotNET.Preconditions;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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

        public Fun(ILogger<Fun> logger)
        {
            _logger = logger;

        }
        [Command("birthday", RunMode = RunMode.Async)]
        [Summary("Posts the rats birthday mixtape lyrics")]
        [Cooldown(5)]
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
        [Command("joke", RunMode = RunMode.Async)]
        [Summary("Get a random dad joke")]
        [Cooldown(5)]
        public async Task Joke()
        {
            var client = new HttpClient();
            var result = await client.GetStringAsync("https://us-central1-dadsofunny.cloudfunctions.net/DadJokes/random/jokes");
            if (result == null)
            {
                await Context.Channel.SendErrorAsync("Dad Joke API", "Something went wrong with the Dad Joke API!");
                return;
            }
            var joke = JsonConvert.DeserializeObject<dynamic>(result);

            var builder = new EmbedBuilder()
               .WithColor(new Color(33, 176, 252))
               .WithTitle(joke.setup.ToString())
               .WithDescription(joke.punchline.ToString());
            await Context.Channel.TriggerTypingAsync();
            await ReplyAsync(embed: builder.Build());
        }
        [Command("meme", RunMode = RunMode.Async)]
        [Alias("reddit")]
        [Summary("Get a random post from a subreddit, default is r/memes")]
        [Cooldown(5)]
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
        [Cooldown(5)]
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
        [Cooldown(5)]
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
        [Command("rr", RunMode = RunMode.Async)]
        [Summary("Play a game of russian roulette")]
        [Cooldown(5)]
        public async Task RR()
        {
            if ((Context.Channel as IDMChannel) != null)
            {
                await Context.Channel.SendErrorAsync("Russian Roulette", "This command can only be used in a server, where the stakes are present.");
                return;
            }

            var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == "Muted"); // Fetch the role you're using to mute someone
            if (role == null) // Create the role if there is no muted role yet
            {
                var newRole = await Context.Guild.CreateRoleAsync("Muted", new GuildPermissions(sendMessages: false, addReactions: false, connect: false, speak: false), null, false, null);
                role = Context.Guild.Roles.FirstOrDefault(x => x.Id == newRole.Id);
            }

            foreach (var channel in Context.Guild.Channels) // Loop over all channels
            {
                if (!channel.GetPermissionOverwrite(role).HasValue ||                              // Check if the channel has the correct permissions for the muted role                             
                    channel.GetPermissionOverwrite(role).Value.SendMessages == PermValue.Allow ||  // If not, update the permissions of the role
                    channel.GetPermissionOverwrite(role).Value.AddReactions == PermValue.Allow ||
                    channel.GetPermissionOverwrite(role).Value.Connect == PermValue.Allow ||
                    channel.GetPermissionOverwrite(role).Value.Speak == PermValue.Allow)
                {
                    await channel.AddPermissionOverwriteAsync(role,
                        new OverwritePermissions(sendMessages: PermValue.Deny, addReactions: PermValue.Deny, connect: PermValue.Deny,
                            speak: PermValue.Deny));
                }
            }
            try
            {
                String reason = "";
                int bullet = new Random().Next(0, 5);
                if (bullet == 1)
                {
                    var channel = await Context.User.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync(reason == null ? $"You've been muted in {Context.Guild.Name} for 2 minutes. You've died in a game of Russian Roulette." : $"You've been muted in {Context.Guild.Name} for 2 minutes. You've died in a game of Russian Roulette. Try again if you dare.");
                    var user = (Context.User as SocketGuildUser);
                    await Task.Delay(2000);
                    await user.AddRoleAsync(role);
                    await Context.Channel.TriggerTypingAsync();
                    await ReplyAsync(reason == null ? $"The chamber was loaded! {Context.User.Username} shot themself in the head!" : $"The chamber was loaded! {Context.User.Username} shot themself in the head!");

                    await Task.Delay(120000);
                    await user.RemoveRoleAsync(role);
                    await channel.SendMessageAsync($"You've been revived in {Context.Guild.Name}");
                }
                else
                {
                    await Context.Channel.TriggerTypingAsync();
                    await ReplyAsync(reason == null ? $"The chamber was empty! {Context.User.Username} has survived!" : $"The chamber was empty! {Context.User.Username} has survived!");
                }
            }
            catch (Exception e)
            {
                await Context.Channel.SendErrorAsync("Russian Roulette", $"Something went wrong: User not found.");
            }
            

        }
        [Command("rps")]
        [Summary("Play a game of rock paper scissors")]
        [Cooldown(5)]
        public async Task RockRPS(string choice)
        {
            if (choice.Contains("paper") || choice.Contains("scissors") || choice.Contains("rock"))
            {
                int winner = new Random().Next(1, 4);
                if (winner == 1)
                {
                    Random random = new Random();
                    string[] responses = new string[]{
                        "Aww man, I lost!", 
                        "Dammit!",
                        "One more go, I'll get it next time!",
                        "I've lost? H-How? I had you!"
                    };
                    await Context.Channel.TriggerTypingAsync();
                    await ReplyAsync($"{responses[random.Next(0, responses.Length)]}");
                }
                else
                {
                    Random random = new Random();
                    string[] responses = new string[]{
                        "I've won! Hah!",
                        "Hehe, you lost this time.",
                        "You've done well to lose against me.",
                        "Outplayed! Don't feel bad, I'm just that great yknow?"
                    };
                    await Context.Channel.TriggerTypingAsync();
                    await ReplyAsync($"{responses[random.Next(0, responses.Length)]}");
                }
            }
            else
            {
                await ReplyAsync("Please select 'rock', 'paper' or 'scissors'.");
            }
        }

        [Command("coin")]
        [Summary("Coin toss")]
        [Cooldown(5)]
        public async Task coin()
        {   //generate a random number between 0 and 1 and assigns heads and tails to each
            int rolledNumber = new Random().Next(2);
            if (rolledNumber == 0)
            {
                await ReplyAsync("Heads");
            }
            else if (rolledNumber == 1)
            {
                await ReplyAsync("Tails");
            }
            else
            {
                await Context.Channel.SendErrorAsync("Error", "Sorry, an error has occured, please try again");
            }
        }
        [Command("dice")]
        [Summary("rolls a n-sided dice")]
        [Cooldown(5)]
        public async Task dice(int sides = 6)
        {
            if (sides >= 2 && sides <= 255)
            {
                int rolledNumber = new Random().Next(sides);
                await ReplyAsync($"You rolled {(rolledNumber + 1).ToString()}");
            }
            else
            {
                await Context.Channel.SendErrorAsync("Error", $"You've entered {sides}, please try again\nMin: 2 | max: 255!");
            }
        }
    }
}

using Bibs_Discord_dotNET.Ultilities;
using Bibs_Infrastructure;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bibs_Discord.NET.Modules
{
    public class General : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<General> _logger;
        private readonly RanksHelper _ranksHelper;

        public General(ILogger<General> logger, RanksHelper ranksHelper)
        {
            _logger = logger;
            _ranksHelper = ranksHelper;
        }

        [Command("ping")]
        [Summary("Check if Bibs is alive or not")]
        public async Task Ping()
        {
            await Context.Channel.TriggerTypingAsync();
            await Context.Channel.SendMessageAsync($"Took me {Context.Client.Latency} ms to think about it, but I'm alive.");
        }

        [Command("answer")]
        [Summary("Answers a yes no question, much like the magic eightball")]
        public async Task Answer([Remainder]string question)
        {
            Random random = new Random();
            string[] answers = { "Yes", "No", "Maybe", "dunno", "Yesn't", "Perhaps", "Possibly", "Positively","Conceivably", "I don't feel like answering right now, try again later", "In your dreams", "You sure you want to know the answer to that?", "*cricket noises*", "W-w-why would you ask that?" };
            if ((question.IndexOf("?", StringComparison.CurrentCultureIgnoreCase) >= 0) == false)
            {
                await Context.Channel.TriggerTypingAsync();
                await ReplyAsync("That's not a question though? Make a question pls!");
                return;
            }
            if ((question.IndexOf("are you cute", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
            {
                await Context.Channel.TriggerTypingAsync();
                await ReplyAsync("I'm the cutest!");
            }
            if (((question.IndexOf("are you a girl", StringComparison.CurrentCultureIgnoreCase) >= 0) == true) 
                || (question.IndexOf("are you boy or a girl", StringComparison.CurrentCultureIgnoreCase) >= 0)
                || (question.IndexOf("what is your gender", StringComparison.CurrentCultureIgnoreCase) >= 0)
                || (question.IndexOf("what's your gender", StringComparison.CurrentCultureIgnoreCase) >= 0))
            {
                await Context.Channel.TriggerTypingAsync();
                await ReplyAsync("I'm a boy!");
            }
            else if (((question.IndexOf("who is ribs", StringComparison.CurrentCultureIgnoreCase) >= 0) == true) 
                    || ((question.IndexOf("who's ribs", StringComparison.CurrentCultureIgnoreCase) >= 0) == true))
            {
                await Context.Channel.TriggerTypingAsync();
                await ReplyAsync("Some weeb who created me.");
            }
            else if ((question.IndexOf("are you gay", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
            {
                await Context.Channel.TriggerTypingAsync();
                await ReplyAsync("I'm not gay, Ribs made me dress like this!");
            }
            else if ((question.IndexOf("vtubers", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
            {
                await Context.Channel.TriggerTypingAsync();
                await ReplyAsync("idk, vtubers kinda bad ngl");

            }
            else if ((question.IndexOf("city hunter", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
            {
                await Context.Channel.TriggerTypingAsync();
                await ReplyAsync("no idea, i only know that city hunter is bad");
            }
            else if ((question.IndexOf("ryo saeba", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
            {
                await Context.Channel.TriggerTypingAsync();
                await ReplyAsync("idk, i think Ryo Saeba is a pervert");
            }
            else if ((question.IndexOf("what is the definition of insanity", StringComparison.CurrentCultureIgnoreCase) >= 0) == true
                || (question.IndexOf("what's the definition of insanity", StringComparison.CurrentCultureIgnoreCase) >= 0) == true
                || (question.IndexOf("did i ever tell you what the definition of insanity is", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
            {
                await Context.Channel.TriggerTypingAsync();
                await ReplyAsync("Mentioning one Far Cry game, over and over again");
            }
            else
            {
                await Context.Channel.TriggerTypingAsync();
                await ReplyAsync(answers[random.Next(0, answers.Length)]);
            }
            
        }

        [Command("avatar")]
        [Summary("Get a user's avatar")]
        public async Task Avatar([Remainder] SocketGuildUser user = null)
        {
           
            if (user == null)
            {
                await Context.Channel.TriggerTypingAsync();
                await Context.Channel.SendMessageAsync($"Your avatar is:\n{Context.User.GetAvatarUrl(size: 1024, format: Discord.ImageFormat.Png) ?? Context.User.GetDefaultAvatarUrl()}");
            }
            else
            {
                await Context.Channel.TriggerTypingAsync();
                await Context.Channel.SendMessageAsync($"{user.Username}'s avatar is:\n{user.GetAvatarUrl(size: 1024, format: Discord.ImageFormat.Png) ?? user.GetDefaultAvatarUrl()}");
            }

        }
        [Command("info")]
        [Summary("Get information on either yourself or another user's")]
        public async Task Info([Remainder] SocketGuildUser user = null)
        {
            if (user == null)

            {
                var builder = new EmbedBuilder()
                .WithThumbnailUrl(Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl())
                .WithTitle("Your Files")
                .WithDescription("Here's what I've found out about you: ")
                .WithColor(new Color(33, 176, 252))
                .AddField("User ID", Context.User.Id, true)
                .AddField("Created at", Context.User.CreatedAt.ToString("dd/MM/yyyy"), true)
                .AddField("Join date", (Context.User as SocketGuildUser).JoinedAt.Value.ToString("dd/MM/yyyy"), true)
                .AddField("Roles", string.Join(" ", (Context.User as SocketGuildUser).Roles.Select(x => x.Mention)))
                .WithCurrentTimestamp();
                var embed = builder.Build();
                await Context.Channel.TriggerTypingAsync();
                await Context.Channel.SendMessageAsync(null, false, embed);
            }
            else
            {
                var builder = new EmbedBuilder()
               .WithThumbnailUrl(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
               .WithTitle($"{user}'s Files")
               .WithDescription($"{user.Username}'s infomation is as follows: ")
               .WithColor(new Color(33, 176, 252))
               .AddField("User ID", user.Id)
               .AddField("Created at", user.CreatedAt.ToString("dd/MM/yyyy"))
               .AddField("Join date", user.JoinedAt.Value.ToString("dd/MM/yyyy"))
               .AddField("Roles", string.Join(" ", user.Roles.Select(x => x.Mention)))
               .WithCurrentTimestamp();
                var embed = builder.Build();
                await Context.Channel.TriggerTypingAsync();
                await Context.Channel.SendMessageAsync(null, false, embed);
            }

        }
        [Command("serverinfo")]
        [Summary("Get server info")]
        public async Task Server()
        {
            var builder = new EmbedBuilder()
                .WithThumbnailUrl(Context.Guild.IconUrl)
                .WithTitle($"{Context.Guild.Name}'s Infomation")
                .WithDescription("This is the server we're in:")
                .WithColor(new Color(33, 176, 252))
                .AddField("Created at: ", Context.Guild.CreatedAt.ToString("dd/MM/yyyy"))
                .AddField("Member Count ", (Context.Guild as SocketGuild).MemberCount + " member(s)")
                .AddField("Online users: ", (Context.Guild as SocketGuild).Users.Where(x => x.Status != UserStatus.Offline).Count());

            var embed = builder.Build();
            await Context.Channel.TriggerTypingAsync();
            await Context.Channel.SendMessageAsync(null, false, embed);
        }
        [Command("getrank", RunMode = RunMode.Async)]
        [Summary("Assign a rank to yourself")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task Rank([Remainder] string identifier)
        {
            await Context.Channel.TriggerTypingAsync();
            var ranks = await _ranksHelper.GetRanksAsync(Context.Guild);

            IRole role;


            if (ulong.TryParse(identifier, out ulong roleId))
            {
                var roleById = Context.Guild.Roles.FirstOrDefault(x => x.Id == roleId);
                if (roleById == null)
                {
                    EmbedBuilder embedBuilder = new EmbedBuilder()
                         .WithThumbnailUrl(Context.Guild.IconUrl)
                         .WithTitle("Ranks")
                         .WithColor(new Color(33, 176, 252))
                         .WithDescription("That role does not exist!");

                    Embed embed1 = embedBuilder.Build();
                    await Context.Channel.TriggerTypingAsync();
                    await Context.Channel.SendMessageAsync(null, false, embed1);
                    return;
                }

                role = roleById;
            }
            else
            {
                var roleByName = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, identifier, StringComparison.CurrentCultureIgnoreCase));
                if (roleByName == null)
                {
                    EmbedBuilder embedBuilder = new EmbedBuilder()
                        .WithThumbnailUrl(Context.Guild.IconUrl)
                        .WithTitle("Ranks")
                        .WithColor(new Color(33, 176, 252))
                        .WithDescription("That role does not exist!");

                    Embed embed2 = embedBuilder.Build();
                    await Context.Channel.TriggerTypingAsync();
                    await Context.Channel.SendMessageAsync(null, false, embed2);
                    return;
                }

                role = roleByName;
            }

            if (ranks.Any(x => x.Id != role.Id))
            {
                EmbedBuilder embedBuilder = new EmbedBuilder()
                    .WithThumbnailUrl(Context.Guild.IconUrl)
                    .WithTitle("Ranks")
                    .WithColor(new Color(33, 176, 252))
                    .WithDescription("That rank does not exist!");

                Embed embed3 = embedBuilder.Build();
                await Context.Channel.TriggerTypingAsync();
                await Context.Channel.SendMessageAsync(null, false, embed3);
                return;
            }

            if ((Context.User as SocketGuildUser).Roles.Any(x => x.Id == role.Id))
            {
                await (Context.User as SocketGuildUser).RemoveRoleAsync(role);
                EmbedBuilder embedBuilder = new EmbedBuilder()
                    .WithThumbnailUrl(Context.Guild.IconUrl)
                    .WithTitle("Ranks")
                    .WithColor(new Color(33, 176, 252))
                    .WithDescription($"Succesfully removed the rank {role.Mention} from you.");

                Embed embed4 = embedBuilder.Build();
                await Context.Channel.TriggerTypingAsync();
                await Context.Channel.SendMessageAsync(null, false, embed4);
                return;
            }

            await (Context.User as SocketGuildUser).AddRoleAsync(role);
            var builder = new EmbedBuilder()
                   .WithThumbnailUrl(Context.Guild.IconUrl)
                   .WithTitle("Ranks")
                   .WithColor(new Color(33, 176, 252))
                   .WithDescription($"Succesfully added the rank {role.Mention} from you.");

            var embed = builder.Build();
            await Context.Channel.TriggerTypingAsync();
            await Context.Channel.SendMessageAsync(null, false, embed);
           
        }
    }
}

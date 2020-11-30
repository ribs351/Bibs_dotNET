using Bibs_Discord_dotNET.Commons;
using Bibs_Discord_dotNET.Ultilities;
using Bibs_Discord_dotNET.Preconditions;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bibs_Discord.NET.Modules
{
    public class General : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<General> _logger;
        private readonly ServerHelper _serverHelper;

        public General(ILogger<General> logger, ServerHelper serverHelper)
        {
            _logger = logger;
            _serverHelper = serverHelper;
        }

        [Command("ping")]
        [Summary("Check if Bibs is alive or not")]

        public async Task Ping()
        {
            await Context.Channel.TriggerTypingAsync();
            await Context.Channel.SendMessageAsync($"Took me {Context.Client.Latency} ms to think about it, but I'm alive.");
        }

        [Command("getbibs")]
        [Summary("Gets an invite link for Bibs")]
        public async Task GetBibs()
        {
            await Context.Channel.TriggerTypingAsync();
            await Context.Channel.SendSuccessAsync("Want to invite me?", "Use this link to invite me to your server: https://discord.com/oauth2/authorize?client_id=767616736941309962&scope=bot&permissions=8");
        }

        [Command("owo")]
        [Summary("OwO")]
        [Cooldown(5)]
        public async Task OWO([Remainder]string phrase)
        {
            String str = phrase.Replace("r", "w");
            str = str.Replace("l", "w");
            await ReplyAsync($"*" + str + " uwu*");
        }
        [Command("avatar")]
        [Summary("Get a user's avatar")]
        [Cooldown(5)]
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
        [Cooldown(5)]
        public async Task Info([Remainder] SocketGuildUser user = null)
        {
            if ((Context.Channel as IDMChannel) != null)
            {
                var builder = new EmbedBuilder()
                .WithThumbnailUrl(Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl())
                .WithTitle("Your Files")
                .WithDescription("Here's what I've found out about you: ")
                .WithColor(new Color(33, 176, 252))
                .AddField("User ID", $"```fix\n{Context.User.Id}```", true)
                .AddField("Created at", $"```fix\n{Context.User.CreatedAt.ToString("dd/MM/yyyy")}```", true)
                .WithCurrentTimestamp()
                .WithFooter(
                x =>
                {
                    x.WithText($"Info | Requested by {Context.User.Username}");
                    x.WithIconUrl(Context.User.GetAvatarUrl());
                }); 
                var embed = builder.Build();
                await Context.Channel.TriggerTypingAsync();
                await Context.Channel.SendMessageAsync(null, false, embed);
                return;
            }
            if (user == null)

            {
                var builder = new EmbedBuilder()
                .WithThumbnailUrl(Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl())
                .WithTitle("Your Files")
                .WithDescription("Here's what I've found out about you: ")
                .WithColor(new Color(33, 176, 252))
                .AddField("User ID", $"```fix\n{Context.User.Id}```", true)
                .AddField("Created at", $"```fix\n{Context.User.CreatedAt.ToString("dd/MM/yyyy")}```", true)
                .AddField("Join date", $"```fix\n{(Context.User as SocketGuildUser).JoinedAt.Value.ToString("dd/MM/yyyy")}```", true)
                .AddField("Roles", string.Join(" ", (Context.User as SocketGuildUser).Roles.Select(x => x.Mention)))
                .WithCurrentTimestamp()
                .WithFooter(
                x =>
                {
                    x.WithText($"Info | Requested by {Context.User.Username}");
                    x.WithIconUrl(Context.User.GetAvatarUrl());
                });
                var embed = builder.Build();
                await Context.Channel.TriggerTypingAsync();
                await Context.Channel.SendMessageAsync(null, false, embed);
                return;
            }
            else
            {
                var builder = new EmbedBuilder()
               .WithThumbnailUrl(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
               .WithTitle($"{user}'s Files")
               .WithDescription($"{user.Username}'s infomation is as follows: ")
               .WithColor(new Color(33, 176, 252))
               .AddField("User ID", user.Id, true)
               .AddField("Created at", $"```fix\n{user.CreatedAt.ToString("dd/MM/yyyy")}```", true)
               .AddField("Join date", $"```fix\n{user.JoinedAt.Value.ToString("dd/MM/yyyy")}```", true)
               .AddField("Roles", string.Join(" ", user.Roles.Select(x => x.Mention)))
               .WithCurrentTimestamp()
               .WithFooter(
                x =>
                {
                    x.WithText($"Info | Requested by {Context.User.Username}");
                    x.WithIconUrl(Context.User.GetAvatarUrl());
                });
                var embed = builder.Build();
                await Context.Channel.TriggerTypingAsync();
                await Context.Channel.SendMessageAsync(null, false, embed);
                return;
            }

        }
        [Command("serverinfo")]
        [Summary("Get server info")]
        [RequireContext(ContextType.Guild, ErrorMessage = "You need to be in a discord server to use this commands!")]
        [Cooldown(5)]
        public async Task Server()
        {
            var builder = new EmbedBuilder()
                .WithThumbnailUrl(Context.Guild.IconUrl)
                .WithTitle($"{Context.Guild.Name}'s Infomation")
                .WithDescription("This is the server we're in:")
                .WithColor(new Color(33, 176, 252))
                .AddField("Created at: ", $"```fix\n{Context.Guild.CreatedAt.ToString("dd/MM/yyyy")}```", true)
                .AddField("Member Count ", $"```fix\n{(Context.Guild as SocketGuild).MemberCount} member(s)```", true)
                .AddField("Online users: ", $"```fix\n{(Context.Guild as SocketGuild).Users.Where(x => x.Status != UserStatus.Offline).Count()}```", true)
                .WithCurrentTimestamp()
                .WithFooter(
                x =>
                {
                    x.WithText($"Server info | Requested by {Context.User.Username}");
                    x.WithIconUrl(Context.User.GetAvatarUrl());
                });

            var embed = builder.Build();
            await Context.Channel.TriggerTypingAsync();
            await Context.Channel.SendMessageAsync(null, false, embed);
        }
        [Command("getrank", RunMode = RunMode.Async)]
        [Summary("Assign a rank to yourself")]
        [RequireContext(ContextType.Guild, ErrorMessage = "You need to be in a discord server to use this commands!")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task Rank([Remainder] string identifier)
        {
            await Context.Channel.TriggerTypingAsync();
            var ranks = await _serverHelper.GetRanksAsync(Context.Guild);

            IRole role;


            if (ulong.TryParse(identifier, out ulong roleId))
            {
                var roleById = Context.Guild.Roles.FirstOrDefault(x => x.Id == roleId);
                if (roleById == null)
                {
                    await Context.Channel.TriggerTypingAsync();
                    await Context.Channel.SendErrorAsync("Ranks", "That role doesn't exist!");
                    return;
                }

                role = roleById;
            }
            else
            {
                var roleByName = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, identifier, StringComparison.CurrentCultureIgnoreCase));
                if (roleByName == null)
                {
                    await Context.Channel.TriggerTypingAsync();
                    await Context.Channel.SendErrorAsync("Ranks", "That role doesn't exist!");
                    return;
                }

                role = roleByName;
            }

            if (!ranks.Any(x => x.Id == role.Id))
            {
                await Context.Channel.TriggerTypingAsync();
                await Context.Channel.SendErrorAsync("Ranks", "That rank doesn't exist!");
                return;
            }

            if ((Context.User as SocketGuildUser).Roles.Any(x => x.Id == role.Id))
            {
                await (Context.User as SocketGuildUser).RemoveRoleAsync(role);
                await Context.Channel.TriggerTypingAsync();
                await Context.Channel.SendSuccessAsync("Ranks", $"Succesfully removed the rank {role.Mention} from you.");
                return;
            }

            await (Context.User as SocketGuildUser).AddRoleAsync(role);
            await Context.Channel.TriggerTypingAsync();
            await Context.Channel.SendSuccessAsync("Ranks", $"Succesfully added the rank {role.Mention} to you.");
            return;

        }
    }
}

using Bibs_Discord_dotNET.Commons;
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
               .AddField("User ID", user.Id, true)
               .AddField("Created at", user.CreatedAt.ToString("dd/MM/yyyy"), true)
               .AddField("Join date", user.JoinedAt.Value.ToString("dd/MM/yyyy"), true)
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
                    await Context.Channel.TriggerTypingAsync();
                    await Context.Channel.SendSuccessAsync("Ranks", "That role doesn't exist!");
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
                    await Context.Channel.SendSuccessAsync("Ranks", "That role doesn't exist!");
                    return;
                }

                role = roleByName;
            }

            if (ranks.Any(x => x.Id != role.Id))
            {
                await Context.Channel.TriggerTypingAsync();
                await Context.Channel.SendSuccessAsync("Ranks", "That rank doesn't exist!");
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

        }
    }
}

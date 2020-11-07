using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Bibs_Infrastructure;
using Bibs_Discord_dotNET.Ultilities;
using Microsoft.EntityFrameworkCore.Internal;
using Discord.Rest;
using Bibs_Discord_dotNET.Commons;

namespace Bibs_Discord.NET.Modules
{
    public class Moderation : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<Moderation> _logger;
        private readonly RanksHelper _ranksHelper;
        private readonly AutoRolesHelper _autoRolesHelper;
        private readonly Servers _servers;
        private readonly Ranks _ranks;
        private readonly AutoRoles _autoRoles;

        private readonly GuildPermissions mutedPerms = new GuildPermissions(sendMessages: false);

        public Moderation(ILogger<Moderation> logger, RanksHelper ranksHelper, Servers servers, Ranks ranks, AutoRolesHelper autoRolesHelper, AutoRoles autoRoles)
        {
            _logger = logger;
            _ranksHelper = ranksHelper;
            _autoRolesHelper = autoRolesHelper;
            _servers = servers;
            _ranks = ranks;
            _autoRoles = autoRoles;
        }

        [Command("say"), Alias("s")]
        [Summary("Make Bibs say what you want")]
        public async Task Say([Remainder] string text)
        {
            await Context.Message.DeleteAsync();
            await Context.Channel.TriggerTypingAsync();
            await ReplyAsync(text);
        }
        [Command("mute", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageMessages, ErrorMessage = "You don't have permission to do that!")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [Summary("Toggle mute a user")]
        public async Task Mute([Remainder] SocketGuildUser user)
        {
            if (user.Hierarchy > ((SocketGuildUser)Context.User).Hierarchy ||    // Check whether the bot has a higher hierarchy than the user
                user.Hierarchy > Context.Guild.CurrentUser.Hierarchy)             // If not, return an error.
            {
                var builder = new EmbedBuilder()
                 .WithThumbnailUrl(Context.Guild.IconUrl)
                 .WithTitle("Permission denied")
                 .WithColor(new Color(33, 176, 252))
                 .WithDescription("You can't mute a user that has the same or a higher position than you or me.")
                 .WithCurrentTimestamp();

                var embed = builder.Build();
                await Context.Channel.TriggerTypingAsync();
                await Context.Channel.SendMessageAsync(null, false, embed);
                return;
            }

            await Context.Channel.TriggerTypingAsync(); // Trigger typing so your user knows you're working on it
            var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == "Muted"); // Fetch the role you're using to mute someone
            if (role == null) // Create the role if there is no muted role yet
            {
                var newRole = await Context.Guild.CreateRoleAsync("Muted", new GuildPermissions(sendMessages: false, addReactions: false, connect: false, speak: false), null, false, null);
                role = Context.Guild.Roles.FirstOrDefault(x => x.Id == newRole.Id);
            }

            if (role.Position > Context.Guild.CurrentUser.Hierarchy) // Return an error when the role has a higher position than the bot
            {
                var builder = new EmbedBuilder()
                 .WithThumbnailUrl(Context.Guild.IconUrl)
                 .WithTitle("Permission denied")
                 .WithColor(new Color(33, 176, 252))
                 .WithDescription($"I can't assign the role {role.Mention} to the user because the role has a higher position than me.")
                 .WithCurrentTimestamp();

                var embed = builder.Build();
                await Context.Channel.SendMessageAsync(null, false, embed);
                return;
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

            if (user.Roles.Any(x => x.Name == "Muted")) // Check if the user already has the muted role
            {
                await user.RemoveRoleAsync(role);

                var builder = new EmbedBuilder()
                 .WithThumbnailUrl(Context.Guild.IconUrl)
                 .WithTitle("Unmute")
                 .WithColor(new Color(33, 176, 252))
                 .WithDescription($"{user.Mention} is no longer muted.")
                 .WithCurrentTimestamp();

                var embed = builder.Build();
                await Context.Channel.SendMessageAsync(null, false, embed);
                return;
            }

            await user.AddRoleAsync(role); // Add the role to the user
            EmbedBuilder embedBuilder = new EmbedBuilder()
                         .WithThumbnailUrl(Context.Guild.IconUrl)
                         .WithTitle("Muted")
                         .WithColor(new Color(33, 176, 252))
                         .WithDescription($"Muted {user.Mention} in all text and voice channels.")
                         .WithCurrentTimestamp();

            Embed embed1 = embedBuilder.Build();
            await Context.Channel.SendMessageAsync(null, false, embed1);
        }

        [Command("nick")]
        [Summary("Change a user's nickname")]
        [RequireUserPermission(GuildPermission.ManageNicknames, ErrorMessage = "You don't have permission to do that!")]
        [RequireBotPermission(GuildPermission.ManageNicknames)]
        public async Task Nick(SocketGuildUser user, [Remainder] string name)
        {
            var builder = new EmbedBuilder()
                 .WithThumbnailUrl(Context.Guild.IconUrl)
                 .WithTitle("Nickname")
                 .WithColor(new Color(33, 176, 252))
                 .WithDescription($"{user.Mention} I changed your name to **{name}**")
                 .AddField("Reason:", $"{Context.User.Mention} invoked the command")
                 .WithCurrentTimestamp();

            var embed = builder.Build();
            await Context.Channel.TriggerTypingAsync();
            await Context.Channel.SendMessageAsync(null, false, embed);
        }

        [Command("prune")]
        [Alias("purge", "delet", "kill")]
        [Summary("Bulk delete messages")]
        [RequireUserPermission(GuildPermission.ManageMessages, ErrorMessage = "You don't have permission to do that!")]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task Prune(int amount)
        {
            await Context.Channel.TriggerTypingAsync();
            var startmessage = await Context.Channel.SendMessageAsync("Okay, purging now...");
            await Task.Delay(500);
            await startmessage.DeleteAsync();


            var messages = await Context.Channel.GetMessagesAsync(amount + 1).FlattenAsync();
            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);

            await Context.Channel.TriggerTypingAsync();
            var message = await Context.Channel.SendMessageAsync($"{messages.Count()} messages has been deleted");
            await Task.Delay(2500);
            await message.DeleteAsync();

            _logger.LogInformation($"{Context.User.Username} executed the purge command!");

        }
        [Command("kick")]
        [Summary("Kick a user")]
        [RequireUserPermission(GuildPermission.KickMembers, ErrorMessage = "You don't have permission to do that!")]
        [RequireBotPermission(GuildPermission.KickMembers)]
        public async Task Kick([Remainder] SocketGuildUser user)
        {
            await Context.Channel.TriggerTypingAsync();
            await user.KickAsync();
            await Context.Channel.SendSuccessAsync("Kicked", $"{user.Mention} was kicked by {Context.User.Mention}!");
        }
        [Command("ban")]
        [Summary("Ban a user")]
        [RequireUserPermission(GuildPermission.BanMembers, ErrorMessage = "You don't have permission to do that!")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task Ban([Remainder] SocketGuildUser user)
        {
            await Context.Channel.TriggerTypingAsync();
            await user.BanAsync();
            await Context.Channel.SendSuccessAsync("Banned", $"{user.Mention} was banned by {Context.User.Mention}!");
            
        }
        [Command("prefix", RunMode = RunMode.Async)]
        [Summary("Set the bot's command prefix")]
        [RequireUserPermission(Discord.GuildPermission.Administrator, ErrorMessage = "You don't have permission to do that!")]
        public async Task Prefix(string prefix = null)
        {
            if(prefix == null)
            {
                var guildprefix = await _servers.GetGuildPrefix(Context.Guild.Id) ?? "!";
                await Context.Channel.TriggerTypingAsync();
                await ReplyAsync($"The current prefix of this bot is `{guildprefix}`");
                return;

            }

            if (prefix.Length > 8)
            {
                await Context.Channel.TriggerTypingAsync();
                await ReplyAsync("Prefix is too long, try another one.");
                return;
            }

            await _servers.ModifyGuildPrefix(Context.Guild.Id, prefix);
            await Context.Channel.TriggerTypingAsync();
            await ReplyAsync($"The prefix has been adjusted to `{prefix}`");

            _logger.LogInformation($"{Context.User.Username} executed the math command!");
        }
        [Command("listranks", RunMode = RunMode.Async)]
        [Summary("Lists all available ranks")]
        public async Task Ranks()
        {
            var ranks = await _ranksHelper.GetRanksAsync(Context.Guild);
            if (ranks.Count == 0)
            {
                await Context.Channel.TriggerTypingAsync();
                await Context.Channel.SendErrorAsync("Ranks", "There's no ranks on this server, consider setting up some!");
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            string description = "This message lists all avaiable ranks.\nIn order to add a rank, use the name of the rank or the rank's ID.";
            foreach (var rank in ranks)
            {
                description += $"\n{rank.Mention} {rank.Id}";
            }
            var builder = new EmbedBuilder()
                   .WithThumbnailUrl(Context.Guild.IconUrl)
                   .WithTitle("Ranks")
                   .WithColor(new Color(33, 176, 252))
                   .WithDescription(description)
                   .WithCurrentTimestamp();

            var embed = builder.Build();
            await Context.Channel.TriggerTypingAsync();
            await Context.Channel.SendMessageAsync(null, false, embed);
        }
        [Command("addrank", RunMode = RunMode.Async)]
        [Summary("Add a rank to the list of usable ranks")]
        [RequireUserPermission(Discord.GuildPermission.Administrator, ErrorMessage = "You don't have permission to do that!")]
        [RequireBotPermission(Discord.GuildPermission.ManageRoles)]
        public async Task AddRank([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var ranks = await _ranksHelper.GetRanksAsync(Context.Guild);

            var role = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (role == null)
            {
                await Context.Channel.TriggerTypingAsync();
                await Context.Channel.SendErrorAsync("Ranks", "That role does not exist!");
                return;
            }

            if (role.Position > Context.Guild.CurrentUser.Hierarchy)
            {
                await Context.Channel.TriggerTypingAsync();
                await Context.Channel.SendErrorAsync("Ranks", "That role has a higher position than the mine!");
                return;
            }

            if (ranks.Any(x => x.Id == role.Id))
            {
                await Context.Channel.TriggerTypingAsync();
                await Context.Channel.SendErrorAsync("Ranks", "That role is already a rank!");
                return;
            }

            await _ranks.AddRankAsync(Context.Guild.Id, role.Id);
            await Context.Channel.TriggerTypingAsync();
            await Context.Channel.SendSuccessAsync("Ranks", $"The role {role.Mention} has been added to the ranks!");
        }
        [Command("delrank", RunMode = RunMode.Async)]
        [Summary("Removes a rank from the list of ranks")]
        [RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "You don't have permission to do that!")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task DelRank([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var ranks = await _ranksHelper.GetRanksAsync(Context.Guild);

            var role = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (role == null)
            {
                await Context.Channel.TriggerTypingAsync();
                await Context.Channel.SendErrorAsync("Ranks", "That role does not exist!");
                return;
            }

            if (ranks.Any(x => x.Id != role.Id))
            {
                await Context.Channel.TriggerTypingAsync();
                await Context.Channel.SendErrorAsync("Ranks", "That role is not a rank yet!");
                return;
            }

            await _ranks.RemoveRankAsync(Context.Guild.Id, role.Id);
            await Context.Channel.TriggerTypingAsync();
            await Context.Channel.SendSuccessAsync("Ranks", $"The role {role.Mention} has been removed from the ranks");
        }
        [Command("autoroles", RunMode = RunMode.Async)]
        [Summary("Lists all available autoroles")]
        [RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "You don't have permission to do that!")]
        public async Task AutoRoles()
        {
            var autoRoles = await _autoRolesHelper.GetAutoRolesAsync(Context.Guild);
            if (autoRoles.Count == 0)
            {;
                await Context.Channel.TriggerTypingAsync();
                await Context.Channel.SendErrorAsync("Auto Roles", "This server does not yet have any autoroles!");
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            string description = "This message lists all autoroles.\nIn order to remove an autorole, use the name or ID.";
            foreach (var autoRole in autoRoles)
            {
                description += $"\n{autoRole.Mention} ({autoRole.Id})";
            }
            var builder = new EmbedBuilder()
                 .WithThumbnailUrl(Context.Guild.IconUrl)
                 .WithTitle("Auto Roles")
                 .WithColor(new Color(33, 176, 252))
                 .WithDescription(description)
                 .WithCurrentTimestamp();

            var embed = builder.Build();
            await Context.Channel.SendMessageAsync(null, false, embed);
        }

        [Command("addautorole", RunMode = RunMode.Async)]
        [Summary("Set an autorole")]
        [RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "You don't have permission to do that!")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task AddAutoRole([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var autoRoles = await _autoRolesHelper.GetAutoRolesAsync(Context.Guild);

            var role = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (role == null)
            {
                await Context.Channel.SendErrorAsync("Auto Roles", "That role does not exist!");
                return;
            }

            if (role.Position > Context.Guild.CurrentUser.Hierarchy)
            {
                await Context.Channel.SendErrorAsync("Auto Roles", "That role has a higher position than the mine!");
                return;
            }

            if (autoRoles.Any(x => x.Id == role.Id))
            {
                await Context.Channel.SendErrorAsync("Auto Roles", "That role is already an autorole!");
                return;
            }

            await _autoRoles.AddAutoRoleAsync(Context.Guild.Id, role.Id);
            await Context.Channel.SendSuccessAsync("Auto Roles", $"The role {role.Mention} has been added to the autoroles!");
        }


        [Command("delautorole", RunMode = RunMode.Async)]
        [Summary("Delete an autorole")]
        [RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "You don't have permission to do that!")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task DelAutoRole([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var autoRoles = await _autoRolesHelper.GetAutoRolesAsync(Context.Guild);

            var role = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (role == null)
            {
                await Context.Channel.SendErrorAsync("Auto Roles", "That role does not exist!");
                return;
            }

            if (autoRoles.Any(x => x.Id == role.Id))
            {
                await _autoRoles.RemoveAutoRoleAsync(Context.Guild.Id, role.Id);
                await Context.Channel.SendSuccessAsync("Auto Roles", $"The role {role.Mention} has been removed from the autoroles!");

                return;
            }

            await Context.Channel.SendErrorAsync("Auto Roles", "That role is not an autorole yet!");

        }
        [Command("welcome")]
        [Summary("Setup the welcome module")]
        [RequireUserPermission(GuildPermission.ManageChannels, ErrorMessage = "You don't have permission to do that!")]
        public async Task Welcome(string option = null, string value = null)
        {
            if (option == null && value == null)
            {
                var fetchedChannelId = await _servers.GetWelcomeAsync(Context.Guild.Id);
                if (fetchedChannelId == 0)
                {
                    await ReplyAsync("There has not been set a welcome channel yet!");
                    return;
                }

                var fetchedChannel = Context.Guild.GetTextChannel(fetchedChannelId);
                if (fetchedChannel == null)
                {
                    await ReplyAsync("There has not been set a welcome channel yet!");
                    await _servers.ClearWelcomeAsync(Context.Guild.Id);
                    return;
                }

                var fetchedBackground = await _servers.GetBackgroundAsync(Context.Guild.Id);

                if (fetchedBackground != null)
                    await ReplyAsync($"The channel used for the welcome module is {fetchedChannel.Mention}.\nThe background is set to {fetchedBackground}.");
                else
                    await ReplyAsync($"The channel used for the welcome module is {fetchedChannel.Mention}.");

                return;
            }

            if (option == "channel" && value != null)
            {
                if (!MentionUtils.TryParseChannel(value, out ulong parsedId))
                {
                    await ReplyAsync("Please pass in a valid channel!");
                    return;
                }

                var parsedChannel = Context.Guild.GetTextChannel(parsedId);
                if (parsedChannel == null)
                {
                    await ReplyAsync("Please pass in a valid channel!");
                    return;
                }

                await _servers.ModifyWelcomeAsync(Context.Guild.Id, parsedId);
                await ReplyAsync($"Successfully modified the welcome channel to {parsedChannel.Mention}.");
                return;
            }

            if (option == "background" && value != null)
            {
                if (value == "clear")
                {
                    await _servers.ClearBackgroundAsync(Context.Guild.Id);
                    await ReplyAsync("Successfully cleared the background for this server.");
                    return;
                }

                await _servers.ModifyBackgroundAsync(Context.Guild.Id, value);
                await ReplyAsync($"Successfully modified the background to {value}.");
                return;
            }

            if (option == "clear" && value == null)
            {
                await _servers.ClearWelcomeAsync(Context.Guild.Id);
                await ReplyAsync("Successfully cleared the welcome channel.");
                return;
            }

            await ReplyAsync("You did not use this command properly!");
        }
    }
}

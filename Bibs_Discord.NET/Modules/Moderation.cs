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
using System.Diagnostics;

namespace Bibs_Discord.NET.Modules
{
    public class Moderation : ModuleBase<SocketCommandContext>
    {
        private readonly ServerHelper _serverHelper;
        private readonly ILogger<Moderation> _logger;
        private readonly Servers _servers;
        private readonly Ranks _ranks;
        private readonly AutoRoles _autoRoles;

        private readonly GuildPermissions mutedPerms = new GuildPermissions(sendMessages: false);

        public Moderation(ServerHelper serverHelper, ILogger<Moderation> logger, Servers servers, Ranks ranks, AutoRoles autoRoles)
        {
            _serverHelper = serverHelper;
            _logger = logger;
            _servers = servers;
            _ranks = ranks;
            _autoRoles = autoRoles;
        }

        public async Task MutePerms(SocketGuildUser user)
        {
            foreach (var channel in Context.Guild.Channels) // Loop over all channels
            {
                await channel.AddPermissionOverwriteAsync(user,
                        new OverwritePermissions(sendMessages: PermValue.Deny, addReactions: PermValue.Deny, connect: PermValue.Deny,
                            speak: PermValue.Deny));
            }
        }
        public async Task UnMutePerms(SocketGuildUser user)
        {
            foreach (var channel in Context.Guild.Channels) // Unmute
            {
                await channel.RemovePermissionOverwriteAsync(user);
            }
        }

        [Command("say"), Alias("s")]
        [Summary("Make Bibs say what you want")]
        public async Task Say([Remainder] string text)
        {
            if ((text.ToString().IndexOf("@") >= 0) == true)
            {
                await Context.Channel.TriggerTypingAsync();
                await Context.Channel.SendMessageAsync("I won't say a message with that symbol.");
                return;
            }
            if ((Context.Channel as IDMChannel) != null)
            {
                await Context.Channel.TriggerTypingAsync();
                await ReplyAsync(text);
                _logger.LogInformation($"{Context.User.Username}#{Context.User.Discriminator} used the say command with the message '{text}'!");
                return;
            }
            await Context.Message.DeleteAsync();
            await Context.Channel.TriggerTypingAsync();
            await ReplyAsync(text);
            _logger.LogInformation($"{Context.User.Username}#{Context.User.Discriminator} used the say command with the message '{text}'!");
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
            

            foreach (var channel in Context.Guild.Channels) // Loop over all channels
            {
                if (!channel.GetPermissionOverwrite(user).HasValue ||                              // Check if the channel has the correct permissions for the muted role                             
                    channel.GetPermissionOverwrite(user).Value.SendMessages == PermValue.Allow ||  // If not, update the permissions of the role
                    channel.GetPermissionOverwrite(user).Value.AddReactions == PermValue.Allow ||
                    channel.GetPermissionOverwrite(user).Value.Connect == PermValue.Allow ||
                    channel.GetPermissionOverwrite(user).Value.Speak == PermValue.Allow)
                {
                    await MutePerms(user);
                    await _serverHelper.SendLogAsync(Context.Guild, "Situation Log", $"{Context.User.Mention} muted {user.Mention}!");
                    EmbedBuilder embedBuilder = new EmbedBuilder()
                                 .WithThumbnailUrl(Context.Guild.IconUrl)
                                 .WithTitle("Muted")
                                 .WithColor(new Color(33, 176, 252))
                                 .WithDescription($"Muted {user.Mention} in all text and voice channels.")
                                 .WithCurrentTimestamp();

                    Embed embed1 = embedBuilder.Build();
                    await Context.Channel.SendMessageAsync(null, false, embed1);
                    return;
                }
                else if (!channel.GetPermissionOverwrite(user).HasValue ||
                    channel.GetPermissionOverwrite(user).Value.SendMessages == PermValue.Deny ||
                    channel.GetPermissionOverwrite(user).Value.AddReactions == PermValue.Deny ||
                    channel.GetPermissionOverwrite(user).Value.Connect == PermValue.Deny ||
                    channel.GetPermissionOverwrite(user).Value.Speak == PermValue.Deny)
                {
                    await UnMutePerms(user);
                    await _serverHelper.SendLogAsync(Context.Guild, "Situation Log", $"{Context.User.Mention} umuted {user.Mention}!");

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
            }
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
            await _serverHelper.SendLogAsync(Context.Guild, "Situation Log", $"{Context.User.Mention} changed {user.Username}'s nickname to {name}!");
        }

        [Command("prune")]
        [Alias("purge", "delet")]
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

            await _serverHelper.SendLogAsync(Context.Guild, "Situation Log", $"{Context.User.Mention} bulk deleted {messages.Count()} messages(s)!");

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
            await _serverHelper.SendLogAsync(Context.Guild, "Banned", $"{user.Mention} was kicked by {Context.User.Mention}!");
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
            await _serverHelper.SendLogAsync(Context.Guild, "Situation Log", $"{user.Mention} was banned by {Context.User.Mention}!");
        }


        [Command("prefix", RunMode = RunMode.Async)]
        [Summary("Set the bot's command prefix")]
        [RequireUserPermission(Discord.GuildPermission.Administrator, ErrorMessage = "You don't have permission to do that!")]
        public async Task Prefix(string prefix = null)
        {
            if (prefix == null)
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

            await _serverHelper.SendLogAsync(Context.Guild, "Situation Log", $"{Context.User.Mention} modified the prefix to `{prefix}`.");
        }
        [Command("listranks", RunMode = RunMode.Async)]
        [Summary("Lists all available ranks")]
        [RequireContext(ContextType.Guild, ErrorMessage = "You need to be in a discord server to use this commands!")]
        public async Task Ranks()
        {
            var ranks = await _serverHelper.GetRanksAsync(Context.Guild);
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
            var ranks = await _serverHelper.GetRanksAsync(Context.Guild);

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
            await _serverHelper.SendLogAsync(Context.Guild, "Situation Log", $"{Context.User.Mention} added {role.Mention} to the ranks!");
        }
        [Command("delrank", RunMode = RunMode.Async)]
        [Summary("Removes a rank from the list of ranks")]
        [RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "You don't have permission to do that!")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task DelRank([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var ranks = await _serverHelper.GetRanksAsync(Context.Guild);

            var role = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (role == null)
            {
                await Context.Channel.TriggerTypingAsync();
                await Context.Channel.SendErrorAsync("Ranks", "That role does not exist!");
                return;
            }

            if (!ranks.Any(x => x.Id == role.Id))
            {
                await Context.Channel.TriggerTypingAsync();
                await Context.Channel.SendErrorAsync("Ranks", "That role is not a rank yet!");
                return;
            }
            await _ranks.RemoveRankAsync(Context.Guild.Id, role.Id);
            await Context.Channel.TriggerTypingAsync();
            await Context.Channel.SendSuccessAsync("Ranks", $"The role {role.Mention} has been removed from the ranks");
            await _serverHelper.SendLogAsync(Context.Guild, "Situation Log", $"{Context.User.Mention} removed {role.Mention} from the ranks!");
            return;
        }
        [Command("autoroles", RunMode = RunMode.Async)]
        [Summary("Lists all available autoroles")]
        [RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "You don't have permission to do that!")]
        public async Task AutoRoles()
        {
            var autoRoles = await _serverHelper.GetAutoRolesAsync(Context.Guild);
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
            var autoRoles = await _serverHelper.GetAutoRolesAsync(Context.Guild);

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
            await _serverHelper.SendLogAsync(Context.Guild, "Situation Log", $"{Context.User.Mention} added {role.Mention} to the autoroles!");
        }


        [Command("delautorole", RunMode = RunMode.Async)]
        [Summary("Delete an autorole")]
        [RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "You don't have permission to do that!")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task DelAutoRole([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var autoRoles = await _serverHelper.GetAutoRolesAsync(Context.Guild);

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
                await _serverHelper.SendLogAsync(Context.Guild, "Situation Log", $"{Context.User.Mention} removed {role.Mention} from the autoroles!");

                return;
            }

            await Context.Channel.SendErrorAsync("Auto Roles", "That role is not an autorole yet!");

        }
        [Command("welcome")]
        [Summary("Setup the welcome module")]
        [RequireUserPermission(GuildPermission.ManageChannels, ErrorMessage = "You don't have permission to do that!")]
        public async Task Welcome(string option = null, string value = null)
        {
            await Context.Channel.TriggerTypingAsync();
            if (option == null && value == null)
            {
                var fetchedChannelId = await _servers.GetWelcomeAsync(Context.Guild.Id);
                if (fetchedChannelId == 0)
                {
                    await Context.Channel.SendErrorAsync("Welcome module", "There has not been set a welcome channel yet!");
                    return;
                }

                var fetchedChannel = Context.Guild.GetTextChannel(fetchedChannelId);
                if (fetchedChannel == null)
                {
                    await Context.Channel.SendErrorAsync("Welcome module", "There has not been set a welcome channel yet!");
                    await _servers.ClearWelcomeAsync(Context.Guild.Id);
                    return;
                }

                var fetchedBackground = await _servers.GetBackgroundAsync(Context.Guild.Id);

                if (fetchedBackground != null)
                    await Context.Channel.SendSuccessAsync("Welcome module", $"The channel used for the welcome module is {fetchedChannel.Mention}.\nThe background is set to {fetchedBackground}.");
                else
                    await Context.Channel.SendSuccessAsync("Welcome module", $"The channel used for the welcome module is {fetchedChannel.Mention}.");

                return;
            }

            if (option == "channel" && value != null)
            {
                if (!MentionUtils.TryParseChannel(value, out ulong parsedId))
                {
                    await Context.Channel.SendErrorAsync("Welcome module", "Please pass in a valid channel!");
                    return;
                }

                var parsedChannel = Context.Guild.GetTextChannel(parsedId);
                if (parsedChannel == null)
                {
                    await Context.Channel.SendErrorAsync("Welcome module", "Please pass in a valid channel!");
                    return;
                }

                await _servers.ModifyWelcomeAsync(Context.Guild.Id, parsedId);
                await Context.Channel.SendSuccessAsync("Welcome module", $"Successfully modified the welcome channel to {parsedChannel.Mention}.");
                await _serverHelper.SendLogAsync(Context.Guild, "Situation Log", $"{Context.User.Mention} modified the welcome channel to {parsedChannel.Mention}!");
                return;
            }

            if (option == "background" && value != null)
            {
                if (value == "clear")
                {
                    await _servers.ClearBackgroundAsync(Context.Guild.Id);
                    await Context.Channel.SendSuccessAsync("Welcome module", "Successfully cleared the background for this server.");
                    await _serverHelper.SendLogAsync(Context.Guild, "Situation Log", $"{Context.User.Mention} cleared the welcome channel!");
                    return;
                }

                await _servers.ModifyBackgroundAsync(Context.Guild.Id, value);
                await Context.Channel.SendSuccessAsync("Welcome module", $"Successfully modified the background to {value}.");
                return;
            }

            if (option == "clear" && value == null)
            {
                await _servers.ClearWelcomeAsync(Context.Guild.Id);
                await Context.Channel.SendSuccessAsync("Welcome module", "Successfully cleared the welcome channel.");
                return;
            }
            await Context.Channel.SendErrorAsync("Welcome module", "You did not use this command properly!");
        }
        [Command("slowmode")]
        [Summary("Sets a channel's slowmode interval to a user's desired ammount")]
        [RequireUserPermission(GuildPermission.ManageChannels, ErrorMessage = "You don't have permission to do that!")]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task SlowMode(int interval = 0)
        {
            await ((Context.Channel as SocketTextChannel).ModifyAsync(x => x.SlowModeInterval = interval));
            await Context.Channel.SendSuccessAsync("Slowmode", $"Channel's slowmode interval has been adjusted to {interval} seconds!");
        }
        [Command("filter")]
        [Summary("Setup the word filter module")]
        [RequireUserPermission(GuildPermission.ManageChannels, ErrorMessage = "You don't have permission to do that!")]
        public async Task Filter()
        {
            await Context.Channel.TriggerTypingAsync();
            await _servers.ModifyFilterAsync(Context.Guild.Id);
            var fetchedServerFilter = await _servers.GetFilterAsync(Context.Guild.Id);
            await Context.Channel.SendSuccessAsync("Word Filter", $"Successfully set the server's word filter to {fetchedServerFilter.ToString()}");
        }

        [Command("logs")]
        [Summary("Setup the Situation Log")]
        [RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "You don't have permission to do that!")]
        public async Task Logs(string value = null)
        {
            await Context.Channel.TriggerTypingAsync();
            if (value == null)
            {
                var fetchedChannelId = await _servers.GetLogsAsync(Context.Guild.Id);
                if (fetchedChannelId == 0)
                {
                    await Context.Channel.SendErrorAsync("Situation Log", "There has not been set a logs channel yet!");
                    return;
                }

                var fetchedChannel = Context.Guild.GetTextChannel(fetchedChannelId);
                if (fetchedChannel == null)
                {
                    await Context.Channel.SendErrorAsync("Situation Log", "There has not been set a logs channel yet!");
                    await _servers.ClearLogsAsync(Context.Guild.Id);
                    return;
                }

                await Context.Channel.SendSuccessAsync("Situation Log", $"The channel used for logs is set to {fetchedChannel.Mention}.");

                return;
            }

            if (value != "clear")
            {
                if (!MentionUtils.TryParseChannel(value, out ulong parsedId))
                {
                    await Context.Channel.SendErrorAsync("Situation Log", "Please pass in a valid channel!");
                    return;
                }

                var parsedChannel = Context.Guild.GetTextChannel(parsedId);
                if (parsedChannel == null)
                {
                    await Context.Channel.SendErrorAsync("Situation Log", "Please pass in a valid channel!");
                    return;
                }

                await _servers.ModifyLogsAsync(Context.Guild.Id, parsedId);
                await Context.Channel.SendSuccessAsync("Situation Log", $"Successfully modified the logs channel to {parsedChannel.Mention}.");
                return;
            }


            if (value == "clear")
            {
                await _servers.ClearLogsAsync(Context.Guild.Id);
                await Context.Channel.SendSuccessAsync("Situation Log", "Successfully cleared the logs channel.");
                return;
            }
            await Context.Channel.SendErrorAsync("Situation Log", "You did not use this command properly!");
        }

        [Command("stats")]
        [Summary("Shares the bot summary here")]
        public async Task BotMainStats()
        {
            var time = DateTime.Now - Process.GetCurrentProcess().StartTime;
            var upTime = "I Have Been Up For: ";

            if (time.Days > 0)
            {
                if (time.Hours <= 0 || time.Minutes <= 0)
                {
                    upTime += $"{time.Days} Day(s) and ";
                }
                else
                {
                    upTime += $"{time.Days} Day(s),";
                }
            }

            if (time.Hours > 0)
            {
                if (time.Minutes > 0)
                {
                    upTime += $" {time.Hours} Hour(s), ";
                }
                else
                {
                    upTime += $"{time.Hours} Hour(s) And ";
                }
            }

            if (time.Minutes > 0)
            {
                upTime += $"{time.Minutes} Minute(s)";
            }

            if (time.Seconds >= 0)
            {
                if (time.Hours > 0 || time.Minutes > 0)
                {
                    upTime += $" And {time.Seconds} Second(s)";
                }
                else
                {
                    upTime += $"{time.Seconds} Second(s)";
                }
            }

            var process = Process.GetCurrentProcess();
            var mem = process.PrivateMemorySize64;
            var memory = mem / 1024 / 1024;
            var totalUsers = Context.Client.Guilds.Sum(guild => guild.MemberCount);

            var builder = new EmbedBuilder();
            builder.WithTitle("Bibs's Stats:");
            builder.WithDescription("You wanna get to know me better huh?");
            builder.WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl());
            builder.WithColor(new Color(0x00ff00));
            builder.AddField("Ping:", $"```fix\n{Context.Client.Latency}ms```", true);
            builder.AddField("Total Servers:", $"```fix\n{Context.Client.Guilds.Count} Servers```", true);
            builder.AddField("Total Users:", $"```fix\n{totalUsers} total users```", true);
            builder.WithCurrentTimestamp();
            builder.AddField("Memory Usage:", $"```fix\n{memory}Mb```", true);
            builder.AddField("Up-time:", $"```prolog\n{upTime}```", true);
            builder.WithFooter(
                x =>
                {
                    x.WithText($"Stats | Requested by {Context.User.Username}");
                    x.WithIconUrl(Context.User.GetAvatarUrl());
                });
            // Sends message and deletes
            await ReplyAsync(string.Empty, false, builder.Build());
        }
        [Command("leaveguild")]
        [Summary("Forces the bot to leave a guild")]
        [RequireOwner]
        public async Task LeaveGuild(ulong guildId)
        {
            SocketGuild guild = Context.Client.GetGuild(guildId);

            //Make sure the bot is in a guild with the provided guildId.
            if (guild != null)
            {
                await Context.Channel.SendMessageAsync("... It was never personal");
                await guild.LeaveAsync();
            }    
            else
                await Context.Channel.SendMessageAsync($"The bot isn't in a guild with the id of {guildId}!");
        }
        [Command("kill")]
        [Summary("Stops Bibs to adapt to any new changes in code.")]
        [RequireOwner]
        public async Task kill()
        {
            _logger.LogDebug("Battle Control Terminated.");
            await Context.Channel.TriggerTypingAsync();
            await ReplyAsync("... It was never personal");
            Environment.Exit(0);
        }
    }
}

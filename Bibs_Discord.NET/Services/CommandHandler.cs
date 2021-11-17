using System;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Bibs_Discord_dotNET.Utilities;
using Bibs_Infrastructure;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Bibs_Discord_dotNET.Ultilities;
using Bibs_Discord_dotNET.Commons;
using Victoria;
using Victoria.EventArgs;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Bibs_Discord_dotNET.Commons.Structs;
using System.Text.RegularExpressions;

namespace Bibs_Discord_dotNET.Services
{
    public class CommandHandler : InitializedService
    {
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _service;
        private readonly IConfiguration _config;
        private readonly Servers _servers;
        private readonly Images _images;
        private readonly ServerHelper _serverHelper;
        private readonly LavaNode _lavaNode;
        private readonly ILogger<CommandHandler> _logger;
        private readonly Limits _limits;
        private readonly Markovs _markovs;
        public static bool shutup = false;

        public CommandHandler(Markovs markovs, Limits limits, ILogger<CommandHandler> logger, IServiceProvider provider, DiscordSocketClient client, CommandService service, IConfiguration config, Servers servers, Images images, ServerHelper serverHelper, LavaNode lavaNode)
        {
            _provider = provider;
            _client = client;
            _service = service;
            _config = config;
            _servers = servers;
            _images = images;
            _serverHelper = serverHelper;
            _lavaNode = lavaNode;
            _logger = logger;
            _limits = limits;
            _markovs = markovs;
        }

        public override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            _client.Ready += OnReadyAsync;
            _client.MessageReceived += OnMessageReceived;
            _client.JoinedGuild += OnJoinedGuild;
            _client.UserJoined += OnUserJoined;
            _client.Connected += OnStartUp;
            //_client.UserIsTyping += OnUserIsTyping;
            _client.LeftGuild += OnLeftGuild;
            //_client.MessageDeleted += OnMessageDeleted;
            //_client.MessageUpdated += OnMessageUpdated;
            _lavaNode.OnTrackEnded += OnTrackEnded;

            _service.CommandExecuted += OnCommandExecuted;
            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }
        /*
        private async Task OnMessageUpdated(Cacheable<IMessage, ulong> msgBefore, SocketMessage msgAfter, ISocketMessageChannel channel)
        {
            var guild = (channel as SocketTextChannel)?.Guild;
            var message = await msgBefore.GetOrDownloadAsync();
            await _serverHelper.SendLogAsync(guild, "Situation Log", $"Message Edited: {msgBefore.Value.Author.Username}#{msgBefore.Value.Author.DiscriminatorValue} edited {message.ToString() ?? "A message was updated, but its content could not be retrieved from cache."}\nTo: {msgAfter.Content.ToString()}.");
            return;

        }*/
        /*
        private async Task OnMessageDeleted(Cacheable<IMessage, ulong> msg, ISocketMessageChannel channel)
        {
            var guild = (channel as SocketTextChannel)?.Guild;
            if (msg.Value.Content == null)
            {
                await _serverHelper.SendLogAsync(guild, "Situation Log", $"Message Deleted: A message was deleted, but its content could not be retrieved from cache.");
            }
            else 
            {
                var message = await msg.GetOrDownloadAsync();
                await _serverHelper.SendLogAsync(guild, "Situation Log", $"Message Deleted: {msg.Value.Author.Username}#{msg.Value.Author.DiscriminatorValue} said {message.ToString()}.");
                return;
            }
        }
        */

        private async Task OnLeftGuild(SocketGuild arg)
        {
            await _client.SetGameAsync($"over {_client.Guilds.Count} servers!", null, ActivityType.Watching);
        }

        
        private async Task OnStartUp()
        {
            await _client.SetGameAsync($"over {_client.Guilds.Count} servers!", null, ActivityType.Watching);
            _logger.LogDebug("Establishing Battlefield Control, standby...");
        }

        private async Task OnReadyAsync()
        {
            _logger.LogDebug("Battle Control Online");
            if (!_lavaNode.IsConnected)
            {
                await _lavaNode.ConnectAsync();
            }

        }

        private async Task OnTrackEnded(TrackEndedEventArgs args)
        {
            if (!args.Reason.ShouldPlayNext())
            {
                return;
            }

            var player = args.Player;
            if (!player.Queue.TryDequeue(out var queueable))
            {
                await (player.TextChannel as ISocketMessageChannel).SendSuccessAsync("Music","Queue completed! Please add more tracks to rock n' roll!");
                return;
            }

            if (!(queueable is LavaTrack track))
            {
                await (player.TextChannel as ISocketMessageChannel).SendErrorAsync("Music", "Next item in queue is not a track.");
                return;
            }

            await args.Player.PlayAsync(track);
            await (player.TextChannel as ISocketMessageChannel).SendSuccessAsync($"{args.Reason}: {args.Track.Title}", $"Now playing: {track.Title}");
        }

        private async Task OnUserJoined(SocketGuildUser arg)
        {
            var newTask = new Task(async () => await HandleUserJoined(arg));
            newTask.Start();
        }

        private async Task HandleUserJoined(SocketGuildUser arg)
        {
            var roles = await _serverHelper.GetAutoRolesAsync(arg.Guild);
            if (roles.Count > 0)
                await arg.AddRolesAsync(roles, options: new RequestOptions()
                {
                    AuditLogReason = "Bibs autorole"
                });

            var channelId = await _servers.GetWelcomeAsync(arg.Guild.Id);
            if (channelId == 0)
                return;

            var channel = arg.Guild.GetTextChannel(channelId);
            if (channel == null)
            {
                await _servers.ClearWelcomeAsync(arg.Guild.Id);
                return;
            }

            var background = await _servers.GetBackgroundAsync(arg.Guild.Id);
            string path = await _images.CreateImageAsync(arg, background);

            await channel.SendFileAsync(path, null);
            System.IO.File.Delete(path);
        }

        private async Task OnJoinedGuild(SocketGuild arg)
        {
            await arg.DefaultChannel.TriggerTypingAsync();
            var builder = new EmbedBuilder()
                 .WithThumbnailUrl(arg.CurrentUser.GetAvatarUrl())
                 .WithTitle("Thanks for inviting me!")
                 .WithDescription("I'm still in training, please take care of me...\nType !help for a list of commands.")
                 .WithColor(new Color(33, 176, 252))
                 .WithCurrentTimestamp();
            var embed = builder.Build();
            await arg.DefaultChannel.SendMessageAsync(null, false, embed);
            //calls this method whenever bibs joins a new server so bibs doesn't do a die
            await _servers.ClearFilterAsync(arg.Id);
            await _servers.ClearRaidAsync(arg.Id);
            await _servers.ClearNoWeebAsync(arg.Id);
            await _servers.ClearHasLimitAsync(arg.Id);
            await _servers.ClearHasMarkovAsync(arg.Id);
            await _client.SetGameAsync($"over {_client.Guilds.Count} servers!", null, ActivityType.Watching);
        }
        private async Task HandleFilter(SocketMessage arg)
        {
            var guild = (arg.Channel as SocketTextChannel)?.Guild;
            var message = (arg as SocketUserMessage);

            /* for some fucking reasons the bot did not loop properly so it only changes the first value like what the f????
             
           
            foreach (KeyValuePair<string, string> replacement in Variables.lelic)
            {
                if (message.Content.ToString().ToLower().Contains(replacement.Key))
                {
                    string sentence = message.Content.ToString().ToLower();
                    string[] words = sentence.Split(null);
                    for (int i = 0; i < words.Count(); i++)
                    {
                        if (words[i].Contains(replacement.Key))
                        {
                            words[Array.IndexOf(words, words[i])] = Regex.Replace(words[i], replacement.Key, replacement.Value, RegexOptions.IgnoreCase);
                        }
                    }
                    string familyFriendly = string.Join(" ", words);
                    await message.DeleteAsync();
                    await message.Channel.SendErrorAsync("Swear Filter!", $"```{message.Author.Username}: {familyFriendly}```");
                    await _serverHelper.SendLogAsync(guild, "Situation Log", $"{message.Author.Mention} said: ```{message.Content.ToString()}```.");
                    return;
                }
            }
            */

            if (message.Content.ToString().ToLower().Split(" ").Intersect(Variables.pottyMouth).Any())
            {
                await message.DeleteAsync();
                await message.Channel.SendErrorAsync("Hey!", $"{message.Author.Mention} You can't say that!");
                await _serverHelper.SendLogAsync(guild, "Situation Log", $"{message.Author.Mention} said `{message.Content.ToString()}`.");
                return;
            }

            if (message.Content.ToString().Contains("@everyone") || message.Content.ToString().Contains("@here") || message.Content.ToString().ToLower().Contains("nitro"))
            {
                var user = message.Author as SocketGuildUser;
                if (user.Hierarchy > guild.CurrentUser.Hierarchy)
                {
                    return;
                }
                await message.DeleteAsync();
                try {
                    var channel = await message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync($"The message `{message.Content}` was deleted because you've triggered the anti-phishing filter, you probably shouldn't be pinging @everyone and you should also do giveaways elsewhere.");
                }
                catch { }
                return;
                
            }

        }
        private async Task HandleAutomatedResponse(SocketMessage arg)
        {
            int argID = 0;
            SocketSelfUser selfUser = _client.CurrentUser;
            var message = (arg as SocketUserMessage);
            

            foreach (var compliment in Variables.compliments)
            {
                if (message.Content.ToLower().Contains(compliment) && (message.Content.ToLower().Contains("bibs") || message.HasMentionPrefix(selfUser as IUser, ref argID)))
                {
                    await message.Channel.TriggerTypingAsync();
                    await message.Channel.SendMessageAsync($"{Variables.response[Variables.randomRepliesToCompliments.Next(0, Variables.response.Length)].ToString()}");
                    return;
                }
            }
            //yknow i could have used switch case here too
            //fcken hell
            if ((message.Content.ToLower().Contains("you're flat") || message.Content.ToLower().Contains("you are flat")) && message.Content.ToLower().Contains("bibs"))
            {
                await message.Channel.TriggerTypingAsync();
                await message.Channel.SendMessageAsync("I'll murder you!");
                return;
            }
            if ((message.HasMentionPrefix(selfUser as IUser, ref argID) && message.Content.ToLower().Contains("shit") && message.Content.ToLower().Contains("bot")))
            {
                await message.Channel.TriggerTypingAsync();
                await message.Channel.SendMessageAsync($"{Variables.comebacks[Variables.randomRepliesToInsults.Next(0, Variables.comebacks.Length)].ToString()}");
                return;
            }

            if ((message.HasMentionPrefix(selfUser as IUser, ref argID) && message.Content.ToLower().Contains("shut up")))
            {
                await message.Channel.TriggerTypingAsync();
                await message.Channel.SendMessageAsync("Awww, you're having a bad day?");
                return;
            }

            if ((message.HasMentionPrefix(selfUser as IUser, ref argID) && message.Content.ToLower().Contains("go away")))
            {
                await message.Channel.TriggerTypingAsync();
                await message.Channel.SendMessageAsync("I would, but you're blocking the way.");
                return;
            }

            if ((message.HasMentionPrefix(selfUser as IUser, ref argID) && message.Content.ToLower().Contains("die")))
            {
                await message.Channel.TriggerTypingAsync();
                await message.Channel.SendMessageAsync("I tried, but hell didn't let me in. Can you show me how it's done?");
                return;
            }

            if ((message.Content.ToLower().Contains("numbers") && message.Content.ToLower().Contains("mean")) && message.Content.ToLower().Contains("bibs"))
            {
                await message.Channel.TriggerTypingAsync();
                await Task.Delay(5000);
                await message.Channel.SendMessageAsync("...all agents. Our new allies in Cuba have graciously permitted the construction of a new...and permanent...broadcast station within their borders. From now until Project Nova’s initiation, all instructions will be broadcast from the Rusalka.");
                return;
            }
            if ((message.ToString().IndexOf("hello there", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
            {
                await message.Channel.TriggerTypingAsync();
                await message.Channel.SendMessageAsync("GENERAL KENOBI! You are a bold one.");
                return;
            }

            if ((message.ToString().IndexOf("it was never personal", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
            {
                await message.Channel.TriggerTypingAsync();
                await message.Channel.SendMessageAsync("https://cdn.discordapp.com/attachments/382242328695275525/781952571282685972/It_was_never_personal.gif");
                return;
            }
            if ((message.ToString().IndexOf("we've got a job to do", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
            {
                await message.Channel.TriggerTypingAsync();
                await message.Channel.SendMessageAsync("https://cdn.discordapp.com/attachments/767653326560821248/782494935928537099/unknown.png");
                return;
            }
            if ((message.ToString().IndexOf("i serve the soviet union", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
            {
                await message.Channel.TriggerTypingAsync();
                await message.Channel.SendMessageAsync("https://cdn.discordapp.com/attachments/767653326560821248/782991988517240842/I_Serve_the_Soviet_Union.png");
                return;
            }
            if ((message.ToString().IndexOf("what is a man", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
            {
                await message.Channel.TriggerTypingAsync();
                await message.Channel.SendMessageAsync("A miserable little pile of secrets.");
                await message.Channel.SendMessageAsync("But enough talk...");
                await message.Channel.SendMessageAsync("Have at you!");
                return;
            }
        }
        private async Task OnMessageReceived(SocketMessage arg)
        {
            
            var guild = (arg.Channel as SocketTextChannel)?.Guild;
            
            if (!(arg is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            var firstTask = new Task(async () => await HandleAutomatedResponse(arg));
            firstTask.Start();

            var argPos = 0;
            string prefix ="!";
            if (guild != null) 
            {
                try {
                    var guildHasFilter = _servers.GetFilterAsync(guild.Id).Result;
                    if (guildHasFilter == true)
                    {
                        var newTask = new Task(async () => await HandleFilter(arg));
                        newTask.Start();
                    }
                }
                catch (Exception) 
                {
                    await _servers.ClearFilterAsync(guild.Id);
                    await message.Channel.SendErrorAsync("Error", "Something went wrong, please try again, if the bot is unresponsive, contact Ribs#8205 on discord.");
                }
                try
                {
                    var guildHasLimit = _servers.GetHasLimitAsync(guild.Id).Result;
                    if (guildHasLimit == true)
                    {
                        var limits = await _limits.GetLimitsAsync(guild.Id);
                        if (!limits.Exists(x => x.ChannelId == arg.Channel.Id))
                        {
                            return;
                        }
                    }
                }
                catch (Exception)
                {
                    //await _servers.ClearHasLimitAsync(guild.Id);
                    await message.Channel.SendErrorAsync("Error", "Something went wrong, please try again, if the bot is unresponsive, contact Ribs#8205 on discord.");
                }

                prefix = await _servers.GetGuildPrefix((message.Channel as SocketGuildChannel).Guild.Id) ?? "!";
                
                if (!message.HasStringPrefix(prefix, ref argPos) && !message.HasMentionPrefix(_client.CurrentUser, ref argPos)) return;

                var context = new SocketCommandContext(_client, message);
                await _service.ExecuteAsync(context, argPos, _provider);
            }
            if ((message.Channel as IDMChannel) != null)
            {
                if (!message.HasStringPrefix(prefix, ref argPos) && !message.HasMentionPrefix(_client.CurrentUser, ref argPos)) return;

                var context = new SocketCommandContext(_client, message);
                await _service.ExecuteAsync(context, argPos, _provider);
            }

        }

        private async Task OnCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (result.Error == CommandError.UnmetPrecondition)
            {
                var embed = new EmbedBuilder
                {
                    Author = new EmbedAuthorBuilder
                    {
                        Name = $"{context.User.Username}#{context.User.Discriminator}",
                        IconUrl = context.User.GetAvatarUrl() ?? context.User.GetDefaultAvatarUrl()
                    },
                    Description = result.ErrorReason.ToString(),
                    Color = Color.Red
                }
                .WithFooter("Did you really think I'd fall for that?")
                .WithCurrentTimestamp();
                await context.Channel.SendMessageAsync(embed: embed.Build());
                return;
            }
            if (result.Error == CommandError.BadArgCount)
            {
                var embed = new EmbedBuilder
                {
                    Author = new EmbedAuthorBuilder
                    {
                        Name = $"{context.User.Username}#{context.User.Discriminator}",
                        IconUrl = context.User.GetAvatarUrl() ?? context.User.GetDefaultAvatarUrl()
                    },
                    Description = result.ErrorReason.ToString(),
                    Color = Color.Red
                }
                .WithFooter("You sure everything is ok there?")
                .WithCurrentTimestamp();
                await context.Channel.SendMessageAsync(embed: embed.Build());
                return;
            }
            if (result.Error == CommandError.ParseFailed)
            {
                var embed = new EmbedBuilder
                {
                    Author = new EmbedAuthorBuilder
                    {
                        Name = $"{context.User.Username}#{context.User.Discriminator}",
                        IconUrl = context.User.GetAvatarUrl() ?? context.User.GetDefaultAvatarUrl()
                    },
                    Description = result.ErrorReason.ToString(),
                    Color = Color.Red
                }
                .WithFooter("Looks like you're trying something you shouldn't...")
                .WithCurrentTimestamp();
                await context.Channel.SendMessageAsync(embed: embed.Build());
                return;
            }
            //if (result.Error == CommandError.UnknownCommand)
            //{
            //    var embed = new EmbedBuilder
            //    {
            //        Author = new EmbedAuthorBuilder
            //        {
            //            Name = $"{context.User.Username}#{context.User.Discriminator}",
            //            IconUrl = context.User.GetAvatarUrl() ?? context.User.GetDefaultAvatarUrl()
            //        },
            //        Description = result.ErrorReason.ToString(),
            //        Color = Color.Red
            //    }
            //    .WithFooter("What are you trying to do?")
            //    .WithCurrentTimestamp();
            //    await context.Channel.SendMessageAsync(embed: embed.Build());
            //    return;
            //}
            if (command.IsSpecified && !result.IsSuccess) await (context.Channel as ISocketMessageChannel).SendErrorAsync("Error", result.ErrorReason);
        }
    }
}

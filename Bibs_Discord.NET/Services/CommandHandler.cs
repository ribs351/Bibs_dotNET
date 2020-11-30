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


        public CommandHandler(IServiceProvider provider, DiscordSocketClient client, CommandService service, IConfiguration config, Servers servers, Images images, ServerHelper serverHelper, LavaNode lavaNode)
        {
            _provider = provider;
            _client = client;
            _service = service;
            _config = config;
            _servers = servers;
            _images = images;
            _serverHelper = serverHelper;
            _lavaNode = lavaNode;
        }

        public override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            _client.Ready += OnReadyAsync;
            _client.MessageReceived += OnMessageReceived;
            _client.JoinedGuild += OnJoinedGuild;
            _client.UserJoined += OnUserJoined;
            _client.Connected += OnStartUp;
            _client.UserIsTyping += OnUserIsTyping;
            _client.LeftGuild += OnLeftGuild;
            //_client.MessageDeleted += OnMessageDeleted;
            _lavaNode.OnTrackEnded += OnTrackEnded;

            _service.CommandExecuted += OnCommandExecuted;
            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }
        /*
        private async Task OnMessageDeleted(Cacheable<IMessage, ulong> arg1, ISocketMessageChannel arg2)
        {
            
        }
        */

        private async Task OnLeftGuild(SocketGuild arg)
        {
            await _client.SetGameAsync($"over {_client.Guilds.Count} servers!", null, ActivityType.Watching);
        }

        private async Task OnUserIsTyping(SocketUser u, ISocketMessageChannel m)
        {
            if (u.IsBot) return;
            int random = new Random().Next(0, 1000);
            string username = u.Username;
            if (random == 5)
            {
                await m.TriggerTypingAsync();
                await m.SendMessageAsync($"Watcha typin' there, {username}?");
            }
        }

        private async Task OnStartUp()
        {
            await _client.SetGameAsync($"over {_client.Guilds.Count} servers!", null, ActivityType.Watching);
        }

        private async Task OnReadyAsync()
        {
            
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
            await _client.SetGameAsync($"over {_client.Guilds.Count} servers!", null, ActivityType.Watching);
        }
       

        private async Task OnMessageReceived(SocketMessage arg)
        {
            var guild = (arg.Channel as SocketTextChannel)?.Guild;

            if (!(arg is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            string[] pottyMouth = new string[] { 
                "faggot",
                "faggots",
                "nigger",
                "niggers",
                "nigga",
                "niggas",
                "fag",
                "fags",
                "faggy", 
                "niglet",
                "niglets",
                "towelhead",
                "towelheads",
                "negro",
                "negros",
                "chink",
                "chinks",
                "zipperhead",
                "zipperheads",
                "beaner",
                "beaners",
                "coon",
                "coons",
                "kike",
                "kikes",
                "jewboy",
                "jewboys"
            };

            if ((message.ToString().IndexOf("hello there", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
            {
                await message.Channel.TriggerTypingAsync();
                await message.Channel.SendMessageAsync("GENERAL KENOBI!");
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
                await message.Channel.SendMessageAsync("https://en.meming.world/images/en/2/28/I_Serve_the_Soviet_Union.jpg");
                return;
            }
            if ((message.ToString().IndexOf("bruh", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
            {
                await message.Channel.TriggerTypingAsync();
                await message.Channel.SendMessageAsync("**Ah, MAN!** This **ReAlLy** be A ***bruh*** moment.");
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
            if (message.Content.ToString().ToLower().Split(" ").Intersect(pottyMouth).Any())
            {
                await message.DeleteAsync();
                await message.Channel.SendErrorAsync("Hey!", $"{message.Author.Mention} You can't say that!");
                await _serverHelper.SendLogAsync(guild, "Situation Log", $"{message.Author.Mention} said `{message.Content.ToString()}`.");
                return;
            }


            var argPos = 0;
            string prefix ="!";
            if (guild != null) 
            {
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
                .WithCurrentTimestamp();
                await context.Channel.SendMessageAsync(embed: embed.Build());
                return;
            }

            if (command.IsSpecified && !result.IsSuccess) await (context.Channel as ISocketMessageChannel).SendErrorAsync("Error", result.ErrorReason);
        }
    }
}

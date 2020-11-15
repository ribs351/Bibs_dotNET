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
        private readonly AutoRolesHelper _autoRolesHelper;
        private readonly LavaNode _lavaNode;


        public CommandHandler(IServiceProvider provider, DiscordSocketClient client, CommandService service, IConfiguration config, Servers servers, Images images, AutoRolesHelper autoRolesHelper, LavaNode lavaNode)
        {
            _provider = provider;
            _client = client;
            _service = service;
            _config = config;
            _servers = servers;
            _images = images;
            _autoRolesHelper = autoRolesHelper;
            _lavaNode = lavaNode;
        }

        public override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            _client.Ready += OnReadyAsync;
            _client.MessageReceived += OnMessageReceived;
            _client.JoinedGuild += OnJoinedGuild;
            _client.UserJoined += OnUserJoined;
            _client.Connected += OnStartUp;
            _lavaNode.OnTrackEnded += OnTrackEnded;

            _service.CommandExecuted += OnCommandExecuted;
            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
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
            var roles = await _autoRolesHelper.GetAutoRolesAsync(arg.Guild);
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
        }
       

        private async Task OnMessageReceived(SocketMessage arg)
        {
            if (!(arg is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            if (message.Content.Contains("bruh"))
            {
                await message.Channel.TriggerTypingAsync();
                await message.Channel.SendMessageAsync("**Ah, MAN!** This **ReAlLy** be A ***bruh*** momment.");
                return;
            }

            var argPos = 0;
            string prefix = await _servers.GetGuildPrefix((message.Channel as SocketGuildChannel).Guild.Id) ?? "!";
            if (!message.HasStringPrefix(prefix, ref argPos) && !message.HasMentionPrefix(_client.CurrentUser, ref argPos)) return;

            var context = new SocketCommandContext(_client, message);
            await _service.ExecuteAsync(context, argPos, _provider);

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

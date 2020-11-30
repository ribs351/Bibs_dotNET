using Bibs_Discord_dotNET.Commons;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;

namespace Bibs_Discord.NET.Modules
{
    [RequireContext(ContextType.Guild, ErrorMessage = "You need to be in a discord server to use this commands!")]
    public class Music : ModuleBase<SocketCommandContext>
    {
        private readonly LavaNode _lavaNode;

        public Music(LavaNode lavaNode)
        {
            _lavaNode = lavaNode;
        }

        [Command("join", RunMode = RunMode.Async)]
        [Summary("Make Bibs join the voice channel you're in")]
        public async Task JoinAsync()
        {
            if (_lavaNode.HasPlayer(Context.Guild))
            {
                await Context.Channel.SendErrorAsync("Music", "I'm already connected to a voice channel!");
                return;
            }

            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await Context.Channel.SendErrorAsync("Music", "You must be connected to a voice channel!");
                return;
            }

            try
            {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
                await Context.Channel.SendSuccessAsync("Music", $"Joined {voiceState.VoiceChannel.Name}!");
            }
            catch (Exception exception)
            {
                await Context.Channel.SendErrorAsync("Music", exception.Message);
            }
        }
        [Command("disconnect", RunMode = RunMode.Async)]
        [Summary("Disconnects Bibs from the voice channel you're in")]
        public async Task Disconnect()
        {
            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await Context.Channel.SendErrorAsync("Music", "I'm not connected to a voice channel.");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);
            if (player.PlayerState is PlayerState.Playing)
            {
                await player.StopAsync();
            }

            await _lavaNode.LeaveAsync(player.VoiceChannel);
            await Context.Channel.SendSuccessAsync("Music", "Disconnected from all voice channels!");
        }

        [Command("play", RunMode = RunMode.Async)]
        [Summary("Searches for a song by title, then plays it")]
        public async Task PlayAsync([Remainder] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                await Context.Channel.SendErrorAsync("Music", "Please provide search terms.");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await Context.Channel.SendErrorAsync("Music", "I'm not connected to a voice channel.");
                return;
            }

            var searchResponse = await _lavaNode.SearchYouTubeAsync(query);
            if (searchResponse.LoadStatus == LoadStatus.LoadFailed ||
                searchResponse.LoadStatus == LoadStatus.NoMatches)
            {
                await Context.Channel.SendErrorAsync("Music", $"I wasn't able to find anything for `{query}`.");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);

            if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
            {

                var track = searchResponse.Tracks[0];
                player.Queue.Enqueue(track);
                await Context.Channel.SendSuccessAsync("Music", $"Enqueued: {track.Title}");
            }

            else
            {
                var track = searchResponse.Tracks[0];
                await player.PlayAsync(track);
                await Context.Channel.SendSuccessAsync("Music", $"Now Playing: {track.Title}");
            }
        }

        [Command("volume", RunMode = RunMode.Async)]
        [Summary("Sets the volume for the current playing song 0-150")]
        public async Task Volume(ushort volume)
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await Context.Channel.SendErrorAsync("Music", "You must be connected to a voice channel!");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await Context.Channel.SendErrorAsync("Music", "I'm not connected to a voice channel!");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);

            if (voiceState.VoiceChannel != player.VoiceChannel)
            {
                await Context.Channel.SendErrorAsync("Music", "You need to be in the same voice channel as I am!");
                return;
            }

            if (volume > 150 || volume <= 0)
            {
                await Context.Channel.SendErrorAsync("Music", "Volume must be between 1 and 150.");
                return;
            }

            await player.UpdateVolumeAsync(volume);
            await Context.Channel.SendSuccessAsync("Music", $"Volume now is set to  **{volume}/150**!");
        }


        [Command("skip", RunMode = RunMode.Async)]
        [Summary("Skips to the next track in the queue")]
        public async Task Skip()
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await Context.Channel.SendErrorAsync("Music", "You must be connected to a voice channel!");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await Context.Channel.SendErrorAsync("Music", "I'm not connected to a voice channel!");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);

            if (voiceState.VoiceChannel != player.VoiceChannel)
            {
                await Context.Channel.SendErrorAsync("Music", "You need to be in the same voice channel as I am!");
                return;
            }

            if (player.Queue.Count == 0)
            {
                await Context.Channel.SendErrorAsync("Music", "There are no more songs in the queue!");
                return;
            }

            await player.SkipAsync();
            await Context.Channel.SendSuccessAsync("Music", $"Skipped! Now playing **{player.Track.Title}**!");
        }

        [Command("stop", RunMode = RunMode.Async)]
        [Summary("Stops the music")]
        public async Task Stop()
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await Context.Channel.SendErrorAsync("Music", "You must be connected to a voice channel!");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await Context.Channel.SendErrorAsync("Music", "I'm not connected to a voice channel!");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);

            if (voiceState.VoiceChannel != player.VoiceChannel)
            {
                await Context.Channel.SendErrorAsync("Music", "You need to be in the same voice channel as I am!");
                return;
            }
            if (player.PlayerState is PlayerState.Playing)
            {
                await player.StopAsync();
                await Context.Channel.SendSuccessAsync("Music", "The music is stopped and the playlist is cleared");
                return;
            }
            else
            {
                await Context.Channel.SendErrorAsync("Music", "Could not aquire player.\nAre you using the bot right now?");
                return;
            }
        }

        [Command("pause", RunMode = RunMode.Async)]
        [Summary("Pauses the music")]
        public async Task Pause()
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await Context.Channel.SendErrorAsync("Music", "You must be connected to a voice channel!");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await Context.Channel.SendErrorAsync("Music", "I'm not connected to a voice channel!");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);

            if (voiceState.VoiceChannel != player.VoiceChannel)
            {
                await Context.Channel.SendErrorAsync("Music", "You need to be in the same voice channel as I am!");
                return;
            }

            if (player.PlayerState == PlayerState.Paused || player.PlayerState == PlayerState.Stopped)
            {
                await Context.Channel.SendErrorAsync("Music", "The music is already paused!");
                return;
            }

            await player.PauseAsync();
            await Context.Channel.SendSuccessAsync("Music", "Paused the music!");
        }
        [Command("resume", RunMode = RunMode.Async)]
        [Summary("Resumes the music")]
        public async Task Resume()
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await Context.Channel.SendErrorAsync("Music", "You must be connected to a voice channel!");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await Context.Channel.SendErrorAsync("Music", "I'm not connected to a voice channel!");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);

            if (voiceState.VoiceChannel != player.VoiceChannel)
            {
                await Context.Channel.SendErrorAsync("Music", "You need to be in the same voice channel as I am!");
                return;
            }

            if (player.PlayerState == PlayerState.Playing)
            {
                await Context.Channel.SendErrorAsync("Music", "The music is already playing!");
                return;
            }

            await player.ResumeAsync();
            await Context.Channel.SendSuccessAsync("Music", "Resumed the track!");
        }
        [Command("playlist", RunMode = RunMode.Async)]
        [Summary("Checks the current playlist")]
        public async Task PlayList()
        {
            var descriptionBuilder = new StringBuilder();
            var player = _lavaNode.GetPlayer(Context.Guild);
            if (player == null)
            {
                await Context.Channel.SendErrorAsync("Music", "Could not aquire player.\nAre you using the bot right now?");
                return;
            }

            if (player.PlayerState is PlayerState.Playing)
            {
                if (player.Queue.Count < 1 && player.Track != null)
                {
                    await Context.Channel.SendSuccessAsync($"Now Playing: {player.Track.Title}", "Nothing Else Is Queued.");
                    return;
                }
                else
                {
                    var trackNum = 2;
                    foreach (LavaTrack track in player.Queue)
                    {
                        descriptionBuilder.Append($"{trackNum}: [{track.Title}]({track.Url})\n");
                        trackNum++;
                    }
                    await Context.Channel.SendSuccessAsync("Music Playlist", $"Now Playing: [{player.Track.Title}]({player.Track.Url})\n{descriptionBuilder}");
                    return;
                }
            }
            else 
            {
                await Context.Channel.SendErrorAsync("Music", "Player doesn't seem to be playing anything right now. If this is an error, contact Ribs");
                return;
            }
        }
    }
}

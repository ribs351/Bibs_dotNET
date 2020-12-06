using Bibs_Discord_dotNET.Services;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bibs_Discord_dotNET.Modules
{
    public class YouTube : ModuleBase<SocketCommandContext>
    {
        private static ApiService _apiService = null;
        public YouTube(ApiService apiService)
        {
            if (_apiService == null)
            {
                _apiService = apiService;
            }
        }
        [Command("yt", RunMode = RunMode.Async)]
        [Summary("Search YouTube for a specific keyword")]
        public async Task SearchYouTube([Remainder] string args = "")
        {
            var builder = new EmbedBuilder();
            string searchFor = string.Empty;
            StringBuilder sb = new StringBuilder();
            List<Google.Apis.YouTube.v3.Data.SearchResult> results = null;


            if (string.IsNullOrEmpty(args))
            {
                sb.AppendLine("Please provide a term to search for!");
                builder.WithTitle($"No search term provided!");
                builder.WithThumbnailUrl(Context.User.GetAvatarUrl());
                builder.WithColor(new Color(33, 176, 252));
                builder.WithDescription(sb.ToString());
                var embed = builder.Build();
                await ReplyAsync(null, false, embed);
                return;
            }
            else
            {
                searchFor = args;
                builder.WithColor(new Color(33, 176, 252));
                results = await _apiService.SearchChannelsAsync(searchFor);
            }

            if (results != null)
            {
                string videoUrlPrefix = $"https://www.youtube.com/watch?v=";
                builder.Title = $"YouTube Search For (**{searchFor}**)";
                var thumbFromVideo = results.Where(r => r.Id.Kind == "youtube#video").Take(1).FirstOrDefault();
                if (thumbFromVideo != null)
                {
                    builder.ThumbnailUrl = thumbFromVideo.Snippet.Thumbnails.Default__.Url;
                }
                foreach (var result in results.Where(r => r.Id.Kind == "youtube#video").Take(3))
                {
                    string fullVideoUrl = string.Empty;
                    string videoId = string.Empty;
                    string description = string.Empty;
                    if (string.IsNullOrEmpty(result.Snippet.Description))
                    {
                        description = "No description available.";
                    }
                    else
                    {
                        description = result.Snippet.Description;
                    }
                    if (result.Id.VideoId != null)
                    {
                        fullVideoUrl = $"{videoUrlPrefix}{result.Id.VideoId.ToString()}";
                    }
                    sb.AppendLine($"**__{result.Snippet.ChannelTitle}__** -> [**{result.Snippet.Title}**]({fullVideoUrl})\n\n *{description}*\n");
                }
                builder.Description = sb.ToString();
                var embed = builder.Build();
                await ReplyAsync(null, false, embed);
            }
        }
    }
}

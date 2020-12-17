using Bibs_Discord_dotNET.Commons;
using Bibs_Discord_dotNET.Preconditions;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Kitsu.Anime;
using Kitsu.Manga;
using NHentaiAPI;
using System.Linq;
using Bibs_Infrastructure;
using Discord.WebSocket;

namespace Bibs_Discord_dotNET.Modules
{
    public class Weeb : ModuleBase<SocketCommandContext>
    {
        public static bool NNN = false;
        private readonly Servers _servers;
        public Weeb(Servers servers)
        {
            _servers = servers;
        }

        [Command("neko", RunMode = RunMode.Async)]
        [Summary("Get a random catgirl")]
        [Cooldown(5)]
        public async Task CatGirl()
        {

            var client = new HttpClient();
            var result = await client.GetStringAsync("https://nekos.moe/api/v1/random/image?count=1&nsfw=false");
            if (result == null)
            {
                await Context.Channel.SendErrorAsync("Neko API", "Something went wrong with the Neko API!");
                return;
            }
            var neko = JsonConvert.DeserializeObject<dynamic>(result);
            var guild = (Context.Channel as SocketTextChannel)?.Guild;
            try
            {
                var guildHasWeebBlackist = _servers.GetNoWeebAsync(guild.Id).Result;
                if (guildHasWeebBlackist == true)
                {
                    var newTask = new Task(async () => await HandleWeebBlacklist());
                    newTask.Start();
                    return;
                }
            }
            catch (Exception e)
            {
                await _servers.ClearFilterAsync(guild.Id);
                await Context.Channel.SendErrorAsync("Error", "Something went wrong, please try again, if the bot is unresponsive, contact Ribs#8205 on discord.");
            }

            var builder = new EmbedBuilder()
              .WithColor(new Color(33, 176, 252))
              .WithTitle("Neko")
              .WithUrl($"https://nekos.moe/image/{neko.images[0].id.ToString()}")
              .WithImageUrl($"https://nekos.moe/image/{neko.images[0].id.ToString()}");
            await Context.Channel.TriggerTypingAsync();
            await ReplyAsync(embed: builder.Build());
            return;

        }
        [Command("hneko", RunMode = RunMode.Async)]
        [Summary("Get a random nsfw catgirl")]
        [Cooldown(5)]
        public async Task NsfwCatGirl()
        {
            var client = new HttpClient();
            var result = await client.GetStringAsync("https://nekos.moe/api/v1/random/image?count=1&nsfw=true");
            if (result == null)
            {
                await Context.Channel.SendErrorAsync("Neko API", "Something went wrong with the Neko API!");
                return;
            }
            var neko = JsonConvert.DeserializeObject<dynamic>(result);
            if ((Context.Channel as IDMChannel) != null)
            {
                var guild = (Context.Channel as SocketTextChannel)?.Guild;
                try
                {
                    var guildHasWeebBlackist = _servers.GetNoWeebAsync(guild.Id).Result;
                    if (guildHasWeebBlackist == true)
                    {
                        var newTask = new Task(async () => await HandleWeebBlacklist());
                        newTask.Start();
                        return;
                    }
                }
                catch (Exception e)
                {
                    await _servers.ClearFilterAsync(guild.Id);
                    await Context.Channel.SendErrorAsync("Error", "Something went wrong, please try again, if the bot is unresponsive, contact Ribs#8205 on discord.");
                }

                var builder = new EmbedBuilder()
                .WithColor(new Color(33, 176, 252))
                .WithTitle("NSFW Neko")
                .WithUrl($"https://nekos.moe/image/{neko.images[0].id.ToString()}")
                .WithImageUrl($"https://nekos.moe/image/{neko.images[0].id.ToString()}");
                await Context.Channel.TriggerTypingAsync();
                await ReplyAsync(embed: builder.Build());
                return;
            }
            else if ((Context.Channel as ITextChannel).IsNsfw)
            {
                var builder = new EmbedBuilder()
                  .WithColor(new Color(33, 176, 252))
                  .WithTitle("NSFW Neko")
                  .WithUrl($"https://nekos.moe/image/{neko.images[0].id.ToString()}")
                  .WithImageUrl($"https://nekos.moe/image/{neko.images[0].id.ToString()}");
                await Context.Channel.TriggerTypingAsync();
                await ReplyAsync(embed: builder.Build());
                return;
            }
            else
            {
                await Context.Channel.TriggerTypingAsync();
                await Context.Channel.SendErrorAsync("Weeb Module", "This command is inappropriate to use here, use it in nsfw or in DMs!");
                return;
            }
        }
        [Command("nukes", RunMode = RunMode.Async)]
        [Summary("Generates launch codes based on your query")]
        public async Task Nukes([Remainder] string query)
        {
            var client = new NHentaiClient();
            var results = (await client.GetSearchPageListAsync(query, 1)).Result;
            string description = "This message lists all available launch codes:";

            if (NNN == true)
            {
                await Context.Channel.TriggerTypingAsync();
                await ReplyAsync("You can't use this now, No Nut November is in effect!");
                return;
            }
            else
            {
                if ((Context.Channel as IDMChannel) != null)
                {
                    try
                    {
                        foreach (var result in results)
                        {
                            foreach (var tag in result.Tags)
                                if (tag.Id == 12227)
                                {
                                    description += $"\n[{result.Id.ToString()}](https://nhentai.net/g/{result.Id.ToString()})";
                                }
                        }
                        var builder = new EmbedBuilder()
                             .WithTitle("Nuclear Launch Codes")
                             .WithColor(new Color(33, 176, 252))
                             .WithDescription(description)
                             .WithCurrentTimestamp();

                        var embed = builder.Build();
                        await Context.Channel.SendMessageAsync(null, false, embed);
                    }
                    catch (Exception e)
                    {
                        await ReplyAsync("Something went wrong with the nukes");
                    }
                    return;
                }
                else if ((Context.Channel as ITextChannel).IsNsfw)
                {
                    var guild = (Context.Channel as SocketTextChannel)?.Guild;
                    try
                    {
                        var guildHasWeebBlackist = _servers.GetNoWeebAsync(guild.Id).Result;
                        if (guildHasWeebBlackist == true)
                        {
                            var newTask = new Task(async () => await HandleWeebBlacklist());
                            newTask.Start();
                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        await _servers.ClearFilterAsync(guild.Id);
                        await Context.Channel.SendErrorAsync("Error", "Something went wrong, please try again, if the bot is unresponsive, contact Ribs#8205 on discord.");
                    }

                    try
                    {
                        foreach (var result in results)
                        {
                            foreach (var tag in result.Tags)
                                if (tag.Id == 12227)
                                {
                                    description += $"\n[{result.Id.ToString()}](https://nhentai.net/g/{result.Id.ToString()})";
                                }
                        }
                        var builder = new EmbedBuilder()
                             .WithTitle("Nuclear Launch Codes")
                             .WithColor(new Color(33, 176, 252))
                             .WithDescription(description)
                             .WithCurrentTimestamp();

                        var embed = builder.Build();
                        await Context.Channel.SendMessageAsync(null, false, embed);
                    }
                    catch (Exception e)
                    {
                        await ReplyAsync("Something went wrong with the nukes");
                    }
                    return;
                }
                await ReplyAsync("You can't leak military secrets here!");
            }
        }
        [Command("anime", RunMode = RunMode.Async)]
        [Summary("Shows info about the given anime by its name")]
        [RequireBotPermission(ChannelPermission.SendMessages | ChannelPermission.EmbedLinks)]
        public async Task AnimeAsync([Remainder, Name("anime_name")] string args)
        {
            try
            {
                await Context.Channel.TriggerTypingAsync();
                var res = await Anime.GetAnimeAsync(args);
                var anime = res.Data[0];

                var embed = new EmbedBuilder
                {
                    Color = new Color(33, 176, 252),
                    Title = anime.Attributes.CanonicalTitle ?? anime.Attributes.Titles.JaJp,
                    ThumbnailUrl = anime.Attributes.PosterImage.Medium,
                    Description = anime.Attributes.Synopsis ?? "-"
                }.Build();

                await ReplyAsync("", embed: embed);
            }
            catch (Exception e)
            {
                var embed = new EmbedBuilder
                {
                    Color = new Color(0xFF0000),
                    Description = e.Message
                }.Build();
                await ReplyAsync("", embed: embed);
                throw;
            }
        }
        [Command("manga", RunMode = RunMode.Async)]
        [Summary("Shows info about the given manga by its name")]
        [RequireBotPermission(ChannelPermission.SendMessages | ChannelPermission.EmbedLinks)]
        public async Task MangaAsync([Remainder, Name("manga_name")] string args)
        {
            var manga = await Manga.GetMangaAsync(args);
            await Context.Channel.TriggerTypingAsync();
            var embed = new EmbedBuilder
            {
                Color = new Color(33, 176, 252),
                Title = manga.Data[0].Attributes.CanonicalTitle ?? manga.Data[0].Attributes.Titles.JaJp,
                Description = manga.Data[0].Attributes.Synopsis,
                ThumbnailUrl = manga.Data[0].Attributes.PosterImage.Medium
            }.Build();

            await ReplyAsync("", embed: embed);
        }
        [Command("nnn")]
        [Summary("Activate No Nut November")]
        [RequireOwner]
        public async Task NoNutNovember()
        {
            NNN = !NNN;
            await Context.Channel.TriggerTypingAsync();
            await ReplyAsync($"Warning: No Nut November is now: {NNN.ToString()}!");

        }
        private async Task HandleWeebBlacklist()
        {
            await Context.Channel.TriggerTypingAsync();
            await Context.Channel.SendErrorAsync("Weeb Module", "This command is blacklisted on this server!");
            return;
        }
    }
}

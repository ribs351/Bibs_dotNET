using Bibs_Discord_dotNET.Ultilities;
using Bibs_Infrastructure;
using Discord;
using Discord.Commands;
using Discord.Addons.Interactive;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bibs_Discord.NET.Modules
{
    public class Help : InteractiveBase
    {
        private readonly CommandService _service;
        private readonly Servers _servers;
        public Help(CommandService service, Servers servers)
        {
            _service = service;
            _servers = servers;
        }
        [Command("help")]
        public async Task HelpAsync()
        {
            List<string> Pages = new List<string>();
            string prefix ="!";
            if ((Context.Channel as IDMChannel) != null)
            {
                foreach (var module in _service.Modules)
                {
                    string page = $"**{module.Name}**\n\n";
                    foreach (var command in module.Commands)
                    {
                        page += $"`{prefix}{command.Aliases.First()}` - {command.Summary ?? "No description found"}\n";
                    }
                    Pages.Add(page);
                }
                await PagedReplyAsync(Pages);
            }
            else 
            {
                prefix = await _servers.GetGuildPrefix((Context.Channel as SocketGuildChannel).Guild.Id);
                foreach (var module in _service.Modules)
                {
                    string page = $"**{module.Name}**\n\n";
                    foreach (var command in module.Commands)
                    {
                        page += $"`{prefix}{command.Aliases.First()}` - {command.Summary ?? "No description found"}\n";
                    }
                    Pages.Add(page);
                }
                await PagedReplyAsync(Pages);
            }
        }
        [Command("help")]
        public async Task HelpAsync(string command)
        {
            var result = _service.Search(Context, command);
            string prefix = "!";
            if ((Context.Channel as IDMChannel) != null)
            {
                if (!result.IsSuccess)
                {
                    await ReplyAsync($"Sorry, I couldn't find a command like **{command}**.");
                    return;
                }
                var builder = new EmbedBuilder()
                {
                    Color = new Color(114, 137, 218),
                    Description = $"Here are some commands like **{command}**"
                };

                foreach (var match in result.Commands)
                {
                    var cmd = match.Command;

                    builder.AddField(x =>
                    {
                        x.Name = string.Join(", ", cmd.Aliases);
                        x.Value = $"Parameters: {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n" +
                                  $"Summary: {cmd.Summary}";
                        x.IsInline = false;
                    });
                }
                await Context.Channel.TriggerTypingAsync();
                await ReplyAsync("", false, builder.Build());
            }
            else
            {
                if (!result.IsSuccess)
                {
                    await ReplyAsync($"Sorry, I couldn't find a command like **{command}**.");
                    return;
                }

                prefix = await _servers.GetGuildPrefix((Context.Channel as SocketGuildChannel).Guild.Id) ?? "!";
                var builder = new EmbedBuilder()
                {
                    Color = new Color(114, 137, 218),
                    Description = $"Here are some commands like **{command}**"
                };

                foreach (var match in result.Commands)
                {
                    var cmd = match.Command;

                    builder.AddField(x =>
                    {
                        x.Name = string.Join(", ", cmd.Aliases);
                        x.Value = $"Parameters: {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n" +
                                  $"Summary: {cmd.Summary}";
                        x.IsInline = false;
                    });
                }
                await Context.Channel.TriggerTypingAsync();
                await ReplyAsync("", false, builder.Build());
            }
            
        }
    }
}

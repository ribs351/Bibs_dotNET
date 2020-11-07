using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bibs_Discord_dotNET.Commons
{
    public static class Extensions
    {
        public static async Task<IMessage> SendSuccessAsync(this ISocketMessageChannel channel, string title, string description)
        {
            var embed = new EmbedBuilder()
                .WithColor(new Color(33, 176, 252))
                .WithDescription(description)
                .WithAuthor(author =>
                {
                    author
                    .WithIconUrl("https://png2.cleanpng.com/sh/0c2b06b47e6cce509be59a4811098fa1/L0KzQYm3VME6N5dniZH0aYP2gLBuTfNwdaF6jNd7LXnmf7B6Tflkd58yfNd8aXfxPbP8kBlvbaR4ReVAY3Pog8S0VfFlQGo3SNZuM3S0Q4O1Vcc1PGM6S6Y6NUK3QYW5VsA2QWQ4SpD5bne=/kisspng-computer-icons-icon-design-business-success-5ad8920de3d132.5744253415241426059332.png")
                    .WithName(title);
                }
                )
                .WithCurrentTimestamp()
                .Build();
            var message = await channel.SendMessageAsync(embed: embed);
            return message;
        }
        public static async Task<IMessage> SendErrorAsync(this ISocketMessageChannel channel, string title, string description)
        {
            var embed = new EmbedBuilder()
                .WithColor(new Color(244, 67, 54))
                .WithDescription(description)
                .WithAuthor(author =>
                {
                    author
                    .WithIconUrl("https://cdn.icon-icons.com/icons2/1380/PNG/512/vcsconflicting_93497.png")
                    .WithName(title);
                }
                )
                .WithCurrentTimestamp()
                .Build();
            var message = await channel.SendMessageAsync(embed: embed);
            return message;
        }
    }
}

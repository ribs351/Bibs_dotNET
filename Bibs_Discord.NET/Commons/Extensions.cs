using Discord;
using Discord.WebSocket;
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
                    .WithIconUrl("http://clipart-library.com/image_gallery2/Success-PNG-Image.png")
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
        public static async Task<IMessage> SendLogAsync(this ITextChannel channel, string title, string description)
        {
            var embed = new EmbedBuilder()
                .WithColor(new Color(33, 176, 252))
                .WithDescription(description)
                .WithAuthor(author =>
                {
                    author
                    .WithIconUrl("https://stellaris.paradoxwikis.com/images/4/48/Normal_sit_log.png")
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

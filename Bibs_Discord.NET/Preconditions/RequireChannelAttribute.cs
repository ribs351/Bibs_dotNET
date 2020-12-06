/*
 * WIP
 * Will expand on this later
using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace Bibs_Discord_dotNET.Preconditions
{
    public class RequireChannelAttribute : PreconditionAttribute
    {
        private readonly string _name;
        public RequireChannelAttribute(string name) => _name = name;
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            
            if (context.Channel is SocketGuildChannel gChannel)
            {
                var channel = await context.Guild.GetChannelAsync(context.Channel.Id) as SocketGuildChannel;
                
                if (channel.Name == _name)
                    
                    return await Task.FromResult(PreconditionResult.FromSuccess());

                else
                    return await Task.FromResult(PreconditionResult.FromError($"You be in a channel called {_name} to run this command."));
            }
            else
                return await Task.FromResult(PreconditionResult.FromError("You must be in a guild to run this command."));
        }
    }
}
*/
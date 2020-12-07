using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bibs_Infrastructure
{
    public class Servers
    {
        private readonly BibsContext _context;

        public Servers(BibsContext context)
        {
            _context = context;
        }

        public async Task ModifyGuildPrefix(ulong id, string prefix)
        {
            var server = await _context.Servers
                .FindAsync(id);

            if (server == null)
                _context.Add(new Server { Id = id, Prefix = prefix });
            else
                server.Prefix = prefix;
            await _context.SaveChangesAsync();
        }

        public async Task<string> GetGuildPrefix(ulong id)
        {
            var prefix = await _context.Servers
                .Where(x => x.Id == id)
                .Select(x => x.Prefix)
                .FirstOrDefaultAsync();
            return await Task.FromResult(prefix);
        }
        public async Task ModifyWelcomeAsync(ulong id, ulong channelId)
        {
            var server = await _context.Servers
                .FindAsync(id);

            if (server == null)
                _context.Add(new Server { Id = id, Welcome = channelId });
            else
                server.Welcome = channelId;

            await _context.SaveChangesAsync();
        }

        public async Task ClearWelcomeAsync(ulong id)
        {
            var server = await _context.Servers
                .FindAsync(id);

            server.Welcome = 0;
            await _context.SaveChangesAsync();
        }

        public async Task<ulong> GetWelcomeAsync(ulong id)
        {
            var server = await _context.Servers
                .FindAsync(id);

            return await Task.FromResult(server.Welcome);
        }

        public async Task ModifyLogsAsync(ulong id, ulong channelId)
        {
            var server = await _context.Servers
                .FindAsync(id);

            if (server == null)
                _context.Add(new Server { Id = id, Logs = channelId });
            else
                server.Logs = channelId;

            await _context.SaveChangesAsync();
        }

        public async Task ClearLogsAsync(ulong id)
        {
            var server = await _context.Servers
                .FindAsync(id);

            server.Logs = 0;
            await _context.SaveChangesAsync();
        }

        public async Task<ulong> GetLogsAsync(ulong id)
        {
            var server = await _context.Servers
                .FindAsync(id);

            return await Task.FromResult(server.Logs);
        }

        public async Task ModifyBackgroundAsync(ulong id, string url)
        {
            var server = await _context.Servers
                .FindAsync(id);

            if (server == null)
                _context.Add(new Server { Id = id, Background = url });
            else
                server.Background = url;

            await _context.SaveChangesAsync();
        }

        public async Task ClearBackgroundAsync(ulong id)
        {
            var server = await _context.Servers
                .FindAsync(id);

            server.Background = null;
            await _context.SaveChangesAsync();
        }

        public async Task<string> GetBackgroundAsync(ulong id)
        {
            var server = await _context.Servers
                .FindAsync(id);

            return await Task.FromResult(server.Background);
        }
        public async Task ModifyFilterAsync(ulong id)
        {
            var server = await _context.Servers
                .FindAsync(id);

            if (server == null)
                _context.Add(new Server { Id = id, Filter = true });
            else
                server.Filter = !server.Filter;

            await _context.SaveChangesAsync();
        }
        //call this method everytime bibs joins a new server otherwise bibs gets a seizure
        public async Task ClearFilterAsync(ulong id)
        {
            var server = await _context.Servers
                .FindAsync(id);

            if (server == null)
                _context.Add(new Server { Id = id, Filter = false }); //when bibs joins a new server, sets this to false by default
            else
                server.Filter = false; //keep setting this to false if the bot was re-added to a server
            await _context.SaveChangesAsync();
        }

        public async Task<bool> GetFilterAsync(ulong id)
        {
            var server = await _context.Servers
                .FindAsync(id);
            return await Task.FromResult(server.Filter);
        }
        public async Task ModifyRaidAsync(ulong id)
        {
            var server = await _context.Servers
                .FindAsync(id);

            if (server == null)
                _context.Add(new Server { Id = id, Raid = true });
            else
                server.Raid = !server.Raid;

            await _context.SaveChangesAsync();
        }
        //call this method everytime bibs joins a new server otherwise bibs gets a seizure
        public async Task ClearRaidAsync(ulong id)
        {
            var server = await _context.Servers
                .FindAsync(id);

            if (server == null)
                _context.Add(new Server { Id = id, Raid = false }); //when bibs joins a new server, sets this to false by default
            else
                server.Raid = false; //keep setting this to false if the bot was re-added to a server
            await _context.SaveChangesAsync();
        }

        public async Task<bool> GetRaidAsync(ulong id)
        {
            var server = await _context.Servers
                .FindAsync(id);
            return await Task.FromResult(server.Raid);
        }
    }
}

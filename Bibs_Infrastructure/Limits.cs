using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bibs_Infrastructure
{
    public class Limits
    {
        private readonly BibsContext _context;

        public Limits(BibsContext context)
        {
            _context = context;
        }
        public async Task<List<Limit>> GetLimitsAsync(ulong id)
        {
            var limits = await _context.Limits
                .Where(x => x.ServerId == id)
                .ToListAsync();

            return await Task.FromResult(limits);
        }

        public async Task AddLimitAsync(ulong id, ulong channelId)
        {
            var server = await _context.Servers
                .FindAsync(id);

            if (server == null)
                _context.Add(new Server { Id = id });

            _context.Add(new Limit { ChannelId = channelId, ServerId = id });
            await _context.SaveChangesAsync();
        }

        public async Task RemoveLimitAsync(ulong id, ulong channelId)
        {
            var limit = await _context.Limits
                .Where(x => x.ChannelId == channelId)
                .FirstOrDefaultAsync();

            _context.Remove(limit);
            await _context.SaveChangesAsync();
        }

        public async Task ClearLimitsAsync(List<Limit> limits)
        {
            _context.RemoveRange(limits);
            await _context.SaveChangesAsync();
        }
    }
}

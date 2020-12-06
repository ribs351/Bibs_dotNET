using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bibs_Infrastructure
{
    public class Muteds
    {
        private readonly BibsContext _context;

        public Muteds(BibsContext context)
        {
            _context = context;
        }
        public async Task<List<Muted>> GetMutesAsync(ulong id)
        {
            var mutes = await _context.Muteds
                .Where(x => x.ServerId == id)
                .ToListAsync();

            return await Task.FromResult(mutes);
        }

        public async Task AddMutedAsync(ulong id, ulong userId)
        {
            var server = await _context.Servers
                .FindAsync(id);

            if (server == null)
                _context.Add(new Server { Id = id });

            _context.Add(new Muted { UserId = userId, ServerId = id });
            await _context.SaveChangesAsync();
        }

        public async Task RemoveMutedAsync(ulong id, ulong userId)
        {
            var muted = await _context.Muteds
                .Where(x => x.UserId == userId)
                .FirstOrDefaultAsync();

            _context.Remove(muted);
            await _context.SaveChangesAsync();
        }

        public async Task ClearMutedssAsync(List<Muted> muteds)
        {
            _context.RemoveRange(muteds);
            await _context.SaveChangesAsync();
        }
    }
}

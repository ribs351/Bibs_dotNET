using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bibs_Infrastructure
{
    public class Markovs
    {
        private readonly BibsContext _context;

        public Markovs(BibsContext context)
        {
            _context = context;
        }
        public async Task<List<Markov>> GetMarkovsAsync(ulong id)
        {
            var markovs = await _context.Markovs
                .Where(x => x.ServerId == id)
                .ToListAsync();

            return await Task.FromResult(markovs);
        }

        public async Task AddMarkovAsync(ulong id, string messageContent)
        {
            var server = await _context.Servers
                .FindAsync(id);

            if (server == null)
                _context.Add(new Server { Id = id });

            _context.Add(new Markov { MessageContent = messageContent, ServerId = id });
            await _context.SaveChangesAsync();
        }

        public async Task RemoveMarkovAsync(ulong id, string messageContent)
        {
            var markov = await _context.Markovs
                .Where(x => x.MessageContent == messageContent)
                .FirstOrDefaultAsync();

            _context.Remove(markov);
            await _context.SaveChangesAsync();
        }

        public async Task ClearMarkovsAsync(List<Markov> Markovs)
        {
            _context.RemoveRange(Markovs);
            await _context.SaveChangesAsync();
        }
    }
}


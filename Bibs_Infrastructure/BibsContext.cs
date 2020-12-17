using System;
using Microsoft.EntityFrameworkCore;

namespace Bibs_Infrastructure
{
    public class BibsContext :DbContext
    {
        public DbSet<Server> Servers { get; set; }
        public DbSet<Rank> Ranks { get; set; }
        public DbSet<AutoRole> AutoRoles { get; set; }
        public DbSet<Limit> Limits { get; set; }
        public DbSet<Muted> Muteds { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseMySql("server=localhost;user=root;database=Bibs_DB;port=3306;Connect Timeout=5;");   
    }
    public class Server
    {
        public ulong Id { get; set; }
        public string Prefix { get; set; }
        public ulong Welcome { get; set; }
        public string Background { get; set; }
        public ulong Logs { get; set; }
        public bool Filter { get; set; }
        public bool Raid { get; set; }
        public bool NoWeeb { get; set; }
        public bool HasLimit { get; set; }
    }
    public class Rank
    {
        public int Id { get; set; }
        public ulong RoleId { get; set; }
        public ulong ServerId { get; set; }
    }
    public class AutoRole
    {
        public int Id { get; set; }
        public ulong RoleId { get; set; }
        public ulong ServerId { get; set; }
    }
    public class Limit
    {
        public int Id { get; set; }
        public ulong ChannelId { get; set; }
        public ulong ServerId { get; set; }
    }
    public class Muted
    {
        public int Id { get; set; }
        public ulong UserId { get; set; }
        public ulong ServerId { get; set; }
    }
}

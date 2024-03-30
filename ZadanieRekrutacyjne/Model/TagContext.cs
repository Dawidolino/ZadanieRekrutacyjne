using Microsoft.EntityFrameworkCore;

namespace ZadanieRekrutacyjne.Model
{
    public class TagContext : DbContext
    {
        public TagContext(DbContextOptions<TagContext> options) : base(options) { }
        public DbSet<Tag> Tags { get; set; }        
    }
}

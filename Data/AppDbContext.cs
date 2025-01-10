using Microsoft.EntityFrameworkCore;
using Table_Reservation.Models;

namespace Table_Reservation.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<TableModel> Tables { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TableModel>()
                .Property(t => t.Id)
                .ValueGeneratedOnAdd(); // Indique que l'ID est généré automatiquement
        }
    }
}

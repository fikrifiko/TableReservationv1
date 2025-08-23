using Microsoft.EntityFrameworkCore;
using Table_Reservation.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    // Autres DbSets pour vos tables
    public DbSet<AdministratorModel> Administrators { get; set; }

 

    public DbSet<TableModel> Tables { get; set; } 
    public DbSet<CanvasModel> CanvasModels { get; set; } 
    public DbSet<ReservationModel> Reservations { get; set; } 

    public DbSet<ClientModel> ClientModels { get; set; }


    public DbSet<PdfFileModel> PdfFiles { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TableModel>()
            .Property(t => t.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<CanvasModel>() 
            .Property(c => c.Id)
            .ValueGeneratedOnAdd();

        // Configuration explicite pour ReservationModel
        modelBuilder.Entity<ReservationModel>()
            .Property(r => r.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<ReservationModel>()
            .Property(r => r.Amount)
            .IsRequired()
            .HasDefaultValue(0); // Définit une valeur par défaut pour Amount

        modelBuilder.Entity<ReservationModel>()
            .Property(r => r.ClientPhone)
            .HasMaxLength(50); // Définit une longueur maximale pour ClientPhone

        modelBuilder.Entity<ClientModel>()
            .Property(C => C.Id)
            .ValueGeneratedOnAdd();



        // Configuration de la table Administrators
        modelBuilder.Entity<AdministratorModel>().HasKey(a => a.Id);
        modelBuilder.Entity<AdministratorModel>().Property(a => a.Username).IsRequired().HasMaxLength(50);
        modelBuilder.Entity<AdministratorModel>().Property(a => a.Password).IsRequired();
        modelBuilder.Entity<AdministratorModel>().Property(a => a.Email).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<AdministratorModel>().Property(a => a.CreatedAt).HasDefaultValueSql("GETDATE()");


    }
}

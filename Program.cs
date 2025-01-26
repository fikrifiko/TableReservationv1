using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Table_Reservation.DAL.Repositories;
using Table_Reservation.Data;
using Table_Reservation.Models;
using Table_Reservation.Services;
using Table_Reservation.Services.Interfaces;
using Table_Reservation.Validator;

var builder = WebApplication.CreateBuilder(args);

// Ajouter les services au conteneur
builder.Services.AddControllersWithViews();

// Ajouter le service Entity Framework Core avec SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//*******INJECTION DE DEPENDANCES*************//

//Service Injection
builder.Services.AddScoped<IClientService, ClientService>();

//Repository Injection
builder.Services.AddTransient<IClientRepository, ClientRepository>();

//Validator Injection
builder.Services.AddScoped<IValidator<ClientModel>, ClientModelValidator>();

//******************//

// Construire l'application
var app = builder.Build();

// Configurer le pipeline des requêtes HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // Politique de sécurité HSTS
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Configuration de la route par défaut
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=RoomLayout}/{action=Index}/{id?}");


// Charger les clés Stripe
var stripeSettings = builder.Configuration.GetSection("Stripe");

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];


// Lancer l'application
app.Run();
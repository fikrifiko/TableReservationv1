using Microsoft.EntityFrameworkCore;
using Stripe;
using Table_Reservation.Data;
using Table_Reservation.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Ajouter Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Ajouter les services au conteneur
builder.Services.AddControllersWithViews();

// Ajouter le service Entity Framework Core avec SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Injection des services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<SmsService>();

// Configuration de la lecture des paramètres
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// 🔹 Clé secrète pour JWT
var key = Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]);

// 🔹 Configuration de l'authentification avec JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // En développement, sinon true
    options.SaveToken = true; // Permet de sauvegarder le token
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = configuration["Jwt:Issuer"],
        ValidAudience = configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        RoleClaimType = ClaimTypes.Role
    };
});

// 🔹 Ajouter la gestion des sessions
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configuration du logging
builder.Logging.AddConsole();
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

// Ajout des services CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost",
        builder => builder.WithOrigins("https://localhost:44340")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials());
});

// Construire l'application
var app = builder.Build();

// Swagger UI (placé correctement après app)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configurer le pipeline des requêtes HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowLocalhost");

app.UseSession(); // Active la gestion des sessions
app.UseAuthentication(); // Active l’authentification des utilisateurs
app.UseAuthorization();  // Active les autorisations

// Middleware de diagnostic pour voir si l'utilisateur est authentifié
app.Use(async (context, next) =>
{
    Console.WriteLine($"🔸 Processing request: {context.Request.Path}");
    if (context.User.Identity?.IsAuthenticated == true)
    {
        Console.WriteLine($"✅ User authenticated: {context.User.Identity.Name}");
    }
    else
    {
        Console.WriteLine("🚫 User NOT authenticated");
    }
    await next();
});

// Configuration de la route par défaut
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "adminViews",
    pattern: "AdminView/{action=Dashboard}/{id?}",
    defaults: new { controller = "AdminView" });

// Charger les clés Stripe
StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];

// Lancer l'application
app.Run();


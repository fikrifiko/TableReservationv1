using Microsoft.EntityFrameworkCore;
using Stripe;
using Table_Reservation.Data;
using Table_Reservation.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Ajouter Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Table Reservation API", Version = "v1" });
    var securityScheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Entrer 'Bearer {token}'"
    };
    c.AddSecurityDefinition("Bearer", securityScheme);
    var securityRequirement = new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            }, new string[] { }
        }
    };
    c.AddSecurityRequirement(securityRequirement);
});

// Ajouter les services au conteneur
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews()
    .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();

// Ajouter le service Entity Framework Core avec SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Injection des services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<SmsService>();

// Configuration de la lecture des paramètres
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

// JWT key
var key = Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]);

// JWT authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true; // Save token
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = configuration["Jwt:Issuer"],
        ValidAudience = configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = ClaimTypes.Role
    };
    // Read token from cookie
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var tokenFromCookie = context.HttpContext.Request.Cookies["access_token"];
            if (!string.IsNullOrEmpty(tokenFromCookie) && string.IsNullOrEmpty(context.Token))
            {
                context.Token = tokenFromCookie;
            }
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            // Redirect 401 to home
            context.HandleResponse();
            context.Response.Redirect("/Home/Index?message=deconnected");
            return Task.CompletedTask;
        },
        OnForbidden = context =>
        {
            context.Response.Redirect("/Home/Index?message=deconnected");
            return Task.CompletedTask;
        }
    };
});

// Session
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

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost",
        builder => builder.WithOrigins("https://localhost:44340")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials());
});

// Rate limiting (login et upload)
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("login-policy", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anon",
        factory: key => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0
        }
    ));

    options.AddPolicy("upload-policy", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anon",
        factory: key => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 3,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0
        }
    ));
});

// Build app
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTP pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();

// Localization (must run before routing so views pick correct culture)
var supportedCultures = new[] { "fr-FR", "nl" };
var localizationOptions = new Microsoft.AspNetCore.Builder.RequestLocalizationOptions()
    .SetDefaultCulture("fr-FR")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
localizationOptions.RequestCultureProviders = new System.Collections.Generic.List<Microsoft.AspNetCore.Localization.IRequestCultureProvider>
{
    new Microsoft.AspNetCore.Localization.CookieRequestCultureProvider(),
    new Microsoft.AspNetCore.Localization.QueryStringRequestCultureProvider(),
    new Microsoft.AspNetCore.Localization.AcceptLanguageHeaderRequestCultureProvider()
};
app.UseRequestLocalization(localizationOptions);

app.UseRouting();

app.UseCors("AllowLocalhost");
app.UseRateLimiter();

app.UseSession(); // Session
app.UseAuthentication(); // Auth
app.UseAuthorization();  // Authorization

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    context.Response.Headers["Permissions-Policy"] = "geolocation=()";
    // CSP (dev: autorise connexions localhost et websockets pour BrowserLink/refresh)
    var csp = "default-src 'self'; img-src 'self' data:; script-src 'self'; style-src 'self' 'unsafe-inline'; font-src 'self'; frame-ancestors 'none'";
    if (app.Environment.IsDevelopment())
    {
        csp += "; connect-src 'self' http://localhost:* ws: wss:";
    }
    context.Response.Headers["Content-Security-Policy"] = csp;
    await next();
});

// Redirect 401/403 to home with message
app.Use(async (context, next) =>
{
    await next();
    if (context.Response.StatusCode == StatusCodes.Status401Unauthorized || context.Response.StatusCode == StatusCodes.Status403Forbidden)
    {
        context.Response.Redirect("/Home/Index?message=deconnected");
    }
});

// Diagnostic logging
app.Use(async (context, next) =>
{
    Console.WriteLine($"Processing request: {context.Request.Path}");
    if (context.User.Identity?.IsAuthenticated == true)
    {
        Console.WriteLine($"User authenticated: {context.User.Identity.Name}");
    }
    else
    {
        Console.WriteLine("User not authenticated");
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
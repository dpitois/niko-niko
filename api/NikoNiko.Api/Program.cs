using System.Net;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NikoNiko.Data;
using NikoNiko.Data.PostgreSql;
using NikoNiko.Data.Sqlite;
using NikoNiko.Services;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// 1. Add services to the container.
// -----------------------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ITeamInvitationService, TeamInvitationService>();

// Add HttpClient for SignalR Service communication
builder.Services.AddHttpClient<INotificationService, NotificationService>(client =>
{
    client.BaseAddress = new Uri(config["SignalRService:BaseUrl"]!);
});

// Configure DbContext based on the provider specified in appsettings.json
var dbProvider = config["DatabaseProvider"];
if (dbProvider == "SQLite")
{
    builder.Services.AddSqlitePersistence(config);
}
else
{
    builder.Services.AddPostgreSqlPersistence(config);
}

// Configure Data Protection
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/root/.aspnet/DataProtection-Keys"));

// Configure Forwarded Headers
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Only loopback proxies are trusted by default.
    // Clear that default and add the proxy that is running in docker-compose.
    // In a production environment, you might specify known proxies/networks.
    // For a typical Docker setup, trusting all is often necessary due to dynamic IPs.
    // Be cautious with this in a highly sensitive environment.
    options.KnownIPNetworks.Add(new System.Net.IPNetwork(IPAddress.Parse("0.0.0.0"), 0)); // Trust all networks
    options.KnownProxies.Add(IPAddress.Parse("0.0.0.0")); // Trust all proxies
});

// Configure Https Redirection
if (builder.Environment.IsProduction())
{
    builder.Services.AddHttpsRedirection(options =>
    {
        options.RedirectStatusCode = (int)HttpStatusCode.TemporaryRedirect;
        options.HttpsPort = 443;
    });
}

// Configure Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddCookie("ExternalCookie")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = config["Authentication:Jwt:Issuer"],
            ValidAudience = config["Authentication:Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Authentication:Jwt:Key"]!))
        };
    })
    .AddGitHub(options =>
    {
        options.SignInScheme = "ExternalCookie";
        options.ClientId = config["Authentication:GitHub:ClientId"]!;
        options.ClientSecret = config["Authentication:GitHub:ClientSecret"]!;
        options.CallbackPath = "/signin-github";
        options.Scope.Add("user:email");
    });

// Add services for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

builder.Services.AddHealthChecks();

// -----------------------------------------------------------------------------
var app = builder.Build();

if (app.Environment.IsProduction())
{
    app.UseForwardedHeaders();
}

// Apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

// 2. Configure the HTTP request pipeline.
// -----------------------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/healthz");

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Lax
});

// Configure Https Redirection
if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// -----------------------------------------------------------------------------
app.Run();
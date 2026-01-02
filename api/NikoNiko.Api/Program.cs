using System.Reflection;
using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using NikoNiko.Data; // Using the common data project
using NikoNiko.Data.PostgreSql;
using NikoNiko.Data.Sqlite;
using NikoNiko.Services; // Using the new services project

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// 1. Add services to the container.
// -----------------------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddScoped<ITokenService, TokenService>();

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
});

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

// -----------------------------------------------------------------------------
var app = builder.Build();

app.UseForwardedHeaders();

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

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Lax
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// -----------------------------------------------------------------------------
app.Run();
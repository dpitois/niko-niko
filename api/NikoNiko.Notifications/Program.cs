using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NikoNiko.Notifications.Hubs;
using NikoNiko.Notifications.Services;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddSignalR();
        builder.Services.AddSingleton<IUserConnectionManager, UserConnectionManager>();

        // Configure JWT Authentication
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Authentication:Jwt:Issuer"],
                ValidAudience = builder.Configuration["Authentication:Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Authentication:Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured.")))
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];

                    // If the request is for our hub...
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) &&
                        (path.StartsWithSegments("/notificationHub")))
                    {
                        // Read the token out of the query string
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        });
        builder.Services.AddAuthorization();

        builder.Services.AddHealthChecks();

        // Configure CORS
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                var allowedOrigins = builder.Configuration["Cors:AllowedOrigins"]?.Split(';', StringSplitOptions.RemoveEmptyEntries);
                if (allowedOrigins != null && allowedOrigins.Any())
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials(); // Important for SignalR
                }
                else
                {
                    // Fallback for development or if origins are not configured (e.g., allow all for local dev)
                    policy.AllowAnyOrigin() // WARNING: Use AllowAnyOrigin() with caution in production
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                }
            });
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage(); // More detailed errors in development
        }

        app.MapHealthChecks("/healthz");

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthentication();
        app.UseCors();

        app.UseAuthorization();

        app.MapControllers();

        app.MapHub<NotificationHub>("/notificationHub");

        app.Run();
    }
}

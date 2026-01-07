
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

        builder.Services.AddControllers(); // Add controllers for potential future API endpoints

        builder.Services.AddSignalR(); // Add SignalR services

        builder.Services.AddSingleton<IUserConnectionManager, UserConnectionManager>(); // Register UserConnectionManager



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



        app.UseHttpsRedirection();

        app.UseRouting(); // Use routing before UseCors



        app.UseAuthentication(); // Use authentication before authorization

        app.UseCors(); // Use CORS policy



        app.UseAuthorization(); // If you add authentication/authorization later



        app.MapControllers(); // Map controllers for potential future API endpoints

        app.MapHub<NotificationHub>("/notificationHub"); // Map SignalR Hub



        app.Run();

    }

}
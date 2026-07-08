using Domain.Entities.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Application.Interfaces.Logging;
using Application.Interfaces.Lookup;
using Application.Interfaces.Utilities;
using Application.Services.Auth;
using Application.Services.Identity;
using Application.Services.Logging;
using Application.Services.Lookup;
using Application.Services.Users;
using Application.Services.Utilities;
using Domain.Dtos.Auth;
using Domain.Interfaces.Auth;
using Domain.Interfaces.Users;
using Infrastructure.Persistence;
using System.Text;

namespace Application;

public static class ApplicationInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddScoped(typeof(ILoggerManager<>), typeof(LoggerManager<>));

        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<ApplicationUserManager>();
        services.AddScoped<UserManager<ApplicationUser>>(sp => sp.GetRequiredService<ApplicationUserManager>());
        services.AddScoped<SignInManager<ApplicationUser>, ApplicationSignInManager>();

        // Business-logic user operations live in UserService, not the UserManager.
        services.AddScoped<IUserService, UserService>();

        services.AddOptions<JwtSettings>()
            .BindConfiguration("JwtSettings")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuthService, AuthService>();

        //Lookup
        services.AddScoped<IExceptionsManager, ExceptionsManager>();


        //Utilities
        services.AddScoped<IFileManager, FileManager>();
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = !environment.IsDevelopment(); 
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings?.Issuer,
                    ValidAudience = jwtSettings?.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings?.SecretKey ?? string.Empty)),
                    ClockSkew = TimeSpan.Zero
                };
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<JwtBearerEvents>>();
                        logger.LogWarning("JWT authentication failed: {Error}", context.Exception.Message);
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        //builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
        //    .AddEntityFrameworkStores<ApplicationDbContext>()
        //    .AddDefaultTokenProviders();
        services
          .AddIdentityCore<ApplicationUser>(options =>
          {
              options.Password.RequiredLength = 6;
              options.Password.RequireDigit = false;
              options.Password.RequireNonAlphanumeric = false;
              options.Password.RequireUppercase = false;
              options.Password.RequireLowercase = false;
              options.Password.RequiredUniqueChars = 1;
              options.SignIn.RequireConfirmedAccount = false;
          })
          .AddRoles<ApplicationRole>()
          .AddEntityFrameworkStores<ApplicationDbContext>();
        return services;
    }
}

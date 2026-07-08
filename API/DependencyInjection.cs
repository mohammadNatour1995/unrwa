using Api.Middlewares;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using System.Collections.ObjectModel;
using System.Data;
using System.Text.Json.Serialization;

namespace Api;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddProblemDetails()
            .AddControllerWithJsonConfiguration()
            .AddConfiguredCors(configuration);

        return services;
    }

   

    public static IServiceCollection AddControllerWithJsonConfiguration(this IServiceCollection services)
    {
        services.AddControllers().AddJsonOptions(options =>
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);

        return services;
    }


    public static WebApplicationBuilder AddSerilogLogging(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        var columnOptions = new ColumnOptions();
        columnOptions.Store.Remove(StandardColumn.Properties);
        columnOptions.Store.Remove(StandardColumn.MessageTemplate);
        columnOptions.Store.Remove(StandardColumn.Id);

        columnOptions.AdditionalColumns = new Collection<SqlColumn>
        {
            new SqlColumn("Id", SqlDbType.UniqueIdentifier),
            new SqlColumn("UserId", SqlDbType.UniqueIdentifier),
            new SqlColumn("FunctionName", SqlDbType.NVarChar, true, 200),
            new SqlColumn("Parameters", SqlDbType.NVarChar, true, -1),
            new SqlColumn("RequestPath", SqlDbType.NVarChar, true, 500)
        };

        var logConfig = new LoggerConfiguration()
            .MinimumLevel.Warning()
            .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Error)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentName();

        if (builder.Environment.IsDevelopment())
            logConfig.WriteTo.Console();

        logConfig
            .WriteTo.File(
                new Serilog.Formatting.Json.JsonFormatter(renderMessage: true),
                "logs/API.log.json",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30)
            .WriteTo.MSSqlServer(
                connectionString: configuration.GetConnectionString("SerilogConnection"),
                sinkOptions: new MSSqlServerSinkOptions
                {
                    TableName = "Logs",
                    AutoCreateSqlTable = true
                },
                columnOptions: columnOptions,
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error);

        Log.Logger = logConfig.CreateLogger();
        builder.Host.UseSerilog();

        return builder;
    }

    public static IServiceCollection AddConfiguredCors(this IServiceCollection services, IConfiguration configuration)
    {
        var policyName = configuration["Cors:CorsPolicyName"] ?? "CorsPolicy";
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

        services.AddCors(options => options.AddPolicy(policyName, policy =>
        {
            if (allowedOrigins.Length > 0)
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            }
            else
            {
                // No origins configured — reject all cross-origin requests.
                policy.AllowAnyHeader().AllowAnyMethod();
            }
        }));

        return services;
    }

    public static IApplicationBuilder UseCoreMiddlewares(this IApplicationBuilder app, IConfiguration configuration)
    {
        // 1. Global exception handler — must be first so it catches errors from all subsequent middleware
        app.UseExceptionHandler();

        // 2. HTTPS redirection
        app.UseHttpsRedirection();

        // 3. Serilog request logging
        app.UseSerilogRequestLogging();

        // 5. CORS
        app.UseCors(configuration["Cors:CorsPolicyName"]!);

        // 6. Authentication → Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        // 7. Populate ICurrentUser from JWT claims
        app.UseCurrentUserMiddleware();

        // 8. Output caching (after auth so cache is keyed per user if needed)

        return app;
    }
}

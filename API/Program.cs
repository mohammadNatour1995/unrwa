using Api;
using Application;
using Domain.Dtos;
using Microsoft.Extensions.Hosting;
using Serilog;


try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddSerilogLogging(builder.Configuration);
    builder.Services.Configure<APISettings>(builder.Configuration.GetSection("APISettings"));
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddControllers();

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddControllers()
          .AddJsonOptions(options =>
          {
              options.JsonSerializerOptions.PropertyNamingPolicy = null;
          });
    builder.Services
        .AddPresentation(builder.Configuration)
        .AddInfrastructureServices(builder.Configuration)
        .AddApplicationServices(builder.Configuration, builder.Environment);

    builder.Services.AddSwaggerGen();

    var app = builder.Build();


    app.UseStaticFiles();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    else
    {
        app.UseHsts();
    }

    app.UseCoreMiddlewares(builder.Configuration);

    app.MapControllers();

    app.Run();
}
catch (HostAbortedException)
{
    // Thrown by EF Core design-time tooling (dotnet ef) after it captures
    // the built host — not a real startup failure, so don't log it.
}
catch (Exception ex)
{
    Log.ForContext("Id", Guid.NewGuid()).Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}

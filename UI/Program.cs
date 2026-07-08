using AdmiUI.Helpers;
using Domain.Dtos;
using Domain.Dtos.Auth;
using UI.Helpers;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var apiPath = builder.Configuration["AppSettings:APIPath"]
    ?? throw new InvalidOperationException("AppSettings:APIPath is not configured.");
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri(apiPath);
    client.Timeout = TimeSpan.FromSeconds(500);
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<IHttpClientHelper, HttpClientHelper>();
builder.Services.Configure<AppSettings>(
    builder.Configuration.GetSection("AppSettings"));
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddControllers()
          .AddJsonOptions(options =>
          {
              options.JsonSerializerOptions.PropertyNamingPolicy = null; 
          });
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Error()
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .WriteTo.File(
        new Serilog.Formatting.Json.JsonFormatter(renderMessage: true),
        "logs/Web.log.json",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error
    ).CreateLogger();

builder.Host.UseSerilog();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // Handle unhandled exceptions (500) -> /Errors/500
    app.UseExceptionHandler("/Errors/500");
    //app.UseHsts();
}
app.Use(async (context, next) =>
{
    context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "0";
    await next();
});
app.UseStatusCodePages(async context =>
{
    var http = context.HttpContext;
    var status = http.Response.StatusCode;

    if (status == 404 &&
        !http.Request.Path.StartsWithSegments("/errors", StringComparison.OrdinalIgnoreCase))
    {
        http.Response.Redirect("/Errors/404");
    }
});
//app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}")
//    .WithStaticAssets();
app.MapControllerRoute(name: "areas", pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
       name: "default",
       pattern: "{controller=Account}/{action=Signin}/{id?}");

app.Run();

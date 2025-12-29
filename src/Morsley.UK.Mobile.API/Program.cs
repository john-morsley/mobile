var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddAzureWebAppDiagnostics();

builder.Configuration.AddUserSecrets<Program>();
builder.ConfigureAzureKeyVault();

builder
    .Services
        .AddOptions<TwilioSettings>()
        .Bind(builder.Configuration.GetSection("TwilioSettings"))
        .ValidateOnStart();

builder.Services.AddSingleton<IValidateOptions<TwilioSettings>, TwilioSettingsValidator>();

builder.Services.AddControllersWithViews();

builder.Services.AddSmsSender(builder.Configuration);
builder.Services.AddMobilePersistence(builder.Configuration);

var startupHealthCheck = new StartupHealthCheck();
builder.Services.AddSingleton(startupHealthCheck);
builder.Services.AddHealthChecks()
    .AddCheck("startup", () => startupHealthCheck.CheckHealthAsync(new HealthCheckContext()).Result)
    .AddCheck<CosmosDbHealthCheck>("cosmosdb", tags: new[] { "ready", "db" });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Morsley UK Mobile API",
        Version = "v1",
        Description = "API for sending and reading mobile messages."
    });
});

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application starting up - Morsley UK Mobile API");

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.InitializeCosmosDbAsync(throwOnError: false);
}

startupHealthCheck.MarkStartupComplete();
logger.LogInformation("Application started successfully");

if (app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        logger.LogInformation("Incoming {Method} {Scheme}://{Host}{Path}{QueryString}",
            context.Request.Method,
            context.Request.Scheme,
            context.Request.Host.Value,
            context.Request.Path.Value,
            context.Request.QueryString.Value);

        logger.LogInformation(
            "Request headers: ContentType={ContentType}; ContentLength={ContentLength}; TransferEncoding={TransferEncoding}; Expect={Expect}",
            context.Request.ContentType,
            context.Request.ContentLength,
            context.Request.Headers.TransferEncoding.ToString(),
            context.Request.Headers.Expect.ToString());

        await next();

        logger.LogInformation("Completed {Method} {Path} -> {StatusCode}",
            context.Request.Method,
            context.Request.Path.Value,
            context.Response.StatusCode);
    });
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Morsley UK Mobile API");
});

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Name == "startup",
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();

public partial class Program { }
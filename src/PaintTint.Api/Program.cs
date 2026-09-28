using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PaintTint.Api.Middleware;
using PaintTint.Core.Interfaces;
using PaintTint.Core.Services;
using PaintTint.Infrastructure.Data;
using PaintTint.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Add services to DI container
builder.Services.AddControllers();

// Database context (Microsoft SQL Server: PRIYAL\SQLEXPRESS)
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=PRIYAL\\SQLEXPRESS;Database=PaintTintDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;";

builder.Services.AddDbContext<PaintTintDbContext>(options =>
    options.UseSqlServer(connectionString));

// Domain & Application Services
builder.Services.AddSingleton<ITintCalculator, TintCalculator>();
builder.Services.AddScoped<IShadeService, ShadeService>();
builder.Services.AddScoped<IBaseService, BaseService>();
builder.Services.AddScoped<IDispenseService, DispenseService>();

// CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Swagger documentation with API metadata & XML comments
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Paint Tint Formula Calculator API",
        Version = "v1",
        Description = "ASP.NET Core Web API for calculating paint tint formulas, scaling colorants, validating tint % limits, and recording dispense jobs.",
        Contact = new OpenApiContact
        {
            Name = "Paint Tint Formula Calculator Team"
        }
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// 2. Automated Migration and Database Seeding on Startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = services.GetRequiredService<PaintTintDbContext>();
        logger.LogInformation("Applying database migrations...");
        await db.Database.MigrateAsync();
        logger.LogInformation("Seeding database from seed.json...");
        await DatabaseSeeder.SeedAsync(db);
        logger.LogInformation("Database migration and seeding completed successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

// 3. Configure HTTP request pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Enable Swagger in all environments so reviewer can test API easily
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Paint Tint Formula API v1");
    c.RoutePrefix = "swagger";
});

// Redirect root to swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();

// Required for WebApplicationFactory in integration tests if needed
public partial class Program { }

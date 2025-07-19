using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using RecipeOptimizer.Core.Interfaces;
using RecipeOptimizer.Core.Services;
using RecipeOptimizer.Infrastructure.Data;
using RecipeOptimizer.Infrastructure.Repositories;
using System.Text.Json.Serialization;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Configure JSON serialization to handle circular references
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// Add health checks including database
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection"), 
               name: "postgresql", 
               tags: new[] { "database", "postgresql" });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Recipe Optimizer API", Version = "v1" });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Add DbContext
builder.Services.AddDbContext<RecipeOptimizerDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    Console.WriteLine($"Configuring DbContext with connection string template: {connectionString}");
    
    options.UseNpgsql(connectionString, npgsqlOptions => 
    {
        npgsqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
        npgsqlOptions.CommandTimeout(30);
    });
    
    // Enable detailed errors in production
    if (!builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
    }
});

// Register repositories
builder.Services.AddScoped<IIngredientRepository, IngredientRepository>();
builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();

// Register services
builder.Services.AddScoped<IRecipeOptimizerService, RecipeOptimizerService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Recipe Optimizer API v1"));
}

// Add diagnostic endpoint for Azure troubleshooting
app.MapGet("/diagnostics", () => {
    var envVars = new Dictionary<string, string>
    {
        { "POSTGRESQLHOST", Environment.GetEnvironmentVariable("POSTGRESQLHOST") ?? "Not set" },
        { "POSTGRESQLPORT", Environment.GetEnvironmentVariable("POSTGRESQLPORT") ?? "Not set" },
        { "POSTGRESQLDATABASE", Environment.GetEnvironmentVariable("POSTGRESQLDATABASE") ?? "Not set" },
        { "POSTGRESQLUSER", Environment.GetEnvironmentVariable("POSTGRESQLUSER") ?? "Not set" },
        { "POSTGRESQLPASSWORD", Environment.GetEnvironmentVariable("POSTGRESQLPASSWORD") != null ? "Set (value hidden)" : "Not set" },
        { "ASPNETCORE_ENVIRONMENT", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Not set" }
    };
    
    var connectionString = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .AddEnvironmentVariables()
        .Build()
        .GetConnectionString("DefaultConnection");
        
    var diagnosticInfo = new {
        Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Not set",
        EnvironmentVariables = envVars,
        ConnectionStringConfigured = !string.IsNullOrEmpty(connectionString) ? "Yes (value hidden)" : "No",
        CurrentDirectory = Directory.GetCurrentDirectory(),
        OSDescription = System.Runtime.InteropServices.RuntimeInformation.OSDescription
    };
    
    return diagnosticInfo;
});

// Add global exception handler middleware
app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        
        var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
        if (contextFeature != null)
        {
            // Log the error
            Console.WriteLine($"[ERROR] {DateTime.UtcNow}: {contextFeature.Error}");
            
            // Return detailed error in development, generic in production
            var response = new 
            {
                StatusCode = context.Response.StatusCode,
                Message = app.Environment.IsDevelopment() ? contextFeature.Error.Message : "Internal Server Error",
                Detail = app.Environment.IsDevelopment() ? contextFeature.Error.StackTrace : string.Empty
            };
                
            await context.Response.WriteAsJsonAsync(response);
        }
    });
});

app.UseHttpsRedirection();

// Enable CORS
app.UseCors();

app.UseAuthorization();

app.MapControllers();

// Map health check endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            Status = report.Status.ToString(),
            Duration = report.TotalDuration,
            Info = report.Entries.Select(e => new
            {
                Key = e.Key,
                Status = e.Value.Status.ToString(),
                Duration = e.Value.Duration,
                Description = e.Value.Description,
                Error = e.Value.Exception?.Message
            })
        };
        
        await context.Response.WriteAsJsonAsync(response);
    }
});

app.UseDefaultFiles();
app.UseStaticFiles();

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<RecipeOptimizerDbContext>();
        
        // Log connection string (without password) for debugging
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(connectionString))
        {
            var sanitizedConnectionString = connectionString.Contains("Password=") 
                ? connectionString.Substring(0, connectionString.IndexOf("Password=")) + "Password=***" 
                : connectionString;
            Console.WriteLine($"Attempting to connect with: {sanitizedConnectionString}");
        }
        else
        {
            Console.WriteLine("WARNING: Connection string is null or empty!");
        }
        
        // Create database and apply any pending migrations
        // This will also apply the seed data defined in OnModelCreating
        context.Database.EnsureCreated();
        
        // Check if database has data
        if (context.Ingredients.Any())
        {
            Console.WriteLine("Database already contains data, seeding skipped.");
        }
        else
        {
            Console.WriteLine("Database was created and seeded successfully.");
        }
    }
    catch (DbUpdateException dbEx)
    {
        Console.WriteLine($"A database update error occurred while initializing the database: {dbEx.Message}");
        Console.WriteLine($"Exception type: {dbEx.GetType().Name}");
        Console.WriteLine($"Stack trace: {dbEx.StackTrace}");
        
        if (dbEx.InnerException != null)
        {
            Console.WriteLine($"Inner exception: {dbEx.InnerException.Message}");
            Console.WriteLine($"Inner exception type: {dbEx.InnerException.GetType().Name}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while initializing the database: {ex.Message}");
        Console.WriteLine($"Exception type: {ex.GetType().Name}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            Console.WriteLine($"Inner exception type: {ex.InnerException.GetType().Name}");
        }
    }
}

app.Run();

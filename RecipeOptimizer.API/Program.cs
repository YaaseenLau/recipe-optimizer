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
app.MapGet("/diagnostics", async (IServiceProvider serviceProvider) => {
    var envVars = new Dictionary<string, string>
    {
        { "POSTGRESQLHOST", Environment.GetEnvironmentVariable("POSTGRESQLHOST") ?? "Not set" },
        { "POSTGRESQLPORT", Environment.GetEnvironmentVariable("POSTGRESQLPORT") ?? "Not set" },
        { "POSTGRESQLDATABASE", Environment.GetEnvironmentVariable("POSTGRESQLDATABASE") ?? "Not set" },
        { "POSTGRESQLUSER", Environment.GetEnvironmentVariable("POSTGRESQLUSER") ?? "Not set" },
        { "POSTGRESQLPASSWORD", Environment.GetEnvironmentVariable("POSTGRESQLPASSWORD") != null ? "Set (value hidden)" : "Not set" },
        { "ASPNETCORE_ENVIRONMENT", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Not set" }
    };
    
    var config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .AddEnvironmentVariables()
        .Build();
        
    var connectionString = config.GetConnectionString("DefaultConnection");
    var sanitizedConnectionString = "Not available";
    
    if (!string.IsNullOrEmpty(connectionString))
    {
        sanitizedConnectionString = connectionString.Contains("Password=") 
            ? connectionString.Substring(0, connectionString.IndexOf("Password=")) + "Password=***" 
            : connectionString;
    }
    
    // Test database connection
    string dbConnectionStatus = "Not tested";
    string dbConnectionError = null;
    
    try
    {   
        using (var scope = serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<RecipeOptimizerDbContext>();
            
            // Test connection by checking if database exists
            bool canConnect = await dbContext.Database.CanConnectAsync();
            dbConnectionStatus = canConnect ? "Success" : "Failed";
            
            if (canConnect)
            {
                // Try to get a count of ingredients to verify data access
                int ingredientCount = await dbContext.Ingredients.CountAsync();
                dbConnectionStatus = $"Success - {ingredientCount} ingredients found";
            }
        }
    }
    catch (Exception ex)
    {
        dbConnectionStatus = "Error";
        dbConnectionError = $"{ex.GetType().Name}: {ex.Message}";
        
        if (ex.InnerException != null)
        {
            dbConnectionError += $" | Inner: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}";
        }
    }
    
    var diagnosticInfo = new {
        Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Not set",
        EnvironmentVariables = envVars,
        ConnectionStringTemplate = sanitizedConnectionString,
        ConnectionStringConfigured = !string.IsNullOrEmpty(connectionString) ? "Yes" : "No",
        DatabaseConnectionStatus = dbConnectionStatus,
        DatabaseConnectionError = dbConnectionError,
        CurrentDirectory = Directory.GetCurrentDirectory(),
        OSDescription = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
        ServerTime = DateTime.UtcNow.ToString("o")
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
            
            // TEMPORARY: Show detailed errors in all environments for troubleshooting
            var response = new 
            {
                StatusCode = context.Response.StatusCode,
                Message = contextFeature.Error.Message,
                Detail = contextFeature.Error.StackTrace,
                InnerError = contextFeature.Error.InnerException?.Message ?? string.Empty,
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Not set"
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
            
            // Check for common Azure PostgreSQL issues
            if (connectionString.Contains("${POSTGRESQLHOST}") || 
                connectionString.Contains("${POSTGRESQLPORT}") || 
                connectionString.Contains("${POSTGRESQLDATABASE}") || 
                connectionString.Contains("${POSTGRESQLUSER}") || 
                connectionString.Contains("${POSTGRESQLPASSWORD}"))
            {
                Console.WriteLine("ERROR: Environment variables in connection string were not replaced!");
                Console.WriteLine("Check that environment variables are properly set and accessible.");
            }
            
            // Check SSL settings for Azure PostgreSQL
            if (!connectionString.Contains("SSL Mode=") || !connectionString.Contains("Trust Server Certificate="))
            {
                Console.WriteLine("WARNING: Connection string may be missing required Azure PostgreSQL SSL settings.");
                Console.WriteLine("Azure PostgreSQL requires 'SSL Mode=Require;Trust Server Certificate=true'");
            }
        }
        else
        {
            Console.WriteLine("WARNING: Connection string is null or empty!");
        }
        
        Console.WriteLine("Attempting database connection...");
        bool canConnect = context.Database.CanConnect();
        Console.WriteLine($"Database connection test: {(canConnect ? "SUCCESS" : "FAILED")}");
        
        if (canConnect)
        {
            // Create database and apply any pending migrations
            // This will also apply the seed data defined in OnModelCreating
            Console.WriteLine("Ensuring database is created...");
            context.Database.EnsureCreated();
            
            // Check if database has data
            Console.WriteLine("Checking for existing data...");
            if (context.Ingredients.Any())
            {
                Console.WriteLine("Database already contains data, seeding skipped.");
            }
            else
            {
                Console.WriteLine("Database was created and seeded successfully.");
            }
        }
        else
        {
            Console.WriteLine("Cannot proceed with database initialization due to connection failure.");
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

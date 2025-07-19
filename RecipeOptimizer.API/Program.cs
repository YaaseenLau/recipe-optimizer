using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using RecipeOptimizer.Core.Interfaces;
using RecipeOptimizer.Core.Services;
using RecipeOptimizer.Infrastructure.Data;
using RecipeOptimizer.Infrastructure.Repositories;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Configure JSON serialization to handle circular references
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { 
        Title = "Recipe Optimizer API", 
        Version = "v1",
        Description = "An API to optimize recipes based on available ingredients"
    });
});

// Add CORS policy for Angular frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        builder => builder
            .AllowAnyOrigin()  // In production, replace with your Angular app's URL
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Configure database
builder.Services.AddDbContext<RecipeOptimizerDbContext>(options =>
{
    // Use PostgreSQL for both development and production
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
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
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Recipe Optimizer API v1"));
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowAngularApp");

app.UseAuthorization();
app.MapControllers();

// Enable serving static files and set default page
app.UseDefaultFiles();
app.UseStaticFiles();

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<RecipeOptimizerDbContext>();
        
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
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while initializing the database: {ex.Message}");
    }
}

app.Run();

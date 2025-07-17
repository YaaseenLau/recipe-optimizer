using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace RecipeOptimizer.Infrastructure.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<RecipeOptimizerDbContext>
    {
        public RecipeOptimizerDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<RecipeOptimizerDbContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Port=5433;Database=recipeoptimizer;Username=postgres;Password=postgres");

            return new RecipeOptimizerDbContext(optionsBuilder.Options);
        }
    }
}

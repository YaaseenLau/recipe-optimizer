using Microsoft.EntityFrameworkCore;
using RecipeOptimizer.Core.Models;

namespace RecipeOptimizer.Infrastructure.Data
{
    public class RecipeOptimizerDbContext : DbContext
    {
        public RecipeOptimizerDbContext(DbContextOptions<RecipeOptimizerDbContext> options)
            : base(options)
        {
        }

        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<RecipeIngredient> RecipeIngredients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Ingredient entity
            modelBuilder.Entity<Ingredient>()
                .HasKey(i => i.Id);

            modelBuilder.Entity<Ingredient>()
                .Property(i => i.Name)
                .IsRequired()
                .HasMaxLength(100);

            // Configure Recipe entity
            modelBuilder.Entity<Recipe>()
                .HasKey(r => r.Id);

            modelBuilder.Entity<Recipe>()
                .Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(100);

            // Configure RecipeIngredient entity
            modelBuilder.Entity<RecipeIngredient>()
                .HasKey(ri => ri.Id);

            modelBuilder.Entity<RecipeIngredient>()
                .HasOne(ri => ri.Recipe)
                .WithMany(r => r.Ingredients)
                .HasForeignKey(ri => ri.RecipeId);

            modelBuilder.Entity<RecipeIngredient>()
                .HasOne(ri => ri.Ingredient)
                .WithMany()
                .HasForeignKey(ri => ri.IngredientId);

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed ingredients
            modelBuilder.Entity<Ingredient>().HasData(
                new Ingredient { Id = 1, Name = "Meat", AvailableQuantity = 6 },
                new Ingredient { Id = 2, Name = "Lettuce", AvailableQuantity = 3 },
                new Ingredient { Id = 3, Name = "Tomato", AvailableQuantity = 6 },
                new Ingredient { Id = 4, Name = "Cheese", AvailableQuantity = 8 },
                new Ingredient { Id = 5, Name = "Dough", AvailableQuantity = 10 },
                new Ingredient { Id = 6, Name = "Cucumber", AvailableQuantity = 2 },
                new Ingredient { Id = 7, Name = "Olives", AvailableQuantity = 2 }
            );

            // Seed recipes
            modelBuilder.Entity<Recipe>().HasData(
                new Recipe { Id = 1, Name = "Burger", ServingSize = 1 },
                new Recipe { Id = 2, Name = "Pie", ServingSize = 1 },
                new Recipe { Id = 3, Name = "Sandwich", ServingSize = 1 },
                new Recipe { Id = 4, Name = "Pasta", ServingSize = 2 },
                new Recipe { Id = 5, Name = "Salad", ServingSize = 3 },
                new Recipe { Id = 6, Name = "Pizza", ServingSize = 4 }
            );

            // Seed recipe ingredients
            modelBuilder.Entity<RecipeIngredient>().HasData(
                // Burger ingredients
                new RecipeIngredient { Id = 1, RecipeId = 1, IngredientId = 1, RequiredQuantity = 1 },  // Meat
                new RecipeIngredient { Id = 2, RecipeId = 1, IngredientId = 2, RequiredQuantity = 1 },  // Lettuce
                new RecipeIngredient { Id = 3, RecipeId = 1, IngredientId = 3, RequiredQuantity = 1 },  // Tomato
                new RecipeIngredient { Id = 4, RecipeId = 1, IngredientId = 4, RequiredQuantity = 1 },  // Cheese
                new RecipeIngredient { Id = 5, RecipeId = 1, IngredientId = 5, RequiredQuantity = 1 },  // Dough

                // Pie ingredients
                new RecipeIngredient { Id = 6, RecipeId = 2, IngredientId = 1, RequiredQuantity = 2 },  // Meat
                new RecipeIngredient { Id = 7, RecipeId = 2, IngredientId = 5, RequiredQuantity = 2 },  // Dough

                // Sandwich ingredients
                new RecipeIngredient { Id = 8, RecipeId = 3, IngredientId = 5, RequiredQuantity = 1 },  // Dough
                new RecipeIngredient { Id = 9, RecipeId = 3, IngredientId = 6, RequiredQuantity = 1 },  // Cucumber

                // Pasta ingredients
                new RecipeIngredient { Id = 10, RecipeId = 4, IngredientId = 1, RequiredQuantity = 1 }, // Meat
                new RecipeIngredient { Id = 11, RecipeId = 4, IngredientId = 3, RequiredQuantity = 1 }, // Tomato
                new RecipeIngredient { Id = 12, RecipeId = 4, IngredientId = 4, RequiredQuantity = 2 }, // Cheese
                new RecipeIngredient { Id = 13, RecipeId = 4, IngredientId = 5, RequiredQuantity = 2 }, // Dough

                // Salad ingredients
                new RecipeIngredient { Id = 14, RecipeId = 5, IngredientId = 6, RequiredQuantity = 1 }, // Cucumber
                new RecipeIngredient { Id = 15, RecipeId = 5, IngredientId = 2, RequiredQuantity = 2 }, // Lettuce
                new RecipeIngredient { Id = 16, RecipeId = 5, IngredientId = 3, RequiredQuantity = 2 }, // Tomato
                new RecipeIngredient { Id = 17, RecipeId = 5, IngredientId = 4, RequiredQuantity = 2 }, // Cheese
                new RecipeIngredient { Id = 18, RecipeId = 5, IngredientId = 7, RequiredQuantity = 1 }, // Olives

                // Pizza ingredients
                new RecipeIngredient { Id = 19, RecipeId = 6, IngredientId = 7, RequiredQuantity = 1 }, // Olives
                new RecipeIngredient { Id = 20, RecipeId = 6, IngredientId = 3, RequiredQuantity = 2 }, // Tomato
                new RecipeIngredient { Id = 21, RecipeId = 6, IngredientId = 4, RequiredQuantity = 3 }, // Cheese
                new RecipeIngredient { Id = 22, RecipeId = 6, IngredientId = 5, RequiredQuantity = 3 }  // Dough
            );
        }
    }
}

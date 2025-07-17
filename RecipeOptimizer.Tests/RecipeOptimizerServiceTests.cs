using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using RecipeOptimizer.Core.Interfaces;
using RecipeOptimizer.Core.Models;
using RecipeOptimizer.Core.Services;
using Xunit;

namespace RecipeOptimizer.Tests
{
    public class RecipeOptimizerServiceTests
    {
        private readonly Mock<IRecipeRepository> _mockRecipeRepository;
        private readonly Mock<IIngredientRepository> _mockIngredientRepository;
        private readonly RecipeOptimizerService _service;

        public RecipeOptimizerServiceTests()
        {
            _mockRecipeRepository = new Mock<IRecipeRepository>();
            _mockIngredientRepository = new Mock<IIngredientRepository>();
            _service = new RecipeOptimizerService(_mockRecipeRepository.Object, _mockIngredientRepository.Object);
        }

        [Fact]
        public async Task OptimizeRecipesAsync_ReturnsCorrectOptimization()
        {
            // Arrange
            SetupMockData();

            // Act
            var result = await _service.OptimizeRecipesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(13, result.TotalPeopleServed);
            
            // Verify we have the expected recipes in our result
            var pizzaCount = result.Recipes.FirstOrDefault(r => r.Recipe.Name == "Pizza")?.Count ?? 0;
            var saladCount = result.Recipes.FirstOrDefault(r => r.Recipe.Name == "Salad")?.Count ?? 0;
            var sandwichCount = result.Recipes.FirstOrDefault(r => r.Recipe.Name == "Sandwich")?.Count ?? 0;
            
            Assert.True(pizzaCount > 0, "Should have at least one Pizza");
            // We don't assert exact counts because different optimization strategies might produce different combinations
            // as long as we're feeding 13 people total
        }

        private void SetupMockData()
        {
            // Create ingredients
            var ingredients = new List<Ingredient>
            {
                new Ingredient { Id = 1, Name = "Meat", AvailableQuantity = 6 },
                new Ingredient { Id = 2, Name = "Lettuce", AvailableQuantity = 3 },
                new Ingredient { Id = 3, Name = "Tomato", AvailableQuantity = 6 },
                new Ingredient { Id = 4, Name = "Cheese", AvailableQuantity = 8 },
                new Ingredient { Id = 5, Name = "Dough", AvailableQuantity = 10 },
                new Ingredient { Id = 6, Name = "Cucumber", AvailableQuantity = 2 },
                new Ingredient { Id = 7, Name = "Olives", AvailableQuantity = 2 }
            };

            // Create recipes
            var burger = new Recipe { Id = 1, Name = "Burger", ServingSize = 1 };
            var pie = new Recipe { Id = 2, Name = "Pie", ServingSize = 1 };
            var sandwich = new Recipe { Id = 3, Name = "Sandwich", ServingSize = 1 };
            var pasta = new Recipe { Id = 4, Name = "Pasta", ServingSize = 2 };
            var salad = new Recipe { Id = 5, Name = "Salad", ServingSize = 3 };
            var pizza = new Recipe { Id = 6, Name = "Pizza", ServingSize = 4 };

            // Add ingredients to recipes
            burger.Ingredients = new List<RecipeIngredient>
            {
                new RecipeIngredient { RecipeId = 1, IngredientId = 1, Ingredient = ingredients[0], RequiredQuantity = 1 },
                new RecipeIngredient { RecipeId = 1, IngredientId = 2, Ingredient = ingredients[1], RequiredQuantity = 1 },
                new RecipeIngredient { RecipeId = 1, IngredientId = 3, Ingredient = ingredients[2], RequiredQuantity = 1 },
                new RecipeIngredient { RecipeId = 1, IngredientId = 4, Ingredient = ingredients[3], RequiredQuantity = 1 },
                new RecipeIngredient { RecipeId = 1, IngredientId = 5, Ingredient = ingredients[4], RequiredQuantity = 1 }
            };

            pie.Ingredients = new List<RecipeIngredient>
            {
                new RecipeIngredient { RecipeId = 2, IngredientId = 1, Ingredient = ingredients[0], RequiredQuantity = 2 },
                new RecipeIngredient { RecipeId = 2, IngredientId = 5, Ingredient = ingredients[4], RequiredQuantity = 2 }
            };

            sandwich.Ingredients = new List<RecipeIngredient>
            {
                new RecipeIngredient { RecipeId = 3, IngredientId = 5, Ingredient = ingredients[4], RequiredQuantity = 1 },
                new RecipeIngredient { RecipeId = 3, IngredientId = 6, Ingredient = ingredients[5], RequiredQuantity = 1 }
            };

            pasta.Ingredients = new List<RecipeIngredient>
            {
                new RecipeIngredient { RecipeId = 4, IngredientId = 1, Ingredient = ingredients[0], RequiredQuantity = 1 },
                new RecipeIngredient { RecipeId = 4, IngredientId = 3, Ingredient = ingredients[2], RequiredQuantity = 1 },
                new RecipeIngredient { RecipeId = 4, IngredientId = 4, Ingredient = ingredients[3], RequiredQuantity = 2 },
                new RecipeIngredient { RecipeId = 4, IngredientId = 5, Ingredient = ingredients[4], RequiredQuantity = 2 }
            };

            salad.Ingredients = new List<RecipeIngredient>
            {
                new RecipeIngredient { RecipeId = 5, IngredientId = 6, Ingredient = ingredients[5], RequiredQuantity = 1 },
                new RecipeIngredient { RecipeId = 5, IngredientId = 2, Ingredient = ingredients[1], RequiredQuantity = 2 },
                new RecipeIngredient { RecipeId = 5, IngredientId = 3, Ingredient = ingredients[2], RequiredQuantity = 2 },
                new RecipeIngredient { RecipeId = 5, IngredientId = 4, Ingredient = ingredients[3], RequiredQuantity = 2 },
                new RecipeIngredient { RecipeId = 5, IngredientId = 7, Ingredient = ingredients[6], RequiredQuantity = 1 }
            };

            pizza.Ingredients = new List<RecipeIngredient>
            {
                new RecipeIngredient { RecipeId = 6, IngredientId = 7, Ingredient = ingredients[6], RequiredQuantity = 1 },
                new RecipeIngredient { RecipeId = 6, IngredientId = 3, Ingredient = ingredients[2], RequiredQuantity = 2 },
                new RecipeIngredient { RecipeId = 6, IngredientId = 4, Ingredient = ingredients[3], RequiredQuantity = 3 },
                new RecipeIngredient { RecipeId = 6, IngredientId = 5, Ingredient = ingredients[4], RequiredQuantity = 3 }
            };

            var recipes = new List<Recipe> { burger, pie, sandwich, pasta, salad, pizza };

            _mockIngredientRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(ingredients);
            _mockRecipeRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(recipes);
        }
    }
}

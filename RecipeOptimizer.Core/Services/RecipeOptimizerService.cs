using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RecipeOptimizer.Core.Interfaces;
using RecipeOptimizer.Core.Models;

namespace RecipeOptimizer.Core.Services
{
    public class RecipeOptimizerService : IRecipeOptimizerService
    {
        private readonly IRecipeRepository _recipeRepository;
        private readonly IIngredientRepository _ingredientRepository;

        public RecipeOptimizerService(
            IRecipeRepository recipeRepository,
            IIngredientRepository ingredientRepository)
        {
            _recipeRepository = recipeRepository;
            _ingredientRepository = ingredientRepository;
        }

        public async Task<OptimizationResult> OptimizeRecipesAsync()
        {
            // Get all recipes and ingredients
            var recipes = await _recipeRepository.GetAllAsync();
            var ingredients = await _ingredientRepository.GetAllAsync();

            // Create a dictionary of available ingredients
            var availableIngredients = ingredients.ToDictionary(i => i.Name, i => i.AvailableQuantity);
            
            // Create a deep copy of available ingredients to work with
            var workingIngredients = new Dictionary<string, int>(availableIngredients);
            
            // Sort recipes by people served per recipe (descending)
            var sortedRecipes = recipes.OrderByDescending(r => r.ServingSize).ToList();
            
            var result = new OptimizationResult();
            
            // Greedy algorithm to maximize people served
            foreach (var recipe in sortedRecipes)
            {
                // Keep making this recipe as long as we have ingredients
                while (CanMakeRecipe(recipe, workingIngredients))
                {
                    // Use ingredients to make this recipe
                    UseIngredientsForRecipe(recipe, workingIngredients);
                    
                    // Add to our result
                    var existingRecipeCount = result.Recipes.FirstOrDefault(rc => rc.Recipe.Id == recipe.Id);
                    if (existingRecipeCount != null)
                    {
                        existingRecipeCount.Count++;
                    }
                    else
                    {
                        result.Recipes.Add(new RecipeCount { Recipe = recipe, Count = 1 });
                    }
                    
                    result.TotalPeopleServed += recipe.ServingSize;
                }
            }
            
            // Store remaining ingredients
            result.RemainingIngredients = workingIngredients;
            
            return result;
        }
        
        private bool CanMakeRecipe(Recipe recipe, Dictionary<string, int> availableIngredients)
        {
            foreach (var ingredient in recipe.Ingredients)
            {
                if (!availableIngredients.TryGetValue(ingredient.Ingredient.Name, out int quantity) || 
                    quantity < ingredient.RequiredQuantity)
                {
                    return false;
                }
            }
            return true;
        }
        
        private void UseIngredientsForRecipe(Recipe recipe, Dictionary<string, int> availableIngredients)
        {
            foreach (var ingredient in recipe.Ingredients)
            {
                availableIngredients[ingredient.Ingredient.Name] -= ingredient.RequiredQuantity;
            }
        }
    }
}

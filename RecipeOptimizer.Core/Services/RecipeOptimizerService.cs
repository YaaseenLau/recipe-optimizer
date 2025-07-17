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
            
            // Try multiple optimization strategies and pick the best result
            var greedyResult = OptimizeGreedy(recipes, availableIngredients);
            var efficiencyResult = OptimizeByEfficiency(recipes, availableIngredients);
            var combinedResult = OptimizeCombined(recipes, availableIngredients);
            var backtrackingResult = OptimizeWithBacktracking(recipes, availableIngredients);
            
            // Choose the best result based on total people served
            var bestResult = new[] { greedyResult, efficiencyResult, combinedResult, backtrackingResult }
                .OrderByDescending(r => r.TotalPeopleServed)
                .First();
            
            return bestResult;
        }
        
        /// <summary>
        /// Basic greedy algorithm that prioritizes recipes with highest serving size
        /// </summary>
        private OptimizationResult OptimizeGreedy(IEnumerable<Recipe> recipes, Dictionary<string, int> availableIngredients)
        {
            // Create a deep copy of available ingredients to work with
            var workingIngredients = new Dictionary<string, int>(availableIngredients);
            var result = new OptimizationResult();
            
            // Sort recipes by serving size (descending)
            var sortedRecipes = recipes.OrderByDescending(r => r.ServingSize).ToList();
            
            foreach (var recipe in sortedRecipes)
            {
                while (CanMakeRecipe(recipe, workingIngredients))
                {
                    UseIngredientsForRecipe(recipe, workingIngredients);
                    AddRecipeToResult(result, recipe);
                    result.TotalPeopleServed += recipe.ServingSize;
                }
            }
            
            result.RemainingIngredients = workingIngredients;
            return result;
        }
        
        /// <summary>
        /// Optimizes by efficiency (people served per ingredient unit)
        /// </summary>
        private OptimizationResult OptimizeByEfficiency(IEnumerable<Recipe> recipes, Dictionary<string, int> availableIngredients)
        {
            // Create a deep copy of available ingredients to work with
            var workingIngredients = new Dictionary<string, int>(availableIngredients);
            var result = new OptimizationResult();
            
            // Calculate efficiency for each recipe
            var recipesWithEfficiency = recipes.Select(recipe => 
            {
                int totalIngredientsUsed = recipe.Ingredients.Sum(i => i.RequiredQuantity);
                double efficiency = totalIngredientsUsed > 0 ? 
                    (double)recipe.ServingSize / totalIngredientsUsed : 0;
                return new { Recipe = recipe, Efficiency = efficiency };
            })
            .OrderByDescending(r => r.Efficiency)
            .ToList();
            
            foreach (var item in recipesWithEfficiency)
            {
                var recipe = item.Recipe;
                while (CanMakeRecipe(recipe, workingIngredients))
                {
                    UseIngredientsForRecipe(recipe, workingIngredients);
                    AddRecipeToResult(result, recipe);
                    result.TotalPeopleServed += recipe.ServingSize;
                }
            }
            
            result.RemainingIngredients = workingIngredients;
            return result;
        }
        
        /// <summary>
        /// Combined approach that balances serving size and efficiency
        /// </summary>
        private OptimizationResult OptimizeCombined(IEnumerable<Recipe> recipes, Dictionary<string, int> availableIngredients)
        {
            // Create a deep copy of available ingredients to work with
            var workingIngredients = new Dictionary<string, int>(availableIngredients);
            var result = new OptimizationResult();
            
            // Keep track of how many times we've made each recipe
            var recipeCounts = recipes.ToDictionary(r => r.Id, _ => 0);
            
            bool madeProgress;
            do
            {
                madeProgress = false;
                
                // Score each recipe based on multiple factors
                var recipeScores = recipes.Select(recipe => 
                {
                    if (!CanMakeRecipe(recipe, workingIngredients))
                        return new { Recipe = recipe, Score = -1.0 };
                    
                    int totalIngredientsUsed = recipe.Ingredients.Sum(i => i.RequiredQuantity);
                    double efficiency = totalIngredientsUsed > 0 ? 
                        (double)recipe.ServingSize / totalIngredientsUsed : 0;
                    
                    // Calculate how well this recipe uses nearly-depleted ingredients
                    double ingredientUtilization = recipe.Ingredients.Sum(i => 
                    {
                        workingIngredients.TryGetValue(i.Ingredient.Name, out int available);
                        // Bonus for using up ingredients that have small quantities left
                        return available > 0 && available <= i.RequiredQuantity * 2 ? 5.0 : 0;
                    });
                    
                    // Penalize making the same recipe too many times
                    double diversityFactor = Math.Max(0, 10 - recipeCounts[recipe.Id] * 2);
                    
                    // Final score combines serving size, efficiency, ingredient utilization and diversity
                    double score = (recipe.ServingSize * 3) + (efficiency * 10) + 
                                  ingredientUtilization + diversityFactor;
                    
                    return new { Recipe = recipe, Score = score };
                })
                .Where(r => r.Score >= 0)
                .OrderByDescending(r => r.Score)
                .ToList();
                
                if (recipeScores.Any())
                {
                    var bestRecipe = recipeScores.First().Recipe;
                    UseIngredientsForRecipe(bestRecipe, workingIngredients);
                    AddRecipeToResult(result, bestRecipe);
                    result.TotalPeopleServed += bestRecipe.ServingSize;
                    recipeCounts[bestRecipe.Id]++;
                    madeProgress = true;
                }
                
            } while (madeProgress);
            
            result.RemainingIngredients = workingIngredients;
            return result;
        }
        
        /// <summary>
        /// Uses a backtracking approach to find optimal combinations
        /// </summary>
        private OptimizationResult OptimizeWithBacktracking(IEnumerable<Recipe> recipes, Dictionary<string, int> availableIngredients)
        {
            var recipesList = recipes.ToList();
            var bestResult = new OptimizationResult();
            var currentResult = new OptimizationResult();
            var workingIngredients = new Dictionary<string, int>(availableIngredients);
            
            // Try different recipe combinations with backtracking
            TryRecipeCombinations(recipesList, workingIngredients, currentResult, bestResult, 0);
            
            return bestResult;
        }
        
        private void TryRecipeCombinations(
            List<Recipe> recipes, 
            Dictionary<string, int> availableIngredients, 
            OptimizationResult currentResult, 
            OptimizationResult bestResult,
            int depth)
        {
            // Limit backtracking depth to avoid excessive computation
            if (depth > 10) return;
            
            // If current result is better than best so far, update best
            if (currentResult.TotalPeopleServed > bestResult.TotalPeopleServed)
            {
                // Create a deep copy of the current result
                bestResult.TotalPeopleServed = currentResult.TotalPeopleServed;
                bestResult.Recipes = currentResult.Recipes.Select(rc => 
                    new RecipeCount { Recipe = rc.Recipe, Count = rc.Count }).ToList();
                bestResult.RemainingIngredients = new Dictionary<string, int>(availableIngredients);
            }
            
            // Try each recipe
            foreach (var recipe in recipes)
            {
                if (CanMakeRecipe(recipe, availableIngredients))
                {
                    // Make a copy of ingredients before using them
                    var ingredientsCopy = new Dictionary<string, int>(availableIngredients);
                    
                    // Use ingredients for this recipe
                    UseIngredientsForRecipe(recipe, availableIngredients);
                    
                    // Add to current result
                    AddRecipeToResult(currentResult, recipe);
                    currentResult.TotalPeopleServed += recipe.ServingSize;
                    
                    // Recursively try more recipes
                    TryRecipeCombinations(recipes, availableIngredients, currentResult, bestResult, depth + 1);
                    
                    // Backtrack: restore ingredients and remove recipe from result
                    availableIngredients = ingredientsCopy;
                    RemoveRecipeFromResult(currentResult, recipe);
                    currentResult.TotalPeopleServed -= recipe.ServingSize;
                }
            }
        }
        
        private void AddRecipeToResult(OptimizationResult result, Recipe recipe)
        {
            var existingRecipeCount = result.Recipes.FirstOrDefault(rc => rc.Recipe.Id == recipe.Id);
            if (existingRecipeCount != null)
            {
                existingRecipeCount.Count++;
            }
            else
            {
                result.Recipes.Add(new RecipeCount { Recipe = recipe, Count = 1 });
            }
        }
        
        private void RemoveRecipeFromResult(OptimizationResult result, Recipe recipe)
        {
            var existingRecipeCount = result.Recipes.FirstOrDefault(rc => rc.Recipe.Id == recipe.Id);
            if (existingRecipeCount != null)
            {
                if (existingRecipeCount.Count > 1)
                {
                    existingRecipeCount.Count--;
                }
                else
                {
                    result.Recipes.Remove(existingRecipeCount);
                }
            }
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

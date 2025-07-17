using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RecipeOptimizer.Core.Models;

namespace RecipeOptimizer.Core.Interfaces
{
    public interface IRecipeOptimizerService
    {
        Task<OptimizationResult> OptimizeRecipesAsync();
    }

    public class OptimizationResult
    {
        public List<RecipeCount> Recipes { get; set; } = new List<RecipeCount>();
        public int TotalPeopleServed { get; set; }
        public Dictionary<string, int> RemainingIngredients { get; set; } = new Dictionary<string, int>();
    }

    public class RecipeCount
    {
        public Recipe Recipe { get; set; }
        public int Count { get; set; }
        public int PeopleServed => Recipe.ServingSize * Count;
    }
}

using System.Collections.Generic;

namespace RecipeOptimizer.API.DTOs
{
    public class OptimizationResultDto
    {
        public List<RecipeCountDto> Recipes { get; set; } = new List<RecipeCountDto>();
        public int TotalPeopleServed { get; set; }
        public Dictionary<string, int> RemainingIngredients { get; set; } = new Dictionary<string, int>();
    }
}

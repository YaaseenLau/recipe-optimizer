using System;
using System.Collections.Generic;

namespace RecipeOptimizer.Core.Models
{
    public class Recipe
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ServingSize { get; set; }
        public List<RecipeIngredient> Ingredients { get; set; } = new List<RecipeIngredient>();
    }
}

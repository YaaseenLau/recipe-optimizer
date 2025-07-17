using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RecipeOptimizer.Core.Models;

namespace RecipeOptimizer.Core.Interfaces
{
    public interface IRecipeRepository
    {
        Task<IEnumerable<Recipe>> GetAllAsync();
        Task<Recipe> GetByIdAsync(int id);
        Task<Recipe> GetByNameAsync(string name);
        Task<Recipe> AddAsync(Recipe recipe);
        Task UpdateAsync(Recipe recipe);
        Task DeleteAsync(int id);
    }
}

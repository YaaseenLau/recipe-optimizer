using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RecipeOptimizer.Core.Models;

namespace RecipeOptimizer.Core.Interfaces
{
    public interface IIngredientRepository
    {
        Task<IEnumerable<Ingredient>> GetAllAsync();
        Task<Ingredient> GetByIdAsync(int id);
        Task<Ingredient> AddAsync(Ingredient ingredient);
        Task UpdateAsync(Ingredient ingredient);
        Task DeleteAsync(int id);
    }
}

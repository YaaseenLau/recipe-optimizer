using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RecipeOptimizer.API.DTOs;
using RecipeOptimizer.Core.Interfaces;
using RecipeOptimizer.Core.Models;

namespace RecipeOptimizer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecipesController : ControllerBase
    {
        private readonly IRecipeRepository _recipeRepository;

        public RecipesController(IRecipeRepository recipeRepository)
        {
            _recipeRepository = recipeRepository;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<RecipeDto>>> GetRecipes()
        {
            var recipes = await _recipeRepository.GetAllAsync();
            var recipeDtos = recipes.Select(r => new RecipeDto
            {
                Id = r.Id,
                Name = r.Name,
                ServingSize = r.ServingSize,
                Ingredients = r.Ingredients.Select(i => new RecipeIngredientDto
                {
                    Id = i.Id,
                    RecipeId = i.RecipeId,
                    IngredientId = i.IngredientId,
                    RequiredQuantity = i.RequiredQuantity,
                    Ingredient = new IngredientDto
                    {
                        Id = i.Ingredient.Id,
                        Name = i.Ingredient.Name,
                        AvailableQuantity = i.Ingredient.AvailableQuantity
                    }
                }).ToList()
            }).ToList();
            
            return Ok(recipeDtos);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RecipeDto>> GetRecipe(int id)
        {
            var recipe = await _recipeRepository.GetByIdAsync(id);
            if (recipe == null)
            {
                return NotFound();
            }
            
            var recipeDto = new RecipeDto
            {
                Id = recipe.Id,
                Name = recipe.Name,
                ServingSize = recipe.ServingSize,
                Ingredients = recipe.Ingredients.Select(i => new RecipeIngredientDto
                {
                    Id = i.Id,
                    RecipeId = i.RecipeId,
                    IngredientId = i.IngredientId,
                    RequiredQuantity = i.RequiredQuantity,
                    Ingredient = new IngredientDto
                    {
                        Id = i.Ingredient.Id,
                        Name = i.Ingredient.Name,
                        AvailableQuantity = i.Ingredient.AvailableQuantity
                    }
                }).ToList()
            };
            
            return Ok(recipeDto);
        }

        [HttpGet("name/{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RecipeDto>> GetRecipeByName(string name)
        {
            var recipe = await _recipeRepository.GetByNameAsync(name);
            if (recipe == null)
            {
                return NotFound();
            }
            
            var recipeDto = new RecipeDto
            {
                Id = recipe.Id,
                Name = recipe.Name,
                ServingSize = recipe.ServingSize,
                Ingredients = recipe.Ingredients.Select(i => new RecipeIngredientDto
                {
                    Id = i.Id,
                    RecipeId = i.RecipeId,
                    IngredientId = i.IngredientId,
                    RequiredQuantity = i.RequiredQuantity,
                    Ingredient = new IngredientDto
                    {
                        Id = i.Ingredient.Id,
                        Name = i.Ingredient.Name,
                        AvailableQuantity = i.Ingredient.AvailableQuantity
                    }
                }).ToList()
            };
            
            return Ok(recipeDto);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RecipeDto>> CreateRecipe(RecipeDto recipeDto)
        {
            // Convert DTO to domain model
            var recipe = new Recipe
            {
                Name = recipeDto.Name,
                ServingSize = recipeDto.ServingSize,
                Ingredients = recipeDto.Ingredients.Select(i => new RecipeIngredient
                {
                    IngredientId = i.IngredientId,
                    RequiredQuantity = i.RequiredQuantity
                }).ToList()
            };
            
            await _recipeRepository.AddAsync(recipe);
            
            // Update the DTO with the generated ID
            recipeDto.Id = recipe.Id;
            
            return CreatedAtAction(nameof(GetRecipe), new { id = recipe.Id }, recipeDto);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateRecipe(int id, RecipeDto recipeDto)
        {
            if (id != recipeDto.Id)
            {
                return BadRequest();
            }

            var existingRecipe = await _recipeRepository.GetByIdAsync(id);
            if (existingRecipe == null)
            {
                return NotFound();
            }
            
            // Update the existing recipe with values from the DTO
            existingRecipe.Name = recipeDto.Name;
            existingRecipe.ServingSize = recipeDto.ServingSize;
            
            // Handle ingredients update (simplified approach - replace all ingredients)
            existingRecipe.Ingredients.Clear();
            foreach (var ingredientDto in recipeDto.Ingredients)
            {
                existingRecipe.Ingredients.Add(new RecipeIngredient
                {
                    RecipeId = id,
                    IngredientId = ingredientDto.IngredientId,
                    RequiredQuantity = ingredientDto.RequiredQuantity
                });
            }

            await _recipeRepository.UpdateAsync(existingRecipe);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRecipe(int id)
        {
            var recipe = await _recipeRepository.GetByIdAsync(id);
            if (recipe == null)
            {
                return NotFound();
            }

            await _recipeRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}

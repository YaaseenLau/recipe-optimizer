using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RecipeOptimizer.API.DTOs;
using RecipeOptimizer.Core.Interfaces;

namespace RecipeOptimizer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OptimizerController : ControllerBase
    {
        private readonly IRecipeOptimizerService _optimizerService;

        public OptimizerController(IRecipeOptimizerService optimizerService)
        {
            _optimizerService = optimizerService;
        }

        [HttpGet("optimize")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<OptimizationResultDto>> OptimizeRecipes()
        {
            var domainResult = await _optimizerService.OptimizeRecipesAsync();
            
            // Map domain model to DTO to break circular references
            var result = new OptimizationResultDto
            {
                TotalPeopleServed = domainResult.TotalPeopleServed,
                RemainingIngredients = domainResult.RemainingIngredients
            };

            // Map recipe counts
            foreach (var recipeCount in domainResult.Recipes)
            {
                var recipeDto = new RecipeDto
                {
                    Id = recipeCount.Recipe.Id,
                    Name = recipeCount.Recipe.Name,
                    ServingSize = recipeCount.Recipe.ServingSize,
                    Ingredients = new List<RecipeIngredientDto>()
                };

                // Map recipe ingredients without circular references
                foreach (var ingredient in recipeCount.Recipe.Ingredients)
                {
                    recipeDto.Ingredients.Add(new RecipeIngredientDto
                    {
                        Id = ingredient.Id,
                        RecipeId = ingredient.RecipeId,
                        IngredientId = ingredient.IngredientId,
                        RequiredQuantity = ingredient.RequiredQuantity,
                        Ingredient = new IngredientDto
                        {
                            Id = ingredient.Ingredient.Id,
                            Name = ingredient.Ingredient.Name,
                            AvailableQuantity = ingredient.Ingredient.AvailableQuantity
                        }
                    });
                }

                result.Recipes.Add(new RecipeCountDto
                {
                    Recipe = recipeDto,
                    Count = recipeCount.Count,
                    PeopleServed = recipeCount.Recipe.ServingSize * recipeCount.Count
                });
            }

            return Ok(result);
        }
    }
}

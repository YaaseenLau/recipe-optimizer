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
    public class IngredientsController : ControllerBase
    {
        private readonly IIngredientRepository _ingredientRepository;

        public IngredientsController(IIngredientRepository ingredientRepository)
        {
            _ingredientRepository = ingredientRepository;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<IngredientDto>>> GetIngredients()
        {
            var ingredients = await _ingredientRepository.GetAllAsync();
            var ingredientDtos = ingredients.Select(i => new IngredientDto
            {
                Id = i.Id,
                Name = i.Name,
                AvailableQuantity = i.AvailableQuantity
            }).ToList();
            
            return Ok(ingredientDtos);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IngredientDto>> GetIngredient(int id)
        {
            var ingredient = await _ingredientRepository.GetByIdAsync(id);
            if (ingredient == null)
            {
                return NotFound();
            }
            
            var ingredientDto = new IngredientDto
            {
                Id = ingredient.Id,
                Name = ingredient.Name,
                AvailableQuantity = ingredient.AvailableQuantity
            };
            
            return Ok(ingredientDto);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IngredientDto>> CreateIngredient(IngredientDto ingredientDto)
        {
            if (ingredientDto == null)
            {
                return BadRequest();
            }

            var ingredient = new Ingredient
            {
                Name = ingredientDto.Name,
                AvailableQuantity = ingredientDto.AvailableQuantity
            };
            
            var createdIngredient = await _ingredientRepository.AddAsync(ingredient);
            
            // Update the DTO with the generated ID
            ingredientDto.Id = createdIngredient.Id;
            
            return CreatedAtAction(nameof(GetIngredient), new { id = createdIngredient.Id }, ingredientDto);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateIngredient(int id, IngredientDto ingredientDto)
        {
            if (id != ingredientDto.Id)
            {
                return BadRequest();
            }

            var existingIngredient = await _ingredientRepository.GetByIdAsync(id);
            if (existingIngredient == null)
            {
                return NotFound();
            }
            
            // Update the existing ingredient with values from the DTO
            existingIngredient.Name = ingredientDto.Name;
            existingIngredient.AvailableQuantity = ingredientDto.AvailableQuantity;

            await _ingredientRepository.UpdateAsync(existingIngredient);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteIngredient(int id)
        {
            var ingredient = await _ingredientRepository.GetByIdAsync(id);
            if (ingredient == null)
            {
                return NotFound();
            }

            await _ingredientRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}

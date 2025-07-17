using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RecipeOptimizer.API.Controllers;
using RecipeOptimizer.API.DTOs;
using RecipeOptimizer.Core.Interfaces;
using RecipeOptimizer.Core.Models;
using Xunit;

namespace RecipeOptimizer.Tests
{
    public class OptimizerControllerTests
    {
        private readonly Mock<IRecipeOptimizerService> _mockOptimizerService;
        private readonly OptimizerController _controller;

        public OptimizerControllerTests()
        {
            _mockOptimizerService = new Mock<IRecipeOptimizerService>();
            _controller = new OptimizerController(_mockOptimizerService.Object);
        }

        [Fact]
        public async Task OptimizeRecipes_ReturnsOkResult_WithOptimizationResult()
        {
            // Arrange
            var expectedResult = new OptimizationResult
            {
                TotalPeopleServed = 13,
                Recipes = new List<RecipeCount>
                {
                    new RecipeCount
                    {
                        Recipe = new Recipe { Id = 6, Name = "Pizza", ServingSize = 4 },
                        Count = 1
                    },
                    new RecipeCount
                    {
                        Recipe = new Recipe { Id = 5, Name = "Salad", ServingSize = 3 },
                        Count = 2
                    },
                    new RecipeCount
                    {
                        Recipe = new Recipe { Id = 3, Name = "Sandwich", ServingSize = 1 },
                        Count = 1
                    }
                },
                RemainingIngredients = new Dictionary<string, int>
                {
                    { "Meat", 6 },
                    { "Lettuce", 0 },
                    { "Tomato", 0 },
                    { "Cheese", 0 },
                    { "Dough", 3 },
                    { "Cucumber", 0 },
                    { "Olives", 0 }
                }
            };

            _mockOptimizerService.Setup(service => service.OptimizeRecipesAsync())
                .ReturnsAsync(expectedResult);

            // Act
            var actionResult = await _controller.OptimizeRecipes();

            // Assert
            var okResult = Assert.IsType<ActionResult<OptimizationResultDto>>(actionResult);
            var result = Assert.IsType<OkObjectResult>(okResult.Result);
            var returnValue = Assert.IsType<OptimizationResultDto>(result.Value);
            Assert.Equal(expectedResult.TotalPeopleServed, returnValue.TotalPeopleServed);
            Assert.Equal(expectedResult.Recipes.Count, returnValue.Recipes.Count);
        }
    }
}

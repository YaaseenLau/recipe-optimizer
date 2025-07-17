namespace RecipeOptimizer.API.DTOs
{
    public class RecipeIngredientDto
    {
        public int Id { get; set; }
        public int RecipeId { get; set; }
        public int IngredientId { get; set; }
        public int RequiredQuantity { get; set; }
        public IngredientDto Ingredient { get; set; }
    }
}

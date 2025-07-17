namespace RecipeOptimizer.API.DTOs
{
    public class RecipeCountDto
    {
        public RecipeDto Recipe { get; set; }
        public int Count { get; set; }
        public int PeopleServed { get; set; }
    }
}

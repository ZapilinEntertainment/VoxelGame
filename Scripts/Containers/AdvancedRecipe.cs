public class AdvancedRecipe : Recipe
{
    public readonly ResourceType input2;
    public readonly float inputValue2;
    new public static readonly AdvancedRecipe NoRecipe;
    public static readonly AdvancedRecipe Food_and_MetalP_to_Supplies;
    public static readonly Recipe[] supplyFactoryRecipes;

    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        if (this == NoRecipe && obj == Recipe.NoRecipe) return true;
        else
        {
            if (GetType() != obj.GetType()) return false;
        }
        if (base.Equals(obj))
        {
            var ar = (obj as AdvancedRecipe);
            if (ar != null)
            {
                if (input2 != ar.input2 || inputValue2 != ar.inputValue2) return false;
                else return true;
            }
            else return false;
        }
        else return false;
    }
    public override int GetHashCode()
    {
        return base.GetHashCode() + input2.ID;
    }

    static AdvancedRecipe()
    {
        NoRecipe = new AdvancedRecipe(ResourceType.Nothing, ResourceType.Nothing, ResourceType.Nothing, 0, 0f, 0f, 0f, 0f);
        Food_and_MetalP_to_Supplies = new AdvancedRecipe(
            ResourceType.Food, ResourceType.metal_P, ResourceType.Supplies,
            FOOD_AND_METALP_TO_SUPPLIES_ID,
            10f, 1f, 5f,
            GetRecipeComplexity(2)
            );

        supplyFactoryRecipes = new Recipe[2] { NoRecipe, Food_and_MetalP_to_Supplies };
    }

    public AdvancedRecipe(ResourceType res_input, ResourceType res_input2, ResourceType res_output, int f_id, float val_input, float val_input2, float val_output, float workflowNeeded) :
        base (res_input, res_output, f_id, val_input, val_output, workflowNeeded)
    {
        input2 = res_input2;
        inputValue2 = val_input2;
    }
}

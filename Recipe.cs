using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recipe {
		public readonly ResourceType input;
		public readonly float inputValue;
		public readonly ResourceType output;
		public readonly float outputValue;
		public readonly FactorySpecialization factoryWorktype;
		public readonly float workflowToResult;

	public static readonly Recipe[] smelteryRecipes;

	public static readonly Recipe NoRecipe;
	public static readonly Recipe StoneToConcrete;
	public static readonly Recipe LumberToElasticMass;

	static Recipe() {
		List<Recipe> smelteryRecipesList = new List<Recipe>();
		NoRecipe = new Recipe(ResourceType.Nothing, ResourceType.Nothing, 0,0, FactorySpecialization.Unspecialized, 0);
		StoneToConcrete = new Recipe(ResourceType.Stone, ResourceType.Concrete, 1, 1, FactorySpecialization.Smeltery, 10); smelteryRecipesList.Add(StoneToConcrete);
		LumberToElasticMass = new Recipe(ResourceType.Lumber, ResourceType.ElasticMass, 5, 1, FactorySpecialization.Smeltery, 15);  smelteryRecipesList.Add(LumberToElasticMass);

		smelteryRecipes = smelteryRecipesList.ToArray();
	}

	public Recipe (ResourceType res_input, ResourceType res_output, float val_input, float val_output, FactorySpecialization neededFactoryType, float workflowNeeded) {
		input = res_input; output = res_output;
		inputValue = val_input; outputValue = val_output;
		factoryWorktype = neededFactoryType;
		workflowToResult = workflowNeeded;
	}
}

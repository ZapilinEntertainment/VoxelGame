using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recipe {
		public readonly ResourceType input;
		public readonly float inputValue;
		public readonly ResourceType output;
		public readonly float outputValue;
		public readonly float workflowToResult;

	public static readonly Recipe[] smelteryRecipes, oreRefiningRecipes, fuelFacilityRecipes, plasticFactoryRecipes;

	public static readonly Recipe NoRecipe;
	public static readonly Recipe StoneToConcrete;
	public static readonly Recipe LumberToPlastics, MineralLToPlastics;
	public static readonly Recipe MetalK_smelting, MetalE_smelting, MetalN_smelting, MetalM_smelting,MetalP_smelting, MetalS_smelting;
	public static readonly Recipe MetalK_refining, MetalE_refining,MetalN_refining,MetalM_refining,MetalP_refining,MetalS_refining;
	public static readonly Recipe Fuel_fromNmetal, Fuel_fromNmetalOre, Fuel_fromMineralF;

	static Recipe() {
		NoRecipe = new Recipe(ResourceType.Nothing, ResourceType.Nothing, 0,0,  0);

		List<Recipe> smelteryRecipesList = new List<Recipe>();
		StoneToConcrete = new Recipe(ResourceType.Stone, ResourceType.Concrete, 1, 1,  10); smelteryRecipesList.Add(StoneToConcrete);
		LumberToPlastics = new Recipe(ResourceType.Lumber, ResourceType.Plastics, 5, 1,  15);  smelteryRecipesList.Add(LumberToPlastics);
		MetalK_smelting = new Recipe(ResourceType.metal_K_ore, ResourceType.metal_K,1,1, 10); smelteryRecipesList.Add(MetalK_smelting);
		MetalE_smelting = new Recipe(ResourceType.metal_E_ore, ResourceType.metal_E,1,1, 10); smelteryRecipesList.Add(MetalE_smelting);
		MetalN_smelting = new Recipe(ResourceType.metal_N_ore, ResourceType.metal_N,1,1, 10); smelteryRecipesList.Add(MetalN_smelting);
		MetalM_smelting = new Recipe(ResourceType.metal_M_ore, ResourceType.metal_M,1,1, 10); smelteryRecipesList.Add(MetalM_smelting);
		MetalP_smelting = new Recipe(ResourceType.metal_P_ore, ResourceType.metal_P,1,1, 10); smelteryRecipesList.Add(MetalP_smelting);
		MetalS_smelting = new Recipe(ResourceType.metal_S_ore, ResourceType.metal_S,1,1, 10); smelteryRecipesList.Add(MetalS_smelting);
		smelteryRecipes = smelteryRecipesList.ToArray();

		oreRefiningRecipes = new Recipe[6];
		MetalK_refining= new Recipe(ResourceType.metal_K_ore, ResourceType.metal_K_ore,1,2, 10);
		MetalE_refining = new Recipe(ResourceType.metal_E_ore, ResourceType.metal_E_ore,1,2,10); 
		MetalN_refining = new Recipe(ResourceType.metal_N_ore, ResourceType.metal_N_ore,1,2,10); 
		MetalM_refining = new Recipe(ResourceType.metal_M_ore, ResourceType.metal_M_ore,1,2, 10); 
		MetalP_refining = new Recipe(ResourceType.metal_P_ore, ResourceType.metal_P_ore,1,2, 10); 
		MetalS_refining= new Recipe(ResourceType.metal_S_ore, ResourceType.metal_S_ore,1,2, 10); 
		oreRefiningRecipes[0] = MetalK_refining; oreRefiningRecipes[1] = MetalE_refining;
		oreRefiningRecipes[2] = MetalN_refining; oreRefiningRecipes[3] = MetalM_refining;
		oreRefiningRecipes[4] = MetalP_refining; oreRefiningRecipes[5] = MetalS_refining;

		fuelFacilityRecipes = new Recipe[3];
		Fuel_fromNmetal = new Recipe(ResourceType.metal_N, ResourceType.Fuel, 1, 100, 25);
		Fuel_fromNmetalOre = new Recipe(ResourceType.metal_N_ore, ResourceType.Fuel, 1, 90, 35);
		Fuel_fromMineralF = new Recipe(ResourceType.mineral_F, ResourceType.Fuel, 1, 10, 10);
		fuelFacilityRecipes[0] = Fuel_fromNmetal;
		fuelFacilityRecipes[1] = Fuel_fromNmetalOre;
		fuelFacilityRecipes[2] = Fuel_fromMineralF;

		plasticFactoryRecipes = new Recipe[2];
		plasticFactoryRecipes[0] = LumberToPlastics;
		MineralLToPlastics = new Recipe(ResourceType.mineral_L, ResourceType.Plastics, 1, 2, 8);
		plasticFactoryRecipes[1] = MineralLToPlastics;
	}

	public Recipe (ResourceType res_input, ResourceType res_output, float val_input, float val_output,  float workflowNeeded) {
		input = res_input; output = res_output;
		inputValue = val_input; outputValue = val_output;
		workflowToResult = workflowNeeded;
	}
}

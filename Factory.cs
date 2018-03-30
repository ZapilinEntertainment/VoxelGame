using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FactoryType {Simple, Advanced, Recycler}
public enum FactorySpecialization {Unspecialized, Smeltery}


public class Factory : WorkBuilding {
	public Recipe recipe {get; protected set;}
	public FactoryType factoryType {get;protected set;}
	public FactorySpecialization specialization;
	protected Storage storage;
	protected const float BUFFER_LIMIT = 10;
	protected float inputResourcesBuffer = 0, outputResourcesBuffer = 0;

	void Awake () {
		PrepareWorkbuilding();
		factoryType = FactoryType.Simple;
		storage = GameMaster.colonyController.storage;
		recipe = Recipe.NoRecipe;
	}

	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetBuildingData(b, pos);
		UI.current.AddFactoryToList(this);
	}

	void Update() {
		if (GameMaster.gameSpeed == 0 || !isActive || !energySupplied) return;
		if (outputResourcesBuffer <= BUFFER_LIMIT) {
			if (workersCount > 0 && recipe != Recipe.NoRecipe) {
				workflow += workSpeed * Time.deltaTime * GameMaster.gameSpeed;
				if (workflow >= workflowToProcess ) {
					LabourResult();
					workflow -= workflowToProcess;
				}
			}
		}
		if (outputResourcesBuffer > 0) {
			outputResourcesBuffer =  storage.AddResources(recipe.output, outputResourcesBuffer); 
		}
	}

	override protected void LabourResult() {
		if (inputResourcesBuffer < recipe.inputValue) {
			float input = storage.GetResources(recipe.input, recipe.inputValue - inputResourcesBuffer);
			inputResourcesBuffer += input;
		}
		if (inputResourcesBuffer >= recipe.inputValue) {
			inputResourcesBuffer -= recipe.inputValue;
			outputResourcesBuffer += recipe.outputValue;
		}
	}

	public void SetRecipe( Recipe r ) {
		if (r == recipe) return;
		if (recipe != Recipe.NoRecipe) {
			if (inputResourcesBuffer > 0) storage.AddResources(recipe.input, recipe.inputValue);
			if (outputResourcesBuffer > 0) storage.AddResources(recipe.output, recipe.outputValue);
			}
		inputResourcesBuffer = 0; outputResourcesBuffer = 0;
		recipe = r;
		workflowToProcess = r.workflowToResult;
	}

	public int GetAppliableRecipesCount() {
		if (recipe == Recipe.NoRecipe) return 0;
		else {
			switch (specialization) {
			case FactorySpecialization.Smeltery:
				return Recipe.smelteryRecipes.Length;
				break;
			case FactorySpecialization.Unspecialized:
				return 0;
				break;
				default:
				return 0;
				break;
			}
		}
	}
		
	void OnDestroy() {
		PrepareWorkbuildingForDestruction();
		UI.current.RemoveFromFactoriesList(this);
	}

}

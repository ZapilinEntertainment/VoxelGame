using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FactorySpecialization {Unspecialized, Smeltery, OreRefiner, FuelFacility, PlasticsFactory}


public class Factory : WorkBuilding {	
	Recipe recipe;
	protected Storage storage;
	protected const float BUFFER_LIMIT = 10;
	public float inputResourcesBuffer  {get; protected set;}
	protected bool gui_showRecipesList = false;
	public FactorySpecialization specialization;
	protected float outputResourcesBuffer = 0;

	void Awake () {
		PrepareWorkbuilding();
		recipe = Recipe.NoRecipe;
		inputResourcesBuffer = 0;
	}

	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetBuildingData(b, pos);
		storage = GameMaster.colonyController.storage;
		UI.current.AddFactoryToList(this);
	}

	void Update() {
		if (GameMaster.gameSpeed == 0 || !isActive || !energySupplied) return;
		if (outputResourcesBuffer <= BUFFER_LIMIT ) {
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
			outputResourcesBuffer += recipe.outputValue * level;
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
			case FactorySpecialization.FuelFacility:
				return Recipe.fuelFacilityRecipes.Length;
				break;
			case FactorySpecialization.OreRefiner:
				return Recipe.oreRefiningRecipes.Length;
				break;
			case FactorySpecialization.Smeltery:
				return Recipe.smelteryRecipes.Length;
				break;
			case FactorySpecialization.PlasticsFactory:
				return Recipe.plasticFactoryRecipes.Length;
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
		
	void OnGUI() {
		if ( !showOnGUI ) return;
		//upgrading
		Rect rr = new Rect(UI.current.rightPanelBox.x, gui_ypos, UI.current.rightPanelBox.width, GameMaster.guiPiece);
		if (canBeUpgraded && level < GameMaster.colonyController.hq.level) {
			rr.y = GUI_UpgradeButton(rr);
		}
		//factory actions
		if (gui_showRecipesList) {
			Recipe[] recipesToShow = null;
			switch (specialization) {
			case FactorySpecialization.FuelFacility: recipesToShow = Recipe.fuelFacilityRecipes;break;
			case FactorySpecialization.OreRefiner: recipesToShow = Recipe.oreRefiningRecipes; break;
			case FactorySpecialization.Smeltery: recipesToShow = Recipe.smelteryRecipes;break;
			case FactorySpecialization.PlasticsFactory: recipesToShow = Recipe.plasticFactoryRecipes;break;
			}
			if (recipesToShow != null && recipesToShow.Length> 0) {
				GUI.Box(new Rect(rr.x, rr.y, rr.width, rr.height * recipesToShow.Length), GUIContent.none);
				foreach ( Recipe r in recipesToShow ) {
					if (GUI.Button(rr, r.input.name + " -> " + r.output.name)) {
						SetRecipe(r);
						gui_showRecipesList = false;
					}
					rr.y += rr.height;
				}
		}
		}
		else {
			if (GUI.Button(rr, recipe.input.name + " -> " + recipe.output.name)) {
				gui_showRecipesList = true;
			}
			rr.y += rr.height;
		}
		GUI.DrawTexture( new Rect(rr.x, rr.y, rr.height * 2, rr.height * 2), recipe.input.icon, ScaleMode.StretchToFill );
		GUI.DrawTexture( new Rect(rr.x + rr.width / 3f, rr.y, rr.height* 2, rr.height* 2), UI.current.rightArrow_tx, ScaleMode.StretchToFill );
		GUI.DrawTexture( new Rect (rr.xMax - rr.height * 2, rr.y, rr.height* 2, rr.height* 2), recipe.output.icon, ScaleMode.StretchToFill );
		rr.y += rr.height * 2;
		GUI.Label( new Rect(rr.x, rr.y, rr.height, rr.height), ((int)inputResourcesBuffer).ToString() + '/' + recipe.inputValue.ToString(), PoolMaster.GUIStyle_CenterOrientedLabel );
		GUI.Label( new Rect(rr.xMax - rr.height, rr.y, rr.height, rr.height), recipe.outputValue.ToString(), PoolMaster.GUIStyle_CenterOrientedLabel);
		if (recipe != Recipe.NoRecipe) GUI.Label ( new Rect(rr.x + rr.width / 3f, rr.y, rr.height * 2f, rr.height), ((int)(workflow / recipe.workflowToResult * 100f)).ToString() + '%');
	}

	void OnDestroy() {
		PrepareWorkbuildingForDestruction();
		UI.current.RemoveFromFactoriesList(this);
	}
}

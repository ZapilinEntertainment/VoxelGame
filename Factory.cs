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

	override public void Prepare() {
		PrepareWorkbuilding();
		recipe = Recipe.NoRecipe;
		inputResourcesBuffer = 0;
	}

	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetBuildingData(b, pos);
		storage = GameMaster.colonyController.storage;
		SetRecipe(Recipe.NoRecipe);
		UI.current.AddFactoryToList(this);
	}

	void Update() {
		if (GameMaster.gameSpeed == 0 || !isActive || !energySupplied) return;
		if (outputResourcesBuffer > 0) {
			outputResourcesBuffer =  storage.AddResource(recipe.output, outputResourcesBuffer); 
		}
		if (outputResourcesBuffer <= BUFFER_LIMIT ) {
			if (workersCount > 0 && recipe != Recipe.NoRecipe) { // сильно намудрил!
				float progress = workflow / workflowToProcess;
				float resourcesSupport = inputResourcesBuffer / recipe.inputValue;
				if (resourcesSupport < 1 ) inputResourcesBuffer += storage.GetResources(recipe.input, recipe.inputValue - inputResourcesBuffer);
				resourcesSupport = inputResourcesBuffer / recipe.inputValue;
				if (resourcesSupport > progress) 	workflow += workSpeed * Time.deltaTime * GameMaster.gameSpeed;
				if (workflow >= workflowToProcess) LabourResult();
			}
		}
	}

	override protected void LabourResult() {
		int iterations = (int)(workflow / workflowToProcess);
		if (inputResourcesBuffer < recipe.inputValue * iterations) inputResourcesBuffer += storage.GetResources(recipe.input, recipe.inputValue * iterations - inputResourcesBuffer);
		while ( iterations >=1 & inputResourcesBuffer >= recipe.inputValue) {
			inputResourcesBuffer -= recipe.inputValue;
			outputResourcesBuffer += recipe.outputValue;
			workflow -= workflowToProcess;
			iterations --;
		}
	}

	public void SetRecipe( Recipe r ) {
		if (r == recipe) return;
		if (recipe != Recipe.NoRecipe) {
			if (inputResourcesBuffer > 0) storage.AddResource(recipe.input, recipe.inputValue);
			if (outputResourcesBuffer > 0) storage.AddResource(recipe.output, recipe.outputValue);
		}
		workflow = 0;
		inputResourcesBuffer = 0; outputResourcesBuffer = 0;
		recipe = r;
		workflowToProcess = r.workflowToResult;
	}

	public int GetAppliableRecipesCount() {
		if (recipe == Recipe.NoRecipe) return 0;
		else {
			switch (specialization) {
			case FactorySpecialization.FuelFacility:		return Recipe.fuelFacilityRecipes.Length;
			case FactorySpecialization.OreRefiner:		return Recipe.oreRefiningRecipes.Length;
			case FactorySpecialization.Smeltery:			return Recipe.smelteryRecipes.Length;
			case FactorySpecialization.PlasticsFactory:		return Recipe.plasticFactoryRecipes.Length;
			case FactorySpecialization.Unspecialized:			return 0;
			default:		return 0;
			}
		}
	}
		
	//---------------------                   SAVING       SYSTEM-------------------------------
	public override string Save() {
		return SaveStructureData() + SaveBuildingData() + SaveWorkBuildingData() + SaveFactoryData();
	}

	protected string SaveFactoryData() {
		string s = "";
		s += string.Format("{0:d3}", recipe.ID);
		s += string.Format("{0:d5}", (int)(inputResourcesBuffer * 1000) );
		s += string.Format("{0:d5}",(int)(outputResourcesBuffer * 1000) );
		return s;
	}

	public override void Load(string s_data, Chunk c, SurfaceBlock surface) {
		byte x = byte.Parse(s_data.Substring(0,2));
		byte z = byte.Parse(s_data.Substring(2,2));
		Prepare();
		SetBasement(surface, new PixelPosByte(x,z));
		//workbuilding class part
		workflow = int.Parse(s_data.Substring(12,3)) / 100f;
		AddWorkers(int.Parse(s_data.Substring(15,3)));
		//factory class part
		SetRecipe(Recipe.GetRecipeByNumber(int.Parse(s_data.Substring(18,3))));
		inputResourcesBuffer = int.Parse(s_data.Substring(21,5)) / 1000f;
		outputResourcesBuffer = int.Parse(s_data.Substring(26,5)) / 1000f;
		//building class part
		SetActivationStatus(s_data[11] == '1');     
		//--
		transform.localRotation = Quaternion.Euler(0, 45 * int.Parse(s_data[7].ToString()), 0);
		hp = int.Parse(s_data.Substring(8,3)) / 100f * maxHp;
	}
	//---------------------------------------------------------------------------------------------------	

	void OnGUI() {
		if ( !showOnGUI ) return;
		//upgrading
		Rect rr = new Rect(UI.current.rightPanelBox.x, gui_ypos, UI.current.rightPanelBox.width, GameMaster.guiPiece);
		if (upgradedIndex != -1 && level < GameMaster.colonyController.hq.level) {
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

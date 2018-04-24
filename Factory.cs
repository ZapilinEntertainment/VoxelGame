using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FactoryType {Simple, Advanced, Recycler}
public enum FactorySpecialization {Unspecialized, Smeltery, OreRefiner}


public class Factory : WorkBuilding {	
	public Recipe recipe {get; protected set;}
	public FactoryType factoryType {get;protected set;}
	protected Storage storage;
	protected const float BUFFER_LIMIT = 10;
	public float inputResourcesBuffer  {get; protected set;}
	protected bool gui_showRecipesList = false;
	public FactorySpecialization specialization;
	protected float outputResourcesBuffer = 0;

	void Awake () {
		PrepareWorkbuilding();
		factoryType = FactoryType.Simple;
		recipe = Recipe.NoRecipe;
		inputResourcesBuffer = 0;
	}

	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetBuildingData(b, pos);
		storage = GameMaster.colonyController.storage;
		factoryType = FactoryType.Simple;
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
			case FactorySpecialization.OreRefiner:
				return Recipe.oreRefiningRecipes.Length;
				break;
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
		
	void OnGUI() {
		if ( !showOnGUI ) return;
		//upgrading
		Rect rr = new Rect(UI.current.rightPanelBox.x, gui_ypos, UI.current.rightPanelBox.width, GameMaster.guiPiece);
		if (nextStage != null && level < GameMaster.colonyController.hq.level) {
			GUI.DrawTexture(new Rect( rr.x, rr.y, rr.height, rr.height), PoolMaster.greenArrow_tx, ScaleMode.StretchToFill);
			if ( GUI.Button(new Rect (rr.x + rr.height, rr.y, rr.height * 4, rr.height), "Level up") ) {
				ResourceContainer[] requiredResources = new ResourceContainer[ResourcesCost.info[nextStage.resourcesContainIndex].Length];
				if (requiredResources.Length > 0) {
					for (int i = 0; i < requiredResources.Length; i++) {
						requiredResources[i] = new ResourceContainer(ResourcesCost.info[nextStage.resourcesContainIndex][i].type, ResourcesCost.info[nextStage.resourcesContainIndex][i].volume * (1 - GameMaster.upgradeDiscount));
					}
				}
				if ( GameMaster.colonyController.storage.CheckBuildPossibilityAndCollectIfPossible( requiredResources ) )
				{
					Building upgraded = Instantiate(nextStage);
					upgraded.SetBasement(basement, PixelPosByte.zero);
				}
				else UI.current.ChangeSystemInfoString(Localization.announcement_notEnoughResources);
			}
			if ( ResourcesCost.info[ nextStage.resourcesContainIndex ].Length > 0) {
				rr.y += rr.height;
				for (int i = 0; i < ResourcesCost.info[ nextStage.resourcesContainIndex ].Length; i++) {
					GUI.DrawTexture(new Rect(rr.x, rr.y, rr.height, rr.height), ResourcesCost.info[ nextStage.resourcesContainIndex ][i].type.icon, ScaleMode.StretchToFill);
					GUI.Label(new Rect(rr.x +rr.height, rr.y, rr.height * 5, rr.height), ResourcesCost.info[ nextStage.resourcesContainIndex ][i].type.name);
					GUI.Label(new Rect(rr.xMax - rr.height * 3, rr.y, rr.height * 3, rr.height), (ResourcesCost.info[ nextStage.resourcesContainIndex ][i].volume * (1 - GameMaster.upgradeDiscount)).ToString(), PoolMaster.GUIStyle_RightOrientedLabel);
					rr.y += rr.height;
				}
			}
		}
		//factory actions
		if (gui_showRecipesList) {
			Recipe[] recipesToShow = null;
			switch (specialization) {
			case FactorySpecialization.OreRefiner: recipesToShow = Recipe.oreRefiningRecipes; break;
			case FactorySpecialization.Smeltery: recipesToShow = Recipe.smelteryRecipes;break;
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

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
	public float inputResourcesBuffer  {get; protected set;}
	protected float outputResourcesBuffer = 0;
	bool gui_showRecipesList = false;

	void Awake () {
		PrepareWorkbuilding();
		factoryType = FactoryType.Simple;
		storage = GameMaster.colonyController.storage;
		recipe = Recipe.NoRecipe;
		inputResourcesBuffer = 0;
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
		
	void OnGUI() {
		if ( !showOnGUI ) return;
		Rect rr = UI.current.rightPanelBox;
		rr.y = gui_ypos; rr.height = GameMaster.guiPiece;
		if (gui_showRecipesList) {
			switch (specialization) {
			case FactorySpecialization.Smeltery:
				GUI.Box(new Rect(rr.x, rr.y, rr.width, rr.height * Recipe.smelteryRecipes.Length), GUIContent.none);
				foreach ( Recipe r in Recipe.smelteryRecipes ) {
					if (GUI.Button(rr, r.input.name + " -> " + r.output.name)) {
						SetRecipe(r);
					}
					rr.y += rr.height;
				}
				break;
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
		GUI.Label( new Rect(rr.x, rr.y, rr.height, rr.height), ((int)inputResourcesBuffer).ToString() + '/' + recipe.inputValue.ToString(), GameMaster.mainGUISkin.customStyles[(int)GUIStyles.CenterOrientedLabel] );
		GUI.Label( new Rect(rr.xMax - rr.height, rr.y, rr.height, rr.height), recipe.outputValue.ToString(), GameMaster.mainGUISkin.customStyles[(int)GUIStyles.CenterOrientedLabel] );
		if (recipe != Recipe.NoRecipe) GUI.Label ( new Rect(rr.x + rr.width / 3f, rr.y, rr.height * 2f, rr.height), ((int)(workflow / recipe.workflowToResult * 100f)).ToString() + '%');
	}

	void OnDestroy() {
		PrepareWorkbuildingForDestruction();
		UI.current.RemoveFromFactoriesList(this);
	}

}

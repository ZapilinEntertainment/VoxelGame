using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Dock : WorkBuilding {
	bool correctLocation = false;
	public Transform meshTransform;
	bool gui_tradeGoodTabActive = false, gui_sellResourcesTabActive = false, gui_addTransactionMenu = false;
	public static bool?[] isForSale{get; private set;}
	public static int[] minValueForTrading{get;private set;}
	int preparingResourceIndex = 0;
	ColonyController colony;
	public static int immigrantsMonthLimit {get; private set;} 
	public static bool immigrationEnabled{get; private set;}
	static int peopleArrivedThisMonth = 0;
	public bool maintainingShip = false;
	const float LOADING_TIME = 10;


	static Dock() {
		immigrantsMonthLimit = 0;
		isForSale = new bool?[ResourceType.resourceTypesArray.Length];
		minValueForTrading= new int[ResourceType.resourceTypesArray.Length];
	}

	void Awake() {
		PrepareWorkbuilding();
		type = StructureType.MainStructure;
		borderOnlyConstruction = true;
		immigrantsMonthLimit = 0;
	}

	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetBuildingData(b, pos);
		if (basement.pos.z == 0) {
			meshTransform.transform.localRotation = Quaternion.Euler(0, 180,0); correctLocation = true;
		}
		else {
			if (basement.pos.z == Chunk.CHUNK_SIZE - 1) {
				correctLocation = true;
			}
			else {
				if (basement.pos.x == 0) {
					meshTransform.transform.localRotation = Quaternion.Euler(0, -90,0); correctLocation = true;
				}
				else {
					if (basement.pos.x == Chunk.CHUNK_SIZE - 1) {
						meshTransform.transform.localRotation = Quaternion.Euler(0, 90,0); correctLocation = true;
					}
				}
			}
		}
		if (correctLocation) 
		{	
			basement.ReplaceMaterial(ResourceType.CONCRETE_ID);
			colony = GameMaster.colonyController;
			colony.AddDock(this);
		}
	}

	void EveryMonthUpdate() {
		peopleArrivedThisMonth = 0;
		// maintance
	}

	public IEnumerator ShipLoading(Ship s) {
		yield return new WaitForSeconds(LOADING_TIME * GameMaster.gameSpeed);
		int availableImmigrants = 0;
		if (immigrantsMonthLimit > 0) availableImmigrants = immigrantsMonthLimit - peopleArrivedThisMonth;
		if (availableImmigrants < 0) availableImmigrants = 0;
		switch (s.type) {
		case ShipType.Passenger:
			if (availableImmigrants > 0) {
				if (s.volume > availableImmigrants) {GameMaster.colonyController.AddCitizens(availableImmigrants); peopleArrivedThisMonth += availableImmigrants;}
				else {GameMaster.colonyController.AddCitizens(s.volume); peopleArrivedThisMonth += s.volume;}
			}
			if (isForSale [ResourceType.FOOD_ID] != null) {
				float vol = s.volume * 0.1f;
				if ( isForSale[ResourceType.FOOD_ID] == true) {
					if (colony.storage.standartResources[ ResourceType.FOOD_ID ] > minValueForTrading[ ResourceType.FOOD_ID ])	SellResource(ResourceType.Food, vol);
				}
				else {
					if (colony.storage.standartResources[ ResourceType.FOOD_ID ] <= minValueForTrading[ ResourceType.FOOD_ID ]) BuyResource(ResourceType.Food, s.volume * 0.1f);
				}
			}
			break;
		case ShipType.Cargo:
			
			break;
		case ShipType.Military:
			break;
		case ShipType.Private:
			break;
		}
		s.Undock();
	}

	void SellResource(ResourceType rt, float volume) {
		float vol = colony.storage.GetResources(rt, volume);
		colony.AddEnergyCrystals(vol * ResourceType.prices[rt.ID] * GameMaster.sellPriceCoefficient);
	}
	void BuyResource(ResourceType rt, float volume) {
		volume = colony.GetEnergyCrystals(volume * ResourceType.prices[rt.ID]) / ResourceType.prices[rt.ID];
		colony.storage.AddResources(rt, volume);
	}

	void OnGUI() {
		if (!showOnGUI) return;
		GUI.skin = GameMaster.mainGUISkin;
		float k =GameMaster.guiPiece;
		Rect r = new Rect(UI.current.rightPanelBox.x, gui_ypos, UI.current.rightPanelBox.width, GameMaster.guiPiece);
		if ( !gui_tradeGoodTabActive ) GUI.DrawTexture(new Rect(r.x, r.y, r.width / 2f, r.height), PoolMaster.orangeSquare_tx, ScaleMode.StretchToFill);
		else GUI.DrawTexture(new Rect(r.x + r.width/2f, r.y, r.width/2f, r.height), PoolMaster.orangeSquare_tx, ScaleMode.StretchToFill);
		if (GUI.Button(new Rect(r.x, r.y, r.width / 2f, r.height), Localization.ui_immigration, PoolMaster.GUIStyle_BorderlessButton)) {gui_tradeGoodTabActive = false;gui_addTransactionMenu = false;}
		if (GUI.Button(new Rect(r.x + r.width/2f, r.y, r.width/2f, r.height), Localization.ui_trading, PoolMaster.GUIStyle_BorderlessButton)) gui_tradeGoodTabActive = true;
		GUI.DrawTexture(r, PoolMaster.twoButtonsDivider_tx, ScaleMode.StretchToFill);
		r.y += r.height;
		if (gui_tradeGoodTabActive) {
			if (!gui_sellResourcesTabActive) GUI.DrawTexture(new Rect(r.x, r.y, r.width / 2f, r.height), PoolMaster.orangeSquare_tx, ScaleMode.StretchToFill);
			else GUI.DrawTexture(new Rect(r.x + r.width/2f, r.y, r.width / 2f, r.height), PoolMaster.orangeSquare_tx, ScaleMode.StretchToFill);
			if (GUI.Button(new Rect(r.x, r.y, r.width / 2f, r.height), Localization.ui_buy, PoolMaster.GUIStyle_BorderlessButton)) gui_sellResourcesTabActive = false;
			if (GUI.Button(new Rect(r.x + r.width/2f, r.y, r.width/2f, r.height), Localization.ui_sell, PoolMaster.GUIStyle_BorderlessButton)) gui_sellResourcesTabActive = true;
			GUI.DrawTexture(r, PoolMaster.twoButtonsDivider_tx, ScaleMode.StretchToFill);
			r.y += r.height;
			if (GUI.Button(r, '<' + Localization.ui_add_transaction + '>')) {
				gui_addTransactionMenu = !gui_addTransactionMenu;
				UI.current.touchscreenTemporarilyBlocked = gui_addTransactionMenu;
			}
			r.y += r.height;
			if (gui_addTransactionMenu) {  // Настройка торговой операции
				GUI.Box(new Rect(2 *k, 2*k, Screen.width - 2 *k - UI.current.rightPanelBox.width,Screen.height - 4 *k), GUIContent.none);
				float resQuad_k = 10 * k / ResourceType.RTYPE_ARRAY_ROWS;
				GUI.Label(new Rect(Screen.width/2f - 4 * resQuad_k, k, 5 * resQuad_k, k ), Localization.ui_selectResource);
				float resQuad_leftBorder = 2 * k ;
				Rect resrect = new Rect(  resQuad_leftBorder, 2 *k , resQuad_k, resQuad_k);
				int index = 1; resrect.x += resrect.width;
				for (int i = 0; i < ResourceType.RTYPE_ARRAY_ROWS; i++) {
					for (int j =0; j < ResourceType.RTYPE_ARRAY_COLUMNS; j++) {
						if (ResourceType.resourceTypesArray[index] == null) {
							index++;
							resrect.x += resrect.width;
							continue;
						}
						if (index == preparingResourceIndex) GUI.DrawTexture(resrect, PoolMaster.orangeSquare_tx, ScaleMode.StretchToFill);
						if (GUI.Button(resrect, ResourceType.resourceTypesArray[index].icon)) {
							preparingResourceIndex = index;
						}
						GUI.Label(new Rect(resrect.x, resrect.yMax -  resrect.height/2f, resrect.width, resrect.height/2f), colony.storage.standartResources[index].ToString());
						index ++;
						resrect.x += resrect.width;
						if (index >= ResourceType.resourceTypesArray.Length) break;
					}
					resrect.x = resQuad_leftBorder;
					resrect.y += resrect.height;
					if (index >= ResourceType.resourceTypesArray.Length) break;
				}

				float xpos = 2 *k, ypos = Screen.height/2f, qsize = 2 *k;
				GUI.Label(new Rect(xpos, ypos, 4 * qsize, qsize), Localization.min_value_to_trade); 
				int val = minValueForTrading[preparingResourceIndex];
				if (GUI.Button(new Rect(xpos + 4 * qsize, ypos , qsize, qsize ), "-10")) val -= 10;
				if (GUI.Button(new Rect(xpos + 5 * qsize, ypos, qsize, qsize), "-1")) val--;
				if (val < 0) val = 0; minValueForTrading[preparingResourceIndex] = val;
				GUI.Label (new Rect(xpos + 6 * qsize, ypos, qsize * 2, qsize), minValueForTrading[preparingResourceIndex].ToString(), PoolMaster.GUIStyle_CenterOrientedLabel);
				if (GUI.Button(new Rect(xpos + 8 * qsize, ypos, qsize, qsize), "+1")) minValueForTrading[preparingResourceIndex]++;
				if (GUI.Button(new Rect(xpos + 9 * qsize, ypos, qsize, qsize), "+10")) minValueForTrading[preparingResourceIndex] += 10; 
				if (isForSale [preparingResourceIndex] == null && minValueForTrading[preparingResourceIndex] != 0) {
					isForSale [preparingResourceIndex] = gui_sellResourcesTabActive;
				}
				else isForSale [preparingResourceIndex] = null;
				ypos += qsize;
				GUI.Label(new Rect(xpos, ypos, 2 * qsize, qsize), '+' + (ResourceType.prices[preparingResourceIndex] * GameMaster.sellPriceCoefficient).ToString(), PoolMaster.GUIStyle_CenterOrientedLabel);
				if (isForSale [preparingResourceIndex] == true ) GUI.DrawTexture(new Rect(xpos + 2 *qsize, ypos, 3 * qsize, qsize) , PoolMaster.orangeSquare_tx, ScaleMode.StretchToFill);
				if (GUI.Button (new Rect(xpos + 2 *qsize, ypos, 3 * qsize, qsize) , Localization.ui_sell)) isForSale [preparingResourceIndex] = true;
				if ( isForSale [preparingResourceIndex] == false ) GUI.DrawTexture(new Rect(xpos + 6 *qsize, ypos, 3 * qsize, qsize) , PoolMaster.orangeSquare_tx, ScaleMode.StretchToFill);
				if (GUI.Button (new Rect(xpos + 6 *qsize, ypos, 3 * qsize, qsize) , Localization.ui_buy)) isForSale [preparingResourceIndex] = false;
				GUI.DrawTexture(new Rect(xpos + 2 * qsize, ypos, 7 * qsize, qsize) , PoolMaster.twoButtonsDivider_tx, ScaleMode.StretchToFill);
				GUI.Label(new Rect(xpos + 9 * qsize, ypos, 2 * qsize, qsize), '-' + ResourceType.prices[preparingResourceIndex] .ToString(), PoolMaster.GUIStyle_CenterOrientedLabel);
				ypos += 2 * qsize;
				if (GUI.Button(new Rect(xpos + 3 * qsize, ypos, 2 * qsize, qsize), Localization.ui_close)) {
					gui_addTransactionMenu = false;
					UI.current.touchscreenTemporarilyBlocked = false;
				}
				if (GUI.Button(new Rect(xpos + 6 * qsize, ypos, 2 * qsize, qsize), Localization.ui_reset)) {
					minValueForTrading[preparingResourceIndex] = 0;
					isForSale [preparingResourceIndex] = null;
					gui_addTransactionMenu = false;
					UI.current.touchscreenTemporarilyBlocked = false;
				}
			}
			// list
			for (int i = 1 ; i < minValueForTrading.Length; i ++) {
				if (ResourceType.resourceTypesArray[i] == null || minValueForTrading[i] == 0 || isForSale [i] != gui_sellResourcesTabActive) {i++;continue;}
				GUI.DrawTexture(new Rect(r.x, r.y, r.height, r.height), ResourceType.resourceTypesArray[i].icon);
				int val = minValueForTrading[i];
				if (GUI.Button(new Rect(r.x + r.height, r.y, r.height, r.height), "-10")) val -= 10;
				if (GUI.Button(new Rect(r.x + 2 *r.height, r.y, r.height, r.height), "-1")) val--;
				if (val < 0) val = 0; minValueForTrading[i] = val;
				GUI.Label(new Rect(r.x + 3 * r.height, r.y, r.height * 2, r.height ), minValueForTrading[i].ToString(), PoolMaster.GUIStyle_CenterOrientedLabel);
				if (GUI.Button(new Rect(r.x + 5 * r.height, r.y, r.height, r.height), "+1")) minValueForTrading[i]++;
				if (GUI.Button(new Rect(r.x + 6 * r.height, r.y, r.height, r.height), "+10")) minValueForTrading[i] += 10;
				if (GUI.Button(new Rect(r.x + 7 * r.height, r.y, r.height, r.height), PoolMaster.minusButton_tx)) {isForSale [i] = null; minValueForTrading[i] = 0;}
					r.y += r.height;
				}
		}
		else // immigration tab
		{
			immigrationEnabled = GUI.Toggle(r, immigrationEnabled, Localization.ui_immigrationEnabled);
			r.y += r.height;
			if ( !immigrationEnabled ) {
				immigrantsMonthLimit = -1;
				GUI.Label(new Rect(r.x + 3 * r.height, r.y, r.height * 3, r.height ), Localization.ui_immigrationDisabled , PoolMaster.GUIStyle_CenterOrientedLabel);
			}
			else {
				if (GUI.Button(new Rect(r.x + r.height, r.y, r.height, r.height), "-10")) immigrantsMonthLimit-=10;
				if (GUI.Button(new Rect(r.x + 2 *r.height, r.y, r.height, r.height), "-1")) immigrantsMonthLimit--;
				if (immigrantsMonthLimit < 0) {immigrantsMonthLimit = -1; immigrationEnabled = false;}
				GUI.Label(new Rect(r.x + 3 * r.height, r.y, r.height * 3, r.height ), immigrantsMonthLimit.ToString(), PoolMaster.GUIStyle_CenterOrientedLabel);
				if (GUI.Button(new Rect(r.x + 6 * r.height, r.y, r.height, r.height), "+1")) immigrantsMonthLimit++;
				if (GUI.Button(new Rect(r.x + 7 * r.height, r.y, r.height, r.height), "+10")) immigrantsMonthLimit += 10;
			}
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum SurfacePanelMode {SelectAction, Build}

public sealed class UISurfacePanelController : UIObserver {
	public Button buildButton, gatherButton, digButton, blockCreateButton, columnCreateButton, changeMaterialButton;
	SurfaceBlock surface;
	bool status_gatherEnabled = true, status_gatherOrdered = false, status_digOrdered = false;
	byte savedHqLevel = 0;
	int constructingBuildingID = -1, selectedBuildingButton = -1;
	public byte constructingLevel = 1;
	SurfacePanelMode mode;
	public Toggle[] buildingsLevelToggles; // fiti
	public Button[] availableBuildingsButtons; // fiti
	public Text name, description; // fiti
	public RawImage[] resourcesCostImage; // fiti
	public Button innerBuildButton, returnButton; // fiti
	public Sprite overridingSprite; // fiti
	HeadQuarters hq;

	void Start() {
		returnButton.onClick.AddListener(() => {
			this.ChangeMode(SurfacePanelMode.SelectAction);
		});
	}

	public void SetObservingSurface(SurfaceBlock sb) {
		if (sb == null) {
			SelfShutOff();
			return;
		}
		else {
			hq = GameMaster.colonyController.hq;
			surface = sb;
			isObserving = true;
			ChangeMode (SurfacePanelMode.SelectAction);
				
			STATUS_UPDATE_TIME = 1;
			timer = STATUS_UPDATE_TIME;
		}
	}

	protected override void StatusUpdate() {
		if ( surface == null) {
			SelfShutOff();
			return;
		}
		hq = GameMaster.colonyController.hq;
		bool check = false;
		check = surface.cellsStatus != 1;
		if (status_gatherEnabled != check) {
			status_gatherEnabled = check;
			if (status_gatherEnabled) {
				check = surface.GetComponent<GatherSite>();
				if (check != status_gatherOrdered) {
					status_gatherOrdered = check;
					gatherButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord( status_gatherOrdered ? LocalizationKey.StopGather : LocalizationKey.Gather);
				}
			}
			else {
				gatherButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord( LocalizationKey.Gather );
				gatherButton.interactable = status_gatherEnabled;
			}
		}
		else {
			check = surface.GetComponent<GatherSite>();
			if (check != status_gatherOrdered) {
				status_gatherOrdered = check;
				gatherButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord( status_gatherOrdered ? LocalizationKey.StopGather : LocalizationKey.Gather);
			}
		}
		check = (surface.GetComponent<CleanSite>() != null && surface.GetComponent<CleanSite>().diggingMission);
		if (status_digOrdered != check) {
			status_digOrdered = check;
			digButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord( status_digOrdered == true ? LocalizationKey.StopDig : LocalizationKey.Dig);
		}
		if (savedHqLevel != hq.level) {
			savedHqLevel = hq.level;
			blockCreateButton.enabled = ( savedHqLevel> 3);
			columnCreateButton.enabled = (savedHqLevel > 4 && surface.pos.y < Chunk.CHUNK_SIZE - 1);
		}
		changeMaterialButton.enabled = (GameMaster.colonyController.gears_coefficient >=2);
	}

	public void BuildButton() {
		ChangeMode( SurfacePanelMode.Build );
	}
	public void BlockBuildingButton() {}
	public void ColumnBuildingButton() {}
	public void MaterialChangingButton() {}
	public void GatherButton() {
		if (surface == null) {
			SelfShutOff();
			return;
		}
		else {
			GatherSite gs = surface.GetComponent<GatherSite>();
			if (gs == null) {
				gs = surface.gameObject.AddComponent<GatherSite>();
				gs.Set(surface);
			}
			else Destroy(gs);
			StatusUpdate(); timer = STATUS_UPDATE_TIME;
		}
	}
	public void DigButton() {
		if (surface == null) {
			SelfShutOff();
			return;
		}
		else {
			CleanSite cs = surface.GetComponent<CleanSite>();
			if (cs == null) {
				cs = surface.gameObject.AddComponent<CleanSite>();
				cs.Set(surface, true);
			}
			else {
				if (cs.diggingMission) Destroy(cs);
			}
			StatusUpdate(); timer = STATUS_UPDATE_TIME;
		}
	}

	public void ChangeMode(SurfacePanelMode newMode) {
		switch (newMode) {
		case SurfacePanelMode.Build:
			float ypos = 0;
			int i = 0;
			hq = GameMaster.colonyController.hq;
			buildingsLevelToggles[0].transform.parent.gameObject.SetActive(true);
			while (i < buildingsLevelToggles.Length ) {
				if (i >= hq.level) buildingsLevelToggles[i].gameObject.SetActive(false);
				else {
					buildingsLevelToggles[i].gameObject.SetActive(true);
					if (i == constructingLevel - 1) buildingsLevelToggles[i].isOn = true; else  buildingsLevelToggles[i].isOn = false;
				}
				i++;
			}
			SetActionPanelStatus(false);

			returnButton.gameObject.SetActive(true);
			availableBuildingsButtons[0].transform.parent.gameObject.SetActive(true); // "surfaceBuildingPanel"
			RewriteBuildingButtons();



			// вывести стоимость выделенного здания в правое окно
			// вывести энергопотребление и размер здания
			// при постройке неполногабаритных зданий перевернуть окно и вывести сетку строительства

			mode = SurfacePanelMode.Build;
			break;
		case SurfacePanelMode.SelectAction:
			switch (mode) {
			case SurfacePanelMode.Build: SetBuildPanelStatus(false);	break;
			}
			SetActionPanelStatus(true);
			mode = SurfacePanelMode.SelectAction;
			break;
		}
	}

	void SetBuildPanelStatus ( bool working ) {
		buildingsLevelToggles[0].transform.parent.gameObject.SetActive(working);
		returnButton.gameObject.SetActive( working );
		availableBuildingsButtons[0].transform.parent.gameObject.SetActive( working ); 
		name.gameObject.SetActive( working );
		description.gameObject.SetActive( working );
		innerBuildButton.gameObject.SetActive( working );
	}

	void SetActionPanelStatus ( bool working ) {
		buildButton.gameObject.SetActive( working  );
		gatherButton.gameObject.SetActive( working  );
		digButton.gameObject.SetActive( working  );
		if (working) {
			status_gatherEnabled = (surface.cellsStatus != 1);
			gatherButton.interactable = status_gatherEnabled;
			status_digOrdered = (surface.GetComponent<CleanSite>() != null && surface.GetComponent<CleanSite>().diggingMission);
			digButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord( status_digOrdered ? LocalizationKey.StopDig : LocalizationKey.Dig);
			savedHqLevel = hq.level;
			blockCreateButton.enabled = ( savedHqLevel> 3);
			columnCreateButton.enabled = (savedHqLevel > 4 && surface.pos.y < Chunk.CHUNK_SIZE - 1);
			changeMaterialButton.enabled = (GameMaster.colonyController.gears_coefficient >=2);
		}
		else {
			if (changeMaterialButton.gameObject.activeSelf) changeMaterialButton.gameObject.SetActive( false ) ;
			if (blockCreateButton.gameObject.activeSelf) blockCreateButton.gameObject.SetActive( false  );
			if (columnCreateButton.gameObject.activeSelf) columnCreateButton.gameObject.SetActive( false  );
		}
	}

	public void SelectBuildingForConstruction (int id, int buttonIndex) {
		constructingBuildingID = id;
		availableBuildingsButtons[buttonIndex].image.overrideSprite =overridingSprite; 
		if (selectedBuildingButton > 0) availableBuildingsButtons[selectedBuildingButton].image.overrideSprite = null;
		selectedBuildingButton = buttonIndex;

		name.text = Localization.GetStructureName(id); name.gameObject.SetActive(true);
		description.text = Localization.GetStructureDescription(id); description.gameObject.SetActive(true);
	}

	public void SetConstructingLevel (int l) {
		constructingLevel = (byte)l;
		for (int i = 0; i < buildingsLevelToggles.Length; i++) {
			buildingsLevelToggles[i].isOn = ( i == (constructingLevel - 1) );
		}
		RewriteBuildingButtons();
	}

	void RewriteBuildingButtons () {
		List<Building> abuildings = Structure.GetApplicableBuildingsList(constructingLevel);
		for (int n = 0; n < availableBuildingsButtons.Length; n++) {
			if (n < abuildings.Count) {
				availableBuildingsButtons[n].gameObject.SetActive(true);
				RawImage rimage = availableBuildingsButtons[n].transform.GetChild(0).GetComponent<RawImage>();
				Vector2 txPos = Structure.GetTexturePosition(abuildings[n].id);
				rimage.uvRect = new Rect(txPos.x, txPos.y, 0.125f, 0.125f);
				availableBuildingsButtons[n].onClick.RemoveAllListeners();
				int sid = new int();
				sid = abuildings[n].id;
				int bid = new int();
				bid = n;
				availableBuildingsButtons[n].onClick.AddListener(() => {
					this.SelectBuildingForConstruction(sid, bid);
				});
			}
			else {
				availableBuildingsButtons[n].gameObject.SetActive(false);
			}
		}
	}
}

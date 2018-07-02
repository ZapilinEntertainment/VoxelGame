using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

enum GUIMode{ViewMode, QuestsWindow}
public enum ChosenObjectType{None,Surface, Cube, Structure, Worksite}

sealed public class UIController : MonoBehaviour {
	public RectTransform questPanel; // fill in Inspector
	public RectTransform[] questButtons; // fill in Inspector
	public GameObject returnToQuestList_button; // fill in Inspector
	public GameObject rightPanel, upPanel, menuPanel, menuButton; // fill in the Inspector
	public Button touchZone; // fill in the Inspector

	public Text selected_nameField; //fiti
	public Button selected_demolishButton;//fiti

	GUIMode mode;
	byte submode = 0;
	bool transformingRectInProgress = false, showMenuWindow = false;
	float rectTransformingSpeed = 0.8f, transformingProgress;
	RectTransform transformingRect; Vector2 resultingAnchorMin, resultingAnchorMax;
	int openedQuest = -1;

	float coinsCount, energyCount, energyMax;
	int citizenCount, freeWorkersCount, livespaceCount;

	SurfaceBlock chosenSurface;
	CubeBlock chosenCube; byte faceIndex = 10;
	Structure chosenStructure; UIObserver workingObserver;
	Worksite chosenWorksite;
	ChosenObjectType chosenObjectType;
	Transform selectionFrame; Material selectionFrameMaterial;

	public static UIController current;

	void Awake() {
		current = this;
		selectionFrame = Instantiate(Resources.Load<GameObject>("Prefs/structureFrame")).transform;
		selectionFrameMaterial = selectionFrame.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial;
		selectionFrame.gameObject.SetActive(false);
	}

	void Update() {
		if (transformingRectInProgress) {
			transformingProgress = Mathf.MoveTowards(transformingProgress, 1, rectTransformingSpeed * Time.deltaTime);
			transformingRect.anchorMin = Vector2.Lerp (transformingRect.anchorMin, resultingAnchorMin, transformingProgress);
			transformingRect.anchorMax = Vector2.Lerp (transformingRect.anchorMax, resultingAnchorMax, transformingProgress);
			if (transformingProgress == 1) {
				transformingProgress = 0;
				transformingRectInProgress = false;
			}
		}			
	}

	#region menu panel
	public void MenuButton() {
		showMenuWindow = !showMenuWindow;
		if (showMenuWindow) {
			if (rightPanel.activeSelf) rightPanel.SetActive(false);
			menuPanel.SetActive(true);
		}
		else {
			if (chosenObjectType != ChosenObjectType.None) {
				rightPanel.SetActive(true);
			}
			menuPanel.SetActive(false);
		}
	}
	public void SaveButton() {GameMaster.realMaster.SaveGame("newsave");}
	public void LoadButton(){GameMaster.realMaster.LoadGame("newsave");}
	#endregion

	public void Raycasting() {
		Vector2 mpos = Input.mousePosition;
		RaycastHit rh;
		if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rh)) {
			GameObject collided = rh.collider.gameObject;
			switch (collided.tag) {
			case "Structure":
				chosenStructure = collided.GetComponent<Structure>();
				chosenCube = null;
				chosenSurface = null;
				chosenWorksite = null;
				if (chosenStructure != null) ChangeChosenObject( ChosenObjectType.Structure ); 
				else ChangeChosenObject( ChosenObjectType.None );
				break;
			case "BlockCollider":
				Block b = collided.transform.parent.gameObject.GetComponent<Block>();
				switch (b.type) {
				case BlockType.Cave:
				case BlockType.Surface:
					chosenSurface = b as SurfaceBlock;
					chosenCube = null;
					if (chosenSurface != null) ChangeChosenObject( ChosenObjectType.Surface); else ChangeChosenObject( ChosenObjectType.None);
					break;
				case BlockType.Cube:
					chosenCube = b as CubeBlock;
					chosenSurface = null;
					if (chosenCube != null) {
						faceIndex = 10;
						for (byte i =0; i< 6; i++) {
							if (chosenCube.faces[i] == null) continue;
							if (chosenCube.faces[i].GetComponent<Collider>() == rh.collider ) {faceIndex = i;break;}
						}
						if (faceIndex  < 6) ChangeChosenObject( ChosenObjectType.Cube );
					}
					else ChangeChosenObject(ChosenObjectType.None);
					break;
				}
				chosenStructure = null;
				chosenWorksite = null;;
				break;
			case "WorksiteSign":
				WorksiteSign ws = collided.GetComponent<WorksiteSign>();
				if (ws != null)	chosenWorksite =  ws.worksite; else chosenWorksite = null;
				chosenStructure = null;
				chosenSurface = null;
				chosenCube = null;
				if (chosenWorksite != null) ChangeChosenObject( ChosenObjectType.Worksite ); else ChangeChosenObject( ChosenObjectType.None );
				break;
			}
		}
		else SelectedObjectLost();
	}

	public void ChangeChosenObject(ChosenObjectType newChosenType ) {
		//отключение предыдущего observer
		if (workingObserver != null) workingObserver.ShutOff();

		if (newChosenType == ChosenObjectType.None) {
			rightPanel.SetActive(false);
			selectionFrame.gameObject.SetActive(false);
			chosenObjectType = ChosenObjectType.None;
		}
		else {
			chosenObjectType = newChosenType;
			rightPanel.transform.SetAsLastSibling();
			rightPanel.SetActive(true);

			selectionFrame.gameObject.SetActive(true);
			if (showMenuWindow) {
				menuPanel.SetActive(false);
				showMenuWindow = false;
			}
		}

		Vector3 sframeColor = Vector3.one;
		switch (chosenObjectType) {
		case ChosenObjectType.None:
			faceIndex = 10;
			break;
		case ChosenObjectType.Surface:
			faceIndex = 10;
			selectionFrame.position = chosenSurface.transform.position + Vector3.down * Block.QUAD_SIZE/2f;
			selectionFrame.rotation = Quaternion.identity;
			selectionFrame.localScale = new Vector3(SurfaceBlock.INNER_RESOLUTION, 1, SurfaceBlock.INNER_RESOLUTION);
			sframeColor = new Vector3(140f/255f, 1,1);
			selectionFrame.gameObject.SetActive(true);
			break;
		case ChosenObjectType.Cube:
			selectionFrame.position = chosenCube.faces[faceIndex].transform.position;
			switch (faceIndex) {
				case 0: selectionFrame.transform.rotation = Quaternion.Euler(90,0,0);break;
				case 1: selectionFrame.transform.rotation = Quaternion.Euler(0,0,-90);break;
				case 2: selectionFrame.transform.rotation = Quaternion.Euler(-90,0,0);break;
				case 3: selectionFrame.transform.rotation = Quaternion.Euler(0,0,90);break;
				case 4: selectionFrame.transform.rotation = Quaternion.identity;break;
				case 5: selectionFrame.transform.rotation = Quaternion.Euler(-180,0,0);break;
			}
			selectionFrame.localScale = new Vector3(SurfaceBlock.INNER_RESOLUTION, 1, SurfaceBlock.INNER_RESOLUTION);
			sframeColor = new Vector3(140f/255f, 1,0.9f);
			break;
		case ChosenObjectType.Structure:
			faceIndex = 10;
			selectionFrame.position = chosenStructure.transform.position;
			selectionFrame.rotation = chosenStructure.transform.rotation;
			selectionFrame.localScale = new Vector3(chosenStructure.innerPosition.x_size, 1, chosenStructure.innerPosition.z_size);
			sframeColor = new Vector3(1,0,1);
			workingObserver = chosenStructure.ShowOnGUI();
			break;
		}

		selectionFrameMaterial.SetColor("_TintColor", Color.HSVToRGB(sframeColor.x, sframeColor.y, sframeColor.z));
	}

	#region right panel
	public void SelectedObjectLost() {
		if (chosenObjectType == ChosenObjectType.None) return;
		ChangeChosenObject(ChosenObjectType.None);
	}
	#endregion

	#region quest window
	public void OpenQuestWindow() {
		mode = GUIMode.QuestsWindow;
		questPanel.gameObject.SetActive(true);
	}

	public void QuestButton_OpenQuest(int index) {
		transformingRect = questButtons[index];
		transformingRectInProgress = true;
		transformingProgress = 0;
		resultingAnchorMin = Vector2.zero;
		resultingAnchorMax= Vector2.one;
		for (int i = 0; i < questButtons.Length; i++) {
			if (i == index) continue;
			else {
				questButtons[i].gameObject.SetActive(false);
			}
		}
		openedQuest = index;
		returnToQuestList_button.SetActive(true);
	}

	public void QuestButton_ReturnToQuestList() {
		if (openedQuest == - 1) return;
		QuestButton_RestoreButtonRect(openedQuest);
		transformingRectInProgress = true;
		transformingProgress = 0;
		for (int i = 0; i < questButtons.Length; i++) {
			if (i == openedQuest) continue;
			else {
				questButtons[i].gameObject.SetActive(true);
			}
		}
	}

	void QuestButton_RestoreButtonRect(int i) {
		switch (i) {
		case 0:
			resultingAnchorMin = new Vector2(0, 0.66f);
			resultingAnchorMax = new Vector2(0.33f, 1);
			break;
		case 1:
			resultingAnchorMin = new Vector2(0.33f, 0.66f);
			resultingAnchorMax = new Vector2(0.66f, 1);
			break;
		case 2:
			resultingAnchorMin = new Vector2(0.66f, 0.66f);
			resultingAnchorMax = new Vector2(1, 1);
			break;
		case 3:
			resultingAnchorMin = new Vector2(0, 0.33f);
			resultingAnchorMax = new Vector2(0.33f, 0.66f);
			break;
		case 4:
			resultingAnchorMin = new Vector2(0.33f, 0.33f);
			resultingAnchorMax= new Vector2(0.66f, 0.66f);
			break;
		case 5:
			resultingAnchorMin = new Vector2(0.66f, 0.33f);
			resultingAnchorMax = new Vector2(1, 0.66f);
			break;
		case 6:
			resultingAnchorMin = new Vector2(0, 0);
			resultingAnchorMax = new Vector2(0.33f, 0.33f);
			break;
		case 7:
			resultingAnchorMin = new Vector2(0.33f, 0);
			resultingAnchorMax = new Vector2(0.66f, 0.33f);
			break;
		case 8:
			resultingAnchorMin = new Vector2(0.66f, 0);
			resultingAnchorMax = new Vector2(1, 0.33f);
			break;
		}
	}
	#endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SurfaceImageSwitcher : MonoBehaviour, IPointerClickHandler {	
	float state = 0; // 0 is window, 1 is grid;
	[SerializeField]
	Sprite windowSprite, gridSprite; // fiti
	bool changingState = false, gridState = false;
	float speed = 3;
	[SerializeField]
	GameObject buttonsContainer; // fiti
	[SerializeField]
	Image obj; // fiti
	Button touchZone; 
	[SerializeField]
	UISurfacePanelController surfacePanelController;
	[SerializeField]
	GameObject buildZone; // fiti

	void Start() {
		touchZone= GetComponent<Button>();
	}

	void Update () {
		if (changingState) {
			if (gridState) {
				float prevState = state;
				state = Mathf.MoveTowardsAngle(state, 1, speed * Time.deltaTime);
				if (prevState < 0.5f & state >= 0.5f) {
					obj.sprite = gridSprite;
					obj.type = Image.Type.Simple;
					buttonsContainer.gameObject.SetActive(false);
					buildZone.SetActive(true);
				}
				if (state == 1) {
					changingState = false;
					touchZone.enabled = true;
					touchZone.interactable = true;
					obj.transform.localRotation = Quaternion.Euler(Vector3.zero);
				}
				else obj.transform.localRotation = Quaternion.Euler(new Vector3(0, state * 180, 0));
			}
			else {
				float prevState = state;
				state = Mathf.MoveTowardsAngle(state, 0, speed * Time.deltaTime);
				if (prevState > 0.5f & state <= 0.5f) {
					obj.sprite = windowSprite;
					obj.type = Image.Type.Sliced;
					buttonsContainer.gameObject.SetActive(true);
					buildZone.SetActive(false);
				}
				if (state == 0) 	{
					changingState = false;
				}
				else obj.transform.localRotation = Quaternion.Euler(new Vector3(0, state * 180, 0));
			}

		}
	}

	public void OnPointerClick(PointerEventData pointerEventData)
	{
		return;
		Vector2 pos = pointerEventData.position;
		print (pos);
		Vector3[] v = new Vector3[4];						 						//  1 2
		GetComponent<RectTransform>().GetWorldCorners(v); // 0 3
		pos.x -= v[0].x;
		pos.y -= v[0].y;
		float width = v[3].x - v[0].x;
		float height = v[2].y - v[3].y;
		byte x = (byte)(pos.x / width * SurfaceBlock.INNER_RESOLUTION) ;
		byte z = (byte)(pos.y / height * SurfaceBlock.INNER_RESOLUTION);
		surfacePanelController.CreateSelectedBuilding(x,z);
	}

	public void Switchy() {
		if ( gridState ) {
			gridState = false;
			touchZone.enabled = false;
			touchZone.interactable = false;
			obj.transform.localRotation = Quaternion.Euler(Vector3.up * 180);
		}
		else {
			gridState = true;

		}
		changingState = true;
	}

	void OnDisable () {
		state = 0;
		gridState = false;
		touchZone.enabled = false;
		touchZone.interactable = false;
		obj.sprite = windowSprite;
		obj.type = Image.Type.Sliced;
		obj.transform.localRotation = Quaternion.Euler(Vector3.zero);
		buildZone.SetActive(false);
	}
}

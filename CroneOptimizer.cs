using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CroneOptimizer : MonoBehaviour {
	static Sprite[] sprites;
	sbyte currentState = -1;
	[SerializeField]
	float maxRenderDistance = 40, meshRenderDistance = 5;
	[SerializeField]
	SpriteRenderer spRender;
	[SerializeField]
	MeshRenderer mRenderer;
	const float OPTIMIZATION_TIME = 1;
	float optimer = 0;


	void Awake() {
		if (sprites == null) {
			sprites = new Sprite[5];
			sprites[0] = Resources.Load<Sprite>("LOD Sprites/crone-45");
			sprites[1] = Resources.Load<Sprite>("LOD Sprites/crone0");
			sprites[2] = Resources.Load<Sprite>("LOD Sprites/crone30");
			sprites[3] = Resources.Load<Sprite>("LOD Sprites/crone45");
			sprites[4] = Resources.Load<Sprite>("LOD Sprites/crone90");
		}
		if (spRender == null) spRender = GetComponent<SpriteRenderer>();
		GameMaster.realMaster.AddToCameraUpdateBroadcast(gameObject);
	}

	void Update() {
		if (GameMaster.realMaster.treesOptimization == false) return;
		optimer -= Time.deltaTime;
		if (optimer <= 0) {  
			Optimize();
			optimer = OPTIMIZATION_TIME;
		}
	}

	void Optimize() {
		float dist = Vector3.Distance(transform.position, GameMaster.camPos);
		if ( dist >= maxRenderDistance) {
			if (spRender.enabled) {spRender.enabled = false;currentState = -1;}
			if (mRenderer.enabled) mRenderer.enabled = false;
		}
		else {
			if (dist > meshRenderDistance) { // SPRITE
				if ( !spRender.enabled ) spRender.enabled = true;
				if (mRenderer.enabled) mRenderer.enabled = false;
				sbyte newState = -1;
				if (optimer <= 0) {
					float ang = GameMaster.realMaster.camTransform.localRotation.eulerAngles.x;
					if (ang < 37.5f) {
						if (ang >= 15) 	newState = 2;
						else {
							if (ang >= -22.5f ) newState = 1;
							else newState = 0;
						}
					}
					else {
						if (ang > 80) newState = 4;
						else newState = 3;
					}
				}
				if (newState != -1 && newState != currentState) {currentState = newState; spRender.sprite = sprites[currentState];}
				spRender.transform.LookAt(GameMaster.camPos);
			}
			else {  // MESH
				if (spRender.enabled) {spRender.enabled = false;currentState = -1;}
				if ( !mRenderer.enabled ) mRenderer.enabled = true;
			}
		}
	}

	public void CameraUpdate(Transform t) {
		if (GameMaster.realMaster.treesOptimization == false) return;
		Optimize();
	}
}

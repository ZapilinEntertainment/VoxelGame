using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crop : Plant {
	[SerializeField]
	Sprite[] stages;
	byte currentStage = 0;
	[SerializeField]
	float maxVisibleDistance = 25;
	bool rangeVisibility = true;

	void Update() {
		if (GameMaster.gameSpeed == 0) return;
		float theoreticalGrowth = lifepower / maxLifepower;
		growth = Mathf.MoveTowards(growth, theoreticalGrowth,  growSpeed * GameMaster.lifeGrowCoefficient * Time.deltaTime);
		byte newStage = (byte)(growth * (stages.Length-1));
		if (newStage != currentStage) {
			(myRenderer as SpriteRenderer).sprite = stages[newStage];
			currentStage = newStage;
		}
	}	

	override public void SetLifepower(float p) {
		lifepower = p; 
		if (lifepower < maxLifepower) full = false; else full = true;
	}

	public override void SetGrowth(float t) {
		growth = t;
		byte newStage = (byte)(growth * (stages.Length-1));
		if (newStage != currentStage) {
			(myRenderer as SpriteRenderer).sprite = stages[newStage];
			currentStage = newStage;
		}
	}

	public void CameraUpdate(Transform t) {
		if (Vector3.Distance(myRenderer.transform.position, t.position) > maxVisibleDistance) { 
			if (rangeVisibility) {
				if (visible) myRenderer.enabled = false; 
				rangeVisibility = false;
			}
		}
		else {
			if (!rangeVisibility) {
				if (visible) myRenderer.enabled = true; 
				rangeVisibility =true;
			}
			Vector3 dir = t.position - myRenderer.transform.position;
			dir.y = 0;
			myRenderer.transform.forward = dir;
		}
	}

	override public void SetVisibility( bool x) {
		if (x == visible) return;
		else {
			visible = x;
			if ( visible && rangeVisibility ) myRenderer.enabled = true;
			else {if (myRenderer.enabled) myRenderer.enabled = false;}
		}
	}

	public override void Annihilate( bool forced ) {
		if (basement != null && !forced ) {
			if (basement.grassland != null) basement.grassland.AddLifepower((int)(lifepower * GameMaster.lifepowerLossesPercent));
			basement.RemoveStructure( this );
		}
		Farm f = ( transform.parent == null ? null : transform.parent.GetComponent<Farm>() );
		if (f == null) Destroy(gameObject);
		else f.ReturnCropToPool(this);
	}
}

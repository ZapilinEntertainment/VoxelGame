﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : Structure {
	public float lifepower {get;protected set;}
	public float maxLifepower {get;protected set;}
	public float maxTall {get; protected set;}
	public bool full {get;protected set;}

	void Awake() {
		PrepareStructure();
		lifepower = 1;
		maxLifepower = 10;
		full = false;
		maxTall = 0.15f + Random.value * 0.05f;
	}

		
	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetStructureData(b,pos);
		transform.localRotation = Quaternion.Euler(0, Random.value * 360, 0);
	}

	public virtual void AddLifepower(int life) {
		if (full) return;
		lifepower += life;
		if (lifepower > maxLifepower) full = true;
	}

	public virtual float TakeLifepower(int life) {
		float lifeTransfer = life;
		if (life > lifepower) {if (lifepower >= 0) lifeTransfer = lifepower; else lifeTransfer = 0;}
		lifepower -= lifeTransfer;
		if (lifepower < maxLifepower) full = false;
		return (int)lifeTransfer;
	}

	public virtual void Annihilate() {
		basement.grassland.AddLifepower((int)lifepower);
		basement =null;
		Destroy(gameObject);
	}
}

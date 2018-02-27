using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : Plant {
	public Transform crone, trunk, crone_sprite, trunk_sprite;

	void Awake() {
		GameMaster.realMaster.AddToCameraUpdateBroadcast(gameObject);
		cellPosition = PixelPosByte.Empty;
		full = false;
		maxLifepower = 10000;
		maxTall = 0.5f;
	}

	public void CameraUpdate(Transform t) {
		crone_sprite.LookAt(t.position);
		trunk_sprite.LookAt(t.position);
	}

	override public void AddLifepower(float life) {
		if (full) return;
		lifepower += life;
		float height = lifepower / maxLifepower ;
		if (height >= 1) {full = true;}
		else {
			transform.localScale = Vector3.one * height * maxTall;
		}
	}
}

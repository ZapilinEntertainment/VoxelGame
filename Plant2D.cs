using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant2D : Plant {
	public LineRenderer body;
	public const float MAXIMUM_LIFEPOWER = 50;
	float startSize = 0.05f;

	void Awake () {
		lifepower = 1;
		maxLifepower = MAXIMUM_LIFEPOWER;
		maxTall = 0.1f;
		full = false;
		hp = maxHp;
		innerPosition = SurfaceRect.Empty;
	}

	override public  void AddLifepower(int life) {
		if (full || life < 0) return;
		lifepower += life;
		float height = lifepower / maxLifepower ;
		if (height >= 1) {full = true; }
		else {
			body.SetPosition(1, body.GetPosition(0) + Vector3.up * (height * maxTall+ startSize));
			body.startWidth = height * maxTall + startSize;
		}
	}
}

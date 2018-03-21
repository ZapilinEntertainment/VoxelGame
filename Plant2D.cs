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
		innerPosition = new SurfaceRect(0,0,xsize_to_set, zsize_to_set);
		isArtificial = markAsArtificial;
		type = setType;
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

	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		basement = b;
		innerPosition = new SurfaceRect(pos.x, pos.y, xsize_to_set, zsize_to_set);
		b.AddStructure(new SurfaceObject(innerPosition, this));
		body.SetPosition(0, transform.position);
		body.SetPosition(1,transform.position + Vector3.up * startSize);
	}
}

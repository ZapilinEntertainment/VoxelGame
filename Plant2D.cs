using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant2D : Plant {
	public LineRenderer body;
	public const float MAXIMUM_LIFEPOWER = 50;
	float startSize = 0.05f;

	void Awake () {
		cellPosition = PixelPosByte.Empty;
		lifepower = 1;
		maxLifepower = MAXIMUM_LIFEPOWER;
		maxTall = 0.1f;
		full = false;
	}

	public override void SetPosition(PixelPosByte cellPos, Block block) {
		cellPosition = cellPos;
		basement = block;
		transform.parent = block.upSurface.transform;
		body.SetPosition(0,block.upSurface.transform.TransformPoint( BlockSurface.GetLocalPosition(cellPos) ));
		body.SetPosition(1, body.GetPosition(0) + Vector3.up * startSize);
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

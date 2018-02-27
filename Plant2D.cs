using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant2D : Plant {
	public LineRenderer body;
	float startSize = 0.01f;

	void Awake () {
		cellPosition = PixelPosByte.Empty;
		lifepower = 1;
		maxLifepower = 1000;
		maxTall = 0.1f;
		full = false;
	}

	public override void SetPosition(PixelPosByte cellPos, Transform block) {
		cellPosition = cellPos;
		transform.parent = block;
		float res = BlockSurface.INNER_RESOLUTION;
		float a = cellPos.x, b = cellPos.y;
		body.SetPosition(0,block.TransformPoint( (a/res - 0.5f) * 0.9f * Block.QUAD_SIZE , Block.QUAD_SIZE / 2f, (b/res - 0.5f)* 0.9f * Block.QUAD_SIZE));
		body.SetPosition(1, body.GetPosition(0) + Vector3.up * startSize);
	}

	override public  void AddLifepower(float life) {
		if (full) return;
		lifepower += life;
		float height = lifepower / maxLifepower ;
		if (height >= 1) {full = true; }
		else {
			body.SetPosition(1, body.GetPosition(0) + Vector3.up * (height * maxTall+ startSize));
			body.startWidth = height * maxTall + startSize;
		}
	}
}

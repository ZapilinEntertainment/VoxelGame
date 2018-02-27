using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : MonoBehaviour {
	public float lifepower {get;protected set;}
	public PixelPosByte cellPosition{get;protected set;}
	public float maxLifepower {get;protected set;}
	public float maxTall {get; protected set;}
	public bool full {get;protected set;}

	void Awake() {
		cellPosition = PixelPosByte.Empty;
		lifepower = 0;
		maxLifepower = 10;
		full = false;
		maxTall = 0.2f;
	}

	public virtual void SetPosition(PixelPosByte cellPos, Transform block) {
		cellPosition = cellPos;
		transform.parent = block;
		float res = BlockSurface.INNER_RESOLUTION;
		float a = cellPos.x, b = cellPos.y;
		transform.localPosition = new Vector3((a/res- 0.5f) * 0.9f * Block.QUAD_SIZE, Block.QUAD_SIZE / 2f, (b/res - 0.5f) * 0.9f* Block.QUAD_SIZE);
		transform.localRotation = Quaternion.Euler(0, Random.value * 360, 0);
	}

	public virtual void AddLifepower(float life) {
		if (full) return;
		lifepower += life;
		if (lifepower > maxLifepower) full = true;
	}

	public virtual float TakeLifepower(float life) {
		float count = life;
		if (life > lifepower) count = lifepower;
		lifepower -= count;
		if (lifepower < maxLifepower) full = false;
		return count;
	}
}

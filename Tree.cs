using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : Plant {
	public Transform crone, trunk;
	public const int MAXIMUM_LIFEPOWER = 1000;

	void Awake() {
		full = false;
		lifepower = 1;
		maxLifepower = MAXIMUM_LIFEPOWER;
		maxTall = 0.4f + Random.value * 0.1f;
		hp = maxHp;
		innerPosition = SurfaceRect.Empty;
	}

		
	public virtual int TakeLifepower(int life) {
		float lifeTransfer = life;
		if (life > lifepower) {if (lifepower >= 0) lifeTransfer = lifepower; else lifeTransfer = 0;}
		lifepower -= lifeTransfer;
		if (lifepower < maxLifepower) full = false;
		return (int)lifeTransfer;
	}

	override public void AddLifepower(int life) {
		if (full) return;
		lifepower += life;
		float height = lifepower / maxLifepower ;
		maxHp = height * maxTall * 1000; hp = maxHp;
		if (height >= 1) { full = true; }
		else {transform.localScale = Vector3.one * height * maxTall;	}
	}

	public override void Annihilate() {
		if (basement != null) {
			basement.grassland.AddLifepower((int)lifepower);
			GameObject g = Instantiate(Resources.Load<GameObject>("Lifeforms/DeadTree")) as GameObject;
			g.transform.localScale = transform.localScale;
			HarvestableResource hr = g.GetComponent<HarvestableResource>();
			hr.SetResources(ResourceType.Lumber, (int)(100 * trunk.transform.localScale.y));
			basement.ReplaceStructure(new SurfaceRect(innerPosition.x, innerPosition.z, innerPosition.x_size, innerPosition.z_size, Content.HarvestableResources, g));
			basement = null;
		}
		else Destroy(gameObject);
	}
}

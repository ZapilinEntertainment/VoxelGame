using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : Plant {
	public Transform crone, trunk;
	public const int MAXIMUM_LIFEPOWER = 1000;
	float croneState = 1;

	void Awake() {
		cellPosition = PixelPosByte.Empty;
		full = false;
		lifepower = 1;
		maxLifepower = MAXIMUM_LIFEPOWER;
		maxTall = 0.4f + Random.value * 0.1f;
	}
		
	public virtual int TakeLifepower(int life) {
		float lifeTransfer = life;
		if (life > lifepower) {if (lifepower >= 0) lifeTransfer = lifepower; else lifeTransfer = 0;}
		lifepower -= lifeTransfer;
		if (croneState > 0.5f) {
			croneState -= life/maxLifepower/2f;
			crone.transform.localScale = Vector3.one * croneState;
			if (croneState <= 0.5f) crone.GetComponent<MeshRenderer>().material = PoolMaster.current.dryingLeaves_material;
		}
		if (lifepower < maxLifepower) full = false;
		return (int)lifeTransfer;
	}

	override public void AddLifepower(int life) {
		if (full) return;
		lifepower += life;
		float height = lifepower / maxLifepower ;
		if (height >= 1) { full = true; croneState = 1; crone.transform.localScale = Vector3.one;}
		else {
			transform.localScale = Vector3.one * height * maxTall;
			if (croneState < 1) {
				float prevCroneState = croneState;
				croneState += life / maxLifepower / 2f; 
				if (croneState > 0.7f && prevCroneState < 0.7f) crone.GetComponent<MeshRenderer>().material = PoolMaster.current.leaves_material;
				if (croneState > 1) croneState = 1;
				crone.transform.localScale = croneState * Vector3.one;
				} 
		}
	}

	public override void Annihilate() {
		basement.grassland.AddLifepower((int)lifepower);
		basement.upSurface.ClearCell(cellPosition, Content.Plant);

		GameObject g = Instantiate(Resources.Load<GameObject>("Lifeforms/DeadTree")) as GameObject;
		g.transform.localScale = transform.localScale;
		HarvestableResource hr = g.GetComponent<HarvestableResource>();
		hr.SetResources(ResourceType.Lumber, (int)(100 * trunk.transform.localScale.y), ResourceType.Nothing, 0);
		hr.SetPosition(cellPosition, basement);

		Destroy(gameObject);
	}
}

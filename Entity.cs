using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour {
	protected float lifepower = 1, speed = 1, hp , maxhp = 1,stamina = 10, restTime = 60, age, maxAge = 10;
	GameObject home;

	void Awake() {
		GameMaster.realMaster.everyYearUpdateList.Add(this);
		age = 0; hp = maxhp;
	}

	public void EveryYearUpdate() {
		age++;
		if (age >= maxAge) Die();
	}

	public virtual void Die() {
		RaycastHit rh;
		if (Physics.Raycast(transform.position, Vector3.down, out rh)) {
			Block b = rh.collider.transform.parent.GetComponent<Block>();
			if (b != null) {
				Grassland gl = b.GetComponent<Grassland>();
				if (gl == null) b.myChunk.AddLifePower((int)lifepower);
				else gl.AddLifepower((int)lifepower);
			}
		}
		Destroy(gameObject);
	}

	void OnDestroy() {
		GameMaster.realMaster.everyYearUpdateList.Remove(this);
	}
}

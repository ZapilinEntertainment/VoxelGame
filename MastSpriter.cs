using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MastSpriter : MonoBehaviour {
	public float maxDistance = 40 * Block.QUAD_SIZE;
	bool enabled = true;
	public SpriteRenderer spRender;
	void Awake() {
		GameMaster.realMaster.AddToCameraUpdateBroadcast(gameObject);
		if (spRender == null) spRender = GetComponent<SpriteRenderer>();
	}

	public void CameraUpdate(Transform t) {
		if (Vector3.Distance(transform.position, t.position) > maxDistance) { if (enabled) {spRender.enabled = false; enabled = false;}}
		else {
			if (!enabled) {spRender.enabled = true;enabled=true;}
			Vector3 dir = t.position - transform.position;
			dir.y = 0;
			transform.forward = dir;
		}
	}
}

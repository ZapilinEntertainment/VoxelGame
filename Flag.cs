using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : MonoBehaviour {
	public float maxDistance = 40 * Block.QUAD_SIZE;
	bool visible = true;
	public SpriteRenderer spRender;
	public GameObject flag;
	void Awake() {
		GameMaster.realMaster.AddToCameraUpdateBroadcast(gameObject);
	}
		

	public void CameraUpdate(Transform t) {
		Vector3 pos = t.position;
		if (Vector3.Distance(pos, transform.position) > maxDistance) { if (visible) {spRender.enabled = false; visible = false;}}
		else {
			if (!visible) {spRender.enabled = true; visible=true;}
			pos.y = transform.position.y;
			flag.transform.LookAt(pos);
		}
	}
}

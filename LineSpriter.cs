using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineSpriter : MonoBehaviour {
	public float maxDistance = 40 * Block.QUAD_SIZE;
	bool visible = true;
	public SpriteRenderer spRender;
	void Awake() {
		GameMaster.realMaster.AddToCameraUpdateBroadcast(gameObject);
		if (spRender == null) spRender = GetComponent<SpriteRenderer>();
	}

	public void CameraUpdate(Transform t) {
		Vector3 dir = transform.InverseTransformDirection(t.position - transform.position);
		if (dir.magnitude > maxDistance) { if (visible) {spRender.enabled = false; visible = false;}}
		else {
			if (!visible) {spRender.enabled = true; visible=true;}
				dir.z = 0; 
				transform.rotation = Quaternion.LookRotation(transform.forward, transform.TransformDirection(dir));
				Vector3 nScale = transform.localScale;
				nScale.x = dir.magnitude;
				transform.localScale = nScale;
		}
	}
}

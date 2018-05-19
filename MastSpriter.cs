using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MastSpriter : MonoBehaviour {
	public float maxRenderDistance = 20;
	public SpriteRenderer spRender;
	const float OPTIMIZATION_TIME = 1;
	float optimer = 0;
	bool visible = true;

	void Awake() {
		if (spRender == null) spRender = GetComponent<SpriteRenderer>();
		GameMaster.realMaster.AddToCameraUpdateBroadcast(gameObject);
	}

	void Update() {
		if (!visible) return;
		optimer -= Time.deltaTime;
		if (optimer <= 0) {
			Vector3 dir = GameMaster.camPos - transform.position;
			if ( dir.magnitude >= maxRenderDistance) {
				if (spRender.enabled) spRender.enabled = false;
			}
			else {
				if ( !spRender.enabled ) spRender.enabled = true;
				dir = Vector3.ProjectOnPlane(dir, transform.TransformDirection(Vector3.up));
				transform.rotation = Quaternion.LookRotation(dir.normalized, transform.TransformDirection(Vector3.up));
			}
			optimer = OPTIMIZATION_TIME;
		}
	}

	public void SetVisibility( bool x) {
		if (x == visible) return;
		visible = x;
		spRender.enabled = x;
		optimer = 0;
	}

	public void CameraUpdate(Transform t) {
		if (!visible) return;
		Vector3 dir = GameMaster.camPos - transform.position;
		if ( dir.magnitude >= maxRenderDistance) {
			if (spRender.enabled) spRender.enabled = false;
		}
		else {
			if ( !spRender.enabled ) spRender.enabled = true;
			dir = Vector3.ProjectOnPlane(dir, transform.TransformDirection(Vector3.up));
			transform.rotation = Quaternion.LookRotation(dir.normalized, transform.TransformDirection(Vector3.up));
		}
		optimer = OPTIMIZATION_TIME;
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spriter : MonoBehaviour {
	void Awake() {
		GameMaster.realMaster.AddToCameraUpdateBroadcast(gameObject);
	}
	void Start() {
		CameraUpdate(Camera.main.transform);
	}

	public void CameraUpdate(Transform t) {
		Vector3 dir = t.position - transform.position;
		dir.y = 0;
		transform.forward = dir;
	}

}

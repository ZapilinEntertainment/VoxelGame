using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fluger : MonoBehaviour {
	Vector3 direction;
	void Awake () {
		GameMaster.realMaster.windUpdateList.Add(this);
		direction = transform.forward;
	}

	void Update() {
		if (transform.forward != direction) transform.forward = Vector3.MoveTowards(transform.forward, direction, 5 * Time.deltaTime);
	}

	public void WindUpdate(Vector3 v) {
		if (v != Vector3.zero) direction = v ;
	}
}

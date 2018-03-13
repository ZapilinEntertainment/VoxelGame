using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fluger : MonoBehaviour {

	void Awake () {
		GameMaster.realMaster.windUpdateList.Add(this);
	}

	public void WindUpdate(Vector3 v) {
		transform.forward = v;
	}
}

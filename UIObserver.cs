using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIObserver : MonoBehaviour {

	virtual public void ShutOff() {
		gameObject.SetActive(false);
	}
}

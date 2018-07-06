using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIObserver : MonoBehaviour {
	protected bool isObserving = false;
	protected float timer = 0, STATUS_UPDATE_TIME = 1;

	/// <summary>
	/// Call from outside
	/// </summary>
	virtual public void ShutOff() {
		isObserving = false;
		gameObject.SetActive(false);
	}
	/// <summary>
	/// Call from inheritors
	/// </summary>
	virtual public void SelfShutOff() {
		isObserving = false;
		gameObject.SetActive(false);
	}

	void Update() {
		timer -= Time.deltaTime;
		if (timer <= 0) {
			StatusUpdate();
			timer = STATUS_UPDATE_TIME;
		}
	}

	protected virtual void StatusUpdate() {
		
	}

	void OnEnable() {
		transform.SetAsLastSibling();
	}
}

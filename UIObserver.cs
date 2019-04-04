using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIObserver : MonoBehaviour {
	public bool isObserving { get; protected set; }
    protected bool subscribedToUpdate = false;
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
    virtual public void LocalizeTitles()
    { }
    protected virtual void StatusUpdate() {		
	}

	private void OnEnable() {
		transform.SetAsLastSibling();
        if (!subscribedToUpdate)
        {
            UIController.current.statusUpdateEvent += StatusUpdate;
            subscribedToUpdate = true;
        }
	}
    private void OnDisable()
    {
        if (subscribedToUpdate)
        {
            UIController.current.statusUpdateEvent -= StatusUpdate;
            subscribedToUpdate = false;
        }
    } 
    private void OnDestroy()
    {
        if (GameMaster.sceneClearing) return;
        if (subscribedToUpdate)
        {
            UIController uc = UIController.current;
            if (uc != null) uc.statusUpdateEvent -= StatusUpdate;
        }
    }
}

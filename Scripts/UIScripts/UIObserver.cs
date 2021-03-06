﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIObserver : MonoBehaviour {
	public bool isObserving { get; protected set; }
    protected bool subscribedToUpdate = false;
    public static MainCanvasController mycanvas { get; private set; }

    public static void LinkToMainCanvasController(MainCanvasController mcc)
    {
        mycanvas = mcc;
    }

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
    public virtual void StatusUpdate() {		
	}

	protected virtual void OnEnable() {
		transform.SetAsLastSibling();
        if (!subscribedToUpdate)
        {
            mycanvas.statusUpdateEvent += StatusUpdate;
            subscribedToUpdate = true;
        }
	}
    protected virtual void OnDisable()
    {
        if (subscribedToUpdate)
        {
            mycanvas.statusUpdateEvent -= StatusUpdate;
            subscribedToUpdate = false;
        }
    } 
    protected virtual void OnDestroy()
    {
        if (GameMaster.sceneClearing) return;
        if (mycanvas != null && subscribedToUpdate) mycanvas.statusUpdateEvent -= StatusUpdate;
        //dependency - UISurfacePanelController
    }
}

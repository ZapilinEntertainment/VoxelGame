using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MastSpriter : MonoBehaviour {
	SpriteRenderer spRender;
	bool visible = true;

	void Awake() {
        FollowingCamera.main.AddMastSprite(transform);
        spRender = GetComponent<SpriteRenderer>();
	}

	public void SetVisibility( bool x) {
		if (x == visible) return;
		visible = x;
		spRender.enabled = x;
        if (visible) FollowingCamera.main.AddMastSprite(transform);
        else FollowingCamera.main.RemoveMastSprite(transform.GetInstanceID());
    }

    private void OnEnable()
    {
        if (visible) FollowingCamera.main.AddMastSprite(transform);
    }
    private void OnDisable()
    {
        if (visible) FollowingCamera.main.RemoveMastSprite(transform.GetInstanceID());
    }

    private void OnDestroy()
    {
        if (GameMaster.applicationStopWorking) return;
        FollowingCamera.main.RemoveMastSprite(transform.GetInstanceID());
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spriter : MonoBehaviour {
    SpriteRenderer spRender;
    bool visible = true;

    void Start() {
        FollowingCamera.main.AddSprite(transform);
        spRender = GetComponent<SpriteRenderer>();
    }

    public void SetVisibility(bool x)
    {
        if (x == visible) return;
        visible = x;
        spRender.enabled = x;
        if (visible) FollowingCamera.main.AddSprite(transform);
        else FollowingCamera.main.RemoveSprite(transform.GetInstanceID());
    }

    private void OnEnable()
    {
        if (visible) FollowingCamera.main.AddSprite(transform);
    }
    private void OnDisable()
    {
        if (visible) FollowingCamera.main.RemoveSprite(transform.GetInstanceID());
    }

    private void OnDestroy()
    {
        if (GameMaster.applicationStopWorking) return;
        FollowingCamera.main.RemoveSprite(transform.GetInstanceID());
    }
}

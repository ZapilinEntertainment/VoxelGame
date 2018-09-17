using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spriter : MonoBehaviour {
    SpriteRenderer spRender;
    bool visible = true, subscribed = false;

    void Start() {
        subscribed = FollowingCamera.main.AddSprite(transform);
        spRender = GetComponent<SpriteRenderer>();
        if (spRender == null & transform.childCount != 0)
        {
            spRender = transform.GetChild(0).GetComponent<SpriteRenderer>();
            if (spRender == null)
            {
                print("error : no renderer");
                Destroy(gameObject);
            }
        }
    }

    public void SetVisibility(bool x)
    {
        if (x == visible) return;
        visible = x;
        spRender.enabled = x;
        if (visible) subscribed = FollowingCamera.main.AddSprite(transform);
        else
        {
            FollowingCamera.main.RemoveSprite(transform.GetInstanceID());
            subscribed = false;
        }
    }

    private void OnEnable()
    {
        if (visible) subscribed = FollowingCamera.main.AddSprite(transform);
    }
    private void OnDisable()
    {
        if (visible)
        {
            FollowingCamera.main.RemoveSprite(transform.GetInstanceID());
            subscribed = false;
        }
    }

    private void OnDestroy()
    {
        if (GameMaster.applicationStopWorking | !subscribed) return;
        FollowingCamera.main.RemoveSprite(transform.GetInstanceID());
    }
}

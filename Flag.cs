using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : MonoBehaviour {
	public float maxDistance = 40 * Block.QUAD_SIZE;
	bool visible = true;
	public SpriteRenderer spRender;
	public GameObject flag;

	void Awake() {
        FollowingCamera.main.cameraChangedEvent += CameraUpdate;
	}		

	public void CameraUpdate() {
        Vector3 pos = FollowingCamera.camPos;
        if (Vector3.Distance(pos, transform.position) > maxDistance) { if (visible) {spRender.enabled = false; visible = false;}}
		else {
			if (!visible) {spRender.enabled = true; visible=true;}
			pos.y = transform.position.y;
			flag.transform.LookAt(pos);
		}
	}

    private void OnDestroy()
    {
        if (GameMaster.applicationStopWorking) return;
        FollowingCamera.main.cameraChangedEvent -= CameraUpdate;
    }
}

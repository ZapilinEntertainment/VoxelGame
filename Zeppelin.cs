using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zeppelin : Transport {
	Block landingPlace;
	bool landing = false, anchored = false;
	public Transform anchor, leftScrew,rightScrew;
	public LineRenderer anchorChain;
	float anchorSpeed = 0, flySpeed = 1, landingSpeed = 10;
	Vector3 centralPoint = new Vector3(8,8,8), anchorStartPos = Vector3.zero;

	void Start() {
		centralPoint = Vector3.one * Chunk.CHUNK_SIZE/2f;
		centralPoint.y ++;
		transform.position = Vector3.one * Chunk.CHUNK_SIZE + Vector3.up;
		Vector3 v = centralPoint - transform.position; v.y = 0;
		transform.forward = v;
		anchorStartPos = transform.InverseTransformPoint(anchor.transform.position);
		leftScrew.Rotate(0, Random.value * 360, 0);
		rightScrew.Rotate(0, Random.value * 360, 0);
	}

	void Update() {
		if ( !landing) {
			Vector3 v = centralPoint - transform.position; v.y = 0;
			transform.forward = Quaternion.AngleAxis(90, Vector3.up) * v;
			transform.Translate(Vector3.forward * flySpeed * Time.deltaTime * GameMaster.gameSpeed,Space.Self);
			if (anchor.transform.localPosition != anchorStartPos) { anchor.transform.localPosition = Vector3.MoveTowards(anchor.transform.localPosition, anchorStartPos, 2 * Time.deltaTime * GameMaster.gameSpeed); }
			if (transform.position.y < Chunk.CHUNK_SIZE) transform.Translate(Vector3.up * landingSpeed/2f * Time.deltaTime * GameMaster.gameSpeed);
		}
		else {
			Vector3 stopPoint = new Vector3(landingPlace.pos.x, centralPoint.y, landingPlace.pos.z);
			if (Vector3.Distance(transform.position, stopPoint) > 0.01f) transform.position = Vector3.MoveTowards(transform.position, stopPoint, flySpeed * Time.deltaTime);
			else {
				if ( !anchored ) {
					anchorSpeed += 9.8f * Time.deltaTime;
					float speed = anchorSpeed * Time.deltaTime * GameMaster.gameSpeed;
					RaycastHit rh;
					if (Physics.Raycast(transform.position, Vector3.down, out rh, Chunk.CHUNK_SIZE * 2)) {
						float delta = (anchor.transform.position - rh.point).y;
						if (delta <= speed) {anchored = true; anchor.transform.position = rh.point;} 
						else anchor.Translate(Vector3.down * speed);
					}
					else {landingPlace = null; landing = false;}
				}
				else {
					RaycastHit rh;
					if (Physics.Raycast(transform.position, Vector3.down, out rh, Chunk.CHUNK_SIZE * 2)) {
						float speed = landingSpeed * Time.deltaTime * GameMaster.gameSpeed;
						float delta = (transform.position - rh.point).y;
						if (delta <= speed) { // zeppelin landed
							
						} 
						else transform.Translate (Vector3.down * speed);
					}
					else {
						landingPlace = null; landing = false; anchored = false;
					}
				}
			}
		}
		anchorChain.SetPosition(0, transform.TransformPoint(anchorStartPos));
		anchorChain.SetPosition(1, anchor.transform.position);
		leftScrew.Rotate(0, 500 * Time.deltaTime * GameMaster.gameSpeed,0);
		rightScrew.Rotate(0, 500 * Time.deltaTime * GameMaster.gameSpeed,0);
	}

	public void SetLandingPlace(Block block) {
		landingPlace = block;
		landing = true;
	} 
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zeppelin : Transport {
	SurfaceBlock landingPlace, s_place1, s_place2;
	bool landing = false, anchored = false, landed = false;
	public Transform anchor, leftScrew,rightScrew, body;
	public LineRenderer anchorChain;
	float anchorSpeed = 0, flySpeed = 1, landingSpeed = 1;
	Vector3 centralPoint = new Vector3(8,8,8), anchorStartPos = Vector3.zero;
	public AudioSource propeller_as, anchorChain_as;
	public AudioClip anchorLanded_ac;

	void Start() {
		centralPoint = Vector3.one * Chunk.CHUNK_SIZE/2f;
		centralPoint.y ++;
		transform.position = Vector3.one * Chunk.CHUNK_SIZE + Vector3.up;
		Vector3 v = centralPoint - transform.position; v.y = 0;
		transform.forward = v;
		anchorStartPos = transform.InverseTransformPoint(anchor.transform.position);
		leftScrew.Rotate(0, Random.value * 360, 0);
		rightScrew.Rotate(0, Random.value * 360, 0);
		LandingUI.current.startTransport = this;
		LandingUI.current.landing = true;
	}

	void Update() {
		if ( !landed ) {
			if ( !landing) {
				Vector3 v = centralPoint - transform.position; v.y = 0;
				transform.forward = Quaternion.AngleAxis(90, Vector3.up) * v;
				transform.Translate(Vector3.forward * flySpeed * Time.deltaTime * GameMaster.gameSpeed,Space.Self);
				if (anchor.transform.localPosition != anchorStartPos) { 
					anchor.transform.localPosition = Vector3.MoveTowards(anchor.transform.localPosition, anchorStartPos, 2 * Time.deltaTime * GameMaster.gameSpeed); 
					anchorChain_as.enabled = true;
				}
				else anchorChain_as.enabled = false;
				if (transform.position.y < Chunk.CHUNK_SIZE) transform.Translate(Vector3.up * landingSpeed/2f * Time.deltaTime * GameMaster.gameSpeed);
				leftScrew.Rotate(0, 500 * Time.deltaTime * GameMaster.gameSpeed,0);
				rightScrew.Rotate(0, 500 * Time.deltaTime * GameMaster.gameSpeed,0);
			}
			else {
				Vector3 stopPoint = new Vector3(landingPlace.pos.x, transform.position.y, landingPlace.pos.z);
				if (Vector3.Distance(transform.position, stopPoint) > 0.01f) {
					transform.position = Vector3.MoveTowards(transform.position, stopPoint, flySpeed * Time.deltaTime);
					transform.forward = stopPoint - transform.position;
					leftScrew.Rotate(0, 500 * Time.deltaTime * GameMaster.gameSpeed,0);
					rightScrew.Rotate(0, 500 * Time.deltaTime * GameMaster.gameSpeed,0);
				}
				else {
					if ( !anchored ) {
						anchorChain_as.enabled = true;
						anchorSpeed += 9.8f * Time.deltaTime;
						float speed = anchorSpeed * Time.deltaTime * GameMaster.gameSpeed;
						RaycastHit rh;
						if (Physics.Raycast(transform.position, Vector3.down, out rh, Chunk.CHUNK_SIZE * 2)) {
							float delta = (anchor.transform.position - rh.point).y;
							if (delta <= speed) {
								anchored = true; 
								AudioSource.PlayClipAtPoint(anchorLanded_ac, anchor.transform.position);
								anchorChain_as.enabled = false;
								anchor.transform.position = rh.point + Vector3.up * 0.01f;
								propeller_as.enabled = false;
							} 
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
								landed = true;
								anchor.gameObject.SetActive(false);
								anchorChain_as.enabled = false;

								GameMaster.colonyController = GameMaster.realMaster.gameObject.AddComponent<ColonyController>();

								Structure hq = Instantiate(Resources.Load<GameObject>("Structures/ZeppelinBasement")).GetComponent<Structure>();
								hq.SetBasement(landingPlace, PixelPosByte.zero);

								landingPlace.MakeIndestructible(true);
								landingPlace.basement.MakeIndestructible(true);
								GameMaster.colonyController.AddCitizens(GameMaster.START_WORKERS_COUNT);
								GameMaster.colonyController.SetHQ(hq.GetComponent<HeadQuarters>());

								Chunk c = landingPlace.myChunk;

								Structure storage = Instantiate(Resources.Load<GameObject>("Structures/Storage_level_0")).GetComponent<Structure>();
								storage.SetBasement(s_place1, PixelPosByte.one);
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
		}
		else {
			Vector3 cs = body.transform.localScale;
			cs -= Vector3.one * Time.deltaTime;
			if (cs.x < 0.1f) Destroy(gameObject);
			else body.transform.localScale = cs;
		}
	}

	public void SetLandingPlace(SurfaceBlock block, SurfaceBlock block2, SurfaceBlock block3) {
		landingPlace = block;
		s_place1 = block2;
		s_place2 = block3;
		landing = true;
	} 
}

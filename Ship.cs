using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShipType {Passenger, Cargo, Private, Military}

public class Ship : MonoBehaviour {
	[SerializeField]
	byte _level = 1; // for editor set
	public byte level {get;private set;}
	ShipType _type;
	public ShipType type{get;private set;}
	const float DISTANCE_TO_ISLAND = 40;
	[SerializeField]
	float width = 0.5f, startSpeed = 10, acceleration = 1, _volume = 50;
	public int volume{get; private set;}
	bool xAxisMoving = false, docked = false, unloaded = false;
	float destination_pos = 0, speed = 0;
	Dock destination;

	void Awake() {
		level = _level;
		type = _type;
	}

	public void SetDestination(Dock d) {
		ChunkPos cpos = d.basement.pos;
		if (cpos.z == 0 || cpos.z == (Chunk.CHUNK_SIZE - 1)) {
			xAxisMoving = true;
			float zpos = (cpos.z ==0 ? -1 * width : Chunk.CHUNK_SIZE * Block.QUAD_SIZE + width);
			if (Random.value > 0.5f) {
				transform.position = new Vector3(Chunk.CHUNK_SIZE * Block.QUAD_SIZE + DISTANCE_TO_ISLAND, d.basement.pos.y * Block.QUAD_SIZE, zpos);
				transform.forward = Vector3.left;
			}
			else {
				transform.position = new Vector3(-1 * DISTANCE_TO_ISLAND, d.basement.pos.y * Block.QUAD_SIZE, zpos);
				transform.forward = Vector3.right;
			}
			destination_pos = cpos.x;
		}
		else {
			if ( cpos.x == 0 || cpos.x == (Chunk.CHUNK_SIZE - 1)) {
				xAxisMoving = false;
				float xpos = (cpos.x ==0 ? -1 * width : Chunk.CHUNK_SIZE * Block.QUAD_SIZE + width);
				if (Random.value > 0.5f) {
					transform.position = new Vector3(xpos, d.basement.pos.y * Block.QUAD_SIZE, Chunk.CHUNK_SIZE * Block.QUAD_SIZE + DISTANCE_TO_ISLAND);
					transform.forward = Vector3.back;
				}
				else {
					transform.position = new Vector3(xpos, d.basement.pos.y * Block.QUAD_SIZE, -1 * DISTANCE_TO_ISLAND);
					transform.forward = Vector3.forward;
				}
				destination_pos = cpos.z;
			} 
			else print ("error : incorrect dock position");
		}
		speed = startSpeed;
		docked = false; unloaded = false;
		destination = d; destination.maintainingShip = true;
	}

	void Update() {
		if (destination == null || GameMaster.gameSpeed == 0 || docked) return;
		float dist = 0;
		if (xAxisMoving) dist = Mathf.Abs(transform.position.x - destination_pos);
		else dist = Mathf.Abs(transform.position.z - destination_pos);
		if (dist <= 0.1f) {
			if ( !unloaded) { // еще не разгрузился
				if (destination != null && destination.isActive) {
						docked = true;
						destination.StartCoroutine(destination.ShipLoading(this));
				}
				else Undock();
			}
			else { //уходит
				docked = false; unloaded = false; 
				destination.maintainingShip = false; destination = null;
				PoolMaster.current.ReturnShipToPool(this);
			}
		}
		if (speed * speed / 2 / acceleration > dist) speed -= acceleration * Time.deltaTime * GameMaster.gameSpeed;
		else speed += acceleration * Time.deltaTime * GameMaster.gameSpeed;
		if (speed != 0) transform.Translate(Vector3.forward * speed * Time.deltaTime * GameMaster.gameSpeed, Space.Self);
	}

	public void Undock() {
		docked = false;
		if (xAxisMoving) destination_pos = (transform.position + transform.forward * 2 * DISTANCE_TO_ISLAND).x;
		else destination_pos = (transform.position + transform.forward * 2 * DISTANCE_TO_ISLAND).z;
		unloaded = true;
	}
}

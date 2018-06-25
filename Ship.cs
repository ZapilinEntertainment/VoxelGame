using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShipType {Passenger, Cargo, Private, Military}

[System.Serializable]
public class ShipSerializer {
	public float xpos, ypos,zpos,xrot,yrot,zrot,wrot;
	public bool xAxisMoving, docked, unloaded;
	public float destinationPos, speed;
	public byte level;
	public ShipType type;
}

public class Ship : MonoBehaviour {
	[SerializeField]
	byte _level = 1; 
	public byte level {get;private set;} // fixed by asset
	[SerializeField]
	ShipType _type;
	public ShipType type{get;private set;} // fixed by asset
	const float DISTANCE_TO_ISLAND = 40;
	[SerializeField]
	float width = 0.5f, startSpeed = 10, acceleration = 1; // fixed by asset
	[SerializeField]
	int _volume = 50;
	public int volume{get; private set;} // fixed by asset
	bool xAxisMoving = false, docked = false, unloaded = false;
	float destination_pos = 0, speed = 0;
	Dock destination;

	void Awake() {
		level = _level;
		type = _type;
		type = _type;
		volume = _volume;
	}

	public void SetDestination(Dock d) {
		ChunkPos cpos = d.basement.pos;
		if (cpos.z == 0 || cpos.z == (Chunk.CHUNK_SIZE - 1)) {
			xAxisMoving = true;
			float zpos = (cpos.z ==0 ? -1 * width - Block.QUAD_SIZE/2f  : (Chunk.CHUNK_SIZE -1 ) * Block.QUAD_SIZE + width);
			if (Random.value > 0.5f) {
				transform.position = new Vector3(Chunk.CHUNK_SIZE * Block.QUAD_SIZE + DISTANCE_TO_ISLAND, (d.basement.pos.y - 1) * Block.QUAD_SIZE, zpos);
				transform.forward = Vector3.left;
			}
			else {
				transform.position = new Vector3(-1 * DISTANCE_TO_ISLAND, (d.basement.pos.y - 1) * Block.QUAD_SIZE, zpos);
				transform.forward = Vector3.right;
			}
			destination_pos = cpos.x + (transform.forward * Block.QUAD_SIZE * 0.5f).x;
		}
		else {
			if ( cpos.x == 0 || cpos.x == (Chunk.CHUNK_SIZE - 1)) {
				xAxisMoving = false;
				float xpos = (cpos.x ==0 ? -1 * width - Block.QUAD_SIZE / 2f : ( Chunk.CHUNK_SIZE - 1 ) * Block.QUAD_SIZE + width);
				if (Random.value > 0.5f) {
					transform.position = new Vector3(xpos, (d.basement.pos.y - 1)* Block.QUAD_SIZE, Chunk.CHUNK_SIZE * Block.QUAD_SIZE + DISTANCE_TO_ISLAND);
					transform.forward = Vector3.back;
				}
				else {
					transform.position = new Vector3(xpos, (d.basement.pos.y - 1) * Block.QUAD_SIZE, -1 * DISTANCE_TO_ISLAND);
					transform.forward = Vector3.forward;
				}
				destination_pos = cpos.z + (transform.forward * Block.QUAD_SIZE * 0.5f).z;
			} 
			else print ("error : incorrect dock position");
		}
		speed = startSpeed;
		docked = false; unloaded = false;
		destination = d; 
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
						destination.ShipLoading(this);
				}
				else Undock();
			}
			else { //уходит
				docked = false; unloaded = false; 
				destination = null;
				PoolMaster.current.ReturnShipToPool(this);
			}
		}
		if (speed * speed / 2 / acceleration > dist) speed -= acceleration * Time.deltaTime * GameMaster.gameSpeed;
		else {
			if (speed < 100) speed += acceleration * Time.deltaTime * GameMaster.gameSpeed;
		}
		if (speed != 0) {
			float step = speed * Time.deltaTime * GameMaster.gameSpeed; // cause of time acceleration errors
			if (step > dist) step = dist;
			transform.Translate(Vector3.forward * step, Space.Self);
			if (Vector3.Distance(transform.position, GameMaster.mainChunk.transform.position) > 2000) PoolMaster.current.ReturnShipToPool(this);
		}
	}

	#region save-load system
	public ShipSerializer GetShipSerializer() {
		ShipSerializer ss = new ShipSerializer();
		ss.destinationPos = destination_pos;
		ss.docked = docked;
		ss.xpos = transform.position.x; ss.ypos = transform.position.y; ss.zpos = transform.position.z; 
		ss.xrot = transform.rotation.x;ss.yrot = transform.rotation.y;ss.zrot = transform.rotation.z;ss.wrot = transform.rotation.w;
		ss.speed = speed;
		ss.unloaded = unloaded;
		ss.xAxisMoving = xAxisMoving;
		ss.level = level;
		ss.type = type;
		return ss;
	}

	public void Load(ShipSerializer ss, Dock d) {
		destination = d;
		destination_pos = ss.destinationPos;
		docked = ss.docked;
		transform.position = new Vector3(ss.xpos, ss.ypos,ss.zpos);
		transform.rotation = new Quaternion(ss.xrot, ss.yrot, ss.zrot,ss.wrot);
		speed= ss.speed;
		unloaded = ss.unloaded;
		xAxisMoving = ss.xAxisMoving;
		level = ss.level;
		type = ss.type;
	}
	#endregion

	public void Undock() {
		docked = false;
		if (xAxisMoving) destination_pos = (transform.position + transform.forward * 2 * DISTANCE_TO_ISLAND).x;
		else destination_pos = (transform.position + transform.forward * 2 * DISTANCE_TO_ISLAND).z;
		unloaded = true;
	}
}

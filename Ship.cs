using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShipType {Passenger, Cargo, Private, Military}

[System.Serializable]
public class ShipSerializer {
	public float xpos, ypos,zpos,xrot,yrot,zrot,wrot;
	public bool xAxisMoving, docked, unloaded;
	public float speed;
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
	[SerializeField] float width = 0.5f,  acceleration = 1; // fixed by asset
	[SerializeField]
	int _volume = 50;
	public int volume{get; private set;} // fixed by asset
	bool xAxisMoving = false, docked = false, unloaded = false;
	float speed = 0;
    private const float START_SPEED = 10;
	Dock destination;

	void Awake() {
		level = _level;
		type = _type;
		type = _type;
		volume = _volume;
	}

	public void SetDestination(Dock d) {
		ChunkPos cpos = d.basement.pos;
        switch (d.modelRotation)
        {
            case 0:
                if (Random.value > 0.5f)
                {
                    transform.position = new Vector3(0 - DISTANCE_TO_ISLAND, d.transform.position.y, (cpos.z + 1) * Block.QUAD_SIZE + Dock.SMALL_SHIPS_PATH_WIDTH / 2f);
                    transform.forward = Vector3.right;
                }
                else
                {
                    transform.position = new Vector3(Chunk.CHUNK_SIZE * Block.QUAD_SIZE + DISTANCE_TO_ISLAND, d.transform.position.y, (cpos.z + 1) * Block.QUAD_SIZE + Dock.SMALL_SHIPS_PATH_WIDTH / 2f);
                    transform.forward = Vector3.left;
                }
                xAxisMoving = true;
                break;
            case 2:
                if (Random.value > 0.5f)
                {
                    transform.position = new Vector3((cpos.x + 1) * Block.QUAD_SIZE + Dock.SMALL_SHIPS_PATH_WIDTH / 2f, d.transform.position.y, 0 - DISTANCE_TO_ISLAND);
                    transform.forward = Vector3.forward;
                }
                else
                {
                    transform.position = new Vector3((cpos.x + 1) * Block.QUAD_SIZE + Dock.SMALL_SHIPS_PATH_WIDTH / 2f, d.transform.position.y, Chunk.CHUNK_SIZE * Block.QUAD_SIZE + DISTANCE_TO_ISLAND);
                    transform.forward = Vector3.back;
                }
                xAxisMoving = false;
                break;
            case 4:
                if (Random.value > 0.5f)
                {
                    transform.position = new Vector3(0 - DISTANCE_TO_ISLAND, d.transform.position.y, cpos.z * Block.QUAD_SIZE - Dock.SMALL_SHIPS_PATH_WIDTH / 2f);
                    transform.forward = Vector3.right;
                }
                else
                {
                    transform.position = new Vector3(Chunk.CHUNK_SIZE * Block.QUAD_SIZE + DISTANCE_TO_ISLAND, d.transform.position.y, cpos.z * Block.QUAD_SIZE - Dock.SMALL_SHIPS_PATH_WIDTH / 2f);
                    transform.forward = Vector3.left;
                }
                xAxisMoving = true;
                break;
            case 6:
                if (Random.value > 0.5f)
                {
                    transform.position = new Vector3(cpos.x * Block.QUAD_SIZE + Dock.SMALL_SHIPS_PATH_WIDTH / 2f, d.transform.position.y, 0 - DISTANCE_TO_ISLAND);
                    transform.forward = Vector3.forward;
                }
                else
                {
                    transform.position = new Vector3(cpos.x * Block.QUAD_SIZE + Dock.SMALL_SHIPS_PATH_WIDTH / 2f, d.transform.position.y, Chunk.CHUNK_SIZE * Block.QUAD_SIZE + DISTANCE_TO_ISLAND);
                    transform.forward = Vector3.back;
                }
                xAxisMoving = false;
                break;
        }

		speed = START_SPEED;
		docked = false; unloaded = false;
		destination = d; 
	}

	void Update() {
		if (GameMaster.gameSpeed == 0 | docked) return;		
        if (destination != null)
        {
            float dist = 0;
            if (xAxisMoving) dist = Mathf.Abs(transform.position.x - destination.transform.position.x);
            else dist = Mathf.Abs(transform.position.z - destination.transform.position.z);
            if (dist <= 0.1f)
            {
                if (!unloaded)
                { // еще не разгрузился
                    if (destination != null && destination.isActive)
                    {
                        docked = true;
                        destination.ShipLoading(this);
                    }
                    else Undock();
                }
                else
                { //уходит
                    docked = false; unloaded = false;
                    destination = null;
                    PoolMaster.current.ReturnShipToPool(this);
                }
            }
            if (speed * speed / 2 / acceleration > dist) speed -= acceleration * Time.deltaTime * GameMaster.gameSpeed;
            else
            {
                if (speed < 100) speed += acceleration * Time.deltaTime * GameMaster.gameSpeed;
            }
        }
        else
        {
            speed += acceleration * Time.deltaTime * GameMaster.gameSpeed;
        }
		
		if (speed != 0) {
			transform.Translate(Vector3.forward * speed * GameMaster.gameSpeed * Time.deltaTime, Space.Self);
			if (Vector3.Distance(transform.position, GameMaster.sceneCenter) >= 500) PoolMaster.current.ReturnShipToPool(this);
		}
	}

	#region save-load system
	public ShipSerializer GetShipSerializer() {
		ShipSerializer ss = new ShipSerializer();
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
		unloaded = true;
        destination = null;
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShipType : byte {Passenger, Cargo, Private, Military}
// при изменении заменить конвертер в сериализаторе

public sealed class Ship : MonoBehaviour {
#pragma warning disable 0649
    [SerializeField] private byte _level = 1;
    [SerializeField] private float acceleration = 1, halfLength = 1; // fixed by asset
    [SerializeField] private int _volume = 50;
    [SerializeField] private ShipType _type;
#pragma warning restore 0649

    public byte level {get;private set;} // fixed by asset	
	public ShipType type{get;private set;} // fixed by asset
	
	public int volume{get; private set;} // fixed by asset
	private bool xAxisMoving = false, docked = false, unloaded = false, dpointSet = false;
	private float speed = 0, awaitingTimer = 0;    
	private Dock destination;
    private Vector3 exitDirection;

    private const float START_SPEED = 10, DISTANCE_TO_ISLAND = 80, AWAITING_TIME = 90;

    void Awake() {
		level = _level;
		type = _type;
		volume = _volume;
	}

	public void SetDestination(Dock d) {
        awaitingTimer = 0;
		ChunkPos cpos = d.GetBlockPosition();
        float width = Dock.SMALL_SHIPS_PATH_WIDTH;
        if (level > 1)
        {
            if (level == 2) width = Dock.MEDIUM_SHIPS_PATH_WIDTH;
            else width = Dock.HEAVY_SHIPS_PATH_WIDTH;
        }
        Vector3 pos, fwd;
        RaycastHit rh;
        bool reverseDirection = false;
        switch (d.modelRotation)
        {
            case 0:
                pos = new Vector3(0 - DISTANCE_TO_ISLAND, d.transform.position.y, d.transform.position.z + 0.5f * Block.QUAD_SIZE + width / 2f);
                fwd = Vector3.right;
                if (Physics.Raycast(pos, fwd, out rh, 2 * DISTANCE_TO_ISLAND))
                {
                    Ship s = rh.collider.GetComponent<Ship>();
                    if (s != null)
                    {
                        if (s.transform.forward == Vector3.left) reverseDirection = true;
                    }
                    else
                    {
                        PoolMaster.current.ReturnShipToPool(this);
                        return;
                    }
                }
                else
                {
                    if (Random.value > 0.5f) reverseDirection = true;
                }
                if (reverseDirection)
                {
                    pos = new Vector3(Chunk.chunkSize * Block.QUAD_SIZE + DISTANCE_TO_ISLAND, d.transform.position.y, d.transform.position.z + 0.5f * Block.QUAD_SIZE + width / 2f);
                    fwd = Vector3.left;
                }
                xAxisMoving = true;
                break;
            case 2:
                pos = new Vector3(d.transform.position.x + 0.5f * Block.QUAD_SIZE + width / 2f, d.transform.position.y, 0 - DISTANCE_TO_ISLAND);
                fwd = Vector3.forward;
                if (Physics.Raycast(pos, fwd, out rh, 2 * DISTANCE_TO_ISLAND))
                {
                    Ship s = rh.collider.GetComponent<Ship>();
                    if (s != null)
                    {
                        if (s.transform.forward == Vector3.back) reverseDirection = true;
                    }
                    else
                    {
                        PoolMaster.current.ReturnShipToPool(this);
                        return;
                    }
                }
                else
                {
                    if (Random.value > 0.5f) reverseDirection = true;
                }
                if (reverseDirection)
                {
                    transform.position = new Vector3(d.transform.position.x + 0.5f * Block.QUAD_SIZE + width / 2f, d.transform.position.y, Chunk.chunkSize * Block.QUAD_SIZE + DISTANCE_TO_ISLAND);
                    transform.forward = Vector3.back;
                }
                xAxisMoving = false;
                break;
            case 4:
                pos = new Vector3(0 - DISTANCE_TO_ISLAND, d.transform.position.y, d.transform.position.z - 0.5f * Block.QUAD_SIZE - width / 2f);
                fwd = Vector3.right;
                if (Physics.Raycast(pos, fwd, out rh, 2 * DISTANCE_TO_ISLAND))
                {
                    Ship s = rh.collider.GetComponent<Ship>();
                    if (s != null)
                    {
                        if (s.transform.forward == Vector3.left) reverseDirection = true;
                    }
                    else
                    {
                        PoolMaster.current.ReturnShipToPool(this);
                        return;
                    }
                }
                else
                {
                    if (Random.value > 0.5f) reverseDirection = true;
                }
                if (reverseDirection)
                {
                    pos = new Vector3(Chunk.chunkSize * Block.QUAD_SIZE + DISTANCE_TO_ISLAND, d.transform.position.y, d.transform.position.z - 0.5f * Block.QUAD_SIZE - width / 2f);
                    fwd = Vector3.left;
                }
                xAxisMoving = true;
                break;            
            case 6:
            default:
                pos = new Vector3(d.transform.position.x - 0.5f * Block.QUAD_SIZE - width / 2f, d.transform.position.y, 0 - DISTANCE_TO_ISLAND);
                fwd = Vector3.forward;
                if (Physics.Raycast(pos, fwd, out rh, 2 * DISTANCE_TO_ISLAND))
                {
                    Ship s = rh.collider.GetComponent<Ship>();
                    if (s != null)
                    {
                        if (s.transform.forward == Vector3.back) reverseDirection = true;
                    }
                    else
                    {
                        PoolMaster.current.ReturnShipToPool(this);
                        return;
                    }
                }
                else
                {
                    if (Random.value > 0.5f) reverseDirection = true;
                }
                if (reverseDirection)
                {
                    transform.position = new Vector3(d.transform.position.x - 0.5f * Block.QUAD_SIZE - width / 2f, d.transform.position.y, Chunk.chunkSize * Block.QUAD_SIZE + DISTANCE_TO_ISLAND);
                    transform.forward = Vector3.back;
                }
                xAxisMoving = false;
                break;
        }
        transform.position = pos;
        transform.forward = fwd;

		speed = START_SPEED;
		docked = false; unloaded = false;
		destination = d; 
	}

	void Update() { 
		if (GameMaster.gameSpeed == 0 | docked) return;
        float breakLength = speed * speed / acceleration / 2f, t = Time.deltaTime * GameMaster.gameSpeed;
        RaycastHit rh;
        bool breaking = false;
        breaking = Physics.Raycast(transform.position + transform.forward * halfLength, transform.forward, out rh, breakLength + halfLength);
        if (breaking)
        {
            if (awaitingTimer == 0) awaitingTimer = AWAITING_TIME;
            awaitingTimer -= t;
            if (awaitingTimer <= 0)
            {
                PoolMaster.current.ReturnShipToPool(this);
                return;
            }
        }
        else awaitingTimer = 0;
        if (destination != null)
        {
            float dist = 0;
            if (xAxisMoving) dist = Mathf.Abs(transform.position.x - destination.transform.position.x);
            else dist = Mathf.Abs(transform.position.z - destination.transform.position.z);            

            if (dist <= speed * Time.deltaTime * GameMaster.gameSpeed)
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
                }
            }
            if (breakLength >= dist | breaking) speed = Mathf.MoveTowards(speed, 0, acceleration * 10 * Time.deltaTime * GameMaster.gameSpeed);
            else
            {
                if (speed != START_SPEED) speed = Mathf.MoveTowards(speed, START_SPEED, acceleration * Time.deltaTime * GameMaster.gameSpeed);
            }
        }
        else
        {
            if (!breaking)
            {
                speed += acceleration * Time.deltaTime * GameMaster.gameSpeed;
                if (dpointSet) transform.forward = Vector3.RotateTowards(transform.forward, exitDirection, acceleration / 4f * t, 1);
                else
                {
                    if ((GameMaster.sceneCenter - transform.position).magnitude > Chunk.chunkSize * Block.QUAD_SIZE * 1.5f / 2f)
                    {
                        exitDirection = Quaternion.AngleAxis((0.5f - Random.value) * 180, Vector3.up) * transform.forward;
                        exitDirection += Vector3.up * (0.5f - Random.value) / 10f;
                        dpointSet = true;
                    }
                }
            }
            else
            {
                if (speed > 0) speed = Mathf.MoveTowards(speed, 0, acceleration * Time.deltaTime * GameMaster.gameSpeed);                
            }
        }
		
		if (speed != 0) {
			transform.Translate(Vector3.forward * speed * GameMaster.gameSpeed * Time.deltaTime, Space.Self);
			if (Vector3.Distance(transform.position, GameMaster.sceneCenter) > DISTANCE_TO_ISLAND * 2 + Chunk.chunkSize * 1.5f * Block.QUAD_SIZE) PoolMaster.current.ReturnShipToPool(this);
		}
	}

	public void Undock() {
		docked = false;
		unloaded = true;
        destination = null;
        if ((GameMaster.sceneCenter - transform.position).magnitude > Chunk.chunkSize * Block.QUAD_SIZE * 1.5f / 2f)
        {
            exitDirection = Quaternion.AngleAxis((0.5f - Random.value) * 180, Vector3.up) * transform.forward;
            exitDirection += Vector3.up * (0.5f - Random.value) / 10f;
            dpointSet = true;
        }
        else dpointSet = false;
    }

    #region save-load system
    public List<byte> Save()
    {
        byte zero = 0, one = 1;
        var data = new List<byte>() {
            level,              
            (byte)type,         
            docked ? one : zero 
        };
        Transform t = transform;
        data.AddRange(System.BitConverter.GetBytes(t.position.x)); // 0 -3
        data.AddRange(System.BitConverter.GetBytes(t.position.y)); // 4 - 7
        data.AddRange(System.BitConverter.GetBytes(t.position.z)); // 8- 11

        data.AddRange(System.BitConverter.GetBytes(t.rotation.x)); // 12 - 15
        data.AddRange(System.BitConverter.GetBytes(t.rotation.y)); // 16 - 19
        data.AddRange(System.BitConverter.GetBytes(t.rotation.z)); // 20- 23
        data.AddRange(System.BitConverter.GetBytes(t.rotation.w)); // 24 - 27

        data.AddRange(System.BitConverter.GetBytes(speed));        // 28 - 31
        data.Add(unloaded ? one : zero);    // 32
        data.Add(xAxisMoving ? one : zero); // 33
        return data;
    }

    public static Ship Load(System.IO.FileStream fs, Dock d)
    {
        byte slevel = (byte)fs.ReadByte();
        ShipType stype = (ShipType)fs.ReadByte();
        Ship s = PoolMaster.current.GetShip(slevel, stype);
        s.level = slevel;
        s.type = stype;
        s.destination = d;
        bool isDocked = fs.ReadByte() == 1;
        if (s.destination != null && !s.destination.IsDestroyed()) s.docked = isDocked;
        else s.docked = false;
        var data = new byte[34];
        fs.Read(data, 0, data.Length);
        s.transform.position = new Vector3(
            System.BitConverter.ToSingle(data,0),
            System.BitConverter.ToSingle(data, 4),
            System.BitConverter.ToSingle(data, 8)
            );
        s.transform.rotation = new Quaternion(
            System.BitConverter.ToSingle(data, 12),
            System.BitConverter.ToSingle(data, 16),
            System.BitConverter.ToSingle(data, 20),
            System.BitConverter.ToSingle(data, 24)
            );
        s.speed = System.BitConverter.ToSingle(data, 28);
        s.unloaded = data[32] == 1;
        s.xAxisMoving = data[33] == 1;
        return s;
    }
    #endregion
}

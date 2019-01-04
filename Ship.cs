using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShipType : byte {Passenger, Cargo, Private, Military}
// при изменении заменить конвертер в сериализаторе

public class Ship : MonoBehaviour {
#pragma warning disable 0649
    [SerializeField] byte _level = 1;
    [SerializeField] float width = 0.5f, acceleration = 1; // fixed by asset
    [SerializeField] int _volume = 50;
    [SerializeField] ShipType _type;
#pragma warning restore 0649

    public byte level {get;private set;} // fixed by asset	
	public ShipType type{get;private set;} // fixed by asset
	
	public int volume{get; private set;} // fixed by asset
	private bool xAxisMoving = false, docked = false, unloaded = false;
	private float speed = 0;    
	private Dock destination;

    private const float START_SPEED = 10, DISTANCE_TO_ISLAND = 40;
    public const int SERIALIZER_LENGTH = 37;

    void Awake() {
		level = _level;
		type = _type;
		volume = _volume;
	}

	public void SetDestination(Dock d) {
		ChunkPos cpos = d.basement.pos;
        float width = Dock.SMALL_SHIPS_PATH_WIDTH;
        if (level > 1)
        {
            if (level == 2) width = Dock.MEDIUM_SHIPS_PATH_WIDTH;
            else width = Dock.HEAVY_SHIPS_PATH_WIDTH;
        }
        switch (d.modelRotation)
        {
            case 0:
                if (Random.value > 0.5f)
                {
                    transform.position = new Vector3(0 - DISTANCE_TO_ISLAND, d.transform.position.y, d.transform.position.z + 0.5f* Block.QUAD_SIZE + width / 2f);
                    transform.forward = Vector3.right;
                }
                else
                {
                    transform.position = new Vector3(Chunk.CHUNK_SIZE * Block.QUAD_SIZE + DISTANCE_TO_ISLAND, d.transform.position.y, d.transform.position.z + 0.5f * Block.QUAD_SIZE + width / 2f);
                    transform.forward = Vector3.left;
                }
                xAxisMoving = true;
                break;
            case 2:
                if (Random.value > 0.5f)
                {
                    transform.position = new Vector3(d.transform.position.x + 0.5f * Block.QUAD_SIZE + width / 2f, d.transform.position.y, 0 - DISTANCE_TO_ISLAND);
                    transform.forward = Vector3.forward;
                }
                else
                {
                    transform.position = new Vector3(d.transform.position.x + 0.5f * Block.QUAD_SIZE + width / 2f, d.transform.position.y, Chunk.CHUNK_SIZE * Block.QUAD_SIZE + DISTANCE_TO_ISLAND);
                    transform.forward = Vector3.back;
                }
                xAxisMoving = false;
                break;
            case 4:
                if (Random.value > 0.5f)
                {
                    transform.position = new Vector3(0 - DISTANCE_TO_ISLAND, d.transform.position.y, d.transform.position.z - 0.5f * Block.QUAD_SIZE - width / 2f);
                    transform.forward = Vector3.right;
                }
                else
                {
                    transform.position = new Vector3(Chunk.CHUNK_SIZE * Block.QUAD_SIZE + DISTANCE_TO_ISLAND, d.transform.position.y, d.transform.position.z - 0.5f * Block.QUAD_SIZE - width / 2f);
                    transform.forward = Vector3.left;
                }
                xAxisMoving = true;
                break;
            case 6:
                if (Random.value > 0.5f)
                {
                    transform.position = new Vector3(d.transform.position.x - 0.5f * Block.QUAD_SIZE - width / 2f, d.transform.position.y, 0 - DISTANCE_TO_ISLAND);
                    transform.forward = Vector3.forward;
                }
                else
                {
                    transform.position = new Vector3(d.transform.position.x - 0.5f * Block.QUAD_SIZE - width / 2f, d.transform.position.y, Chunk.CHUNK_SIZE * Block.QUAD_SIZE + DISTANCE_TO_ISLAND);
                    transform.forward = Vector3.back;
                }
                xAxisMoving = false;
                break;
        }

		speed = START_SPEED;
		docked = false; unloaded = false;
		destination = d; 
	}

	void Update() { // добавить рейкаст на препятствия ?
		if (GameMaster.gameSpeed == 0 | docked) return;		
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
            if (speed * speed / 2 / acceleration >= dist) speed -= acceleration * Time.deltaTime * GameMaster.gameSpeed;
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

	public void Undock() {
		docked = false;
		unloaded = true;
        destination = null;
	}

    #region save-load system
    public List<byte> GetShipSerializer()
    {
        byte zero = 0, one = 1;
        var data = new List<byte>() {
            level,              // 0
            (byte)type,         // 1
            docked ? one : zero //2
        };
        Transform t = transform;
        data.AddRange(System.BitConverter.GetBytes(t.position.x)); // 3 - 6
        data.AddRange(System.BitConverter.GetBytes(t.position.y)); // 7 - 10
        data.AddRange(System.BitConverter.GetBytes(t.position.z)); // 11 - 14

        data.AddRange(System.BitConverter.GetBytes(t.rotation.x)); // 15 - 18
        data.AddRange(System.BitConverter.GetBytes(t.rotation.y)); // 19 - 22
        data.AddRange(System.BitConverter.GetBytes(t.rotation.z)); // 23 - 26
        data.AddRange(System.BitConverter.GetBytes(t.rotation.w)); // 27 - 30

        data.AddRange(System.BitConverter.GetBytes(speed));        // 31 - 34
        data.Add(unloaded ? one : zero);    // 35
        data.Add(xAxisMoving ? one : zero); // 36
        //SERIALIZER_LENGTH = 37
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
        s.docked = fs.ReadByte() == 1;
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

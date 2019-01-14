using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkConsumingEffect : MonoBehaviour {
    private struct FlyingBlock
    {
        public GameObject cube;
        public bool goingUp;
        public FlyingBlock(GameObject i_go, bool i_goingUp)
        {
            cube = i_go;
            goingUp = i_goingUp;
        }
    }

    private float lsConsumingSpeed = 1, realConsumingSpeed = 1;
    private float lsTimer = 1, realTimer = 1;
    private Chunk chunk;
    private List<GameObject> cubesPool = new List<GameObject>();
    private List<FlyingBlock> activeCubes = new List<FlyingBlock>();

    private const float FLY_SPEED = 8, ROTATION_SPEED = 5;

    private void Awake()
    {
        chunk = gameObject.GetComponent<Chunk>();
    }

    public void SetSettings(byte upSkyStatus, byte lowSkyStatus)
    {
        lsConsumingSpeed = upSkyStatus;
        realConsumingSpeed = lowSkyStatus;
    }

    public void Update()
    {
        if (chunk == null) return;
        float t = Time.deltaTime * GameMaster.gameSpeed;
        lsTimer -= t * lsConsumingSpeed;
        if (lsTimer <= 0)
        {
            SpawnCube_UpToDown();
            lsTimer = GameConstants.WORLD_CONSUMING_TIMER;
        }
        realTimer -= t * realConsumingSpeed;
        if (realTimer <= 0)
        {
            SpawnCube_BottomToUp();
            realTimer = GameConstants.WORLD_CONSUMING_TIMER;
        }

        if (activeCubes.Count > 0)
        {
            int i = 0;
            FlyingBlock fb;
            Transform tf;            
            float fspeed = FLY_SPEED * t;
            Vector3 one = Vector3.one * ROTATION_SPEED * t, up = Vector3.up * fspeed * t, down = Vector3.down * fspeed * t;
            float upBorder = Chunk.CHUNK_SIZE * 2, lowBorder = Chunk.CHUNK_SIZE * (-1);

            while ( i < activeCubes.Count)
            {
                fb = activeCubes[i];
                tf = fb.cube.transform;
                if (fb.goingUp)
                {
                    if (tf.position.y > upBorder)
                    {
                        fb.cube.SetActive(false);
                        fb.cube.transform.parent = transform;
                        cubesPool.Add(fb.cube);                        
                        activeCubes.RemoveAt(i);
                        continue;
                    }
                    else
                    {
                        tf.position += up;
                    }
                }
                else
                {
                    if (tf.position.y < lowBorder)
                    {
                        fb.cube.SetActive(false);
                        fb.cube.transform.parent = transform;
                        cubesPool.Add(fb.cube);
                        activeCubes.RemoveAt(i);
                        continue;
                    }
                    else
                    {
                        tf.position += down; 
                    }
                }

                tf.Rotate(one);
                i++;
            }
        }
    }

    private void SpawnCube_UpToDown()
    {
        int len = Chunk.CHUNK_SIZE;
        Block[,,] blocks = chunk.blocks;
        for (int y = len - 1; y > -1; y--)
        {
            for (int x = 0; x < len; x++)
            {
                for (int z = 0; z< len;z++)
                {
                    Block b = blocks[x, y, z];
                    if (b == null) continue;
                    else
                    {
                        if (b.type == BlockType.Cube)
                        {
                            SurfaceBlock sb = chunk.GetBlock(x, y + 1, z) as SurfaceBlock;
                            if (sb != null && sb.artificialStructures != 0) continue;
                            Vector3 pos = b.transform.position;
                            chunk.DeleteBlock(new ChunkPos(x, y, z));
                            SpawnEffectCube(pos, true);
                            return;
                        }
                    }
                }
            }
        }
    }
    private void SpawnCube_BottomToUp()
    {
        int len = Chunk.CHUNK_SIZE;
        Block[,,] blocks = chunk.blocks;
        for (int y = 0; y < len; y++)
        {
            for (int x = 0; x < len; x++)
            {
                for (int z = 0; z < len; z++)
                {
                    Block b = blocks[x, y, z];
                    if (b == null) continue;
                    else
                    {
                        if (b.type == BlockType.Cube)
                        {
                            Vector3 pos = b.transform.position;
                            chunk.DeleteBlock(new ChunkPos(x, y, z));
                            SpawnEffectCube(pos, false);
                            return;
                        }
                    }
                }
            }
        }
    }
    private void SpawnEffectCube(Vector3 position, bool flyUp)
    {

        GameObject g = null;
        if (cubesPool.Count > 0)
        {
            g = cubesPool[0];
            cubesPool.RemoveAt(0);
            g.transform.parent = null;
        }
        else  g = Instantiate(Resources.Load<GameObject>("Prefs/zoneCube"));
        g.GetComponent<MeshRenderer>().sharedMaterial = flyUp ? PoolMaster.energy_material : PoolMaster.darkness_material;
        g.transform.position = position;
        g.transform.rotation = Quaternion.identity;
        activeCubes.Add(new FlyingBlock(g, flyUp));
        g.SetActive(true);
    }
}

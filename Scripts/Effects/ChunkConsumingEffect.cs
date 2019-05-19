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

    private const float FLY_SPEED = 16, ROTATION_SPEED = 5, CONSUMING_MULTIPLIER = 3;

    private void Awake()
    {
        chunk = gameObject.GetComponent<Chunk>();
    }

    public void SetSettings(byte upSkyStatus, byte lowSkyStatus)
    {
        lsConsumingSpeed = upSkyStatus * CONSUMING_MULTIPLIER;
        realConsumingSpeed = lowSkyStatus * CONSUMING_MULTIPLIER;
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
            float upBorder = GameConstants.GetUpperBorder(), lowBorder = GameConstants.GetBottomBorder();

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
        var blocks = chunk.blocks;
        Block b;
        foreach (var bd in blocks)
        {
            b = bd.Value;
            if (b.type == BlockType.Cube)
            {
                SurfaceBlock sb = chunk.GetBlock(b.pos.x , b.pos.y + 1, b.pos.z) as SurfaceBlock;
                if (sb != null && sb.artificialStructures != 0) continue;
                chunk.DeleteBlock(b.pos);
                SpawnEffectCube(b.pos.ToWorldSpace(), true);
                return;
            }
        }
        GameMaster.realMaster.GameOver(GameEndingType.ConsumedByReal);
    }
private void SpawnCube_BottomToUp()
    {        
        var blocks = chunk.blocks;
        Block b;
        foreach (var bd in blocks)
        {
            b = bd.Value;
            if (b.type == BlockType.Cube)
            {
                chunk.DeleteBlock(b.pos);
                SpawnEffectCube(b.pos.ToWorldSpace(), false);
                return;
            }
        }
        GameMaster.realMaster.GameOver(GameEndingType.ConsumedByLastSector);
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
        g.GetComponent<MeshRenderer>().sharedMaterial = flyUp ? PoolMaster.energyMaterial : PoolMaster.darkness_material;
        g.transform.position = position;
        g.transform.rotation = Quaternion.identity;
        activeCubes.Add(new FlyingBlock(g, flyUp));
        g.SetActive(true);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PixelPosByte
{
    public byte x, y;
    public bool exists;
    public static readonly PixelPosByte Empty, zero, one;
    public PixelPosByte(byte xpos, byte ypos) { x = xpos; y = ypos; exists = true; }
    public PixelPosByte(int xpos, int ypos)
    {
        if (xpos < 0) xpos = 0; if (ypos < 0) ypos = 0;
        x = (byte)xpos; y = (byte)ypos;
        exists = true;
    }
    static PixelPosByte()
    {
        Empty = new PixelPosByte(0, 0); Empty.exists = false;
        zero = new PixelPosByte(0, 0); // but exists
        one = new PixelPosByte(1, 1);
    }

    public static bool operator ==(PixelPosByte lhs, PixelPosByte rhs) { return lhs.Equals(rhs); }
    public static bool operator !=(PixelPosByte lhs, PixelPosByte rhs) { return !(lhs.Equals(rhs)); }

    public override bool Equals(object obj)
    {
        // Check for null values and compare run-time types.
        if (obj == null || GetType() != obj.GetType())
            return false;

        PixelPosByte p = (PixelPosByte)obj;
        return (x == p.x) && (y == p.y) && (exists == p.exists);
    }

    public override int GetHashCode()
    {
        if (exists) return x * y;
        else return x * y * (-1);
    }
}

[System.Serializable]
public class GrasslandSerializer
{
    public float progress = 0, lifepower = 0;
    public byte prevStage = 0;
}

public class Grassland : MonoBehaviour
{
    public const float LIFEPOWER_TO_PREPARE = 16, LIFE_CREATION_TIMER = 22;
    const byte MAX_PLANTS_COUNT = 8;

    public SurfaceBlock myBlock { get; private set; }
    float progress = 0;
    public float lifepower;
    byte prevStage = 0;
    private float plantCreateCooldown = 0;
    bool destroyed = false;

    static List<Grassland> grasslandList = new List<Grassland>(); 

    public static void ScriptReset()
    {
        grasslandList = new List<Grassland>();
    }

    public static int GrasslandUpdate(float tax)
    {
        if (grasslandList.Count == 0) return (int)tax;
        float returnVal = tax;
        if (tax != 0)
        {
            if (tax > 0)
            {
                int pos = (int)(Random.value * (grasslandList.Count - 1));
                int count = (int)(GameMaster.MAX_LIFEPOWER_TRANSFER * GameMaster.lifeGrowCoefficient);
                grasslandList[pos].AddLifepower(count);
                returnVal -= count;
            }
            else
            {
                tax *= -1;
                int i = 0;
                int tax2 = Mathf.RoundToInt(tax / grasslandList.Count);
                while (i < grasslandList.Count)
                {                    
                    returnVal += grasslandList[i].TakeLifepower(tax2);
                    i++;
                }
            }
        }
        List<Plant> plants = new List<Plant>();
        SurfaceBlock myBlock; Chunk c; float lifepower = 0;

        float lifepowerTick = GameMaster.LIFEPOWER_TICK;
        for  (var a = 0; a < grasslandList.Count; a++)
        {
            Grassland gl = grasslandList[a];
            myBlock = gl.myBlock;
            c = myBlock.myChunk;
            lifepower = gl.lifepower;
            foreach (Structure s in gl.myBlock.surfaceObjects)
            {
                if (s.id == Structure.PLANT_ID) plants.Add(s as Plant);
            }

            byte stage = gl.CheckGrasslandStage();
            if (lifepower > 2 * LIFEPOWER_TO_PREPARE )
            {                
                if (stage > 2 & plants.Count < MAX_PLANTS_COUNT )
                {
                    int i = 0;
                    while (lifepower > 2 * LIFEPOWER_TO_PREPARE & i < plants.Count)
                    {
                        Plant p = plants[i];
                        if (p.lifepower < p.lifepowerToGrow)
                        {
                            float donation = p.lifepowerToGrow - p.lifepower;
                            float mt = p.GetMaxLifeTransfer();
                            if (donation > mt) donation = mt;
                            if (donation > (lifepower - 2 * LIFEPOWER_TO_PREPARE)) donation = (lifepower - 2 * LIFEPOWER_TO_PREPARE);
                            lifepower -= donation;
                            p.AddLifepower((int)donation);
                        }
                        i++;
                    }
                    if (gl.plantCreateCooldown <= 0)
                    {
                        int cost = Plant.GetCreateCost(Plant.TREE_OAK_ID);
                        if (lifepower > (2 * LIFEPOWER_TO_PREPARE + cost) & myBlock.cellsStatus != 1)
                        {
                            PixelPosByte pos = myBlock.GetRandomCell();
                            if (pos != PixelPosByte.Empty)
                            {
                                Plant p = Plant.GetNewPlant(Plant.TREE_OAK_ID);
                                p.SetBasement(myBlock, pos);
                                gl.TakeLifepower(cost);
                                gl.plantCreateCooldown = 10f / lifepowerTick;
                                //MonoBehaviour.print("new plant created");
                            }
                        }
                    }
                    else gl.plantCreateCooldown -= lifepowerTick;
                }
            }
            else
            { // lifepower falls down
                if (lifepower < LIFEPOWER_TO_PREPARE & plants.Count > 0)
                {
                    float lifepowerNeeded = Mathf.Abs(lifepower) + LIFEPOWER_TO_PREPARE + 2;
                    int lifepowerFromSinglePlant = Mathf.RoundToInt(lifepowerNeeded / (float)plants.Count);
                    while (lifepower <= LIFEPOWER_TO_PREPARE & plants.Count > 0)
                    {
                        int i = (int)(Random.value * (plants.Count - 1));
                        lifepower += plants[i].TakeLifepower(lifepowerFromSinglePlant);
                        plants.RemoveAt(i);
                    }
                }
                gl.CheckGrasslandStage();
            }
            plants.Clear();
        }
        return (int)returnVal;
    }

    public static Grassland CreateOn(SurfaceBlock sblock)
    {
        if (sblock == null || sblock.grassland != null) return null;
        Grassland gl = sblock.gameObject.AddComponent<Grassland>();
        gl.myBlock = sblock;
        sblock.SetGrassland(gl);
        grasslandList.Add(gl);
        return gl;
    }

    byte CheckGrasslandStage() // удаление при недостаче - > изменение структуры списка grasslands
    {
        progress = Mathf.Clamp(lifepower / LIFEPOWER_TO_PREPARE, 0, 1);
        byte stage = (byte)Mathf.RoundToInt(progress / 0.2f);
        if (stage != prevStage)
        {
            if (Mathf.Abs(stage - prevStage) > 1)
            {
                if (stage > prevStage) stage = (byte)(prevStage + 1);
                else stage = (byte)(prevStage - 1);
            }
            SetGrassTexture(stage);
            prevStage = stage;
        }
        return stage;
    }

    public void AddLifepower(int count)
    {
        lifepower += count;
    }
    public int TakeLifepower(float count)
    {
        if (count < 0) return 0;
        if (lifepower < -25) count = 0;
        else lifepower -= count;
        int lifeTransfer = (int)count;
        return lifeTransfer;
    }
    public void SetLifepower(float count)
    {
        lifepower = count;
        progress = Mathf.Clamp(lifepower / LIFEPOWER_TO_PREPARE, 0, 1);
        byte stage = (byte)(Mathf.RoundToInt(progress / 0.2f));
        if (stage != prevStage)
        {
            SetGrassTexture(stage);
            prevStage = stage;
        }
    }
    void SetGrassTexture(byte stage)
    {
        switch (stage)
        {
            case 0:
                myBlock.surfaceRenderer.sharedMaterial = PoolMaster.GetBasicMaterial(BasicMaterial.Dirt, myBlock.surfaceRenderer.GetComponent<MeshFilter>(), myBlock.illumination);
                break;
            case 1:
                myBlock.surfaceRenderer.sharedMaterial = PoolMaster.GetGreenMaterial(GreenMaterial.Grass20, myBlock.surfaceRenderer.GetComponent<MeshFilter>(), myBlock.illumination);
                break;
            case 2:
                myBlock.surfaceRenderer.sharedMaterial = PoolMaster.GetGreenMaterial(GreenMaterial.Grass40, myBlock.surfaceRenderer.GetComponent<MeshFilter>(), myBlock.illumination);
                break;
            case 3:
                myBlock.surfaceRenderer.sharedMaterial = PoolMaster.GetGreenMaterial(GreenMaterial.Grass60, myBlock.surfaceRenderer.GetComponent<MeshFilter>(), myBlock.illumination);
                break;
            case 4:
                myBlock.surfaceRenderer.sharedMaterial = PoolMaster.GetGreenMaterial(GreenMaterial.Grass80, myBlock.surfaceRenderer.GetComponent<MeshFilter>(), myBlock.illumination);
                break;
            case 5:
                myBlock.surfaceRenderer.sharedMaterial = PoolMaster.GetGreenMaterial(GreenMaterial.Grass100, myBlock.surfaceRenderer.GetComponent<MeshFilter>(), myBlock.illumination);
                break;
        }
    }

    /// <summary>
    /// Use this only on pre-gen
    /// </summary>
    public void AddLifepowerAndCalculate(int count)
    {
        if (count > 2 * LIFEPOWER_TO_PREPARE)
        {
            lifepower = 2 * LIFEPOWER_TO_PREPARE;
            float freeEnergy = count - lifepower;
            int treesCount = (int)(Random.value * 10 + 4);
            int i = 0;
            List<PixelPosByte> positions = myBlock.GetRandomCells(treesCount);
            if (treesCount > positions.Count) treesCount = positions.Count;
            int lifepowerDosis = (int)(freeEnergy / treesCount);
            if (treesCount != 0)
            {
                while (i < treesCount & freeEnergy > 0 & myBlock.cellsStatus != 1)
                {
                    int plantID = Plant.TREE_OAK_ID;
                    int ld = (int)(lifepowerDosis * (0.3f + Random.value));
                    if (ld > freeEnergy) { lifepower += freeEnergy; break; }
                    byte maxStage = OakTree.MAX_STAGE;
                    float maxEnergy = OakTree.GetLifepowerLevelForStage(maxStage);
                    byte getStage = (byte)(ld / maxEnergy * maxStage);
                    if (getStage > maxStage) getStage = maxStage;
                    if (getStage == maxStage & Random.value > 0.7f) getStage--;

                    if (Random.value > 0.1f)
                    {
                        Plant p = Plant.GetNewPlant(plantID);
                        p.SetBasement(myBlock, positions[i]);
                        p.AddLifepower(ld);
                        p.SetStage(getStage);
                        freeEnergy -= (Plant.GetCreateCost(plantID) + ld);
                    }
                    else
                    {
                        HarvestableResource hr = Structure.GetStructureByID(Structure.CONTAINER_ID) as HarvestableResource;
                        hr.SetResources(ResourceType.Food, 10);
                        hr.SetBasement(myBlock, positions[i]);
                        freeEnergy -= 10;
                    }
                    
                    i++;
                }
            }
            lifepower = freeEnergy + 2 * LIFEPOWER_TO_PREPARE;
        }
        else lifepower = count;
        progress = Mathf.Clamp(lifepower / LIFEPOWER_TO_PREPARE, 0, 1);
        byte stage = (byte)(Mathf.RoundToInt(progress / 0.2f));
        prevStage = stage;
        SetGrassTexture(stage);        
    }

    public void Annihilation() { Annihilation(false); }
    public void Annihilation(bool forced)
    {
        if (destroyed) return;
        else destroyed = true;
        if (!forced) myBlock.myChunk.AddLifePower((int)lifepower);
        myBlock.surfaceRenderer.sharedMaterial = PoolMaster.GetBasicMaterial(BasicMaterial.Dirt, myBlock.surfaceRenderer.GetComponent<MeshFilter>(), myBlock.illumination);
        int i = grasslandList.IndexOf(this);
        if (i > 0)
        {
            grasslandList.RemoveAt(i);
        }
    }

    public GrasslandSerializer Save()
    {
        GrasslandSerializer gs = new GrasslandSerializer();
        gs.progress = progress;
        gs.lifepower = lifepower;
        gs.prevStage = prevStage;
        return gs;
    }

    public void Load(GrasslandSerializer gs)    {
        
        SetLifepower(gs.lifepower);
        prevStage = gs.prevStage;
        progress = gs.progress;
    }

    private void OnDestroy()
    {
        if (!destroyed & !GameMaster.applicationStopWorking) Annihilation(true);
    }
}

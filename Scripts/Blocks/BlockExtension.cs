using System.Collections.Generic;
using UnityEngine;
public sealed class BlockExtension
{
    public readonly Block myBlock;
    private bool isNatural;
    private int materialID;
    private float fossilsVolume;
    private const float MAX_VOLUME = 4096;
    private Dictionary<byte, Plane> planes;

    public BlockExtension(Block myBlock,  int i_materialID, bool i_natural)
    {
        materialID = i_materialID;
        isNatural = i_natural;
        fossilsVolume = isNatural ? MAX_VOLUME : 0f;
    }

    public int GetMaterialID() { return materialID; }
    public void ChangeMaterial(int i_materialID)
    {
        if (materialID != i_materialID)
        {
            materialID = i_materialID;
            if (planes != null)
            {
                foreach (var fp in planes)
                {
                    fp.Value.ReplaceMaterial(materialID);
                }
            }
        }
    }

    public bool IsFaceTransparent(byte faceIndex)
    {
        if (planes == null || !planes.ContainsKey(faceIndex)) return true;
        else return MeshMaster.IsMeshTransparent(planes[faceIndex].meshType);
    }
    public bool HavePlane(byte faceIndex)
    {
        if (planes == null) return false;
        else return planes.ContainsKey(faceIndex);
    }
    public bool TryGetPlane(byte faceIndex, out Plane result)
    {
        if (planes == null || !planes.ContainsKey(faceIndex)) { result = null; return false; }
        else
        {
            result = planes[faceIndex];
            return true;
        }
    }
    public float GetFossilsVolume() { return fossilsVolume; }
    public void TakeFossilsVolume(float f) { fossilsVolume -= f; if (fossilsVolume < 0f) fossilsVolume = 0f; }

    public List<BlockpartVisualizeInfo> GetVisualizeInfo(byte vismask)
    {
        if (planes == null) return null;
        else
        {
            var data = new List<BlockpartVisualizeInfo>();
            var pot = GameConstants.powersOfTwo;
            var cpos = myBlock.pos;
            Plane p;
            var chunk = myBlock.myChunk;
            BlockpartVisualizeInfo bvi;
            foreach (var fp in planes)
            {
                p = fp.Value;
                if ((pot[fp.Key] & vismask) != 0)
                {
                    bvi = p.GetVisualInfo(chunk, cpos);
                    if (bvi != null) data.Add(bvi);
                }
            }
            return data;
        }
    }

    //returns false if transparent or wont be instantiated
    public bool InitializePlane(byte faceIndex)
    {
        if (planes == null) planes = new Dictionary<byte, Plane>();
        Plane p = null;
        var pos = myBlock.pos;
        if (faceIndex == Block.UP_FACE_INDEX && pos.y == Chunk.CHUNK_SIZE - 1)
            p = MeshMaster.GetRooftop(this, pos.x % 2 == 0 & pos.z % 2 == 0 & Random.value > 0.5f, isNatural);
        else p = new Plane(this, MeshType.Quad, materialID, faceIndex);
        if (p == null) return false;
        else
        {
            planes.Add(faceIndex, p);
            return MeshMaster.IsMeshTransparent(p.meshType);
        }
    }
    public void DeactivatePlane(byte faceIndex)
    {
        if (planes != null)
        {
            if (planes.ContainsKey(faceIndex))
            {
                if (planes[faceIndex].isClean()) planes.Remove(faceIndex);
                else planes[faceIndex].SetVisibility(false);
            }
        }
    }
    public void RewritePlane(Plane oldplane, Plane newplane)
    {
        if (planes == null)
        {
            planes = new Dictionary<byte, Plane>() { { oldplane.faceIndex, oldplane } };
        }
        else
        {
            byte ix = oldplane.faceIndex;
            planes.Remove(ix);
            planes.Add(ix, newplane);
        }
    }
}

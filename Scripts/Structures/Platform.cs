using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : Structure, IPlanable
{
    override public void SetBasement(Plane p, PixelPosByte pos)
    {
        if (p == null) return;
        SetStructureData(p, pos);
        basement.myChunk.AddBlock()
    }

    //INTERFACE
    public Structure GetStructureData()
    {
        return this;
    }
    public List<BlockpartVisualizeInfo> GetVisualizeInfo(byte visualMask)
    {
        if ((visualMask & (1 << Block.UP_FACE_INDEX)) != 0)
        {
            return new List<BlockpartVisualizeInfo>() { GetFaceVisualData(Block.UP_FACE_INDEX) };
        }
        else return null;
    }
    public BlockpartVisualizeInfo GetFaceVisualData(byte faceIndex)
    {
        if (faceIndex == Block.UP_FACE_INDEX)
        {
            return new BlockpartVisualizeInfo(basement.pos,
                new MeshVisualizeInfo(Block.UP_FACE_INDEX, MaterialType.Basic, basement.myChunk.GetLightValue(basement.pos.OneBlockHigher())),
                MeshType.Quad,
                ResourceType.CONCRETE_ID,
                0
            );
        }
        else return null;
    }
}

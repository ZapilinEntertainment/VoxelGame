using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructurePointer : MonoBehaviour
{
    private Structure source;
    private byte faceIndex;
    public void SetStructureLink(Structure s, byte i_faceIndex)
    {
        source = s;
        var mc = GetComponent<MeshCollider>();
        if (mc == null)
        {
            Destroy(this);
            return;
        }
        else
        {
            mc.sharedMesh = MeshMaster.GetMeshSourceLink(MeshType.Quad);
            faceIndex = i_faceIndex;
        }
    }
    public Structure GetStructureLink()
    {
        return source;
    }
    public byte GetFaceIndex() { return faceIndex; }
}

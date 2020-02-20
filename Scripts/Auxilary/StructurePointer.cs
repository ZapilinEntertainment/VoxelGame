using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructurePointer : MonoBehaviour
{
    private Structure source;
    public void SetStructureLink(Structure s)
    {
        source = s;
        GetComponent<MeshCollider>().sharedMesh = MeshMaster.GetMeshSourceLink(MeshType.Quad);
    }
    public Structure GetStructureLink()
    {
        return source;
    }
}

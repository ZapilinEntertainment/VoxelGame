using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultimaterialPlane : Plane
{
    private GameObject model;
    new public bool isQuad { get { return false; } }

    public MultimaterialPlane(IPlanable i_host, MeshType i_meshType, byte i_faceIndex, GameObject i_model, byte modelRotation) : 
        base (i_host, i_meshType, PoolMaster.MATERIAL_MULTIMATERIAL_ID, i_faceIndex, modelRotation)
    {
        model = i_model;
        model.transform.parent = myChunk.GetRenderersHolderTransform(i_faceIndex);
        model.transform.localPosition = GetCenterPosition();
        model.transform.localRotation = Quaternion.Euler(GetEulerRotation() + Vector3.up * 90f * meshRotation);
    }

    override public void ChangeMaterial(int newId, bool redrawCall) { }
    override public void SetVisibility(bool x)
    {
        if (x != isVisible)
        {
            isVisible = x;
            mainStructure?.SetVisibility(isVisible);
            model.SetActive(x);
        }
    }
    override public BlockpartVisualizeInfo GetVisualInfo(Chunk chunk, ChunkPos cpos)
    {
        model.SetActive(isVisible);
        return null;        
    }

    override public void Annihilate(bool compensateStructures)
    {
        base.Annihilate(compensateStructures);
        if (model != null) Object.Destroy(model);
    }
}

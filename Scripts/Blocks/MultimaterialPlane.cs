using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultimaterialPlane : Plane
{
    private bool isActive = true;
    private GameObject model;
    new public bool isQuad { get { return false; } }

    #region save-load system
    override public void Save(System.IO.FileStream fs)
    {
        if (destroyed) return;
        fs.WriteByte(MULTIMATERIAL_PLANE_CODE);
        SaveData(fs);
    }
    #endregion

    public MultimaterialPlane(IPlanable i_host, MeshType i_meshType, byte i_faceIndex,byte modelRotation) : 
        base (i_host, i_meshType, PoolMaster.MATERIAL_MULTIMATERIAL_ID, i_faceIndex, modelRotation)
    {
        meshType = i_meshType;
    }

    private void PrepareModel()
    {
        model = MeshMaster.InstantiateAdvancedMesh(meshType);
        model.transform.parent = myChunk.GetRenderersHolderTransform(faceIndex);
        model.transform.localPosition = GetCenterPosition();
        model.transform.localRotation = Quaternion.Euler(GetEulerRotationForBlockpart() + Vector3.forward * 90f * meshRotation);
        model.AddComponent<StructurePointer>().SetStructureLink((Structure)host, faceIndex);
        if (!isActive) PoolMaster.SwitchMaterialToOffline(model.GetComponentInChildren<Renderer>());
        model.SetActive(isVisible);
    }
    public void ChangeMesh(MeshType mtype)
    {
        if (meshType == mtype) return;
        else
        {
            meshType = mtype;
            if (model != null) Object.Destroy(model);
            PrepareModel();            
        }
    }
    public void SetActivationStatus(bool x)
    {
        if (x != isActive)
        {
            isActive = x;
            if (model != null)
            {
                if (x) PoolMaster.SwitchMaterialToOnline(model.GetComponentInChildren<Renderer>());
                else PoolMaster.SwitchMaterialToOffline(model.GetComponentInChildren<Renderer>());
            }
        }
    }

    override public void ChangeMaterial(int newId, bool redrawCall) { }  
    override public void SetVisibility(bool x)
    {
        if (x != isVisible)
        {
            isVisible = x;
            mainStructure?.SetVisibility(isVisible);
            if (model == null )
            {
                if (x) PrepareModel();
            }
            else
            {
                if (!x) model.SetActive(false);
            }
        }
    }
    override public BlockpartVisualizeInfo GetVisualInfo(Chunk chunk, ChunkPos cpos)
    {
        if (model == null) PrepareModel();
        return null;        
    }

    override public void Annihilate(bool compensateStructures)
    {
        base.Annihilate(compensateStructures);
        if (model != null) Object.Destroy(model);
    }
}

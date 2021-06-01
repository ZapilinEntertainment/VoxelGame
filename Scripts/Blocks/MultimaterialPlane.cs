using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultimaterialPlane : Plane
{
    private bool isActive = true;
    private GameObject model;
    new public bool isQuad { get { return false; } }

    #region save-load system
    override public void Save(System.IO.Stream fs)
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
        model.transform.parent = myChunk.transform;
        model.transform.localPosition = GetCenterPosition();
        model.transform.localRotation = Quaternion.Euler(GetEulerRotationForBlockpart() + Vector3.forward * 90f * meshRotation);
        model.AddComponent<StructurePointer>().SetStructureLink((Structure)host, faceIndex);
        if (!isActive) PoolMaster.SwitchMaterialToOffline(model.GetComponentInChildren<Renderer>());
        model.SetActive(visibilityMode != VisibilityMode.Invisible & visibilityMode != VisibilityMode.LayerCutHide);
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
    override public void SetVisibilityMode(VisibilityMode vmode, bool forcedRefresh)
    {
        if (vmode == visibilityMode && !forcedRefresh) return;
        if (vmode == VisibilityMode.Invisible | vmode == VisibilityMode.LayerCutHide)
        {
            if (model != null) model.SetActive(false);
        }
        else
        {
            if (visibilityMode == VisibilityMode.Invisible | visibilityMode == VisibilityMode.LayerCutHide)
            {
                if (model == null) PrepareModel();
                else model.SetActive(true);
            }
        }
        visibilityMode = vmode;
        base.SetVisibilityMode(visibilityMode, true);
    }
    override public BlockpartVisualizeInfo GetVisualInfo(Chunk chunk, ChunkPos cpos)
    {
        if (model == null) PrepareModel();
        return null;        
    }

    override public void Annihilate(PlaneAnnihilationOrder order)
    {
        base.Annihilate(order);
        if (model != null) Object.Destroy(model);
    }
}

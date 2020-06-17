using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : Structure, IPlanable
{
    private Block myBlock;
    private Plane upperPlane;

    override protected void SetModel() { }
    override public void SetModelRotation(int r)
    {
        modelRotation = 0;
    }

    override public void SetBasement(Plane p, PixelPosByte pos)
    {
        if (p == null) return;
        //        
        basement = p;
        if (transform.childCount == 0)
        {
            GameObject model;
            var fi = basement.faceIndex;
            if (fi == Block.SURFACE_FACE_INDEX || fi == Block.UP_FACE_INDEX || fi == Block.CEILING_FACE_INDEX || fi == Block.DOWN_FACE_INDEX)
            {
                model = Instantiate(Resources.Load<GameObject>("Structures/Column"));
                surfaceRect = new SurfaceRect(0, 0, 4);
                surfaceRect = new SurfaceRect((byte)(PlaneExtension.INNER_RESOLUTION / 2 - surfaceRect.size / 2), (byte)(PlaneExtension.INNER_RESOLUTION / 2 - surfaceRect.size / 2), surfaceRect.size);
            }
            else
            {
                model = Instantiate(Resources.Load<GameObject>("Structures/ColumnEdge"));
                switch (fi)
                {
                    case Block.FWD_FACE_INDEX: model.transform.localRotation = Quaternion.Euler(0f, 180f, 0f); break;
                    case Block.RIGHT_FACE_INDEX: model.transform.localRotation = Quaternion.Euler(0f, -90f, 0f); break;
                    case Block.LEFT_FACE_INDEX: model.transform.localRotation = Quaternion.Euler(0f, 90f, 0f); break;
                }
                surfaceRect = SurfaceRect.full;
            }
            model.transform.parent = transform;
            model.transform.localPosition = Vector3.zero;
            if (!PoolMaster.useDefaultMaterials) PoolMaster.ReplaceMaterials(model);
        }
        placeInCenter = true;
        basement.AddStructure(this);
        //
        if (!GameMaster.loading) IPlanableSupportClass.AddBlockRepresentation(this, basement, ref myBlock, true);
    }

    private Plane PrepareUpperPlane()
    {
        upperPlane = new Plane(this, MeshType.Quad, ResourceType.CONCRETE_ID, Block.UP_FACE_INDEX, 0);
        return upperPlane;
    }

    override public void Annihilate(bool clearFromSurface, bool compensateResources, bool leaveRuins)
    {
        if (myBlock == null) Delete(clearFromSurface, compensateResources, leaveRuins);
        else
        {
            myBlock.myChunk.DeleteBlock(myBlock.pos, compensateResources);
        }
        
    }
    #region interface
    public void Delete(bool clearFromSurface, bool compensateResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareStructureForDestruction(clearFromSurface, compensateResources, leaveRuins);
        basement = null;
        Destroy(gameObject);
    }
    override public bool IsIPlanable() { return true; }
    public bool IsStructure() { return true; }
    public bool IsFaceTransparent(byte faceIndex)
    {
        return !(faceIndex == Block.UP_FACE_INDEX);
    }
    public bool HavePlane(byte faceIndex)
    {
        return (faceIndex == Block.UP_FACE_INDEX);
    }
    public bool TryGetPlane(byte faceIndex, out Plane result)
    {
        if (faceIndex == Block.UP_FACE_INDEX && upperPlane != null)
        {
            result = upperPlane;
            return true;
        }
        else
        {
            result = null;
            return false;
        }
    }
    public Plane FORCED_GetPlane(byte faceIndex)
    {
        if (faceIndex == Block.UP_FACE_INDEX)
        {
            if (upperPlane == null) PrepareUpperPlane();
            return upperPlane;
        }
        else return null;
    }
    public Block GetBlock() { return myBlock; }
    public bool IsCube() { return false; }
    public bool TryToRebasement() { return false; }
    public bool ContainSurface()
    {
        return upperPlane != null;
    }
    public byte GetAffectionMask() { return (1 << Block.UP_FACE_INDEX); }

    public bool ContainsStructures()
    {
        if (upperPlane == null) return false;
        else return upperPlane.ContainStructures();
    }
    public bool TryGetStructuresList(ref List<Structure> result)
    {
        if (upperPlane == null)
        {
            result = null;
            return false;
        }
        else
        {
            var slist = upperPlane.GetStructuresList();
            if (slist != null)
            {
                result = slist;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }
    }

    //returns false if transparent or wont be instantiated
    public bool InitializePlane(byte faceIndex)
    {
        if (faceIndex == Block.UP_FACE_INDEX)
        {
            if (upperPlane == null) PrepareUpperPlane();
            return true;
        }
        else return false;
    }
    public void DeactivatePlane(byte faceIndex)
    {
        if (faceIndex == Block.UP_FACE_INDEX)
        {
            if (upperPlane != null)
            {
                if (upperPlane.isClean) upperPlane = null; else upperPlane.SetVisibility(false);
                myBlock.myChunk.RefreshBlockVisualising(myBlock, faceIndex);
            }
        }
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
            if (upperPlane != null) return upperPlane.GetVisualInfo(myBlock.myChunk, myBlock.pos);
            else return PrepareUpperPlane().GetVisualInfo(myBlock.myChunk, myBlock.pos);
        }
        else return null;
    }
 

    public void Damage(float f, byte faceIndex)
    {
        ApplyDamage(f);
    }
    #endregion

    #region save-load
    public void SavePlanesData(System.IO.FileStream fs)
    {
        if (upperPlane != null)
        {
            fs.WriteByte(1);
            upperPlane.Save(fs);
        }
        else fs.WriteByte(0);
    }
    public void LoadPlanesData(System.IO.FileStream fs)
    {
        var b = fs.ReadByte();
        IPlanableSupportClass.AddBlockRepresentation(this, basement, ref myBlock, false);
        if (b == 1)
        {
            upperPlane = Plane.Load(fs, this);
        }
    }
    #endregion
}

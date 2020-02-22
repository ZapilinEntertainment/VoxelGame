using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : Structure, IPlanable
{
    private Block myBlock;
    private Plane upperPlane;

    override public void SetBasement(Plane p, PixelPosByte pos)
    {
        if (p == null) return;
        SetStructureData(p, pos);
        IPlanableSupportClass.AddBlockRepresentation(this, basement, ref myBlock);
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
}

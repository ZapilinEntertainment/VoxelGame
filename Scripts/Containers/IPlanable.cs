﻿using System.Collections.Generic;
using UnityEngine;

public interface IPlanable
{
    Plane FORCED_GetPlane(byte faceIndex);
    Block GetBlock();

    bool IsIPlanable();
    bool IsStructure();
    bool IsFaceTransparent(byte faceIndex);
    bool HavePlane(byte faceIndex);
    bool TryGetPlane(byte faceIndex, out Plane result);    
    bool IsCube();
    bool ContainSurface();
    bool ContainsStructures();
    bool TryGetStructuresList(ref List<Structure> result);
    bool TryToRebasement();
    byte GetAffectionMask();

    //returns false if transparent or wont be instantiated
    bool InitializePlane(byte faceIndex);
    void DeactivatePlane(byte faceIndex);

    void Damage(float f, byte faceIndex);
    void Delete(bool clearFromSurface, bool compensateResources, bool leaveRuins);

    void SavePlanesData(System.IO.FileStream fs);
    void LoadPlanesData(System.IO.FileStream fs);
    List<BlockpartVisualizeInfo> GetVisualizeInfo(byte visualMask);
    BlockpartVisualizeInfo GetFaceVisualData(byte faceIndex);
}

public static class IPlanableSupportClass
{
    public static void AddBlockRepresentation(IPlanable s, Plane basement, ref Block myBlock, bool checkPlanes)
    {
        var chunk = basement.myChunk;
        ChunkPos cpos = basement.pos;
        switch (basement.faceIndex)
        {
            case Block.FWD_FACE_INDEX: cpos = cpos.OneBlockForward(); break;
            case Block.RIGHT_FACE_INDEX: cpos = cpos.OneBlockRight(); break;
            case Block.BACK_FACE_INDEX: cpos = cpos.OneBlockBack(); break;
            case Block.LEFT_FACE_INDEX: cpos = cpos.OneBlockLeft(); break;
            case Block.UP_FACE_INDEX: cpos = cpos.OneBlockHigher(); break;
            case Block.DOWN_FACE_INDEX: cpos = cpos.OneBlockDown(); break;
        }
        myBlock = chunk.AddBlock(cpos, s, false, checkPlanes);
        if (myBlock == null)
        {            
            s.Delete(true, true, false);
            Debug.LogException(new System.Exception("new IPlanable block cannot be created"));
            return;
        }
        else
        {
            chunk.RecalculateVisibilityAtPoint(myBlock.pos, s.GetAffectionMask());
        }
    }    
}

using System.Collections.Generic;

public interface IPlanable
{
    Plane FORCED_GetPlane(byte faceIndex);
    Block GetBlock();

    bool IsStructure();
    bool IsFaceTransparent(byte faceIndex);
    bool HavePlane(byte faceIndex);
    bool TryGetPlane(byte faceIndex, out Plane result);    
    bool IsCube();
    bool ContainSurface();
    bool ContainsStructures();
    bool TryGetStructuresList(ref List<Structure> result);
    byte GetAffectionMask();

    //returns false if transparent or wont be instantiated
    bool InitializePlane(byte faceIndex);
    void DeactivatePlane(byte faceIndex);

    void Damage(float f, byte faceIndex);

    List<BlockpartVisualizeInfo> GetVisualizeInfo(byte visualMask);
    BlockpartVisualizeInfo GetFaceVisualData(byte faceIndex);
}

using System.Collections.Generic;

public interface IPlanable
{
    Structure GetStructureData();
    List<BlockpartVisualizeInfo> GetVisualizeInfo(byte visualMask);
    BlockpartVisualizeInfo GetFaceVisualData(byte faceIndex);
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hexTester : MonoBehaviour
{
    private AnchorBasement anchor;
    private byte stage = 0;

    private void Update()
    {
        if (Input.GetKeyDown("q"))
        {
            if (stage == 0)
            {
                var c = GameMaster.realMaster.mainChunk;
                var b = c.AddBlock(ChunkPos.zer0, ResourceType.CONCRETE_ID, false, true);
                var p = b.FORCED_GetPlane(Block.DOWN_FACE_INDEX);
                anchor = p.CreateStructure(Structure.ANCHOR_BASEMENT_ID) as AnchorBasement;
                Debug.Log("Anchor constructed");
                stage++;
            }
        }
    }

}

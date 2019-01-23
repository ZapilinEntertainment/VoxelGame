using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class BlockRendererController : MonoBehaviour
{
    public MeshRenderer[] faces;
    public byte renderMask = 0, visibilityMask = 63;
    private bool visible = true;
    public Structure structure { get; private set; }

    public void SetStructure(Structure s)
    {
        structure = s;
    }

    public void SetRenderBitmask(byte newRenderMask)
    {
        if (structure != null)
        {
            byte inputMask = newRenderMask;
            int b = 0;
            if (false)
            {
                switch (structure.modelRotation)
                {
                    case 2:
                        b = newRenderMask & 8;
                        if ((newRenderMask & 1) != b) { if (b == 0) newRenderMask--; else newRenderMask++; } // 3
                        b = inputMask & 1;
                        if ((newRenderMask & 2) != b) { if (b == 0) newRenderMask -= 2; else newRenderMask += 2; } // 0
                        b = inputMask & 2;
                        if ((newRenderMask & 4) != b) { if (b == 0) newRenderMask -= 4; else newRenderMask += 4; } // 1
                        b = inputMask & 4;
                        if ((newRenderMask & 8) != b) { if (b == 0) newRenderMask -= 8; else newRenderMask += 8; } // 2
                        break;
                    case 4:
                        b = newRenderMask & 4;
                        if ((newRenderMask & 1) != b) { if (b == 0) newRenderMask--; else newRenderMask++; } // 3
                        b = inputMask & 8;
                        if ((newRenderMask & 2) != b) { if (b == 0) newRenderMask -= 2; else newRenderMask += 2; } // 0
                        b = inputMask & 1;
                        if ((newRenderMask & 4) != b) { if (b == 0) newRenderMask -= 4; else newRenderMask += 4; } // 1
                        b = inputMask & 2;
                        if ((newRenderMask & 8) != b) { if (b == 0) newRenderMask -= 8; else newRenderMask += 8; } // 2
                        break;
                    case 6:
                        b = newRenderMask & 2;
                        if ((newRenderMask & 1) != b) { if (b == 0) newRenderMask--; else newRenderMask++; } // 3
                        b = inputMask & 4;
                        if ((newRenderMask & 2) != b) { if (b == 0) newRenderMask -= 2; else newRenderMask += 2; } // 0
                        b = inputMask & 8;
                        if ((newRenderMask & 4) != b) { if (b == 0) newRenderMask -= 4; else newRenderMask += 4; } // 1
                        b = inputMask & 1;
                        if ((newRenderMask & 8) != b) { if (b == 0) newRenderMask -= 8; else newRenderMask += 8; } // 2
                        break;
                }
            }
        }
        if (renderMask != newRenderMask)
        {
            renderMask = newRenderMask;
            if (visibilityMask == 0 | !visible) return;
            //faces[0].enabled = ((renderMask & 1) != 0);
            //faces[1].enabled = ((renderMask & 2) != 0);
            //faces[2].enabled = ((renderMask & 4) != 0);
            //faces[3].enabled = ((renderMask & 8) != 0);
            faces[4].enabled = ((renderMask & visibilityMask & 16) != 0);
            faces[5].enabled = ((renderMask & visibilityMask & 32) != 0);
        }
    }

    public void SetVisibilityMask(byte newVisibilityMask)
    {
        visibilityMask = newVisibilityMask;
        if (renderMask == 0 | !visible) return;
        faces[4].enabled = ((visibilityMask & 16) != 0);
        faces[5].enabled = ((visibilityMask & 32) != 0);
    }

    public void SetVisibility(bool x)
    {
        if (x == visible) return;
        visible = x;
        if (visible)
        {
            //faces[0].enabled = ((renderMask & 1) != 0);
            //faces[1].enabled = ((renderMask & 2) != 0);
            //faces[2].enabled = ((renderMask & 4) != 0);
            //faces[3].enabled = ((renderMask & 8) != 0);
            faces[4].enabled = ((renderMask & visibilityMask & 16) != 0);
            faces[5].enabled = ((renderMask & visibilityMask & 32) != 0);
        }
        else
        {
            faces[0].enabled = false;
            faces[1].enabled = false;
            faces[2].enabled = false;
            faces[3].enabled = false;
            faces[4].enabled = false;
            faces[5].enabled = false;
        }
    }
}

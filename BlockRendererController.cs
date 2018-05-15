using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockRendererController : MonoBehaviour {
	public MeshRenderer[] faces;
	public byte renderMask = 0, visibilityMask = 63;

	public void SetRenderBitmask(byte x) {
		if (renderMask != x) {
			renderMask = x;
			if ( visibilityMask == 0) return;
			for (int i = 0; i< 6; i++) {
				if ((renderMask & ((int)Mathf.Pow(2, i)) & visibilityMask) != 0) faces[i].enabled = true;
				else faces[i].enabled = false;
			}
			}
	}

	public void SetVisibilityMask (byte x) {
		visibilityMask = x;
		if (renderMask == 0) return;
		for (int i = 0; i< 6; i++) {
			if ((renderMask & ((int)Mathf.Pow(2, i)) & visibilityMask) != 0) faces[i].enabled = true;
			else faces[i].enabled = false;
		}
		visibilityMask &= 47;
	}
}

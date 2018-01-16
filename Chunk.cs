using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour {
	readonly Vector3 CENTER_POS = new Vector3(8,8,8);
	Block[,,] blocks;
	bool cameraHasMoved = false; Vector3 prevCamPos = Vector3.zero; Quaternion prevCamRot = Quaternion.identity;
	float cullingTimer =0, cullingUpdateTime = 0.04f;
	bool isTotalCulled = false;
	public byte prevBitmask = 63;

	void Start() {
		prevCamPos = Camera.main.transform.position;
		prevCamRot = Camera.main.transform.rotation;
	}

	void Update() {
		if (prevCamPos != Camera.main.transform.position || prevCamRot != Camera.main.transform.rotation) {
			cameraHasMoved = true;
			prevCamPos = Camera.main.transform.position;
			prevCamRot = Camera.main.transform.rotation;
		}
		if (cullingTimer > 0) cullingTimer-= Time.deltaTime;
		if (cullingTimer <= 0 && cameraHasMoved) CullingUpdate(); 
	}

	public void SetChunk(int[,,] newData) {
		if (blocks != null) ClearChunk();
		blocks = new Block[16,16,16];
		int a =16,b =16,c =16;
		if (newData.GetLength(0) < a) a= newData.GetLength(0);
		if (newData.GetLength(1) < b) a= newData.GetLength(1);
		if (newData.GetLength(2) < c) a= newData.GetLength(2);
		if (a == 0 || b== 0 || c== 0) return;
		for (int i = 0; i< a; i++) {
			for (int j =0; j< b; j++) {
				for (int k =0; k< c; k++) {
					if (newData[i,j,k] == 0) {blocks[i,j,k] = null;}
					else {
						blocks[i,j,k] = new Block(newData[i,j,k]);
						blocks[i,j,k].body.transform.parent = transform;
						blocks[i,j,k].body.transform.localPosition = new Vector3(i,j,k);
						blocks[i,j,k].SetChunk(this);
					}
				}
			}
		}
		for (int i = 0; i< 16; i++) {
			for (int j =0; j< 16; j++) {
				for (int k = 0; k< 16; k++) {
					if (blocks[i,j,k] == null) continue;
					byte vmask = 63;
					Block bx = GetBlock(i+1,j,k); if (bx != null && !bx.IsTransparent()) vmask &= 61;
					bx = GetBlock(i-1,j,k); if (bx != null && !bx.IsTransparent()) vmask &= 55;
					bx = GetBlock(i,j+1,k); if (bx != null && !bx.IsTransparent()) vmask &= 47;
					bx = GetBlock(i,j - 1,k); if (bx != null && !bx.IsTransparent()) vmask &= 31;
					bx = GetBlock(i,j,k+1); if (bx != null && !bx.IsTransparent()) vmask &= 62;
					bx = GetBlock(i,j,k-1); if (bx != null && !bx.IsTransparent()) vmask &= 59;
					blocks[i,j,k].SetVisibilityMask(vmask);
				}
			}
		}
	}

	public Block GetBlock (int x, int y, int z) {
		if (x < 0 ||x > 15 || y < 0 || y > 15 || z < 0 || z > 15) return null;
		else {return blocks[x,y,z];}
	}

	public void ChangeBlockVisibilityOnReplacement (int x, int y, int z, bool value) {
		Block b = GetBlock(x+1,y,z); if (b != null) {b.ChangeVisibilityMask(3,value);}
		b = GetBlock(x-1,y,z); if (b != null) {b.ChangeVisibilityMask(1,value);}
		b = GetBlock(x,y + 1,z); if (b != null) {b.ChangeVisibilityMask(5,value);}
		b = GetBlock(x,y-1,z); if (b != null) {b.ChangeVisibilityMask(4,value);}
		b = GetBlock(x,y,z+1); if (b != null) {b.ChangeVisibilityMask(2,value);}
		b = GetBlock(x,y,z-1); if (b != null) {b.ChangeVisibilityMask(0,value);}
	}


	public void ClearChunk() {
		for (int i = 0; i< 16; i++) {
			for (int j =0; j< 16; j++) {
				for (int k =0; k< 16; k++) {
					if ( blocks[i,j,k] == null) continue;
					blocks[i,j,k].Destroy();
					blocks[i,j,k] = null;
				}
			}
		}
	}

	void CullingUpdate() {
		byte camSector = 0;
		Vector3 cpos = transform.InverseTransformPoint(Camera.main.transform.position);
		Vector3 v = Vector3.one * (-1);
		if (cpos.x > 0) { if (cpos.x > 16) v.x = 1; else v.x = 0;} 
		if (cpos.y > 0) {if (cpos.y > 16) v.y = 1; else v.y = 0;}
		if (cpos.z > 0) {if (cpos.z > 16) v.z = 1; else v.z = 0;}
		//print (v);
		if (v != Vector3.zero) {
			//easy-culling
			Vector3 cdir = transform.InverseTransformDirection(Camera.main.transform.forward);
			float av = Vector3.Angle(CENTER_POS - cpos, cdir);
			if (av > 90 + Camera.main.fieldOfView/2) { //total culling
				if (!isTotalCulled ) {
					foreach (Block b in blocks) {if (b != null) b.body.SetActive(false);}
					isTotalCulled = true;
				}
			}
			else {
				if (isTotalCulled) {
					foreach (Block b in blocks) {if (b!= null) b.body.SetActive(true);}
					isTotalCulled = false;
				}
					
				byte renderBitmask = 63;
				if (v.x ==1) renderBitmask &= 55; else if (v.x == -1) renderBitmask &= 61;
				if (v.y == 1) renderBitmask &= 31; else if (v.y == -1) renderBitmask &= 47;
				if (v.z == 1) renderBitmask &= 59; else if (v.z == -1) renderBitmask &= 62;
				if (renderBitmask != prevBitmask) {
				 foreach(Block b in blocks) {if (b!=null) b.SetRenderBitmask(renderBitmask);}
					prevBitmask = renderBitmask;
				}
			}
		}
		else {
			//enable dynamic culling for all blocks
		}
		cullingTimer = cullingUpdateTime;
		cameraHasMoved = false;
	}

}

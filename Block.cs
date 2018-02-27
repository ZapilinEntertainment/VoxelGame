using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block {

	static readonly GameObject quad_pref;
	public static readonly Material dirt_material, grass_material, stone_material, default_material;
	public static readonly Texture2D dirt_texture;
	public const int STONE_ID = 1, DIRT_ID = 2, GRASS_ID = 3;
	public const float QUAD_SIZE = 1;
	public static Block BlockedByStructure;

	MeshRenderer[] faces; // 0 - north, 1 - east, 2 - south, 3 - west, 4 - up, 5 - down
	byte visibilityMask = 63;
	public readonly GameObject body;
	byte renderMask = 63; // bitmask
	public int f_id {get;private set;}
	public Chunk myChunk {get; private set;}
	bool isTransparent = false, isVisible =true;
	public bool onSurface {get;private set;}
	public int daytimeUpdatePosition = -1;
	public ChunkPos pos {get; private set;}

	public Block Clone() {
		Block b = new Block(f_id);
		b.isTransparent = isTransparent;
		b.myChunk = myChunk;
		b.isVisible = isVisible;
		b.body.transform.localPosition = body.transform.localPosition;
		b.body.transform.localRotation = body.transform.localRotation;
		b.renderMask = renderMask;
		b.visibilityMask = 0;
		return b;
	}

	static Block () {
		quad_pref = GameObject.CreatePrimitive(PrimitiveType.Quad);
		quad_pref.GetComponent<MeshRenderer>().enabled =false;
		dirt_material = Resources.Load<Material>("Materials/Dirt");
		grass_material = Resources.Load<Material>("Materials/Grass");
		stone_material = Resources.Load<Material>("Materials/Stone");
		default_material = Resources.Load<Material>("Materials/Default");
		dirt_texture = Resources.Load<Texture2D>("Textures/Dirt_tx");

		BlockedByStructure = new Block();
		BlockedByStructure.isVisible = false;
		BlockedByStructure.isTransparent = true;
	}

	public Block() {
		body = new GameObject("block");
		renderMask = 1;
		f_id =0;
		onSurface = false;
	}

	public Block (int id) {
		body = new GameObject("block");
		renderMask = 1;
		f_id = id;
		onSurface = false;
}
	public void Replace( int newId) {
		f_id = newId;
		if (faces != null) {
			for (int i =0; i< 6; i++) {
				if (faces[i] != null) {
					faces[i].material =  GetMaterialById(newId);
				}
			}
			if (faces[4] != null) {
				Grassland gl = faces[4].GetComponent<Grassland>(); if (gl != null) gl.Annihilation();
				BlockSurface bs = faces[4].GetComponent<BlockSurface>(); if (bs!=null) bs.Annihilation();
			}
		}
	}

	public void SetRenderBitmask(byte x) {
		if (!isVisible) return;
		if (renderMask != x) {
			renderMask = x;
			if (visibilityMask == 0) return;
		for (int i = 0; i< 6; i++) {
				if ((renderMask & ((int)Mathf.Pow(2, i)) & visibilityMask) != 0) {
					if (faces != null && faces[i]!= null) faces[i].enabled = true;
					else CreateFace(i);
				}
				else {if (faces != null && faces[i]!=null) faces[i].enabled = false;}
		}
		}
	}

	public void SetVisibilityMask (byte x) {
		if (!isVisible) return;
		visibilityMask = x;
		for (int i = 0; i< 6; i++) {
			if ((renderMask & ((int)Mathf.Pow(2, i)) & visibilityMask) != 0) {
				if (faces != null && faces[i]!= null) faces[i].enabled = true;
				else CreateFace(i);
			}
			else {if (faces != null && faces[i]!=null) faces[i].enabled = false;}
		}
	}
	public byte GetVisibilityMask() {return visibilityMask;}
	public void ChangeVisibilityMask (byte index, bool value) { 
		if (!isVisible) return;
		int vm = visibilityMask;
		int im =(byte) (63 - (int)Mathf.Pow(2, index));
		if (value == false) { vm &= im;}
		else {vm = ~vm; vm &= im; vm = ~vm;}
		if (vm != visibilityMask) SetVisibilityMask((byte)vm);
	}

	void CreateFace(int i) {
		if (!isVisible || body == null) return;
		if (faces == null) faces =new MeshRenderer[6];
		else {if (faces[i] != null) return;}
		GameObject g = GameObject.Instantiate(quad_pref) as GameObject;
		faces[i] =g.GetComponent <MeshRenderer>();
		g.transform.parent = body.transform;
		switch (i) {
		case 0: faces[i].name = "north_plane"; faces[i].transform.localRotation = Quaternion.Euler(0, 180, 0); faces[i].transform.localPosition = new Vector3(0, 0, 0.5f); break;
		case 1: faces[i].transform.localRotation = Quaternion.Euler(0, 270, 0); faces[i].name = "east_plane"; faces[i].transform.localPosition = new Vector3(0.5f, 0, 0); break;
		case 2: faces[i].name = "south_plane"; faces[i].transform.localPosition = new Vector3(0, 0, -0.5f); break;
		case 3: faces[i].transform.localRotation = Quaternion.Euler(0, 90, 0);faces[i].name = "west_plane"; faces[i].transform.localPosition = new Vector3(-0.5f, 0, 0); break;
		case 4: 
			faces[i].transform.localRotation = Quaternion.Euler(90, 0, 0);
			faces[i].name = "upper_plane"; 
			faces[i].transform.localPosition = new Vector3(0, 0.5f, 0); break;
			if (onSurface) {
				BlockSurface bs =  faces[i].gameObject.AddComponent<BlockSurface>();
				bs.SetBasement(this, faces[4]);
			}
		case 5: 
			faces[i].transform.localRotation = Quaternion.Euler(-90, 0, 0); 
			faces[i].name = "bottom_plane"; 
			faces[i].transform.localPosition = new Vector3(0, -0.5f, 0); 
			GameObject.Destroy( faces[i].gameObject.GetComponent<MeshCollider>() );
			break;
		}
		faces[i].material = GetMaterialById(f_id);
		faces[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		faces[i].enabled = true;
	}
		
	public MeshRenderer GetSurfacePlane() {	if (faces != null && faces[4] != null) return faces[4]; else return null;}
	public void SetChunk(Chunk c) {myChunk = c;}
	public bool IsTransparent() {return isTransparent;}
	public bool IsVisible() {return isVisible;}
	public void SetSurfaceStatus( bool x) {
		onSurface = x; 
		if (onSurface) {
			if (faces == null) faces = new MeshRenderer[6];
			if (faces[4] == null) CreateFace(4);
			else {
				if (faces[4].GetComponent<BlockSurface>() == null)  
				{
					BlockSurface bs =  faces[4].gameObject.AddComponent<BlockSurface>();
					bs.SetBasement(this, faces[4]);
				}
			}
		}
		}

	public void SetPos(ChunkPos p) {
		pos = p;
		body.transform.localPosition = new Vector3(pos.x,pos.y,pos.z);
		body.transform.localRotation = Quaternion.Euler(Vector3.zero);
	}
	public void SetPos(int a, int b, int c) {
		pos = new ChunkPos(a,b,c);
		body.transform.localPosition = new Vector3(pos.x,pos.y,pos.z);
		body.transform.localRotation = Quaternion.Euler(Vector3.zero);
	}

	public static Material GetMaterialById(int id) {
		switch (id) {
		case STONE_ID: return stone_material;  break;
		case DIRT_ID: return dirt_material; break;
		case GRASS_ID: return grass_material; break;
		default: return default_material; break;
		}
	}

	public void Destroy() {
		if (daytimeUpdatePosition != -1) GameMaster.realMaster.RemoveBlockFromDaytimeUpdateList(daytimeUpdatePosition);
		GameObject.Destroy(body);
		isVisible =false;
		visibilityMask = 0;
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI : MonoBehaviour {
	public bool landing = true, landingEnded = false, landPointSet = false;
	bool drawLine =false, drawOneCell = false, landingBlocked = true;
	public Transport startTransport;
	public LineRenderer lineDrawer;
	SurfaceBlock blockToLanding = null, add_block1, add_block2;
	GameObject landingX;
	public static UI current;
	Rect landButtonRect;

	void Awake () {
		landingX = Instantiate(Resources.Load<GameObject>("Prefs/LandingX")) as GameObject;
		landingX.SetActive(false);
		landing = false;
		current = this;
		landButtonRect = new Rect(0,0,0,0);
	}

	void Update () {
		if (landing) {
			Vector3 lineStartPos = Vector3.zero;
			bool ignoreClick = false;
			if (blockToLanding != null && !landingBlocked) {
				Vector2 mp = Input.mousePosition;
				mp.y = Screen.height - mp.y;
				if (mp.x < landButtonRect.x + landButtonRect.width && mp.x > landButtonRect.x && mp.y > landButtonRect.y && mp.y < landButtonRect.y + landButtonRect.height) ignoreClick = true;
			}

			if (Input.GetMouseButton(0) && !ignoreClick) {
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit rh;
				if (Physics.Raycast(ray, out rh)) {
					SurfaceBlock contactedBlock = rh.collider.transform.parent.GetComponent<SurfaceBlock>();
					if (contactedBlock != null ) {
						bool drawHorizontally = true;
						drawLine = true;
						int x = contactedBlock.pos.x, z = contactedBlock.pos.z, y = contactedBlock.pos.y;
						Chunk c = contactedBlock.myChunk;

						bool found = false, fit1 = false, fit2 = false;
						SurfaceBlock b = c.GetSurfaceBlock(x, z+2); fit1 = (b != null && b.pos.y == y && b.mainStructure == null); 
						SurfaceBlock b2 = c.GetSurfaceBlock(x,z+1); fit2 = (b2 != null && b2.pos.y == y && b2.mainStructure == null); 
						if ( (fit1 & fit2) == true) {
							found = true; 
							lineStartPos = new Vector3(x - Block.QUAD_SIZE/2f, y -Block.QUAD_SIZE/2f + 0.05f, z + Block.QUAD_SIZE * 2.5f); 
							blockToLanding = b2; add_block1 = b; add_block2 = contactedBlock;
						}
						else {
							b = c.GetSurfaceBlock(x,z-1); 
							fit1 = (b != null && b.pos.y == y && b.mainStructure == null); 
							if ( (fit2 & fit1) == true) {
								found = true; 
								lineStartPos = new Vector3(x - Block.QUAD_SIZE/2f, y -Block.QUAD_SIZE/2f + 0.05f, z + Block.QUAD_SIZE * 1.5f);
								blockToLanding = contactedBlock; add_block1 = b; add_block2 = b2;
							}
							else {
								b2 = c.GetSurfaceBlock(x,z-2); fit2 = (b2 != null && b2.pos.y == y && b2.mainStructure == null); 
								if ( (fit1 & fit2) == true) {
									found = true; 
									lineStartPos = new Vector3(x - Block.QUAD_SIZE/2f, y -Block.QUAD_SIZE/2f + 0.05f, z + Block.QUAD_SIZE/2f);
									blockToLanding = b;	add_block1 = contactedBlock; add_block2 = b2;
								}
							}
						}
						if (found == false) {
							b = c.GetSurfaceBlock(x-2, z); fit1 = (b != null && b.pos.y == y && b.mainStructure == null); 
							b2 = c.GetSurfaceBlock(x-1,z); fit2 = (b2 != null && b2.pos.y == y && b2.mainStructure == null); 

							if ( (fit1 & fit2) == true) {
								found = true; 
								lineStartPos = new Vector3(x - Block.QUAD_SIZE * 2.5f, y -Block.QUAD_SIZE/2f + 0.05f, z + Block.QUAD_SIZE/2); 
								blockToLanding = b2; add_block1 = b; add_block2 = contactedBlock;
							}
							else {
								b = c.GetSurfaceBlock(x+1,z); fit1 = (b != null && b.pos.y == y && b.mainStructure == null); 
								if ( (fit2 & fit1) == true) {
									found = true; 
									lineStartPos = new Vector3(x - Block.QUAD_SIZE * 1.5f, y -Block.QUAD_SIZE/2f + 0.05f, z + Block.QUAD_SIZE/2f); 
									blockToLanding = contactedBlock; add_block1 = b; add_block2 = b2;
								}
								else {
									b2 = c.GetSurfaceBlock(x+2,z); fit2 = (b2 != null && b2.pos.y == y && b2.mainStructure == null); 
									if ( (fit2 & fit1) == true) {
										found = true; 
										lineStartPos = new Vector3(x - Block.QUAD_SIZE/2f, y -Block.QUAD_SIZE/2f + 0.05f, z + Block.QUAD_SIZE/2f);
										blockToLanding = b; add_block1 = b2; add_block2 = contactedBlock;
									}
								}
							}
							if (found) {drawHorizontally = true;}
						}
						else { drawHorizontally = false; }
						if (found) {
							landingX.transform.position = blockToLanding.transform.position + Vector3.up * (-Block.QUAD_SIZE/2f + 0.01f);
							landingX.SetActive(true);
							lineDrawer.material = PoolMaster.lr_green_material;
							drawOneCell = false;
							landingBlocked = false;
						} 
						else {
							lineDrawer.material = PoolMaster.lr_red_material; 
							lineStartPos = new Vector3(x - Block.QUAD_SIZE/2f, y -Block.QUAD_SIZE/2f  + 0.05f, z + Block.QUAD_SIZE * 0.5f);
							landingX.SetActive(false);
							drawOneCell = true;
							landingBlocked = true;
						}
						Vector3[] points = new Vector3[5];
						points[0] =  lineStartPos; points[4] = points[0];
						if (!drawOneCell) {
							if (drawHorizontally) {
								points[1] = lineStartPos + Vector3.right * 3 * Block.QUAD_SIZE;
								points[2] = points[1] + Vector3.back * Block.QUAD_SIZE;
								points[3] = points[2] + Vector3.left * Block.QUAD_SIZE * 3;
							}
							else {	
								points[1] = lineStartPos + Vector3.right  * Block.QUAD_SIZE;
								points[2] = points[1] + Vector3.back * 3 * Block.QUAD_SIZE;
								points[3] = points[2] + Vector3.left * Block.QUAD_SIZE;
							}
						}
						else {
							points[1] = points[0] + Vector3.right * Block.QUAD_SIZE;
							points[2] = points[1] + Vector3.back * Block.QUAD_SIZE;
							points[3] = points[2] + Vector3.left * Block.QUAD_SIZE;
						}
						lineDrawer.SetPositions(points);
						if ( !lineDrawer.enabled ) lineDrawer.enabled = true;
					}
					else {drawLine =false;blockToLanding = null;landingX.SetActive(false);}
				}
				else {blockToLanding = null;drawLine = false;landingX.SetActive(false);}
			}
			if (blockToLanding != null) {drawLine = true;}
			if (drawLine == false) {
				if (lineDrawer.enabled) lineDrawer.enabled = false;
				blockToLanding = null;
			}
		}
		if (!landingEnded && landPointSet) {
			if (startTransport == null) {
				landingEnded = true;
				landingX.SetActive(false);
				landPointSet = false;
			}
		}
	}

	void OnGUI() {
		if (landing) {
			if (blockToLanding != null && !landingBlocked) {
				Vector3 sc_pos = Camera.main.WorldToScreenPoint(blockToLanding.transform.position);
				sc_pos.y = Screen.height - sc_pos.y;
				landButtonRect = new Rect(sc_pos.x, sc_pos.y, 256,64);
				if (GUI.Button(landButtonRect, "Land")) {
					startTransport.GetComponent<Zeppelin>().SetLandingPlace(blockToLanding, add_block1, add_block2);
					landing = false;
					lineDrawer.enabled = false;
					landPointSet = true;
				}
			}
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour {
	 public static  GameMaster realMaster;
	public static float gameSpeed  {get; private set;}
	public float newGameSpeed = 1;
	List<Block> daytimeUpdates_blocks;

	public Transform camTransform;
	List<GameObject> cameraUpdateBroadcast;
	bool cameraHasMoved = false; Vector3 prevCamPos = Vector3.zero; Quaternion prevCamRot = Quaternion.identity;
	float cameraTimer =0, cameraUpdateTime = 0.04f;
	bool loadingScreen = false, fontSize_set = false;
	int camCullingMask = 1;
	public Chunk mainChunk;

	public const int LIFEPOWER_SPREAD_SPEED = 10, START_LIFEPOWER = 100000;
	public static float lifeGrowCoefficient {get;private set;}

	public GameMaster () {
		if (realMaster != null) realMaster = null;
		realMaster = this;
		daytimeUpdates_blocks = new List<Block>();
		lifeGrowCoefficient = 1;
	}

	void Awake() {
		gameSpeed = 1;
		cameraUpdateBroadcast = new List<GameObject>();
	}

	void Start() {
		if (camTransform == null) camTransform = Camera.main.transform;
		prevCamPos = camTransform.position * (-1);
		camCullingMask = Camera.main.cullingMask;
		Camera.main.cullingMask = 0;
		loadingScreen = true;
		gameSpeed = 100;
		lifeGrowCoefficient = 1.1f;
	}

	void Update() {
		if (loadingScreen) {
			if (mainChunk != null) {
				if (mainChunk.lifePower <= 0) {
					mainChunk.lifePower = START_LIFEPOWER;
					loadingScreen = false;
					gameSpeed = 1; lifeGrowCoefficient = 1;
					Camera.main.cullingMask = camCullingMask;
					mainChunk.CameraUpdate(camTransform);
					Instantiate(Resources.Load<GameObject>("Prefs/Zeppelin"));
				}
			}
			return;
		}

		if (gameSpeed != newGameSpeed) gameSpeed = newGameSpeed;

		if (camTransform != null) {
			if (prevCamPos != camTransform.position || prevCamRot != Camera.main.transform.rotation) {
				cameraHasMoved = true;
				prevCamPos = camTransform.position;
				prevCamRot = camTransform.rotation;
			}
			if (cameraTimer > 0) cameraTimer-= Time.deltaTime;
			if (cameraTimer <= 0 && cameraHasMoved) { 
				int c = cameraUpdateBroadcast.Count - 1;
				while (c >= 0) {
					if (cameraUpdateBroadcast[c] == null) cameraUpdateBroadcast.RemoveAt(c);
					else cameraUpdateBroadcast[c].SendMessage("CameraUpdate", camTransform, SendMessageOptions.DontRequireReceiver);
					c--;
				}
				cameraTimer = cameraUpdateTime;
			}
		}
	}

	public void AddToCameraUpdateBroadcast(GameObject g) {
		if (g != null) cameraUpdateBroadcast.Add(g);
	}

	public void AddBlockToDaytimeUpdateList(Block b) {
		if (b == null) return;
		daytimeUpdates_blocks.Add(b);
		b.daytimeUpdatePosition = daytimeUpdates_blocks.Count - 1;
	}

	public void RemoveBlockFromDaytimeUpdateList (int index) {
		if (daytimeUpdates_blocks[index] == null) return;
		daytimeUpdates_blocks[index].daytimeUpdatePosition = -1;
		daytimeUpdates_blocks[index] = daytimeUpdates_blocks[daytimeUpdates_blocks.Count - 1];
		daytimeUpdates_blocks.RemoveAt(daytimeUpdates_blocks.Count - 1);
	}

	void OnGUI() {
		if (!fontSize_set) {
			GUI.skin.GetStyle("Label").fontSize = 50;
			fontSize_set = true;
		}
		if (loadingScreen) {
			float f = mainChunk.lifePower;
			f /= (float)START_LIFEPOWER;
			GUI.Label(new Rect(Screen.width/2 - 200, Screen.height/2 - 100, 400, 200),((int) (f * 100)).ToString() + '%');
		}
	}
}

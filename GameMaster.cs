using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

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
	static string path;

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
		path = Application.dataPath;
	}

	void Start() {
		if (camTransform == null) camTransform = Camera.main.transform;
		prevCamPos = camTransform.position * (-1);
		Instantiate(Resources.Load<GameObject>("Prefs/Zeppelin"));
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


	public static void Save2DMatrix (float[,] arr, string fileName ) {
		// Не совсем правильный вывод, поправить
		using (StreamWriter sw = File.CreateText(path + '/' + fileName+".txt")) 
		{
			for (int i = arr.GetLength(0) - 1; i >= 0; i--) {
				string s = "";
				for (int j = 0; j< arr.GetLength(1); j++) {
					string ts = arr[i,j].ToString();
					if (ts.Length > 5) ts = ts.Substring(0,5); else {
						switch (ts.Length) {
						case 1: ts += ".000"; break;
						case 3: ts +="00";break;
						}
					}
					s += ts + ' ';
				}
				sw.WriteLine(s);
			}
		}	
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

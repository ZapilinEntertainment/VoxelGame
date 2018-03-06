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
	bool fontSize_set = false;
	int camCullingMask = 1;
	public Chunk mainChunk;
	static string path;

	public const int LIFEPOWER_SPREAD_SPEED = 10, START_LIFEPOWER = 100000, CRITICAL_DEPTH = - 200;
	public static float lifeGrowCoefficient {get;private set;}

	float t;
	uint day = 0, week = 0, month = 0, year = 0, millenium = 0;
	const byte DAYS_IN_WEEK = 7, WEEKS_IN_MONTH = 4, MONTHS_IN_YEAR = 12;
	const float DAY_LONG = 60;
	public List<Component> everydayUpdateList, everyYearUpdateList;

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

		everydayUpdateList = new List<Component>();
		everyYearUpdateList = new List<Component>();
	}

	void Start() {
		if (camTransform == null) camTransform = Camera.main.transform;
		prevCamPos = camTransform.position * (-1);
		Instantiate(Resources.Load<GameObject>("Prefs/Zeppelin"));
	}

	void Update() {
		if (gameSpeed != newGameSpeed) gameSpeed = newGameSpeed;
		t += Time.deltaTime * gameSpeed;
		if (t >= DAY_LONG) {
			day += (uint)(t / DAY_LONG);
			t = t % DAY_LONG;
			if (day >= DAYS_IN_WEEK) {
				week += day / DAYS_IN_WEEK;
				day = day % DAYS_IN_WEEK;
				if (week >= WEEKS_IN_MONTH) {
					month += week / WEEKS_IN_MONTH;
					week = week % WEEKS_IN_MONTH;
					if (month > MONTHS_IN_YEAR) {
						year += month / MONTHS_IN_YEAR;
						month = month % MONTHS_IN_YEAR;
						if (year > 1000) {
							millenium += year / 1000;
							year = year % 1000;
						}
					}
				}
			}
			if (everydayUpdateList.Count > 0) {
				int i =0;
				while (i < everydayUpdateList.Count) {
					if (everydayUpdateList[i] == null) {everydayUpdateList.RemoveAt(i); continue;}
					else {
						everydayUpdateList[i].SendMessage("EverydayUpdate", SendMessageOptions.DontRequireReceiver);
						i++;
					}
				}
			}
			if (everyYearUpdateList.Count > 0) {
				int i = 0;
				while (i < everyYearUpdateList.Count) {
					if (everyYearUpdateList[i] == null) {everyYearUpdateList.RemoveAt(i); continue;}
					else {
						everyYearUpdateList[i].SendMessage("EveryYearUpdate", SendMessageOptions.DontRequireReceiver);
						i++;
					}
				}
			}
		}


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
	public static void Save2DMatrix (bool[,] arr, string fileName ) {
		// Не совсем правильный вывод, поправить
		using (StreamWriter sw = File.CreateText(path + '/' + fileName+".txt")) 
		{
			for (int i = arr.GetLength(1) - 1; i >= 0; i--) {
				string s = "";
				for (int j = 0; j< arr.GetLength(0); j++) {
					if (arr[i,j] == true) s+=" 1"; else s+=" 0";
					}
				sw.WriteLine(s);
				}
			}
		}	

	void OnGUI() {
		if (!fontSize_set) {
			GUI.skin.GetStyle("Label").fontSize = 27;
			fontSize_set = true;
		}
		GUI.Label(new Rect(Screen.width - 128, 0, 128,32), "day "+day.ToString());
		GUI.Label(new Rect(Screen.width - 128, 32, 128,32), "week "+week.ToString());
		GUI.Label(new Rect(Screen.width - 128, 64, 128,32), "month "+month.ToString());
		GUI.Label(new Rect(Screen.width - 128, 96, 128,32), "year "+year.ToString());
	}
}

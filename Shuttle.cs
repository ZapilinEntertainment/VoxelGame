using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShipStatus {InPort, OnMission}

public class Shuttle : MonoBehaviour {
	const float START_VOLUME = 20, STANDART_FUEL_CAPACITY = 100;
    public const float STANDART_COST = 700;
	public float volume{get;private set;}
	public float cost{get;private set;}
    public float maintenance { get; private set; }
	public float fuelReserves{get;private set;}
	public float fuelCapacity{get;private set;}
	public int ID{get;private set;}
	public float condition{get;private set;} // общее состояние
#pragma warning disable 0649
    [SerializeField] Renderer[] renderers;
#pragma warning restore 0649
    public Crew crew{get; private set;}
	public Hangar hangar{get;private set;}
	public ShipStatus status{get;private set;}
	public Expedition assignedToExpedition{get;private set;}

	public static List<Shuttle> shuttlesList;
	public static int lastIndex{get;private set;}

    static Shuttle()
    {
        shuttlesList = new List<Shuttle>();
    }

	public static void Reset() {
		shuttlesList = new List<Shuttle>();
		lastIndex = 0;
	}
    public static void PrepareList()
    {
        if (shuttlesList == null) shuttlesList = new List<Shuttle>();
    }

    public void FirstSet(Hangar h) {
		hangar = h;
		transform.position = hangar.transform.position;
		foreach (Renderer r in renderers) {
			r.enabled = false;
		}
		status = ShipStatus.InPort;
		name = Localization.NameShuttle();
		ID = lastIndex; lastIndex ++;
		volume = START_VOLUME;
		condition = 1;
		cost =STANDART_COST;
        maintenance = cost / 10f;
		fuelCapacity = STANDART_FUEL_CAPACITY;
		fuelReserves = GameMaster.realMaster.colonyController.storage.GetResources(ResourceType.Fuel, fuelCapacity);
		shuttlesList.Add(this);
	}

	public void RepairForResources() {}
	public void RepairForCoins() {
        if (condition == 1) return;
        float repairCost = (1 - condition) * cost;
        float availableSum = GameMaster.realMaster.colonyController.GetEnergyCrystals(repairCost);
        condition += availableSum / repairCost * (1 - condition);
        UIController.current.MakeAnnouncement(Localization.GetPhrase(LocalizedPhrase.ShuttleRepaired) + ", " + Localization.GetWord(LocalizedWord.Price) + ": " + string.Format("{0:0.##}", availableSum));
    }
	public void Refuel() {
		float shortage = fuelCapacity - fuelReserves;
		if (shortage > 0) {
			fuelReserves += GameMaster.realMaster.colonyController.storage.GetResources(ResourceType.Fuel, shortage);
		}
	}

	public bool AttributeCrew(Crew c) {
        if (crew != null) return false;
        else
        {
            crew = c;
            return true;
        }
	}
    public void UnattributeCrew(Crew c)
    {
        if (crew == c) crew = null;
    }

	public static Shuttle GetShuttle( int id ) {
		if (shuttlesList.Count == 0) return null;
		else {
			int i = 0;
			while (i < shuttlesList.Count) {
				if (shuttlesList[i] == null) {
					shuttlesList.RemoveAt(i);
					continue;
				}
				else {
					if (shuttlesList[i].ID == id) return shuttlesList[i];
				}
				i++;
			}
			return null;
		}
	}

	// used only by Expedition class, use expedition.AssignShuttle
	public void AssignTo(Expedition e) {
		assignedToExpedition = e;
	}

    public void DrawShuttleIcon(UnityEngine.UI.RawImage ri)
    {
        ri.texture = UIController.current.iconsTexture;
        ri.uvRect = UIController.GetTextureUV((condition < 0.5f) ? Icons.ShuttleBadIcon : ((condition > 0.85f) ? Icons.ShuttleGoodIcon : Icons.ShuttleNormalIcon));
    }

    /// <summary>
    /// use only from hangar.deconstructShuttle
    /// </summary>
    public void Deconstruct()
    {
        float pc = GameMaster.realMaster.demolitionLossesPercent;
        if (pc != 1) {
            ResourceContainer[] compensation = ResourcesCost.GetCost(ResourcesCost.SHUTTLE_BUILD_COST_ID);
            Storage s = GameMaster.realMaster.colonyController.storage;
            for (int i = 0; i < compensation.Length; i++)
            {
                s.AddResource(compensation[i].type, compensation[i].volume * GameMaster.realMaster.demolitionLossesPercent);
            }
            GameMaster.realMaster.colonyController.AddEnergyCrystals(cost * pc);
         }
        if (status == ShipStatus.InPort)
        {
            shuttlesList.Remove(this);
        }
        if (crew != null)
        {
            Crew c = crew;
            crew = null;
            c.Dismiss();
        }
    }

    public void Disappear() // исчезновение
    {
        if (crew != null) crew.Disappear();
        if (status == ShipStatus.InPort)
        {
            shuttlesList.Remove(this);            
        }
    }

    #region save-load system
    public static ShuttleStaticSerializer SaveStaticData() {
		ShuttleStaticSerializer s3 = new ShuttleStaticSerializer();
		s3.shuttlesList = new List<ShuttleSerializer>();
		s3.haveShuttles = false;
		if (shuttlesList != null && shuttlesList.Count > 0) {
			int i =0;
			while (i < shuttlesList.Count) {
				if (shuttlesList[i] == null) {
					shuttlesList.RemoveAt(i);
					continue;
				}
				else {
					s3.shuttlesList.Add(shuttlesList[i].Save());
					i++;
				}
			}
			if (s3.shuttlesList.Count > 0) s3.haveShuttles = true;
		}
		s3.lastIndex = lastIndex;
		return s3;
	}
	public static void LoadStaticData(ShuttleStaticSerializer s3) {
		lastIndex = s3.lastIndex;
		shuttlesList = new List<Shuttle>();
		if (s3.haveShuttles) {
			foreach (ShuttleSerializer ss in s3.shuttlesList) {
				Shuttle s = Instantiate(Resources.Load<GameObject>("Prefs/shuttle")).GetComponent<Shuttle>();
				s.Load(ss);
				shuttlesList.Add(s);
			}
		}
	}

	public ShuttleSerializer Save() {
		ShuttleSerializer ss = new ShuttleSerializer();
		ss.volume = volume;
		ss.cost = cost;
		ss.fuelReserves = fuelReserves;
		ss.fuelCapacity = fuelCapacity;
		ss.condition = condition;
		ss.ID = ID; 
		ss.status = status;
		ss.name = name;
		ss.xpos = transform.position.x; ss.ypos = transform.position.y;ss.zpos=transform.position.z;
		ss.xrotation = transform.rotation.x;ss.yrotation = transform.rotation.y;ss.zrotation = transform.rotation.z;ss.wrotation = transform.rotation.w;
		return ss;
	}
	public void Load(ShuttleSerializer ss) {
		volume =ss.volume;
		cost = ss.cost;
		fuelCapacity = ss.fuelCapacity;
		fuelReserves = ss.fuelReserves;
		condition = ss.condition;
		ID = ss.ID;
		status =ss.status;
		name = ss.name;
		transform.position = new Vector3(ss.xpos,ss.ypos,ss.zpos);
		transform.rotation = new Quaternion(ss.xrotation, ss.yrotation,ss.zrotation,ss.wrotation);
	}
	#endregion 
}

[System.Serializable]
public class ShuttleSerializer {
	public float volume, cost,fuelReserves, fuelCapacity, condition;
	public float xpos,ypos,zpos,xrotation,yrotation,zrotation,wrotation;
	public int ID;
	public ShipStatus status;
	public string name;
}
[System.Serializable]
public class ShuttleStaticSerializer {
	public bool haveShuttles;
	public List<ShuttleSerializer> shuttlesList;
	public int lastIndex;
}

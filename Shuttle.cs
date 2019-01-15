using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShipStatus : byte {Docked, OnMission} // при изменении дополнить Localization.GetShuttleStatus

public sealed class Shuttle : MonoBehaviour {
    public const float GOOD_CONDITION_THRESHOLD = 0.85f, BAD_CONDITION_THRESHOLD = 0.5f;
	private const float START_VOLUME = 20;

    public const float STANDART_COST = 700;
	public float volume{get;private set;}
	public float cost{get;private set;}
	public int ID{get;private set;}
    public float condition = 1; // общее состояние, автоматически чинится в работающих ангарах
#pragma warning disable 0649
    [SerializeField] Renderer[] renderers;
#pragma warning restore 0649
    public Crew crew{get; private set;}
	public Hangar hangar{get;private set;}
	public ShipStatus status{get;private set;}
	public Expedition assignedToExpedition{get;private set;}

	public static List<Shuttle> shuttlesList;
	public static int lastIndex{get;private set;}
    public static int actionsHash { get; private set; }

    static Shuttle()
    {
        shuttlesList = new List<Shuttle>();
    }

	public static void Reset() {
		shuttlesList = new List<Shuttle>();
		lastIndex = 0;
        actionsHash = 0;
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
		status = ShipStatus.Docked;
		name = Localization.NameShuttle();
		ID = lastIndex; lastIndex ++;
		volume = START_VOLUME;
		condition = 1;
		cost = STANDART_COST;
		shuttlesList.Add(this);
	}
    public void AssignToHangar(Hangar h)
    {
        hangar = h;
    }

	public void SetCrew(Crew c) {
        crew = c;
        actionsHash++;
	}
    public void SetVisibility(bool x)
    {
        foreach (Renderer r in renderers) r.enabled = x;
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
        if (e != null)
        {
            assignedToExpedition = e;
            e.AddParticipant(this);
        }
        else
        {
            if (assignedToExpedition != null) assignedToExpedition.RemoveParticipant(this);
            assignedToExpedition = null;
        }
        actionsHash++;
	}

    public void DrawShuttleIcon(UnityEngine.UI.RawImage ri)
    {
        ri.texture = UIController.current.iconsTexture;
        Icons chosenIcon;
        if (condition > GOOD_CONDITION_THRESHOLD) chosenIcon = Icons.ShuttleGoodIcon;
        else
        {
            if (condition < BAD_CONDITION_THRESHOLD) chosenIcon = Icons.ShuttleBadIcon;
            else chosenIcon = Icons.ShuttleNormalIcon;
        }
        ri.uvRect = UIController.GetTextureUV(chosenIcon);
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
        if (status == ShipStatus.Docked)
        {
            shuttlesList.Remove(this);
        }
        if (crew != null)
        {
            Crew c = crew;
            crew = null;
            c.Dismiss();
        }
        actionsHash++;
    }

    public void Disappear() // исчезновение
    {
        if (crew != null) crew.Disappear();
        if (status == ShipStatus.Docked)
        {
            shuttlesList.Remove(this);            
        }
        actionsHash++;
    }

    #region save-load system
    public static void SaveStaticData( System.IO.FileStream fs) {
        int shuttlesCount = shuttlesList != null ? shuttlesList.Count : 0;
        var data = new List<byte>();
        if (shuttlesCount > 0)
        {
            shuttlesCount = 0;            
            while (shuttlesCount < shuttlesList.Count)
            {
                if (shuttlesList[shuttlesCount] == null)
                {
                    shuttlesList.RemoveAt(shuttlesCount);
                    continue;
                }
                else
                {
                    data.AddRange(shuttlesList[shuttlesCount].Save());
                    shuttlesCount++;
                }
            }
        }
        fs.Write(System.BitConverter.GetBytes(shuttlesCount),0,4);
        if (shuttlesCount > 0) {
            var dataArray = data.ToArray();
            if (shuttlesCount > 0) fs.Write(dataArray, 0, dataArray.Length);
        }
        fs.Write(System.BitConverter.GetBytes(lastIndex), 0, 4);
    }
	public static void LoadStaticData(System.IO.FileStream fs) {
        var data = new byte[4];
        fs.Read(data, 0, 4);
        int shuttlesCount = System.BitConverter.ToInt32(data,0);

        shuttlesList = new List<Shuttle>();
        while (shuttlesCount > 0)
        {
            Shuttle s = Instantiate(Resources.Load<GameObject>("Prefs/shuttle")).GetComponent<Shuttle>();
            s.Load(fs);
            shuttlesList.Add(s);
            shuttlesCount--;
        }
        fs.Read(data, 0, 4);
        lastIndex = System.BitConverter.ToInt32(data, 0);
	}

	public List<byte> Save() {
        var data = new List<byte>();
        data.AddRange(System.BitConverter.GetBytes(volume));
        data.AddRange(System.BitConverter.GetBytes(cost));
        data.AddRange(System.BitConverter.GetBytes(condition));
        data.AddRange(System.BitConverter.GetBytes(ID));
        data.Add((byte)status);
        var nameArray = System.Text.Encoding.Default.GetBytes(name);
        int count = nameArray.Length;
        data.AddRange(System.BitConverter.GetBytes(count)); // количество байтов, не длина строки
        if (count > 0) data.AddRange(nameArray);

        Transform t = transform;
        data.AddRange(System.BitConverter.GetBytes(t.position.x));
        data.AddRange(System.BitConverter.GetBytes(t.position.y));
        data.AddRange(System.BitConverter.GetBytes(t.position.z));

        data.AddRange(System.BitConverter.GetBytes(t.rotation.x));
        data.AddRange(System.BitConverter.GetBytes(t.rotation.y));
        data.AddRange(System.BitConverter.GetBytes(t.rotation.z));
        data.AddRange(System.BitConverter.GetBytes(t.rotation.w));

        return data;
	}
	public void Load(System.IO.FileStream fs) {
        var data = new byte[21];
        fs.Read(data, 0, 21);
        volume = System.BitConverter.ToSingle(data, 0);
        cost = System.BitConverter.ToSingle(data, 4);
        condition = System.BitConverter.ToSingle(data, 8);
        ID = System.BitConverter.ToInt32(data, 12);
        status = (ShipStatus)data[16];

        int bytesCount = System.BitConverter.ToInt32(data, 17); //выдаст количество байтов, не длину строки
        data = new byte[bytesCount];
        fs.Read(data, 0, bytesCount);
        if (bytesCount > 0)
        {
            System.Text.Decoder d = System.Text.Encoding.Default.GetDecoder();
            var chars = new char[d.GetCharCount(data, 0, bytesCount)];
            d.GetChars(data, 0, bytesCount,chars,0,true);
            name = new string(chars);
        }

        data = new byte[28];
        fs.Read(data, 0, 28);
        transform.position = new Vector3(
            System.BitConverter.ToSingle(data, 0),
            System.BitConverter.ToSingle(data, 4),
            System.BitConverter.ToSingle(data, 8)
            );
        transform.rotation = new Quaternion(
            System.BitConverter.ToSingle(data, 12),
            System.BitConverter.ToSingle(data, 16),
            System.BitConverter.ToSingle(data, 20),
            System.BitConverter.ToSingle(data, 24)
            );
	}
	#endregion 
}

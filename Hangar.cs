using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public sealed class HangarSerializer {
	public WorkBuildingSerializer workBuildingSerializer;
	public bool constructing;
	public int shuttle_id;
}

public sealed class Hangar : WorkBuilding {
	static int hangarsCount = 0;
	public Shuttle shuttle{get; private set;}
	const float CREW_HIRE_BASE_COST = 100;
    public bool constructing = false;
    public static UIHangarObserver hangarObserver;

	public static void Reset() {
		hangarsCount = 0;
	}

	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetBuildingData(b, pos);
		Transform meshTransform = transform.GetChild(0);
		if (basement.pos.z == 0) {
			meshTransform.transform.localRotation = Quaternion.Euler(0, 180,0); 
		}
		else {
			if (basement.pos.z != Chunk.CHUNK_SIZE - 1) {
				if (basement.pos.x == 0) {
					meshTransform.transform.localRotation = Quaternion.Euler(0, -90,0); 
				}
				else {
					if (basement.pos.x == Chunk.CHUNK_SIZE - 1) {
						meshTransform.transform.localRotation = Quaternion.Euler(0, 90,0);
					}
				}
			}
		}
		hangarsCount++;
		if (hangarsCount == 1) ExpeditionCorpus.InitializeQuest(Quest.FIRST_COMMUNICATOR_SET_ID);
	}

	void Update() {
		if (GameMaster.gameSpeed == 0 || !isActive || !energySupplied) return;
		if ( constructing & workersCount > 0 & shuttle == null) {
			workflow += workSpeed * Time.deltaTime * GameMaster.gameSpeed ;
			if (workflow >= workflowToProcess) {
				LabourResult();
			}
		}
	}

	override protected void LabourResult() {
		shuttle = Instantiate(Resources.Load<GameObject>("Prefs/shuttle")).GetComponent<Shuttle>();
		shuttle.FirstSet(this);
		constructing = false;
		workflow -= workflowToProcess;
        if (showOnGUI)
        {
            hangarObserver.PrepareHangarWindow();
        }
        UIController.current.MakeAnnouncement(Localization.GetPhrase(LocalizedPhrase.ShuttleConstructed));
	}

	override protected void RecalculateWorkspeed() {
		workSpeed = GameMaster.CalculateWorkspeed(workersCount, WorkType.MachineConstructing);
	}

    public override UIObserver ShowOnGUI()
    {
        if (hangarObserver == null) hangarObserver = UIHangarObserver.InitializeHangarObserverScript() ;
        else hangarObserver.gameObject.SetActive(true);
        hangarObserver.SetObservingHangar(this);
        showOnGUI = true;
        return hangarObserver;
    }

    public void DeconstructShuttle()
    {
        if (shuttle == null) return;
        else
        {
            shuttle.Deconstruct();
            shuttle = null;
            hangarObserver.PrepareHangarWindow();
        }
    }

    #region save-load system
    override public StructureSerializer Save() {
		StructureSerializer ss = GetStructureSerializer();
		using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
		{
			new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(stream, GetHangarSerializer());
			ss.specificData =  stream.ToArray();
		}
		return ss;
	}

	override public void Load(StructureSerializer ss, SurfaceBlock sblock) {
		LoadStructureData(ss, sblock);
		HangarSerializer hs = new HangarSerializer();
		GameMaster.DeserializeByteArray<HangarSerializer>(ss.specificData, ref hs);
		constructing = hs.constructing;
		LoadWorkBuildingData(hs.workBuildingSerializer);
		shuttle = Shuttle.GetShuttle(hs.shuttle_id);
	}

	HangarSerializer GetHangarSerializer() {
		HangarSerializer hs = new HangarSerializer();
		hs.workBuildingSerializer = GetWorkBuildingSerializer();
		hs.constructing = constructing;
		hs.shuttle_id = (shuttle == null ? -1 : shuttle.ID);
		return hs;
	}
	#endregion

    override public void Annihilate(bool forced)
    {
        if (forced) { UnsetBasement(); }
        PrepareWorkbuildingForDestruction();
        hangarsCount--;
        if (shuttle != null) shuttle.Deconstruct();
        Destroy(gameObject);
    }
}

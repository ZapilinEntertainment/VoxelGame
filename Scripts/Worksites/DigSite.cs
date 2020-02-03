using UnityEngine;
using System.Collections.Generic;

public class DigSite : Worksite {
	public bool dig = true;
	BlockExtension workObject;
    const int START_WORKERS_COUNT = 10;

    override public int GetMaxWorkers() { return 64; }

    public DigSite(Plane i_plane, bool work_is_dig) : base (i_plane)
    {
        dig = work_is_dig;
		if (dig) {
			sign = Object.Instantiate(Resources.Load<GameObject> ("Prefs/DigSign")).GetComponent<WorksiteSign>(); 
		}
		else sign = Object.Instantiate(Resources.Load<GameObject>("Prefs/PourInSign")).GetComponent<WorksiteSign>();
		sign.worksite = this;

        workObject = workplace.myBlockExtension;
        switch (workplace.faceIndex)
        {
            case Block.FWD_FACE_INDEX: sign.transform.position = workplace.pos.ToWorldSpace() + Vector3.forward * Block.QUAD_SIZE * 0.5f; break;
            case Block.RIGHT_FACE_INDEX: sign.transform.position = workplace.pos.ToWorldSpace() + Vector3.right * Block.QUAD_SIZE * 0.5f; break;
            case Block.BACK_FACE_INDEX: sign.transform.position = workplace.pos.ToWorldSpace() + Vector3.back * Block.QUAD_SIZE * 0.5f; break;
            case Block.LEFT_FACE_INDEX: sign.transform.position = workplace.pos.ToWorldSpace() + Vector3.left * Block.QUAD_SIZE * 0.5f; break;
            case Block.UP_FACE_INDEX: sign.transform.position = workplace.pos.ToWorldSpace() + Vector3.up * Block.QUAD_SIZE * 0.5f; break;
            case Block.DOWN_FACE_INDEX: sign.transform.position = workplace.pos.ToWorldSpace() + Vector3.down * Block.QUAD_SIZE * 0.5f; break;
            case Block.SURFACE_FACE_INDEX: sign.transform.position = workplace.pos.ToWorldSpace() + Vector3.up * Block.QUAD_SIZE * 0.5f; break;
            case Block.CEILING_FACE_INDEX: sign.transform.position = workplace.pos.ToWorldSpace() + Vector3.down * Block.QUAD_SIZE * 0.5f; break;
        }

        int wom = workObject.materialID;
        if (workplace.materialID != wom) workplace.ChangeMaterial(wom, true);
		if (workersCount < START_WORKERS_COUNT) colony.SendWorkers(START_WORKERS_COUNT, this);
        worksitesList.Add(this);
        GameMaster.realMaster.labourUpdateEvent += WorkUpdate;
        subscribedToUpdate = true;
    }

    override public void WorkUpdate () {
		if (workersCount > 0) {
			workflow += workSpeed ;
            colony.gears_coefficient -= gearsDamage;
			if (workflow >= 1f) LabourResult();
		}
	}

    void LabourResult()
    {
        int x = (int)workflow;
        float production = x;
        if (dig)
        {
            production = workObject.Dig(x, true, workplace.faceIndex);
            if (production == 0f)
            {
                StopWork();
                return;
            }
        }
        else
        {
            production = workObject.Dig(x, true, workplace.faceIndex);
            if (production != 0)
            {
                production = workObject.PourIn((int)production, workplace.faceIndex);
                if (production == 0) { StopWork(); return; }
            }
        }
        workflow -= production;
        if (dig)
        {
            actionLabel = Localization.GetActionLabel(LocalizationActionLabels.DigInProgress) + " (" + ((int)((1 - workObject.GetVolumePercent()) * 100f)).ToString() + "%)";
        }
        else
        {
            actionLabel = Localization.GetActionLabel(LocalizationActionLabels.PouringInProgress) + " (" + ((int)(workObject.GetVolumePercent() * 100f)).ToString() + "%)";
        }
    }

    protected override void RecalculateWorkspeed() {
        workSpeed = colony.labourCoefficient * workersCount * GameConstants.DIGGING_SPEED;
        gearsDamage = GameConstants.WORKSITES_GEARS_DAMAGE_COEFFICIENT * workSpeed;
	}

    override public void StopWork()
    {
        if (destroyed) return;
        else destroyed = true;
        if (workersCount > 0)
        {
            colony.AddWorkers(workersCount);
            workersCount = 0;
        }
        if (sign != null)
        {
           // FollowingCamera.main.cameraChangedEvent -= SignCameraUpdate;
            MonoBehaviour.Destroy(sign.gameObject);
        }
        /*
        if (workObject != null)
        {            
            if ( workObject.excavatingStatus == 0) workObject.myChunk.AddBlock(new ChunkPos(workObject.pos.x, workObject.pos.y + 1, workObject.pos.z), BlockType.Surface, workObject.material_id, false);
            if (workObject.worksite == this) workObject.ResetWorksite();
        }        
        */
        if (subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent -= WorkUpdate;
            subscribedToUpdate = false;
        }
        if (showOnGUI)
        {
            observer.SelfShutOff();
            showOnGUI = false;
            //if (workObject != null) UIController.current.ChangeChosenObject(workObject);
            //else UIController.current.ChangeChosenObject(ChosenObjectType.None);
        }
        if (worksitesList.Contains(this)) worksitesList.Remove(this);
    }

    #region save-load system
    override protected List<byte> Save() {
		if (workObject == null) {
            StopWork();
			return null;
		}
        var pos = workplace.pos;
        var data = new List<byte>() {
            (byte)WorksiteType.DigSite,
            pos.x, pos.y, pos.z, workplace.faceIndex, dig ? (byte)1 : (byte)0
        };
        data.AddRange(SerializeWorksite());
		return data;
	}
	public static DigSite Load(System.IO.FileStream fs, Chunk chunk)
    {
        var data = new byte[5];
        fs.Read(data, 0, data.Length);
        Plane plane = null;
        if (chunk.GetBlock(data[0], data[1], data[2])?.TryGetPlane(data[3], out plane) == true)
        {
            var cs = new DigSite(plane, data[4] == 1);
            cs.LoadWorksiteData(fs);
            return cs;
        }
        else
        {
            Debug.Log("digsite load error");
            return null;
        }
    }
	#endregion
}

using UnityEngine;
using System.Collections.Generic;

public class TunnelBuildingSite : Worksite {
	public byte signsMask = 0;
	CubeBlock workObject;
	const int START_WORKERS_COUNT = 10;
    // public const int MAX_WORKERS = 32


    override public void WorkUpdate () {
		if (GameMaster.gameSpeed == 0) return;
        if (workObject == null)
        {
            StopWork();
            return;
        }
        else
        {
            if (workersCount > 0)
            {
                workflow += workSpeed;
                colony.gears_coefficient -= gearsDamage;
                if (workflow >= 1) {
                    // labour result
                    int x = (int)workflow;
                    float production = x;
                    production = workObject.Dig(x, false);
                    if (workObject == null)
                    {
                        StopWork();
                        return;
                    }
                    GameMaster.geologyModule.CalculateOutput(production, workObject, GameMaster.realMaster.colonyController.storage);
                    workflow -= production;
                    actionLabel = Localization.GetActionLabel(LocalizationActionLabels.DigInProgress) + " (" + ((int)(( 1 - ((float)workObject.volume / (float)CubeBlock.MAX_VOLUME)) * 100)).ToString() + "%)";
                }
            }
        }
		}

	protected override void RecalculateWorkspeed() {
		workSpeed = colony.labourCoefficient * workersCount * GameConstants.DIGGING_SPEED;
        gearsDamage = GameConstants.WORKSITES_GEARS_DAMAGE_COEFFICIENT * workSpeed;        
	}

	public void Set(CubeBlock block) {
		workObject = block;
        workObject.SetWorksite(this);
		colony.SendWorkers(START_WORKERS_COUNT, this);
        if (!worksitesList.Contains(this)) worksitesList.Add(this);
        if (!subscribedToUpdate) {
            GameMaster.realMaster.labourUpdateEvent += WorkUpdate;
            subscribedToUpdate = true;
        }
    }

	public void CreateSign(byte side) {
		if ((signsMask & side) != 0) return;
		switch (side) {
		case 0:
				sign = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Prefs/tunnelBuildingSign")).GetComponent<WorksiteSign>();
				sign.transform.position = workObject.pos.ToWorldSpace() + Vector3.forward * Block.QUAD_SIZE / 2f;
				signsMask += 1;
			break;
		case 1:
				sign = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Prefs/tunnelBuildingSign")).GetComponent<WorksiteSign>();
			sign.transform.position = workObject.pos.ToWorldSpace() + Vector3.right * Block.QUAD_SIZE / 2f;
				sign.transform.rotation = Quaternion.Euler(0,90,0);
				signsMask += 2;
			break;
		case 2:
				sign = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Prefs/tunnelBuildingSign")).GetComponent<WorksiteSign>();
				sign.transform.position = workObject.pos.ToWorldSpace() + Vector3.back * Block.QUAD_SIZE / 2f;
				sign.transform.rotation = Quaternion.Euler(0,180,0);
				signsMask += 4;
			break;
		case 3:
				sign = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Prefs/tunnelBuildingSign")).GetComponent<WorksiteSign>();
				sign.transform.position = workObject.pos.ToWorldSpace() + Vector3.left * Block.QUAD_SIZE / 2f;
				sign.transform.rotation = Quaternion.Euler(0,-90,0);
				signsMask += 8;
			break;
		}
		if (sign != null) sign.worksite = this;
	}

    override public void StopWork()
    {
        if (destroyed) return;
        else destroyed = true;
        if (workersCount > 0)
        {
            GameMaster.realMaster.colonyController.AddWorkers(workersCount);
            workersCount = 0;
        }
        if (sign != null) MonoBehaviour.Destroy(sign.gameObject);        
        if (subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent -= WorkUpdate;
            subscribedToUpdate = false;
        }
        if (workObject != null)
        {
            if (workObject.worksite == this) workObject.ResetWorksite();
            workObject = null;
        }
        if (showOnGUI)
        {
            observer.SelfShutOff();
            showOnGUI = false;
        }
        if (worksitesList.Contains(this)) worksitesList.Remove(this);
    }

    #region save-load system
    override protected List<byte> Save() {
		if (workObject == null) {
            StopWork();
			return null;
		}
        var data = new List<byte>() { (byte)WorksiteType.TunnelBuildingSite };
        data.Add(workObject.pos.x);
        data.Add(workObject.pos.y);
        data.Add(workObject.pos.z);
        data.Add(signsMask);
        data.AddRange(SerializeWorksite());
        return data;
	}
	override protected void Load (System.IO.FileStream fs, ChunkPos pos) {
        Set(GameMaster.realMaster.mainChunk.GetBlock(pos) as CubeBlock);
        int smask = fs.ReadByte();
        if ((smask & 1) != 0) CreateSign(0);
        if ((smask & 2) != 0) CreateSign(1);
        if ((smask & 4) != 0) CreateSign(2);
        if ((smask & 8) != 0) CreateSign(3);
        LoadWorksiteData(fs);        
	}
	#endregion
}

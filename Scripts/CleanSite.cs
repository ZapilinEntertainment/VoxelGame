using UnityEngine;
using System.Collections.Generic;

public class CleanSite : Worksite {
	public bool diggingMission {get;protected set;}
	SurfaceBlock workObject;
	const int START_WORKERS_COUNT = 10; 
    // public const int MAX_WORKERS = 32

	override public void WorkUpdate () {
		if (workObject == null) {
            StopWork();
			return;
		}
		if (workObject.structures.Count == 0) {
            int x = workObject.pos.x, y = workObject.pos.y, z = workObject.pos.z;
            Chunk ch = workObject.myChunk;			
            destroyed = true; // чтобы скрипт игнорировал StopWork при удалении блока
            if (workObject.type != BlockType.Cave) ch.DeleteBlock(workObject.pos);
            else (workObject as CaveBlock).DestroySurface();
            workObject = null;
            destroyed = false; // включаем обратно, чтобы удаление прошло нормально           
            

            if (diggingMission) {
				Block basement = ch.GetBlock(x, y - 1, z);
                if (basement == null || basement.type != BlockType.Cube)
                {
                    StopWork();
                }
                else
                {                    
                    DigSite ds = new DigSite();
                    TransferWorkers(this, ds);
                    ds.Set(basement as CubeBlock, true);
                    if (showOnGUI) { ds.ShowOnGUI(); showOnGUI = false; }
                    StopWork();
                }
			}            
            return;
		}		
        else
        {
            workflow += workSpeed;
            colony.gears_coefficient -= gearsDamage;
            Structure s = workObject.structures[0];
            if (s == null)
            {
                workObject.RecalculateSurface();
                return;
            }
            float workGained = 0;
            if (s.ID == Structure.PLANT_ID)
            {
                workGained = s.hp;
                (s as Plant).Harvest();                
            }
            else
            {
                HarvestableResource hr = s as HarvestableResource;
                if (hr != null)
                {
                    workGained = hr.resourceCount;
                    hr.Harvest();
                }
                else
                {
                    s.ApplyDamage(workflow);
                    workGained = workflow;
                }
            }
            workflow -= workGained;
            actionLabel = Localization.GetActionLabel(LocalizationActionLabels.CleanInProgress) + " (" +Localization.GetPhrase(LocalizedPhrase.ObjectsLeft) +" :" + workObject.structures.Count.ToString()  +  ")";
        }		
	}

	protected override void RecalculateWorkspeed() {
        workSpeed = colony.labourCoefficient * workersCount * GameConstants.CLEARING_SPEED;
        gearsDamage = GameConstants.WORKSITES_GEARS_DAMAGE_COEFFICIENT * workSpeed;
	}

	public void Set(SurfaceBlock block, bool f_diggingMission) {
		workObject = block;
        workObject.SetWorksite(this);
		if (block.grassland != null) block.grassland.Annihilation(true, false); 
		if (sign == null) sign = MonoBehaviour.Instantiate(Resources.Load<GameObject> ("Prefs/ClearSign")).GetComponent<WorksiteSign>();
		sign.worksite = this;
		sign.transform.position = workObject.pos.ToWorldSpace() + Vector3.up * 0.5f * Block.QUAD_SIZE;
        //FollowingCamera.main.cameraChangedEvent += SignCameraUpdate;

        diggingMission = f_diggingMission;
        colony = GameMaster.realMaster.colonyController;
		if (workersCount < START_WORKERS_COUNT) colony.SendWorkers(START_WORKERS_COUNT, this);
        if (!worksitesList.Contains(this)) worksitesList.Add(this);
        if (!subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent += WorkUpdate;
            subscribedToUpdate = true;
        }
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
          //  FollowingCamera.main.cameraChangedEvent -= SignCameraUpdate;
            MonoBehaviour.Destroy(sign.gameObject);
        }        
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
            if (observer.observingWorksite == this)
            {
                observer.SelfShutOff();
                UIController.current.ChangeChosenObject(ChosenObjectType.Surface);
            }
            showOnGUI = false;
        }
        if (worksitesList.Contains(this)) worksitesList.Remove(this);
    }

    #region save-load mission
    override protected List<byte> Save() {
		if (workObject == null) {
            StopWork();
			return null;
		}
        var data = new List<byte>() { (byte)WorksiteType.CleanSite };
        data.Add(workObject.pos.x);
        data.Add(workObject.pos.y);
        data.Add(workObject.pos.z);
        data.Add(diggingMission ? (byte)1 : (byte)0);
        data.AddRange(SerializeWorksite());
		return data;
	}
	override protected void Load(System.IO.FileStream fs, ChunkPos cpos) {
        Set(
            GameMaster.realMaster.mainChunk.GetBlock(cpos) as SurfaceBlock, 
            fs.ReadByte() == 1
            );
        LoadWorksiteData(fs);
	}
	#endregion
			
}

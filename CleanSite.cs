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
		if (workObject.surfaceObjects.Count == 0) {
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
                    DigSite ds = basement.gameObject.AddComponent<DigSite>();
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
            Structure s = workObject.surfaceObjects[0];
            if (s == null)
            {
                workObject.RecalculateSurface();
                return;
            }
            float workGained = 0;
            if (s.id == Structure.PLANT_ID)
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
            actionLabel = Localization.GetActionLabel(LocalizationActionLabels.CleanInProgress) + " (" + workObject.surfaceObjects.Count.ToString() + ' ' + Localization.GetPhrase(LocalizedPhrase.ObjectsLeft) + ")";
        }		
	}

	protected override void RecalculateWorkspeed() {
		workSpeed = GameMaster.realMaster.CalculateWorkspeed(workersCount, WorkType.Clearing);
	}

	public void Set(SurfaceBlock block, bool f_diggingMission) {
		workObject = block;
        workObject.SetWorksite(this);
		if (block.grassland != null) block.grassland.Annihilation(true); 
		if (sign == null) sign = Instantiate(Resources.Load<GameObject> ("Prefs/ClearSign")).GetComponent<WorksiteSign>();
		sign.worksite = this;
		sign.transform.position = workObject.transform.position;
        //FollowingCamera.main.cameraChangedEvent += SignCameraUpdate;

        diggingMission = f_diggingMission;
		if (workersCount < START_WORKERS_COUNT) GameMaster.realMaster.colonyController.SendWorkers(START_WORKERS_COUNT, this);
        if (!worksitesList.Contains(this)) worksitesList.Add(this);
        if (!subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent += WorkUpdate;
            subscribedToUpdate = true;
        }
    }

   // public void SignCameraUpdate()
   // {
    //    sign.transform.LookAt(FollowingCamera.camPos);
    //}

    override public void StopWork()
    {
        if (destroyed) return;
        else destroyed = true;
        if (workersCount > 0)
        {
            GameMaster.realMaster.colonyController.AddWorkers(workersCount);
            workersCount = 0;
        }
        if (sign != null)
        {
          //  FollowingCamera.main.cameraChangedEvent -= SignCameraUpdate;
            Destroy(sign.gameObject);
        }
        if (worksitesList.Contains(this)) worksitesList.Remove(this);
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
        Destroy(this);
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
            transform.root.GetComponent<Chunk>().GetBlock(cpos) as SurfaceBlock, 
            fs.ReadByte() == 1
            );
        LoadWorksiteData(fs);
	}
	#endregion
			
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OakTree : Plant {
	static Sprite[] stageSprites;
	static Mesh trunk_stage4, trunk_stage5, trunk_stage6, crones_stage4, crones_stage5, crones_stage6;
	static GameObject container;
	static List<GameObject> treeBlanks;
    static LODController modelsLodController;
    static short oak4spritesIndex = -1, oak5spritesIndex = -1, oak6spritesIndex = -1;
	//const int MAX_INACTIVE_BUFFERED = 25;
	// myRenderers : 0 -sprite, 1 - crone, 2 - trunk

	const byte MAX_STAGE = 6;
	public const int CREATE_COST = 10, LUMBER = 100, FIRST_LIFEPOWER_TO_GROW = 10;
    float timer;

	void Awake() {
		if (container == null) {
			stageSprites = Resources.LoadAll<Sprite>("Textures/Plants/oakTree");
			container = new GameObject("oakTreesContainer");            

			treeBlanks = new List<GameObject>();
            modelsLodController = LODController.GetCurrent();
            GameObject trunkPref = LoadNewModel(4);
			crones_stage4 = trunkPref.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh;
			trunk_stage4 = trunkPref.transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh;
			trunkPref.SetActive(false);

			trunkPref = LoadNewModel(5);
			crones_stage5 = trunkPref.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh;
			trunk_stage5 = trunkPref.transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh;
			trunkPref.SetActive(false);

			trunkPref = LoadNewModel(6);
			crones_stage6 = trunkPref.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh;
			trunk_stage6 = trunkPref.transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh;
			trunkPref.SetActive(false);

			maxStage = MAX_STAGE;            
        }
        name = "Oak tree";
	}

	override public void Reset() {
		lifepower = CREATE_COST;
		lifepowerToGrow = FIRST_LIFEPOWER_TO_GROW;
		stage = 0;
		growth = 0;
		hp = maxHp;
		SetStage(0);
	}

	override public void Prepare() {		
		PrepareStructure();
		plant_ID = TREE_OAK_ID;
		innerPosition = SurfaceRect.one; isArtificial = false; 
		lifepower = CREATE_COST;
		lifepowerToGrow = FIRST_LIFEPOWER_TO_GROW;
		maxLifeTransfer = 10;
		growSpeed = 0.05f;
		decaySpeed = growSpeed;
		harvestableStage = 4;
		growth = 0;

		gameObject.name = "Tree - Oak";
		GameObject spriteSatellite = new GameObject("sprite");
		spriteSatellite.transform.parent = transform;
		spriteSatellite.transform.localPosition = Vector3.zero;
		GameMaster.realMaster.mastSpritesList.Add(spriteSatellite);
		myRenderers = new List<Renderer>();
		myRenderers.Add( spriteSatellite.AddComponent<SpriteRenderer>());
		(myRenderers[0] as SpriteRenderer).sprite = stageSprites[0];
	}

	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetStructureData(b,pos);
		if (myRenderers.Count > 1) {
			myRenderers[1].transform.parent.rotation = transform.rotation;
			myRenderers[1].transform.parent.position = transform.position;
		}
	}

    void Update()
    {
        float t = GameMaster.gameSpeed * Time.deltaTime;
        if (t == 0) return;

        timer -= t;

            float theoreticalGrowth = lifepower / lifepowerToGrow;
            if (growth < theoreticalGrowth)
            {
                growth = Mathf.MoveTowards(growth, theoreticalGrowth, growSpeed * t);
            }
            else
            {
                lifepower -= decaySpeed * t;
                if (lifepower == 0) Dry();
            }
        if (timer <= 0)
        {
            if (growth >= 1 & stage < maxStage)
            {
                byte nextStage = stage;
                nextStage++;
                if (CanGrowAround(nextStage)) SetStage(nextStage);
            }
            timer = stage;
        }
    }


    #region lifepower operations
    public override void AddLifepowerAndCalculate(int life) {
		lifepower += life;
		byte nstage = 0;
		float lpg = FIRST_LIFEPOWER_TO_GROW;
		while ( lifepower > lifepowerToGrow & nstage < maxStage) {
			nstage++;
			lpg = GetLifepowerLevelForStage(nstage);
		}
		lifepowerToGrow = lpg;
		SetStage(nstage);
	}
	public override int TakeLifepower(int life) {
		int lifeTransfer = life;
		if (life > lifepower) {if (lifepower >= 0) lifeTransfer = (int)lifepower; else lifeTransfer = 0;}
		lifepower -= lifeTransfer;
		return lifeTransfer;
	}
	override public void SetLifepower(float p) {
		lifepower = p; 
	}
	override public void SetGrowth(float t) {
		growth = t;
	}
	override public void SetStage( byte newStage) {
		if (newStage == stage) return;
		stage = newStage;
		if (stage < 4) {
			(myRenderers[0] as SpriteRenderer).sprite = stageSprites[stage];
			if (myRenderers.Count > 1) ReturnModelToPool();
		}
		else {			
			if (myRenderers.Count == 1) {
                myRenderers[0].enabled = false;
					GameObject myModel = GetModel(stage);
					myModel.transform.rotation = transform.rotation;
					myModel.transform.position = transform.position;
					myRenderers.Add(myModel.transform.GetChild(0).GetComponent<MeshRenderer>());
					myRenderers.Add(myModel.transform.GetChild(1).GetComponent<MeshRenderer>());
					myModel.SetActive(true);
			}
		}	
		lifepowerToGrow = GetLifepowerLevelForStage(stage);
		growth = lifepower/ lifepowerToGrow;
	}

	bool CanGrowAround( byte st) {
		//central cell : (rewrite if size % 2 == 0)
		SurfaceRect sr = new SurfaceRect((byte)(innerPosition.x + innerPosition.x_size / 2), (byte)(innerPosition.z + innerPosition.z_size / 2), innerPosition.x_size, innerPosition.z_size);
		switch (st) {
		case 0:
		case 1:
		case 2:
			sr = new SurfaceRect(sr.x, sr.z, 1,1);
			break;
		case 3:
		case 4:
			if (sr.x - 1 < 0 | sr.z - 1 < 0) return false;
			sr = new SurfaceRect((byte)(sr.x - 1), (byte)(sr.z - 1), 3,3);
			break;
		case 5:
			if (sr.x - 2 < 0 | sr.z - 2 < 0) return false;
			sr = new SurfaceRect((byte)(sr.x -2), (byte)(sr.z - 2), 5,5);
			break;
		case 6:
			if (sr.x - 3 < 0 | sr.z - 3 < 0) return false;
			sr = new SurfaceRect((byte)(sr.x - 3 ),(byte)(sr.z - 3), 7,7);
			break;
		}
		if (sr == innerPosition) return true;
		if (sr.x + sr.x_size > SurfaceBlock.INNER_RESOLUTION | sr.z_size + sr.z > SurfaceBlock.INNER_RESOLUTION)	return false;
		else {
			innerPosition = sr;
			return true;
		}
	}

	new static public float GetLifepowerLevelForStage(byte st) {
		switch (st) {
		default:
		case 0: return FIRST_LIFEPOWER_TO_GROW; // 10
		case 1: return FIRST_LIFEPOWER_TO_GROW * 2; // 20
		case 2: return FIRST_LIFEPOWER_TO_GROW * 4;// 40;
		case 3: return FIRST_LIFEPOWER_TO_GROW * 8;// 80 - full 3d tree
		case 4: return FIRST_LIFEPOWER_TO_GROW * 16;// 160
		case 5: return FIRST_LIFEPOWER_TO_GROW * 32;// 320 - max 
		}
	}
	#endregion

	override public void Harvest() {
		GameMaster.colonyController.storage.AddResource(ResourceType.Lumber, CountLumber());
		if (myRenderers.Count > 1) {
			FallingTree ft = myRenderers[1].transform.parent.gameObject.AddComponent<FallingTree>();
			ft.containList = treeBlanks;
			myRenderers.RemoveAt(2);
			myRenderers.RemoveAt(1);
		}
		Annihilate(false);
	}

	override public void Dry() {
		Destroy(gameObject);
		Structure s = Structure.GetNewStructure(Structure.CONTAINER_ID);
		GameObject g = GetModel(stage);
		Destroy(g.transform.GetChild(0).gameObject);
        MeshRenderer mr = g.transform.GetChild(1).GetComponent<MeshRenderer>();
        mr.sharedMaterial = PoolMaster.GetBasicMaterial(BasicMaterial.Lumber, mr.GetComponent<MeshFilter>(), true);
		g.transform.rotation = s.transform.rotation;
		g.transform.parent = s.transform;
		g.transform.localPosition = Vector3.zero;
		s.ChangeInnerPosition(innerPosition);
		s.SetBasement(basement, new PixelPosByte(innerPosition.x, innerPosition.z));
		//заменить на скрипт dead tree
		(s as HarvestableResource).SetResources(ResourceType.Lumber, CountLumber());
	}

	float CountLumber() {
		switch (stage) {
		default: return LUMBER/10f;
		case 4:return LUMBER/4f;
		case 5: return LUMBER/2f;
		case 6: return LUMBER;
		}
	}
	override public void SetVisibility( bool x) {
		if (x == visible) return;
		else {
			visible = x;
			foreach (Renderer r in myRenderers) {
				r.enabled = x;
			}
		}
	}

    override public void Annihilate(bool forced)
    {
        //print("oak annihilation");
        if (myRenderers.Count > 1) ReturnModelToPool();
        if (forced) { UnsetBasement(); }
        PreparePlantForDestruction();
        
        Destroy(gameObject);
    }

    public static GameObject GetModel(byte stage) {
		if (stage < 4 | stage > 6) stage = 4;
		GameObject model = null;
		int i =0;
		while (i < treeBlanks.Count) {
			model = treeBlanks[ i ];
			if (model == null) {
				treeBlanks.RemoveAt(i);
				continue;
			}
			else 	break;
		}
        if (model == null) { model = LoadNewModel(stage); i = 0; }
        short packIndex = -1;
        switch (stage)
        {
            case 4:
                model.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh = crones_stage4;
                model.transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh = trunk_stage4;
                packIndex = oak4spritesIndex;
                break;
            case 5:
                model.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh = crones_stage5;
                model.transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh = trunk_stage5;
                packIndex = oak5spritesIndex;
                break;
            case 6:
                model.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh = crones_stage6;
                model.transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh = trunk_stage6;
                packIndex = oak6spritesIndex;
                break;
        }
        modelsLodController.ChangeModelSpritePack(model.transform.GetChild(2), ModelType.Tree, packIndex);
		model.transform.parent = container.transform;
		treeBlanks.RemoveAt(i);
		return model;
	}

	static GameObject LoadNewModel( byte stage) {
		GameObject model = null;
		model = Instantiate(Resources.Load<GameObject>("Lifeforms/oak-" + stage.ToString()));
		if (model == null) print ("Error : oak model not loaded");
        else
        {
            short modelSpritePack = 0;
            switch (stage)
            {
                case 4:
                    if (oak4spritesIndex == -1)
                    {
                        Vector3[] positions = new Vector3[] { new Vector3(0, 0.222f, -0.48f), new Vector3(0, 0.479f, -0.434f), new Vector3(0, 0.458f, -0.232f), new Vector3(0, 0.551f, -0.074f) };
                        Vector3[] angles = new Vector3[] { Vector3.zero, new Vector3(30, 0, 0), new Vector3(45, 0, 0), new Vector3(75, 0, 0) };
                        Texture2D spritesAtlas = LODSpriteMaker.current.MakeSpriteLODs(model, positions, angles,0.25f, Color.green);
                        Sprite[] lodSprites = new Sprite[4];
                        int size = spritesAtlas.width / 2;

                        lodSprites[0] = Sprite.Create(spritesAtlas, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 128);
                        lodSprites[1] = Sprite.Create(spritesAtlas, new Rect(size, 0, size, size), new Vector2(0.5f, 0.5f), 128);
                        lodSprites[2] = Sprite.Create(spritesAtlas, new Rect(0, size, size, size), new Vector2(0.5f, 0.5f), 128);
                        lodSprites[3] = Sprite.Create(spritesAtlas, new Rect(size, size, size, size), new Vector2(0.5f, 0.5f), 128);
                        oak4spritesIndex = LODController.AddSpritePack(lodSprites);
                    }
                    modelSpritePack = oak4spritesIndex;
                    break;
                case 5:
                    if (oak5spritesIndex == -1)
                    {
                        Vector3[] positions = new Vector3[] { new Vector3(0, 0.222f, -0.48f), new Vector3(0, 0.479f, -0.434f), new Vector3(0, 0.458f, -0.232f), new Vector3(0, 0.551f, -0.074f) };
                        Vector3[] angles = new Vector3[] { Vector3.zero, new Vector3(30, 0, 0), new Vector3(45, 0, 0), new Vector3(75, 0, 0) };
                        Texture2D spritesAtlas = LODSpriteMaker.current.MakeSpriteLODs(model, positions, angles,0.25f, Color.green);
                        Sprite[] lodSprites = new Sprite[4];
                        int size = spritesAtlas.width / 2;

                        lodSprites[0] = Sprite.Create(spritesAtlas, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 128);
                        lodSprites[1] = Sprite.Create(spritesAtlas, new Rect(size, 0, size, size), new Vector2(0.5f, 0.5f), 128);
                        lodSprites[2] = Sprite.Create(spritesAtlas, new Rect(0, size, size, size), new Vector2(0.5f, 0.5f), 128);
                        lodSprites[3] = Sprite.Create(spritesAtlas, new Rect(size, size, size, size), new Vector2(0.5f, 0.5f), 128);
                        oak5spritesIndex = LODController.AddSpritePack(lodSprites);
                    }
                    modelSpritePack = oak5spritesIndex;
                    break;
                case 6:
                    if (oak6spritesIndex == -1)
                    {
                        Vector3[] positions = new Vector3[] { new Vector3(0, 0.222f, -0.48f), new Vector3(0, 0.479f, -0.434f), new Vector3(0, 0.458f, -0.232f), new Vector3(0, 0.551f, -0.074f) };
                        Vector3[] angles = new Vector3[] { Vector3.zero, new Vector3(30, 0, 0), new Vector3(45, 0, 0), new Vector3(75, 0, 0) };
                        Texture2D spritesAtlas = LODSpriteMaker.current.MakeSpriteLODs(model, positions, angles, 0.25f, Color.green);
                        Sprite[] lodSprites = new Sprite[4];
                        int size = spritesAtlas.width / 2;

                        lodSprites[0] = Sprite.Create(spritesAtlas, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 128);
                        lodSprites[1] = Sprite.Create(spritesAtlas, new Rect(size, 0, size, size), new Vector2(0.5f, 0.5f), 128);
                        lodSprites[2] = Sprite.Create(spritesAtlas, new Rect(0, size, size, size), new Vector2(0.5f, 0.5f), 128);
                        lodSprites[3] = Sprite.Create(spritesAtlas, new Rect(size, size, size, size), new Vector2(0.5f, 0.5f), 128);
                        oak6spritesIndex = LODController.AddSpritePack(lodSprites);
                    }
                    modelSpritePack = oak6spritesIndex;
                    break;
                }
            modelsLodController.AddObject(model.transform.GetChild(2), ModelType.Tree, modelSpritePack);
            treeBlanks.Add(model);
            model.transform.parent = container.transform;
        }               
		return model;
	}

	void ReturnModelToPool () {
		GameObject myModel = myRenderers[1].transform.parent.gameObject;
        if (myModel == null)  return; 
		treeBlanks.Add(myModel);
		myModel.SetActive(false);
		myRenderers.RemoveAt(2);
		myRenderers.RemoveAt(1);
	}

    private void OnDestroy() // правильное использование onDestroy
    {
        if ( !GameMaster.applicationStopWorking & myRenderers.Count > 1)
        {
            GameObject myModel = myRenderers[1].transform.parent.gameObject;
            treeBlanks.Add(myModel);
            myModel.SetActive(false);
        }
    }
}


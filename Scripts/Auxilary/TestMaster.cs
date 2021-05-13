using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TestMaster : MonoBehaviour
{
    [SerializeField] private GameObject replacingMatsHost;
    [SerializeField] private Transform facingObj;

    public Environment.EnvironmentPreset selectedPreset;
    private EnvironmentMaster emaster;


    public static void CreateCube(Vector3 point)
    {
        GameObject.CreatePrimitive(PrimitiveType.Cube).transform.position = point;
    }

    private void Awake()
    {
        emaster = GameMaster.realMaster.environmentMaster;
        selectedPreset = Environment.EnvironmentPreset.Default;    
        

    }

    private void Start()
    {
        //InstantiateHouses();
        if (replacingMatsHost != null) ReplaceMaterials(replacingMatsHost);
        emaster.TEST_SetEnvironment(Environment.EnvironmentPreset.FoundationSkies, true);
    }

    private void Update()
    {
        if (Input.GetKeyDown("h"))
        {
            // FollowingCamera.main.CameraToStartPosition();           
            //GameMaster.realMaster.globalMap.TEST_MakeNewPoint(MapMarkerType.Star);
            //UIController.GetCurrent().GetMainCanvasController().questUI.StartEndQuest((byte)Knowledge.ResearchRoute.Foundation);

            
        }
        if (Input.GetKeyDown("x"))
        {
            //TEST_AddCitizens(1000);
            //TEST_PrepareForExpeditions();
            // GiveRoutePoints(Knowledge.ResearchRoute.Foundation, 2);
            //WritePuzzleColors();
            //GameMaster.realMaster.colonyController.storage.AddResource(ResourceType.metal_S, 10000);
            //GameMaster.realMaster.colonyController.storage.AddResource(ResourceType.metal_E, 10000);

            GameMaster.realMaster.mainChunk.ClearChunk();
        }
        if (Input.GetKeyDown("c"))
        {
            //GivePuzzleParts(50);
        }
        if (Input.GetKeyDown("n"))
        {
            AddCitizens(5000, 30000f);
        }
    }

    private void ReplaceMaterials(GameObject g)
    {
        Material env_basic = Resources.Load<Material>("Materials/Advanced/Basic_ENV_ADV"),
            env_glass = Resources.Load<Material>("Materials/Advanced/Glass_ENV_ADV"),
            env_metal = Resources.Load<Material>("Materials/Advanced/Metal_ENV_ADV"),
            env_green = Resources.Load<Material>("Materials/Advanced/Green_ENV_ADV"),
            env_energy = Resources.Load<Material>("Materials/Environment/ColouredENV");
        Material[] mts, nmts;
        int layerIndex = GameConstants.GetEnvironmentLayerMask();
        foreach (var r in g.GetComponentsInChildren<Renderer>())
        {
            {
                if (r.sharedMaterials == null || r.sharedMaterials.Length == 1)
                {
                    switch (r.sharedMaterial.name)
                    {
                        case "Basic": r.sharedMaterial = env_basic; break;
                        case "Glass": r.sharedMaterial = env_glass; break;
                        case "Metal": r.sharedMaterial = env_metal; break;
                        case "Green": r.sharedMaterial = env_green; break;
                        case "ChargedMaterial": r.sharedMaterial = env_energy; break;
                    }
                }
                else
                {
                    mts = r.sharedMaterials;
                    nmts = new Material[mts.Length];
                    int i = 0;
                    foreach (var m in mts)
                    {
                        switch (m.name)
                        {
                            case "Basic": nmts[i++] = env_basic; break;
                            case "Glass": nmts[i++] = env_glass; break;
                            case "Metal": nmts[i++] = env_metal; break;
                            case "Green": nmts[i++] = env_green; break;
                            case "ChargedMaterial": nmts[i++] = env_energy; break;
                            default: nmts[i++] = m; break;
                        }
                    }
                    mts = null;
                    r.sharedMaterials = nmts;
                    nmts = null;
                }
            }
            r.receiveShadows = false;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.gameObject.layer = layerIndex;
        }
    }
    private void InstantiateDecorativeHouses()
    {
       
        GameObject[] prefs = new GameObject[12];
        prefs[0] = Resources.Load<GameObject>("Structures/Settlement/housePart_lvl1");
        prefs[1] = Resources.Load<GameObject>("Structures/Settlement/housePart_lvl2");
        prefs[2] = Resources.Load<GameObject>("Structures/Settlement/housePart_lvl3");
        prefs[3] = Resources.Load<GameObject>("Structures/Settlement/housePart_lvl4");
        prefs[4] = Resources.Load<GameObject>("Structures/Settlement/housePart_lvl5");
        prefs[5] = Resources.Load<GameObject>("Structures/Settlement/housePart_lvl6");
        //
        prefs[6] = Resources.Load<GameObject>("Structures/Settlement/settlementCenter_1");
        prefs[7] = Resources.Load<GameObject>("Structures/Settlement/settlementCenter_2");
        prefs[8] = Resources.Load<GameObject>("Structures/Settlement/settlementCenter_3");
        prefs[9] = Resources.Load<GameObject>("Structures/Settlement/settlementCenter_4");
        prefs[10] = Resources.Load<GameObject>("Structures/Settlement/settlementCenter_5");
        prefs[11] = Resources.Load<GameObject>("Structures/Settlement/settlementCenter_6");

        int rowsCount = 10, totalCount = 16 + rowsCount * 2, index;
        float density = 0.9f, x, sz = 5f;
        Transform parent = new GameObject("decsParent").transform;
        GameObject g;        


        for (int i = 0; i < totalCount;i++ )
        {
            for (int j = 0; j < totalCount; j++)
            {
               // if (i > rowsCount - 1 && i < totalCount - rowsCount && j > rowsCount - 1 && j < totalCount - rowsCount) continue;
                x = Random.value;
                if (x <= density)
                {
                    x /= density;
                    if (x > 0.85f)
                    {//spire
                        x = (x - 0.85f) / 0.15f;
                        if (x > 0.7f)
                        {
                            if (x > 0.9f) index = 11;
                            else if (x < 0.82f) index = 10; else index = 9;
                        }
                        else
                        {
                            if (x < 0.3f) index = 6;
                            else index = x > 0.55f ? 8 : 7; 
                        }
                    }
                    else
                    { // house
                        x /= 0.85f;
                        if (x > 0.7f)
                        {
                            if (x > 0.9f) index = 5;
                            else if (x < 0.82f) index = 3; else index = 4;
                        }
                        else
                        {
                            if (x < 0.3f) index = 0;
                            else index = x > 0.55f ? 2 : 1;
                        }
                    }

                    if (index < 6) continue;
                    g = Instantiate(
                        prefs[index],
                        new Vector3((i - rowsCount) * sz, -1f, (j - rowsCount) * sz),
                        Quaternion.Euler(Vector3.up * Random.value * 360f),
                        parent
                        );
                    ReplaceMaterials(g);
                    g.transform.localScale = Vector3.one * 4f;
                }
            }
        }
        //parent.localScale = Vector3.one * 16f;
    }

    public void StartApplyingEnvChanges()
    {
        emaster.StartConvertingEnvironment(Environment.GetEnvironment(selectedPreset));
    }
    public void RedrawSkybox()
    {
        var m = RenderSettings.skybox;
        Environment.GetEnvironment(selectedPreset).lightSettings.ApplyToSkyboxMaterial(ref m);
        RenderSettings.skybox = m;
    }
    public void FaceObj()
    {
        if (facingObj != null)
        {
            var pos = Vector3.up * facingObj.position.y;
            facingObj.LookAt(pos);
        }
    }

    public static void CreateCubeFromVertices(Vector3 pos, Material mat)
    {
        var m = new Mesh();
        m.vertices = new Vector3[] { Vector3.zero, Vector3.forward, new Vector3(1, 0, 1), Vector3.right, new Vector3(1, 1, 0), Vector3.one, new Vector3(0, 1, 1), Vector3.up };
        m.triangles = new int[] { 0, 2, 1, 0, 3, 2, 0, 4, 3, 0, 7, 4, 0, 1, 6, 0, 6, 7, 1, 5, 6, 1, 2, 5, 2, 4, 5, 2, 3, 4, 7, 6, 5, 7, 5, 4 };
        var g = new GameObject();
        var mf = g.AddComponent<MeshFilter>();
        mf.sharedMesh = m;
        var mr = g.AddComponent<MeshRenderer>();
        mr.sharedMaterial = mat;
        g.transform.position = pos;
    }
    public static void CreateCubePrimitive(Vector3 pos)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.transform.position = pos;
    }
    public static void RepairGameMaster()
    {
        if (GameMaster.realMaster == null)
        {
            var g = new GameObject("gameMaster");
            g.AddComponent<GameMaster>();
        }
    }

    public static void GivePuzzleParts(int count)
    {
        var k = Knowledge.GetCurrent();
        k.AddPuzzlePart(Knowledge.GREENCOLOR_CODE, count);
        k.AddPuzzlePart(Knowledge.BLUECOLOR_CODE, count);
        k.AddPuzzlePart(Knowledge.REDCOLOR_CODE, count);
        k.AddPuzzlePart(Knowledge.WHITECOLOR_CODE, count);
        k.AddPuzzlePart(Knowledge.CYANCOLOR_CODE, count);
    }
    public static void AddCitizens(int count, float foodCount)
    {
        var cc = GameMaster.realMaster.colonyController;
        if (cc != null)
        {
            if (foodCount != 0f) cc.storage.AddResource(ResourceType.Food, foodCount);
            cc.AddCitizens(count, true);
        }
    }
    public static void PrepareForExpeditions()
    {
        var rm = GameMaster.realMaster;
        var colonyController = rm.colonyController;
        var mainChunk = rm.mainChunk;
        if (colonyController == null) return;
        var planes = mainChunk.GetUnoccupiedSurfaces(8);
        planes[0]?.CreateStructure(Structure.RECRUITING_CENTER_4_ID);
        var c = Crew.CreateNewCrew(colonyController, Crew.MAX_CREWS_COUNT); c.Rename("Mony Mony");
        c = Crew.CreateNewCrew(colonyController, Crew.MAX_CREWS_COUNT); c.Rename("Black Rabbit");
        c = Crew.CreateNewCrew(colonyController, Crew.MAX_CREWS_COUNT); c.Rename("Bonemaker");
        c = Crew.CreateNewCrew(colonyController, Crew.MAX_CREWS_COUNT); c.Rename("Eiffiel Dungeon");
        planes[1]?.CreateStructure(Structure.QUANTUM_TRANSMITTER_4_ID);
        planes[2]?.CreateStructure(Structure.QUANTUM_TRANSMITTER_4_ID);
        planes[3]?.CreateStructure(Structure.QUANTUM_TRANSMITTER_4_ID);
        planes[4]?.CreateStructure(Structure.QUANTUM_TRANSMITTER_4_ID);
        planes[5]?.CreateStructure(Structure.EXPEDITION_CORPUS_4_ID);
        planes[6]?.CreateStructure(Structure.MINI_GRPH_REACTOR_3_ID);
        planes[7]?.CreateStructure(Structure.STORAGE_BLOCK_ID);
        colonyController.storage.AddResource(ResourceType.Fuel, 25000);
        colonyController.storage.AddResource(ResourceType.Supplies, 2000);

        var pf = mainChunk.GetUnoccupiedEdgePosition(false, false);
        var p = pf.plane;
        Structure s;
        if (p != null)
        {
            s = p.CreateStructure(Structure.SHUTTLE_HANGAR_4_ID);
            if (s != null)
            {
                s.SetModelRotation(pf.faceIndex * 2);
                (s as Hangar).FORCED_MakeShuttle();
            }
        }
        pf = mainChunk.GetUnoccupiedEdgePosition(false, false);
        p = pf.plane;
        if (p != null)
        {
            s = p.CreateStructure(Structure.SHUTTLE_HANGAR_4_ID);
            if (s != null)
            {
                s.SetModelRotation(pf.faceIndex * 2);
                (s as Hangar).FORCED_MakeShuttle();
            }
        }
        pf = mainChunk.GetUnoccupiedEdgePosition(false, false);
        p = pf.plane;
        if (p != null)
        {
            s = p.CreateStructure(Structure.SHUTTLE_HANGAR_4_ID);
            if (s != null)
            {
                s.SetModelRotation(pf.faceIndex * 2);
                (s as Hangar).FORCED_MakeShuttle();
            }
        }
        pf = mainChunk.GetUnoccupiedEdgePosition(false, false);
        p = pf.plane;
        if (p != null)
        {
            s = p.CreateStructure(Structure.SHUTTLE_HANGAR_4_ID);
            if (s != null)
            {
                s.SetModelRotation(pf.faceIndex * 2);
                (s as Hangar).FORCED_MakeShuttle();
            }
        }
        var globalMap = rm.globalMap;
        globalMap.FORCED_CreatePointOfInterest();
        globalMap.FORCED_CreatePointOfInterest();
        globalMap.FORCED_CreatePointOfInterest();
        globalMap.FORCED_CreatePointOfInterest();
        globalMap.FORCED_CreatePointOfInterest();
        globalMap.FORCED_CreatePointOfInterest();
        globalMap.FORCED_CreatePointOfInterest();
        globalMap.FORCED_CreatePointOfInterest();
        globalMap.FORCED_CreatePointOfInterest();
        globalMap.FORCED_CreatePointOfInterest();
    }
    public static void GiveRoutePoints(Knowledge.ResearchRoute rr, int count)
    {
        Knowledge.GetCurrent()?.AddResearchPoints(rr, count);
    }

    public static void CreateColony()
    {
        var rm = GameMaster.realMaster;
        var chunk = rm.mainChunk;
        chunk.GetRandomSurface().CreateStructure(Structure.HEADQUARTERS_ID);
        chunk.GetRandomSurface().CreateStructure(Structure.STORAGE_0_ID);
        chunk.GetRandomSurface().CreateStructure(Structure.SETTLEMENT_CENTER_ID);
        chunk.GetRandomSurface().CreateStructure(Structure.MINI_GRPH_REACTOR_3_ID);
        rm.SetStartResources();        
    }

    //
    private void WritePuzzleColors()
    {
        byte x;
        var knowledge = Knowledge.GetCurrent();
        byte redcount = 0, bluecount = 0, greencount = 0, cyancount = 0;
        for (byte i = 0; i < 8; i++)
        {
            string s = ((Knowledge.ResearchRoute)i).ToString() + ": ";
            for (byte j = 0; j < 8; j++)
            {
                x = knowledge.GenerateCellColor(i, j);
                switch (x)
                {
                    case Knowledge.REDCOLOR_CODE:
                        redcount++;
                        //s += "red ";
                        break;
                    case Knowledge.GREENCOLOR_CODE:
                        greencount++;
                        //s += "green ";
                        break;
                    case Knowledge.BLUECOLOR_CODE:
                        bluecount++;
                        //s += "blue ";
                        break;
                    case Knowledge.CYANCOLOR_CODE:
                        cyancount++;
                        //s += "cyan ";
                        break;
                }
            }
            //Debug.Log(s);
            
        }
        Debug.Log("recount: " + redcount.ToString());
        Debug.Log("greencount: " + greencount.ToString());
        Debug.Log("bluecount: " + bluecount.ToString());
        Debug.Log("cyancount :" + cyancount.ToString());
    }
}

[CustomEditor(typeof(TestMaster))]
public class TestMasterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        TestMaster script = (TestMaster)target;
         if (GUILayout.Button("Apply in game")) script.StartApplyingEnvChanges();
        if (GUILayout.Button("Redraw")) script.RedrawSkybox();
        if (GUILayout.Button("Face to zero")) script.FaceObj();
    }
}

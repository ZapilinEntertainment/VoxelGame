using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TestMaster : MonoBehaviour
{
    [SerializeField] private GameObject testObject0;
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
        ReplaceMaterials(testObject0);
    }

    private void Update()
    {
        if (Input.GetKeyDown("h"))
        {
            //GameMaster.realMaster.globalMap.TEST_MakeNewPoint(MapMarkerType.Star);
        }
    }

    private void ReplaceMaterials(GameObject g)
    {
        Material env_basic = Resources.Load<Material>("Materials/Advanced/Basic_ENV_ADV"),
            env_glass = Resources.Load<Material>("Materials/Advanced/Glass_ENV_ADV"),
            env_metal = Resources.Load<Material>("Materials/Advanced/Metal_ENV_ADV"),
            env_green = Resources.Load<Material>("Materials/Advanced/Green_ENV_ADV"),
            env_energy = Resources.Load<Material>("Materials/ColouredENV");
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
    private void InstantiateHouses()
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
}

[CustomEditor(typeof(TestMaster))]
public class TestMasterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        TestMaster script = (TestMaster)target;
         if (GUILayout.Button("Apply")) script.StartApplyingEnvChanges();
    }
}

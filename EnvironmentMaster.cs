using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class EnvironmentMaster : MonoBehaviour {
    [SerializeField] private Vector2 newWindVector;

    public bool positionChanged = false; // может быть отмечена другими скриптами
    public float environmentalConditions { get; private set; } // 0 is hell, 1 is very favourable
    public Vector2 windVector { get; private set; }
    public Transform sun;
    public delegate void WindChangeHandler(Vector2 newVector);
    public event WindChangeHandler WindUpdateEvent;

    private bool prepared = false, cloudsEnabled = true, sunEnabled = true;
    private int vegetationShaderWindPropertyID;
    private float windTimer = 0, prevSkyboxSaturation = 1;
    private Environment currentEnvironment;
    private GameObject physicalSun;
    private GlobalMap gmap;
    private MapPoint cityPoint, sunPoint;
    private Material skyboxMaterial;
    private ParticleSystem.MainModule cloudEmitterMainModule;
    private Transform cloudEmitter;

    private const float WIND_CHANGE_STEP = 1, WIND_CHANGE_TIME = 120;
    private const int SKY_SPHERE_RADIUS = 9;

    public void Prepare()
    {
        if (prepared) return; else prepared = true;
        vegetationShaderWindPropertyID = Shader.PropertyToID("_Windpower");
        windVector = Random.insideUnitCircle;
        newWindVector = windVector;
        if (cloudEmitter == null)
        {
            cloudEmitter = Instantiate(Resources.Load<Transform>("Prefs/cloudEmitter"), Vector3.zero, Quaternion.identity, transform);
        }
        cloudEmitter.rotation = Quaternion.LookRotation(new Vector3(windVector.x,0, windVector.y), Vector3.up);
        cloudEmitterMainModule = cloudEmitter.GetComponent<ParticleSystem>().main;
        cloudEmitterMainModule.simulationSpeed = 1;
        Shader.SetGlobalFloat(vegetationShaderWindPropertyID, 1);
        sun = FindObjectOfType<Light>().transform;
        gmap = GameMaster.realMaster.globalMap;
        cityPoint = gmap.mapPoints[GlobalMap.CITY_POINT_INDEX];
        sunPoint = gmap.mapPoints[GlobalMap.SUN_POINT_INDEX];

        skyboxMaterial = RenderSettings.skybox;
        ChangeEnvironment(gmap.GetCurrentEnvironment(), true);
    }
    public void PrepareIslandBasis(ChunkGenerationMode cmode)
    { // его придётся сохранять!
        int[,,] data;
        switch (cmode) {
            case ChunkGenerationMode.Peak:
                float size = SKY_SPHERE_RADIUS;
                GameObject g = Constructor.CreatePeakBasis(8, ResourceType.STONE_ID, size);
                g.transform.position = new Vector3(-size / 2f, - size/2f, -size / 2f);
                g.layer = cloudEmitter.gameObject.layer;
                break;
            default: return;
        }        
    }

    public void SetEnvironmentalConditions(float t)
    {
        environmentalConditions = t;
    }
    public void ChangeEnvironment(Environment e, bool haveSun)
    {
        GlobalMap gmap = GameMaster.realMaster.globalMap;
        currentEnvironment = e;       
        prevSkyboxSaturation = skyboxMaterial.GetFloat("_Saturation");
        skyboxMaterial.SetFloat("_Saturation", prevSkyboxSaturation);
        environmentalConditions = currentEnvironment.conditions;

        if (haveSun)
        {
            sun.GetComponent<Light>().color = currentEnvironment.sunColor;
            if (!sun.gameObject.activeSelf) sun.gameObject.SetActive(true);
        }
        else
        {
            if (sun.gameObject.activeSelf) sun.gameObject.SetActive(false);
        }
        sunEnabled = haveSun;
        positionChanged = true;
    }

    private void LateUpdate()
    {
        windTimer -= Time.deltaTime;
        if (windTimer <= 0)
        {
            windTimer = WIND_CHANGE_TIME * (0.1f + Random.value * 1.9f);
            newWindVector = Quaternion.AngleAxis((0.5f - Random.value) * 30, Vector3.up) * windVector;
            newWindVector = newWindVector * ( 1 + (0.5f - Random.value) * 0.5f );
        }

        if (windVector != newWindVector)
        {
            float t = WIND_CHANGE_STEP * Time.deltaTime;            
            windVector = Vector3.RotateTowards(windVector, newWindVector, t, t);
            if (windVector.magnitude != 0)
            {
                cloudEmitter.transform.forward = new Vector3(windVector.x, 0, windVector.y);
            }
            float windpower = windVector.magnitude;
            Shader.SetGlobalFloat(vegetationShaderWindPropertyID, windpower);
            if (cloudsEnabled) cloudEmitterMainModule.simulationSpeed = windpower;
            if (WindUpdateEvent != null) WindUpdateEvent(windVector);
        }

        if (positionChanged)
        {
            byte ring = cityPoint.ringIndex;
            float angleX = (cityPoint.angle - sunPoint.angle) / (gmap.sectorsDegrees[ring] / 2f);
            float heightY = (cityPoint.height - sunPoint.height) / ((gmap.ringsBorders[ring] - gmap.ringsBorders[ring + 1]) / 2f);
            Vector2 lookDist = new Vector2(angleX, heightY);
            sun.transform.position = new Vector3(lookDist.x * SKY_SPHERE_RADIUS, 2, lookDist.y * SKY_SPHERE_RADIUS);
            sun.transform.LookAt(Vector3.zero);


            float d = lookDist.magnitude;
            if (d > 1) d = 1;
            float s = Mathf.Sin((d + 1) * 90 * Mathf.Deg2Rad);
            if (sunEnabled) sun.GetComponent<Light>().intensity = s;
            if (s != prevSkyboxSaturation)
            {
                prevSkyboxSaturation = s;
                skyboxMaterial.SetFloat("_Saturation", prevSkyboxSaturation);
            }
            positionChanged = false;
        }
    }

    public void Save( System.IO.FileStream fs)
    {
        fs.Write(System.BitConverter.GetBytes(newWindVector.x),0,4); // 0 - 3
        fs.Write(System.BitConverter.GetBytes(newWindVector.y), 0, 4);// 4 - 7
        fs.Write(System.BitConverter.GetBytes(windVector.x), 0, 4);   // 8 - 11
        fs.Write(System.BitConverter.GetBytes(windVector.y), 0, 4);  // 12 - 15
        fs.Write(System.BitConverter.GetBytes(environmentalConditions), 0, 4); // 16 - 19
        fs.Write(System.BitConverter.GetBytes(windTimer), 0, 4);  // 20 - 23
    }
    public void Load(System.IO.FileStream fs)
    {
        var data = new byte[24];
        fs.Read(data, 0, data.Length);
        newWindVector = new Vector2(System.BitConverter.ToSingle(data, 0), System.BitConverter.ToSingle(data, 4));
        windVector = new Vector2(System.BitConverter.ToSingle(data, 8), System.BitConverter.ToSingle(data, 12));
        environmentalConditions = System.BitConverter.ToSingle(data, 16);
        windTimer = System.BitConverter.ToSingle(data, 20);

        vegetationShaderWindPropertyID = Shader.PropertyToID("_Windpower");
        if (cloudEmitter == null)
        {
            cloudEmitter = Instantiate(Resources.Load<Transform>("Prefs/cloudEmitter"), Vector3.zero, Quaternion.identity);
        }
        if (windVector != Vector2.zero)  cloudEmitter.rotation = Quaternion.LookRotation(new Vector3(windVector.x, 0, windVector.y), Vector3.up);
        cloudEmitterMainModule = cloudEmitter.GetComponent<ParticleSystem>().main;
        float windPower = windVector.magnitude;
        cloudEmitterMainModule.simulationSpeed = windPower;
        Shader.SetGlobalFloat(vegetationShaderWindPropertyID, windPower);
        prepared = true;
        if (WindUpdateEvent != null) WindUpdateEvent(windVector);
    }
}

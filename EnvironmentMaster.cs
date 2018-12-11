using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public sealed class EnvironmentMasterSerializer
{
    public float newWindVector_x, newWindVector_y, windVector_x, windVector_y;
    public float environmentalConditions, windTimer;
}

public sealed class EnvironmentMaster : MonoBehaviour {
    [SerializeField] private Vector2 newWindVector;

    public float environmentalConditions { get; private set; } // 0 is hell, 1 is very favourable
    public Vector2 windVector { get; private set; }
    public delegate void WindChangeHandler(Vector2 newVector);
    public event WindChangeHandler WindUpdateEvent;

    private bool prepared = false;
    private int vegetationShaderWindPropertyID;
    private float windTimer = 0;
    private ParticleSystem.MainModule cloudEmitterMainModule;
    private Transform cloudEmitter;    

    private const float WIND_CHANGE_STEP = 1, WIND_CHANGE_TIME = 120;

    public void Prepare()
    {
        if (prepared) return; else prepared = true;
        vegetationShaderWindPropertyID = Shader.PropertyToID("_Windpower");
        windVector = Random.insideUnitCircle;
        newWindVector = windVector;
        if (cloudEmitter == null)
        {
            cloudEmitter = Instantiate(Resources.Load<Transform>("Prefs/cloudEmitter"), Vector3.zero, Quaternion.identity);
        }
        cloudEmitter.rotation = Quaternion.LookRotation(new Vector3(windVector.x,0, windVector.y), Vector3.up);
        cloudEmitterMainModule = cloudEmitter.GetComponent<ParticleSystem>().main;
        cloudEmitterMainModule.simulationSpeed = 1;
        Shader.SetGlobalFloat(vegetationShaderWindPropertyID, 1);
    }

    public void SetEnvironmentalConditions(float t)
    {
        environmentalConditions = t;
    }

    private void LateUpdate()
    {
        windTimer -= Time.deltaTime;
        if (windTimer <= 0)
        {
            windTimer = WIND_CHANGE_TIME * (0.1f + Random.value * 1.9f);
            float a = (Random.value - 0.5f) * 2 * Mathf.Deg2Rad;
            float cos = Mathf.Cos(a), sin = Mathf.Sin(a);
            newWindVector = new Vector2(windVector.x * cos - windVector.y * sin, windVector.x * sin + windVector.y * cos).normalized * windVector.magnitude * (0.9f + Random.value * 0.2f);
        }

        if (windVector != newWindVector)
        {
            float t = WIND_CHANGE_STEP * Time.deltaTime;
            windVector = Vector3.RotateTowards(windVector, newWindVector, t, t);
            cloudEmitter.transform.forward = new Vector3(windVector.x, 0, windVector.y);
            float windpower = windVector.magnitude;
            Shader.SetGlobalFloat(vegetationShaderWindPropertyID, windpower);
            cloudEmitterMainModule.simulationSpeed = windpower;
            if (WindUpdateEvent != null) WindUpdateEvent(windVector);
        }
    }

    public EnvironmentMasterSerializer Save()
    {
        EnvironmentMasterSerializer ems = new EnvironmentMasterSerializer();
        ems.newWindVector_x = newWindVector.x;
        ems.newWindVector_y = newWindVector.y;
        ems.windVector_x = windVector.x;
        ems.windVector_y = windVector.y;
        ems.environmentalConditions = environmentalConditions;
        ems.windTimer = windTimer;
        return ems;
    }
    public void Load(EnvironmentMasterSerializer ems)
    {
        newWindVector = new Vector2(ems.newWindVector_x, ems.newWindVector_y);
        windVector = new Vector2(ems.windVector_x, ems.windVector_y);
        environmentalConditions = ems.environmentalConditions;
        windTimer = ems.windTimer;

        vegetationShaderWindPropertyID = Shader.PropertyToID("_Windpower");
        if (cloudEmitter == null)
        {
            cloudEmitter = Instantiate(Resources.Load<Transform>("Prefs/cloudEmitter"), Vector3.zero, Quaternion.identity);
        }
        cloudEmitter.rotation = Quaternion.LookRotation(new Vector3(windVector.x, 0, windVector.y), Vector3.up);
        cloudEmitterMainModule = cloudEmitter.GetComponent<ParticleSystem>().main;
        float windPower = windVector.magnitude;
        cloudEmitterMainModule.simulationSpeed = windPower;
        Shader.SetGlobalFloat(vegetationShaderWindPropertyID, windPower);
        prepared = true;
    }
}

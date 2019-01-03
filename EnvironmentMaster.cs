using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            if (windVector.magnitude != 0)
            {
                cloudEmitter.transform.forward = new Vector3(windVector.x, 0, windVector.y);
            }
            float windpower = windVector.magnitude;
            Shader.SetGlobalFloat(vegetationShaderWindPropertyID, windpower);
            cloudEmitterMainModule.simulationSpeed = windpower;
            if (WindUpdateEvent != null) WindUpdateEvent(windVector);
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
    public int Load(byte[] data, int startIndex)
    {
        newWindVector = new Vector2();
        windVector = new Vector2(System.BitConverter.ToSingle(data, startIndex + 8), System.BitConverter.ToSingle(data, startIndex + 12));
        environmentalConditions = System.BitConverter.ToSingle(data, startIndex + 16);
        windTimer = System.BitConverter.ToSingle(data, startIndex + 20);

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
        return startIndex + 24;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Meteorite : MonoBehaviour
{
    public enum MeteoritePackageType: byte { NoPackage, CommonResource, RareResource, ValuableResource, Lifestone, Plant, Artifact}

    private MeteoritePackageType packageType;
    private int packageID = -1;
    private float packageVolume = 0f;

    private static ParticleSystem strikeParticles;
    private static GameObject[] meteorites;
    private const float G = 9.8f;

    public static void EffectLaunchSmall(Vector3 pos, Vector3 normal)
    {

    }
    public static void DeliveryLaunchSmall(Vector3 pos, Vector3 normal, MeteoritePackageType ptype, int i_packageID, float i_packageVolume)
    {

    }

    public void Update()
    {
        
    }
}

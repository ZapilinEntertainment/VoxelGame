using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBehaviour : MonoBehaviour
{
    [SerializeField] private GameObject[] obj;

    void Start()
    {
        if (obj != null)
        {
            int i = 0;
            var rtypes = ResourceType.materialsForCovering;
            MeshFilter mf;
            MeshRenderer mr;
            while (i < obj.Length & i < rtypes.Length)
            {
                mf = obj[i].GetComponent<MeshFilter>();
                mr = obj[i].GetComponent<MeshRenderer>();
                PoolMaster.SetMaterialByID(ref mf, ref mr, rtypes[i].ID, 255);
                i++;
            }
        }
    }

}

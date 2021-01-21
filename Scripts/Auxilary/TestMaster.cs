using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMaster : MonoBehaviour
{
    public static void CreateCube(Vector3 point)
    {
        GameObject.CreatePrimitive(PrimitiveType.Cube).transform.position = point;
    }
}

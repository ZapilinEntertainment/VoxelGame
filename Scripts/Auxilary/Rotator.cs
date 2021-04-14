using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] private Vector3 rotationVector;
    [SerializeField] private Space rotationSpace;

    void Update()
    {
        transform.Rotate(rotationVector * Time.deltaTime * GameMaster.gameSpeed, rotationSpace);
    }

    public void SetRotationVector(Vector3 rv)
    {
        rotationVector = rv;
    }
}

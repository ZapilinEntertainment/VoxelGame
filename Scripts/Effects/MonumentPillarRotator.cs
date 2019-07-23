using UnityEngine;

public sealed class MonumentPillarRotator : MonoBehaviour
{
#pragma warning disable 0649
    private Vector3 startPos;
#pragma warning restore 0649
    private void Awake()
    {
        if (PoolMaster.qualityLevel == 0) Destroy(this);
        startPos = transform.localPosition;
    }

    private void Update()
    {
        var it = Time.deltaTime * GameMaster.gameSpeed;        
        transform.Rotate(Vector3.up * it * 3f, Space.Self);
        transform.localPosition = startPos + Vector3.up * Mathf.Sin(Time.time) * 0.002f;
    }
}

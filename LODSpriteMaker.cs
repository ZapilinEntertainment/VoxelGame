using UnityEngine;
using System.Collections.Generic;

public enum LODPackType { Point, OneSide, Full}

public class LODSpriteMaker : MonoBehaviour {
    public static LODSpriteMaker current {get; private set;}
    Camera cam;
    // [SerializeField]   Texture2D lastResult; // для тестов
    private const float PIXELS_PER_UNIT = 128;

    private void Awake()
    {
        current = this;  
        cam = GetComponent<Camera>();
    }
    
    public Texture2D CreateLODPack(LODPackType i_lpackType, GameObject model, Color backgroundColor, LODRegisterInfo regInfo)
    {
        LODController lcontroller = LODController.GetCurrent();
        int registeredIndex = lcontroller.LOD_existanceCheck(regInfo);
        if (registeredIndex != -1)  return lcontroller.registeredLODs[registeredIndex].spriteAtlas;
        MeshFilter[] mfilters = model.GetComponentsInChildren<MeshFilter>();
        if (mfilters == null || mfilters.Length == 0) return null;

        float maxY = 0, radius = 0, minY = 0;
        int savedLayer = model.layer;
        var layerNumber = 8;
        // вообще модели идут из блендера с поворотом по x на 90 градусов. Ждём-с ошибки
        foreach (MeshFilter mf in mfilters)
        {
            foreach (Vector3 v in mf.sharedMesh.vertices)
            {
                if (v.y > maxY) maxY = v.y;
                if (v.y < minY) minY = v.y;
                if (v.sqrMagnitude > radius) radius = v.sqrMagnitude;
            }
            mf.gameObject.layer = layerNumber;
        }
        float halfsize = Mathf.Sqrt(radius);
        cam.orthographicSize = halfsize;
        cam.backgroundColor = backgroundColor;

        int spriteSize = (int)(halfsize * 2 * 64); // 64px в единице
        RenderTexture m_RenderTexture = new RenderTexture(spriteSize, spriteSize, 8, RenderTextureFormat.ARGB32);
        cam.transform.parent = model.transform;
        cam.enabled = true;
        Texture2D atlas;
        switch (i_lpackType)
        {
            case LODPackType.Point:
                {
                    cam.transform.localPosition = Vector3.back * (halfsize + cam.nearClipPlane);
                    cam.transform.localRotation = Quaternion.Euler(Vector3.zero);                    
                    m_RenderTexture.Create();
                    cam.targetTexture = m_RenderTexture;
                    cam.Render();
                    RenderTexture.active = m_RenderTexture;
                    atlas = new Texture2D(spriteSize, spriteSize);
                    atlas.ReadPixels(new Rect(0, 0, spriteSize, spriteSize), 0, 0);
                    atlas.Apply();
                    break;
                }
            case LODPackType.OneSide:
                {
                   Texture2D[] spriteRenders = new Texture2D[4];
                    Vector3 modelPos = model.transform.position;
                    // 0 degrees
                    cam.transform.localPosition = Vector3.back * (halfsize + cam.nearClipPlane);
                    cam.transform.localRotation = Quaternion.Euler(Vector3.zero);
                    Texture2D spriteRender = new Texture2D(spriteSize, spriteSize);
                    m_RenderTexture.Create();
                    cam.targetTexture = m_RenderTexture;
                    cam.Render();
                    RenderTexture.active = m_RenderTexture;
                    spriteRenders[0] = new Texture2D(spriteSize, spriteSize);
                    spriteRenders[0].ReadPixels(new Rect(0, 0, spriteSize, spriteSize), 0, 0);
                    spriteRenders[0].Apply();
                    // 22.5 degrees
                    cam.transform.localPosition = Quaternion.Euler(LODController.SECOND_LOD_ANGLE, 0, 0) * (Vector3.back * (halfsize + cam.nearClipPlane));
                    cam.transform.LookAt(modelPos);
                    spriteRender = new Texture2D(spriteSize, spriteSize);
                    m_RenderTexture.Create();
                    cam.targetTexture = m_RenderTexture;
                    cam.Render();
                    RenderTexture.active = m_RenderTexture;
                    spriteRenders[1] = new Texture2D(spriteSize, spriteSize);
                    spriteRenders[1].ReadPixels(new Rect(0, 0, spriteSize, spriteSize), 0, 0);
                    spriteRenders[1].Apply();
                    // 45 degrees
                    cam.transform.localPosition = Quaternion.Euler(LODController.THIRD_LOD_ANGLE, 0, 0) * (Vector3.back * (halfsize + cam.nearClipPlane));
                    cam.transform.LookAt(modelPos);
                    spriteRender = new Texture2D(spriteSize, spriteSize);
                    m_RenderTexture.Create();
                    cam.targetTexture = m_RenderTexture;
                    cam.Render();
                    RenderTexture.active = m_RenderTexture;
                    spriteRenders[2] = new Texture2D(spriteSize, spriteSize);
                    spriteRenders[2].ReadPixels(new Rect(0, 0, spriteSize, spriteSize), 0, 0);
                    spriteRenders[2].Apply();
                    // 85 degrees
                    cam.transform.localPosition = Quaternion.Euler(LODController.FOURTH_LOD_ANGLE, 0, 0) * (Vector3.back * (halfsize + cam.nearClipPlane));
                    cam.transform.LookAt(modelPos);
                    spriteRender = new Texture2D(spriteSize, spriteSize);
                    m_RenderTexture.Create();
                    cam.targetTexture = m_RenderTexture;
                    cam.Render();
                    RenderTexture.active = m_RenderTexture;
                    spriteRenders[3] = new Texture2D(spriteSize, spriteSize);
                    spriteRenders[3].ReadPixels(new Rect(0, 0, spriteSize, spriteSize), 0, 0);
                    spriteRenders[3].Apply();

                    atlas = new Texture2D(2 * spriteSize, 2 * spriteSize);
                    atlas.PackTextures(spriteRenders, 0, 2 * spriteSize, false);                    
                    break;
                }
            case LODPackType.Full:
                {
                    Texture2D[] spriteRenders = new Texture2D[32];
                    Vector3 modelPos = model.transform.position;
                    int index = 0;
                    for (int i = 0; i < 8; i++)
                    {
                        // 0 degrees
                        cam.transform.localPosition = Vector3.back * (halfsize + cam.nearClipPlane);
                        cam.transform.localRotation = Quaternion.Euler(0, i * 45, 0);
                        Texture2D spriteRender = new Texture2D(spriteSize, spriteSize);
                        m_RenderTexture.Create();
                        cam.targetTexture = m_RenderTexture;
                        cam.Render();
                        RenderTexture.active = m_RenderTexture;
                        spriteRenders[index] = new Texture2D(spriteSize, spriteSize);
                        spriteRenders[index].ReadPixels(new Rect(0, 0, spriteSize, spriteSize), 0, 0);
                        spriteRenders[index].Apply();
                        index++;
                        // 22.5 degrees
                        cam.transform.localPosition = Quaternion.Euler(LODController.SECOND_LOD_ANGLE, i * 45, 0) * (Vector3.back * (halfsize + cam.nearClipPlane));
                        cam.transform.LookAt(modelPos);
                        spriteRender = new Texture2D(spriteSize, spriteSize);
                        m_RenderTexture.Create();
                        cam.targetTexture = m_RenderTexture;
                        cam.Render();
                        RenderTexture.active = m_RenderTexture;
                        spriteRenders[index] = new Texture2D(spriteSize, spriteSize);
                        spriteRenders[index].ReadPixels(new Rect(0, 0, spriteSize, spriteSize), 0, 0);
                        spriteRenders[index].Apply();
                        index++;
                        // 45 degrees
                        cam.transform.localPosition = Quaternion.Euler(LODController.THIRD_LOD_ANGLE, i * 45, 0) * (Vector3.back * (halfsize + cam.nearClipPlane));
                        cam.transform.LookAt(modelPos);
                        spriteRender = new Texture2D(spriteSize, spriteSize);
                        m_RenderTexture.Create();
                        cam.targetTexture = m_RenderTexture;
                        cam.Render();
                        RenderTexture.active = m_RenderTexture;
                        spriteRenders[index] = new Texture2D(spriteSize, spriteSize);
                        spriteRenders[index].ReadPixels(new Rect(0, 0, spriteSize, spriteSize), 0, 0);
                        spriteRenders[index].Apply();
                        index++;
                        // 85 degrees
                        cam.transform.localPosition = Quaternion.Euler(LODController.FOURTH_LOD_ANGLE, i * 45, 0) * (Vector3.back * (halfsize + cam.nearClipPlane));
                        cam.transform.LookAt(modelPos);
                        spriteRender = new Texture2D(spriteSize, spriteSize);
                        m_RenderTexture.Create();
                        cam.targetTexture = m_RenderTexture;
                        cam.Render();
                        RenderTexture.active = m_RenderTexture;
                        spriteRenders[index] = new Texture2D(spriteSize, spriteSize);
                        spriteRenders[index].ReadPixels(new Rect(0, 0, spriteSize, spriteSize), 0, 0);
                        spriteRenders[index].Apply();
                        index++;
                    }
                    atlas = new Texture2D(4 * spriteSize, 8 * spriteSize);
                    atlas.PackTextures(spriteRenders, 0, 8 * spriteSize, false);                    
                    break;
                }
            default: return null;
        }
        cam.enabled = false;
        cam.transform.parent = null;
        RenderTexture.active = null;
        foreach (MeshFilter mf in mfilters)
        {
            mf.gameObject.layer = savedLayer;
        }
        lcontroller.RegisterLOD(new LODRegistrationTicket(regInfo, atlas, i_lpackType));
        return atlas;
    }

    void ChangeLayerRecursively(Transform t, int layerNumber)
    {        
        for ( int i = 0; i < t.childCount; i++)
        {
            Transform child = t.GetChild(i);
                child.gameObject.layer = layerNumber;
            if (child.childCount > 0) ChangeLayerRecursively(child, layerNumber);
        }
    }

}

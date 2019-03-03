using UnityEngine;
using System.Collections.Generic;

public enum LODPackType { Point, OneSide, Full}

public struct RenderPoint
{
    public Vector3 position, rotation;
    public RenderPoint (Vector3 pos, Vector3 rot)
    {
        position = pos;
        rotation = rot;
    }
}


public class LODSpriteMaker : MonoBehaviour {
    public static LODSpriteMaker current {get; private set;}
    Camera cam;
    //[SerializeField]   Texture2D lastResult; // для тестов    
    public Texture2D a, b, c; // тоже для тестов
    public const float PIXELS_PER_UNIT = 64;

    private void Awake()
    {
        current = this;  
        cam = GetComponent<Camera>();
    }
    
    /// <summary>
    /// Make checks before invoking
    /// </summary>
    public int CreateLODPack(LODPackType i_lpackType, GameObject model, RenderPoint[] renderPoints, int resolution, float shotSize, Color backgroundColor, LODRegisterInfo regInfo)
    {
        LODController lcontroller = LODController.GetCurrent();

        backgroundColor.a = 0;      
        int savedLayer = model.layer;
        var layerNumber = 8;
        Renderer[] allObjects = model.transform.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in allObjects)
        {
            r.gameObject.layer = layerNumber;
        }     

        cam.orthographicSize = shotSize;
        cam.backgroundColor = backgroundColor;
        
        RenderTexture m_RenderTexture = new RenderTexture(resolution, resolution, 8, RenderTextureFormat.ARGB32);
        cam.transform.parent = model.transform;
        cam.transform.localRotation = Quaternion.Euler(Vector3.up * 180);

        cam.enabled = true;
        Texture2D atlas;        
        switch (i_lpackType)
        {
            case LODPackType.Point:
                {
                    cam.transform.localPosition = renderPoints[0].position;
                    cam.transform.localRotation = Quaternion.Euler(renderPoints[0].rotation);                    
                    m_RenderTexture.Create();
                    cam.targetTexture = m_RenderTexture;
                    cam.Render();
                    RenderTexture.active = m_RenderTexture;
                    atlas = new Texture2D(resolution, resolution);
                    atlas.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
                    atlas.Apply();
                    break;
                }
            case LODPackType.OneSide:
                {
                   Texture2D[] spriteRenders = new Texture2D[4];
                    // 0 degrees                    
                    cam.transform.localPosition = renderPoints[0].position;
                    cam.transform.localRotation = Quaternion.Euler(renderPoints[0].rotation);
                    m_RenderTexture.Create();
                    cam.targetTexture = m_RenderTexture;
                    cam.Render();
                    RenderTexture.active = m_RenderTexture;
                    spriteRenders[0] = new Texture2D(resolution, resolution);
                    spriteRenders[0].ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
                    spriteRenders[0].Apply();
                    // 22.5 degrees
                    cam.transform.localPosition = renderPoints[1].position;
                    cam.transform.localRotation = Quaternion.Euler(renderPoints[1].rotation);
                    m_RenderTexture.Create();
                    cam.targetTexture = m_RenderTexture;
                    cam.Render();
                    RenderTexture.active = m_RenderTexture;
                    spriteRenders[1] = new Texture2D(resolution, resolution);
                    spriteRenders[1].ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
                    spriteRenders[1].Apply();
                    // 45 degrees
                    cam.transform.localPosition = renderPoints[2].position;
                    cam.transform.localRotation = Quaternion.Euler(renderPoints[2].rotation);
                    m_RenderTexture.Create();
                    cam.targetTexture = m_RenderTexture;
                    cam.Render();
                    RenderTexture.active = m_RenderTexture;
                    spriteRenders[2] = new Texture2D(resolution, resolution);
                    spriteRenders[2].ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
                    spriteRenders[2].Apply();
                    // 85 degrees
                    cam.transform.localPosition = renderPoints[3].position;
                    cam.transform.localRotation = Quaternion.Euler(renderPoints[3].rotation);
                    m_RenderTexture.Create();
                    cam.targetTexture = m_RenderTexture;
                    cam.Render();
                    RenderTexture.active = m_RenderTexture;
                    spriteRenders[3] = new Texture2D(resolution, resolution);
                    spriteRenders[3].ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
                    spriteRenders[3].Apply();

                    atlas = new Texture2D(2 * resolution, 2 * resolution);
                    atlas.PackTextures(spriteRenders, 0, 2 * resolution, false);                    
                    break;
                }
            case LODPackType.Full:
                {
                    Texture2D[] spriteRenders = new Texture2D[32];
                    int index = 0;
                    for (int i = 0; i < 8; i++)
                    {
                        // 0 degrees
                        cam.transform.localPosition = renderPoints[index].position;
                        cam.transform.localRotation = Quaternion.Euler(renderPoints[index].rotation);
                        m_RenderTexture.Create();
                        cam.targetTexture = m_RenderTexture;
                        cam.Render();
                        RenderTexture.active = m_RenderTexture;
                        spriteRenders[index] = new Texture2D(resolution, resolution);
                        spriteRenders[index].ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
                        spriteRenders[index].Apply();
                        index++;
                        // 22.5 degrees
                        cam.transform.localPosition = renderPoints[index].position;
                        cam.transform.localRotation = Quaternion.Euler(renderPoints[index].rotation);
                        m_RenderTexture.Create();
                        cam.targetTexture = m_RenderTexture;
                        cam.Render();
                        RenderTexture.active = m_RenderTexture;
                        spriteRenders[index] = new Texture2D(resolution, resolution);
                        spriteRenders[index].ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
                        spriteRenders[index].Apply();
                        index++;
                        // 45 degrees
                        cam.transform.localPosition = renderPoints[index].position;
                        cam.transform.localRotation = Quaternion.Euler(renderPoints[index].rotation);
                        m_RenderTexture.Create();
                        cam.targetTexture = m_RenderTexture;
                        cam.Render();
                        RenderTexture.active = m_RenderTexture;
                        spriteRenders[index] = new Texture2D(resolution, resolution);
                        spriteRenders[index].ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
                        spriteRenders[index].Apply();
                        index++;
                        // 85 degrees
                        cam.transform.localPosition = renderPoints[index].position;
                        cam.transform.localRotation = Quaternion.Euler(renderPoints[index].rotation);
                        m_RenderTexture.Create();
                        cam.targetTexture = m_RenderTexture;
                        cam.Render();
                        RenderTexture.active = m_RenderTexture;
                        spriteRenders[index] = new Texture2D(resolution, resolution);
                        spriteRenders[index].ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
                        spriteRenders[index].Apply();
                        index++;
                    }
                    atlas = new Texture2D(4 * resolution, 8 * resolution);
                    atlas.PackTextures(spriteRenders, 0, 8 * resolution, false);                    
                    break;
                }
            default: return -1;
        }
        cam.enabled = false;
        cam.transform.parent = null;
        RenderTexture.active = null;

        foreach (Renderer r in allObjects)
        {
            r.gameObject.layer = savedLayer;
        }
         return lcontroller.RegisterLOD(new LODRegistrationTicket(regInfo, atlas, i_lpackType));
    }

}

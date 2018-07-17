using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LODSpriteMaker : MonoBehaviour {
    [SerializeField]
    GameObject objToShot;
    Camera cam;
    [SerializeField]
    Texture showing;

    private void Start()
    {
        cam = GetComponent<Camera>();
        MakeSpriteLODs(objToShot, new Vector3(0, -0.222f, 0.48f));
    }

    public GameObject MakeSpriteLODs(GameObject g, Vector3 correctionVector) {
        Vector3 savedPosition = g.transform.position;
        Quaternion savedRotation = g.transform.rotation;
        int savedLayer = g.layer;
        g.transform.position = transform.position + cam.transform.TransformDirection(correctionVector);
        g.transform.rotation = Quaternion.identity;
        var layerNumber = 8;
        gameObject.layer = layerNumber;
        ChangeLayerRecursively(g.transform, layerNumber);   
        
        cam.enabled = true;
        RenderTexture m_RenderTexture = new RenderTexture(128, 128, 8, RenderTextureFormat.ARGB32);
        m_RenderTexture.Create();
        cam.targetTexture = m_RenderTexture;
        cam.Render();
        cam.enabled = false;

        ChangeLayerRecursively(g.transform, savedLayer);
        g.transform.position = savedPosition;
        g.transform.rotation = savedRotation;

        showing = m_RenderTexture;
        RenderTexture.active = m_RenderTexture;
        Texture2D spriteBlank = new Texture2D(m_RenderTexture.width, m_RenderTexture.height);
        spriteBlank.ReadPixels(new Rect(0, 0, spriteBlank.width, spriteBlank.height), 0, 0);
        spriteBlank.Apply();
        RenderTexture.active = null;
        GameObject spr = new GameObject("lod");
        SpriteRenderer sr = spr.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(spriteBlank, new Rect(0,0, spriteBlank.width, spriteBlank.height), Vector2.one * 0.5f, 256);

        return null;
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

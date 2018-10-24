using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LODSpriteMaker : MonoBehaviour {
    public static LODSpriteMaker current {get; private set;}
    Camera cam;
   // [SerializeField]   Texture2D lastResult; // для тестов

    private void Awake()
    {
        current = this;  
        cam = GetComponent<Camera>();
    }

    public Texture2D MakeSpriteLODs(GameObject g, Vector3[] positions, Vector3[] angles, float cameraSize, Color i_backgroundColor) {
        cam.orthographicSize = cameraSize;
        cam.backgroundColor = new Color(i_backgroundColor.r, i_backgroundColor.g, i_backgroundColor.b, 0);
        int savedLayer = g.layer;
        var layerNumber = 8;
        gameObject.layer = layerNumber;
        ChangeLayerRecursively(g.transform, layerNumber);     
        
        int spriteSize = 64;       
        RenderTexture m_RenderTexture = new RenderTexture(spriteSize, spriteSize, 8, RenderTextureFormat.ARGB32);
        Texture2D[] spriteBlanks = new Texture2D[positions.Length];
        cam.transform.parent = g.transform;
        cam.enabled = true;
              
        for (int i = 0; i < positions.Length; i++)
            {
                cam.transform.localPosition = positions[i];
                cam.transform.localRotation = Quaternion.Euler(angles[i]);
                m_RenderTexture.Create();
                cam.targetTexture = m_RenderTexture;
                cam.Render();
                RenderTexture.active = m_RenderTexture;
                spriteBlanks[i] = new Texture2D(spriteSize, spriteSize);
                spriteBlanks[i].ReadPixels(new Rect(0,0, spriteSize, spriteSize), 0, 0);
                spriteBlanks[i].Apply();
            //if (positions.Length == 1) lastResult = spriteBlanks[i];
        }
        cam.enabled = false;
        cam.transform.parent = null;
        ChangeLayerRecursively(g.transform, savedLayer);
        RenderTexture.active = null;
        //GameObject spr = new GameObject("lod");        
        // SpriteRenderer sr = spr.AddComponent<SpriteRenderer>();
        // sr.sprite = Sprite.Create(spriteBlank, new Rect(0,0, spriteBlank.width, spriteBlank.height), Vector2.one * 0.5f, 128);
        if (positions.Length == 4)
        {
            Texture2D atlas = new Texture2D(2 * spriteSize, 2 * spriteSize);
            //lastResult = atlas;
            atlas.PackTextures(spriteBlanks, 0, 2 * spriteSize, false);
            return atlas;
        }
        else
        {
            //lastResult = spriteBlanks[0];
            return spriteBlanks[0];
        }
       
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

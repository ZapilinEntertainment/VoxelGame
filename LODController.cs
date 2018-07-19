using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelWithLOD
{
    public Transform transform;
    public bool spriteIsActive;
    public byte drawingSpriteIndex;
    public short lodPackIndex;

    public ModelWithLOD(Transform i_transform, bool i_spriteIsActive, byte i_drawingSpriteIndex, short i_lodPackIndex)
    {
        transform = i_transform;
        spriteIsActive = i_spriteIsActive;
        drawingSpriteIndex = i_drawingSpriteIndex;
        lodPackIndex = i_lodPackIndex;
    }
}

public class LODController : MonoBehaviour {
    List<ModelWithLOD> models = new List<ModelWithLOD>();
    static List<Sprite[]> lodPacks = new List<Sprite[]>(); // not destroying between loads
    static LODController current; // singleton
    float lodDistance = 5;
    Vector3 camPos;

    private void Awake()
    {
        camPos = GameMaster.camPos;
    }

    public static LODController GetCurrent()
    {
        if (current == null)
        {
            GameObject g = new GameObject("lodController");
            current = g.AddComponent<LODController>();
            current.models = new List<ModelWithLOD>();
            GameMaster.realMaster.AddToCameraUpdateBroadcast(g);
        }
        return current;
    }

    public void CameraUpdate(Transform cam)
    {  
        camPos = cam.transform.position;

        if (models.Count > 0)
        {
            for (int i = 0; i < models.Count; i++)
            {
                if (models[i].transform.parent.gameObject.activeSelf) TreeCheck(i);               
            }
        }
    }


     void TreeCheck(int index)
    {
        ModelWithLOD tree = models[index];
        Transform fullModelTransform = tree.transform.parent, spriteTransform = tree.transform;
        Vector3 treePos = fullModelTransform.position;
        if ((treePos - camPos).magnitude > lodDistance)
        {
            if (!tree.spriteIsActive)
            {
                fullModelTransform.GetChild(0).gameObject.SetActive(false);
                fullModelTransform.GetChild(1).gameObject.SetActive(false);
                spriteTransform.gameObject.SetActive(true);
                tree.spriteIsActive = true;
            }
            byte spriteStatus = 0;
            float angle = Vector3.Angle(Vector3.up, camPos - treePos);
            if (angle < 30)
            {
                if (angle < 10) spriteStatus = 3;
                else spriteStatus = 2;
            }
            else
            {
                if (angle > 70) spriteStatus = 0;
                else spriteStatus = 1;
            }
            if (spriteStatus != tree.drawingSpriteIndex)
            {
                spriteTransform.GetComponent<SpriteRenderer>().sprite = lodPacks[tree.lodPackIndex][spriteStatus];
                tree.drawingSpriteIndex = spriteStatus;
            }
            if (spriteStatus == 0)
            {
                Vector3 dir = camPos - treePos;
                dir.y = 0;
                spriteTransform.forward = dir.normalized;
            }
            else
            {
                spriteTransform.LookAt(camPos);
            }
        }
        else
        {
            if (tree.spriteIsActive)
            {
                fullModelTransform.GetChild(0).gameObject.SetActive(true);            
                fullModelTransform.GetChild(1).gameObject.SetActive(true);
                spriteTransform.gameObject.SetActive(false);
                tree.spriteIsActive = false;
            }
        }
    }

    public void AddObject(Transform t, short lodPackIndex)
    {
        if (t == null) return;
        ModelWithLOD newTree = new ModelWithLOD(t, false, 0, lodPackIndex);
        //GameMaster.realMaster.standartSpritesList.Add(t.gameObject);
        models.Add(newTree);
        TreeCheck(models.Count - 1);        
    }

    public static short AddSpritePack(Sprite[] sprites)
    {
        lodPacks.Add(sprites);
        return (short)(lodPacks.Count - 1);
    }
    
    public void ChangeModelSpritePack(Transform t, short newPackIndex)
    {
        if (models.Count == 0) return;
        else
        {
            int i = 0;
            while (i < models.Count)
            {
                Transform mt = models[i].transform;
                if (mt == null)
                {
                    models.RemoveAt(i);
                    continue;
                }
                else
                {
                    if (mt == t)
                    {
                        if (models[i].lodPackIndex != newPackIndex)
                        {
                            models[i].lodPackIndex = newPackIndex;
                            TreeCheck(i);
                        }
                    }
                    i++;
                }
            }
        }
    }
}

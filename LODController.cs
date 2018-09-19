using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ModelType { Tree, Boulder}

public class ModelWithLOD
{
    public Transform transform;
    public bool spriteIsActive;
    public byte drawingSpriteIndex;
    public short lodPackIndex;
    public ModelType type;

    public ModelWithLOD(Transform i_transform, ModelType i_type, bool i_spriteIsActive, byte i_drawingSpriteIndex, short i_lodPackIndex)
    {
        transform = i_transform;
        type = i_type;
        spriteIsActive = i_spriteIsActive;
        drawingSpriteIndex = i_drawingSpriteIndex;
        lodPackIndex = i_lodPackIndex;
    }
}

public class LODController : MonoBehaviour {
    List<ModelWithLOD> models = new List<ModelWithLOD>();
    Vector3 camPos;

    static List<Sprite[]> lodPacks = new List<Sprite[]>(); // not destroying between loads
    static LODController current; // singleton
    public static float lodDistance { get; private set; }
    const string LOD_DIST_KEY = "LOD distance";

    private void Awake()
    {
        if (PlayerPrefs.HasKey(LOD_DIST_KEY)) lodDistance = PlayerPrefs.GetFloat(LOD_DIST_KEY);
        else lodDistance = 5;
    }

    public static void SetLODdistance(float f)
    {
        if (f != lodDistance)
        {
            lodDistance = f;
            PlayerPrefs.SetFloat(LOD_DIST_KEY, lodDistance);
            current.CameraUpdate();
        }
    }

    public static LODController GetCurrent()
    {
        if (current == null)
        {
            GameObject g = new GameObject("lodController");
            current = g.AddComponent<LODController>();
            current.models = new List<ModelWithLOD>();
            FollowingCamera.main.cameraChangedEvent += current.CameraUpdate;
        }
        return current;
    }

    public void CameraUpdate()
    {
        camPos = FollowingCamera.camPos;
        if (models.Count > 0)
        {
            int i = 0;
            while (i < models.Count)
            {
                if (models[i].transform == null)
                {
                    models.RemoveAt(i);
                    continue;
                }
                else
                {
                    switch (models[i].type)
                    {
                        case ModelType.Tree: TreeCheck(i); break; // какого фига? надо переделать на нормальные перечисления!
                        case ModelType.Boulder: BoulderCheck(i);break;
                    }
                    i++;
                }
            }
        }
    }


     void TreeCheck(int index)
    {
        ModelWithLOD tree = models[index];
        Transform modelParent = tree.transform.parent, spriteTransform = tree.transform;
        Vector3 treePos = modelParent.position;
        if ((treePos - camPos).magnitude > lodDistance)
        {
            if (!tree.spriteIsActive)
            {
                modelParent.GetChild(0).gameObject.SetActive(false);
                modelParent.GetChild(1).gameObject.SetActive(false);
                spriteTransform.gameObject.SetActive(true);
                tree.spriteIsActive = true;
            }
            byte spriteStatus = 0;
            float angle = Vector3.Angle(Vector3.up, camPos - treePos);
            if (angle < 20)
            {
                if (angle < 10) spriteStatus = 3;
                else spriteStatus = 2;
            }
            else
            {
                if (angle > 80) spriteStatus = 0;
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
           // else
           // {
          //      spriteTransform.LookAt(camPos);
           // }
        }
        else
        {
            if (tree.spriteIsActive)
            {
                modelParent.GetChild(0).gameObject.SetActive(true);            
                modelParent.GetChild(1).gameObject.SetActive(true);
                spriteTransform.gameObject.SetActive(false);
                tree.spriteIsActive = false;
            }
        }
    }
    void BoulderCheck(int index)
    {
        ModelWithLOD m = models[index];
        Vector3 pos = m.transform.position;
        if ((camPos - pos).magnitude > lodDistance)
        {
            if ( !m.spriteIsActive )
            {
                m.transform.GetChild(1).gameObject.SetActive(true);
                m.transform.GetChild(0).gameObject.SetActive(false);
                m.spriteIsActive = true;
            }
            //m.transform.GetChild(1).LookAt(camPos);
        }
        else
        {
            if (m.spriteIsActive)
            {
                m.transform.GetChild(1).gameObject.SetActive(false);
                m.transform.GetChild(0).gameObject.SetActive(true);
                m.spriteIsActive = false;
            }
        }
    }

    public void AddObject(Transform t, ModelType type, short lodPackIndex)
    {
        if (t == null) return;
        ModelWithLOD newModel = new ModelWithLOD(t, type, false, 0, lodPackIndex);
        //GameMaster.realMaster.standartSpritesList.Add(t.gameObject);
        models.Add(newModel);
        switch (type)
        {
            case ModelType.Tree:
                TreeCheck(models.Count - 1); break;
            case ModelType.Boulder:
                SpriteRenderer sr = newModel.transform.GetChild(1).GetComponent<SpriteRenderer>();
                sr.sprite = lodPacks[lodPackIndex][0];
                sr.sharedMaterial = PoolMaster.billboardMaterial;
                BoulderCheck(models.Count - 1);
                break;
        }               
    }

    public static short AddSpritePack(Sprite[] sprites)
    {
        lodPacks.Add(sprites);
        return (short)(lodPacks.Count - 1);
    }
    
    public void ChangeModelSpritePack(Transform t, ModelType ftype, short newPackIndex)
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
                    if (models[i].type == ftype)
                    {
                        if (mt == t)
                        {
                            if (models[i].lodPackIndex != newPackIndex)
                            {
                                models[i].lodPackIndex = newPackIndex;
                                TreeCheck(i);
                                return;
                            }
                        }
                    }
                    i++;
                }
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ModelType { Boulder}

public class ModelWithLOD
{
    public Transform transform;
    public bool? drawStatus; // null - totally disabled, false - draw sprite, true - draw model;
    public byte drawingSpriteIndex;
    public short lodPackIndex;
    public ModelType type;

    public ModelWithLOD(Transform i_transform, ModelType i_type, bool? i_drawStatus, byte i_drawingSpriteIndex, short i_lodPackIndex)
    {
        transform = i_transform;
        type = i_type;
        drawStatus = i_drawStatus;
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
    const float BOULDER_SPRITE_MAX_VISIBILITY = 10;

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
        // ДОДЕЛАТЬ
        camPos = FollowingCamera.camPos;
        Transform camTransform = FollowingCamera.camTransform;
        float zpos, dist;
        Vector3 mpos;
        bool? newDrawStatus;
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
                    ModelWithLOD m = models[i];
                    if (m.transform.gameObject.activeSelf)
                    {
                        mpos = m.transform.position;
                        zpos = camTransform.InverseTransformPoint(mpos).z;
                        if (zpos > 0)
                        {
                            dist = (mpos - camPos).magnitude;
                            switch (m.type)
                            {
                                case ModelType.Boulder:
                                    if (dist > BOULDER_SPRITE_MAX_VISIBILITY) newDrawStatus = null;
                                    else
                                    {
                                        if (dist > lodDistance) newDrawStatus = false;
                                        else newDrawStatus = true;
                                    }
                                    break;
                            }
                        }
                    }
                    i++;
                }
            }
        }
    }


    void BoulderCheck(int index)
    {
        ModelWithLOD m = models[index];
        Vector3 pos = m.transform.position;
        float dist = (camPos - pos).magnitude;
        //if (dist > lodDistance)
        //{
          //  if ( !m.spriteIsActive )
           // {
           //     m.transform.GetChild(1).gameObject.SetActive(true);
            //    m.transform.GetChild(0).gameObject.SetActive(false);
            //    m.spriteIsActive = true;
           // }
            //m.transform.GetChild(1).LookAt(camPos);
       // }
       // else
      //  {
          //  if (m.spriteIsActive)
         //   {
             //   m.transform.GetChild(1).gameObject.SetActive(false);
             //   m.transform.GetChild(0).gameObject.SetActive(true);
            //    m.spriteIsActive = false;
           // }
        //}
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="t"> Transform of the lod sprite GO </param>
    /// <param name="type"></param>
    /// <param name="lodPackIndex"></param>
    public void AddObject(Transform t, ModelType type, short lodPackIndex)
    {
        if (t == null) return;
        ModelWithLOD newModel = new ModelWithLOD(t, type, false, 0, lodPackIndex);
        //GameMaster.realMaster.standartSpritesList.Add(t.gameObject);
        models.Add(newModel);
        switch (type)
        {
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

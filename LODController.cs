using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public sealed class ModelWithLOD
{
    public Transform transform;
    public SpriteRenderer spriteRenderer;
    public bool? drawStatus; // null - totally disabled, false - draw sprite, true - draw model;
    public byte drawingSpriteIndex;
    public Sprite[] sprites;
    public int ticketIndex = -1;
}

public struct LODRegisterInfo
{
    public int modelTypeID { get; private set; }
    public int modelSubID { get; private set; }
    public int addInfo { get; private set; }

    public LODRegisterInfo(int i_modelTypeID, int i_modelSubID, int i_addInfo)
    {
        modelTypeID = i_modelTypeID;
        modelSubID = i_modelSubID;
        addInfo = i_addInfo;
    }

    public static bool operator == (LODRegisterInfo A, LODRegisterInfo B)
    {
        return ( (A.modelTypeID == B.modelTypeID) && (A.modelSubID == B.modelSubID) && (A.addInfo == B.addInfo) );
    }
    public static bool operator != (LODRegisterInfo A, LODRegisterInfo B)
    {
        return !(A == B);
    }
    public override int GetHashCode()
    {
        var hashCode = 67631244;
        hashCode = hashCode * -1521134295 + modelTypeID.GetHashCode();
        hashCode = hashCode * -1521134295 + modelSubID.GetHashCode();
        hashCode = hashCode * -1521134295 + addInfo.GetHashCode();
        return hashCode;
    }
    public override bool Equals(object obj)
    {
        if (!(obj is LODRegisterInfo))
        {
            return false;
        }

        var info = (LODRegisterInfo)obj;
        return modelTypeID == info.modelTypeID &&
               modelSubID == info.modelSubID &&
               addInfo == info.addInfo;
    }
}

public sealed class LODRegistrationTicket
{
    public LODRegisterInfo registerInfo { get; private set; }
    public Texture2D spriteAtlas { get; private set; }
    public Sprite[] sprites { get; private set; }
    public LODPackType lodPackType { get; private set; }
    public int activeUsers = 0; // для очистки ненужных текстур

    public LODRegistrationTicket(LODRegisterInfo regInfo, Texture2D i_spriteAtlas, LODPackType i_packType)
    {
        registerInfo = regInfo;
        spriteAtlas = i_spriteAtlas;
        lodPackType = i_packType;
        switch (lodPackType)
        {
            case LODPackType.Full:
                {
                    sprites = new Sprite[32];
                    int index = 0;
                    float p = spriteAtlas.width / 4;
                    Vector3 pivot = Vector3.one * 0.5f;
                    for (int i = 0; i < 8; i++)
                    {
                        sprites[index++] = Sprite.Create(spriteAtlas, new Rect(0, i * p, p, p), pivot);
                        sprites[index++] = Sprite.Create(spriteAtlas, new Rect(p, i * p, p, p), pivot);
                        sprites[index++] = Sprite.Create(spriteAtlas, new Rect(2 * p, i * p, p, p), pivot);
                        sprites[index++] = Sprite.Create(spriteAtlas, new Rect(3 * p, i * p, p, p), pivot);
                    }
                    break;
                }
            case LODPackType.OneSide:
                {
                    sprites = new Sprite[4];
                    float p = spriteAtlas.width / 2;
                    Vector3 pivot = Vector3.one * 0.5f;
                    sprites[0] = Sprite.Create(spriteAtlas, new Rect(0, 0, p, p), pivot);
                    sprites[1] = Sprite.Create(spriteAtlas, new Rect(p, 0, p, p), pivot);
                    sprites[2] = Sprite.Create(spriteAtlas, new Rect(0, p, p, p), pivot);
                    sprites[3] = Sprite.Create(spriteAtlas, new Rect(p, p, p, p), pivot);
                    break;
                }
            default:
                sprites = new Sprite[1];
                break;
        }
    }
}

public sealed class LODController : MonoBehaviour {
    private static LODController current; // singleton
    public static float lodCoefficient { get; private set; } // 0 is always lod, 1 is when it matches its real pixelsize

    public const float SECOND_LOD_ANGLE = 22.5f, THIRD_LOD_ANGLE = 45, FOURTH_LOD_ANGLE = 85;
    public const int CONTAINER_MODEL_ID = 1, OAK_MODEL_ID = 2;

    public List<LODRegistrationTicket> registeredLODs { get; private set; }

    private List<ModelWithLOD> models = new List<ModelWithLOD>();
    private Vector3 camPos;    
    private const string LOD_DIST_KEY = "LOD distance";
    

    private void Awake()
    {
        if (PlayerPrefs.HasKey(LOD_DIST_KEY)) lodCoefficient = PlayerPrefs.GetFloat(LOD_DIST_KEY);
        else lodCoefficient = 1;
    }

    public static void SetLODdistance(float f)
    {
        if (f != lodCoefficient)
        {
            lodCoefficient = f;
            PlayerPrefs.SetFloat(LOD_DIST_KEY, lodCoefficient);
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
        Transform camTransform = FollowingCamera.camTransform;
        int count = models.Count;
        if (count > 0)
        {
            int i = 0;
            ModelWithLOD mwl;
            Vector3 pos;
            while (i < count)
            {
                mwl = models[i];
                if (mwl.transform == null)
                {
                    if (mwl.ticketIndex > 0)
                    {
                        LODRegistrationTicket ticket = registeredLODs[mwl.ticketIndex];
                        ticket.activeUsers--;
                        // надо впаять таймер удаления, чтобы не гонять лоды туда-сюда
                    }
                    models.RemoveAt(i);
                    continue;
                }
                else
                {
                    pos = mwl.transform.position;
                    //....awaiting
                }
            }
        }
    }

    public int LOD_existanceCheck(LODRegisterInfo i_regInfo)
    {
        if (registeredLODs.Count == 0) return -1;
        else
        {
            for (int i = 0; i < registeredLODs.Count; i++)
            {
                 if (registeredLODs[i].registerInfo == i_regInfo) return i;
            }
            return -1;
        }
    }
    public int RegisterLOD(LODRegistrationTicket lticket)
    {
        registeredLODs.Add(lticket);
        return registeredLODs.Count - 1;
    }

    public void TakeCare(Transform modelHolder, int indexInRegistered)
    {
        if (indexInRegistered == -1 | modelHolder == null) return;
        ModelWithLOD mwl = new ModelWithLOD();
        mwl.transform = modelHolder;
        mwl.spriteRenderer = modelHolder.GetChild(1).GetComponent<SpriteRenderer>();
        LODRegistrationTicket ticket = registeredLODs[indexInRegistered];
        mwl.sprites = ticket.sprites;
        ticket.activeUsers++;
        mwl.drawingSpriteIndex = 0;
        mwl.spriteRenderer.sprite = mwl.sprites[0];
        mwl.drawStatus = true;
        models.Add(mwl);
    }
}

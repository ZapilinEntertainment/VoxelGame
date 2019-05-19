using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#region auxiliary classes
public class PointLODModel
{
    public GameObject model3d;
    public SpriteRenderer spriteRenderer;
    public bool? drawStatus;
    public float lodSqrDistance, visibilitySqrDistance;
}

public sealed class AdvancedLODModel : PointLODModel
{
    public bool oneSide = true;
    public int ticketIndex = -1;
    public byte drawingSpriteIndex = 0;
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
        float PIXELS_PER_UNIT = LODSpriteMaker.PIXELS_PER_UNIT;
        switch (lodPackType)
        {
            case LODPackType.Full:
                {
                    sprites = new Sprite[32];
                    int index = 0;
                    float p = spriteAtlas.width / 4;
                    Vector3 pivot = Vector2.one * 0.5f;
                    for (int i = 0; i < 8; i++)
                    {
                        sprites[index++] = Sprite.Create(spriteAtlas, new Rect(0, i * p, p, p), pivot, PIXELS_PER_UNIT);
                        sprites[index++] = Sprite.Create(spriteAtlas, new Rect(p, i * p, p, p), pivot, PIXELS_PER_UNIT);
                        sprites[index++] = Sprite.Create(spriteAtlas, new Rect(2 * p, i * p, p, p), pivot, PIXELS_PER_UNIT);
                        sprites[index++] = Sprite.Create(spriteAtlas, new Rect(3 * p, i * p, p, p), pivot, PIXELS_PER_UNIT);
                    }
                    break;
                }
            case LODPackType.OneSide:
                {
                    sprites = new Sprite[4];
                    float p = spriteAtlas.width / 2;
                    Vector3 pivot = Vector2.one * 0.5f;
                    sprites[0] = Sprite.Create(spriteAtlas, new Rect(0, 0, p, p), pivot, PIXELS_PER_UNIT);
                    sprites[1] = Sprite.Create(spriteAtlas, new Rect(p, 0, p, p), pivot, PIXELS_PER_UNIT);
                    sprites[2] = Sprite.Create(spriteAtlas, new Rect(0, p, p, p), pivot, PIXELS_PER_UNIT);
                    sprites[3] = Sprite.Create(spriteAtlas, new Rect(p, p, p, p), pivot, PIXELS_PER_UNIT);
                    break;
                }
            default:
                sprites = new Sprite[1];
                sprites[0] = Sprite.Create(spriteAtlas, new Rect(0, 0, spriteAtlas.width, spriteAtlas.height), Vector2.one * 0.5f, PIXELS_PER_UNIT);
                break;
        }
    }
}
#endregion

public sealed class LODController : MonoBehaviour
{
    private static LODController current; // singleton
    public static float lodCoefficient { get; private set; } // 0 is always lod, 1 is when it matches its real pixelsize

    public const float FIRST_LOD_THRESHOLD = 22.5f, SECOND_LOD_THRESHOLD = 50, THIRD_LOD_THRESHOLD = 85;
    public const int CONTAINER_MODEL_ID = 1, OAK_MODEL_ID = 2;

    public List<LODRegistrationTicket> registeredLODs { get; private set; }

    private List<PointLODModel> pointLODmodels;
    private List<AdvancedLODModel> advancedLODmodels;
    private Vector3 camPos;
    private const string LOD_DIST_KEY = "LOD distance";


    static LODController()
    {
        if (PlayerPrefs.HasKey(LOD_DIST_KEY)) lodCoefficient = PlayerPrefs.GetFloat(LOD_DIST_KEY);
        else {
            lodCoefficient = 0.5f;
            PlayerPrefs.SetFloat(LOD_DIST_KEY, lodCoefficient);
                }
    }

    public static void SetLODdistance(float f)
    {
        if (f != lodCoefficient)
        {
            lodCoefficient = f;
            PlayerPrefs.SetFloat(LOD_DIST_KEY, lodCoefficient);
            PlayerPrefs.Save();
            if (FollowingCamera.main != null) FollowingCamera.main.WeNeedUpdate();
        }
    }

    public static LODController GetCurrent()
    {
        if (current == null)
        {
            GameObject g = new GameObject("lodController");
            current = g.AddComponent<LODController>();
            current.pointLODmodels = new List<PointLODModel>();
            current.advancedLODmodels = new List<AdvancedLODModel>();
            current.registeredLODs = new List<LODRegistrationTicket>();
            FollowingCamera.main.cameraChangedEvent += current.CameraUpdate;
        }
        return current;
    }

    public void CameraUpdate()
    {
        camPos = FollowingCamera.camPos;
        float sqdist = 0;
        bool? newStatus = null;
        int i = 0, count = pointLODmodels.Count;
        Vector3 pos;

        if (count > 0)
        {
            PointLODModel plm;
            if (!PoolMaster.shadowCasting)
            {
                while (i < count)
                {
                    plm = pointLODmodels[i];
                    if (plm.model3d == null)
                    {
                        pointLODmodels.RemoveAt(i);
                        count = pointLODmodels.Count;
                        continue;
                    }
                    else
                    {
                        i++;
                        pos = plm.model3d.transform.position;
                        sqdist = (pos - camPos).sqrMagnitude;
                        if (sqdist > plm.lodSqrDistance * lodCoefficient)
                        {
                            if (sqdist < plm.visibilitySqrDistance * lodCoefficient) newStatus = false;
                            else newStatus = null;
                        }
                        else newStatus = true;
                        if (newStatus != plm.drawStatus)
                        {
                            if (newStatus == true)
                            {// model view
                                plm.model3d.SetActive(true);
                                plm.spriteRenderer.enabled = false;
                            }
                            else
                            {
                                if (newStatus == false)
                                {// lod
                                    plm.model3d.SetActive(false);
                                    plm.spriteRenderer.enabled = true;
                                }
                                else
                                {// not visible
                                    plm.model3d.SetActive(false);
                                    plm.spriteRenderer.enabled = false;
                                }
                            }
                            plm.drawStatus = newStatus;
                        }
                    }
                }
            }
            else
            {
                //точная копия, только с разворотом. 
                while (i < count)
                {
                    plm = pointLODmodels[i];
                    if (plm.model3d == null)
                    {
                        pointLODmodels.RemoveAt(i);
                        count = pointLODmodels.Count;
                        continue;
                    }
                    else
                    {
                        i++;
                        pos = plm.model3d.transform.position;
                        sqdist = (pos - camPos).sqrMagnitude;
                        if (sqdist > plm.lodSqrDistance * lodCoefficient)
                        {
                            if (sqdist < plm.visibilitySqrDistance * lodCoefficient) newStatus = false;
                            else newStatus = null;
                        }
                        else newStatus = true;
                        if (newStatus != plm.drawStatus)
                        {
                            if (newStatus == true)
                            {// model view
                                plm.model3d.SetActive(true);
                                plm.spriteRenderer.enabled = false;
                            }
                            else
                            {
                                if (newStatus == false)
                                {// lod
                                    plm.model3d.SetActive(false);
                                    plm.spriteRenderer.enabled = true;                                    
                                }
                                else
                                {// not visible
                                    plm.model3d.SetActive(false);
                                    plm.spriteRenderer.enabled = false;
                                }
                            }
                            plm.drawStatus = newStatus;
                        }
                        plm.spriteRenderer.transform.rotation = Quaternion.LookRotation(plm.spriteRenderer.transform.position - camPos, Vector3.up);
                    }
                }
            }
        }

        count = advancedLODmodels.Count;
        if (count > 0)
        {
            i = 0;
            AdvancedLODModel alm;
            float angle = 0;
            byte newSpriteIndex = 0;
            while (i < count)
            {
                alm = advancedLODmodels[i];
                if (alm.model3d == null)
                {
                    advancedLODmodels.RemoveAt(i);
                    count = advancedLODmodels.Count;
                    continue;
                }
                else
                {
                    i++;
                    if (alm.model3d == null)
                    {
                        pointLODmodels.RemoveAt(i);
                        continue;
                    }
                    else
                    {
                        i++;
                        pos = alm.model3d.transform.position;
                        sqdist = (pos - camPos).sqrMagnitude;
                        if (sqdist > alm.lodSqrDistance * lodCoefficient)
                        {
                            if (sqdist < alm.visibilitySqrDistance * lodCoefficient) newStatus = false;
                            else newStatus = null;
                        }
                        else newStatus = true;
                        if (newStatus != alm.drawStatus)
                        {
                            if (newStatus == true)
                            {// model view
                                alm.model3d.SetActive(true);
                                alm.spriteRenderer.enabled = false;
                            }
                            else
                            {
                                if (newStatus == false)
                                {// lod
                                    alm.model3d.SetActive(false);
                                    alm.spriteRenderer.enabled = true;
                                }
                                else
                                {// not visible
                                    alm.model3d.SetActive(false);
                                    alm.spriteRenderer.enabled = false;
                                }
                            }
                            alm.drawStatus = newStatus;
                            if (newStatus == false)
                            {
                                if (alm.oneSide)
                                {
                                    angle = Vector3.Angle(camPos - pos, Vector3.up);
                                    if (angle > SECOND_LOD_THRESHOLD)
                                    {
                                        if (angle > THIRD_LOD_THRESHOLD) newSpriteIndex = 3;
                                        else newSpriteIndex = 2;
                                    }
                                    else
                                    {
                                        if (angle < FIRST_LOD_THRESHOLD) newSpriteIndex = 0;
                                        else newSpriteIndex = 1;
                                    }
                                    if (newSpriteIndex != alm.drawingSpriteIndex)
                                    {
                                        alm.drawingSpriteIndex = newSpriteIndex;
                                        alm.spriteRenderer.sprite = registeredLODs[alm.ticketIndex].sprites[newSpriteIndex];
                                    }
                                }
                                else
                                {
                                    //full check
                                }
                            }
                        }
                    }
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

    public void TakeCare(Transform modelHolder, int indexInRegistered, float i_lodSqrDist, float i_visibilitySqrDist)
    {
        if (indexInRegistered == -1 | modelHolder == null) return;
        LODRegistrationTicket ticket = registeredLODs[indexInRegistered];
        ticket.activeUsers++;
        PointLODModel mwl;
        if (ticket.lodPackType == LODPackType.Point)
        {
            mwl = new PointLODModel();
            pointLODmodels.Add(mwl);
        }
        else
        {
            AdvancedLODModel aml = new AdvancedLODModel();
            mwl = aml;
            aml.ticketIndex = indexInRegistered;
            advancedLODmodels.Add(aml);
        }

        mwl.model3d = modelHolder.GetChild(0).gameObject;
        mwl.spriteRenderer = modelHolder.GetChild(1).GetComponent<SpriteRenderer>();
        mwl.spriteRenderer.sprite = ticket.sprites[0];
        if (mwl.spriteRenderer == null) return;
        mwl.lodSqrDistance = i_lodSqrDist * i_lodSqrDist;
        mwl.visibilitySqrDistance = i_visibilitySqrDist * i_visibilitySqrDist;

        if (FollowingCamera.cam != null)
        {
            Vector3 pos = modelHolder.position;
            float sqdist = (pos - camPos).sqrMagnitude;
            if (sqdist > mwl.visibilitySqrDistance)
            {
                mwl.drawStatus = null;
                mwl.model3d.SetActive(false);
                mwl.spriteRenderer.enabled = false;
            }
            else
            {
                if (sqdist < mwl.lodSqrDistance)
                {
                    mwl.drawStatus = true;
                    mwl.model3d.SetActive(true);
                    mwl.spriteRenderer.enabled = false;
                }
                else
                {
                    mwl.drawStatus = false;
                    mwl.model3d.SetActive(false);
                    switch (ticket.lodPackType)
                    {
                        case LODPackType.Point:
                            mwl.spriteRenderer.sprite = ticket.sprites[0];
                            break;
                        case LODPackType.OneSide:
                            {
                                //#oneside sprite check
                                AdvancedLODModel alm = mwl as AdvancedLODModel;
                                alm.oneSide = ticket.lodPackType == LODPackType.OneSide;
                                float angle = Vector3.Angle(camPos - pos, Vector3.up);
                                if (angle > SECOND_LOD_THRESHOLD)
                                {
                                    if (angle > THIRD_LOD_THRESHOLD) alm.drawingSpriteIndex = 3;
                                    else alm.drawingSpriteIndex = 2;
                                }
                                else
                                {
                                    if (angle < FIRST_LOD_THRESHOLD) alm.drawingSpriteIndex = 0;
                                    else alm.drawingSpriteIndex = 1;
                                }
                                alm.spriteRenderer.sprite = ticket.sprites[alm.drawingSpriteIndex];
                                //eo oneside sprite check
                                break;
                            }
                    }
                    mwl.spriteRenderer.enabled = true;
                }
            }
        }
        else
        {
            mwl.spriteRenderer.enabled = false;           
            mwl.drawStatus = null;
            mwl.model3d.SetActive(false);
        }
    }
}

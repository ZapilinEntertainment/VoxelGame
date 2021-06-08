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

public sealed class PlaneBoundLODModel {
    private Plane basement;
    private GameObject model;
    private SpriteRenderer spriter;
    private bool? usingLOD;

    public PlaneBoundLODModel(Plane p, GameObject m, SpriteRenderer sr)
    {
        basement = p;
        model = m;
        spriter = sr;
        if (basement == null) throw new System.Exception("lod was assigned to unexisting object");
        else
        {
            basement.visibilityChangedEvent += this.SetVisibility;
        }
        SetVisibility(basement.visibilityMode, true);
    }
    public void SetVisibility(VisibilityMode vmode, bool forcedRefresh)
    {
        byte vm = (byte)vmode;
        bool? newDrawMode;
        if (vm >= (byte)VisibilityMode.HugeObjectsLOD )  newDrawMode = null; 
        else
        {
            if (vm < (byte)VisibilityMode.SmallObjectsLOD) newDrawMode = false;
            else newDrawMode = true;
        }
        if (newDrawMode != usingLOD | forcedRefresh)
        {
            usingLOD = newDrawMode;
            if (usingLOD == null)
            {
                model.SetActive(false);
                spriter.enabled = false;
            }
            else
            {
                if (usingLOD == true)
                {
                    model.SetActive(false);
                    spriter.enabled = true;
                }
                else
                {
                    model.SetActive(true);
                    spriter.enabled = false;
                }
            }
        }
    }
    public void ChangeBasement(Plane p) {
        if (basement != null) basement.visibilityChangedEvent -= this.SetVisibility;
        basement = p;
        if (basement == null) throw new System.Exception("lod was assigned to unexisting object");
        else
        {
            basement.visibilityChangedEvent += this.SetVisibility;
        }
    }
    public void PrepareToDestroy()
    {
        if (basement != null) basement.visibilityChangedEvent -= this.SetVisibility;
    }

    public override bool Equals(object obj)
    {
        // Check for null values and compare run-time types.
        if (obj == null || GetType() != obj.GetType())
            return false;

        PlaneBoundLODModel b = (PlaneBoundLODModel)obj;
        return basement == b.basement && model == b.model && spriter == b.spriter;
    }
    public override int GetHashCode()
    {
        int x = 0;
        if (usingLOD == null) x = -1;
        else
        {
            if (usingLOD == true) x = 2;
            else x = 1;
        }
        x += model.GetHashCode();
        x -= spriter.GetHashCode();
        x += basement.GetHashCode();
        return x;
    }

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
    // НЕ НУЖЕН И ДОЛЖЕН БЫТЬ УДАЛЕН

    private static LODController current; // singleton

    public const float FIRST_LOD_THRESHOLD = 22.5f, SECOND_LOD_THRESHOLD = 50, THIRD_LOD_THRESHOLD = 85;
    public const int CONTAINER_MODEL_ID = 1, OAK_MODEL_ID = 2;

    public List<LODRegistrationTicket> registeredLODs { get; private set; }

    private List<PointLODModel> pointLODmodels;
    private List<AdvancedLODModel> advancedLODmodels;
    private Vector3 camPos;

    public static LODController GetCurrent()
    {
        if (current == null)
        {
            GameObject g = new GameObject("lodController");
            current = g.AddComponent<LODController>();
            current.pointLODmodels = new List<PointLODModel>();
            current.advancedLODmodels = new List<AdvancedLODModel>();
            current.registeredLODs = new List<LODRegistrationTicket>();
        }
        return current;
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

    public void SetInControl(Transform modelHolder, int indexInRegistered, float i_lodSqrDist, float i_visibilitySqrDist)
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
    public PlaneBoundLODModel SetInControl(Plane basement, GameObject model, SpriteRenderer sr, int indexInRegistered)
    {
        if (indexInRegistered == -1 || model == null ||  sr== null|| basement == null) return null;
        LODRegistrationTicket ticket = registeredLODs[indexInRegistered];
        ticket.activeUsers++;
        sr.sprite = ticket.sprites[0];
        return new PlaneBoundLODModel(basement, model, sr);
    }
}

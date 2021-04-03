using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnchorBasement : WorkBuilding
{
    public const float POWER_CONSUMPTION = -1500f, MAX_EXCESS_POWER = 15000f;

    public static bool CheckSpecialBuildingCondition(Plane p, ref string refusalReason)
    {
        if (p.faceIndex == Block.DOWN_FACE_INDEX) return true;
        return false;
    }


    override public void SetBasement(Plane b, PixelPosByte pos)
    {
        if (b == null) return;
        SetBuildingData(b, pos, false);        
    }

    public void StartActivating(System.Action endFunction)
    {
        GameMaster.audiomaster.MakeSoundEffect(SoundEffect.FD_anchorLaunch);
        // graphic effect
        //time        
        endFunction();
    }

    // добавить влияние на global map и вывод сообщения о том, что оcтров заякорен

    public void AddInnerSector()
    {

    }

    public enum FDSectorType : byte { Utilitary, LivingQuarters, Smeltery}
}

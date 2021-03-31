using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnchorBasement : WorkBuilding
{
    public const float POWER_CONSUMPTION = -10000f, MAX_EXCESS_POWER = 15000f;

    public static bool CheckSpecialBuildingCondition(Plane p, ref string refusalReason)
    {
        return false;
    }

    public void StartActivating(System.Action endFucntion)
    {

    }

    // добавить влияние на global map и вывод сообщения о том, что оcтров заякорен

    public void AddInnerSector()
    {

    }

    public enum FDSectorType : byte { Utilitary, LivingQuarters, Smeltery}
}

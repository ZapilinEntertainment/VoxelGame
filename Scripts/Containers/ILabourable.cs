using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILabourable 
{
    float GetLabourCoefficient();
    void LabourUpdate();
    /// <summary>
    /// returns excess workers
    /// </summary>
    int AddWorkers(int x);
    void FreeWorkers(int x);
    void FreeWorkers();
    bool ShowWorkspeed();
    string UI_GetProductionSpeedInfo();
}


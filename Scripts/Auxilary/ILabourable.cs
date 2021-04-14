using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILabourable 
{
    bool IsWorksite();
    float GetLabourCoefficient();
    void LabourUpdate();
    /// <summary>
    /// returns excess workers
    /// </summary>
    int AddWorkers(int x);
    int FreeWorkers(int x);
    int FreeWorkers();
    int GetWorkersCount();
    int GetMaxWorkersCount();
    bool MaximumWorkersReached();
    bool ShowUIInfo();
    string UI_GetInfo();
    void DisabledOnGUI();
}


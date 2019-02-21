using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NotificationSound : ushort { Default, ColonyFounded,PowerFailure, BatteryCharged, NotEnoughResources, NotEnoughMoney,
NotEnoughSlots, StorageOverload, newQuestAvailable, newObjectFound, SystemError}
public class Audiomaster : MonoBehaviour {
    private AudioSource notificationSource;

    private AudioClip[] notificationSounds;

    public void Prepare()
    {
        notificationSource = gameObject.AddComponent<AudioSource>();
    }

    public void Notify(NotificationSound type)
    {

    }
}

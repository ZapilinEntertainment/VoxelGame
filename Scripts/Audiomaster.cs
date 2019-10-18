using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NotificationSound : ushort { Default, ColonyFounded, PowerFailure, BatteryCharged, NotEnoughResources, NotEnoughMoney,
NotEnoughSlots, StorageOverload, newQuestAvailable, newObjectFound, SystemError, Disagree, CrewTaskCompleted, HQ_Upgraded}
public enum SoundEffect: byte { Default, DicesRoll, Lightning, RollFail, SuccessfulRoll, Thunder}
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
    public void MakeSoundEffect(SoundEffect setype, Vector3 pos)
    {

    }
    public void MakeSoundEffect(SoundEffect setype)
    {

    }
}

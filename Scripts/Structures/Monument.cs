using UnityEngine;
using System.Collections.Generic;

public sealed class Monument : Building
{
    public Artifact[] artifacts { get; private set; }
    public Path affectionPath { get; private set; }
    public float affectionValue { get; private set; }
    private bool ringEnabled = false, subscribedToRestoreBlockersEvent = false;
    private int affectionID = -1;
    private Transform ringSprite;

    private static UIMonumentObserver monumentObserver;
    private const int MAX_ARTIFACTS_COUNT = 4;
    public const float MAX_AFFECTION_VALUE = 8f;

    public static void SetObserver(UIMonumentObserver mo)
    {
        monumentObserver = mo;
    }

    override public void SetBasement(Plane b, PixelPosByte pos)
    {         
        base.SetBasement(b, pos);
        if (ringSprite == null)
        {
            var g = new GameObject("ringSprite");
            ringSprite = g.transform;
            ringSprite.parent = transform;
            ringSprite.localPosition = Vector3.up * 1.5f;
            ringSprite.localRotation = Quaternion.Euler(90f, 0f, 0f);
            g.AddComponent<SpriteRenderer>();
        }
        if (artifacts == null)
        {
            artifacts = new Artifact[MAX_ARTIFACTS_COUNT];
        }
        else
        {
            RecalculateAffection();          
        }
        if (!GameMaster.loading) b.myChunk.CreateBlocker(b.GetLookingPosition(), this, false, false);
        else
        {
            if (!subscribedToRestoreBlockersEvent)
            {
                GameMaster.realMaster.blockersRestoreEvent += RestoreBlockers;
                subscribedToRestoreBlockersEvent = true;
            }
        }
    }
    public void RestoreBlockers()
    {
        if (subscribedToRestoreBlockersEvent)
        {
            basement.myChunk.CreateBlocker(basement.pos.OneBlockHigher(), this, false, false);
            GameMaster.realMaster.blockersRestoreEvent -= RestoreBlockers;
            subscribedToRestoreBlockersEvent = false;
        }
    }

    public void AddArtifact(Artifact a, int slotIndex)
    {
        if (a != null && !a.destructed )
        {
            if (!a.researched)
            {
                GameMaster.audiomaster.Notify(NotificationSound.Disagree);
                GameLogUI.MakeImportantAnnounce(Localization.GetPhrase(LocalizedPhrase.ArtifactNotResearched));
                return;
            }
            else {
                if (affectionPath != Path.TechPath)
                {
                    GameMaster.audiomaster.Notify(NotificationSound.Disagree);
                    GameLogUI.MakeImportantAnnounce(Localization.GetPhrase(LocalizedPhrase.AffectionTypeNotMatch));
                    return;
                }
                else
                {
                    if (artifacts == null) artifacts = new Artifact[4];
                    else
                    {
                        if (artifacts[slotIndex] != null)
                        {
                            artifacts[slotIndex].ChangeStatus(Artifact.ArtifactStatus.OnConservation);
                        }
                    }
                    artifacts[slotIndex] = a;                    
                    a.ChangeStatus(Artifact.ArtifactStatus.UsingInMonument);
                    RecalculateAffection();
                }
            }
        }
    }
    public void RemoveArtifact(int index)
    {
        if (artifacts != null && artifacts[index] != null)
        {
            artifacts[index].ChangeStatus(Artifact.ArtifactStatus.OnConservation);
            artifacts[index] = null;
            RecalculateAffection();
            bool allArtifactsSlotsEmpty = true;
            foreach (var a in artifacts)
            {
                if (a != null) allArtifactsSlotsEmpty = false;
            }
            if (allArtifactsSlotsEmpty) ClearAffection();
        }
    }

    private void RecalculateAffection()
    {
        if (artifacts != null)
        {
            float af = 0; float count = 0;
            Artifact a;
            for (int i = 0; i < artifacts.Length; i++)
            {
                a = artifacts[i];
                if (a != null)
                {
                    af += a.GetAffectionValue();
                    affectionPath = a.affectionPath;
                    count++;
                }
            }
            if (count != 0)
            {
                float m = count / (float)MAX_ARTIFACTS_COUNT;
                affectionValue = af * (0.5f + m * m * 1.5f); // MAX_AFFECTION_VALUE = 8 (1 * 4 * 2)

                ringSprite.GetComponent<SpriteRenderer>().sprite = Artifact.GetAffectionSprite(affectionPath);
                ringEnabled = true;
                ringSprite.gameObject.SetActive(ringEnabled);
                switch (affectionPath)
                {
                    case Path.LifePath:
                        {
                            var nature = GameMaster.realMaster.mainChunk.GetNature();
                            if (affectionID == -1)
                            {
                                affectionID = nature.AddLifepowerAffection(affectionValue);
                            }
                            else nature.ChangeLifepowerAffection(affectionID, affectionValue);
                            break;
                        }
                    case Path.SecretPath:
                        {
                            var gmap = GameMaster.realMaster.globalMap;
                            if (affectionID != -1) affectionID = gmap.AddWorldAffection(affectionValue);
                            else gmap.ChangeWorldAffection(affectionID, affectionValue);
                            break;
                        }
                    case Path.TechPath:
                        {
                            var em = GameMaster.realMaster.environmentMaster;
                            if (affectionID != -1) affectionID = em.AddStabilityModifier(affectionValue);
                            else em.ChangeStabilityModifierValue(affectionID, affectionValue);
                            break;
                        }
                }
            }
            else ClearAffection();
        }
        else
        {
            if (ringSprite != null)
            {
                ringEnabled = false;
                ringSprite.gameObject.SetActive(ringEnabled);
            }
        }
    }
    private void ClearAffection()
    {
        //#clearing
        if (affectionID != -1)
        {
            switch (affectionPath)
            {
                case Path.LifePath: GameMaster.realMaster.mainChunk.GetNature()?.RemoveLifepowerAffection(affectionID); break;
                case Path.TechPath: GameMaster.realMaster.environmentMaster?.RemoveStabilityModifier(affectionID); break;
                case Path.SecretPath: GameMaster.realMaster.globalMap.RemoveWorldAffection(affectionID); break;
            }
            affectionID = -1;
        }
        affectionPath = Path.NoPath;
        affectionValue = 0f;
        ringSprite.GetComponent<SpriteRenderer>().sprite = Artifact.GetAffectionSprite(affectionPath);        
        ringEnabled =false;
        ringSprite.gameObject.SetActive(ringEnabled);
        //
    }

    private void Update()
    {
        if (ringEnabled) ringSprite.Rotate(Vector3.forward, affectionValue * 3f * GameMaster.gameSpeed * Time.deltaTime);
    }

    protected override void SwitchActivityState()
    {
        base.SwitchActivityState();
        if (artifacts != null && ((isActive & isEnergySupplied) == false)) ArtifactsStabilityTest();
    }

    private void ArtifactsStabilityTest()
    {
        float hardness = 1f;
        switch (GameMaster.realMaster.difficulty)
        {
            case Difficulty.Utopia: hardness = 0; break;
            case Difficulty.Easy: hardness = 0.1f; break;
            case Difficulty.Normal: hardness = 0.33f; break;
            case Difficulty.Hard: hardness = 0.5f; break;
        }
        bool? b = null;
        bool needRecalculation = false;
        if (artifacts[0] != null)
        {
            b = artifacts[0].StabilityTest(hardness);
            if (b == null) {
                artifacts[0] = null;
                needRecalculation = true;
            }
        }
        if (artifacts[1] != null)
        {
            b = artifacts[1].StabilityTest(hardness);
            if (b == null)
            {
                artifacts[1] = null;
                needRecalculation = true;
            }
        }
        if (artifacts[2] != null)
        {
            b = artifacts[2].StabilityTest(hardness);
            if (b == null)
            {
                artifacts[2] = null;
                needRecalculation = true;
            }
        }
        if (artifacts[3] != null)
        {
            b = artifacts[3].StabilityTest(hardness);
            if (b == null)
            {
                artifacts[3] = null;
                needRecalculation = true;
            }
        }
        if (needRecalculation) RecalculateAffection();
    }

    override public UIObserver ShowOnGUI()
    {
        if (monumentObserver == null) monumentObserver = UIMonumentObserver.InitializeMonumentObserverScript();
        else monumentObserver.gameObject.SetActive(true);
        monumentObserver.SetObservingMonument(this);
        showOnGUI = true;
        return monumentObserver;
    }

    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        if (basement != null)
        {
            basement.myChunk.GetBlock(basement.pos.OneBlockHigher())?.DropBlockerLink(this);
        }
        PrepareBuildingForDestruction(clearFromSurface, returnResources, leaveRuins);
        if (subscribedToRestoreBlockersEvent)
        {
            GameMaster.realMaster.blockersRestoreEvent -= RestoreBlockers;
            subscribedToRestoreBlockersEvent = false;
        }
        Destroy(gameObject);
    }

    #region save-load system
    override public List<byte> Save()
    {
        var data = base.Save();
        data.AddRange(System.BitConverter.GetBytes(artifacts[0] == null ? -1 : artifacts[0].ID));
        data.AddRange(System.BitConverter.GetBytes(artifacts[1] == null ? -1 : artifacts[1].ID));
        data.AddRange(System.BitConverter.GetBytes(artifacts[2] == null ? -1 : artifacts[2].ID));
        data.AddRange(System.BitConverter.GetBytes(artifacts[3] == null ? -1 : artifacts[3].ID));
        return data;
    }
    override public void Load(System.IO.FileStream fs, Plane sblock)
    {
        base.Load(fs, sblock);
        artifacts = new Artifact[MAX_ARTIFACTS_COUNT];
        var data = new byte[16];
        fs.Read(data, 0, 16);
        int x = System.BitConverter.ToInt32(data, 0);
        if (x != -1) artifacts[0] = Artifact.GetArtifactByID(x); else artifacts[0] = null;
        x = System.BitConverter.ToInt32(data, 4);
        if (x != -1) artifacts[1] = Artifact.GetArtifactByID(x); else artifacts[1] = null;
        x = System.BitConverter.ToInt32(data, 8);
        if (x != -1) artifacts[2] = Artifact.GetArtifactByID(x); else artifacts[2] = null;
        x = System.BitConverter.ToInt32(data, 12);
        if (x != -1) artifacts[3] = Artifact.GetArtifactByID(x); else artifacts[3] = null;
        RecalculateAffection();
    }
    #endregion
}

using UnityEngine;
using System.Collections.Generic;

public sealed class Monument : Building
{
    public Artifact[] artifacts { get; private set; }
    public Path affectionPath { get; private set; }
    public float affectionValue { get; private set; }
    private bool ringEnabled = false, subscribedToRestoreBlockersEvent = false;
    private int stabilityArrayIndex = -1;
    private Transform ringSprite;

    private static UIMonumentObserver monumentObserver;
    private const int ARTIFACTS_COUNT = 4;

    public static void SetObserver(UIMonumentObserver mo)
    {
        monumentObserver = mo;
    }

    override public void SetBasement(SurfaceBlock b, PixelPosByte pos)
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
            artifacts = new Artifact[ARTIFACTS_COUNT];
        }
        else
        {
            RecalculateAffection();          
        }
        if (!GameMaster.loading ) b.myChunk.BlockByStructure(b.pos.x, b.pos.y + 1, b.pos.z, this); 
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
            basement.myChunk.BlockByStructure(basement.pos.x, basement.pos.y + 1, basement.pos.z, this);
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
                affectionValue = af / count;
                ringSprite.GetComponent<SpriteRenderer>().sprite = Artifact.GetAffectionSprite(affectionPath);
                ringEnabled = true;
                if (stabilityArrayIndex == -1)
                {
                    if (affectionValue != 0) stabilityArrayIndex = GameMaster.realMaster.AddStabilityModifier(affectionValue);
                }
                else
                {
                    GameMaster.realMaster.ChangeStabilityModifierValue(stabilityArrayIndex, affectionValue);
                }
            }
            else
            {
                affectionValue = 0;
                ringSprite.GetComponent<SpriteRenderer>().sprite = null;
                ringEnabled = false;
                if (stabilityArrayIndex != -1)
                {
                    GameMaster.realMaster.RemoveStabilityModifier(stabilityArrayIndex);
                    stabilityArrayIndex = -1;
                }
            }
        }
    }

    private void Update()
    {
        if (ringEnabled) ringSprite.Rotate(Vector3.forward, affectionValue * 3f * GameMaster.gameSpeed * Time.deltaTime);
    }

    override public void SetActivationStatus(bool x, bool recalculateAfter)
    {
        if (!GameMaster.loading && isActive != x & artifacts != null) ArtifactsStabilityTest();
        base.SetActivationStatus(x, recalculateAfter);
    }
    override public void SetEnergySupply(bool x, bool recalculateAfter)
    {
        if (!GameMaster.loading && isEnergySupplied != x & artifacts != null) ArtifactsStabilityTest();
        base.SetEnergySupply(x, recalculateAfter);
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
            var c = basement.myChunk;
            var cpos = new ChunkPos(basement.pos.x, basement.pos.y + 1, basement.pos.z);
            var b = c.GetBlock(cpos);
            if (b != null && b.mainStructure == this)
            {
                basement.myChunk.DeleteBlock(cpos);
            }
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
    override public void Load(System.IO.FileStream fs, SurfaceBlock sblock)
    {
        base.Load(fs, sblock);
        artifacts = new Artifact[ARTIFACTS_COUNT];
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

﻿using UnityEngine;

public sealed class Monument : Building
{
    public Artifact[] artifacts { get; private set; }
    public Artifact.AffectionType affectionType { get; private set; }
    public float affectionValue { get; private set; }
    private bool ringEnabled = false;
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
        else ringSprite.GetComponent<SpriteRenderer>().sprite = Artifact.GetAffectionSprite(affectionType);
        b.myChunk.BlockByStructure(b.pos.x, b.pos.y + 1, b.pos.z, this);        
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
                if (affectionType != Artifact.AffectionType.NoAffection & a.affectionType != affectionType)
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
                            artifacts[slotIndex].Conservate();
                        }
                    }
                    artifacts[slotIndex] = a;                    
                    a.UseInMonument();
                    RecalculateAffection();
                }
            }
        }
    }
    public void RemoveArtifact(int index)
    {
        if (artifacts != null && artifacts[index] != null)
        {
            artifacts[index].Conservate();
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
                    affectionType = a.affectionType;
                    count++;
                }
            }
            if (count != 0)
            {
                affectionValue = af / count;
                ringSprite.GetComponent<SpriteRenderer>().sprite = Artifact.GetAffectionSprite(affectionType);
                ringEnabled = true;
            }
            else
            {
                affectionValue = 0;
                affectionType = Artifact.AffectionType.NoAffection;
                ringSprite.GetComponent<SpriteRenderer>().sprite = null;
                ringEnabled = false;
            }
        }
    }

    private void Update()
    {
        if (ringEnabled) ringSprite.Rotate(Vector3.forward, affectionValue * 2f * GameMaster.gameSpeed * Time.deltaTime);
    }

    override public void SetActivationStatus(bool x, bool recalculateAfter)
    {
        if (isActive != x & artifacts != null) ArtifactsStabilityTest();
        base.SetActivationStatus(x, recalculateAfter);
    }
    override public void SetEnergySupply(bool x, bool recalculateAfter)
    {
        if (isEnergySupplied != x & artifacts != null) ArtifactsStabilityTest();
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
        Destroy(gameObject);
    }
}

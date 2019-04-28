public class Monument : Building
{
    public Artifact[] artifacts { get; private set; }
    public Artifact.AffectionType affectionType { get; private set; }
    public float affectionValue { get; private set; }

    public void AddArtifact(Artifact a, int i)
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
                        if (artifacts[i] != null)
                        {
                            artifacts[i].Conservate();
                        }
                    }
                    artifacts[i] = a;
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
            if (artifacts[0] != null)
            {
                af += artifacts[0].GetAffectionValue();
                count++;
            }
            if (artifacts[1] != null)
            {
                af += artifacts[1].GetAffectionValue();
                count++;
            }
            if (artifacts[2] != null)
            {
                af += artifacts[2].GetAffectionValue();
                count++;
            }
            if (artifacts[3] != null)
            {
                af += artifacts[3].GetAffectionValue();
                count++;
            }
            if (count != 0)  affectionValue = af / count;
            else
            {
                affectionValue = 0;
                affectionType = Artifact.AffectionType.NoAffection;
            }
        }
    }

    override public void SetActivationStatus(bool x, bool recalculateAfter)
    {
        if (isActive != x & artifacts != null) ArtifactsStabilityTest();
        base.SetActivationStatus(x, recalculateAfter);
    }
    override public void SetEnergySupply(bool x, bool recalculateAfter)
    {
        if (isEnergySupplied != x) ArtifactsStabilityTest();
        base.SetEnergySupply(x, recalculateAfter);
    }

    private void ArtifactsStabilityTest()
    {
        float hardness = 1f;
        switch (GameMaster.difficulty)
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
}

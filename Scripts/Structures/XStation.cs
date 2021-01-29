using UnityEngine;

public sealed class XStation : WorkBuilding {

    private static bool indicatorPrepared = false, markerEnabled = false;
    public static XStation current { get; private set; }
    private static RectTransform cityMarker;
    private static EnvironmentMaster envMaster;

    private  float INDICATOR_EDGE_POSITION;
    private const float VISIBLE_LOW_BORDER = 0.2f, VISIBLE_UP_BORDER = 0.8f;

    // EnvironmentMaster.environmentalConditions
    //GameMaster.lifegrowCoefficient

    public static void ResetStaticData()
    {
        current = null;
        if (cityMarker != null)
        {
            indicatorPrepared = true;
            markerEnabled = cityMarker.transform.parent.gameObject.activeSelf;
        }
        else
        {
            indicatorPrepared = false;
            markerEnabled = false;
        }
        envMaster = GameMaster.realMaster.environmentMaster;
    }

    override public void SetBasement(Plane b, PixelPosByte pos)
    {
        if (b == null) return;
        SetWorkbuildingData(b, pos);
        if (current != null)
        {
            if (current != null) current.Annihilate(true, true, false);
        }
        else
        {
            AddToResetList(typeof(XStation));
        }
        current = this;
        if (!indicatorPrepared)
        {
            PrepareIndicator();            
            cityMarker.parent.gameObject.SetActive(false);
            markerEnabled = false;
        }
    }

    protected override void SwitchActivityState()
    {
        base.SwitchActivityState();
        bool x = isActive & isEnergySupplied;
        if (x)
        {
            if (!indicatorPrepared) PrepareIndicator();
            cityMarker.anchoredPosition = Vector3.right * (envMaster.islandStability - 0.5f) * INDICATOR_EDGE_POSITION * 2f;
            if (!markerEnabled)
            {
                var s = envMaster.islandStability;
                if (s <= VISIBLE_LOW_BORDER | s >= VISIBLE_UP_BORDER | showOnGUI)
                {
                    cityMarker.parent.gameObject.SetActive(true);
                    markerEnabled = true;
                }
            }
        }
        else
        {
            if (indicatorPrepared & markerEnabled)
            {
                cityMarker.transform.parent.gameObject.SetActive(false);
                markerEnabled = false;
            }
        }
    }

    public override UIObserver ShowOnGUI()
    {
        var wo = base.ShowOnGUI();
        colony.observer.EnableTextfield(ID);
        return wo;
    }
    override public void DisabledOnGUI()
    {
        showOnGUI = false;
        if (markerEnabled)
        {
            var s = envMaster.islandStability;
            if (s <= VISIBLE_LOW_BORDER | s >= VISIBLE_UP_BORDER)
            {
                cityMarker.parent.gameObject.SetActive(false);
                markerEnabled = false;
            }
        }
        colony.observer.DisableTextfield(ID);
    }

    private void PrepareIndicator()
    {
        var g = Instantiate(Resources.Load<GameObject>("UIPrefs/stabilityIndicator"), colony.observer.GetMainCanvasTransform());
        var t = g.transform;
        //t.parent = UIController.current.mainCanvas;
        //(t as RectTransform).position = Vector2.down * 60f;
        INDICATOR_EDGE_POSITION = (t as RectTransform).rect.width / 2f * 0.99f;
        t.SetAsFirstSibling();
        cityMarker = t.GetChild(2).GetComponent<RectTransform>();
        indicatorPrepared = true;
        cityMarker.parent.gameObject.SetActive(false);
        markerEnabled = false;
        envMaster = GameMaster.realMaster.environmentMaster;
    }
    private void Update()
    {
        var s = envMaster.islandStability;
        bool show = isEnergySupplied & isActive & indicatorPrepared & ( showOnGUI | s <= VISIBLE_LOW_BORDER | s >= VISIBLE_UP_BORDER );
        if (show)
        {
            cityMarker.anchoredPosition = Vector3.right * (s - 0.5f) * INDICATOR_EDGE_POSITION * 2f;
            if (!markerEnabled)
            {
                cityMarker.parent.gameObject.SetActive(true);
                markerEnabled = true;
            }
        }
        else
        {
            if (markerEnabled)
            {
                cityMarker.parent.gameObject.SetActive(false);
                markerEnabled = false;
            }
        }

    }

    public static string GetInfo()
    {
       return Localization.GetWord(LocalizedWord.Stability) + ": " + ((int)(GameMaster.stability * 100)).ToString() + "%\n"

            ;
    }

    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        if (!clearFromSurface) { basement = null; }
        PrepareWorkbuildingForDestruction(clearFromSurface, returnResources, leaveRuins);
        if (current == this) current = null;
        if (indicatorPrepared & markerEnabled)
        {
            cityMarker.transform.parent.gameObject.SetActive(false);
            markerEnabled = false;
        }
        Destroy(gameObject);
    }
}

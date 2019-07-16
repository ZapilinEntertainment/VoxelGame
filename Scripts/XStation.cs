using UnityEngine;

public sealed class XStation : WorkBuilding {
    public static XStation current { get; private set; }
    private static RectTransform cityMarker;
    private static bool indicatorPrepared = false, markerEnabled = false;
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
    }

    override public void SetBasement(SurfaceBlock b, PixelPosByte pos)
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

    override public void SetActivationStatus(bool x, bool recalculateAfter)
    {
        // #m1
        if (x & isEnergySupplied)
        {
            if (!indicatorPrepared) PrepareIndicator();
            cityMarker.anchoredPosition = Vector3.right * (GameMaster.realMaster.stability - 0.5f) * INDICATOR_EDGE_POSITION * 2f;
            if (!markerEnabled)
            {
                var s = GameMaster.realMaster.stability;
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
        //
        base.SetActivationStatus(x, recalculateAfter);
    }
    public override void SetEnergySupply(bool x, bool recalculateAfter)
    {
        // #m1 - mod
        if (x & isActive)
        {
            if (!indicatorPrepared) PrepareIndicator();
            cityMarker.anchoredPosition = Vector3.right * (GameMaster.realMaster.stability - 0.5f) * INDICATOR_EDGE_POSITION * 2f;
            var s = GameMaster.realMaster.stability;
            if (!markerEnabled)
            {
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
        //
        base.SetEnergySupply(x, recalculateAfter);
    }
    override public void DisableGUI()
    {
        showOnGUI = false;
        if (markerEnabled)
        {
            var s = GameMaster.realMaster.stability;
            if (s <= VISIBLE_LOW_BORDER | s >= VISIBLE_UP_BORDER)
            {
                cityMarker.parent.gameObject.SetActive(false);
                markerEnabled = false;
            }
        }
    }

    private void PrepareIndicator()
    {
        var g = Instantiate(Resources.Load<GameObject>("UIPrefs/stabilityIndicator"), UIController.current.mainCanvas);
        var t = g.transform;
        //t.parent = UIController.current.mainCanvas;
        //(t as RectTransform).position = Vector2.down * 60f;
        INDICATOR_EDGE_POSITION = (t as RectTransform).rect.width / 2f * 0.99f;
        t.SetAsFirstSibling();
        cityMarker = t.GetChild(0).GetComponent<RectTransform>();
        indicatorPrepared = true;
        cityMarker.parent.gameObject.SetActive(false);
        markerEnabled = false;
    }
    private void Update()
    {
        var s = GameMaster.realMaster.stability;
        bool show = isEnergySupplied & isActive & indicatorPrepared & ( showOnGUI | s <= VISIBLE_LOW_BORDER | s >= VISIBLE_UP_BORDER );
        if (show)
        {
            cityMarker.anchoredPosition = Vector3.right * (GameMaster.realMaster.stability - 0.5f) * INDICATOR_EDGE_POSITION * 2f;
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

    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        if (!clearFromSurface) { UnsetBasement(); }
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

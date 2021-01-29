
public class ArtifactsRepository : Building
{
    override public UIObserver ShowOnGUI()
    {
        if (buildingObserver == null) buildingObserver = UIBuildingObserver.InitializeBuildingObserverScript();
        else buildingObserver.gameObject.SetActive(true);
        buildingObserver.SetObservingBuilding(this);
        showOnGUI = true;


        // open artifacts window
        return buildingObserver;
    }
    override public void DisabledOnGUI()
    {
        showOnGUI = false;
        //disable artifacts window
    }
}

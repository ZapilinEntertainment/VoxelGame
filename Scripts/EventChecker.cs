public abstract class EventChecker
{

    public static void PlayerConstructedBuilding(Structure s)
    {
        switch (s.ID) {
            case Structure.HOTEL_BLOCK_6_ID:
                Knowledge.GetCurrent()?.CountRouteBonus(Knowledge.ResearchRoute.Foundation, (byte)Knowledge.FoundationRouteBoosters.HotelBuilded);
                break;
            case Structure.HOUSING_MAST_6_ID:
                Knowledge.GetCurrent()?.CountRouteBonus(Knowledge.ResearchRoute.Foundation, (byte)Knowledge.FoundationRouteBoosters.HousingMastBuilded);
                break;
    }
    }
    public static void BuildingUpgraded (Building b)
    {
    }
    public static void PointReached (MapPoint mp)
    {
        if (mp.type == MapMarkerType.Colony) Knowledge.GetCurrent()?.CountRouteBonus(Knowledge.ResearchRoute.Foundation, (byte)Knowledge.FoundationRouteBoosters.AnotherColonyFound);
    }
    public static void MoneyChanged (float val)
    {

    }
    public static void ImmigrantsCountIncreased(int newTotalCount)
    {
        Knowledge.GetCurrent()?.ImmigrantsCheck(newTotalCount);
    }
}

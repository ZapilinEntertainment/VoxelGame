using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Knowledge
{
    public enum ResearchRoute : byte { Foundation = 0, CloudWhale, Engine, Pipes, Crystal, Monument, Blossom, Pollen }
    //dependence: 
    //uiController icons enum
    // labels in Localization - get challenge label

    private static Knowledge current;

    public readonly bool[] puzzlePins;
    public bool allRoutesUnblocked { get; private set; }
    public float[] routePoints { get; private set; }
    public byte[] puzzlePartsCount { get; private set; }
    public byte[] colorCodesArray{get; private set;}
    public float completeness { get; private set; }
    public int changesMarker { get; private set; }

    private byte[] routeBonusesMask = new byte[ROUTES_COUNT]; // учет полученных бонусов
    private KnowledgeTabUI observer;
    private byte dayNumber = 0;

    public const byte ROUTES_COUNT = 8, STEPS_COUNT = 7,
       WHITECOLOR_CODE = 0, REDCOLOR_CODE = 1, GREENCOLOR_CODE = 2, BLUECOLOR_CODE = 3, CYANCOLOR_CODE = 4, BLACKCOLOR_CODE = 5, NOCOLOR_CODE = 6;

    public static readonly Color[] colors = new Color[6]
    {
        Color.white, // 0 - white
        new Color(0.8f, 0.3f, 0.3f, 1f), // 1 - red
        new Color(0.43f, 0.8f, 0.3f), // 2 - green
        new Color(0.25f, 0.52f, 0.86f, 1f), // 3 - blue
        new Color(0.1f, 0.92f, 0.9f, 1f), // 4 - cyan
        new Color(0.2f, 0.17f, 0.17f, 1f), // 5 - black      
    };
    public static readonly float[] STEPVALUES = new float[7] { 10f, 15f, 25f, 50f, 75f, 100f, 125f }; // 400 total
    public const float MAX_RESEARCH_PTS = 400f;
    public readonly byte[,] routeButtonsIndexes = new byte[8, 7] {
        {36, 44, 43,51,52,60,59},
        {45, 53,46,54,62,55,63},
        {28,29,37,38,30,31,39 },
        {21,22, 13,14,15,6,7},
        {27,19,20,11,12,3,4 },
        {18, 10,17,9,1,8,0},
        {35,34,26,25,33,32,24 },
        {42,50,41,49,57,48,56 }
    };
    private readonly byte[] blockedCells = new byte[8] { 2, 5, 16, 23, 40, 47, 58, 61 };

    #region routes conditions
    //foundation:
    private const float R_F_HAPPINESS_COND = 0.8f, R_E_ENERGY_STORED_COND = 10000f;
    private const int R_F_POPULATION_COND = 2500,R_F_IMMIGRANTS_CONDITION = 1000, R_CW_GRASSLAND_COUNT_COND = 6, R_CW_STREAMGENS_COUNT_COND = 8, R_CW_CREWS_COUNT_COND = 4;
    private const byte R_F_SETTLEMENT_LEVEL_COND = 6, R_CW_GRASSLAND_LEVEL_COND = 4, R_CW_UPDATE_FREQUENCY = 5, R_CW_CREW_LEVEL_COND = 3;
    private const uint R_F_IMMIGRANTS_COUNT_COND = 1000;
    public enum FoundationRouteBoosters : byte {HappinessBoost, PopulationBoost, SettlementBoost, ImmigrantsBoost, HotelBoost, HousingMastBoost, ColonyPointBoost, QuestBooster }
    public enum CloudWhaleRouteBoosters: byte { GrasslandsBoost, StreamGensBoost, CrewsBoost, ArtifactBoost, XStationBoost, AscensionEngineBoost, PointBoost, QuestBooster}
    public enum EngineRouteBoosters : byte { EnergyBooster, CityMoveBooster,  GearsBooster, FactoryBooster, IslandEngineBooster, ControlCenterBooster, PointBooster, QuestBooster}
    #endregion

    private void SetSubscriptions()
    {
        var colony = GameMaster.realMaster.colonyController;
        byte mask = routeBonusesMask[(int)ResearchRoute.Foundation];
        if (!BoostCounted(FoundationRouteBoosters.HappinessBoost)) colony.happinessUpdateEvent += HappinessCheck;
        if (!BoostCounted(FoundationRouteBoosters.PopulationBoost)) colony.populationUpdateEvent += PopulationCheck;

        var gm = GameMaster.realMaster;
        gm.eventTracker.buildingConstructionEvent += BuildingConstructionCheck;
        gm.eventTracker.buildingUpgradeEvent += BuildingUpgradeCheck;
        gm.globalMap.pointsExploringEvent += PointCheck;

        mask = routeBonusesMask[(int)ResearchRoute.CloudWhale];
        bool subscribeToEverydayUpdate = 
            !BoostCounted(CloudWhaleRouteBoosters.GrasslandsBoost)
            |
            !BoostCounted(CloudWhaleRouteBoosters.StreamGensBoost)
            |
            !BoostCounted(CloudWhaleRouteBoosters.CrewsBoost)
            ;
        mask = routeBonusesMask[(int)ResearchRoute.Engine];
        if (!BoostCounted(EngineRouteBoosters.EnergyBooster)) subscribeToEverydayUpdate = true;
        if (subscribeToEverydayUpdate) gm.everydayUpdate += EverydayUpdate;
    }

    private void HappinessCheck(float h)
    {
        if (h >= R_F_HAPPINESS_COND)
        {
            var b = (byte)FoundationRouteBoosters.HappinessBoost;
            if (CountRouteBonus(ResearchRoute.Foundation, b))
            {
                GameMaster.realMaster.colonyController.happinessUpdateEvent -= HappinessCheck;
            }
        }
    }
    private void PopulationCheck(int p)
    {
        if (p >= R_F_POPULATION_COND)
        {
            var b = (byte)FoundationRouteBoosters.PopulationBoost;
            if (CountRouteBonus(ResearchRoute.Foundation, b))
            {
                GameMaster.realMaster.colonyController.populationUpdateEvent -= PopulationCheck;
            }
        }
    }
    private void PointCheck(MapPoint mp)
    {
        switch (mp.type)
        {
            case MapMarkerType.Colony:
                 CountRouteBonus(ResearchRoute.Foundation, (byte)FoundationRouteBoosters.ColonyPointBoost);
                break;
            case MapMarkerType.Wiseman:
                if ((PointOfInterest.WisemanSubtype)mp.subIndex == PointOfInterest.WisemanSubtype.AncientEntity)
                    CountRouteBonus(ResearchRoute.CloudWhale, (byte)CloudWhaleRouteBoosters.PointBoost);
                break;
            case MapMarkerType.SOS:
                if ((PointOfInterest.SOSSubtype)mp.subIndex == PointOfInterest.SOSSubtype.Ship)
                    CountRouteBonus(ResearchRoute.CloudWhale, (byte)CloudWhaleRouteBoosters.PointBoost);
                break;
        }
    }
    private void BuildingConstructionCheck(Structure s)
    {
        switch (s.ID)
        {
            case Structure.HOTEL_BLOCK_6_ID:
                if (CountRouteBonus(ResearchRoute.Foundation, (byte)FoundationRouteBoosters.HotelBoost)) ;
                break;
            case Structure.HOUSING_MAST_6_ID:
                CountRouteBonus(ResearchRoute.Foundation, (byte)FoundationRouteBoosters.HousingMastBoost);
                break;
            case Structure.XSTATION_3_ID:
                CountRouteBonus(ResearchRoute.CloudWhale, (byte)CloudWhaleRouteBoosters.XStationBoost);
                break;
        }
    }
    private void BuildingUpgradeCheck(Building b)
    {
        switch (b.ID)
        {
            case Structure.SETTLEMENT_CENTER_ID:
                if (b.level == R_F_SETTLEMENT_LEVEL_COND)
                {
                    var x = (byte)FoundationRouteBoosters.SettlementBoost;
                    CountRouteBonus(ResearchRoute.Foundation, x);
                }
                break;

        }
    }

    private void EverydayUpdate()
    {
        dayNumber++;
        byte unsubscribeVotes = 0;        
        var gm = GameMaster.realMaster;
        int count = 0;

        #region cloud whale route
        //grasslands
        if ( !BoostCounted(CloudWhaleRouteBoosters.GrasslandsBoost) && dayNumber >= R_CW_UPDATE_FREQUENCY)
        {
            var glist = gm.mainChunk.GetNature()?.GetGrasslandsList();
            if (glist != null)
            {

                foreach (var g in glist)
                {
                    if (g.level >= R_CW_GRASSLAND_LEVEL_COND) count++;
                }
                if (count >= R_CW_GRASSLAND_COUNT_COND) CountRouteBonus(CloudWhaleRouteBoosters.GrasslandsBoost);
            }
            dayNumber = 0;
        }
        else unsubscribeVotes++;
        // stream gens
        if ( !BoostCounted(CloudWhaleRouteBoosters.StreamGensBoost))
        {
            var pg = gm.colonyController.GetPowerGrid();
            if (pg != null)
            {
                count = 0;
                foreach (var b in pg)
                {
                    if (b.ID == Structure.WIND_GENERATOR_1_ID) count++;
                }
                if (count >= R_CW_STREAMGENS_COUNT_COND) CountRouteBonus(CloudWhaleRouteBoosters.StreamGensBoost);
            }
        }
        else unsubscribeVotes++;
        //crews
        if (!BoostCounted(CloudWhaleRouteBoosters.CrewsBoost))
        {
            var crewslist = Crew.crewsList;
            if (crewslist != null)
            {
                count = 0;
                foreach (var c in crewslist)
                {
                    if (c.level > R_CW_CREW_LEVEL_COND) count++;
                }
                if (count >= R_CW_CREWS_COUNT_COND) CountRouteBonus(CloudWhaleRouteBoosters.CrewsBoost);
            }
        }
        else unsubscribeVotes++;
        //artifacts
        if (!BoostCounted(CloudWhaleRouteBoosters.ArtifactBoost))
        {
            var alist = Artifact.artifactsList;
            if (alist != null)
            {
                foreach (var a in alist)
                {
                    if (a.affectionPath == Path.SecretPath)
                    {
                        CountRouteBonus(CloudWhaleRouteBoosters.ArtifactBoost);
                        break;
                    }
                }
            }
        }
        else unsubscribeVotes++;
        //
        #endregion

        #region engineRoute
        if (!BoostCounted(EngineRouteBoosters.EnergyBooster))
        {
            if (gm.colonyController.energyStored >= R_E_ENERGY_STORED_COND) CountRouteBonus(EngineRouteBoosters.EnergyBooster);
        }
        else unsubscribeVotes++;
        #endregion

        if (unsubscribeVotes == 5) GameMaster.realMaster.everydayUpdate -= EverydayUpdate;
    }
    #region boost masks
    private bool BoostCounted(CloudWhaleRouteBoosters type)
    {
        return (routeBonusesMask[(int)ResearchRoute.CloudWhale] & (1 << (byte)type)) != 0;
    }
    private bool BoostCounted(FoundationRouteBoosters type)
    {
        return (routeBonusesMask[(int)ResearchRoute.Foundation] & (1 << (byte)type)) != 0;
    }
    private bool BoostCounted(EngineRouteBoosters type)
    {
        return (routeBonusesMask[(int)ResearchRoute.Engine] & (1 << (byte)type)) != 0;
    }

    public bool CountRouteBonus(FoundationRouteBoosters type)
    {
        return CountRouteBonus(ResearchRoute.Foundation, (byte)type);
    }
    public bool CountRouteBonus(CloudWhaleRouteBoosters type)
    {
        return CountRouteBonus(ResearchRoute.CloudWhale, (byte)type);
    }
    public bool CountRouteBonus(EngineRouteBoosters type)
    {
        return CountRouteBonus(ResearchRoute.Engine, (byte)type);
    }
    private bool CountRouteBonus(ResearchRoute rr, byte boosterIndex)
    {
        byte mask = (byte)(1 << boosterIndex), routeIndex = (byte)rr;
        if ((routeBonusesMask[routeIndex] & mask) == 0)
        {
            routeBonusesMask[routeIndex] += mask;
            mask = routeBonusesMask[routeIndex];
            byte bonusIndex = 0;
            if ((mask & 1) != 0) bonusIndex++;
            if ((mask & 2) != 0) bonusIndex++;
            if ((mask & 4) != 0) bonusIndex++;
            if ((mask & 8) != 0) bonusIndex++;
            if ((mask & 16) != 0) bonusIndex++;
            if ((mask & 32) != 0) bonusIndex++;
            if ((mask & 64) != 0) bonusIndex++;
            if ((mask & 128) != 0) bonusIndex++;
            if (bonusIndex < 4) AddResearchPoints(rr, STEPVALUES[bonusIndex]);
            else AddResearchPoints(rr, STEPVALUES[bonusIndex] / 2f);
            return true;
        }
        else return false;
    }
    #endregion

    public void ImmigrantsCheck(uint c)
    {
        if (c >= R_F_IMMIGRANTS_COUNT_COND)
        {
            var b = (byte)FoundationRouteBoosters.ImmigrantsBoost;
            if (CountRouteBonus(ResearchRoute.Foundation, b))
                GameMaster.realMaster.eventTracker?.StopTracking(ResearchRoute.Foundation, b);
        }
    }    

    
    
    

    public static Knowledge GetCurrent()
    {
        if (current == null) current = new Knowledge();
        return current;
    }

    private Knowledge()
    {
        puzzlePins = new bool[112]; // 7 * 8 + 8 * 7
        for (int i =0; i < puzzlePins.Length; i++)
        {
            puzzlePins[i] = Random.value > 0.55f ? true : false;
        }
        routePoints = new float[ROUTES_COUNT];
        completeness = 0f;
        puzzlePartsCount = new byte[6];
        colorCodesArray = new byte[64]; //filed with 0 - whitecolor
        foreach (byte b in blockedCells)
        {
            colorCodesArray[b] = BLACKCOLOR_CODE;
        }     
    }

    public byte GenerateCellColor(byte route, byte step)
    {
        float[] varieties; // r g b c
        switch ((ResearchRoute)route)
        {
            case ResearchRoute.Foundation:
                {
                    if (step == 1) varieties = new float[] { 3, 3, 3, 1 };
                    else
                    {
                        if (step == 6) varieties = new float[] { 0, 1, 3, 3 };
                        else varieties = new float[] { 1, 0.9f, 1, 0.1f };
                    }
                    break;
                }
            case ResearchRoute.CloudWhale:
                {
                    if (step < 4) varieties = new float[] { 1, 3, 6, 2 };
                    else varieties = new float[] { 0, 2, 6, 4 };
                    break;
                }
            case ResearchRoute.Engine:
                {
                    if (step < 3) varieties = new float[] { 0, 0, 1, 0 };
                    else varieties = new float[] { 1, 2, 12, 8 };
                    break;
                }
            case ResearchRoute.Pipes:
                {
                    if (step == 1) varieties = new float[] { 0, 0, 0, 1 };
                    else
                    {
                        if (step < 4) varieties = new float[] { 1, 1, 10, 3 };
                        else varieties = new float[] { 0, 0, 1, 1 };
                    }
                    break;
                }
            case ResearchRoute.Crystal:
                {
                    if (step == 1) varieties = new float[] { 0, 0, 0, 1 };
                    else
                    {
                        varieties = new float[] { 0.2f, 0.4f, 5, 5 }; ;
                    }
                    break;
                }
            case ResearchRoute.Monument:
                {
                    if (step < 5) varieties = new float[] { 1, 2, 5, 4 };
                    else varieties = new float[] { 0, 1, 5, 5 };
                    break;
                }
            case ResearchRoute.Blossom:
                {
                    varieties = new float[] { 1, 20, 5, 10 };
                    break;
                }
            case ResearchRoute.Pollen:
                {
                    if (step == 1) varieties = new float[] { 0, 10, 0, 1 };
                    else
                    {
                        varieties = new float[] { 1, 20, 10, 10 };
                    }
                    break;
                }
            default: varieties = new float[] { 1, 1, 1, 1 }; break;
        }

        float s = varieties[0] + varieties[1] + varieties[2] + varieties[3];
        float v = Random.value;
        byte col;
        if (v < (varieties[0] + varieties[1] + varieties[2]) / s)
        {
            if (v < varieties[0]) col = REDCOLOR_CODE; else col = GREENCOLOR_CODE;
        }
        else
        {
            if (v > varieties[3]) col = CYANCOLOR_CODE; else col = BLUECOLOR_CODE;
        }
        return col;
    }

    public void AddPuzzlePart(byte colorcode)
    {
        if (colorcode < puzzlePartsCount.Length && puzzlePartsCount[colorcode] < 255) puzzlePartsCount[colorcode]++;  
    }
    public void AddResearchPoints (ResearchRoute route, float pts)
    {
        byte routeIndex = (byte)route;
        float f = routePoints[routeIndex] + pts;
        float maxvalue = STEPVALUES[STEPS_COUNT - 1];
        if (f >= maxvalue)
        {
            routePoints[routeIndex] = maxvalue;
            for (byte step = 0; step < STEPS_COUNT; step++)
            {
                if (colorCodesArray[routeButtonsIndexes[routeIndex, step]] == WHITECOLOR_CODE)
                {
                    colorCodesArray[routeButtonsIndexes[routeIndex, step]] = GenerateCellColor(routeIndex, step);
                    changesMarker++;
                }                    
            }
        }
        else
        {
            byte step = 0;
            while (step < STEPS_COUNT && f >= STEPVALUES[step])
            {
                if ( colorCodesArray[routeButtonsIndexes[routeIndex, step]] == WHITECOLOR_CODE)
                {
                    colorCodesArray[routeButtonsIndexes[routeIndex, step]] = GenerateCellColor(routeIndex, step);
                    changesMarker++;
                }
                step++;
            }
            routePoints[routeIndex] = f;
        }
    }
    public bool UnblockButton(int i)
    {
        var colorcode = colorCodesArray[i];
        if (colorcode == NOCOLOR_CODE) return true;
        if (puzzlePartsCount[colorcode] > 0)
        {
            puzzlePartsCount[colorcode]--;
            colorCodesArray[i] = NOCOLOR_CODE;
            changesMarker++;
            return true;
        }
        else return false;
    }

    public void OpenResearchTab()
    {
        GameMaster.realMaster.environmentMaster.DisableDecorations();
        if (observer == null)
        {
            observer = GameObject.Instantiate(Resources.Load<GameObject>("UIPrefs/knowledgeTab")).GetComponent<KnowledgeTabUI>();
            observer.Prepare(this);            
        }
        if (!observer.gameObject.activeSelf) observer.gameObject.SetActive(true);
        observer.Redraw();
    }

    public (byte,byte) CellIndexToRouteAndStep(int buttonIndex)
    {
        for (byte ri = 0; ri < ROUTES_COUNT; ri++)
        {
            for (byte si = 0; si < STEPS_COUNT;si++)
            {
                if (routeButtonsIndexes[ri, si] == buttonIndex) return (ri,si);
            }
        }
        return (255,255);
    }
}

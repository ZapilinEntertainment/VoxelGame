using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ExploringLocation {
    public enum LocationType: byte {
        AncientRuins, ModernRuins, SoulRuins,
        LiveIsland, DeadIsland, RockIsland, ArtificialIsland,
        RefugeesVesselWreck, ColonistsVesselWreck, ScienceVesselWreck, MilitaryVesselWreck, TradeVesselWreck, PersonalVesselWreck,
        RefugeesVessel, ColonistsVessel, ScienceVessel, MilitaryVessel, TradeVessel, PersonalVessel,
        OceanWorld, UndergroundWorld, ClosedWorld, EarthtypeWorld, DiedWorld, ForestWorld, DesertWorld,
        ScienceStation, TradeStation, RefuelStation, MilitaryStation, 
        LostScienceStation, LostTradeStation, LostRefuelStation, LostMilitaryStation,
        GardenColony, ResidentialColony, IndustrialColony, LibraryColony,
        LostGardenColony, LostResidentialColony, LostIndustrialColony, LostLibraryColony,
        EndlessCity, GiantUndergroundCatacombs, EndlessFields, LightStorm, LostCityOfCyclops, UnderrealmCaverns,
        WhiteSpace, BlackSpace, IceSpace, FireSpace, WaterSpace, DarkCanyon, EndOfRealm, DreamRealm
    }
    public readonly LocationType type;
    public float exploredPart { get; private set; }
}

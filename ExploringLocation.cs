using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ExploringLocation {
    public enum LocationType: byte {
        WhiteSpace, BlackSpace, IceSpace, FireSpace, WaterSpace, DarkCanyon, EndOfRealm, DreamRealm,
        AncientRuins, ModernRuins, SoulRuins,
        LiveIsland, DeadIsland, RockIsland, ArtificialIsland,
        RefugeesVesselWreck, ColonistsVesselWreck, ScienceVesselWreck, MilitaryVesselWreck, TradeVesselWreck, PersonalVesselWreck,
        RefugeesVessel, ColonistsVessel, ScienceVessel, MilitaryVessel, TradeVessel, PersonalVessel,
        OceanWorld, UndergroundWorld, ClosedWorld, EarthtypeWorld, DiedWorld, ForestWorld, DesertWorld,
        ScienceStation, TradeStation, RefuelStation, MilitaryStation, 
        LostScienceStation, LostTradeStation, LostRefuelStation, LostMilitaryStation,
        GardenColony, ResidentialColony, IndustrialColony, LibraryColony,
        LostGardenColony, LostResidentialColony, LostIndustrialColony, LostLibraryColony,
        EndlessCity, GiantUndergroundCatacombs, EndlessFields, LightStorm, LostCityOfCyclops, UnderrealmCaverns        
    }
    public readonly LocationType type;
    public float exploredPart { get; private set; }

    private ExploringLocation(LocationType i_type)
    {
        type = i_type;
        exploredPart = 0;
    }

    #region save-load
    public List<byte> Save()
    {
        var bytes = new List<byte>();
        bytes.Add((byte)type);
        bytes.AddRange(System.BitConverter.GetBytes(exploredPart));
        return bytes;
    }
    public static ExploringLocation Load(System.IO.FileStream fs)
    {
        LocationType ltype = (LocationType)fs.ReadByte();
        ExploringLocation location = new ExploringLocation(ltype);
        var data = new byte[4];
        fs.Read(data, 0, 4);
        location.exploredPart = System.BitConverter.ToSingle(data, 0);
        return location;
    }
    #endregion
}

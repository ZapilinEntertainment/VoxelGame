using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ExploringLocation {
    public enum LocationType: byte {
        Default,        
        LiveIsland, DeadIsland, RockIsland, ArtificialIsland,
        RefugeesVesselWreck, ColonistsVesselWreck, ScienceVesselWreck, MilitaryVesselWreck, TradeVesselWreck, PersonalVesselWreck,
        RefugeesVessel, ColonistsVessel, ScienceVessel, MilitaryVessel, TradeVessel, PersonalVessel,        
        ScienceStation, TradeStation, RefuelStation, MilitaryStation, 
        LostScienceStation, LostTradeStation, LostRefuelStation, LostMilitaryStation,
        GardenColony, ResidentialColony, IndustrialColony, LibraryColony,
        LostGardenColony, LostResidentialColony, LostIndustrialColony, LostLibraryColony,
        EndlessCity, GiantUndergroundCatacombs, LostCityOfCyclops       
    }
    public readonly LocationType type;
    public float exploredPart { get; private set; }
    public Color color { get; private set; }

    public ExploringLocation(LocationType i_type)
    {
        type = i_type;
        exploredPart = 0;
        switch (type)
        {
            default: color = Color.white; break;
        }
    }

    public Rect GetIconRect()
    {
        return Rect.zero;
    }
    public Texture GetBottomTexture()
    {
        return null;
    }
    public GameObject GetDecoration(int size)
    {
        switch (type) {
            case LocationType.Default:
            case LocationType.LiveIsland:
            case LocationType.DeadIsland:
            case LocationType.RockIsland:
            case LocationType.ArtificialIsland:
            case LocationType.RefugeesVesselWreck:
            case LocationType.ColonistsVesselWreck:
            case LocationType.ScienceVesselWreck:
            case LocationType.MilitaryVesselWreck:
            case LocationType.TradeVesselWreck:
            case LocationType.PersonalVesselWreck:
            case LocationType.RefugeesVessel:
            case LocationType.ColonistsVessel:
            case LocationType.ScienceVessel:
            case LocationType.MilitaryVessel:
            case LocationType.TradeVessel:
            case LocationType.PersonalVessel:
            case LocationType.ScienceStation:
            case LocationType.TradeStation:
            case LocationType.RefuelStation:
            case LocationType.MilitaryStation:
            case LocationType.LostScienceStation:
            case LocationType.LostTradeStation:
            case LocationType.LostRefuelStation:
            case LocationType.LostMilitaryStation:
            case LocationType.GardenColony:
            case LocationType.ResidentialColony:
            case LocationType.IndustrialColony:
            case LocationType.LibraryColony:
            case LocationType.LostGardenColony:
            case LocationType.LostResidentialColony:
            case LocationType.LostIndustrialColony:
            case LocationType.LostLibraryColony:
            case LocationType.EndlessCity:
            case LocationType.GiantUndergroundCatacombs:
            case LocationType.LostCityOfCyclops:
            default: return null;
        }
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

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
    public readonly float difficulty;

    public ExploringLocation(LocationType i_type)
    {
        type = i_type;
        difficulty = 1f;
        switch (type)
        {
            case LocationType.LiveIsland:
                difficulty += (0.5f - Random.value);
                break;
            case LocationType.DeadIsland:
                difficulty += Random.value * 0.2f;
                break;
            case LocationType.RockIsland:
                difficulty += (0.5f - Random.value) * 0.2f;
                break;
            case LocationType.ArtificialIsland:
                difficulty += Random.value;
                break;
            case LocationType.RefugeesVesselWreck:
                difficulty += Random.value * 0.3f;
                break;
            case LocationType.ColonistsVesselWreck:
            case LocationType.TradeVesselWreck:
                difficulty += Random.value * 0.2f;
                break;
            case LocationType.ScienceVesselWreck:               
            case LocationType.MilitaryVesselWreck:
                difficulty += Random.value * 0.5f;
                break;
            case LocationType.PersonalVesselWreck:
                difficulty = Random.value * 1.4f;
                break;
            case LocationType.RefugeesVessel:
            case LocationType.ColonistsVessel:
            case LocationType.ScienceVessel:
            case LocationType.MilitaryVessel:
            case LocationType.TradeVessel:
            case LocationType.PersonalVessel:
            case LocationType.TradeStation:
            case LocationType.RefuelStation:
                difficulty -= Random.value * 0.9f;
                break;           
            case LocationType.ScienceStation:            
            case LocationType.MilitaryStation:
                difficulty -= Random.value * 0.3f;
                break;
            case LocationType.LostScienceStation:
            case LocationType.LostMilitaryStation:
                difficulty += Random.value * 0.5f;
                break;
            case LocationType.LostTradeStation:
            case LocationType.LostRefuelStation:
                difficulty += Random.value * 0.3f;
                break;
            case LocationType.GardenColony:
                difficulty -= 0.9f * Random.value;
                break;
            case LocationType.ResidentialColony:
            case LocationType.IndustrialColony:
            case LocationType.LibraryColony:
                difficulty -= 0.7f * Random.value;
                break;
            case LocationType.LostGardenColony:
            case LocationType.LostResidentialColony:
                difficulty += 0.3f * Random.value;
                break;
            case LocationType.LostIndustrialColony:
            case LocationType.LostLibraryColony:
                difficulty += 0.5f * Random.value;
                break;
            case LocationType.EndlessCity:               
            case LocationType.GiantUndergroundCatacombs:
            case LocationType.LostCityOfCyclops:
                difficulty += Random.value;
                break;            
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
  
    public void TakeTreasure(Crew c)
    {

    }

    #region save-load
    public List<byte> Save()
    {
        var bytes = new List<byte>();
        bytes.Add((byte)type);
        return bytes;
    }
    public static ExploringLocation Load(System.IO.FileStream fs)
    {
        LocationType ltype = (LocationType)fs.ReadByte();
        ExploringLocation location = new ExploringLocation(ltype);
        var data = new byte[4];
        fs.Read(data, 0, 4);
        return location;
    }
    #endregion
}

public enum Language{English, Russian};
public enum LocalizedWord{Level, Offline, Dig, Upgrade, UpgradeCost, Cancel, Buy, Sell, Limit, Demand, Price, Trading, Gather, Immigration,  Normal, Improved, Lowered,  Dismiss, Disassemble, Total, Repair,
Save, Load, Options, Exit, Build, Shuttles, Crews, Reward, Delete, Rewrite, Yes, MainMenu}
public enum LocalizedPhrase { StopDig, StopGather, RequiredSurface, ImmigrationEnabled, ImmigrationDisabled, TicketsLeft, ColonistsArrived, PointsSec, BirthrateMode, ShuttlesAvailable, CrewsAvailable, TransmittersAvailable,
ImproveGears, NoActivity, CrewSlots, HireNewCrew, ConstructShuttle, ShuttleRepaired, ShuttleConstructed, ObjectsLeft, NoSavesFound, CreateNewSave, CameraZoom, LODdistance, GraphicQuality}
public enum LocalizationActionLabels {Extracted, WorkStopped, BlockCompleted, MineLevelFinished, CleanInProgress, DigInProgress, GatherInProgress }
public enum GameAnnouncements{NotEnoughResources, NotEnoughEnergyCrystals, GameSaved, GameLoaded, SavingFailed, LoadingFailed, PowerFailure, NewQuestAvailable };
public enum RestrictionKey{SideConstruction, UnacceptableSurfaceMaterial, HeightBlocked}
public enum RefusalReason {Unavailable, MaxLevel, HQ_RR1, HQ_RR2, HQ_RR3, HQ_RR4, HQ_RR5, HQ_RR6, SpaceAboveBlocked, NoBlockBelow, NotEnoughSlots}

public static class Localization {

	public static Language currentLanguage;

	static Localization() {
		ChangeLanguage(Language.English);
	}

	public static void ChangeLanguage(Language lan ) {
        currentLanguage = lan;
	}


    public static string GetStructureName(int id) {
        switch (id) {
            default: return "Unknown building";
            case Structure.PLANT_ID: return "Some plant";
            case Structure.LANDED_ZEPPELIN_ID: return "Landed Zeppelin";
            case Structure.STORAGE_0_ID: return "Primary storage";
            case Structure.STORAGE_1_ID: return "Storage pit";
            case Structure.STORAGE_2_ID: return "Small warehouse";
            case Structure.STORAGE_3_ID: return "Warehouse";
            case Structure.STORAGE_5_ID: return "Storage block";
            case Structure.CONTAINER_ID: return "Container";
            case Structure.MINE_ELEVATOR_ID: return "Mine elevator";
            case Structure.LIFESTONE_ID: return "Life stone";
            case Structure.HOUSE_0_ID: return "Tent";
            case Structure.HOUSE_1_ID: return "Small house";
            case Structure.HOUSE_2_ID: return "House";
            case Structure.HOUSE_3_ID: return "Advanced house";
            case Structure.HOUSE_5_ID: return "Residential Block";
            case Structure.DOCK_ID: return "Basic dock";
            case Structure.ENERGY_CAPACITOR_1_ID: return "Power capacitor";
            case Structure.ENERGY_CAPACITOR_2_ID: return "Power capacitor";
            case Structure.ENERGY_CAPACITOR_3_ID: return "Power capacitor";
            case Structure.FARM_1_ID: return "Farm (lvl 1)";
            case Structure.FARM_2_ID: return "Farm (lvl 2)";
            case Structure.FARM_3_ID: return "Farm (lvl 3)";
            case Structure.FARM_4_ID: return "Covered farm ";
            case Structure.FARM_5_ID: return "Farm Block ";
            case Structure.HQ_2_ID: return "HeadQuarters";
            case Structure.HQ_3_ID: return "HeadQuarters";
            case Structure.HQ_4_ID: return "HeadQuarters";
            case Structure.LUMBERMILL_1_ID: return "Lumbermill";
            case Structure.LUMBERMILL_2_ID: return "Lumbermill";
            case Structure.LUMBERMILL_3_ID: return "Lumbermill";
            case Structure.LUMBERMILL_4_ID: return "Covered lumbermill";
            case Structure.LUMBERMILL_5_ID: return "Lumbermill Block";
            case Structure.MINE_ID: return "Mine Entrance";
            case Structure.SMELTERY_1_ID: return "Smeltery";
            case Structure.SMELTERY_2_ID: return "Smeltery";
            case Structure.SMELTERY_3_ID: return "Smelting Facility";
            case Structure.SMELTERY_5_ID: return "Smeltery Block";
            case Structure.WIND_GENERATOR_1_ID: return "Wind generator";
            case Structure.BIOGENERATOR_2_ID: return "Biogenerator";
            case Structure.HOSPITAL_2_ID: return "Hospital";
            case Structure.MINERAL_POWERPLANT_2_ID: return "Mineral F powerplant";
            case Structure.ORE_ENRICHER_2_ID: return "Ore enricher";
            case Structure.ROLLING_SHOP_ID: return "Rolling shop";
            case Structure.MINI_GRPH_REACTOR_ID: return "Small Graphonum reactor";
            case Structure.FUEL_FACILITY_3_ID: return "Fuel facility";
            case Structure.GRPH_REACTOR_4_ID: return "Graphonium reactor";
            case Structure.PLASTICS_FACTORY_3_ID: return "Plastics factory";
            case Structure.FOOD_FACTORY_4_ID: return "Food factory";
            case Structure.FOOD_FACTORY_5_ID: return "Food factory Block";
            case Structure.GRPH_ENRICHER_ID: return "Graphonium enricher";
            case Structure.XSTATION_ID: return "Experimental station";
            case Structure.QUANTUM_ENERGY_TRANSMITTER_ID: return "Quantum energy transmitter";
            case Structure.CHEMICAL_FACTORY_ID: return "Chemical factory";
            case Structure.RESOURCE_STICK_ID: return "Constructing block...";
            case Structure.COLUMN_ID: return "Column";
            case Structure.SWITCH_TOWER_ID: return "Switch tower";
            case Structure.SHUTTLE_HANGAR_ID: return "Shuttle hangar";
            case Structure.RECRUITING_CENTER_ID: return "Recruiting Center";
            case Structure.EXPEDITION_CORPUS_ID: return "Expedition Corpus";
            case Structure.QUANTUM_TRANSMITTER_ID: return "Long-range transmitter";
        }
    }
	public static string GetStructureDescription(int id) {
		return "no descriptions yet";
	}
	public static string GetResourceName(int id) {
		switch (id) {
		default: return "Unregistered resource";
		case 0: return "Nothing"; 
		case ResourceType.DIRT_ID: return "Dirt"; 
		case ResourceType.FOOD_ID: return "Food"; 
		case ResourceType.LUMBER_ID : return "Wood";
		case ResourceType.STONE_ID : return "Stone";
		case ResourceType.METAL_K_ID : return "Metal K ";
		case ResourceType.METAL_M_ID : return "Metal M ";
		case ResourceType.METAL_E_ID : return "Metal E ";
		case ResourceType.METAL_N_ID : return "Metal N ";
		case ResourceType.METAL_P_ID : return "Metal P ";
		case ResourceType.METAL_S_ID : return "Metal S ";
		case ResourceType.METAL_K_ORE_ID : return "Metal K (ore)";
		case ResourceType.METAL_M_ORE_ID : return "Metal M (ore)";
		case ResourceType.METAL_E_ORE_ID : return "Metal E (ore)";
		case ResourceType.METAL_N_ORE_ID : return "Metal N (ore)";
		case ResourceType.METAL_P_ORE_ID : return "Metal P (ore)";
		case ResourceType.METAL_S_ORE_ID : return "Metal S (ore)";
		case ResourceType.MINERAL_F_ID : return "Mineral F";
		case ResourceType.MINERAL_L_ID : return "Mineral L";
		case ResourceType.PLASTICS_ID : return "Plastic";
		case ResourceType.CONCRETE_ID : return "L-Concrete";
		case ResourceType.FERTILE_SOIL_ID : return "Fertile soil";
		case ResourceType.FUEL_ID : return "Fuel";
		case ResourceType.GRAPHONIUM_ID : return "Graphonium";
		case ResourceType.SUPPLIES_ID : return "Supplies";			
		}
	}
    public static string GetResourcesDescription(int id)
    {
        switch (id)
        {
            default: return "No description";
            case 0: return "Nothing";
            case ResourceType.DIRT_ID: return "Organic cover of floating islands.";
            case ResourceType.FOOD_ID: return "Organic fuel for your citizens.";
            case ResourceType.LUMBER_ID:
                return "Different elastic wood, growing only in Last Sector Dominion. Used for building and sometimes decorating.";
            case ResourceType.STONE_ID:
                return "Nature material used in construction. Processing into L-Concrete.";
            case ResourceType.METAL_K_ID:
            case ResourceType.METAL_K_ORE_ID:
                return "Used in construction.";
            case ResourceType.METAL_M_ID:
            case ResourceType.METAL_M_ORE_ID:
                return  "Used in  machinery building.";
            case ResourceType.METAL_E_ID:
            case ResourceType.METAL_E_ORE_ID:
                return "Used in electronic components production.";
            case ResourceType.METAL_N_ID:
            case ResourceType.METAL_N_ORE_ID:
                return "Rare and expensive metal.";
            case ResourceType.METAL_P_ID:
            case ResourceType.METAL_P_ORE_ID:
                return "Used in mass-production.";
            case ResourceType.METAL_S_ID:
            case ResourceType.METAL_S_ORE_ID:
                return "Used in ship building.";
            case ResourceType.MINERAL_L_ID:
                return "Used to create plastic mass.";
            case ResourceType.MINERAL_F_ID:
                return "Very effective as fuel.";
            case ResourceType.PLASTICS_ID:
                return "Easy-forming by special influence relatively tough material, used for building and manufacturing";
            case ResourceType.CONCRETE_ID:
                return "Comfortable and easy-forming building material.";
            case ResourceType.FERTILE_SOIL_ID:
                return "Soil, appliable for growing edibles.";
            case ResourceType.FUEL_ID:
                return "Standart fuel for spaceship engine";
            case ResourceType.GRAPHONIUM_ID:
                return "Superstructured material, wrapping reality nearby";
            case ResourceType.SUPPLIES_ID: return "Well-packed food, medicaments and another life-support goods.";
        }
    }

	public static string GetAnnouncementString( GameAnnouncements announce) {
		switch (announce) {
		    default: return "<announcement not found>";
		    case GameAnnouncements.NotEnoughResources : return "Not enough resources!";
            case GameAnnouncements.NotEnoughEnergyCrystals: return "Not enough energy crystals";
            case GameAnnouncements.GameSaved: return "Game saved";
            case GameAnnouncements.GameLoaded: return "Load successful";
            case GameAnnouncements.SavingFailed: return "Saving failed";
            case GameAnnouncements.LoadingFailed: return "Loading failed";
            case GameAnnouncements.PowerFailure: return "Power failure";
            case GameAnnouncements.NewQuestAvailable: return "New quest available";
		}
	}

	public static string GetRestrictionPhrase(RestrictionKey rkey ) {
		switch (rkey) {
		    default : return "Action not possible";
		    case RestrictionKey.SideConstruction: return "Can be built only on side blocks";
		    case RestrictionKey.UnacceptableSurfaceMaterial: return "Unacceptable surface material";
            case RestrictionKey.HeightBlocked: return "Height blocked";
		}
	}


	public static string CostInCoins(float count) {
		switch (currentLanguage) {
		default:
		case Language.English: return count.ToString() + " coins";
		}
	}

	public static string AnnounceCrewReady( string name ) {
		switch (currentLanguage) {
		default:
		case Language.English: return "crew \" " + name + "\" ready";
		}
	}
    public static string AnnounceQuestCompleted (string name)
    {
        switch (currentLanguage)
        {
            default:
            case Language.English: return "Quest \"" + name + "\" completed!";
        }
    }

	public static string NameCrew() { // waiting for креатив
		switch (currentLanguage) {
		default:
		case Language.English:		return "crew " + Crew.lastNumber.ToString();
		}
	}

	public static string NameShuttle() { // waiting for креатив
		switch (currentLanguage) {
		default:
		case Language.English:		return "shuttle "+ Shuttle.lastIndex.ToString();
		}
	}

	public static string GetWord(LocalizedWord word) {
		switch (word) {
		    case LocalizedWord.Level: return "level"; // building technology level
		    case LocalizedWord.Offline: return "offline"; // out of power		    
		    case LocalizedWord.Dig : return "Dig";		   
		    case LocalizedWord.Gather: return "Gather";	// gather resources	    
            case LocalizedWord.UpgradeCost: return "Upgrade cost";
            case LocalizedWord.Upgrade:return "Upgrade"; // upgrade building
            case LocalizedWord.Cancel: return "Cancel"; // cancel work and cancel save
            case LocalizedWord.Buy: return "Buy";
            case LocalizedWord.Sell: return "Sell";
            case LocalizedWord.Limit: return "Limit"; // trade count limit
            case LocalizedWord.Demand: return "Demand";
            case LocalizedWord.Price: return "Price";
            case LocalizedWord.Trading: return "Trading";
            case LocalizedWord.Immigration: return "Immigration";            
            case LocalizedWord.Normal: return "Normal"; // birthrate (spawnrate)
            case LocalizedWord.Improved: return "Improved"; // birthrate (spawnrate)
            case LocalizedWord.Lowered:return "Lowered";//birthrate (spawnrate)
            case LocalizedWord.Dismiss: return "Dismiss"; // dismiss crew
            case LocalizedWord.Disassemble: return "Disassemble"; // disassemble shuttle to resources
            case LocalizedWord.Total: return "Total"; // storage volume string
            case LocalizedWord.Repair: return "Repair"; // repair shuttle
            case LocalizedWord.Save: return "Save"; // save game
            case LocalizedWord.Load: return "Load"; // load game
            case LocalizedWord.Options: return "Options";
            case LocalizedWord.Exit: return "Exit"; // exit game
            case LocalizedWord.Build: return "Build"; // switch to building mode
            case LocalizedWord.Shuttles: return "Shuttles";
            case LocalizedWord.Crews: return "Crews";
            case LocalizedWord.Reward: return "Reward"; // reward in coins
            case LocalizedWord.Delete: return "Delete"; // delete save
            case LocalizedWord.Rewrite: return "Rewrite"; // rewrite save?
            case LocalizedWord.Yes: return "Yes"; // rewrite - yes
            case LocalizedWord.MainMenu: return "Main menu";
		default: return "...";
		}
	}
    public static string GetPhrase(LocalizedPhrase lp)
    {
        switch (lp)
        {
            default: return "<...>";
            case LocalizedPhrase.PointsSec: return "points/sec";
            case LocalizedPhrase.StopDig:return "Stop digging";
            case LocalizedPhrase.StopGather: return "Stop gathering";
            case LocalizedPhrase.RequiredSurface:return "Required surface";
            case LocalizedPhrase.ImmigrationEnabled: return "Immigration enabled";
            case LocalizedPhrase.ImmigrationDisabled:  return "Immigration disabled";
            case LocalizedPhrase.TicketsLeft: return "Tickets left";
            case LocalizedPhrase.ColonistsArrived: return "Colonists arrived";
            case LocalizedPhrase.BirthrateMode: return "Spawnrate mode";
            case LocalizedPhrase.ShuttlesAvailable: return "Shuttles available";
            case LocalizedPhrase.CrewsAvailable: return "Crews available";
            case LocalizedPhrase.TransmittersAvailable: return "Transmitters available";
            case LocalizedPhrase.ImproveGears: return "Improve gears";
            case LocalizedPhrase.NoActivity: return "No activity";
            case LocalizedPhrase.CrewSlots: return "Crew slots";
            case LocalizedPhrase.HireNewCrew: return "Hire new crew";
            case LocalizedPhrase.ConstructShuttle: return "Construct shuttle";
            case LocalizedPhrase.ShuttleRepaired: return "A shuttle has been repaired";
            case LocalizedPhrase.ShuttleConstructed: return "New shuttle constructed";
            case LocalizedPhrase.ObjectsLeft: return "Objects left";
            case LocalizedPhrase.NoSavesFound: return "No saves found";
            case LocalizedPhrase.CreateNewSave: return "Create new save";
            case LocalizedPhrase.CameraZoom: return "Camera zoom";
            case LocalizedPhrase.LODdistance: return "LOD sprite distance";
            case LocalizedPhrase.GraphicQuality: return "Graphic quality";
        }
    }

    public static string GetRefusalReason(RefusalReason rr) {
        switch (rr) {
            default: return "bad developer guy prohibits it"; 
            case RefusalReason.Unavailable: return "Unavailable";
            case RefusalReason.MaxLevel: return "Maximum level reached";
            case RefusalReason.HQ_RR1: return "No docks built";
            case RefusalReason.HQ_RR2: return "No rolling shops built";
            case RefusalReason.HQ_RR3: return "No graphonium enrichers built";
            case RefusalReason.HQ_RR4: return "No chemical factories";
            case RefusalReason.HQ_RR5: return "No reason, just prohibited;";
            case RefusalReason.HQ_RR6: return "No reason, just prohibited;"; 
            case RefusalReason.SpaceAboveBlocked: return "Space above blocked";
            case RefusalReason.NoBlockBelow: return "No block below";
            case RefusalReason.NotEnoughSlots: return "Not enough slots";
        }
    }

    public static string GetActionLabel(LocalizationActionLabels label)
    {
        switch (label)
        {
            default: return "No activity";
            case LocalizationActionLabels.Extracted: return "extracted";
            case LocalizationActionLabels.WorkStopped: return "Work has stopped";
            case LocalizationActionLabels.BlockCompleted: return "Block completed";
            case LocalizationActionLabels.MineLevelFinished: return "Mine level finished";
            case LocalizationActionLabels.CleanInProgress: return "Clean in progress";
            case LocalizationActionLabels.DigInProgress: return "Dig in progress";
            case LocalizationActionLabels.GatherInProgress: return "Gather in progress";
        }
    }

    #region questsData
    public static void FillProgressQuest(Quest q)
    {
        switch (q.type)
        {
            case QuestType.Progress:
                switch ((ProgressQuestID)q.subIndex)
                {
                    case ProgressQuestID.Progress_HousesToMax:
                        q.name = "Living space";
                        q.description = "Maybe your citizen want more comfortable houses. Provide all your citizens with housing adequate to your technology level.";
                        q.steps[0] = "Housing provided  ";
                        break;
                    case ProgressQuestID.Progress_2Docks:
                        q.name = "Trade basement";
                        q.description = "Establish your colony's state by constructing two docks. Keep in mind, that two docks cannot be built on the same height, if they are on the same side of the island";
                        q.steps[0] = "Docks built ";
                        break;
                    case ProgressQuestID.Progress_2Storages:
                        q.name = "Storage infrastructure";
                        q.description = "Enlarge your storage space by building 2 new warehouses (any level)";
                        q.steps[0] = "Warehouses built ";
                        break;
                    case ProgressQuestID.Progress_Tier2:
                        q.name = "Techology progress I";
                        q.description = "It is time to grow your settlement up. Upgrade your HQ.";
                        q.steps[0] = "Upgrade HQ to level 2";
                        break;
                    case ProgressQuestID.Progress_300Population:
                        q.name = "First colonization wave";
                        q.description = "Your colony needs new members to advance. Use immigration panel in docks to bring new citizens to your island.";
                        q.steps[0] = "Colonists arrived ";
                        break;
                    case ProgressQuestID.Progress_OreRefiner:
                        q.name = "Ore refining";
                        q.description = "Your mines produces too many waste. Build and launch new ore refining facility to get much more resources from the mine dumps";
                        q.steps[0] = "Build " + GetStructureName(Structure.ORE_ENRICHER_2_ID);
                        break;
                    case ProgressQuestID.Progress_HospitalCoverage:
                        q.name = "Medical support";
                        q.description = "You should build enough hospitals to provide adequate medical supply to all your citizens";
                        q.steps[0] = "Medical supply coefficient ";
                        break;
                    case ProgressQuestID.Progress_Tier3:
                        q.name = "Technology progress II";
                        q.description = "Upgrade your HQ to level 3";
                        q.steps[0] = "Upgrade HQ to level 3";
                        break;
                    case ProgressQuestID.Progress_4MiniReactors:
                        q.name = "Four-chambered heart";
                        q.description = "Energy is the lifeblood of settlement and it will never be much enough. Build 4 mini reactors to be prepared to the further development";
                        q.steps[0] = "Mini reactors built ";
                        break;
                    case ProgressQuestID.Progress_100Fuel:
                        q.name = "Space gas station";
                        q.description = "There is a lot of space travellers who will be were happy to refuel ships at your docks. Build fuel factory and produce 100 points of fuel to help exploring the Last Sector";
                        q.steps[0] = "Collect 100 fuel ";
                        break;
                    case ProgressQuestID.Progress_XStation:
                        q.name = "Experimental prognosis";
                        q.description = "The Last Sector is dangerous place. Organise your own meteorologist team to foresee threats";
                        q.steps[0] = "Build " + GetStructureName(Structure.XSTATION_ID);
                        break;
                    case ProgressQuestID.Progress_Tier4:
                        q.name = "Technology progress III";
                        q.description = "Upgrade your HQ to level 4";
                        q.steps[0] = "Upgrade HQ to level 4";
                        break;
                    case ProgressQuestID.Progress_CoveredFarm:
                        q.name = "Covered field";
                        q.description = "Replace your old farm with new covered one";
                        q.steps[0] = "Build " + GetStructureName(Structure.FARM_4_ID); ;
                        break;
                    case ProgressQuestID.Progress_CoveredLumbermill:
                        q.name = "Covered forest";
                        q.description = "Replace your old lumbermills with new covered one";
                        q.steps[0] = "Build " + GetStructureName(Structure.LUMBERMILL_4_ID); ;
                        break;
                    case ProgressQuestID.Progress_Reactor:
                        q.name = "Power well";
                        q.description = "Built a massive graphonium reactor";
                        q.steps[0] = "Build " + GetStructureName(Structure.GRPH_REACTOR_4_ID); ;
                        break;
                    case ProgressQuestID.Progress_FirstExpedition:
                        q.name = "Brave explorers";
                        q.description = "Initialize and succeed your first expedition in the mysterious Last Sector. For that, you should assemble a team in the recruiting center, construct a shuttle for them and prepare the new expedition in the Expedition corpus.";
                        q.steps[0] = "Crew assembled ";
                        q.steps[1] = "Shuttle constructed";
                        q.steps[2] = "Expedition launched";
                        q.steps[3] = "Expedition succeed";
                        break;
                    case ProgressQuestID.Progress_Tier5:
                        q.name = "Technology progress IV";
                        q.description = "Upgrade your HQ to level 5";
                        q.steps[0] = "Upgrade HQ to level 5";
                        break;
                    case ProgressQuestID.Progress_FactoryComplex:
                        q.name = "Complex factory";
                        q.description = "Construct factory onto factory block to make a combined factory";
                        q.steps[0] = "Factory block constructed ";
                        q.steps[1] = "Factory over it completed ";
                        break;
                    case ProgressQuestID.Progress_SecondFloor:
                        q.name = "Second floor";
                        q.description = "Construct a building onto the column";
                        q.steps[0] = "Column constructed ";
                        q.steps[1] = "Building over it completed ";
                        break;
                }
                break;
        }
    }
    #endregion
}

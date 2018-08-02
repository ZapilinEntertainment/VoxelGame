public enum Language{English, Russian};
public enum LocalizedWord{Level, Offline, Dig, Upgrade, UpgradeCost, Cancel, Buy, Sell, Limit, Demand, Price, Trading, Gather, Immigration,  Normal, Improved, Lowered,  Dismiss, Disassemble, Total, Repair}
public enum LocalizedPhrase { StopDig, StopGather, RequiredSurface, ImmigrationEnabled, ImmigrationDisabled, TicketsLeft, PointsSec, BirthrateMode, ShuttlesAvailable, CrewsAvailable, TransmittersAvailable,
ImproveGears, NoActivity, CrewSlots, HireNewCrew, ConstructShuttle, ShuttleRepaired, ShuttleConstructed}
public enum LocalizationActionLabels {Extracted, WorkStopped, BlockCompleted, MineLevelFinished }
public enum GameAnnouncements{NotEnoughResources, NotEnoughEnergyCrystals, GameSaved, GameLoaded, SavingFailed, LoadingFailed };
public enum RestrictionKey{SideConstruction, UnacceptableSurfaceMaterial}
public enum RefusalReason {Unavailable, MaxLevel, HQ_RR1, HQ_RR2, HQ_RR3, HQ_RR4, HQ_RR5, HQ_RR6, SpaceAboveBlocked, NoBlockBelow, NotEnoughSlots}

public static class Localization {

	public static string ui_build, ui_dig_block, ui_pourIn, ui_clear, ui_storage_name, ui_stopWork, 
	ui_accept_destruction_on_clearing, ui_accept, ui_decline, ui_choose_block_action, ui_toPlain, ui_toGather, ui_cancelGathering, ui_workers, 
	ui_dig_in_progress, ui_clean_in_progress, ui_gather_in_progress, ui_pouring_in_progress, ui_activeSelf, ui_immigration, ui_trading, ui_buy, ui_sell,
	ui_selectResource, ui_immigrationEnabled, ui_immigrationDisabled, ui_immigrationPlaces, ui_close, ui_reset, ui_add_transaction, ui_setMode, ui_currentMode,
	ui_changeMaterial, ui_heightBlocked, ui_buildOnSideOnly, ui_freeSlots, ui_recruitmentInProgress, ui_showCrewCard, ui_showCrewsList, ui_noShip,
	ui_assemblyInProgress, ui_showVesselsList, ui_points_sec, ui_graphic_settings;
	public static string announcement_powerFailure, announcement_starvation, announcement_peopleArrived, announcement_notEnoughResources,
	announcement_stillWind;
	public static string objects_left,  sales_volume, min_value_to_trade, empty, vessel, change,cost;
	public static string lowered_birthrate, normal_birthrate, improved_birthrate, material_required, no_activity, block,resources,coins;
	public static string rollingShop_gearsProduction, rollingShop_boatPartsProduction;
	public static string hangar_noShuttle, hangar_noCrew, hangar_hireCrew, hangar_hireCost, hangar_crewSalary, hangar_repairFor, hangar_readiness,
	hangar_refuel;
	public static string crew_membersCount,crew_stamina,crew_perception, crew_persistence, crew_bravery, crew_techSkills, crew_survivalSkills, crew_teamWork,
	crew_successfulMissions, crew_totalMissions;
	public static string quests_vesselsAvailable, quests_transmittersAvailable, quests_crewsAvailable, quests_vesselsRequired, quests_crewsRequired,
	quests_no_suitable_vessels;
	public static Language currentLanguage;

	static Localization() {
		ChangeLanguage(Language.English);
	}

	public static void ChangeLanguage(Language lan ) {
		switch (lan) {
		case Language.English:			
		

			ui_build = "Build"; ui_clear = "Clear"; ui_dig_block = "Dig block"; ui_pourIn = "Pour in";
			ui_storage_name = "Storage"; 
			ui_stopWork = "Stop works";
			ui_accept = "Yes"; ui_decline = "No";
			ui_choose_block_action = "Choose block action";
			ui_toPlain = "Plain ground"; ui_workers = "Workers : ";
			ui_toGather = "Gather resources"; ui_cancelGathering = "Stop gathering";
			ui_dig_in_progress = "Digging in progress"; ui_clean_in_progress = "Clearing in progress"; 
			ui_gather_in_progress = "Gathering in progress"; ui_pouring_in_progress = "Pouring in progress";
			ui_accept_destruction_on_clearing = "Demolish all buildings in zone?";
			ui_activeSelf = "Active";
			ui_immigration = "Immigration"; ui_trading = "Trading";
			ui_buy = "Buy"; ui_sell = "Sell"; 
			ui_selectResource = "Select resource"; ui_add_transaction = "Add transaction";
			ui_immigrationEnabled = "Immigration allowed"; ui_immigrationDisabled = "Immigration is not possible"; 
			ui_immigrationPlaces = "Immigrants count";
			ui_close = "Close"; ui_reset = "Reset";
			ui_setMode = "Set mode"; ui_currentMode = "Current mode";
			ui_changeMaterial = "Change material";
			ui_heightBlocked = "Height blocked";
			ui_buildOnSideOnly = "Can be placed only on side blocks";
			ui_freeSlots = "Free slots";
			ui_showCrewCard = "Show crew card";
			ui_showCrewsList = "Show crews list"; ui_showVesselsList = "Show vessels list";
			ui_noShip = "No ship";
			ui_assemblyInProgress = "Assembly in progress"; // shuttle assembling
			ui_points_sec = "points/sec";
			ui_graphic_settings = "Graphic settings";

			announcement_powerFailure = "Power Failure";
			announcement_starvation = "People are starving!";
			announcement_peopleArrived = "New colonists arrived";
			announcement_notEnoughResources = "Not enough resources";
			announcement_stillWind = "No wind! All wind generators stopped";

			objects_left = "objects left";
			sales_volume = "Sales volume";
			min_value_to_trade = "Limit";
			lowered_birthrate = "Low birthrate";
			normal_birthrate = "Normal birthrate";
			improved_birthrate = "Stimulate birthrate";
			material_required = "Required material: ";
			no_activity = "No activity";
			block = "block";
			vessel = "vessel";
			change = "change"; cost ="cost";

			hangar_noShuttle = "No shuttle";
			hangar_noCrew = "No crew";
			hangar_hireCrew = "Hire crew";
			hangar_hireCost = "Hire cost";
			hangar_crewSalary = "Monthly payment";
			hangar_repairFor = "Repair for";
			resources = "resources";
			coins = "coins";
			hangar_readiness = "Readiness";
			hangar_refuel = "Refuel";

			crew_membersCount = "Members count";
			crew_stamina = "Stamina";
			crew_perception = "Perception";
			crew_persistence = "Persistence";
			crew_bravery = "Bravery";
			crew_techSkills = "Tech skills";
			crew_survivalSkills = "Survival skills";
			crew_teamWork = "Team work";
			crew_successfulMissions = "Successful missions";
			crew_totalMissions = "Total missions";

			quests_vesselsAvailable = "Vessels available";
			quests_transmittersAvailable = "Transmitters available";
			quests_crewsAvailable = "Crews available";
			quests_crewsRequired = "Crews required";
			quests_vesselsRequired = "Vessels required";
			quests_no_suitable_vessels = "No suitable vessels";

			rollingShop_gearsProduction = "Gears production";
			rollingShop_boatPartsProduction = "Boat parts production";

			currentLanguage = Language.English;
			break;
		case  Language.Russian: 
			ui_storage_name = "Склад";
			ui_accept_destruction_on_clearing = "Снести все здания в зоне покрытия?";
			ui_choose_block_action = "Выберите действие с блоком";
			ui_dig_block = "Выкопать блок";
			ui_pourIn = "Засыпать блок";
			ui_toPlain = "Разровнять"; ui_workers = "Рабочие : ";
			ui_toGather = "Собрать ресурсы"; ui_cancelGathering = "Остановить сбор ресурсов";
			ui_dig_in_progress = "Идет извлечение грунта"; ui_clean_in_progress = "Идет очистка"; 
			ui_gather_in_progress = "Идет сбор"; ui_pouring_in_progress = "Идет засыпка";
			ui_stopWork = "Остановить работы"; ui_activeSelf = "Работает";
			ui_immigration = "Иммиграция"; ui_trading = "Торговля";
			ui_buy = "Покупка"; ui_sell = "Продажа"; 
			ui_selectResource = "Выберите ресурс";  ui_add_transaction = "Добавить операцию";
			ui_immigrationEnabled = "Иммиграция разрешена"; ui_immigrationDisabled = "Въезд в город закрыт"; 
			ui_immigrationPlaces = "Мест для приёма иммигрантов";
			ui_close = "Закрыть"; ui_reset = "Сброс";
			ui_setMode = "Изменить режим"; ui_currentMode = "Текущий режим";


			announcement_powerFailure = "Энергоснабжение нарушено!";
			announcement_starvation = "Закончилась провизия!";
			announcement_peopleArrived = "Прибыли новые поселенцы";
			announcement_notEnoughResources = "Недостаточно ресурсов!";
			announcement_stillWind = "Безветрие! Все ветрогенераторы остановились";

			objects_left = "осталось";
			sales_volume = "Объём продажи";
			min_value_to_trade = "Ограничение";
			no_activity = "Бездействует"; // not a developer status!!!

			rollingShop_gearsProduction = "Производство оборудования";
			rollingShop_boatPartsProduction = "Производство комплектующих для шаттлов";

			currentLanguage = Language.Russian;
			break;	
		}
	}


    public static string GetStructureName(int id) {
        switch (id) {
            default: return "Unknown building";
            case Structure.PLANT_ID: return "Some plant";
            case Structure.LANDED_ZEPPELIN_ID: return "Landed Zeppelin";
            case Structure.STORAGE_0_ID: return "Primary storage";
            case Structure.STORAGE_1_ID: return "Storage";
            case Structure.STORAGE_2_ID: return "Storage";
            case Structure.STORAGE_3_ID: return "Storage";
            case Structure.STORAGE_5_ID: return "Storage";
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
		}
	}

	public static string GetRestrictionPhrase(RestrictionKey rkey ) {
		switch (rkey) {
		default : return "Action not possible";
		case RestrictionKey.SideConstruction: return "Can be built only on side blocks";
		case RestrictionKey.UnacceptableSurfaceMaterial: return "Unacceptable surface material";
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
		    case LocalizedWord.Level: return "level"; 
		    case LocalizedWord.Offline: return "offline";		    
		    case LocalizedWord.Dig : return "Dig";		   
		    case LocalizedWord.Gather: return "Gather";		    
            case LocalizedWord.UpgradeCost: return "Upgrade cost";
            case LocalizedWord.Upgrade:return "Upgrade";
            case LocalizedWord.Cancel: return "Cancel";
            case LocalizedWord.Buy: return "Buy";
            case LocalizedWord.Sell: return "Sell";
            case LocalizedWord.Limit: return "Limit";
            case LocalizedWord.Demand: return "Demand";
            case LocalizedWord.Price: return "Price";
            case LocalizedWord.Trading: return "Trading";
            case LocalizedWord.Immigration: return "Immigration";            
            case LocalizedWord.Normal: return "Normal"; // birthrate
            case LocalizedWord.Improved: return "Improved"; // birthrate
            case LocalizedWord.Lowered:return "Lowered";//birthrate
            case LocalizedWord.Dismiss: return "Dismiss";
            case LocalizedWord.Disassemble: return "Disassemble";
            case LocalizedWord.Total: return "Total"; // storage volume string
            case LocalizedWord.Repair: return "Repair";
            
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
            case LocalizedPhrase.BirthrateMode: return "Birthrate mode";
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
        }
    }
}

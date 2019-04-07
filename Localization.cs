using UnityEngine;

public enum Language : ushort{English, Russian}; // menuUI - options preparing
public enum LocalizedWord : ushort {
    Buy, Cancel, Close, Crew, Dig, Expedition,Launch,Level,Mission, Offline, Progress, Sell, Upgrade, UpgradeCost,   Limitation, Demand, Price, Trading, Gather, Colonization,  Normal, Improved, Lowered,  Dismiss, Disassemble, Total, 
Save, Load, Options, Exit, Build, Shuttles, Crews, Reward, Delete, Rewrite, Yes, MainMenu, Accept, PourIn, Year_short, Month_short, Day_short,Day, Score, Disabled, Land_verb, Editor, Highscores, Generate, Size,
Difficulty, Start, Language, Quality, Apply, Continue, Menu, Stop, Play, Info, Goals, Refuse, Return};

public enum LocalizedPhrase : ushort { ConnectionLost, GoOnATrip, RecallExpedition, NoSuitableShuttles,  StopDig, StopGather, UnoccupiedTransmitters,RequiredSurface, ColonizationEnabled, ColonizationDisabled, TicketsLeft, ColonistsArrived, PointsSec, PerSecond, BirthrateMode, 
ImproveGears, NoActivity, NoArtifact, NoArtifacts, CrewSlots, NoFreeSlots, NotResearched,  HireNewCrew, NoCrew, ConstructShuttle, ShuttleConstructed, ShuttleOnMission, NoShuttle, ObjectsLeft, NoSavesFound, CreateNewSave, LODdistance, GraphicQuality, Ask_DestroyIntersectingBuildings,
MakeSurface, BufferOverflow, NoEnergySupply, PowerFailure, NoMission, NoHighscores, NoTransmitters, AddCrew, NewGame, UsePresets,  GenerationType, NoLimit, UpperLimit,IterationsCount, ChangeSurfaceMaterial, CreateColumn, CreateBlock,
AddPlatform, OpenMap
}
public enum LocalizationActionLabels : ushort {Extracted, WorkStopped, BlockCompleted, MineLevelFinished, CleanInProgress, DigInProgress, GatherInProgress, PouringInProgress,
    FlyingToMissionPoint, FlyingHome, Dissmissed, TryingToLeave }
public enum GameAnnouncements : ushort{NotEnoughResources, NotEnoughEnergyCrystals, GameSaved, GameLoaded, SavingFailed, LoadingFailed, NewQuestAvailable, GamePaused,
    GameUnpaused, StorageOverloaded, ActionError, ShipArrived, NotEnoughFood, SetLandingPoint, IslandCollapsing, NewObjectFound};
public enum LocalizedTutorialHint : byte { Landing}
public enum RestrictionKey : ushort{SideConstruction, UnacceptableSurfaceMaterial, HeightBlocked}
public enum RefusalReason : ushort {Unavailable, MaxLevel, HQ_RR1, HQ_RR2, HQ_RR3, HQ_RR4, HQ_RR5, HQ_RR6, SpaceAboveBlocked, NoBlockBelow, NotEnoughSlots, WorkNotFinished, MustBeBuildedOnFoundationBlock, NoEmptySpace, AlreadyBuilt}
public enum LocalizedExpeditionStatus : byte { CannotReachDestination}

public static class Localization {
    public const int CREW_INFO_STRINGS_COUNT = 14;

    public static Language currentLanguage { get; private set; }

	static Localization() {
        int x = 0;
        if (PlayerPrefs.HasKey(GameConstants.BASE_SETTINGS_PLAYERPREF))
        {
            x = PlayerPrefs.GetInt(GameConstants.BASE_SETTINGS_PLAYERPREF);
        }
        if ((x&1) == 0) ChangeLanguage(Language.English); // default language
        else ChangeLanguage(Language.Russian);
	}

	public static void ChangeLanguage(Language lan ) {
        currentLanguage = lan;
	}



    public static string GetStructureName(int id) {
        switch (currentLanguage)
        {            
            case Language.Russian:
                switch (id)
                {                    
                    case Structure.PLANT_ID: return "Растение";
                    case Structure.LANDED_ZEPPELIN_ID: return "Временный штаб";
                    case Structure.STORAGE_0_ID: return "Основной склад";
                    case Structure.STORAGE_1_ID: return "Складское помещение";
                    case Structure.STORAGE_2_ID: return "Небольшой склад";
                    case Structure.STORAGE_3_ID: return "Склад";
                    case Structure.STORAGE_5_ID: return "Блок склада";
                    case Structure.CONTAINER_ID: return "Контейнер";
                    case Structure.MINE_ELEVATOR_ID: return "Подъёмник шахты";
                    case Structure.LIFESTONE_ID: return "Камень жизни";
                    case Structure.TENT_ID: return "Палатка";
                    case Structure.HOUSE_1_ID: return "Небольшой дом";
                    case Structure.HOUSE_2_ID: return "Жилой дом";
                    case Structure.HOUSE_3_ID: return "Улучшенный жилой дом";
                    case Structure.HOUSE_5_ID: return "Жилой блок";
                    case Structure.DOCK_ID: return "Док";
                    case Structure.DOCK_2_ID: return "Улучшенный док";
                    case Structure.DOCK_3_ID: return "Продвинутый док";
                    case Structure.ENERGY_CAPACITOR_1_ID: 
                    case Structure.ENERGY_CAPACITOR_2_ID: 
                    case Structure.ENERGY_CAPACITOR_3_ID: return "Аккумулятор";
                    case Structure.FARM_1_ID: 
                    case Structure.FARM_2_ID: 
                    case Structure.FARM_3_ID: return "Ферма";
                    case Structure.FARM_4_ID: return "Ферма закрытого типа ";
                    case Structure.FARM_5_ID: return "Блок фермы";
                    case Structure.HQ_2_ID: 
                    case Structure.HQ_3_ID: 
                    case Structure.HQ_4_ID: return "Администрация";
                    case Structure.LUMBERMILL_1_ID: 
                    case Structure.LUMBERMILL_2_ID: 
                    case Structure.LUMBERMILL_3_ID: return "Лесопилка";
                    case Structure.LUMBERMILL_4_ID: return "Лесопилка закрытого типа";
                    case Structure.LUMBERMILL_5_ID: return "Блок лесопилки";
                    case Structure.MINE_ID: return "Шахта";
                    case Structure.SMELTERY_1_ID: 
                    case Structure.SMELTERY_2_ID: 
                    case Structure.SMELTERY_3_ID: return "Плавильня";
                    case Structure.SMELTERY_5_ID: return "Плавильный блок";
                    case Structure.WIND_GENERATOR_1_ID: return "Потоковый генератор";
                    case Structure.BIOGENERATOR_2_ID: return "Биореактор";
                    case Structure.HOSPITAL_2_ID: return "Клиника";
                    case Structure.MINERAL_POWERPLANT_2_ID: return "Электростанция на минерале F";
                    case Structure.ORE_ENRICHER_2_ID: return "Обогатитель руды";
                    case Structure.WORKSHOP_ID: return "Мастерская";
                    case Structure.MINI_GRPH_REACTOR_3_ID: return "Малый графониевый реактор";
                    case Structure.FUEL_FACILITY_3_ID: return "Топливный завод";
                    case Structure.GRPH_REACTOR_4_ID: return "Графониевый реактор";
                    case Structure.PLASTICS_FACTORY_3_ID: return "Фабрика пластика";
                    case Structure.SUPPLIES_FACTORY_4_ID: return "Фабрика снаряжения";
                    case Structure.SUPPLIES_FACTORY_5_ID: return "Блок фабрики снаряжения";
                    case Structure.GRPH_ENRICHER_3_ID: return "Графониевый обогатитель";
                    case Structure.XSTATION_3_ID: return "Экспериментальная станция";
                    case Structure.QUANTUM_ENERGY_TRANSMITTER_5_ID: return "Квантовый передатчик энергии";
                    case Structure.CHEMICAL_FACTORY_4_ID: return "Химический завод";
                    case Structure.RESOURCE_STICK_ID: return "Идет сборка блока...";
                    case Structure.COLUMN_ID: return "Опора";
                    case Structure.SWITCH_TOWER_ID: return "Башня переключения";
                    case Structure.SHUTTLE_HANGAR_4_ID: return "Ангар";
                    case Structure.RECRUITING_CENTER_4_ID: return "Центр подготовки";
                    case Structure.EXPEDITION_CORPUS_4_ID: return "Экспедиционный корпус";
                    case Structure.QUANTUM_TRANSMITTER_4_ID: return "Передатчик дальнего действия";
                    case Structure.REACTOR_BLOCK_5_ID: return "Блок реактора";
                    case Structure.FOUNDATION_BLOCK_5_ID: return "Блок основания";
                    case Structure.CONNECT_TOWER_6_ID: return "Башня Сообщения";
                    case Structure.CONTROL_CENTER_6_ID: return "Контрольный центр";
                    case Structure.HOTEL_BLOCK_6_ID: return "Блок отеля";
                    case Structure.HOUSING_MAST_6_ID: return "Жилой шпиль";
                    case Structure.DOCK_ADDON_1_ID: return "Пристройка дока - 1";
                    case Structure.DOCK_ADDON_2_ID: return "Пристройка дока - 2";
                    case Structure.OBSERVATORY_ID: return "Обсерватория";
                    case Structure.ARTIFACTS_REPOSITORY_ID: return "Хранилище артефактов";
                    default: return "Неизвестное здание";
                }
            case Language.English:
            default:
                switch (id)
                {                    
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
                    case Structure.TENT_ID: return "Tent";
                    case Structure.HOUSE_1_ID: return "Small house";
                    case Structure.HOUSE_2_ID: return "House";
                    case Structure.HOUSE_3_ID: return "Advanced house";
                    case Structure.HOUSE_5_ID: return "Residential Block";
                    case Structure.DOCK_ID: return "Dock";
                    case Structure.DOCK_2_ID: return "Improved dock";
                    case Structure.DOCK_3_ID: return "Advanced dock";
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
                    case Structure.WIND_GENERATOR_1_ID: return "Stream generator";
                    case Structure.BIOGENERATOR_2_ID: return "Biogenerator";
                    case Structure.HOSPITAL_2_ID: return "Hospital";
                    case Structure.MINERAL_POWERPLANT_2_ID: return "Mineral F powerplant";
                    case Structure.ORE_ENRICHER_2_ID: return "Ore enricher";
                    case Structure.WORKSHOP_ID: return "Workshop";
                    case Structure.MINI_GRPH_REACTOR_3_ID: return "Small Graphonum reactor";
                    case Structure.FUEL_FACILITY_3_ID: return "Fuel facility";
                    case Structure.GRPH_REACTOR_4_ID: return "Graphonium reactor";
                    case Structure.PLASTICS_FACTORY_3_ID: return "Plastics factory";
                    case Structure.SUPPLIES_FACTORY_4_ID: return "Supplies factory";
                    case Structure.SUPPLIES_FACTORY_5_ID: return "Supplies factory Block";
                    case Structure.GRPH_ENRICHER_3_ID: return "Graphonium enricher";
                    case Structure.XSTATION_3_ID: return "Experimental station";
                    case Structure.QUANTUM_ENERGY_TRANSMITTER_5_ID: return "Quantum energy transmitter";
                    case Structure.CHEMICAL_FACTORY_4_ID: return "Chemical factory";
                    case Structure.RESOURCE_STICK_ID: return "Constructing block...";
                    case Structure.COLUMN_ID: return "Column";
                    case Structure.SWITCH_TOWER_ID: return "Switch tower";
                    case Structure.SHUTTLE_HANGAR_4_ID: return "Shuttle hangar";
                    case Structure.RECRUITING_CENTER_4_ID: return "Recruiting Center";
                    case Structure.EXPEDITION_CORPUS_4_ID: return "Expedition Corpus";
                    case Structure.QUANTUM_TRANSMITTER_4_ID: return "Long-range transmitter";
                    case Structure.REACTOR_BLOCK_5_ID: return "Reactor block";
                    case Structure.FOUNDATION_BLOCK_5_ID: return "Foundation block";
                    case Structure.CONNECT_TOWER_6_ID: return "Connect Tower";
                    case Structure.CONTROL_CENTER_6_ID: return "Control center";
                    case Structure.HOTEL_BLOCK_6_ID: return "Hotel block";
                    case Structure.HOUSING_MAST_6_ID: return "Housing spire";
                    case Structure.DOCK_ADDON_1_ID: return "Dock addon 1";
                    case Structure.DOCK_ADDON_2_ID: return "Dock addon 2";
                    case Structure.OBSERVATORY_ID: return "Observatory";
                    case Structure.ARTIFACTS_REPOSITORY_ID: return "Artifacts repository";
                    default: return "Unknown building";
                }
        }
    }
	public static string GetStructureDescription(int id) {
		switch (currentLanguage)
        {
            case Language.Russian:
                switch (id)
                {                    
                    case Structure.STORAGE_0_ID: return "Стартовое хранилище объёмом " + StorageHouse.GetMaxVolume(0) + " eдиниц.";
                    case Structure.STORAGE_1_ID: return "Небольшое хранилище объёмом " + StorageHouse.GetMaxVolume(1) + " eдиниц.";
                    case Structure.STORAGE_2_ID: return "Хранилище объёмом " + StorageHouse.GetMaxVolume(2) + " eдиниц.";
                    case Structure.STORAGE_3_ID: return "Хранилище объёмом " + StorageHouse.GetMaxVolume(3) + " eдиниц.";
                    case Structure.STORAGE_5_ID: return "Блок для хранения объёмом " + StorageHouse.GetMaxVolume(5) + " eдиниц.";
                    case Structure.CONTAINER_ID: return "Содержит ресурсы.";
                    case Structure.LIFESTONE_ID: return "Источает энергию жизни.";
                    case Structure.TENT_ID: return "Временное жильё.";
                    case Structure.HOUSE_1_ID: return "Небольшой дом на " + House.GetHousingValue(id) + " жилых мест.";
                    case Structure.HOUSE_2_ID: return "Жилой дом, вмещающий " + House.GetHousingValue(id) + " человек.";
                    case Structure.HOUSE_3_ID: return "Улучшенный жилой дом вмещает " + House.GetHousingValue(id) + " человек.";
                    case Structure.HOUSE_5_ID: return "В жилом блоке могут проживать до " + House.GetHousingValue(id) + " человек.";
                    case Structure.HOUSING_MAST_6_ID: return "Массивный жилой комплекс, вмещающий " + House.GetHousingValue(id) + " человек";
                    case Structure.DOCK_ID:
                        {
                            string s = Dock.SMALL_SHIPS_PATH_WIDTH.ToString();
                            return "Принимает поселенцев и товары. Для нормального функционирования нужен сквозной коридор " + s + " на " + s + " рядом с доком.";
                        }
                    case Structure.DOCK_2_ID:
                        {
                            string s = Dock.MEDIUM_SHIPS_PATH_WIDTH.ToString();
                            return "Может принимать средние торговые корабли. Для нормального функционирования нужен сквозной коридор " + s + " на " + s + " рядом с доком.";
                        }
                    case Structure.DOCK_3_ID:
                        {
                            string s = Dock.HEAVY_SHIPS_PATH_WIDTH.ToString();
                            return "Может принимать крупные торговые корабли. Для нормального функционирования нужен сквозной коридор " + s + " на " + s + " рядом с доком.";
                        }
                    case Structure.ENERGY_CAPACITOR_1_ID: 
                    case Structure.ENERGY_CAPACITOR_2_ID: 
                    case Structure.ENERGY_CAPACITOR_3_ID: return "Запасает до " + Building.GetEnergyCapacity(id) + "единиц энергии. Может конвертировать кристаллы в энергию.";
                    case Structure.FARM_1_ID: 
                    case Structure.FARM_2_ID: 
                    case Structure.FARM_3_ID: return "Специально подготовленная площадка для выращивания еды. Потребляет жизненную энергию острова. Может быть построена только на грунте.";
                    case Structure.FARM_4_ID: 
                    case Structure.FARM_5_ID: return "Постоянно производит некоторое количество еды. Не потребляет жизненную энергию острова.";
                    case Structure.LANDED_ZEPPELIN_ID:
                    case Structure.HQ_2_ID: 
                    case Structure.HQ_3_ID: 
                    case Structure.HQ_4_ID: return "Главное здание колонии. Производит немного энергии и имеет несколько жилых помещений.";
                    case Structure.LUMBERMILL_1_ID: 
                    case Structure.LUMBERMILL_2_ID: 
                    case Structure.LUMBERMILL_3_ID: return "Выращивает и рубит деревья. Потребляет жизненную энергию острова.";
                    case Structure.LUMBERMILL_4_ID: 
                    case Structure.LUMBERMILL_5_ID: return "Постоянно производит некоторое количество древесины. Не потребляет жизненную энергию острова.";
                    case Structure.MINE_ID: return "Добыча полезных ископаемых закрытым методом.";
                    case Structure.SMELTERY_1_ID: 
                    case Structure.SMELTERY_2_ID: 
                    case Structure.SMELTERY_3_ID: 
                    case Structure.SMELTERY_5_ID: return "Перерабатывает ресурсы.";
                    case Structure.WIND_GENERATOR_1_ID: return "Нестабильно вырабатывает энергию в зависимости от силы местных потоков. Лучше располагать как можно выше.";
                    case Structure.BIOGENERATOR_2_ID: return "Вырабатывает энергию, потребляя органическую материю";
                    case Structure.HOSPITAL_2_ID: return "Обеспечивает колонию медицинской помощью. Может регулировать темп появления новых жителей.";
                    case Structure.MINERAL_POWERPLANT_2_ID: return "Вырабатывает энергию, потребляя минерал F.";
                    case Structure.ORE_ENRICHER_2_ID: return "Позволяет добывать нужные руды из обычной горной породы.";
                    case Structure.WORKSHOP_ID: return "Улучшает или поддерживает в норме оборудование колонистов.";
                    case Structure.MINI_GRPH_REACTOR_3_ID: return "Вырабатывает энергию, ничего не потребляя.";
                    case Structure.FUEL_FACILITY_3_ID: return "Производит топливо для кораблей";
                    case Structure.REACTOR_BLOCK_5_ID:
                    case Structure.GRPH_REACTOR_4_ID: return "Вырабатывает большое количество энергии, потребляя Графониум.";
                    case Structure.PLASTICS_FACTORY_3_ID: return "Специализированная фабрика для производства пластика.";
                    case Structure.SUPPLIES_FACTORY_4_ID: 
                    case Structure.SUPPLIES_FACTORY_5_ID: return "Производит снаряжение для экспедиций и нужд колонии.";
                    case Structure.GRPH_ENRICHER_3_ID: return "Обогащает N-метал до Графония.";                    
                    case Structure.QUANTUM_ENERGY_TRANSMITTER_5_ID: return "Конденсирует лишнюю энергию в кристаллы. Может быть построен только одно такое здание!";
                    case Structure.SWITCH_TOWER_ID: return "При выделении включает срез слоя, на котором находится.";
                    case Structure.SHUTTLE_HANGAR_4_ID: return "Вмещает и обслуживает один челнок.";
                    case Structure.RECRUITING_CENTER_4_ID: return "Набирает и подготавливает команды исследователей из добровольцев.";
                    case Structure.EXPEDITION_CORPUS_4_ID: return "Центр управления миссиями.";
                    case Structure.QUANTUM_TRANSMITTER_4_ID: return "Обеспечивает связь с экспедициями за пределами острова.";                    
                    case Structure.FOUNDATION_BLOCK_5_ID: return "Содержит инфраструктуру для обеспечения высокоуровневых зданий";                              
                    case Structure.DOCK_ADDON_1_ID: return "Стройте вплотную к доку, чтобы улучшить его до уровня 2.";
                    case Structure.DOCK_ADDON_2_ID: return "Стройте вплотную к доку, чтобы улучшить его до уровня 3.";
                    case Structure.HOTEL_BLOCK_6_ID:
                    case Structure.CONTROL_CENTER_6_ID:
                    case Structure.CONNECT_TOWER_6_ID:
                    case Structure.CHEMICAL_FACTORY_4_ID:
                    case Structure.XSTATION_3_ID: return "<В разработке>";
                    case Structure.OBSERVATORY_ID: return "Отслеживает события в ближайшем пространстве. Нужно свободное пространство в 1 блок радиусом от самого низа до верха.";
                    case Structure.ARTIFACTS_REPOSITORY_ID: return "Хранит найденные артефакты. В случае отключения питания возможны потери.";
                    default: return "Без описания";
                }
            case Language.English:
            default:
                switch (id)
                {
                    case Structure.STORAGE_0_ID: return "Start storage building contains " + StorageHouse.GetMaxVolume(0) + " points.";
                    case Structure.STORAGE_1_ID: return "Small storage building contains " + StorageHouse.GetMaxVolume(1) + " points.";
                    case Structure.STORAGE_2_ID: return "This storage can contain " + StorageHouse.GetMaxVolume(2) + " points.";
                    case Structure.STORAGE_3_ID: return "This storage can contain " + StorageHouse.GetMaxVolume(3) + " points.";
                    case Structure.STORAGE_5_ID: return "Storage block can contain " + StorageHouse.GetMaxVolume(5) + " points.";
                    case Structure.CONTAINER_ID: return "Contain resources.";
                    case Structure.LIFESTONE_ID: return "Emit lifepower.";
                    case Structure.TENT_ID: return "Temporary housing.";
                    case Structure.HOUSE_1_ID: return "Small house for " + House.GetHousingValue(id) + " persons.";
                    case Structure.HOUSE_2_ID: return "Residential house for " + House.GetHousingValue(id) + " persons.";
                    case Structure.HOUSE_3_ID: return "Advanced house for " + House.GetHousingValue(id) + " persons.";
                    case Structure.HOUSE_5_ID: return "Residential block can be house for " + House.GetHousingValue(id) + " persons.";
                    case Structure.HOUSING_MAST_6_ID: return "Massive residential complex for " + House.GetHousingValue(id) + " persons";
                    case Structure.DOCK_ID:
                        {
                            string s = Dock.SMALL_SHIPS_PATH_WIDTH.ToString();
                            return "Receives new colonists and trade goods. Needs in " + s + " x " + s + " corridor close to the dock to function.";
                        }
                    case Structure.DOCK_2_ID:
                        {
                            string s = Dock.MEDIUM_SHIPS_PATH_WIDTH.ToString();
                            return "Maintain medium trade vessels.  Needs in " + s + " x " + s + " corridor close to the dock to function.";
                        }
                    case Structure.DOCK_3_ID:
                        {
                            string s = Dock.HEAVY_SHIPS_PATH_WIDTH.ToString();
                            return "Maintain heavy trade vessels.  Needs in " + s + " x " + s + " corridor close to the dock to function.";
                        }
                    case Structure.ENERGY_CAPACITOR_1_ID:
                    case Structure.ENERGY_CAPACITOR_2_ID:
                    case Structure.ENERGY_CAPACITOR_3_ID: return "Store up to " + Building.GetEnergyCapacity(id) + "energy points. Converts energy crystals to energy points.";
                    case Structure.FARM_1_ID:
                    case Structure.FARM_2_ID:
                    case Structure.FARM_3_ID: return "A field prepared for growing up food. Consumes island lifepower. Must be located on dirt.";
                    case Structure.FARM_4_ID:
                    case Structure.FARM_5_ID: return "Constantly produces food. Doesn't consume lifepower.";
                    case Structure.LANDED_ZEPPELIN_ID:
                    case Structure.HQ_2_ID:
                    case Structure.HQ_3_ID:
                    case Structure.HQ_4_ID: return "Colony's main building. Produces small amount of energy and has a small living space.";
                    case Structure.LUMBERMILL_1_ID:
                    case Structure.LUMBERMILL_2_ID:
                    case Structure.LUMBERMILL_3_ID: return "Grows and cuts trees. Consumes island lifepower.";
                    case Structure.LUMBERMILL_4_ID:
                    case Structure.LUMBERMILL_5_ID: return "Constantly produces wood. Doesn't consume lifepower.";
                    case Structure.MINE_ID: return "Extract fossils in closed way.";
                    case Structure.SMELTERY_1_ID:
                    case Structure.SMELTERY_2_ID:
                    case Structure.SMELTERY_3_ID:
                    case Structure.SMELTERY_5_ID: return "Process resources.";
                    case Structure.WIND_GENERATOR_1_ID: return "Generate energy in dependence of local streams. It is better to place as high as possible.";
                    case Structure.BIOGENERATOR_2_ID: return "Generate energy from organic.";
                    case Structure.HOSPITAL_2_ID: return "Supplies colony with healthcare. Can control the spawnrate.";
                    case Structure.MINERAL_POWERPLANT_2_ID: return "Generate energy from mineral F.";
                    case Structure.ORE_ENRICHER_2_ID: return "Extract ores from stone.";
                    case Structure.WORKSHOP_ID: return "Improve or stabilize colonist's gears.";
                    case Structure.MINI_GRPH_REACTOR_3_ID: return "Generate energy, consumes nothing.";
                    case Structure.FUEL_FACILITY_3_ID: return "Produces fuel for vessels.";
                    case Structure.REACTOR_BLOCK_5_ID:
                    case Structure.GRPH_REACTOR_4_ID: return "Generates a lot of energy, consumes Graphonium.";
                    case Structure.PLASTICS_FACTORY_3_ID: return "Factory specialized on plastics producing.";
                    case Structure.SUPPLIES_FACTORY_4_ID:
                    case Structure.SUPPLIES_FACTORY_5_ID: return "Produces supplies for expeditions and colony needs.";
                    case Structure.GRPH_ENRICHER_3_ID: return "Transform N-metal into Graphonium.";
                    case Structure.QUANTUM_ENERGY_TRANSMITTER_5_ID: return "Transform energy excess in energy crystals. It can be only one building of this type!";
                    case Structure.SWITCH_TOWER_ID: return "Being selected activates layer cut on its own height.";
                    case Structure.SHUTTLE_HANGAR_4_ID: return "Base and maintain one shuttle.";
                    case Structure.RECRUITING_CENTER_4_ID: return "Recruit and train exploring teams from volunteers.";
                    case Structure.EXPEDITION_CORPUS_4_ID: return "Control missions activities.";
                    case Structure.QUANTUM_TRANSMITTER_4_ID: return "Provide connection with expeditions outside the island.";
                    case Structure.FOUNDATION_BLOCK_5_ID: return "Infrastructure for support high-level structures.";
                    case Structure.DOCK_ADDON_1_ID: return "Build it next to dock to up it to level 2.";
                    case Structure.DOCK_ADDON_2_ID: return "Build it next to dock to up it to level 3.";
                    case Structure.HOTEL_BLOCK_6_ID:
                    case Structure.CONTROL_CENTER_6_ID:
                    case Structure.CONNECT_TOWER_6_ID:
                    case Structure.CHEMICAL_FACTORY_4_ID:
                    case Structure.XSTATION_3_ID: return "<In development>";
                    case Structure.OBSERVATORY_ID: return "Observing near space for events. Must have empty space in 1 block radius, from down to top";
                    case Structure.ARTIFACTS_REPOSITORY_ID: return "Contains artifacts. Power shortage may cause losses.";break;
                    default: return "No description.";
                }
        }
	}
    public static string GetResourceName(int id)
    {
        switch (currentLanguage)
        {
            case Language.Russian:
                {
                    switch (id)
                    {
                        case PoolMaster.MATERIAL_ADVANCED_COVERING_ID: return GetStructureName(Structure.FOUNDATION_BLOCK_5_ID);
                        case 0: return "Ничего";
                        case ResourceType.DIRT_ID: return "Земля";
                        case ResourceType.FOOD_ID: return "Пища";
                        case ResourceType.LUMBER_ID: return "Древесина";
                        case ResourceType.STONE_ID: return "Камень";
                        case ResourceType.METAL_K_ID: return "Металл K ";
                        case ResourceType.METAL_M_ID: return "Металл M ";
                        case ResourceType.METAL_E_ID: return "Металл E ";
                        case ResourceType.METAL_N_ID: return "Металл N ";
                        case ResourceType.METAL_P_ID: return "Металл P ";
                        case ResourceType.METAL_S_ID: return "Металл S ";
                        case ResourceType.METAL_K_ORE_ID: return "Металл K (руда)";
                        case ResourceType.METAL_M_ORE_ID: return "Металл M (руда)";
                        case ResourceType.METAL_E_ORE_ID: return "Металл E (руда)";
                        case ResourceType.METAL_N_ORE_ID: return "Металл N (руда)";
                        case ResourceType.METAL_P_ORE_ID: return "Металл P (руда)";
                        case ResourceType.METAL_S_ORE_ID: return "Металл S (руда)";
                        case ResourceType.MINERAL_F_ID: return "Минерал F";
                        case ResourceType.MINERAL_L_ID: return "Минерал L";
                        case ResourceType.PLASTICS_ID: return "Пластик";
                        case ResourceType.CONCRETE_ID: return "Бетон";
                        case ResourceType.FERTILE_SOIL_ID: return "Плодородная почва";
                        case ResourceType.FUEL_ID: return "Топливо";
                        case ResourceType.GRAPHONIUM_ID: return "Графоний";
                        case ResourceType.SUPPLIES_ID: return "Снаряжение";
                        case ResourceType.SNOW_ID: return "Снег";
                        default: return "Незарегистрированный ресурс";
                    }
                }
            default:
            case Language.English:
                {
                    switch (id)
                    {
                        case 0: return "Nothing";
                        case ResourceType.DIRT_ID: return "Dirt";
                        case ResourceType.FOOD_ID: return "Food";
                        case ResourceType.LUMBER_ID: return "Wood";
                        case ResourceType.STONE_ID: return "Stone";
                        case ResourceType.METAL_K_ID: return "Metal K ";
                        case ResourceType.METAL_M_ID: return "Metal M ";
                        case ResourceType.METAL_E_ID: return "Metal E ";
                        case ResourceType.METAL_N_ID: return "Metal N ";
                        case ResourceType.METAL_P_ID: return "Metal P ";
                        case ResourceType.METAL_S_ID: return "Metal S ";
                        case ResourceType.METAL_K_ORE_ID: return "Metal K (ore)";
                        case ResourceType.METAL_M_ORE_ID: return "Metal M (ore)";
                        case ResourceType.METAL_E_ORE_ID: return "Metal E (ore)";
                        case ResourceType.METAL_N_ORE_ID: return "Metal N (ore)";
                        case ResourceType.METAL_P_ORE_ID: return "Metal P (ore)";
                        case ResourceType.METAL_S_ORE_ID: return "Metal S (ore)";
                        case ResourceType.MINERAL_F_ID: return "Mineral F";
                        case ResourceType.MINERAL_L_ID: return "Mineral L";
                        case ResourceType.PLASTICS_ID: return "Plastic";
                        case ResourceType.CONCRETE_ID: return "L-Concrete";
                        case ResourceType.FERTILE_SOIL_ID: return "Fertile soil";
                        case ResourceType.FUEL_ID: return "Fuel";
                        case ResourceType.GRAPHONIUM_ID: return "Graphonium";
                        case ResourceType.SUPPLIES_ID: return "Supplies";
                        case ResourceType.SNOW_ID: return "Snow";
                        default: return "Unregistered resource";
                    }
                }
        }
        
    }
    public static string GetResourcesDescription(int id)
    {
        switch(currentLanguage)
        {
            case Language.Russian:
                {
                    switch (id)
                    {                        
                        case 0: return string.Empty;
                        case ResourceType.DIRT_ID: return "Органическое покрытие острова.";
                        case ResourceType.FOOD_ID: return "Топливо для живых.";
                        case ResourceType.LUMBER_ID:
                            return "Эластичная древесина, растущая на островах. Используются для производства пластика и строительства небольших конструкций.";
                        case ResourceType.STONE_ID:
                            return "Природный материал, иногда использующийся в строительстве. Перерабатывается в бетон.";
                        case ResourceType.METAL_K_ID:
                        case ResourceType.METAL_K_ORE_ID:
                            return "Используется в строительстве.";
                        case ResourceType.METAL_M_ID:
                        case ResourceType.METAL_M_ORE_ID:
                            return "Используется в машиностроении.";
                        case ResourceType.METAL_E_ID:
                        case ResourceType.METAL_E_ORE_ID:
                            return "Используется для производства электронных компонентов.";
                        case ResourceType.METAL_N_ID:
                        case ResourceType.METAL_N_ORE_ID:
                            return "Редкий и очень ценный металл.";
                        case ResourceType.METAL_P_ID:
                        case ResourceType.METAL_P_ORE_ID:
                            return "Используется для производства товаров потребления.";
                        case ResourceType.METAL_S_ID:
                        case ResourceType.METAL_S_ORE_ID:
                            return "Используется в кораблестроении.";
                        case ResourceType.MINERAL_L_ID:
                            return "Сырьё для производства пластика.";
                        case ResourceType.MINERAL_F_ID:
                            return "Эффективное ископаемое топливо.";
                        case ResourceType.PLASTICS_ID:
                            return "Легко формируемый материал, используемый в строительстве и производстве.";
                        case ResourceType.CONCRETE_ID:
                            return "Основной строительный материал.";
                        case ResourceType.FERTILE_SOIL_ID:
                            return "Пригодная для выращивания пищи почва.";
                        case ResourceType.FUEL_ID:
                            return "Стандартное топливо для кораблей.";
                        case ResourceType.GRAPHONIUM_ID:
                            return "Суперструктурированный материал, искажающий реальность вокруг себя.";
                        case ResourceType.SUPPLIES_ID: return "Плотно упакованная еда, медикаменты и другие необходимые вещи.";
                        case ResourceType.SNOW_ID: return "Кристаллизованная вода.";
                        default: return "Нет описания.";
                    }
                }
            case Language.English:
            default:
                {
                    switch (id)
                    {                        
                        case 0: return string.Empty;
                        case ResourceType.DIRT_ID: return "Organic cover of floating islands.";
                        case ResourceType.FOOD_ID: return "A fuel for living.";
                        case ResourceType.LUMBER_ID:
                            return "Elastic wood, growing on islands. Using in plastics processing and constructing small buildings.";
                        case ResourceType.STONE_ID:
                            return "Nature material used in construction. Processing into L-Concrete.";
                        case ResourceType.METAL_K_ID:
                        case ResourceType.METAL_K_ORE_ID:
                            return "Used in construction.";
                        case ResourceType.METAL_M_ID:
                        case ResourceType.METAL_M_ORE_ID:
                            return "Used in  machinery building.";
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
                            return "Easy-forming material, using in building and manufacturing";
                        case ResourceType.CONCRETE_ID:
                            return "Main building material.";
                        case ResourceType.FERTILE_SOIL_ID:
                            return "Soil, appliable for growing edibles.";
                        case ResourceType.FUEL_ID:
                            return "Standart fuel for spaceship engine";
                        case ResourceType.GRAPHONIUM_ID:
                            return "Superstructured material, wrapping reality nearby";
                        case ResourceType.SUPPLIES_ID: return "Well-packed food, medicaments and another life-support goods.";
                        case ResourceType.SNOW_ID: return "Crystalised N-water";
                        default: return "No description";
                    }
                }
        }
       
    }

	public static string GetAnnouncementString( GameAnnouncements announce) {
        switch (currentLanguage)
        {
            case Language.Russian:
                {
                    switch (announce)
                    {                        
                        case GameAnnouncements.NotEnoughResources: return "Недостаточно ресурсов";
                        case GameAnnouncements.NotEnoughEnergyCrystals: return "Недостаточно энергокристаллов";
                        case GameAnnouncements.NotEnoughFood: return "Нехватка продуктов!";
                        case GameAnnouncements.GameSaved: return "Игра сохранена";
                        case GameAnnouncements.GameLoaded: return "Игра успешно загружена";
                        case GameAnnouncements.SavingFailed: return "Не удалось сохранить";
                        case GameAnnouncements.LoadingFailed: return "Не удалось загрузить";
                        case GameAnnouncements.NewQuestAvailable: return "Доступно новое задание";
                        case GameAnnouncements.GamePaused: return "Игра приостановлена";
                        case GameAnnouncements.GameUnpaused: return "Игра продолжается";
                        case GameAnnouncements.StorageOverloaded: return "Склад переполнен";
                        case GameAnnouncements.ActionError: return "Ошибка, десу";
                        case GameAnnouncements.ShipArrived: return "В нашу гавань зашёл корабль";
                        case GameAnnouncements.SetLandingPoint: return "Установите место посадки";
                        case GameAnnouncements.IslandCollapsing: return "Остров рушится!";
                        case GameAnnouncements.NewObjectFound: return "Обсерватория : обнаружен новый объект";
                        default: return "<пустое уведомление>";
                    }
                }
            case Language.English:
            default:
                {
                    switch (announce)
                    {                        
                        case GameAnnouncements.NotEnoughResources: return "Not enough resources";
                        case GameAnnouncements.NotEnoughEnergyCrystals: return "Not enough energy crystals";
                        case GameAnnouncements.NotEnoughFood: return "Not enough food!";
                        case GameAnnouncements.GameSaved: return "Game saved";
                        case GameAnnouncements.GameLoaded: return "Load successful";
                        case GameAnnouncements.SavingFailed: return "Saving failed";
                        case GameAnnouncements.LoadingFailed: return "Loading failed";
                        case GameAnnouncements.NewQuestAvailable: return "New quest available";
                        case GameAnnouncements.GamePaused: return "Game paused";
                        case GameAnnouncements.GameUnpaused: return "Game unpaused";
                        case GameAnnouncements.StorageOverloaded: return "Storage overloaded";
                        case GameAnnouncements.ActionError: return "Error desu";
                        case GameAnnouncements.ShipArrived: return "A ship has docked";
                        case GameAnnouncements.SetLandingPoint: return "Set landing point";
                        case GameAnnouncements.IslandCollapsing: return "Island starts collapsing!";
                        case GameAnnouncements.NewObjectFound: return "Observatory: new object found";
                        default: return "<announcement not found>";
                    }
                }
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
    public static string GetTutorialHint(LocalizedTutorialHint lth)
    {
        switch (lth)
        {
            case LocalizedTutorialHint.Landing:
                return "Select landing zone - a place , where your HQ will set. Landing place must be a line of three same-height surfaces. " +
                    "When you select suitable surface, a contour will appear. All you will need is to click the \"Land\" button.";
            default: return "I think it is too easy for you to learn.";
        }
    }

	public static string CostInCoins(float count) {
		switch (currentLanguage) {
            case Language.Russian:
                {
                    int x = (int)count;
                    if (count - x > 0) return count.ToString() + " кристалла";
                    else
                    {
                        x %= 10;
                        if (x == 1) return count.ToString() + " кристалл";
                        else
                        {
                            if (x == 2 | x == 3 | x == 4) return count.ToString() + " кристалла";
                            else return count.ToString() + " кристаллов";
                        }
                    }
                }
		case Language.English:
            default:
                return count.ToString() + " crystals";
		}
	}
	
    public static string AnnounceQuestCompleted (string name)
    {
        switch (currentLanguage)
        {
            case Language.Russian:
                return "Задание \"" + name + "\" выполнено!";
            case Language.English:
            default:
                return "Quest \"" + name + "\" completed!";
        }
    }

    public static string AnnounceCrewReady(string name)
    {
        switch (currentLanguage)
        {
            case Language.Russian:
                return "команда \" " + name + "\" готова";
            case Language.English:
            default: return "crew \" " + name + "\" ready";
        }
    }
  
    public static string GetCrewInfo(Crew c)
    {
        // CREW_INFO_STRINS_COUNT = 14
        switch (currentLanguage)
        {
            case Language.Russian:
                {
                    string s = "Участников: " + c.membersCount.ToString() + " / " + Crew.MAX_MEMBER_COUNT.ToString() + "\n"
        + '\n'
        +"Опыта до следующего уровня: " +  string.Format("{0:0.##}", c.nextExperienceLimit - c.experience) + "\n"      
        + "Готовность: " + ((int)(c.stamina * 100f)).ToString() + "%\n" +
        + '\n'
        + "Восприятие: " + string.Format("{0:0.##}", c.perception) + "\n"
        + "Настойчивость: " + string.Format("{0:0.##}", c.persistence) + "\n"
        + "Храбрость: " + string.Format("{0:0.##}", c.bravery) + "\n"
        + "Технические навыки: " + string.Format("{0:0.##}", c.techSkills) + "\n"
        + "Навыки выживания: " + string.Format("{0:0.##}", c.survivalSkills) + "\n"
        + "Эффективность командной работы: " + string.Format("{0:0.##}", c.teamWork) + "\n"
        + "\n"
        + "Участие в миссиях: " + c.missionsParticipated.ToString() + "\n"
        + "Успешные миссии: " + c.missionsSuccessed.ToString();
                    return s;
                }
            case Language.English:
            default:
                {
                    string s = "Members: " + c.membersCount.ToString() + " / " + Crew.MAX_MEMBER_COUNT.ToString() + "\n"
        + '\n'
        +"Stamina: " + ((int)(c.stamina * 100f)).ToString() + "%\n"
        +"Experience need for next level: " + string.Format("{0:0.##}", c.nextExperienceLimit - c.experience) + "\n" 
        +'\n'
        + "Perception: " + string.Format("{0:0.##}", c.perception) + "\n"
        + "Persistence: " + string.Format("{0:0.##}", c.persistence) + "\n"
        + "Bravery: " + string.Format("{0:0.##}", c.bravery) + "\n"
        + "Technical skills: " + string.Format("{0:0.##}", c.techSkills) + "\n"
        + "Survival skills: " + string.Format("{0:0.##}", c.survivalSkills) + "\n"
        + "Team work efficientcy: " + string.Format("{0:0.##}", c.teamWork) + "\n"
        + "\n"
        + "Missions participated: " + c.missionsParticipated.ToString() + "\n"
        + "Missions successed: " + c.missionsSuccessed.ToString();
                    return s;
                }
        }
    }
    public static string GetCrewStatus(CrewStatus cs)
    {
        switch (currentLanguage)
        {
            case Language.Russian:
                {
                    switch (cs)
                    {
                        case CrewStatus.AtHome: return "На базе";
                        case CrewStatus.OnMission: return "На миссии";
                        case CrewStatus.Travelling: return "Путешествуют";
                        default: return "<статус>";
                    }
                }
            case Language.English:
            default:
                {
                    switch (cs)
                    {
                        case CrewStatus.AtHome: return "At home";
                        case CrewStatus.OnMission: return "On mission";
                        case CrewStatus.Travelling: return "Travelling";
                        default: return "<crew status>";
                    }
                }
        }       
    }
    public static string GetShuttleStatus(Shuttle s)
    {
        switch (currentLanguage)
        {
            case Language.Russian:
                {
                    switch (s.status)
                    {
                        case ShipStatus.Docked: return "В порту";
                        case ShipStatus.OnMission: return "На задании";
                        default: return "<статус челнока>";
                    }
                }
            case Language.English:
            default:
                {
                    switch (s.status)
                    {
                        case ShipStatus.Docked: return "Docked";
                        case ShipStatus.OnMission: return "On mission";
                        default: return "<shuttle status>";
                    }
                }
        }        
    }
    public static string GetArtifactStatus(Artifact.ArtifactStatus status)
    {
        switch (currentLanguage)
        {
            case Language.Russian:
                {
                    switch (status)
                    {                        
                        case Artifact.ArtifactStatus.Researching: return "Исследуется";
                        case Artifact.ArtifactStatus.UsingByCrew: return "Используется командой";
                        case Artifact.ArtifactStatus.UsingInMonument: return "Используется в монументе";
                        case Artifact.ArtifactStatus.OnConservation: return "Законсервирован";
                        case Artifact.ArtifactStatus.Exists:
                        default:  return string.Empty;
                    }
                }
            case Language.English:
            default:
                {
                    switch (status)
                    {                        
                        case Artifact.ArtifactStatus.Researching: return "Researching";
                        case Artifact.ArtifactStatus.UsingByCrew: return "Using by crew";
                        case Artifact.ArtifactStatus.UsingInMonument: return "Using in monument";
                        case Artifact.ArtifactStatus.OnConservation: return "On conservation";
                        case Artifact.ArtifactStatus.Exists:
                        default: return string.Empty;
                    }
                }
        }
    }

    #region naming
    public static string NameCrew()
    { // waiting for креатив
        switch (currentLanguage)
        {
            case Language.Russian: return "Команда " + Crew.lastFreeID.ToString();
            case Language.English:
            default: return "Сrew " + Crew.lastFreeID.ToString();
        }
    }
    public static string NameShuttle() { // waiting for креатив
		switch (currentLanguage) {
            case Language.Russian: return "Челнок " + Shuttle.lastIndex.ToString();            
		    case Language.English:
            default: return "shuttle "+ Shuttle.lastIndex.ToString();
		}
	}
    public static string NameArtifact()
    {
        return "artifact name";
    }
    #endregion

    public static string GetWord(LocalizedWord word) {
        switch (currentLanguage)
        {
            case Language.Russian:
                {
                    switch (word)
                    {
                        case LocalizedWord.Buy: return "Покупать";
                        case LocalizedWord.Cancel: return "Отмена"; // cancel work and cancel save
                        case LocalizedWord.Close: return "Закрыть";
                        case LocalizedWord.Crew: return "Команда";
                        case LocalizedWord.Dig: return "Копать";
                        case LocalizedWord.Expedition: return "Экспедиция";
                        case LocalizedWord.Gather: return "Собирать"; // gather resources	  
                        case LocalizedWord.Launch: return "Запустить";
                        case LocalizedWord.Level: return "уровень"; // building technology level
                        case LocalizedWord.Mission: return "Миссия";
                        case LocalizedWord.Offline: return "Не подключено"; // out of power		 
                        case LocalizedWord.Progress: return "Прогресс";
                        case LocalizedWord.Sell: return "Продавать";
                        case LocalizedWord.UpgradeCost: return "Стоимость улучшения";
                        case LocalizedWord.Upgrade: return "Улучшить"; // upgrade building
                            
                        case LocalizedWord.Limitation: return "Ограничение"; // trade count limit
                        case LocalizedWord.Demand: return "Спрос";
                        case LocalizedWord.Price: return "Цена";
                        case LocalizedWord.Trading: return "Торговля";
                        case LocalizedWord.Colonization: return "Колонизация";
                        case LocalizedWord.Normal: return "Нормальный"; // birthrate (spawnrate)
                        case LocalizedWord.Improved: return "Увеличенный"; // birthrate (spawnrate)
                        case LocalizedWord.Lowered: return "Пониженный";//birthrate (spawnrate)
                        case LocalizedWord.Dismiss: return "Распустить"; // dismiss crew
                        case LocalizedWord.Disassemble: return "Разобрать"; // disassemble shuttle to resources
                        case LocalizedWord.Total: return "Всего"; // storage volume string
                        case LocalizedWord.Save: return "Сохранить"; // save game
                        case LocalizedWord.Load: return "Загрузить"; // load game
                        case LocalizedWord.Options: return "Настройки";
                        case LocalizedWord.Exit: return "Выход"; // exit game
                        case LocalizedWord.Build: return "Построить"; // switch to building mode
                        case LocalizedWord.Shuttles: return "Челноки";
                        case LocalizedWord.Crews: return "Команды";
                        case LocalizedWord.Reward: return "Награда"; // reward in coins
                        case LocalizedWord.Delete: return "Удалить"; // delete save
                        case LocalizedWord.Rewrite: return "Перезаписать"; // rewrite save?
                        case LocalizedWord.Yes: return "Да"; // rewrite - yes
                        case LocalizedWord.MainMenu: return "Главное меню";
                        case LocalizedWord.Accept: return "Подтвердить";
                        case LocalizedWord.PourIn: return "Засыпать";// pour in the hole in cube block
                        case LocalizedWord.Year_short: return "Г:";
                        case LocalizedWord.Month_short: return "М:";
                        case LocalizedWord.Day_short: return "День:";
                        case LocalizedWord.Day: return "День";
                        case LocalizedWord.Score: return "Счёт";
                        case LocalizedWord.Disabled: return "Неактивно"; // when building is not active
                        case LocalizedWord.Land_verb: return "Приземлиться";
                        case LocalizedWord.Editor: return "Редактор";
                        case LocalizedWord.Highscores: return "Рекорды";
                        case LocalizedWord.Generate: return "Сгенерировать"; // new game -generate
                        case LocalizedWord.Size: return "Размер";
                        case LocalizedWord.Difficulty: return "Сложность";
                        case LocalizedWord.Start: return "Начать";
                        case LocalizedWord.Language: return "Язык";
                        case LocalizedWord.Quality: return "Качество графики";
                        case LocalizedWord.Apply: return "Применить";
                        case LocalizedWord.Continue: return "Продолжить";
                        case LocalizedWord.Menu: return "Меню";
                        case LocalizedWord.Stop: return "Остановить";
                        case LocalizedWord.Play: return "Играть";
                        case LocalizedWord.Info: return "Информация";
                        case LocalizedWord.Goals: return "Цели:";
                        case LocalizedWord.Refuse: return "Отказаться";
                        case LocalizedWord.Return: return "Вернуться";
                        default: return "...";
                    }
                }
            case Language.English:
            default:
                {
                    switch (word)
                    {
                        case LocalizedWord.Buy: return "Buy";
                        case LocalizedWord.Cancel: return "Cancel"; // cancel work and cancel save
                        case LocalizedWord.Close: return "Close"; // close a panel
                        case LocalizedWord.Crew: return "Crew";
                        case LocalizedWord.Dig: return "Dig";
                        case LocalizedWord.Expedition: return "Expedition";
                        case LocalizedWord.Gather: return "Gather"; // gather resources	    
                        case LocalizedWord.Launch: return "Launch";
                        case LocalizedWord.Level: return "level"; // building technology level
                        case LocalizedWord.Mission: return "Mission";
                        case LocalizedWord.Offline: return "offline"; // out of power	
                        case LocalizedWord.Progress: return "Progress";   
                        case LocalizedWord.Sell: return "Sell";
                        case LocalizedWord.UpgradeCost: return "Upgrade cost";
                        case LocalizedWord.Upgrade: return "Upgrade"; // upgrade building                                
                        
                        case LocalizedWord.Limitation: return "Limitation"; // trade count limit
                        case LocalizedWord.Demand: return "Demand";
                        case LocalizedWord.Price: return "Price";
                        case LocalizedWord.Trading: return "Trading";
                        case LocalizedWord.Colonization: return "Colonization";
                        case LocalizedWord.Normal: return "Normal"; // birthrate (spawnrate)
                        case LocalizedWord.Improved: return "Improved"; // birthrate (spawnrate)
                        case LocalizedWord.Lowered: return "Lowered";//birthrate (spawnrate)
                        case LocalizedWord.Dismiss: return "Dismiss"; // dismiss crew
                        case LocalizedWord.Disassemble: return "Disassemble"; // disassemble shuttle to resources
                        case LocalizedWord.Total: return "Total"; // storage volume string
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
                        case LocalizedWord.Accept: return "Accept";
                        case LocalizedWord.PourIn: return "Pour In";// pour in the hole in cube block
                        case LocalizedWord.Year_short: return "Y:";
                        case LocalizedWord.Month_short: return "M:";
                        case LocalizedWord.Day_short: return "Day:";
                        case LocalizedWord.Day: return "Day";
                        case LocalizedWord.Score: return "Score";
                        case LocalizedWord.Disabled: return "Disabled"; // when building is not active
                        case LocalizedWord.Land_verb: return "Land";
                        case LocalizedWord.Editor: return "Editor";
                        case LocalizedWord.Highscores: return "Highscores";
                        case LocalizedWord.Generate: return "Generate";
                        case LocalizedWord.Size: return "Size";
                        case LocalizedWord.Difficulty: return "Difficulty";
                        case LocalizedWord.Start: return "Start";
                        case LocalizedWord.Language: return "Language";
                        case LocalizedWord.Quality: return "Quality";
                        case LocalizedWord.Apply: return "Apply";
                        case LocalizedWord.Continue: return "Continue";
                        case LocalizedWord.Menu: return "Menu";
                        case LocalizedWord.Stop: return "Stop";
                        case LocalizedWord.Play: return "Play";
                        case LocalizedWord.Info: return "Info";
                        case LocalizedWord.Goals: return "Goals:";
                        case LocalizedWord.Refuse: return "Refuse";
                        case LocalizedWord.Return: return "Return"; // lol
                        default: return "...";
                    }
                }
        }
		
	}
    public static string GetPhrase(LocalizedPhrase lp)
    {
        switch (currentLanguage)
        {
            case Language.Russian:
                {
                    switch (lp)
                    {
                        case LocalizedPhrase.ConnectionLost: return "Связь потеряна";
                        case LocalizedPhrase.GoOnATrip: return "Отправить в путешествие";
                        case LocalizedPhrase.NoArtifact: return "Нет артефакта"; // у команды
                        case LocalizedPhrase.NoArtifacts: return "У вас нет артефактов"; // в хранилище
                        case LocalizedPhrase.NoSuitableShuttles: return "Нет подходящего челнока";
                        case LocalizedPhrase.NotResearched: return "Не исследован"; // artifact
                        case LocalizedPhrase.PointsSec: return " ед./сек";
                        case LocalizedPhrase.PerSecond: return "в секунду";
                        case LocalizedPhrase.RecallExpedition: return "Отозвать экспедицию";
                        case LocalizedPhrase.StopDig: return "Остановить добычу";
                        case LocalizedPhrase.StopGather: return "Остановить сбор";
                        case LocalizedPhrase.UnoccupiedTransmitters: return "Свободно передатчиков: ";
                        case LocalizedPhrase.RequiredSurface: return "Требуемая поверхность";
                        case LocalizedPhrase.ColonizationEnabled: return "Привозить колонистов";
                        case LocalizedPhrase.ColonizationDisabled: return "Не привозить колонистов";
                        case LocalizedPhrase.TicketsLeft: return "Запросов";
                        case LocalizedPhrase.ColonistsArrived: return "Прибыли колонисты";
                        case LocalizedPhrase.BirthrateMode: return "Режим появления";
                        case LocalizedPhrase.ImproveGears: return "Улучшать оборудование";
                        case LocalizedPhrase.NoActivity: return "Ничего не делать";
                        case LocalizedPhrase.CrewSlots: return "Ячейки команд";
                        case LocalizedPhrase.NoFreeSlots: return "Нет свободных ячеек";
                        case LocalizedPhrase.HireNewCrew: return "Собрать новую команду";
                        case LocalizedPhrase.NoCrew: return "Нет команды";
                        case LocalizedPhrase.ConstructShuttle: return "Собрать челнок";
                        case LocalizedPhrase.ShuttleConstructed: return "Собран новый челнок";
                        case LocalizedPhrase.ShuttleOnMission: return "Челнок на миссии";
                        case LocalizedPhrase.NoShuttle: return "Нет челнока";
                        case LocalizedPhrase.AddCrew: return "Добавить команду";
                        case LocalizedPhrase.ObjectsLeft: return "Осталось объектов";
                        case LocalizedPhrase.NoSavesFound: return "Нет сохранений";
                        case LocalizedPhrase.CreateNewSave: return "Создать новое сохранение";
                        case LocalizedPhrase.LODdistance: return "Дистанция уровня детализации";
                        case LocalizedPhrase.GraphicQuality: return "Качество графики";
                        case LocalizedPhrase.Ask_DestroyIntersectingBuildings: return "Снести все пересекающиеся здания?";
                        case LocalizedPhrase.MakeSurface: return "Сделать поверхность";
                        case LocalizedPhrase.BufferOverflow: return "Буфер переполнен"; // factory resource buffer overflowed
                        case LocalizedPhrase.NoEnergySupply: return "Нет энергии";
                        case LocalizedPhrase.PowerFailure: return "Отказ энергосистемы";
                        case LocalizedPhrase.NoMission: return "Нет миссии";
                        case LocalizedPhrase.NoHighscores: return "Еще нет рекордов";
                        case LocalizedPhrase.NoTransmitters: return "Нет передатчиков";
                        case LocalizedPhrase.NewGame: return "Новая игра";
                        case LocalizedPhrase.UsePresets: return "Использовать существующие";
                        case LocalizedPhrase.GenerationType: return "Тип генерации";
                        case LocalizedPhrase.NoLimit: return "Без ограничений";
                        case LocalizedPhrase.UpperLimit: return "Пороговое значение";
                        case LocalizedPhrase.IterationsCount: return "Количество циклов";
                        case LocalizedPhrase.ChangeSurfaceMaterial: return "Заменить покрытие";
                        case LocalizedPhrase.CreateBlock: return "Построить блок";
                        case LocalizedPhrase.CreateColumn: return "Построить опору";
                        case LocalizedPhrase.AddPlatform: return "Прикрепить платформу";
                        case LocalizedPhrase.OpenMap: return "Открыть карту";
                        default: return "<...>";
                    }
                }
            case Language.English:
            default:
                {
                    switch (lp)
                    {
                        case LocalizedPhrase.ConnectionLost: return "Connection lost";
                        case LocalizedPhrase.GoOnATrip: return "Go on a trip";
                        case LocalizedPhrase.NoArtifact: return "No artifact"; // crew has no artifact
                        case LocalizedPhrase.NoArtifacts: return "You have no artifacts"; // no artifacts in storage
                        case LocalizedPhrase.NoSuitableShuttles: return "No suitable shuttles";
                        case LocalizedPhrase.NotResearched: return "Not researched"; // artifact
                        case LocalizedPhrase.PointsSec: return "points/sec";
                        case LocalizedPhrase.PerSecond: return "per second";
                        case LocalizedPhrase.RecallExpedition: return "Recall expedition";
                        case LocalizedPhrase.StopDig: return "Stop digging";
                        case LocalizedPhrase.StopGather: return "Stop gathering";
                        case LocalizedPhrase.UnoccupiedTransmitters: return "Unoccupied transmitters: ";
                        case LocalizedPhrase.RequiredSurface: return "Required surface";
                        case LocalizedPhrase.ColonizationEnabled: return "Immigration enabled";
                        case LocalizedPhrase.ColonizationDisabled: return "Immigration disabled";
                        case LocalizedPhrase.TicketsLeft: return "Tickets left";
                        case LocalizedPhrase.ColonistsArrived: return "Colonists arrived";
                        case LocalizedPhrase.BirthrateMode: return "Spawnrate mode";
                        case LocalizedPhrase.ImproveGears: return "Improve gears";
                        case LocalizedPhrase.NoActivity: return "No activity";
                        case LocalizedPhrase.CrewSlots: return "Crew slots";
                        case LocalizedPhrase.NoFreeSlots: return "No free slots";
                        case LocalizedPhrase.HireNewCrew: return "Hire new crew";
                        case LocalizedPhrase.NoCrew: return "No crew";
                        case LocalizedPhrase.ConstructShuttle: return "Construct shuttle";
                        case LocalizedPhrase.ShuttleConstructed: return "New shuttle constructed";
                        case LocalizedPhrase.ShuttleOnMission: return "Shuttle on mission";
                        case LocalizedPhrase.NoShuttle: return "No shuttle";
                        case LocalizedPhrase.AddCrew: return "Add crew";
                        case LocalizedPhrase.ObjectsLeft: return "Objects left";
                        case LocalizedPhrase.NoSavesFound: return "No saves found";
                        case LocalizedPhrase.CreateNewSave: return "Create new save";
                        case LocalizedPhrase.LODdistance: return "LOD sprite distance";
                        case LocalizedPhrase.GraphicQuality: return "Graphic quality";
                        case LocalizedPhrase.Ask_DestroyIntersectingBuildings: return "Destroy all intersecting buildings?";
                        case LocalizedPhrase.MakeSurface: return "Make surface";
                        case LocalizedPhrase.BufferOverflow: return "Buffer overflow"; // factory resource buffer overflowed
                        case LocalizedPhrase.NoEnergySupply: return "No energy supply";
                        case LocalizedPhrase.PowerFailure: return "Power failure";
                        case LocalizedPhrase.NoMission: return "No mission";
                        case LocalizedPhrase.NoHighscores: return "No highscores yet";
                        case LocalizedPhrase.NoTransmitters: return "No transmitters";
                        case LocalizedPhrase.NewGame: return "New game";
                        case LocalizedPhrase.UsePresets: return "Use presets";
                        case LocalizedPhrase.GenerationType: return "Generation type";
                        case LocalizedPhrase.NoLimit: return "No limit";
                        case LocalizedPhrase.UpperLimit: return "Upper limit";
                        case LocalizedPhrase.IterationsCount: return "Iterations count";
                        case LocalizedPhrase.ChangeSurfaceMaterial:return "Change surface";
                        case LocalizedPhrase.CreateBlock: return "Create block";
                        case LocalizedPhrase.CreateColumn: return "Create column";
                        case LocalizedPhrase.AddPlatform: return "Add platform";
                        case LocalizedPhrase.OpenMap: return "Open map";
                        default: return "<...>";
                    }
                }
        }       
    }
    public static string GetRefusalReason(RefusalReason rr) {
        switch (currentLanguage)
        {
            case Language.Russian:
                {
                    switch (rr)
                    {
                        default: return "<заблокировано>";
                        case RefusalReason.Unavailable: return "Не все условия выполнены";
                        case RefusalReason.MaxLevel: return "Максимальный уровень";
                        case RefusalReason.HQ_RR1: return "Нет дока";
                        case RefusalReason.HQ_RR2: return "Нет мастерской";
                        case RefusalReason.HQ_RR3: return "Нет графониевого обогатителя";
                        case RefusalReason.HQ_RR4: return "Нет химзавода";

                        case RefusalReason.SpaceAboveBlocked: return "Пространство сверху блокируется";
                        case RefusalReason.NoBlockBelow: return "Ниже ничего нет";
                        case RefusalReason.NotEnoughSlots: return "Нет свободных ячеек";
                        case RefusalReason.WorkNotFinished: return "Работы не окончены";
                        case RefusalReason.MustBeBuildedOnFoundationBlock: return "Нужно строить на блоке основания";
                        case RefusalReason.NoEmptySpace: return "Нет свободного пространства";
                        case RefusalReason.AlreadyBuilt: return "Уже построено";
                    }
                }
            case Language.English:
            default:
                {
                    switch (rr)
                    {
                        default: return "<blocked>";
                        case RefusalReason.Unavailable: return "Requirements not met";
                        case RefusalReason.MaxLevel: return "Maximum level reached";
                        case RefusalReason.HQ_RR1: return "No docks built";
                        case RefusalReason.HQ_RR2: return "No workshops built";
                        case RefusalReason.HQ_RR3: return "No graphonium enrichers built";
                        case RefusalReason.HQ_RR4: return "No chemical factories";

                        case RefusalReason.SpaceAboveBlocked: return "Space above blocked";
                        case RefusalReason.NoBlockBelow: return "No block below";
                        case RefusalReason.NotEnoughSlots: return "Not enough slots";
                        case RefusalReason.WorkNotFinished: return "Work not finished";
                        case RefusalReason.MustBeBuildedOnFoundationBlock: return "Must be builded on Foundation Block";
                        case RefusalReason.NoEmptySpace: return "No empty space";
                        case RefusalReason.AlreadyBuilt: return "Already built";
                    }
                }
        }       
    }
    public static string GetActionLabel(LocalizationActionLabels label)
    {
        switch (currentLanguage)
        {
            case Language.Russian:
                {
                    switch (label)
                    {
                        default: return "Не работает";
                        case LocalizationActionLabels.Extracted: return "извлечено";
                        case LocalizationActionLabels.WorkStopped: return "Работы остановлены";
                        case LocalizationActionLabels.BlockCompleted: return "Блок завершён";
                        case LocalizationActionLabels.MineLevelFinished: return "Уровень шахты завершён";
                        case LocalizationActionLabels.CleanInProgress: return "Идёт очистка";
                        case LocalizationActionLabels.DigInProgress: return "Идёт добыча";
                        case LocalizationActionLabels.GatherInProgress: return "Идёт сбор";
                        case LocalizationActionLabels.PouringInProgress: return "Идёт засыпка";

                        case LocalizationActionLabels.FlyingHome: return "Возвращается домой";
                        case LocalizationActionLabels.FlyingToMissionPoint: return "Летит к точке миссии";
                        case LocalizationActionLabels.TryingToLeave: return "Пытается покинуть локацию";
                        case LocalizationActionLabels.Dissmissed: return "Распущена";
                    }
                }
            case Language.English:
            default:
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
                        case LocalizationActionLabels.PouringInProgress: return "Pouring in progress";

                        case LocalizationActionLabels.FlyingHome: return "Flying home";
                        case LocalizationActionLabels.FlyingToMissionPoint: return "Flying to mission point";
                        case LocalizationActionLabels.TryingToLeave: return "Trying to leave location";
                        case LocalizationActionLabels.Dissmissed: return "Dismissed";
                    }
                }
        }      
    }
    public static string GetExpeditionStatus(LocalizedExpeditionStatus les, Expedition e)
    {
        switch (les)
        {
            case LocalizedExpeditionStatus.CannotReachDestination: return "Expedition \"" + e.name + "\" cannot reach destination.";
            default: return "Expedition \"" + e.name + "\" is okay";
        }
    }
    public static string GetEndingTitle (GameEndingType endType)
    {
        switch (currentLanguage)
        {
            case Language.Russian:
                {
                    switch (endType)
                    {
                        case GameEndingType.ColonyLost: return "И никого не осталось";
                        case GameEndingType.TransportHubVictory: return "Вы установили прочную связь между вашей колонией и другими Ограниченными Мирами. Благодаря вам, будущее колонии в безопасности. Теперь вы свободны.";
                        case GameEndingType.ConsumedByLastSector: return "Ваш остров пропал в Последнем Секторе. Теперь, он в безопасности.";
                        case GameEndingType.ConsumedByReal: return "Ваш остров был выброшен обратно в реальный космос и теперь потерян навсегда.";
                        case GameEndingType.Default:
                        default:
                            return "Игра окончена";
                    }
                }
            case Language.English:
            default:
                {
                    switch (endType)
                    {
                        case GameEndingType.ColonyLost: return "All citizens gone.";
                        case GameEndingType.TransportHubVictory: return "Your established a thick connection between your colony and other Limited Worlds. Thanks to you, now colony's future is safe. You are free now.";
                        case GameEndingType.ConsumedByLastSector: return "Your island has been consumed by the Last Sector. It is in safe now.";
                        case GameEndingType.ConsumedByReal: return "Your island has been pushed back to real space and now it is lost forever.";
                        case GameEndingType.Default:
                        default:
                            return "Game Over";
                    }
                }
        }        
    }    

    public static string GetCredits()
    {
        switch (currentLanguage)
        {            
            case Language.Russian:
                return "Выдумано и реализовано Zapilin Entertainment (мной), 2018 - 2019гг н.э. \n" +
                    "Собрано на Unity Engine 2018.2. \n" +
                    "В процессе разработки использовались следующие freeware - программы: \n" +
                    " - Blender 2.69 от Blender Foundations \n" +
                    " - Paint.net 4.1.5 \n" +
                    " - Audacity 2.2.2 \n" +
                    " - MS Visual Studio Community 2017\n" +
                    "\n" +
                    "Использован шрифт \"neuropolitical rg\" от Typodermic Fonts Inc. (добавил адаптированную кириллицу). \n" +
                    "\n" +
                    "\n" +
                    "Вы можете поддержать мои разработки, купив сюжетные дополнения к Limited Worlds или сделав пожертвование на один из следующих кошельков: \n" +
                    "\nЯндекс кошелёк: 410 0155 8473 6426\n" +
                    "Paypal: paypal.me/mahariusls\n" +
                    "Не забудьте приложить к дотации комментарий :)";
            case Language.English:
                return "Imaginated and created by Zapilin Entertainment (me), 2018 - 2019 AD \n" +
                    "Builded on Unity Engine 2018.2. \n" +
                    "The following freeware was used in the development: \n" +
                    " - Blender 2.69 by Blender Foundations \n" +
                    " - Paint.net 4.1.5 \n" +
                    " - Audacity 2.2.2 \n" +
                    " - MS Visual Studio Community 2017\n" +
                    "\n" +
                    "Used font \"neuropolitical rg\" by Typodermic Fonts Inc. \n" +
                    "\n" +
                    "\n" +
                    "You can support my development by buying storyline addons for Limited Worlds or making a donation to one of this accounts: \n" +
                    "\nYandex money: 410 0155 8473 6426\n" +
                    "Paypal: paypal.me/mahariusls\n" +
                    "Don't forget to leave your commentary to the transaction :)";
            default:
                return "" +
                    "";
        }
    }

    #region questsData
    public static void FillProgressQuest(Quest q)
    {
        switch (currentLanguage)
        {
            case Language.Russian:
                {
                    switch (q.type)
                    {
                        case QuestType.Progress:
                            switch ((ProgressQuestID)q.subIndex)
                            {
                                case ProgressQuestID.Progress_HousesToMax:
                                    q.name = "Жилое пространство";
                                    q.description = "Возможно, ваши поселенцы хотят жить в условиях получше. Обеспечьте колонию жильём, соответствующим технологическому уровню.";
                                    q.steps[0] = "Обеспеченность жильём  ";
                                    break;
                                case ProgressQuestID.Progress_2Docks:
                                    q.name = "Основы торговли";
                                    q.description = "Упрочните положение своей колонии, построив два дока. Обратите внимание, чтобы доки работали, их полётные коридоры не должны пересекаться.";
                                    q.steps[0] = "Построено доков ";
                                    break;
                                case ProgressQuestID.Progress_2Storages:
                                    q.name = "Система хранения";
                                    q.description = "Расширьте свои инфраструктуру, построив два новых склада (любого уровня)";
                                    q.steps[0] = "Построено складов ";
                                    break;
                                case ProgressQuestID.Progress_Tier2:
                                    q.name = "Технологический прогресс I";
                                    q.description = "Время улучшений! Модернизируйте здание администрации до второго уровня";
                                    q.steps[0] = "Главное здание улучшено до уровня 2";
                                    break;
                                case ProgressQuestID.Progress_300Population:
                                    q.name = "Первая колонизационная волна";
                                    q.description = "Вашей колонии нужно развиваться. Используйте панель колонизации в доках, чтобы привлечь больше колонистов на свой остров.";
                                    q.steps[0] = "Колонистов прибыло ";
                                    break;
                                case ProgressQuestID.Progress_OreRefiner:
                                    q.name = "Переработка руд";
                                    q.description = "От раскопок отсаётся очень много камня, в котором остается еще очень много руды. Постройте обогатитель руды, чтобы получать максимум ресурсов.";
                                    q.steps[0] = GetStructureName(Structure.ORE_ENRICHER_2_ID) + " построен";
                                    break;
                                case ProgressQuestID.Progress_HospitalCoverage:
                                    q.name = "Медицинское обеспечение";
                                    q.description = "Вам стоит построить клинику, чтобы все колонисты могли получить медицинскую помощь в случае необходимости.";
                                    q.steps[0] = "Коэффициент медицинского обслуживания ";
                                    break;
                                case ProgressQuestID.Progress_Tier3:
                                    q.name = "Технологический прогресс II";
                                    q.description = "Улучшите главное здание до уровня 3";
                                    q.steps[0] = "Главное здание улучшено";
                                    break;
                                case ProgressQuestID.Progress_4MiniReactors:
                                    q.name = "Четырёхкамерное сердце";
                                    q.description = "Энергия - то, что наполняет вашу колонию жизнью. Постройте 4 мини-реактора, чтобы подготовить к дальнейшему развитию.";
                                    q.steps[0] = GetStructureName(Structure.MINI_GRPH_REACTOR_3_ID) + ", построено ";
                                    break;
                                case ProgressQuestID.Progress_100Fuel:
                                    q.name = "Космическая заправка";
                                    q.description = "Множество космических путешественников было бы радо заправляться в ваших доках. Постройте топливный завод и произведите 100 единиц топлива - так вы очень поможете исследователям Последнего Сектора.";
                                    q.steps[0] = "Собрать 100 топлива ";
                                    break;
                                case ProgressQuestID.Progress_XStation:
                                    q.name = "Экспериментальный прогноз <в разработке>";
                                    q.description = "Последний Сектор непредсказуем. Организуйте свою метеорологическую службу, чтобы предвидеть надвигающиеся угрозы.";
                                    q.steps[0] = GetStructureName(Structure.XSTATION_3_ID) + " построена";
                                    break;
                                case ProgressQuestID.Progress_Tier4:
                                    q.name = "Технологический прогресс III";
                                    q.description = "Улучшите здание администрации до уровня 4";
                                    q.steps[0] = "Главное здание улучшено";
                                    break;
                                case ProgressQuestID.Progress_CoveredFarm:
                                    q.name = "Поле под колпаком";
                                    q.description = "Замените обычную ферму крытой. Так вы стабилизируете производство пищи и снизите потребление жизненной энергии острова.";
                                    q.steps[0] = GetStructureName(Structure.FARM_4_ID) + " построена"; 
                                    break;
                                case ProgressQuestID.Progress_CoveredLumbermill:
                                    q.name = "Лес под крышей";
                                    q.description = "Замените обычную лесопилку крытой - это стабилизирует поток древесины и снизит расход жизненной силы острова.";
                                    q.steps[0] = GetStructureName(Structure.LUMBERMILL_4_ID) + " построена";
                                    break;
                                case ProgressQuestID.Progress_Reactor:
                                    q.name = "Источник энергии";
                                    q.description = "Постройте полноценный графониевый реактор.";
                                    q.steps[0] = GetStructureName(Structure.GRPH_REACTOR_4_ID) + " построен" ;
                                    break;
                                case ProgressQuestID.Progress_FirstExpedition:
                                    q.name = "Храбрые исследователи <в разработке>";
                                    q.description = "Подготовьте и успешно завершите экспедицию в загадочный Последний Сектор. Для этого вам нужно набрать команду, произвести для неё челнок, и сформировать экспедицию в Экспедиционном корпусе.";
                                    q.steps[0] = "Команда подготовлена ";
                                    q.steps[1] = "Челнок собран ";
                                    q.steps[2] = "Экспедиция запущена";
                                    q.steps[3] = "Миссия завершена успешно";
                                    break;
                                case ProgressQuestID.Progress_Tier5:
                                    q.name = "Технологический прогресс IV";
                                    q.description = "Улучшите здание администрации до уровня 5";
                                    q.steps[0] = "Главное здание улучшено";
                                    break;
                                case ProgressQuestID.Progress_FactoryComplex:
                                    q.name = "Фабричный комплекс <в разработке>";
                                    q.description = "Постройте фабрику на фабричном блоке, чтобы объединить их в фабричный комплекс.";
                                    q.steps[0] = "Фабричный блок построен ";
                                    q.steps[1] = "Фабрика над ним построена ";
                                    break;
                                case ProgressQuestID.Progress_SecondFloor:
                                    q.name = "Второй этаж";
                                    q.description = "Постройте здание поверх опоры";
                                    q.steps[0] = "Опора построена ";
                                    q.steps[1] = "Здание на ней построено ";
                                    break;
                                default: return;
                            }
                            break;
                        case QuestType.Endgame:
                            switch ((EndgameQuestID)q.subIndex)
                            {
                                case EndgameQuestID.Endgame_TransportHub_step1:
                                    q.name = "Создание транспортного узла - начало";
                                    q.description = "Превращение колонии в надёжный портовый город стабилизирует этот участок пространства и жить здесь в дальнейшем будет безопасно. Чтобы пойти по этому пути, нужно сначала построить док третьего уровня с соответствующей инфраструктурой.";
                                    q.steps[0] = "Количество доков (любого уровня) ";
                                    q.steps[1] = "Док третьего уровня построен ";
                                    q.steps[2] = "Количество блоков склада ";
                                    break;
                                case EndgameQuestID.Endgame_TransportHub_step2:
                                    q.name = "Создание транспортного узла - шаг 2";
                                    q.description = "Для управления транспортным узлом потребуются соответствующие службы и оборудование. Постройте Контрольный центр и Башню Сообщения.";
                                    q.steps[0] = GetStructureName(Structure.CONTROL_CENTER_6_ID) + " построен ";
                                    q.steps[1] = GetStructureName(Structure.CONNECT_TOWER_6_ID) + " построена ";
                                    break;
                                case EndgameQuestID.Endgame_TransportHub_step3:
                                    q.name = "Создание транспортного узла - завершение";
                                    q.description = "В завершении, ваш город должен быть в состоянии принять множество людей. Постройте жилые шпили и один блок отеля. Также, не стоит забывать и про энергоснабжение, поэтому не лишним будет построить блок реактора.";
                                    q.steps[0] = "Блок реактора построен ";
                                    q.steps[1] = "Жилые шпили построены ";
                                    q.steps[2] = "Блок отеля построен ";
                                    break;
                            }
                            break;
                        default: return;
                    }
                }
                break;
            case Language.English:
            default:
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
                                    q.description = "Establish your colony's state by constructing two docks. Keep in mind, that docks's fly corridors must not crossing over to be functional.";
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
                                    q.description = "Your colony needs new members to advance. Use colonization panel in docks to bring new citizens to your island.";
                                    q.steps[0] = "Colonists arrived ";
                                    break;
                                case ProgressQuestID.Progress_OreRefiner:
                                    q.name = "Ore refining";
                                    q.description = "Digging left a lot of stone, still contains ores inside. Build an Ore Enricher, to extract maximum resources.";
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
                                    q.name = "Experimental prognosis <in development>";
                                    q.description = "The Last Sector is an unpredictable place. Organise your own meteorologist team to foresee threats";
                                    q.steps[0] = "Build " + GetStructureName(Structure.XSTATION_3_ID);
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
                                    q.name = "Brave explorers <in development>";
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
                                    q.name = "Complex factory <in development>";
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
                                default: return;
                            }
                            break;
                        case QuestType.Endgame:
                            switch ((EndgameQuestID)q.subIndex)
                            {
                                case EndgameQuestID.Endgame_TransportHub_step1:
                                    q.name = "Transport Hub way 1 - start";
                                    q.description = "Stabling your city as solid port make it's future certain and life will be safe here. Chosing this path requires third-stage dock and attendant infrastructure first.";
                                    q.steps[0] = "Docks count (any level) ";
                                    q.steps[1] = "Third stage dock built ";
                                    q.steps[2] = "Storage blocks count ";
                                    break;
                                case EndgameQuestID.Endgame_TransportHub_step2:
                                    q.name = "Transport Hub way 2";
                                    q.description = "Second step means your port needs strict management and reliable connection hardware. Build the Connection Tower and a control center.";
                                    q.steps[0] = "Control center built ";
                                    q.steps[1] = "Connect Tower built ";
                                    break;
                                case EndgameQuestID.Endgame_TransportHub_step3:
                                    q.name = "Transport Hub way 3 - final";
                                    q.description = "At the end, you city must be prepared to place lot of people. Build new housing skyscrapers and at least one hotel complex. Also you shall care about power supply, so it will be good decision to built a reactor block.";
                                    q.steps[0] = "Reactor block built ";
                                    q.steps[1] = "Housing spires built ";
                                    q.steps[2] = "Hotel block built ";
                                    break;
                                default: return;
                            }
                            break;
                        default: return;
                    }
                }
                break;
        }       
    }
    #endregion

    #region map points
    public static string GetMapPointTitle(MapMarkerType mmtype)
    {
        switch (currentLanguage)
        {
            case Language.Russian:
                switch (mmtype)
                {
                    case MapMarkerType.MyCity: return "Мой город";
                    case MapMarkerType.Station: return "Станция";
                    case MapMarkerType.Wreck: return "Обломки";
                    case MapMarkerType.Shuttle: return "Челнок";
                    case MapMarkerType.Island: return "Остров";
                    case MapMarkerType.SOS: return "Запрос о помощи";
                    case MapMarkerType.Portal: return "Портал";
                    case MapMarkerType.QuestMark: return "Отметка задания"; //заменить
                    case MapMarkerType.Colony: return "Другая колония";
                    case MapMarkerType.Star: return "Звезда";
                    case MapMarkerType.Wiseman: return "Источник знания";
                    case MapMarkerType.Wonder: return "Что-то непостижимое";
                    case MapMarkerType.Resources: return "Ресурсы";
                    case MapMarkerType.Unknown:
                    default: return "Неизвестно";
                }
            case Language.English:
            default:
                switch (mmtype)
                {
                    case MapMarkerType.MyCity: return "My city";
                    case MapMarkerType.Station: return "Station";
                    case MapMarkerType.Wreck: return "Wreck";
                    case MapMarkerType.Shuttle: return "Shuttle";
                    case MapMarkerType.Island: return "Island";
                    case MapMarkerType.SOS: return "Someone needs help";
                    case MapMarkerType.Portal: return "Portal";
                    case MapMarkerType.QuestMark: return "Quest mark"; //заменить
                    case MapMarkerType.Colony: return "Another colony";
                    case MapMarkerType.Star: return "Star";
                    case MapMarkerType.Wiseman: return "Source of wisdom";
                    case MapMarkerType.Wonder: return "Something inconceivable";
                    case MapMarkerType.Resources: return "Drifting resources";
                    case MapMarkerType.Unknown:
                    default: return "Unknown point";
                }
        }

    }
    public static string GetMapPointDescription(MapMarkerType mmtype, byte subIndex)
    {
        switch (currentLanguage)
        {
            case Language.Russian:
                return string.Empty;
            case Language.English:
            default:
                {
                    switch (mmtype)
                    {
                        case MapMarkerType.MyCity: return "Our stable piece of dream-reality.";
                        case MapMarkerType.Station: return "Lonely station, which has connection with real space out there.";
                        case MapMarkerType.Wreck:
                            {
                                switch (subIndex)
                                {
                                    case 3: return "Broken refugee's ship. No visible signs of life.";
                                    case 2: return "Parts of something big and made from metal dropped everywhere there";
                                    case 1: return "It painted with pain and sorrow. Broken ship from outside.";
                                    case 0:
                                    default:
                                        return "Something broken to pieces.";
                                }
                            }
                        case MapMarkerType.Shuttle: return "Our colony's shuttle."; // заменить
                        case MapMarkerType.Island: return "An uninhabitated island.";
                        case MapMarkerType.SOS: return "Maybe we should send someone?";
                        case MapMarkerType.Portal: return "Temporary way to another world.";
                        case MapMarkerType.QuestMark: return "Your questmission here."; //заменить
                        case MapMarkerType.Colony: return "We are not alone here!";
                        case MapMarkerType.Star: return "Shines.";
                        case MapMarkerType.Wiseman: return "Our explorers can take wisdom here.";
                        case MapMarkerType.Wonder: return "Mighty construction built for unobvious purpose.";
                        case MapMarkerType.Resources: return "We can gain it and use.";
                        case MapMarkerType.Unknown:
                        default:
                            return "It can't be described with words";
                    }
                }
        }
    }
    #endregion

    #region missions data
    public static string GetMissionCodename(MissionType mtype)
    {
        switch (currentLanguage) {
            case Language.Russian:
                switch (mtype)
                {
                    case MissionType.Exploring: return "Изучение";
                    case MissionType.FindingKnowledge: return "В поисках знания";
                    case MissionType.FindingItem: return "Поиск предмета";
                    case MissionType.FindingPerson: return "Поиск человека";
                    case MissionType.FindingPlace: return "Поиск места";
                    case MissionType.FindingResources: return "Поиск ресурсов";
                    case MissionType.FindingEntrance: return "Найти вход";
                    case MissionType.FindingExit: return "Найти дорогу обратно";
                    case MissionType.Awaiting:
                    default:
                        return "Awaiting";
                }
            case Language.English:
            default:
                {
                    switch (mtype)
                    {
                        case MissionType.Exploring: return "Exploring"; 
                        case MissionType.FindingKnowledge: return "Finding knowledge";
                        case MissionType.FindingItem: return "Finding item";
                        case MissionType.FindingPerson: return "Finding person";
                        case MissionType.FindingPlace: return "Finding place";
                        case MissionType.FindingResources: return "Finding resources";
                        case MissionType.FindingEntrance: return "Finding entrance";
                        case MissionType.FindingExit: return "Finding way back";
                        case MissionType.Awaiting:
                        default:
                            return "Awaiting";
                    }
        }
        }
    }
    #endregion
}

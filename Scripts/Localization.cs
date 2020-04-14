using UnityEngine;

public enum Language : ushort { English, Russian }; // menuUI - options preparing
public enum LocalizedWord : ushort
{
     Buy, Cancel, Close, Crew, Dig, Expedition, Launch, Level, Mission, Offline, Owner, Pass, Progress, Repair, Roll, Sell,Stability,Stamina, Step, Upgrade, UpgradeCost, Limitation, Demand, Price, Trading, Gather, Colonization, Normal, Improved, Lowered, Dismiss, Disassemble, Total,
    Save, Load, Options, Exit, Build, Shuttles, Crews, Reward, Delete, Rewrite, Yes, MainMenu, Accept, PourIn, Year_short, Month_short, Day_short, Day, Score, Disabled, Land_verb, Editor, Highscores, Generate, Size,
    Difficulty, Start, Language, Quality, Apply, Continue, Menu, Stop, Play, Info, Goals, Refuse, Return,
    Persistence, SurvivalSkills, Perception, SecretKnowledge, Intelligence, TechSkills
};

public enum LocalizedPhrase : ushort
{
    AddBuilding,ArtifactNotResearched, AscensionLevel, AffectionTypeNotMatch, ClearSlot,ConnectionOK, ConnectionLost, ConvertToBlock, CrewFoundArtifact,CrystalsCollected, GoOnATrip, KnowledgePoints, MembersCount, NoCrews, NoExpeditions, NoSuitableArtifacts, NoSuitableShuttles, NoShuttles, NotEnoughEnergySupply, PressToTurnOn, RecallExpedition, StopDig, StopGather,SuppliesLeft, UnoccupiedTransmitters, RequiredSurface,
    ColonizationEnabled, ColonizationDisabled, TicketsLeft, ColonistsArrived, PointsSec, PerSecond, BirthrateMode,
     NoActivity, NoArtifact, NoArtifacts, CrewSlots, NoFreeSlots, NotResearched, HireNewCrew, NoCrew, ConstructShuttle, ShuttleConstructed, ShuttleReady, ShuttleOnMission, NoShuttle, ObjectsLeft, NoSavesFound, CreateNewSave, LODdistance, GraphicQuality, Ask_DestroyIntersectingBuildings,
    MakeSurface, BufferOverflow, NoEnergySupply, PowerFailure, NoMission, NoHighscores, NoTransmitters, AddCrew, NewGame, UsePresets, GenerationType, NoLimit, UpperLimit, IterationsCount, ChangeSurfaceMaterial, CreateColumn, CreateBlock,
    AddPlatform, OpenMap, OpenResearchTab, FreeAttributePoints, YouAreHere, SendExpedition, FreeTransmitters, FreeShuttles, FuelNeeded, OpenExpeditionWindow, StopMission, NoSuitableParts
}
public enum LocalizationActionLabels : ushort
{
    Extracted, WorkStopped, BlockCompleted, MineLevelFinished, CleanInProgress, DigInProgress, GatherInProgress, PouringInProgress,
    FlyingToMissionPoint, FlyingHome, Dissmissed, TryingToLeave
}
public enum GameAnnouncements : ushort
{
    NotEnoughResources, NotEnoughEnergyCrystals, GameSaved, GameLoaded, SavingFailed, LoadingFailed, NewQuestAvailable, GamePaused,
    GameUnpaused, StorageOverloaded, ActionError, ShipArrived, NotEnoughFood, SetLandingPoint, IslandCollapsing, NewObjectFound, CrewsLimitReached
};
public enum LocalizedTutorialHint : byte { Landing }
public enum RestrictionKey : ushort { SideConstruction, UnacceptableSurfaceMaterial, HeightBlocked }
public enum RefusalReason : ushort { Unavailable, MaxLevel, HQ_RR1, HQ_RR2, HQ_RR3, HQ_RR4, HQ_RR5, HQ_RR6, SpaceAboveBlocked, NoBlockBelow, NotEnoughSlots, WorkNotFinished, MustBeBuildedOnFoundationBlock, NoEmptySpace, AlreadyBuilt, UnacceptableHeight}
public enum ExpeditionComposingErrors : byte { ShuttleUnavailable, CrewUnavailable, NotEnoughFuel}
public enum LocalizedCrewAction : byte { CannotCompleteMission, LeaveUs, CrewTaskCompleted, CannotReachDestination }

public static class Localization
{
    public static Language currentLanguage { get; private set; }

    static Localization()
    {
        int x = 0;
        if (PlayerPrefs.HasKey(GameConstants.BASE_SETTINGS_PLAYERPREF))
        {
            x = PlayerPrefs.GetInt(GameConstants.BASE_SETTINGS_PLAYERPREF);
        }
        if ((x & 1) == 0) ChangeLanguage(Language.English); // default language
        else ChangeLanguage(Language.Russian);
    }

    public static void ChangeLanguage(Language lan)
    {
        currentLanguage = lan;
    }

    public static string GetStructureName(int id)
    {
        switch (currentLanguage)
        {
            case Language.Russian:
                switch (id)
                {
                    case Structure.PLANT_ID: return "Растение";
                    case Structure.STORAGE_0_ID: return "Основной склад";
                    case Structure.STORAGE_1_ID: 
                    case Structure.STORAGE_2_ID: return "Склад";
                    case Structure.STORAGE_BLOCK_ID: return "Блок склада";
                    case Structure.CONTAINER_ID: return "Контейнер";
                    case Structure.MINE_ELEVATOR_ID: return "Подъёмник шахты";
                    case Structure.LIFESTONE_ID: return "Камень жизни";
                    case Structure.TENT_ID: return "Палатка";
                    case Structure.HOUSE_BLOCK_ID: return "Жилой блок";
                    case Structure.DOCK_ID: return "Док";
                    case Structure.DOCK_2_ID: return "Улучшенный док";
                    case Structure.DOCK_3_ID: return "Продвинутый док";
                    case Structure.ENERGY_CAPACITOR_1_ID:
                    case Structure.ENERGY_CAPACITOR_2_ID:  return "Аккумулятор";
                    case Structure.FARM_1_ID:
                    case Structure.FARM_2_ID:
                    case Structure.FARM_3_ID: return "Ферма";
                    case Structure.COVERED_FARM: return "Ферма закрытого типа ";
                    case Structure.FARM_BLOCK_ID: return "Блок фермы";
                    case Structure.HEADQUARTERS_ID: return "Администрация";
                    case Structure.LUMBERMILL_1_ID:
                    case Structure.LUMBERMILL_2_ID:
                    case Structure.LUMBERMILL_3_ID: return "Лесопилка";
                    case Structure.COVERED_LUMBERMILL: return "Лесопилка закрытого типа";
                    case Structure.LUMBERMILL_BLOCK_ID: return "Блок лесопилки";
                    case Structure.MINE_ID: return "Шахта";
                    case Structure.SMELTERY_1_ID:
                    case Structure.SMELTERY_2_ID:
                    case Structure.SMELTERY_3_ID: return "Плавильня";
                    case Structure.SMELTERY_BLOCK_ID: return "Плавильный блок";
                    case Structure.WIND_GENERATOR_1_ID: return "Потоковый генератор";
                    case Structure.BIOGENERATOR_2_ID: return "Биореактор";
                    case Structure.HOSPITAL_2_ID: return "Клиника";
                    case Structure.MINERAL_POWERPLANT_2_ID: return "Электростанция на минерале F";
                    case Structure.ORE_ENRICHER_2_ID: return "Обогатитель руды";
                    case Structure.WORKSHOP_ID: return "Мастерская";
                    case Structure.MINI_GRPH_REACTOR_3_ID: return "Малый графониевый реактор";
                    case Structure.FUEL_FACILITY_ID: return "Топливный завод";
                    case Structure.GRPH_REACTOR_4_ID: return "Графониевый реактор";
                    case Structure.PLASTICS_FACTORY_3_ID: return "Фабрика пластика";
                    case Structure.SUPPLIES_FACTORY_4_ID: return "Фабрика снаряжения";
                    case Structure.SUPPLIES_FACTORY_5_ID: return "Блок фабрики снаряжения";
                    case Structure.GRPH_ENRICHER_3_ID: return "Графониевый обогатитель";
                    case Structure.XSTATION_3_ID: return "Экспериментальная станция";
                    case Structure.QUANTUM_ENERGY_TRANSMITTER_5_ID: return "Квантовый передатчик энергии";
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
                    case Structure.HOTEL_BLOCK_6_ID: return "Блок отеля";
                    case Structure.HOUSING_MAST_6_ID: return "Жилой шпиль";
                    case Structure.DOCK_ADDON_1_ID: return "Пристройка дока - 1";
                    case Structure.DOCK_ADDON_2_ID: return "Пристройка дока - 2";
                    case Structure.OBSERVATORY_ID: return "Обсерватория";
                    case Structure.ARTIFACTS_REPOSITORY_ID: return "Хранилище артефактов";
                    case Structure.MONUMENT_ID: return "Монумент";
                    case Structure.SETTLEMENT_CENTER_ID: return "Центр поселения";
                    case Structure.PSYCHOKINECTIC_GEN_ID: return "Психокинетический генератор";
                    case Structure.SCIENCE_LAB_ID: return "Исследовательская лаборатория";
                    default: return "Неизвестное здание";
                }
            case Language.English:
            default:
                switch (id)
                {
                    case Structure.PLANT_ID: return "Some plant";
                    case Structure.STORAGE_0_ID: return "Primary storage";
                    case Structure.STORAGE_1_ID:
                    case Structure.STORAGE_2_ID: return "Warehouse";
                    case Structure.STORAGE_BLOCK_ID: return "Storage block";
                    case Structure.CONTAINER_ID: return "Container";
                    case Structure.MINE_ELEVATOR_ID: return "Mine elevator";
                    case Structure.LIFESTONE_ID: return "Life stone";
                    case Structure.TENT_ID: return "Tent";
                    case Structure.HOUSE_BLOCK_ID: return "Residential Block";
                    case Structure.DOCK_ID: return "Dock";
                    case Structure.DOCK_2_ID: return "Improved dock";
                    case Structure.DOCK_3_ID: return "Advanced dock";
                    case Structure.ENERGY_CAPACITOR_1_ID:
                    case Structure.ENERGY_CAPACITOR_2_ID: return "Power capacitor";
                    case Structure.FARM_1_ID: return "Farm (lvl 1)";
                    case Structure.FARM_2_ID: return "Farm (lvl 2)";
                    case Structure.FARM_3_ID: return "Farm (lvl 3)";
                    case Structure.COVERED_FARM: return "Covered farm ";
                    case Structure.FARM_BLOCK_ID: return "Farm Block ";
                    case Structure.HEADQUARTERS_ID: return "HeadQuarters";
                    case Structure.LUMBERMILL_1_ID: return "Lumbermill";
                    case Structure.LUMBERMILL_2_ID: return "Lumbermill";
                    case Structure.LUMBERMILL_3_ID: return "Lumbermill";
                    case Structure.COVERED_LUMBERMILL: return "Covered lumbermill";
                    case Structure.LUMBERMILL_BLOCK_ID: return "Lumbermill Block";
                    case Structure.MINE_ID: return "Mine Entrance";
                    case Structure.SMELTERY_1_ID: return "Smeltery";
                    case Structure.SMELTERY_2_ID: return "Smeltery";
                    case Structure.SMELTERY_3_ID: return "Smelting Facility";
                    case Structure.SMELTERY_BLOCK_ID: return "Smeltery Block";
                    case Structure.WIND_GENERATOR_1_ID: return "Stream generator";
                    case Structure.BIOGENERATOR_2_ID: return "Biogenerator";
                    case Structure.HOSPITAL_2_ID: return "Hospital";
                    case Structure.MINERAL_POWERPLANT_2_ID: return "Mineral F powerplant";
                    case Structure.ORE_ENRICHER_2_ID: return "Ore enricher";
                    case Structure.WORKSHOP_ID: return "Workshop";
                    case Structure.MINI_GRPH_REACTOR_3_ID: return "Small Graphonum reactor";
                    case Structure.FUEL_FACILITY_ID: return "Fuel facility";
                    case Structure.GRPH_REACTOR_4_ID: return "Graphonium reactor";
                    case Structure.PLASTICS_FACTORY_3_ID: return "Plastics factory";
                    case Structure.SUPPLIES_FACTORY_4_ID: return "Supplies factory";
                    case Structure.SUPPLIES_FACTORY_5_ID: return "Supplies factory Block";
                    case Structure.GRPH_ENRICHER_3_ID: return "Graphonium enricher";
                    case Structure.XSTATION_3_ID: return "Experimental station";
                    case Structure.QUANTUM_ENERGY_TRANSMITTER_5_ID: return "Quantum energy transmitter";
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
                    case Structure.HOTEL_BLOCK_6_ID: return "Hotel block";
                    case Structure.HOUSING_MAST_6_ID: return "Housing spire";
                    case Structure.DOCK_ADDON_1_ID: return "Dock addon 1";
                    case Structure.DOCK_ADDON_2_ID: return "Dock addon 2";
                    case Structure.OBSERVATORY_ID: return "Observatory";
                    case Structure.ARTIFACTS_REPOSITORY_ID: return "Artifacts repository";
                    case Structure.MONUMENT_ID: return "Monument";
                    case Structure.SETTLEMENT_CENTER_ID: return "Settlement center";
                    case Structure.PSYCHOKINECTIC_GEN_ID: return "Psychokinetic generator";
                    case Structure.SCIENCE_LAB_ID: return "Research laboratory";
                    default: return "Unknown building";
                }
        }
    }
    public static string GetStructureDescription(int id)
    {
        switch (currentLanguage)
        {
            case Language.Russian:
                switch (id)
                {
                    case Structure.STORAGE_0_ID: return "Стартовое хранилище объёмом " + StorageHouse.GetMaxVolume(0) + " eдиниц.";
                    case Structure.STORAGE_1_ID: return "Небольшое хранилище объёмом " + StorageHouse.GetMaxVolume(1) + " eдиниц.";
                    case Structure.STORAGE_2_ID: return "Хранилище объёмом " + StorageHouse.GetMaxVolume(2) + " eдиниц.";
                    case Structure.STORAGE_BLOCK_ID: return "Блок для хранения объёмом " + StorageHouse.GetMaxVolume(5) + " eдиниц.";
                    case Structure.CONTAINER_ID: return "Содержит ресурсы.";
                    case Structure.LIFESTONE_ID: return "Источает энергию жизни.";
                    case Structure.TENT_ID: return "Временное жильё.";                  
                    case Structure.HOUSE_BLOCK_ID: return "В жилом блоке могут проживать до " + House.GetHousingValue(id) + " человек.";
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
                     return "Запасает до " + Building.GetEnergyCapacity(id) + "единиц энергии. Может конвертировать кристаллы в энергию.";
                    case Structure.FARM_1_ID:
                    case Structure.FARM_2_ID:
                    case Structure.FARM_3_ID: return "Специально подготовленная площадка для выращивания еды. Потребляет жизненную энергию острова. Может быть построена только на грунте.";
                    case Structure.COVERED_FARM:
                    case Structure.FARM_BLOCK_ID: return "Постоянно производит некоторое количество еды. Не потребляет жизненную энергию острова.";
                    case Structure.HEADQUARTERS_ID: return "Главное здание колонии. Производит немного энергии и имеет несколько жилых помещений.";
                    case Structure.LUMBERMILL_1_ID:
                    case Structure.LUMBERMILL_2_ID:
                    case Structure.LUMBERMILL_3_ID: return "Выращивает и рубит деревья. Потребляет жизненную энергию острова.";
                    case Structure.COVERED_LUMBERMILL:
                    case Structure.LUMBERMILL_BLOCK_ID: return "Постоянно производит некоторое количество древесины. Не потребляет жизненную энергию острова.";
                    case Structure.MINE_ID: return "Добыча полезных ископаемых закрытым методом.";
                    case Structure.SMELTERY_1_ID:
                    case Structure.SMELTERY_2_ID:
                    case Structure.SMELTERY_3_ID:
                    case Structure.SMELTERY_BLOCK_ID: return "Перерабатывает ресурсы.";
                    case Structure.WIND_GENERATOR_1_ID: return "Нестабильно вырабатывает энергию в зависимости от силы местных потоков. Лучше располагать как можно выше.";
                    case Structure.BIOGENERATOR_2_ID: return "Вырабатывает энергию, потребляя органическую материю, эффективность зависит от количества рабочих.";
                    case Structure.HOSPITAL_2_ID: return "Обеспечивает колонию медицинской помощью. Может регулировать темп появления новых жителей.";
                    case Structure.MINERAL_POWERPLANT_2_ID: return "Вырабатывает энергию, потребляя минерал F. Выход энергии зависит от количества рабочих.";
                    case Structure.ORE_ENRICHER_2_ID: return "Позволяет добывать нужные руды из обычной горной породы.";
                    case Structure.WORKSHOP_ID: return "Улучшает или поддерживает в норме оборудование колонистов.";
                    case Structure.MINI_GRPH_REACTOR_3_ID: return "Бесконечный источник энергии, не требует рабочих.";
                    case Structure.FUEL_FACILITY_ID: return "Производит топливо для кораблей";
                    case Structure.REACTOR_BLOCK_5_ID:
                    case Structure.GRPH_REACTOR_4_ID: return "Вырабатывает большое количество энергии, потребляя Графониум. Выход энергии зависит от количества рабочих";
                    case Structure.PLASTICS_FACTORY_3_ID: return "Специализированная фабрика для производства пластика.";
                    case Structure.SUPPLIES_FACTORY_4_ID:
                    case Structure.SUPPLIES_FACTORY_5_ID: return "Производит снаряжение для экспедиций и нужд колонии.";
                    case Structure.GRPH_ENRICHER_3_ID: return "Обогащает N-метал до Графония.";
                    case Structure.QUANTUM_ENERGY_TRANSMITTER_5_ID: return "Конденсирует лишнюю энергию в кристаллы. Может быть построен только одно такое здание!";
                    case Structure.SWITCH_TOWER_ID: return "При выделении включает срез слоя, на котором находится.";
                    case Structure.SHUTTLE_HANGAR_4_ID: return "Вмещает и обслуживает один челнок. Для корректной работы должен иметь свободный коридор до конца карты шириной в 1 блок.";
                    case Structure.RECRUITING_CENTER_4_ID: return "Набирает и подготавливает команды исследователей из добровольцев.";
                    case Structure.EXPEDITION_CORPUS_4_ID: return "Центр управления миссиями. Может быть построено только одно такое здание.";
                    case Structure.QUANTUM_TRANSMITTER_4_ID: return "Обеспечивает связь с экспедициями за пределами острова.";
                    case Structure.FOUNDATION_BLOCK_5_ID: return "Cтруктура для обеспечения высокоуровневых зданий";
                    case Structure.DOCK_ADDON_1_ID: return "Стройте вплотную к доку, чтобы улучшить его до уровня 2.";
                    case Structure.DOCK_ADDON_2_ID: return "Стройте вплотную к доку, чтобы улучшить его до уровня 3.";
                    case Structure.HOTEL_BLOCK_6_ID: return "Принимает гостей колонии и приносит прибыль раз в день";
                    case Structure.CONNECT_TOWER_6_ID: return "<В разработке>";
                    case Structure.XSTATION_3_ID: return "Собирает информацию об окружающей среде, измеряет уровень Стабильности, предсказывает надвигающиеся угрозы.";
                    case Structure.OBSERVATORY_ID: return "Отслеживает события в ближайшем пространстве. Должна быть построена на максимальной высоте. В радиусе одного блока не должно быть других блоков и поверхностей. Может быть построена только одна обсерватория.";
                    case Structure.ARTIFACTS_REPOSITORY_ID: return "Даёт доступ к хранилищу найденных артефактов.";
                    case Structure.MONUMENT_ID: return "Активирует и использует до четырёх артефактов. Внимание: перебои с питанием могут привести к повреждению артефактов.";
                    case Structure.SETTLEMENT_CENTER_ID: return "Вокруг центра автоматически начнут появляться дома, которые центр будет снабжать всем необходимым. Его можно улучшать, чтобы начали строиться улучшенные здания, и спонсировать, чтобы мгновенно закончить текущее строительство.";
                    case Structure.PSYCHOKINECTIC_GEN_ID: return "Вырабатывает энергию с помощью человеческих усилий, эффективность зависит от количества рабочих. Не требует топлива, но снижает настроение колонии.";
                    case Structure.SCIENCE_LAB_ID: return "Исследует найденные Закономерности или вырабатывает очки Знания.";
                    default: return "Описание отсутствует";
                }
            case Language.English:
            default:
                switch (id)
                {
                    case Structure.STORAGE_0_ID: return "Start storage building contains " + StorageHouse.GetMaxVolume(0) + " points.";
                    case Structure.STORAGE_1_ID: return "Small storage building contains " + StorageHouse.GetMaxVolume(1) + " points.";
                    case Structure.STORAGE_2_ID: return "This storage can contain " + StorageHouse.GetMaxVolume(2) + " points.";
                    
                    case Structure.STORAGE_BLOCK_ID: return "Storage block can contain " + StorageHouse.GetMaxVolume(5) + " points.";
                    case Structure.CONTAINER_ID: return "Contains resources.";
                    case Structure.LIFESTONE_ID: return "Emits lifepower.";
                    case Structure.TENT_ID: return "Temporary housing.";                   
                    case Structure.HOUSE_BLOCK_ID: return "Residential block can be house for " + House.GetHousingValue(id) + " persons.";
                    case Structure.HOUSING_MAST_6_ID: return "Massive residential complex for " + House.GetHousingValue(id) + " persons";
                    case Structure.DOCK_ID:
                        {
                            string s = Dock.SMALL_SHIPS_PATH_WIDTH.ToString();
                            return "Receives new colonists and trade goods. Needs in " + s + " x " + s + " corridor close to the dock to function.";
                        }
                    case Structure.DOCK_2_ID:
                        {
                            string s = Dock.MEDIUM_SHIPS_PATH_WIDTH.ToString();
                            return "Maintains medium trade vessels.  Needs in " + s + " x " + s + " corridor close to the dock to function.";
                        }
                    case Structure.DOCK_3_ID:
                        {
                            string s = Dock.HEAVY_SHIPS_PATH_WIDTH.ToString();
                            return "Maintains heavy trade vessels.  Needs in " + s + " x " + s + " corridor close to the dock to function.";
                        }
                    case Structure.ENERGY_CAPACITOR_1_ID:
                    case Structure.ENERGY_CAPACITOR_2_ID:
                       return "Store up to " + Building.GetEnergyCapacity(id) + "energy points. Converts energy crystals to energy points.";
                    case Structure.FARM_1_ID:
                    case Structure.FARM_2_ID:
                    case Structure.FARM_3_ID: return "A field prepared for growing up food. Consumes island lifepower. Must be located on dirt.";
                    case Structure.COVERED_FARM:
                    case Structure.FARM_BLOCK_ID: return "Constantly produces food. Doesn't consume lifepower.";
                    case Structure.HEADQUARTERS_ID: return "Colony's main building. Produces small amount of energy and has a small living space.";
                    case Structure.LUMBERMILL_1_ID:
                    case Structure.LUMBERMILL_2_ID:
                    case Structure.LUMBERMILL_3_ID: return "Grows and cuts trees. Consumes island lifepower.";
                    case Structure.COVERED_LUMBERMILL:
                    case Structure.LUMBERMILL_BLOCK_ID: return "Constantly produces wood. Doesn't consume lifepower.";
                    case Structure.MINE_ID: return "Extracts fossils in closed way.";
                    case Structure.SMELTERY_1_ID:
                    case Structure.SMELTERY_2_ID:
                    case Structure.SMELTERY_3_ID:
                    case Structure.SMELTERY_BLOCK_ID: return "Process resources.";
                    case Structure.WIND_GENERATOR_1_ID: return "Generates energy in dependence of local streams. Build it as high as possible.";
                    case Structure.BIOGENERATOR_2_ID: return "Generates energy from organic. Energy production depends on the number of workers.";
                    case Structure.HOSPITAL_2_ID: return "Supplies colony with healthcare. Can control the spawnrate.";
                    case Structure.MINERAL_POWERPLANT_2_ID: return "Generates energy from mineral F. Energy production depends on the number of workers.";
                    case Structure.ORE_ENRICHER_2_ID: return "Extracts ores from stone.";
                    case Structure.WORKSHOP_ID: return "Improves or stabilizes colonist's gears.";
                    case Structure.MINI_GRPH_REACTOR_3_ID: return "Generates energy, consumes nothing. No need in workers.";
                    case Structure.FUEL_FACILITY_ID: return "Produces fuel for vessels.";
                    case Structure.REACTOR_BLOCK_5_ID:
                    case Structure.GRPH_REACTOR_4_ID: return "Generates a lot of energy, consumes Graphonium. Energy production depends on the number of workers.";
                    case Structure.PLASTICS_FACTORY_3_ID: return "Factory specialized on plastics producing.";
                    case Structure.SUPPLIES_FACTORY_4_ID:
                    case Structure.SUPPLIES_FACTORY_5_ID: return "Produces supplies for expeditions and colony needs.";
                    case Structure.GRPH_ENRICHER_3_ID: return "Transforms N-metal into Graphonium.";
                    case Structure.QUANTUM_ENERGY_TRANSMITTER_5_ID: return "Transforms energy excess in energy crystals. It can be only one building of this type!";
                    case Structure.SWITCH_TOWER_ID: return "Activates layer cut on its own height when selected.";
                    case Structure.SHUTTLE_HANGAR_4_ID: return "Holds and maintains one shuttle. Hangar needs one-block wide flight corridor to the edge of the map to work properly";
                    case Structure.RECRUITING_CENTER_4_ID: return "Recruits and trains exploring teams.";
                    case Structure.EXPEDITION_CORPUS_4_ID: return "Controls missions activities.";
                    case Structure.QUANTUM_TRANSMITTER_4_ID: return "Provides connection with expeditions outside the island.";
                    case Structure.FOUNDATION_BLOCK_5_ID: return "A structure for high-level structures support.";
                    case Structure.DOCK_ADDON_1_ID: return "Build it next to dock to up it to level 2.";
                    case Structure.DOCK_ADDON_2_ID: return "Build it next to dock to up it to level 3.";
                    case Structure.HOTEL_BLOCK_6_ID: return "Houses guests and pays earnings everyday.";
                    case Structure.CONNECT_TOWER_6_ID: return "<In development>";
                    case Structure.XSTATION_3_ID: return "Gathering environmental intelligence, measures Stability level, predicting threats.";
                    case Structure.OBSERVATORY_ID: return "Observing near space for events. Must have empty space in 1 block radius, and be built on maximum height. Only one observatory can be built.";
                    case Structure.ARTIFACTS_REPOSITORY_ID: return "Gives access to your non-using artifacts";
                    case Structure.MONUMENT_ID: return "Activates and utilizes artifacts. Be careful : switching power supply may hurt artifacts.";
                    case Structure.SETTLEMENT_CENTER_ID: return "Automatically builds houses. Can be upgraded for creating advanced houses or sponsored to finish current construction immediately.";
                    case Structure.PSYCHOKINECTIC_GEN_ID: return "Produces energy through colonists effort, energy production depends on the number of workers. Does not require fuel, but lowers colony's happiness.";
                    case Structure.SCIENCE_LAB_ID: return "Investigates Regularities found or generates Knowledge points.";
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
        switch (currentLanguage)
        {
            case Language.Russian:
                {
                    switch (id)
                    {
                        case 0: return string.Empty;
                        case ResourceType.DIRT_ID: return "Органическое покрытие острова.";
                        case ResourceType.FOOD_ID: return "Топливо для живущих.";
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

    public static string GetAnnouncementString(GameAnnouncements announce)
    {
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
                        case GameAnnouncements.CrewsLimitReached: return "Достигнут предел количества команд";
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
                        case GameAnnouncements.CrewsLimitReached: return "Crews limit reached";
                        default: return "<announcement not found>";
                    }
                }
        }

    }
    public static string GetRestrictionPhrase(RestrictionKey rkey)
    {
        switch (rkey)
        {
            default: return "Action not possible";
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

    public static string CostInCoins(float count)
    {
        switch (currentLanguage)
        {
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
    public static string AnnounceQuestCompleted(string name)
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

    public static string GetExpeditionName(Expedition e)
    {
        if (e == null) return string.Empty;
        else
        {
            switch (currentLanguage)
            {
                case Language.Russian:
                    {
                        if (e.destination != null) return "Экспедиция к " + e.destination.GetName();
                        else
                        {
                            switch (e.crew.exploringPath)
                            {
                                case Path.LifePath: return "К новым горизонтам!";
                                case Path.SecretPath: return "Путешествие в неизведанное!";
                                case Path.TechPath: return "За пределы пространства!";
                                default: return "Expedition to nowhere";
                            }
                        }
                    }
                case Language.English:
                default:
                    {
                        if (e.destination != null) return "Expedition to " + e.destination.GetName();
                        else
                        {
                            switch (e.crew.exploringPath)
                            {
                                case Path.LifePath: return "To new horizons!";
                                case Path.SecretPath: return "Journey to the unknown!";
                                case Path.TechPath: return "Out of the bounds!";
                                default: return "Expedition to nowhere";
                            }
                        }                        
                    }
            }
        }
    }
    public static string GetExpeditionName(PointOfInterest poi)
    {
        if (poi == null) return string.Empty;
        else
        {
            switch (currentLanguage)
            {
                case Language.Russian: return "Экспедиция к " + poi.GetName();
                case Language.English:
                default:
                    return "Expedition to " + poi.GetName();
            }
        }
    }
    public static string GetExpeditionDescription(Expedition e)
    {
        string s;
        switch (currentLanguage)
        {
            case Language.Russian:
                {
                    s = "Участников: " + e.crew.membersCount.ToString() +
                        "\nУсталость: " + ((int)((1f - e.crew.stamina) * 100f)).ToString() +
                        "%\nСтатус: ";
                    switch (e.stage)
                    {
                        case Expedition.ExpeditionStage.LeavingMission: s += "Завершают миссию"; break;
                        case Expedition.ExpeditionStage.OnMission: s += "На задании"; break;
                        case Expedition.ExpeditionStage.WayIn: s += "В пути"; break;
                        case Expedition.ExpeditionStage.WayOut: s += "Возвращаются"; break;
                        default: s += "Отдыхают"; break;
                    }
                    break;
                }
            case Language.English:
            default:
                {
                    s = "Members: " + e.crew.membersCount.ToString() +
                        "\nWeariness: " + ((int)((1f - e.crew.stamina) * 100f)).ToString() +
                        "%\nStatus: ";
                    switch (e.stage)
                    {
                        case Expedition.ExpeditionStage.LeavingMission: s += "Leaving mission"; break;
                        case Expedition.ExpeditionStage.OnMission: s += "On mission"; break;
                        case Expedition.ExpeditionStage.WayIn: s += "Advancing"; break;
                        case Expedition.ExpeditionStage.WayOut: s += "Returning"; break;
                        default: s += "Resting"; break;
                    }
                    break;
                }
        }
        return s;
    }

    public static string GetCrewInfo(Crew c)
    {
        if (c == null) return string.Empty;
        var a = c;
        // CREW_INFO_STRINS_COUNT = 14
        switch (currentLanguage)
        {
            case Language.Russian:
                {                    
                    string s = 
         "Участие в миссиях: " + c.missionsParticipated.ToString() + "\n"
        + "Успешные миссии: " + c.missionsSuccessed.ToString();
                    return s;
                }
            case Language.English:
            default:
                {
                    string s =
         "Missions participated: " + c.missionsParticipated.ToString() + "\n"
        + "Missions successed: " + c.missionsSuccessed.ToString();
                    return s;
                }
        }
    }
    public static string GetCrewAction(LocalizedCrewAction ca, Crew c)
    {
        switch (currentLanguage)
        {
            case Language.English:
                {
                    switch (ca)
                    {
                        case LocalizedCrewAction.CannotCompleteMission: return '"' + c.name + "\" cannot complete mission and therefore return.";
                        case LocalizedCrewAction.LeaveUs: return '"' + c.name + "\" leave us.";
                        case LocalizedCrewAction.CrewTaskCompleted: return '"' + c.name + "\" completed their task";
                        case LocalizedCrewAction.CannotReachDestination: return "Crew " + '"' + c.name + "\" cannot reach destination and therefore returning.";
                        default: return '"' + c.name + "\" is da best.";
                    }
                }
            case Language.Russian:
                switch (ca)
                {
                    case LocalizedCrewAction.CannotCompleteMission: return '"' + c.name + "\" не могут завершить миссию и возвращаются.";
                    case LocalizedCrewAction.LeaveUs: return '"' + c.name + "\" покинули нас.";
                    case LocalizedCrewAction.CrewTaskCompleted: return '"' + c.name + "\" завершили миссию";
                    case LocalizedCrewAction.CannotReachDestination: return "Команда " + '"' + c.name + "\" не может достичь цели и возвращается.";
                    default: return '"' + c.name + "\" просто топчег.";
                }
            default: return '"' + c.name + "\"!111 0)0))).";
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
                        case Artifact.ArtifactStatus.UsingInMonument: return "Используется в монументе";
                        case Artifact.ArtifactStatus.OnConservation: return "Законсервирован";
                        case Artifact.ArtifactStatus.Uncontrollable:
                        default: return string.Empty;
                    }
                }
            case Language.English:
            default:
                {
                    switch (status)
                    {
                        case Artifact.ArtifactStatus.Researching: return "Researching";
                        case Artifact.ArtifactStatus.UsingInMonument: return "Using in monument";
                        case Artifact.ArtifactStatus.OnConservation: return "On conservation";
                        case Artifact.ArtifactStatus.Uncontrollable:
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
            case Language.Russian: return "Команда " + Crew.nextID.ToString();
            case Language.English:
            default: return "Сrew " + Crew.nextID.ToString();
        }
    }
    public static string NameArtifact(Artifact a)
    {
        var c = a.GetColor();
        byte r = (byte)(c.r / 0.125f), g = (byte)(c.g / 0.125f), b = (byte)(c.b / 0.125f);
        string s = string.Empty;
        switch (r)
        {
            case 0:
                switch (g)
                {
                    case 0:
                        switch (b)
                        {
                            case 0: s = "Abyssal "; break;
                            case 1: s = "Black "; break;
                            case 2: s = "Waterspace"; break;
                            case 3: s = "Ultradark "; break; // черный ультрамариновый
                            case 4: s = "Naval "; break; // формы морских офицеров?!
                            case 5: s = "Farsea "; break; // дальнего моря
                            case 6: s = "Blue "; break;
                            case 7: s = "Waterstatic "; break;
                            case 8: s = "Depression "; break;
                            default: s = "Quiet "; break;
                        }
                        break;
                    case 1:
                        switch (b)
                        {
                            case 0: s = "Olivissal "; break; // olive + abyss
                            case 1: s = "Forgotten "; break;
                            case 2: s = "Darkblue "; break;
                            case 3: s = "Notpheare "; break; // not sapphire
                            case 4: s = "Eaphere"; break; // enough sapphire
                            case 5: s = "Dustphere"; break; //blue dust + sapphire
                            case 6: s = "Deablue "; break; //bsod + blue
                            case 7: s = "Bersian "; break;//blue + persian
                            case 8: s = "Sea "; break;
                            default: s = "Lost"; break;
                        }
                        break;
                    case 2:
                        switch (b)
                        {
                            case 0: s = "Pinal"; break; // pine + abyssal
                            case 1: s = "Olivian "; break;
                            case 2: s = "Greegle "; break; // green eagle
                            case 3: s = "[DDGO] "; break; //dead indigo
                            case 4: s = "Transported "; break;
                            case 5: s = "Lazurity "; break;
                            case 6: s = "Cobalt"; break;
                            case 7: s = "[JNS] "; break;
                            case 8: s = "Degreed "; break;
                            default: s = "Cold "; break;
                        }
                        break;
                    case 3:
                        switch (b)
                        {
                            case 0: s = "Aborres "; break; // abyssal forest
                            case 1: s = "Marchio"; break; //marshes
                            case 2: s = "Dartmonth "; break;
                            case 3: s = "Lazyon "; break; // lazur + cyan
                            case 4: s = "Watered "; break;
                            case 5: s = "Capreed "; break;
                            case 6: s = "[CLN] "; break;
                            case 7: s = "[GB7] "; break;
                            case 8: s = "Defendo "; break;
                            default: s = "Ice "; break;
                        }
                        break;
                    case 4:
                        switch (b)
                        {
                            case 0: s = "Virido "; break; //green
                            case 1: s = "Harlequino "; break;
                            case 2: s = "Mayan "; break;
                            case 3: s = "Opalo "; break;
                            case 4: s = "[TROCK] "; break;
                            case 5: s = "Bondi "; break;
                            case 6: s = "Bluehound "; break;
                            case 7: s = "[GB11] "; break;
                            case 8: s = "Electric "; break;
                            default: s = "Power"; break;
                        }
                        break;
                    case 5:
                        switch (b)
                        {
                            case 0: s = "Toaden "; break;
                            case 1: s = "Moslin "; break;
                            case 2: s = "Pigmented "; break;
                            case 3: s = "Threelined "; break;
                            case 4: s = "Shiny"; break;
                            case 5: s = "[GB160] "; break;
                            case 6: s = "[LK] "; break;
                            case 7: s = "Pacific "; break;
                            case 8: s = "Aqua "; break;
                            default: s = "Drown"; break;
                        }
                        break;
                    case 6:
                        switch (b)
                        {
                            case 0: s = "Jungle "; break;
                            case 1: s = "Melchite "; break;
                            case 2: s = "Screaming "; break;
                            case 3: s = "Aphrite"; break;
                            case 4: s = "Ephrite"; break;
                            case 5: s = "Shephrite"; break; // shiny
                            case 6: s = "Ousel "; break;
                            case 7: s = "Seasel "; break; //ousel + sea
                            case 8: s = "Blizzy "; break;
                            default: s = "Winter"; break;
                        }
                        break;
                    case 7:
                        switch (b)
                        {
                            case 0: s = "Limy "; break;
                            case 1: s = "Melyme "; break;
                            case 2: s = "Spreaning "; break;
                            case 3: s = "Springy "; break;
                            case 4: s = "Gemperate "; break; // temperate green
                            case 5: s = "Saladian "; break;
                            case 6: s = "Lightsea "; break;
                            case 7: s = "Searain "; break;
                            case 8: s = "Elcyan "; break;
                            default: s = "Wave "; break;
                        }
                        break;
                    case 8:
                        switch (b)
                        {
                            case 0: s = "Lime"; break;
                            case 1: s = "Lige "; break; // lime + green
                            case 2: s = "Chromakey "; break;
                            case 3: s = "Temperacio "; break;
                            case 4: s = "Folian "; break; // folia
                            case 5: s = "[B160] "; break;
                            case 6: s = "Insomnia "; break;
                            case 7: s = "Turqie "; break;
                            case 8: s = "Cyan "; break;
                            default: s = "Energy"; break;
                        }
                        break;
                    default: s = "Out-of-red "; break;
                }
                break;
            case 1:
                switch (g)
                {
                    case 0:
                        switch (b)
                        {
                            case 0: s = "Cabaret"; break;
                            case 1: s = "Black purpur "; break;
                            case 2: s = "Horrorific "; break;
                            case 3: s = "Prendigo "; break; //persian idigo
                            case 4: s = "Bluestone "; break;
                            case 5: s = "Seastone "; break;
                            case 6: s = "Cobaline "; break;
                            case 7: s = "Hopeless "; break;
                            case 8: s = "Depth "; break;
                            default: s = "Falling"; break;
                        }
                        break;
                    case 1:
                        switch (b)
                        {
                            case 0: s = "Frabyss "; break; //fire abyss
                            case 1: s = "[SIGN32] "; break;
                            case 2: s = "Nightsky "; break;
                            case 3: s = "Nightfear "; break;
                            case 4: s = "Nightphire "; break; // sapphire
                            case 5: s = "Cobalamine "; break;
                            case 6: s = "Cobbleblue "; break;
                            case 7: s = "Secretsign "; break; // shadows in the moonlight
                            case 8: s = "Watermean "; break;
                            default: s = "Secret "; break;
                        }
                        break;
                    case 2:
                        switch (b)
                        {
                            case 0: s = "Virabyss "; break; // virido abyss
                            case 1: s = "Mythilien "; break; ///myrt
                            case 2: s = "[GBS] "; break;
                            case 3: s = "Midnight "; break;
                            case 4: s = "Blacksean "; break;
                            case 5: s = "Cobaliet "; break;
                            case 6: s = "Beroyal "; break; // royal blue
                            case 7: s = "Mafestic "; break;
                            case 8: s = "Hydra "; break;
                            default: s = "Betrayal"; break;
                        }
                        break;
                    case 3:
                        switch (b)
                        {
                            case 0: s = "Evlemon "; break; // evil(very dark) lemon
                            case 1: s = "Mint "; break;
                            case 2: s = "Pineleaf "; break;
                            case 3: s = "Murena "; break;
                            case 4: s = "Capporal "; break;
                            case 5: s = "Blueclain "; break;
                            case 6: s = "Jeanchan "; break; // lol
                            case 7: s = "Lightean "; break;
                            case 8: s = "Fazurity "; break; //far lazure
                            default: s = "Farlander "; break;
                        }
                        break;
                    case 4:
                        switch (b)
                        {
                            case 0: s = "Hindeen "; break; // hindian green
                            case 1: s = "Arboreal "; break;
                            case 2: s = "Emeraldean "; break;
                            case 3: s = "Wetropical "; break;
                            case 4: s = "Lonepine "; break; // lone pine across the sea
                            case 5: s = "Anticyan "; break;
                            case 6: s = "Uncelestial "; break; // not actually celestial
                            case 7: s = "Aqualized "; break;
                            case 8: s = "Seazure "; break;
                            default: s = "Longsail"; break;
                        }
                        break;
                    case 5:
                        switch (b)
                        {
                            case 0: s = "Monk "; break;
                            case 1: s = "Een "; break; //green
                            case 2: s = "Grasshopper "; break;
                            case 3: s = "Signeen "; break; // signal green
                            case 4: s = "Krayfolia "; break; // krayola + threefolia
                            case 5: s = "Perentia "; break; // persian green
                            case 6: s = "Tearock "; break;
                            case 7: s = "Precific "; break; //pacific
                            case 8: s = "Selectric "; break; //eletric sea
                            default: s = "Modulating"; break;
                        }
                        break;
                    case 6:
                        switch (b)
                        {
                            case 0: s = "Hireland "; break; //ireland green
                            case 1: s = "Verdepom "; break; //?
                            case 2: s = "Garden "; break;
                            case 3: s = "Treecover "; break;
                            case 4: s = "Greasin "; break; //green sea
                            case 5: s = "Underwater "; break;
                            case 6: s = "Travelling "; break;
                            case 7: s = "Twea "; break; // twitter
                            case 8: s = "Twizard "; break;
                            default: s = "Celestia "; break;
                        }
                        break;
                    case 7:
                        switch (b)
                        {
                            case 0: s = "Fime "; break; // forest lime;
                            case 1: s = "Coreland "; break;
                            case 2: s = "Itchy "; break;
                            case 3: s = "Olochite "; break;
                            case 4: s = "Caraib "; break;
                            case 5: s = "Aquacaribe "; break;
                            case 6: s = "Berious "; break; // бирюза
                            case 7: s = "Elberious "; break; // light berious
                            case 8: s = "Sky "; break;
                            default: s = "Cloudshifting "; break;
                        }
                        break;
                    case 8:
                        switch (b)
                        {
                            case 0: s = "Chlorokey "; break;
                            case 1: s = "J-flower"; break; // jungle flower
                            case 2: s = "Acid "; break;
                            case 3: s = "Spring "; break;
                            case 4: s = "Tranquility "; break;
                            case 5: s = "Salan "; break;
                            case 6: s = "B-Cloth "; break; // birusa
                            case 7: s = "[FFC] "; break;
                            case 8: s = "Recyan "; break;
                            default: s = "Timered "; break;
                        }
                        break;
                    default: s = "Absorpting "; break;
                }
                break;
            case 2:
                switch (g)
                {
                    case 0:
                        switch (b)
                        {
                            case 0: s = "Ancfire "; break; // ancient fire
                            case 1: s = "Hardlove "; break;
                            case 2: s = "Purple "; break;
                            case 3: s = "Curtain "; break;
                            case 4: s = "Mutato "; break;
                            case 5: s = "Puredigo "; break;//purple indigo
                            case 6: s = "Acril "; break;
                            case 7: s = "Olei "; break;
                            case 8: s = "Bluetour "; break; // blue + 2 red
                            default: s = "Closed "; break;
                        }
                        break;
                    case 1:
                        switch (b)
                        {
                            case 0: s = "Daown "; break; // dark brown
                            case 1: s = "[B-Paste] "; break;
                            case 2: s = "Violet "; break;
                            case 3: s = "Oolet "; break;
                            case 4: s = "Fletch "; break;
                            case 5: s = "[F160] "; break;
                            case 6: s = "Foughn "; break;
                            case 7: s = "F-Blue "; break;
                            case 8: s = "Blues "; break;
                            default: s = "Solution "; break;
                        }
                        break;
                    case 2:
                        switch (b)
                        {
                            case 0: s = "[DPPL] "; break;
                            case 1: s = "Ormitted "; break;
                            case 2: s = "D-Grey "; break;
                            case 3: s = "Afterdusk "; break;
                            case 4: s = "Arctic sky "; break;
                            case 5: s = "Dismay "; break;
                            case 6: s = "Sad "; break;
                            case 7: s = "Ignorance blue "; break;
                            case 8: s = "Bluefill "; break;
                            default: s = "End-of-the-way "; break;
                        }
                        break;
                    case 3:
                        switch (b)
                        {
                            case 0: s = "Elivian "; break;
                            case 1: s = "Militarized "; break;
                            case 2: s = "Tayga "; break;
                            case 3: s = "Icewave "; break;
                            case 4: s = "Polar "; break;
                            case 5: s = "Coldwave "; break;
                            case 6: s = "Coldsky "; break;
                            case 7: s = "North "; break;
                            case 8: s = "Full-ice "; break;
                            default: s = "Frost "; break;
                        }
                        break;
                    case 4:
                        switch (b)
                        {
                            case 0: s = "Iguana "; break;
                            case 1: s = "Tree "; break;
                            case 2: s = "Uncertainity "; break;
                            case 3: s = "Picht "; break;
                            case 4: s = "Solesea "; break;
                            case 5: s = "Lonesea "; break;
                            case 6: s = "[BOLD] "; break; // blue of dark and light
                            case 7: s = "Dead ice "; break;
                            case 8: s = "Cold aqua "; break;
                            default: s = "Eternity "; break;
                        }
                        break;
                    case 5:
                        switch (b)
                        {
                            case 0: s = "Leaves "; break;
                            case 1: s = "Leaven "; break;
                            case 2: s = "Elven "; break;
                            case 3: s = "Forester "; break;
                            case 4: s = "El Tyr "; break;
                            case 5: s = "Mountain "; break;
                            case 6: s = "Blue steel "; break;
                            case 7: s = "Flying ice "; break;
                            case 8: s = "Ice covered "; break;
                            default: s = "Clear water "; break;
                        }
                        break;
                    case 6:
                        switch (b)
                        {
                            case 0: s = "Green city "; break;
                            case 1: s = "Lawn "; break;
                            case 2: s = "Oldgreen "; break;
                            case 3: s = "Nepthturne "; break;
                            case 4: s = "Meadows "; break;
                            case 5: s = "Noquamarine "; break;
                            case 6: s = "Oory "; break;
                            case 7: s = "Blugenkampf "; break;
                            case 8: s = "Aelytter "; break;
                            default: s = "Precious "; break;
                        }
                        break;
                    case 7:
                        switch (b)
                        {
                            case 0: s = "Tropical "; break;
                            case 1: s = "Grasslungs "; break;
                            case 2: s = "Greensilk "; break;
                            case 3: s = "Oretree "; break;
                            case 4: s = "Dreaming grass "; break;
                            case 5: s = "Watergrass "; break;
                            case 6: s = "Calm "; break;
                            case 7: s = "Epacila "; break;
                            case 8: s = "Lightblue "; break;
                            default: s = "Winterbreath "; break;
                        }
                        break;
                    case 8:
                        switch (b)
                        {
                            case 0: s = "Extralime "; break;
                            case 1: s = "Extragrass "; break;
                            case 2: s = "Grassy sky"; break;
                            case 3: s = "Youngspring "; break;
                            case 4: s = "Salantian "; break;
                            case 5: s = "E salantero "; break;
                            case 6: s = "Nettocyan "; break;
                            case 7: s = "Electrocyan "; break;
                            case 8: s = "Dreamlands "; break;
                            default: s = "Okey "; break;
                        }
                        break;
                    default: s = "Temporal "; break;
                }
                break;
            case 3:
                switch (g)
                {
                    case 0:
                        switch (b)
                        {
                            case 0: s = "Loddark "; break; // blood dark
                            case 1: s = "Far-space candy "; break;
                            case 2: s = "Piolet "; break; //purple violet
                            case 3: s = "Plumy "; break;
                            case 4: s = "Suplumy "; break;
                            case 5: s = "Inviolet "; break;
                            case 6: s = "Purpellion "; break;
                            case 7: s = "Untouchable "; break;
                            case 8: s = "Slightly visible "; break;
                            default: s = "Clothtouch "; break;
                        }
                        break;
                    case 1:
                        switch (b)
                        {
                            case 0: s = "Clockwork "; break; // orange
                            case 1: s = "Oxyred "; break;
                            case 2: s = "Urple "; break;
                            case 3: s = "Pulveriser "; break;
                            case 4: s = "Propurple "; break;
                            case 5: s = "Slighly dead "; break;
                            case 6: s = "Old gouache "; break;
                            case 7: s = "Irreal sea "; break;
                            case 8: s = "Unreachable shores "; break;
                            default: s = "Nostromo "; break;
                        }
                        break;
                    case 2:
                        switch (b)
                        {
                            case 0: s = "Umbra pencil "; break;
                            case 1: s = "Corryvean "; break;
                            case 2: s = "Stonemind "; break;
                            case 3: s = "Puregain "; break;
                            case 4: s = "Alopurple "; break; // also purple
                            case 5: s = "Notagain "; break;
                            case 6: s = "Difficult "; break;
                            case 7: s = "Patiency "; break;
                            case 8: s = "Final "; break;
                            default: s = "Superbad "; break; // i prefer ultraviolence howewer
                        }
                        break;
                    case 3:
                        switch (b)
                        {
                            case 0: s = "Insidious olive "; break;
                            case 1: s = "Olive hunters "; break;
                            case 2: s = "Obscure grey "; break;
                            case 3: s = "[GRAY-96] "; break;
                            case 4: s = "Nowever "; break;
                            case 5: s = "Weatherman "; break;
                            case 6: s = "[GREY-U] "; break;
                            case 7: s = "Purple shark "; break;
                            case 8: s = "Seasnake "; break;
                            default: s = "Au revoir "; break;
                        }
                        break;
                    case 4:
                        switch (b)
                        {
                            case 0: s = "Clivian "; break;
                            case 1: s = "Grelivian "; break;
                            case 2: s = "Protected "; break;
                            case 3: s = "Defending "; break;
                            case 4: s = "Fortified "; break;
                            case 5: s = "Iron wave "; break;
                            case 6: s = "Steel sea "; break;
                            case 7: s = "Silent sea "; break;
                            default: s = "Sky "; break;
                        }
                        break;
                    case 5:
                        switch (b)
                        {
                            case 0: s = "Vivian "; break;
                            case 1: s = "Buried "; break;
                            case 2: s = "Masked "; break;
                            case 3: s = "Armed "; break;
                            case 4: s = "Stoic "; break;
                            case 5: s = "Unpleasant "; break;
                            case 6: s = "Sharp edge "; break;
                            case 7: s = "Dolphin "; break;
                            case 8: s = "Clear sky "; break;
                            default: s = "Wind "; break;
                        }
                        break;
                    case 6:
                        switch (b)
                        {
                            case 0: s = "Lizard king "; break;
                            case 1: s = "Snakebyte "; break;
                            case 2: s = "Grasslawn "; break;
                            case 3: s = "Eternity leaf "; break;
                            case 4: s = "Decision "; break;
                            case 5: s = "Tough "; break;
                            case 6: s = "Ship "; break;
                            case 7: s = "Seawolf "; break;
                            case 8: s = "Summersea "; break;
                            default: s = "Aviation "; break;
                        }
                        break;
                    case 7:
                        switch (b)
                        {
                            case 0: s = "Forest maiden "; break;
                            case 1: s = "Driad "; break;
                            case 2: s = "Clover "; break;
                            case 3: s = "Melancholy "; break;
                            case 4: s = "Regressive "; break;
                            case 5: s = "Aquazone "; break;
                            case 6: s = "Seawhale "; break;
                            case 7: s = "Northern "; break;
                            case 8: s = "Skyloud "; break;
                            default: s = "Blue ice "; break;
                        }
                        break;
                    case 8:
                        switch (b)
                        {
                            case 0: s = "Venenum "; break;
                            case 1: s = "Vegetation "; break;
                            case 2: s = "Connate "; break;
                            case 3: s = "Green powder "; break;
                            case 4: s = "Trauma "; break;
                            case 5: s = "Youngsail "; break;
                            case 6: s = "Aquosis "; break;
                            case 7: s = "Cyanosis "; break;
                            case 8: s = "Cyanity "; break;
                            default: s = "Eraser "; break;
                        }
                        break;
                    default: s = "Fast "; break;
                }
                break;
            case 4:
                switch (g)
                {
                    case 0:
                        switch (b)
                        {
                            case 0: s = "Raspberry "; break;
                            case 1: s = "Pinky "; break;
                            case 2: s = "Amygdala "; break;
                            case 3: s = "Purplebottom "; break;
                            case 4: s = "Purple "; break;
                            case 5: s = "Unpurple "; break;
                            case 6: s = "Dark orchidea "; break;
                            case 7: s = "Purple heart "; break;
                            case 8: s = "Purpledove "; break;
                            default: s = "Despair "; break;
                        }
                        break;
                    case 1:
                        switch (b)
                        {
                            case 0: s = "Cinnamon "; break;
                            case 1: s = "Deadsun "; break;
                            case 2: s = "Brownshoe "; break;
                            case 3: s = "Red desert "; break;
                            case 4: s = "Ticketbooth "; break;
                            case 5: s = "Amethyst "; break;
                            case 6: s = "Forgotten "; break;
                            case 7: s = "Compote "; break;
                            case 8: s = "Villain "; break;
                            default: s = "Alien sky "; break;
                        }
                        break;
                    case 2:
                        switch (b)
                        {
                            case 0: s = "Chair "; break;
                            case 1: s = "Powder "; break;
                            case 2: s = "Guilty "; break;
                            case 3: s = "Holoplum"; break;
                            case 4: s = "Violence "; break;
                            case 5: s = "Viorage "; break;
                            case 6: s = "Viaduct "; break;
                            case 7: s = "Unknown "; break;
                            case 8: s = "Mystic "; break;
                            default: s = "Conphagious "; break;
                        }
                        break;
                    case 3:
                        switch (b)
                        {
                            case 0: s = "Stagnated "; break;
                            case 1: s = "Desert dust "; break;
                            case 2: s = "Northwood "; break;
                            case 3: s = "Granite snow "; break;
                            case 4: s = "Isoconcrete "; break;
                            case 5: s = "Endtime"; break;
                            case 6: s = "Version "; break;
                            case 7: s = "Sentencia"; break;
                            case 8: s = "Alien dawn "; break;
                            default: s = "Foreign "; break;
                        }
                        break;
                    case 4:
                        switch (b)
                        {
                            case 0: s = "Untold "; break;
                            case 1: s = "Carrying "; break;
                            case 2: s = "Cavern "; break;
                            case 3: s = "Venus "; break;
                            case 4: s = "Gray "; break;
                            case 5: s = "Turtur "; break;
                            case 6: s = "Sad sky "; break;
                            case 7: s = "Sad ocean "; break;
                            case 8: s = "Morningfreeze "; break;
                            default: s = "Kolotun "; break;
                        }
                        break;
                    case 5:
                        switch (b)
                        {
                            case 0: s = "Froggy "; break;
                            case 1: s = "Toppa "; break;
                            case 2: s = "Chiseled "; break;
                            case 3: s = "Bunkered "; break;
                            case 4: s = "Unnamed "; break;
                            case 5: s = "Guarding "; break;
                            case 6: s = "Breeze "; break;
                            case 7: s = "Skypillar "; break;
                            case 8: s = "Celestial line "; break;
                            default: s = "Windsteam "; break;
                        }
                        break;
                    case 6:
                        switch (b)
                        {
                            case 0: s = "Colorified "; break;
                            case 1: s = "Python "; break;
                            case 2: s = "Grassblade "; break;
                            case 3: s = "Grassmade "; break;
                            case 4: s = "Flowertongue "; break;
                            case 5: s = "Ocean steel "; break;
                            case 6: s = "Steel "; break;
                            case 7: s = "Storm "; break;
                            case 8: s = "Celestore "; break;
                            default: s = "Steel sky "; break;
                        }
                        break;
                    case 7:
                        switch (b)
                        {
                            case 0: s = "Snakemade "; break;
                            case 1: s = "Meadowalker "; break;
                            case 2: s = "Grassshadow "; break;
                            case 3: s = "Greencover "; break;
                            case 4: s = "Little leaf "; break;
                            case 5: s = "Veridomarine "; break;
                            case 6: s = "Noquanox "; break;
                            case 7: s = "Seapicture "; break;
                            case 8: s = "Skylair "; break;
                            default: s = "Dophamine "; break;
                        }
                        break;
                    case 8:
                        switch (b)
                        {
                            case 0: s = "Limonyte "; break;
                            case 1: s = "Colorific "; break;
                            case 2: s = "Flowered "; break;
                            case 3: s = "Springreen "; break;
                            case 4: s = "Green island "; break;
                            case 5: s = "Gaudy "; break;
                            case 6: s = "Laminaria "; break;
                            case 7: s = "Green ice "; break;
                            case 8: s = "Watercyan "; break;
                            default: s = "Waterlife "; break;
                        }
                        break;
                    default: s = "Skyfallen "; break;
                }
                break;
            case 5:
                switch (g)
                {
                    case 0:
                        switch (b)
                        {
                            case 0: s = "Arterial "; break;
                            case 1: s = "Wine "; break;
                            case 2: s = "Impure "; break;
                            case 3: s = "Purpureal "; break;
                            case 4: s = "Qviolet "; break;
                            case 5: s = "Byzanchium "; break;
                            case 6: s = "Orchid "; break;
                            case 7: s = "Nightwind "; break;
                            case 8: s = "Jupiter "; break;
                            default: s = "Occult "; break;
                        }
                        break;
                    case 1:
                        switch (b)
                        {
                            case 0: s = "Tomato "; break;
                            case 1: s = "Carmine "; break;
                            case 2: s = "Amaranto "; break;
                            case 3: s = "Untouchable "; break;
                            case 4: s = "Sviozine "; break;
                            case 5: s = "Vapor "; break;
                            case 6: s = "Strange "; break;
                            case 7: s = "Avion "; break;
                            case 8: s = "Quere "; break;
                            default: s = "Perfection "; break;
                        }
                        break;
                    case 2:
                        switch (b)
                        {
                            case 0: s = "Orange dust "; break;
                            case 1: s = "Lumber "; break;
                            case 2: s = "Martian "; break;
                            case 3: s = "Illirian "; break;
                            case 4: s = "Terminal "; break;
                            case 5: s = "Crescendo "; break;
                            case 6: s = "Crucial "; break;
                            case 7: s = "Evilian "; break;
                            case 8: s = "Vicino "; break;
                            default: s = "Fantasy "; break;
                        }
                        break;
                    case 3:
                        switch (b)
                        {
                            case 0: s = "Sand "; break;
                            case 1: s = "Dryed "; break;
                            case 2: s = "Pebble "; break;
                            case 3: s = "Degranite "; break;
                            case 4: s = "Invasive "; break;
                            case 5: s = "Alloreactive "; break;
                            case 6: s = "Volatus "; break;
                            case 7: s = "Perforated "; break;
                            case 8: s = "Poisoned "; break;
                            default: s = "Dangerous "; break;
                        }
                        break;
                    case 4:
                        switch (b)
                        {
                            case 0: s = "Gyptus "; break;
                            case 1: s = "Nyle "; break;
                            case 2: s = "Chamois "; break;
                            case 3: s = "Beaver "; break;
                            case 4: s = "Mount baguette "; break;
                            case 5: s = "P-Quartz "; break;
                            case 6: s = "Mount-Majestic "; break;
                            case 7: s = "Amethitus "; break;
                            case 8: s = "Elegant "; break;
                            default: s = "Paris "; break;
                        }
                        break;
                    case 5:
                        switch (b)
                        {
                            case 0: s = "Basilisk "; break;
                            case 1: s = "Convetional "; break;
                            case 2: s = "Brass "; break;
                            case 3: s = "Frogshadow "; break;
                            case 4: s = "Chisel "; break;
                            case 5: s = "Electrum "; break;
                            case 6: s = "Silverbone "; break;
                            case 7: s = "Knopweed "; break;
                            case 8: s = "Watereagle "; break;
                            default: s = "Junkweed "; break;
                        }
                        break;
                    case 6:
                        switch (b)
                        {
                            case 0: s = "Toxic ocean "; break;
                            case 1: s = "Toadland "; break;
                            case 2: s = "Crocodile "; break;
                            case 3: s = "Caterpillar "; break;
                            case 4: s = "Potion "; break;
                            case 5: s = "Tea "; break;
                            case 6: s = "Boat "; break;
                            case 7: s = "Niagara "; break;
                            case 8: s = "Stroken "; break;
                            default: s = "Waterfall "; break;
                        }
                        break;
                    case 7:
                        switch (b)
                        {
                            case 0: s = "Chartres "; break;
                            case 1: s = "Autumn dryad "; break;
                            case 2: s = "Pond "; break;
                            case 3: s = "July "; break;
                            case 4: s = "June "; break;
                            case 5: s = "Seladone "; break;
                            case 6: s = "Magicmint "; break;
                            case 7: s = "Dustblue "; break;
                            case 8: s = "Hoarfrost "; break;
                            default: s = "Season "; break;
                        }
                        break;
                    case 8:
                        switch (b)
                        {
                            case 0: s = "Bud "; break;
                            case 1: s = "Flowersea "; break;
                            case 2: s = "Surface "; break;
                            case 3: s = "Truecolor "; break;
                            case 4: s = "Wavemonger "; break;
                            case 5: s = "Peppermint "; break;
                            case 6: s = "Concepcion "; break;
                            case 7: s = "Sea-like "; break;
                            case 8: s = "Snow cyan "; break;
                            default: s = "Snow "; break;
                        }
                        break;
                    default: s = "Campfire "; break; // 
                }
                break;
            case 6:
                switch (g)
                {
                    case 0:
                        switch (b)
                        {
                            case 0: s = "Firesmell "; break;
                            case 1: s = "Scarlet "; break;
                            case 2: s = "Cardinal "; break;
                            case 3: s = "Jazz "; break;
                            case 4: s = "Amorale "; break;
                            case 5: s = "Bizarre "; break;
                            case 6: s = "Majestia "; break;
                            case 7: s = "Voxiferrum "; break;
                            case 8: s = "Partynight "; break;
                            default: s = "Music "; break;
                        }
                        break;
                    case 1:
                        switch (b)
                        {
                            case 0: s = "Furioso "; break;
                            case 1: s = "Black Arlet "; break; // black scarlet
                            case 2: s = "Persed "; break; // persian red
                            case 3: s = "Changed "; break;
                            case 4: s = "Tasty "; break;
                            case 5: s = "Fuchsia "; break;
                            case 6: s = "Fushun "; break;
                            case 7: s = "A-Magenta "; break;
                            case 8: s = "Mystic flame "; break;
                            default: s = "Owl bunshee "; break;
                        }
                        break;
                    case 2:
                        switch (b)
                        {
                            case 0: s = "Redwood "; break;
                            case 1: s = "Pepper "; break;
                            case 2: s = "Redpearl "; break;
                            case 3: s = "Bright cherry  "; break;
                            case 4: s = "Dusk flyer "; break;
                            case 5: s = "Aubergine "; break;
                            case 6: s = "Guinny "; break;
                            case 7: s = "Orvidia "; break;
                            case 8: s = "Savannah "; break;
                            default: s = "Shell "; break;
                        }
                        break;
                    case 3:
                        switch (b)
                        {
                            case 0: s = "Burned happiness "; break;
                            case 1: s = "Burned coral "; break;
                            case 2: s = "Burned clay "; break;
                            case 3: s = "Pink valley "; break;
                            case 4: s = "Antique "; break;
                            case 5: s = "Pink gold "; break;
                            case 6: s = "Viola "; break;
                            case 7: s = "Toolong "; break;
                            case 8: s = "Heliotrope "; break;
                            default: s = "Burning man "; break;
                        }
                        break;
                    case 4:
                        switch (b)
                        {
                            case 0: s = "Zond "; break;
                            case 1: s = "Ochre "; break;
                            case 2: s = "Bronze "; break;
                            case 3: s = "Bodyless "; break;
                            case 4: s = "Opera floor "; break;
                            case 5: s = "Curtain "; break;
                            case 6: s = "Used "; break;
                            case 7: s = "Yolo "; break;
                            case 8: s = "Shocking "; break;
                            default: s = "Meme "; break;
                        }
                        break;
                    case 5:
                        switch (b)
                        {
                            case 0: s = "Warned "; break;
                            case 1: s = "Rakite "; break;
                            case 2: s = "Peach "; break;
                            case 3: s = "Tomb "; break;
                            case 4: s = "Headstone "; break;
                            case 5: s = "Nutstone "; break;
                            case 6: s = "Glycynia-X "; break;
                            case 7: s = "Glycyrine "; break;
                            case 8: s = "Lilac "; break;
                            default: s = "Illiad "; break;
                        }
                        break;
                    case 6:
                        switch (b)
                        {
                            case 0: s = "Fog "; break;
                            case 1: s = "Lemontree "; break;
                            case 2: s = "Liontooth "; break;
                            case 3: s = "Tachi "; break;
                            case 4: s = "Running "; break;
                            case 5: s = "Beach "; break;
                            case 6: s = "Silver "; break;
                            case 7: s = "Periwinkle "; break;
                            case 8: s = "Aerice "; break;
                            default: s = " "; break;
                        }
                        break;
                    case 7:
                        switch (b)
                        {
                            case 0: s = "Toxic waste "; break;
                            case 1: s = "Poisoned water "; break;
                            case 2: s = "Peachless "; break;
                            case 3: s = "Junglemen "; break;
                            case 4: s = "Slightly toxic "; break;
                            case 5: s = "Teaspoon "; break;
                            case 6: s = "Highlander "; break;
                            case 7: s = "Polar ice "; break;
                            case 8: s = "Hided "; break;
                            default: s = "Untold "; break;
                        }
                        break;
                    case 8:
                        switch (b)
                        {
                            case 0: s = "Clime "; break;
                            case 1: s = "Slime "; break;
                            case 2: s = "Blame "; break;
                            case 3: s = "Claim "; break;
                            case 4: s = "Touchnote "; break;
                            case 5: s = "Moss "; break;
                            case 6: s = "Herba "; break;
                            case 7: s = "Grown "; break;
                            case 8: s = "Perfect sky "; break;
                            default: s = "Horse "; break;
                        }
                        break;
                    default: s = "Invisible "; break;
                }
                break;
            case 7:
                switch (g)
                {
                    case 0:
                        switch (b)
                        {
                            case 0: s = "Burning blood "; break;
                            case 1: s = "Wish "; break;
                            case 2: s = "Alarm "; break;
                            case 3: s = "Access "; break;
                            case 4: s = "Fashionable "; break;
                            case 5: s = "Motein "; break;
                            case 6: s = "Agenda "; break;
                            case 7: s = "Odourant "; break;
                            case 8: s = "Purpur wall "; break;
                            default: s = "Prophet "; break;
                        }
                        break;
                    case 1:
                        switch (b)
                        {
                            case 0: s = "Scalar "; break;
                            case 1: s = "Firetrick "; break;
                            case 2: s = "Alizarin "; break;
                            case 3: s = "Vanity "; break;
                            case 4: s = "Parfume "; break;
                            case 5: s = "Smiling "; break;
                            case 6: s = "Lips "; break;
                            case 7: s = "Oxide "; break;
                            case 8: s = "Dusk "; break;
                            default: s = "Fallen sun "; break;
                        }
                        break;
                    case 2:
                        switch (b)
                        {
                            case 0: s = "Tician "; break;
                            case 1: s = "Firewood "; break;
                            case 2: s = "Ainese "; break;
                            case 3: s = "Loneskin "; break;
                            case 4: s = "Boring "; break;
                            case 5: s = "Silktouch "; break;
                            case 6: s = "Flickering "; break;
                            case 7: s = "Nocturne "; break;
                            case 8: s = "Indifferent "; break;
                            default: s = "Easy-forming "; break;
                        }
                        break;
                    case 3:
                        switch (b)
                        {
                            case 0: s = "Orange cup "; break;
                            case 1: s = "Wrench "; break;
                            case 2: s = "Calmon "; break;
                            case 3: s = "Shipyard "; break;
                            case 4: s = "Chemical "; break;
                            case 5: s = "Tender "; break;
                            case 6: s = "Calmaridea "; break;
                            case 7: s = "Viotouch "; break;
                            case 8: s = "Hexagon "; break;
                            default: s = "Ovidian "; break;
                        }
                        break;
                    case 4:
                        switch (b)
                        {
                            case 0: s = "Carrotian "; break;
                            case 1: s = "Menodrine"; break; //man mandarine
                            case 2: s = "Plank "; break;
                            case 3: s = "Oldnote "; break;
                            case 4: s = "Satellite "; break;
                            case 5: s = "Reminded "; break;
                            case 6: s = "Lauranda "; break; //lavanda
                            case 7: s = "Vial "; break;
                            case 8: s = "Cheliocrop "; break; //heliotrop
                            default: s = "Che confusione "; break; // sara perche ti amo
                        }
                        break;
                    case 5:
                        switch (b)
                        {
                            case 0: s = "Corn "; break;
                            case 1: s = "Gummy "; break;
                            case 2: s = "Elbow "; break; // yellow elbow
                            case 3: s = "Crust "; break;
                            case 4: s = "Somon "; break;
                            case 5: s = "Carnation "; break;
                            case 6: s = "Cotton "; break;
                            case 7: s = "Aquaphobia "; break;
                            case 8: s = "Cynthia "; break;
                            default: s = "Glamour moon "; break;
                        }
                        break;
                    case 6:
                        switch (b)
                        {
                            case 0: s = "Saffran "; break;
                            case 1: s = "Molden desert "; break;
                            case 2: s = "Jugbone "; break;
                            case 3: s = "Thickhair "; break;
                            case 4: s = "Dirt "; break;
                            case 5: s = "Backpanel "; break;
                            case 6: s = "Weightless "; break;
                            case 7: s = "Witness "; break;
                            case 8: s = "Hologramm "; break;
                            default: s = "Endgame "; break;
                        }
                        break;
                    case 7:
                        switch (b)
                        {
                            case 0: s = "Unacceptable "; break;
                            case 1: s = "Spoiled "; break;
                            case 2: s = "Determined "; break;
                            case 3: s = "Riverbeam "; break;
                            case 4: s = "Lazyland "; break;
                            case 5: s = "Flax "; break;
                            case 6: s = "Cavesilver "; break;
                            case 7: s = "Tonnelstone "; break;
                            case 8: s = "Wisdom "; break;
                            default: s = "Oldtimes "; break;
                        }
                        break;
                    case 8:
                        switch (b)
                        {
                            case 0: s = "Electrotree "; break;
                            case 1: s = "Eternal lemon "; break;
                            case 2: s = "Juicemaster "; break;
                            case 3: s = "Fruitbreaker "; break;
                            case 4: s = "Yummy "; break;
                            case 5: s = "Cleargrass "; break;
                            case 6: s = "Highground meadows "; break;
                            case 7: s = "Firstborn "; break;
                            case 8: s = "Powercyan "; break;
                            default: s = "Highsky "; break;
                        }
                        break;
                    default: s = "Watching "; break;
                }
                break;
            case 8:
                switch (g)
                {
                    case 0:
                        switch (b)
                        {
                            case 0: s = "Red "; break;
                            case 1: s = "Redpowered "; break;
                            case 2: s = "Redflower "; break;
                            case 3: s = "Pseudopink "; break;
                            case 4: s = "Touchmaster "; break;
                            case 5: s = "Extracute "; break;
                            case 6: s = "Capricci "; break;
                            case 7: s = "Violand "; break;
                            case 8: s = "Majenta "; break;
                            default: s = "Tachibana "; break;
                        }
                        break;
                    case 1:
                        switch (b)
                        {
                            case 0: s = "Firestorm "; break;
                            case 1: s = "Immolating "; break;
                            case 2: s = "Rosengarden "; break;
                            case 3: s = "Conservative "; break;
                            case 4: s = "Ocean coral "; break;
                            case 5: s = "Pinkwise "; break;
                            case 6: s = "Elytralight "; break;
                            case 7: s = "Midforce "; break;
                            case 8: s = "Handclap "; break;
                            default: s = "Fashion "; break;
                        }
                        break;
                    case 2:
                        switch (b)
                        {
                            case 0: s = "Orange mood "; break;
                            case 1: s = "Sunset "; break;
                            case 2: s = "Deepcoral "; break;
                            case 3: s = "Radical "; break;
                            case 4: s = "Fiancais"; break; //french
                            case 5: s = "Dreaming "; break;
                            case 6: s = "Manganese "; break;
                            case 7: s = "Flamingo "; break;
                            case 8: s = "Outerspace "; break;
                            default: s = "Pleasure "; break;
                        }
                        break;
                    case 3:
                        switch (b)
                        {
                            case 0: s = "Sunhope "; break;
                            case 1: s = "Oldsun "; break;
                            case 2: s = "Pepperpeach "; break;
                            case 3: s = "Golem "; break;
                            case 4: s = "Lifeboosted "; break;
                            case 5: s = "Fingertouch "; break;
                            case 6: s = "Aroma "; break;
                            case 7: s = "Skystrip "; break;
                            case 8: s = "Volatus "; break;
                            default: s = "Skyrunning "; break;
                        }
                        break;
                    case 4:
                        switch (b)
                        {
                            case 0: s = "Orange machine "; break;
                            case 1: s = "Decision "; break;
                            case 2: s = "Mango "; break;
                            case 3: s = "Catalogue "; break;
                            case 4: s = "Delicious "; break;
                            case 5: s = "Delicate "; break;
                            case 6: s = "Trim "; break;
                            case 7: s = "Nightstar "; break;
                            case 8: s = "Emotional "; break;
                            default: s = "Bear "; break; // haha
                        }
                        break;
                    case 5:
                        switch (b)
                        {
                            case 0: s = "Orangepeel "; break;
                            case 1: s = "Signal of hope "; break;
                            case 2: s = "Palatable "; break;
                            case 3: s = "Placid "; break;
                            case 4: s = "Distant "; break;
                            case 5: s = "Wide "; break;
                            case 6: s = "Introvert "; break;
                            case 7: s = "Flask "; break;
                            case 8: s = "Elusive "; break;
                            default: s = "Gorgeous "; break;
                        }
                        break;
                    case 6:
                        switch (b)
                        {
                            case 0: s = "Amberlight "; break;
                            case 1: s = "Sunwave "; break;
                            case 2: s = "Softgrass "; break;
                            case 3: s = "Widefield "; break;
                            case 4: s = "Pasta del "; break;
                            case 5: s = "Appricot "; break;
                            case 6: s = "Palehand "; break;
                            case 7: s = "Skyfields "; break;
                            case 8: s = "Softdream "; break;
                            default: s = "Flying thought "; break;
                        }
                        break;
                    case 7:
                        switch (b)
                        {
                            case 0: s = "Flying ball "; break;
                            case 1: s = "Orandex "; break;
                            case 2: s = "Thick "; break;
                            case 3: s = "Mustard "; break;
                            case 4: s = "Housebone "; break;
                            case 5: s = "Crates "; break;
                            case 6: s = "Treebird "; break;
                            case 7: s = "Dimsun "; break;
                            case 8: s = "Creativity "; break;
                            default: s = "Painless "; break;
                        }
                        break;
                    case 8:
                        switch (b)
                        {
                            case 0: s = "Yellow "; break;
                            case 1: s = "Sulphur "; break;
                            case 2: s = "Spacelemon "; break;
                            case 3: s = "Lemonmint "; break;
                            case 4: s = "Self-igniting lemon "; break;
                            case 5: s = "Speechless lemon "; break;
                            case 6: s = "Lemontrail "; break;
                            case 7: s = "Yellow air "; break;
                            case 8: s = "White "; break;
                            default: s = "Air "; break;
                        }
                        break;
                }
                break;
            default:
                break;
        }

        byte x = (byte)(a.GetAffectionValue() / 0.04f);
        switch (a.affectionPath)
        {
            case Path.LifePath:
                switch (x)
                {
                    case 0: s += "Husk"; break;
                    case 1: s += "Stub"; break;
                    case 2: s += "Pea"; break;
                    case 3: s += "Dewdrop"; break;
                    case 4: s += "Seed"; break;
                    case 5: s += "Petal"; break;
                    case 6: s += "Plast"; break;
                    case 7: s += "Stalk"; break;
                    case 8: s += "Sprout"; break;
                    case 9: s += "Rootlet"; break;
                    case 12: s += "Bourgeon"; break;
                    case 13: s += "Floret"; break;
                    case 14: s += "Bush"; break;
                    case 15: s += "Shrub"; break;
                    case 16: s += "Lifetree"; break;
                    case 17: s += "Grove"; break;
                    case 18: s += "Scafford"; break;
                    case 19: s += "Sphere"; break;
                    case 20: s += "Source"; break;
                    case 21: s += "Emitter"; break;
                    case 22: s += "Fountain"; break;
                    case 23: s += "Grantor"; break;
                    case 24: s += "Heart"; break;
                    case 25: s += "Might"; break;
                    default: s += "Hand"; break;
                }
                break;
            case Path.SecretPath:
                switch (x)
                {
                    case 0: s += "Speck"; break;
                    case 1: s += "Drop"; break;
                    case 2: s += "Point"; break;
                    case 3: s += "Line"; break;
                    case 4: s += "Layout"; break;
                    case 5: s += "Scheme"; break;
                    case 6: s += "Sketch"; break;
                    case 7: s += "Canvas"; break;
                    case 8: s += "Vignette"; break;
                    case 9: s += "Illustatio"; break;
                    case 10: s += "Gravur"; break;
                    case 11: s += "Fresco"; break;
                    case 12: s += "Graffiti"; break;
                    case 13: s += "Painting"; break;
                    case 14: s += "Triptych"; break;
                    case 15: s += "Album"; break;
                    case 16: s += "Maquette"; break;
                    case 17: s += "Model"; break;
                    case 18: s += "Sculpture"; break;
                    case 19: s += "Panorama"; break;
                    case 20: s += "Exhibition"; break;
                    case 21: s += "Dimension"; break;
                    case 22: s += "Simulator"; break;
                    case 23: s += "Equalizer"; break;
                    case 24: s += "Settingbox"; break;
                    case 25: s += "File"; break; // напильник!)
                    case 26: s += "Modulator"; break;
                }
                break;
            case Path.TechPath:
                switch (x)
                {
                    case 0: s += "Negation"; break;
                    case 1: s += "Shield"; break;
                    case 2: s += "Box"; break;
                    case 3: s += "Cage"; break;
                    case 4: s += "Discord"; break;

                    case 5: s += "Wraith"; break;
                    case 6: s += "Ragequit"; break;
                    case 7: s += "Pain"; break;
                    case 8: s += "Injector"; break;
                    case 9: s += "Sedative"; break;

                    case 10: s += "Chaffer"; break;
                    case 11: s += "Fighter"; break;
                    case 12: s += "Opponent"; break;
                    case 13: s += "Balancer"; break;
                    case 14: s += "Gyroscope"; break;

                    case 15: s += "Press"; break;
                    case 16: s += "Pillar"; break;
                    case 17: s += "Slab"; break;
                    case 18: s += "Block"; break;
                    case 19: s += "Compound"; break;

                    case 20: s += "Adoption"; break;
                    case 21: s += "Consensus"; break;
                    case 22: s += "Freeze"; break;
                    case 23: s += "Tranquility"; break;
                    case 24: s += "Silence"; break;
                    case 25: s += "Sleep"; break;
                    default: s += "Stabilizer"; break;
                }
                break;
            case Path.NoPath:
            default:
                switch (x)
                {
                    case 0: s += "Dustpile"; break;
                    case 1: s += "Puddle"; break;
                    case 2: s += "Shadow"; break;
                    case 3: s += "Whisper"; break;
                    case 4: s += "Footprint"; break;
                    case 5: s += "Thing"; break;
                    case 6: s += "Accessory"; break;
                    case 7: s += "Watch"; break;
                    case 8: s += "Key"; break;
                    case 9: s += "Ball"; break;
                    case 10: s += "Case"; break;
                    case 11: s += "Cookie"; break;
                    case 12: s += "Engine"; break;
                    case 13: s += "Ring"; break;
                    case 14: s += "Book"; break;
                    case 15: s += "Broken sword"; break;
                    case 16: s += "Chandelier"; break;
                    case 17: s += "Throne"; break;
                    case 18: s += "Opinion"; break;
                    case 19: s += "Guide"; break;
                    case 20: s += "Torch"; break;
                    case 21: s += "Rock"; break;
                    case 22: s += "Lamp"; break;
                    case 23: s += "Diamond"; break;
                    case 24: s += "Stick"; break;
                    case 25: s += "Glasses"; break;
                    default: s += "Chessmaster"; break;
                }
                break;
        }
        return s;
    }
    #endregion

    public static string GetWord(LocalizedWord word)
    {
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
                        case LocalizedWord.Owner: return "Владелец";
                        case LocalizedWord.Pass: return "Пройти";
                        case LocalizedWord.Progress: return "Прогресс";
                        case LocalizedWord.Repair: return "Починить"; // shuttle
                        case LocalizedWord.Roll: return "БРОСОК";
                        case LocalizedWord.Sell: return "Продавать";
                        case LocalizedWord.Stability: return "Стабильность";
                        case LocalizedWord.Stamina: return "Выносливость";
                        case LocalizedWord.Step: return "Шаг";
                        case LocalizedWord.UpgradeCost: return "Стоимость улучшения";
                        case LocalizedWord.Upgrade: return "Улучшить"; // upgrade building

                        case LocalizedWord.Persistence: return "НАСТОЙЧИВОСТЬ";
                        case LocalizedWord.SurvivalSkills: return "НАВЫКИ ВЫЖИВАНИЯ";
                        case LocalizedWord.Perception: return "ВОСПРИЯТИЕ";
                        case LocalizedWord.SecretKnowledge: return "ТАЙНОЕ ЗНАНИЕ";
                        case LocalizedWord.Intelligence: return "ИНТЕЛЛЕКТ";
                        case LocalizedWord.TechSkills: return "ОБРАЩЕНИЕ С ТЕХНИКОЙ";

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
                        case LocalizedWord.Owner: return "Owner"; // shuttle owner
                        case LocalizedWord.Pass: return "Pass";
                        case LocalizedWord.Progress: return "Progress";
                        case LocalizedWord.Repair: return "Repair"; // shuttle
                        case LocalizedWord.Roll: return "ROLL";
                        case LocalizedWord.Sell: return "Sell";
                        case LocalizedWord.Stability: return "Stability";
                        case LocalizedWord.Step: return "Step";
                        case LocalizedWord.UpgradeCost: return "Upgrade cost";
                        case LocalizedWord.Upgrade: return "Upgrade"; // upgrade building       

                        case LocalizedWord.Persistence: return "PERSISTENCE";
                        case LocalizedWord.SurvivalSkills: return "SURVIVAL SKILLS";
                        case LocalizedWord.Perception: return "PERCEPTION";
                        case LocalizedWord.SecretKnowledge: return "SECRET KNOWLEDGE";
                        case LocalizedWord.Intelligence: return "INTELLIGENCE";
                        case LocalizedWord.TechSkills: return "TECH SKILLS";
 
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
                        case LocalizedWord.Stamina: return "Stamina";
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
                        case LocalizedPhrase.AddBuilding: return "Добавить здание";
                        case LocalizedPhrase.AffectionTypeNotMatch: return "Тип воздействия не совпадает";
                        case LocalizedPhrase.ArtifactNotResearched: return "Артефакт не изучен";
                        case LocalizedPhrase.AscensionLevel: return "Уровень Возвышения";
                        case LocalizedPhrase.ClearSlot: return "< Очистить слот >";
                        case LocalizedPhrase.ConnectionOK: return "Есть связь";
                        case LocalizedPhrase.ConnectionLost: return "Связь потеряна";
                        case LocalizedPhrase.ConvertToBlock: return "Перестроить в блок"; // settlement
                        case LocalizedPhrase.CrewFoundArtifact: return "Наша команда нашла артефакт!";
                        case LocalizedPhrase.CrystalsCollected: return "Кристаллов найдено";
                        case LocalizedPhrase.FreeShuttles: return "Свободные челноки: ";
                        case LocalizedPhrase.FreeTransmitters: return "Незанятые передатчики: ";
                        case LocalizedPhrase.FuelNeeded: return "Требуется топлива: ";
                        case LocalizedPhrase.GoOnATrip: return "Отправить в путешествие";
                        case LocalizedPhrase.KnowledgePoints: return "Очки Знания";
                        case LocalizedPhrase.MembersCount: return "Число участников";
                        case LocalizedPhrase.NoArtifact: return "Нет артефакта"; // у команды
                        case LocalizedPhrase.NoArtifacts: return "У вас нет артефактов"; // в хранилище
                        case LocalizedPhrase.NoCrews: return "Нет готовых команд";
                        case LocalizedPhrase.NoExpeditions: return "Нет экспедиций";
                        case LocalizedPhrase.NoShuttles: return "Нет челноков";
                        case LocalizedPhrase.NoSuitableArtifacts: return "Нет подходящих артефактов";
                        case LocalizedPhrase.NoSuitableParts: return "Нет нужных деталек"; // исследования
                        case LocalizedPhrase.NoSuitableShuttles: return "Нет подходящего челнока";
                        case LocalizedPhrase.NotEnoughEnergySupply: return "Недостаточно мощная энергосеть";
                        case LocalizedPhrase.NotResearched: return "Не исследован"; // artifact
                        case LocalizedPhrase.OpenExpeditionWindow: return "Открыть окно экспедиции";
                        case LocalizedPhrase.PointsSec: return " ед./сек";
                        case LocalizedPhrase.PerSecond: return "в секунду";
                        case LocalizedPhrase.PressToTurnOn: return "Нажмите, чтобы включить";
                        case LocalizedPhrase.RecallExpedition: return "Отозвать экспедицию";
                        case LocalizedPhrase.SendExpedition: return "Отправить экспедицию";
                        case LocalizedPhrase.ShuttleReady: return "Челнок готов";
                        case LocalizedPhrase.StopDig: return "Остановить добычу";
                        case LocalizedPhrase.StopGather: return "Остановить сбор";
                        case LocalizedPhrase.StopMission: return "Прервать миссию";
                        case LocalizedPhrase.SuppliesLeft: return "Припасов осталось";
                        case LocalizedPhrase.UnoccupiedTransmitters: return "Свободно передатчиков: ";
                        case LocalizedPhrase.YouAreHere: return "Вы находитесь здесь";

                        case LocalizedPhrase.FreeAttributePoints: return "ДОСТУПНО ОЧКОВ: ";

                        case LocalizedPhrase.RequiredSurface: return "Требуемая поверхность";
                        case LocalizedPhrase.ColonizationEnabled: return "Привозить колонистов";
                        case LocalizedPhrase.ColonizationDisabled: return "Не привозить колонистов";
                        case LocalizedPhrase.TicketsLeft: return "Запросов";
                        case LocalizedPhrase.ColonistsArrived: return "Прибыли колонисты";
                        case LocalizedPhrase.BirthrateMode: return "Режим появления";
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
                        case LocalizedPhrase.OpenResearchTab: return "К исследованиям";
                        default: return "<...>";
                    }
                }
            case Language.English:
            default:
                {
                    switch (lp)
                    {
                        case LocalizedPhrase.AddBuilding: return "Add building";
                        case LocalizedPhrase.AffectionTypeNotMatch: return "Affection type not match"; // artifact aff type
                        case LocalizedPhrase.ArtifactNotResearched: return "Artifact not researched";
                        case LocalizedPhrase.AscensionLevel: return "Ascension level";
                        case LocalizedPhrase.ClearSlot: return "< Clear slot >";
                        case LocalizedPhrase.ConnectionOK: return "Real-time connection";
                        case LocalizedPhrase.ConnectionLost: return "Connection lost";
                        case LocalizedPhrase.ConvertToBlock: return "Convert to block"; // settlement
                        case LocalizedPhrase.CrewFoundArtifact: return "Our crew has found an artifact!";
                        case LocalizedPhrase.CrystalsCollected: return "Crystals collected";
                        case LocalizedPhrase.FreeShuttles: return "Shuttles free: ";
                        case LocalizedPhrase.FreeTransmitters: return "Transmitters free: ";
                        case LocalizedPhrase.FuelNeeded: return "Fuel needed: ";
                        case LocalizedPhrase.GoOnATrip: return "Go on a trip";
                        case LocalizedPhrase.KnowledgePoints:return "Knowledge points";
                        case LocalizedPhrase.MembersCount: return "Members count";
                        case LocalizedPhrase.NoArtifact: return "No artifact"; // crew has no artifact
                        case LocalizedPhrase.NoArtifacts: return "You have no artifacts"; // no artifacts in storage
                        case LocalizedPhrase.NoCrews: return "No crews available";
                        case LocalizedPhrase.NoExpeditions: return "No expeditions at this moment";
                        case LocalizedPhrase.NoShuttles: return "No shuttles available";
                        case LocalizedPhrase.NoSuitableArtifacts: return "No suitable artifacts";
                        case LocalizedPhrase.NoSuitableParts: return "No suitable parts";
                        case LocalizedPhrase.NoSuitableShuttles: return "No suitable shuttles";
                        case LocalizedPhrase.NotEnoughEnergySupply: return "Energy supply is low";
                        case LocalizedPhrase.NotResearched: return "Not researched"; // artifact
                        case LocalizedPhrase.OpenExpeditionWindow: return "Open expedition window";
                        case LocalizedPhrase.PointsSec: return "points/sec";
                        case LocalizedPhrase.PerSecond: return "per second";
                        case LocalizedPhrase.PressToTurnOn: return "Press to turn on";
                        case LocalizedPhrase.RecallExpedition: return "Recall expedition";
                        case LocalizedPhrase.SendExpedition: return "Send expedition";
                        case LocalizedPhrase.ShuttleReady: return "Shuttle ready";
                        case LocalizedPhrase.StopDig: return "Stop digging";
                        case LocalizedPhrase.StopGather: return "Stop gathering";
                        case LocalizedPhrase.StopMission: return "Stop mission";
                        case LocalizedPhrase.SuppliesLeft: return "Supplies left";
                        case LocalizedPhrase.UnoccupiedTransmitters: return "Unoccupied transmitters: ";
                        case LocalizedPhrase.YouAreHere: return "You are here";

                        case LocalizedPhrase.FreeAttributePoints: return "FREE POINTS: ";

                        case LocalizedPhrase.RequiredSurface: return "Required surface";
                        case LocalizedPhrase.ColonizationEnabled: return "Immigration enabled";
                        case LocalizedPhrase.ColonizationDisabled: return "Immigration disabled";
                        case LocalizedPhrase.TicketsLeft: return "Tickets left";
                        case LocalizedPhrase.ColonistsArrived: return "Colonists arrived";
                        case LocalizedPhrase.BirthrateMode: return "Spawnrate mode";
                        case LocalizedPhrase.NoActivity: return "No activity";
                        case LocalizedPhrase.CrewSlots: return "Crew slots";
                        case LocalizedPhrase.NoFreeSlots: return "No free slots";
                        case LocalizedPhrase.HireNewCrew: return "Hire new crew";
                        case LocalizedPhrase.NoCrew: return "No crew";
                        case LocalizedPhrase.ConstructShuttle: return "Construct shuttle";
                        case LocalizedPhrase.ShuttleConstructed: return "Shuttle construction complete";
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
                        case LocalizedPhrase.ChangeSurfaceMaterial: return "Change surface";
                        case LocalizedPhrase.CreateBlock: return "Create block";
                        case LocalizedPhrase.CreateColumn: return "Create column";
                        case LocalizedPhrase.AddPlatform: return "Add platform";
                        case LocalizedPhrase.OpenMap: return "Open map";
                        case LocalizedPhrase.OpenResearchTab: return "Open research tab";
                        default: return "<...>";
                    }
                }
        }
    }
    public static string GetRefusalReason(RefusalReason rr)
    {
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
                        case RefusalReason.HQ_RR4: return "Нет топливного завода";

                        case RefusalReason.SpaceAboveBlocked: return "Пространство сверху блокируется";
                        case RefusalReason.NoBlockBelow: return "Ниже ничего нет";
                        case RefusalReason.NotEnoughSlots: return "Нет свободных ячеек";
                        case RefusalReason.WorkNotFinished: return "Работы не окончены";
                        case RefusalReason.MustBeBuildedOnFoundationBlock: return "Нужно строить на блоке основания или на другом искусственном блоке";
                        case RefusalReason.NoEmptySpace: return "Нет свободного пространства";
                        case RefusalReason.AlreadyBuilt: return "Уже построено";
                        case RefusalReason.UnacceptableHeight: return "Неподходящая высота";
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
                        case RefusalReason.HQ_RR3: return "No graphonium enrichers";
                        case RefusalReason.HQ_RR4: return "No fuel facility";

                        case RefusalReason.SpaceAboveBlocked: return "Space above blocked";
                        case RefusalReason.NoBlockBelow: return "No block below";
                        case RefusalReason.NotEnoughSlots: return "Not enough slots";
                        case RefusalReason.WorkNotFinished: return "Work not finished";
                        case RefusalReason.MustBeBuildedOnFoundationBlock: return "Must be builded on Foundation Block or another artificial block";
                        case RefusalReason.NoEmptySpace: return "No empty space";
                        case RefusalReason.AlreadyBuilt: return "Already built";
                        case RefusalReason.UnacceptableHeight: return "Height is not suitable";
                    }
                }
        }
    }
    public static string GetExpeditionErrorText(ExpeditionComposingErrors ece)
    {
        switch (currentLanguage)
        {
            case Language.English:
            default:
                switch (ece)
                {
                    default: return "Expedition composing error";
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
    public static string GetBuyMsg(ResourceType rtype, float count, float price)
    {
        switch(currentLanguage)
        {
            case Language.Russian:
                return "Купили \"" + GetResourceName(rtype.ID) + " в количестве " + string.Format("{0:0.##}", count) + " за " + string.Format("{0:0.##}", price);
            default:
                return "Bought " + string.Format("{0:0.##}", count) + " of " + GetResourceName(rtype.ID) + " have bougth for a " + string.Format("{0:0.##}", price);
        }
    }
    public static string GetSellMsg(ResourceType rtype, float count, float price)
    {
        switch (currentLanguage)
        {
            case Language.Russian:
                return "Продали \"" + GetResourceName(rtype.ID) + " в количестве " + string.Format("{0:0.##}", count) + " за " + string.Format("{0:0.##}", price);
            default:
                return "Sold " + string.Format("{0:0.##}", count) + " of " + GetResourceName(rtype.ID) + " for a " + string.Format("{0:0.##}", price);
        }
    }

    public static string GetEndingTitle(GameEndingType endType)
    {
        switch (currentLanguage)
        {
            case Language.Russian:
                {
                    switch (endType)
                    {
                        case GameEndingType.ColonyLost: return "И никого не стало.";
                        case GameEndingType.TransportHubVictory: return "Вы установили прочную связь между вашей колонией и другими Ограниченными Мирами. Благодаря вам, будущее колонии в безопасности. Теперь вы свободны.";
                        case GameEndingType.ConsumedByLastSector: return "Ваш остров пропал в Последнем Секторе. Теперь он в безопасности.";
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
                        case GameEndingType.ColonyLost: return "And then there were none."; 
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
    public static string GetAffectionTitle(Path atype)
    {
        switch (currentLanguage)
        {
            case Language.English:
                {
                    switch (atype)
                    {
                        case Path.SecretPath: return "Space affection";
                        case Path.TechPath: return "Stability affection";
                        case Path.LifePath: return "Lifepower flow affection";
                        default: return "No affection";

                    }
                }
            case Language.Russian:
                {
                    switch (atype)
                    {
                        case Path.SecretPath: return "Влияние на пространство";
                        case Path.TechPath: return "Влияние на стабильность";
                        case Path.LifePath: return "Влияние на поток жизненной силы";
                        default: return "Не активен";
                    }
                }
            default: return "<no affection>";
        }
    }
    public static string GetChallengeLabel(ChallengeType ctype)
    {
        switch (currentLanguage)
        {
            case Language.Russian:
                switch (ctype)
                {
                    case ChallengeType.Impassable: return "Непроходимое место";
                    case ChallengeType.Random: return "Случайное событие!";
                    case ChallengeType.PersistenceTest: return "Проверка Настойчивости";
                    case ChallengeType.SurvivalSkillsTest: return "Проверка Навыков Выживания";
                    case ChallengeType.PerceptionTest: return "Проверка на Восприятие";
                    case ChallengeType.SecretKnowledgeTest: return "Проверка на Тайное Знание";
                    case  ChallengeType.IntelligenceTest: return "Проверка на Интеллект";
                    case  ChallengeType.TechSkillsTest: return "Проверка на Обращение с Техникой";
                    case  ChallengeType.Treasure: return "Сокровища!";
                    case  ChallengeType.QuestTest: return "Сюжетное испытание!";
                    case  ChallengeType.CrystalFee: return "Заплати, чтобы пройти!";
                    case  ChallengeType.AscensionTest: return "Проверка на уровень Возвышения";
                    case ChallengeType.PuzzlePart: return "Кусочек паззла";
                    case ChallengeType.FoundationPts: return "Еще шаг на пути к Основанию";
                    case ChallengeType.CloudWhalePts: return "Еще шаг по пути Облачного Кита";
                    case ChallengeType.EnginePts: return "Еще на шаг ближе к созданию Движителя";
                    case ChallengeType.PipesPts: return "Еще ближе к технологии Порталов";
                    case ChallengeType.CrystalPts: return "Еще шаг к Кристаллизации";
                    case ChallengeType.MonumentPts: return "Еще один шаг к созднаию Монумента";
                    case ChallengeType.BlossomPts: return "Еще шаг на пути к Цветению";
                    case ChallengeType.PollenPts: return "Еще на шаг ближе к Распылению";
                    case  ChallengeType.NoChallenge:
                    default:
                        return "No challenge";
                }
            case Language.English:
            default:
                {
                    switch (ctype)
                    {
                        case  ChallengeType.Impassable: return "Impassable place";
                        case  ChallengeType.Random: return "Random event!";
                        case  ChallengeType.PersistenceTest: return "Persistence test";
                        case  ChallengeType.SurvivalSkillsTest: return "Survival skills test";
                        case  ChallengeType.PerceptionTest: return "Perception test";
                        case  ChallengeType.SecretKnowledgeTest: return "Secret Knowledge test";
                        case  ChallengeType.IntelligenceTest: return "Intelligence test";
                        case  ChallengeType.TechSkillsTest: return "Technical skills test";
                        case  ChallengeType.Treasure: return "Treasures!";
                        case  ChallengeType.QuestTest: return "Quest test!";
                        case  ChallengeType.CrystalFee: return "Pay to pass!";
                        case  ChallengeType.AscensionTest: return "Ascension level test";
                        case ChallengeType.PuzzlePart: return "Puzzle part";
                        case ChallengeType.FoundationPts: return "One step closer to Foundation";
                        case ChallengeType.CloudWhalePts: return "One more step on Cloud Whale way";
                        case ChallengeType.EnginePts: return "One step closer to Engine creation";
                        case ChallengeType.PipesPts: return "One step closer to Portals technology";
                        case ChallengeType.CrystalPts: return "A bit closer to Crystallisation";
                        case ChallengeType.MonumentPts: return "One step closer to Monument creation";
                        case ChallengeType.BlossomPts: return "One more step to Blossom start";
                        case ChallengeType.PollenPts: return "One more step to Pollen event";
                        case  ChallengeType.NoChallenge:
                        default:
                            return "No challenge";
                    }
                }
        }
    }

    public static string LevelReachedString(byte lvl)
    {
        switch (currentLanguage)
        {
            case Language.Russian:  return "Достигнут уровень " + lvl.ToString() + '!';
            case Language.English:
            default: return "Level " + lvl.ToString() + " reached!";
        }
    }

    public static string GetCredits()
    {
        switch (currentLanguage)
        {
            case Language.Russian:
                return "Выдумано и реализовано Zapilin Entertainment (мной), 2018 - 2019гг н.э. \n" +
                    "Собрано на Unity Engine 2018.3.11 \n" +
                    "В процессе разработки использовались следующие freeware - программы: \n" +
                    " - Blender от Blender Foundations \n" +
                    " - Paint.net \n" +
                    " - Audacity \n" +
                    " - MS Visual Studio Community 2017\n" +
                    "\n" +
                    "Использован шрифт \"neuropolitical rg\" от Typodermic Fonts Inc. (добавил адаптированную кириллицу). \n" +
                    "\n" +
                    "\n" +
                    "Вы можете поддержать мои разработки, купив сюжетные дополнения к Limited Worlds (в недалеком будущем) или сделав пожертвование на один из следующих кошельков: \n" +
                    "\nЯндекс кошелёк: 410 0155 8473 6426\n" +
                    "Paypal: paypal.me/mahariusls\n" +
                    "Не забудьте приложить к дотации комментарий :)";
            case Language.English:
                return "Imaginated and created by Zapilin Entertainment (me), 2018 - 2019 AD \n" +
                    "Builded on Unity Engine 2018.3.11 \n" +
                    "The following freeware was used in the development: \n" +
                    " - Blender by Blender Foundations \n" +
                    " - Paint.net \n" +
                    " - Audacity \n" +
                    " - MS Visual Studio Community 2017\n" +
                    "\n" +
                    "Used font \"neuropolitical rg\" by Typodermic Fonts Inc. \n" +
                    "\n" +
                    "\n" +
                    "You can support my development by buying storyline addons for Limited Worlds (not released yet) or making a donation to one of this accounts: \n" +
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
                                    q.description = "Множество космических путешественников были бы рады заправляться в ваших доках. Постройте топливный завод и произведите 100 единиц топлива - так вы очень поможете исследователям Последнего Сектора.";
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
                                    q.steps[0] = GetStructureName(Structure.COVERED_FARM) + " построена";
                                    break;
                                case ProgressQuestID.Progress_CoveredLumbermill:
                                    q.name = "Лес под крышей";
                                    q.description = "Замените обычную лесопилку крытой - это стабилизирует поток древесины и снизит расход жизненной силы острова.";
                                    q.steps[0] = GetStructureName(Structure.COVERED_LUMBERMILL) + " построена";
                                    break;
                                case ProgressQuestID.Progress_Reactor:
                                    q.name = "Источник энергии";
                                    q.description = "Постройте полноценный графониевый реактор.";
                                    q.steps[0] = GetStructureName(Structure.GRPH_REACTOR_4_ID) + " построен";
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
                                case ProgressQuestID.Progress_FoodStocks:
                                    q.name = "Запасы на зиму";
                                    q.description = "Накопите месячный запас провизии.";
                                    q.steps[0] = "Собрано еды: ";
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
                                    //q.steps[0] = GetStructureName(Structure.CONTROL_CENTER_6_ID) + " построен ";
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
                                    q.description = "Establish your colony's state by constructing two docks. Keep in mind, that docks's flight corridors must not crossing over to be functional.";
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
                                    q.steps[0] = "Build " + GetStructureName(Structure.COVERED_FARM); ;
                                    break;
                                case ProgressQuestID.Progress_CoveredLumbermill:
                                    q.name = "Covered forest";
                                    q.description = "Replace your old lumbermills with new covered one";
                                    q.steps[0] = "Build " + GetStructureName(Structure.COVERED_LUMBERMILL); ;
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
                                case ProgressQuestID.Progress_FoodStocks:
                                    q.name = "Food reserve";
                                    q.description = "Gather food enough for a month";
                                    q.steps[0] = "Food on stocks: ";
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
                    case MapMarkerType.FlyingExpedition: return "Челнок";
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
                    case MapMarkerType.FlyingExpedition: return "Shuttle";
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
    public static string GetMapPointDescription(MapPoint mp)
    {
        switch (currentLanguage)
        {
            case Language.Russian:
                {
                    switch (mp.type)
                    {
                        case MapMarkerType.MyCity: return "Наш маленький кусочек реального мира.";
                        case MapMarkerType.Station: return "Одинокая станция, имеющая сообщение с внешним миром.";
                        case MapMarkerType.Wreck:
                            {
                                return "Металлические остатки чего-то крупного парят повсюду.";
                            }
                        case MapMarkerType.FlyingExpedition: return "Челнок из нашей колонии."; // заменить
                        case MapMarkerType.Island: return "Необитаемый остров.";
                        case MapMarkerType.SOS: return "Может, нам стоит кого-то туда отправить?";
                        case MapMarkerType.Portal: return "Временный проход в какой-то другой мир.";
                        case MapMarkerType.QuestMark: return "Место, обозначенное заданием."; //заменить
                        case MapMarkerType.Colony: return "Мы здесь не одни!";
                        case MapMarkerType.Star: return "Сияет.";
                        case MapMarkerType.Wiseman: return "Наши исследователи могут обрести здесь мудрость.";
                        case MapMarkerType.Wonder: return "Величественное сооружение, возведённое в неясных целях.";
                        case MapMarkerType.Resources: return "То, что мы можем подобрать и использовать.";
                        case MapMarkerType.Unknown:
                        default:
                            return "Это не описать словами.";
                    }
                }
            case Language.English:
            default:
                {
                    switch (mp.type)
                    {
                        case MapMarkerType.MyCity: return "Our stable piece of dream-reality.";
                        case MapMarkerType.Station: return "Lonely station, which has connection with real space out there.";
                        case MapMarkerType.Wreck:
                            {
                                return "Parts of something big and made from metal flying everywhere.";
                            }
                        case MapMarkerType.FlyingExpedition: return "Our colony's shuttle."; // заменить
                        case MapMarkerType.Island: return "An uninhabitated island.";
                        case MapMarkerType.SOS: return "Should we send someone?";
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
    public static string GetMyColonyDescription()
    {
        var c = GameMaster.realMaster.colonyController;
        int lvl = c.hq != null ? c.hq.level : 0;
        var gm = GameMaster.realMaster.globalMap;
        int asc = (int)((gm != null ? gm.ascension : 0f) * 100f);
        if (c != null)
        {
            switch (currentLanguage)
            {
                case Language.Russian:
                    return "Население: " + c.citizenCount.ToString() + "\nУровень колонии: " + lvl.ToString() +
                        "\nУровень Возвышения: " + asc.ToString() + '%';
                case Language.English:
                default:
                    return "Population: " + c.citizenCount.ToString() + "\nColony level: " + lvl.ToString() +
                       "\nAscension level: " + asc.ToString() + '%';
            }
        }
        else
        {
            switch (currentLanguage)
            {
                case Language.Russian: return "Пока здесь ничего нет";
                case Language.English:
                default: return "There's nothing here right now";
            }
        }
    }
    #endregion
}
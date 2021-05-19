public static partial class Localization
{
    public static void FillQuestData(Quest q)
    {
        string name = "<name>", description = "<description>";
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
                                    name = "Жилое пространство";
                                    description = "Возможно, ваши поселенцы хотят жить в условиях получше. Обеспечьте колонию жильём, соответствующим технологическому уровню.";
                                    q.steps[0] = "Обеспеченность жильём  ";
                                    break;
                                case ProgressQuestID.Progress_2Docks:
                                    name = "Основы торговли";
                                    description = "Упрочните положение своей колонии, построив два дока. Обратите внимание, чтобы доки работали, их полётные коридоры не должны пересекаться.";
                                    q.steps[0] = "Построено доков ";
                                    break;
                                case ProgressQuestID.Progress_2Storages:
                                    name = "Система хранения";
                                    description = "Расширьте свои инфраструктуру, построив два новых склада (любого уровня)";
                                    q.steps[0] = "Построено складов ";
                                    break;
                                case ProgressQuestID.Progress_Tier2:
                                    name = "Технологический прогресс I";
                                    description = "Время улучшений! Модернизируйте здание администрации до второго уровня";
                                    q.steps[0] = "Главное здание улучшено до уровня 2";
                                    break;
                                case ProgressQuestID.Progress_300Population:
                                    name = "Первая колонизационная волна";
                                    description = "Вашей колонии нужно развиваться. Используйте панель колонизации в доках, чтобы привлечь больше колонистов на свой остров.";
                                    q.steps[0] = "Колонистов прибыло ";
                                    break;
                                case ProgressQuestID.Progress_OreRefiner:
                                    name = "Переработка руд";
                                    description = "От раскопок остаётся очень много камня, в котором остается еще очень много руды. Постройте обогатитель руды, чтобы получать максимум ресурсов.";
                                    q.steps[0] = GetStructureName(Structure.ORE_ENRICHER_2_ID) + " построен";
                                    break;
                                case ProgressQuestID.Progress_HospitalCoverage:
                                    name = "Медицинское обеспечение";
                                    description = "Вам стоит построить клинику, чтобы все колонисты могли получить медицинскую помощь в случае необходимости.";
                                    q.steps[0] = "Коэффициент медицинского обслуживания ";
                                    break;
                                case ProgressQuestID.Progress_Tier3:
                                    name = "Технологический прогресс II";
                                    description = "Улучшите главное здание до уровня 3";
                                    q.steps[0] = "Главное здание улучшено";
                                    break;
                                case ProgressQuestID.Progress_4MiniReactors:
                                    name = "Четырёхкамерное сердце";
                                    description = "Энергия - то, что наполняет вашу колонию жизнью. Постройте 4 мини-реактора, чтобы подготовить к дальнейшему развитию.";
                                    q.steps[0] = GetStructureName(Structure.MINI_GRPH_REACTOR_3_ID) + ", построено ";
                                    break;
                                case ProgressQuestID.Progress_100Fuel:
                                    name = "Космическая заправка";
                                    description = "Множество космических путешественников были бы рады заправляться в ваших доках. Постройте топливный завод и произведите 100 единиц топлива - так вы очень поможете исследователям Последнего Сектора.";
                                    q.steps[0] = "Собрать 100 топлива ";
                                    break;
                                case ProgressQuestID.Progress_XStation:
                                    name = "Экспериментальный прогноз";
                                    description = "Последний Сектор непредсказуем. Организуйте свою метеорологическую службу, чтобы предвидеть надвигающиеся угрозы.";
                                    q.steps[0] = GetStructureName(Structure.XSTATION_3_ID) + " построена";
                                    break;
                                case ProgressQuestID.Progress_Tier4:
                                    name = "Технологический прогресс III";
                                    description = "Улучшите здание администрации до уровня 4";
                                    q.steps[0] = "Главное здание улучшено";
                                    break;
                                case ProgressQuestID.Progress_CoveredFarm:
                                    name = "Поле под колпаком";
                                    description = "Попробуйте новую гидропонную ферму! Так вы стабилизируете производство пищи и снизите потребление жизненной энергии острова.";
                                    q.steps[0] = GetStructureName(Structure.COVERED_FARM) + " построена";
                                    break;
                                case ProgressQuestID.Progress_CoveredLumbermill:
                                    name = "Лес под крышей";
                                    description = "Внезапно, гидропонная лесопилк! Стабилизирует поток древесины и снизит расход жизненной силы острова.";
                                    q.steps[0] = GetStructureName(Structure.COVERED_LUMBERMILL) + " построена";
                                    break;
                                case ProgressQuestID.Progress_Reactor:
                                    name = "Источник энергии";
                                    description = "Постройте полноценный графониевый реактор.";
                                    q.steps[0] = GetStructureName(Structure.GRPH_REACTOR_4_ID) + " построен";
                                    break;
                                case ProgressQuestID.Progress_FirstExpedition:
                                    name = "Храбрые исследователи";
                                    description = "Подготовьте и успешно завершите экспедицию в загадочный Последний Сектор. Для этого вам нужно набрать команду, произвести для неё челнок, и сформировать экспедицию в Экспедиционном корпусе.";
                                    q.steps[0] = "Команда подготовлена ";
                                    q.steps[1] = "Челнок собран ";
                                    q.steps[2] = "Обсерватория построена";
                                    q.steps[3] = "Передатчик построен ";
                                    q.steps[4] = "Экспедиция запущена ";
                                    q.steps[5] = "Миссия завершена ";
                                    break;
                                case ProgressQuestID.Progress_Tier5:
                                    name = "Технологический прогресс IV";
                                    description = "Улучшите здание администрации до уровня 5";
                                    q.steps[0] = "Главное здание улучшено";
                                    break;
                                case ProgressQuestID.Progress_FactoryComplex:
                                    name = "Фабричный комплекс <в разработке>";
                                    description = "Постройте фабрику на фабричном блоке, чтобы объединить их в фабричный комплекс.";
                                    q.steps[0] = "Фабричный блок построен ";
                                    q.steps[1] = "Фабрика над ним построена ";
                                    break;
                                case ProgressQuestID.Progress_SecondFloor:
                                    name = "Второй этаж";
                                    description = "Постройте здание поверх опоры";
                                    q.steps[0] = "Опора построена ";
                                    q.steps[1] = "Здание на ней построено ";
                                    break;
                                case ProgressQuestID.Progress_FoodStocks:
                                    name = "Запасы на зиму";
                                    description = "Накопите месячный запас провизии.";
                                    q.steps[0] = "Собрано еды: ";
                                    break;
                                default: return;
                            }
                            break;
                        case QuestType.Endgame:
                            switch ((Knowledge.ResearchRoute)q.subIndex)
                            {
                                case Knowledge.ResearchRoute.Foundation:
                                    {
                                        int b = Knowledge.R_F_QUEST_POPULATION_COND;
                                        name = "Путь Основания - Завершение";
                                        description = "Для победы по Пути Основания доведите популяцию до " + b.ToString();
                                        q.steps[0] = "Текущее население: " + GameMaster.realMaster.colonyController.citizenCount.ToString() + " / " + b.ToString();
                                        break;
                                    }
                            }
                            break;
                        case QuestType.Foundation:
                            {
                                var colony = GameMaster.realMaster.colonyController;
                                switch ((Knowledge.FoundationRouteBoosters)q.subIndex)
                                {
                                    case Knowledge.FoundationRouteBoosters.HappinessBoost:
                                        name = "Путь Основания";
                                        description = "";
                                        q.steps[0] = "Уровень довольства: ";
                                        break;
                                    case Knowledge.FoundationRouteBoosters.HotelBoost:
                                        {
                                            name = "Путь Основания";
                                            description = "";
                                            q.steps[0] = GetStructureName(Structure.HOTEL_BLOCK_6_ID) + " построен ";
                                            break;
                                        }
                                    case Knowledge.FoundationRouteBoosters.HousingMastBoost:
                                        name = "Путь Основания";
                                        description = "";
                                        q.steps[0] = GetStructureName(Structure.HOUSING_MAST_6_ID) + " построена ";
                                        break;
                                    case Knowledge.FoundationRouteBoosters.ImmigrantsBoost:
                                        name = "Путь Основания";
                                        description = "";
                                        q.steps[0] = "Количество прибывших: ";
                                        break;
                                    case Knowledge.FoundationRouteBoosters.PointBoost:
                                        name = "Путь Основания";
                                        description = "";
                                        q.steps[0] = "Найти другую Колонию ";
                                        break;
                                    case Knowledge.FoundationRouteBoosters.PopulationBoost:
                                        name = "Путь Основания";
                                        description = "";
                                        q.steps[0] = "Текущее население: ";
                                        break;
                                    case Knowledge.FoundationRouteBoosters.QuestBoost:
                                        name = "Путь Основания - Квест";
                                        description = "";
                                        q.steps[0] = "";
                                        break;
                                    case Knowledge.FoundationRouteBoosters.SettlementBoost:
                                        name = "Путь Основания";
                                        description = "";
                                        q.steps[0] = GetStructureName(Structure.SETTLEMENT_CENTER_ID) + " шестого уровня построен.";
                                        break;
                                }
                                break;
                            }
                        case QuestType.CloudWhale:
                            {
                                switch ((Knowledge.CloudWhaleRouteBoosters)q.subIndex)
                                {
                                    case Knowledge.CloudWhaleRouteBoosters.StreamGensBoost:
                                        name = "Путь Облачного Кита";
                                        description = "Постройте " + GetStructureName(Structure.WIND_GENERATOR_1_ID) + " в количестве " + Knowledge.R_CW_STREAMGENS_COUNT_COND.ToString();
                                        q.steps[0] = "Потоковые генераторы: ";
                                        break;
                                    case Knowledge.CloudWhaleRouteBoosters.CrewsBoost:
                                        name = "Путь Облачного Кита";
                                        description = "Набрать не менее " + Knowledge.R_CW_CREWS_COUNT_COND.ToString() + " команд не ниже уровня " + Knowledge.R_CW_CREW_LEVEL_COND.ToString() + " для охраны острова.";
                                        q.steps[0] = "Команд собрано: ";
                                        break;
                                    case Knowledge.CloudWhaleRouteBoosters.ArtifactBoost:
                                        name = "Путь Облачного Кита";
                                        description = "Найдите артефакт Тайного Пути";
                                        q.steps[0] = "Артефакт найден ";
                                        break;
                                    case Knowledge.CloudWhaleRouteBoosters.XStationBoost:
                                        name = "Путь Облачного Кита";
                                        description = "Постройте следующее сооружение: " + GetStructureName(Structure.XSTATION_3_ID);
                                        q.steps[0] = "Станция построена ";
                                        break;
                                    case Knowledge.CloudWhaleRouteBoosters.StabilityEnforcerBooster:
                                        name = "Путь Облачного Кита";
                                        description = "Постройте следующее сооружение: " + GetStructureName(Structure.STABILITY_ENFORCER_ID);
                                        q.steps[0] = "Стабилизатор построен ";
                                        break;
                                        //case Knowledge.CloudWhaleRouteBoosters.PointBoost:
                                        //case Knowledge.CloudWhaleRouteBoosters.QuestBoost:
                                }
                                break;
                            }
                        case QuestType.Engine:
                            {
                                switch ((Knowledge.EngineRouteBoosters)q.subIndex)
                                {
                                    case Knowledge.EngineRouteBoosters.EnergyBoost:
                                        name = "Путь Движителя";
                                        description = "Попробуйте накопить достаточно энергии, чтобы оценить накопительный потенциал вашей колонии.";
                                        q.steps[0] = "Энергии накоплено: ";
                                        break;
                                    case Knowledge.EngineRouteBoosters.CityMoveBoost:
                                        name = "Путь Движителя";
                                        description = "Попробуйте изменить положение острова в Последнем Секторе.";
                                        q.steps[0] = "Остров сдвинулся ";
                                        break;
                                    case Knowledge.EngineRouteBoosters.GearsBoost:
                                        name = "Путь Движителя";
                                        description = "Улучшите оснащение своих фабрик и мастерских";
                                        q.steps[0] = "Уровень оснащения ";
                                        break;
                                    case Knowledge.EngineRouteBoosters.FactoryBoost:
                                        name = "Путь Движителя";
                                        description = "Нам потребуются немалые мощности, чтобы подготовить путешествие.";
                                        q.steps[0] = "Построено фабрик-кубов: ";
                                        break;
                                    case Knowledge.EngineRouteBoosters.IslandEngineBoost:
                                        name = "Путь Движителя";
                                        description = string.Empty;
                                        q.steps[0] = "Движитель построен ";
                                        break;
                                    case Knowledge.EngineRouteBoosters.ControlCenterBoost:
                                        name = "Путь Движителя";
                                        description = string.Empty;
                                        q.steps[0] = "Контрольный центр построен ";
                                        break;
                                }
                                break;
                            }
                        case QuestType.Pipes:
                            {
                                switch ((Knowledge.PipesRouteBoosters)q.subIndex)
                                {
                                    case Knowledge.PipesRouteBoosters.FarmsBoost:
                                        name = "Путь Труб";
                                        description = "В качестве подготовки, замените все оычные фермы крытыми";
                                        q.steps[0] = "Обычные фермы: ";
                                        q.steps[1] = "Крытые фермы: ";
                                        break;
                                    case Knowledge.PipesRouteBoosters.SizeBoost:
                                        {
                                            name = "Путь Труб";
                                            description = "Крупный объект не сможет пройти сквозь Врата Труб. Убедитесь, что ваш остров достаточно компактный (Достаточно выполнить два условия из трех).";
                                            q.steps[0] = "Ширина: ";
                                            q.steps[1] = "Высота: ";
                                            q.steps[2] = "Длина: ";
                                            break;
                                        }
                                    case Knowledge.PipesRouteBoosters.FuelBoost:
                                        {
                                            name = "Путь Труб";
                                            description = "Нас понадобится большой запас топлива для путешествия.";
                                            q.steps[0] = "Собрано топлива: ";
                                            break;
                                        }
                                    case Knowledge.PipesRouteBoosters.BiomesBoost:
                                        {
                                            name = "Путь Труб";
                                            description = "Подготовьте остров к дальнему путешествию - посетите 4 биома";
                                            q.steps[0] = "Видели океан ";
                                            q.steps[1] = "Видели огненный биом ";
                                            q.steps[2] = "Посетили внешний космос ";
                                            q.steps[3] = "Посетили луга ";
                                            break;
                                        }
                                    case Knowledge.PipesRouteBoosters.QETBoost:
                                        {
                                            name = "Путь Труб";
                                            description = "Энергокристаллы будут нужны всегда. Постройте " + GetStructureName(Structure.QUANTUM_ENERGY_TRANSMITTER_5_ID);
                                            q.steps[0] = "Строение готово ";
                                            break;
                                        }
                                    case Knowledge.PipesRouteBoosters.CapacitorMastBoost:
                                        {
                                            name = "Путь Труб";
                                            description = "Нам понадобится крупный накопитель энергии - " + GetStructureName(Structure.CAPACITOR_MAST_ID);
                                            q.steps[0] = "Накопитель построен ";
                                            break;
                                        }
                                }
                                break;
                            }
                        case QuestType.Crystal:
                            {
                                switch ((Knowledge.CrystalRouteBoosters)q.subIndex)
                                {
                                    case Knowledge.CrystalRouteBoosters.MoneyBoost:
                                    case Knowledge.CrystalRouteBoosters.PinesBoost:
                                    case Knowledge.CrystalRouteBoosters.GCubeBoost:
                                    case Knowledge.CrystalRouteBoosters.BiomeBoost:
                                    case Knowledge.CrystalRouteBoosters.CrystalliserBoost:
                                    case Knowledge.CrystalRouteBoosters.CrystalMastBoost:
                                    case Knowledge.CrystalRouteBoosters.PointBoost:
                                        break;
                                }
                                break;
                            }
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
                                    name = "Living space";
                                    description = "Maybe your citizen want more comfortable houses. Provide all your citizens with housing adequate to your technology level.";
                                    q.steps[0] = "Housing provided  ";
                                    break;
                                case ProgressQuestID.Progress_2Docks:
                                    name = "Trade basement";
                                    description = "Establish your colony's state by constructing two docks. Keep in mind, that docks's flight corridors must not crossing over to be functional.";
                                    q.steps[0] = "Docks built ";
                                    break;
                                case ProgressQuestID.Progress_2Storages:
                                    name = "Storage infrastructure";
                                    description = "Enlarge your storage space by building 2 new warehouses (any level)";
                                    q.steps[0] = "Warehouses built ";
                                    break;
                                case ProgressQuestID.Progress_Tier2:
                                    name = "Techology progress I";
                                    description = "It is time to grow your settlement up. Upgrade your HQ.";
                                    q.steps[0] = "Upgrade HQ to level 2";
                                    break;
                                case ProgressQuestID.Progress_300Population:
                                    name = "First colonization wave";
                                    description = "Your colony needs new members to advance. Use colonization panel in docks to bring new citizens to your island.";
                                    q.steps[0] = "Colonists arrived ";
                                    break;
                                case ProgressQuestID.Progress_OreRefiner:
                                    name = "Ore refining";
                                    description = "Digging left a lot of stone, still contained ores inside. Build an Ore Enricher to extract maximum resources amount.";
                                    q.steps[0] = "Build " + GetStructureName(Structure.ORE_ENRICHER_2_ID);
                                    break;
                                case ProgressQuestID.Progress_HospitalCoverage:
                                    name = "Medical support";
                                    description = "You should build enough hospitals to provide adequate medical supply to all your citizens";
                                    q.steps[0] = "Medical supply coefficient ";
                                    break;
                                case ProgressQuestID.Progress_Tier3:
                                    name = "Technology progress II";
                                    description = "Upgrade your HQ to level 3";
                                    q.steps[0] = "Upgrade HQ to level 3";
                                    break;
                                case ProgressQuestID.Progress_4MiniReactors:
                                    name = "Four-chambered heart";
                                    description = "Energy is the lifeblood of settlement and it will never be much enough. Build 4 mini reactors to be prepared to the further development";
                                    q.steps[0] = "Mini reactors built ";
                                    break;
                                case ProgressQuestID.Progress_100Fuel:
                                    name = "Space gas station";
                                    description = "There is a lot of space travellers who will be were happy to refuel ships at your docks. Build fuel factory and produce 100 points of fuel to help exploring the Last Sector";
                                    q.steps[0] = "Collect 100 fuel ";
                                    break;
                                case ProgressQuestID.Progress_XStation:
                                    name = "Experimental prognosis";
                                    description = "The Last Sector is an unpredictable place. Organise your own meteorologist team to foresee threats";
                                    q.steps[0] = "Build " + GetStructureName(Structure.XSTATION_3_ID);
                                    break;
                                case ProgressQuestID.Progress_Tier4:
                                    name = "Technology progress III";
                                    description = "Upgrade your HQ to level 4";
                                    q.steps[0] = "Upgrade HQ to level 4";
                                    break;
                                case ProgressQuestID.Progress_CoveredFarm:
                                    name = "Covered field";
                                    description = "Try out new covered hydroponics farm! It stabilizes food supply and reduces lifepower consumption.";
                                    q.steps[0] = "Build " + GetStructureName(Structure.COVERED_FARM); ;
                                    break;
                                case ProgressQuestID.Progress_CoveredLumbermill:
                                    name = "Covered forest";
                                    description = "Suddenly hydroponics lumbermill! Stabilizes lumber supply and reduces lifepower consumption.";
                                    q.steps[0] = "Build " + GetStructureName(Structure.COVERED_LUMBERMILL); ;
                                    break;
                                case ProgressQuestID.Progress_Reactor:
                                    name = "Power well";
                                    description = "Built a massive graphonium reactor";
                                    q.steps[0] = "Build " + GetStructureName(Structure.GRPH_REACTOR_4_ID); ;
                                    break;
                                case ProgressQuestID.Progress_FirstExpedition:
                                    name = "Brave explorers";
                                    description = "Initialize and succeed your first expedition in the mysterious Last Sector. For that, you should assemble a team in the recruiting center, construct a shuttle for them and prepare the new expedition in the Expedition corpus.";
                                    q.steps[0] = "Crew assembled ";
                                    q.steps[1] = "Shuttle constructed ";
                                    q.steps[2] = "Observatory built ";
                                    q.steps[3] = "Transmitter built ";
                                    q.steps[4] = "Expedition launched ";
                                    q.steps[5] = "Expedition succeed ";
                                    break;
                                case ProgressQuestID.Progress_Tier5:
                                    name = "Technology progress IV";
                                    description = "Upgrade your HQ to level 5";
                                    q.steps[0] = "Upgrade HQ to level 5";
                                    break;
                                case ProgressQuestID.Progress_FactoryComplex:
                                    name = "Complex factory <in development>";
                                    description = "Construct factory onto factory block to make a combined factory";
                                    q.steps[0] = "Factory block constructed ";
                                    q.steps[1] = "Factory over it completed ";
                                    break;
                                case ProgressQuestID.Progress_SecondFloor:
                                    name = "Second floor";
                                    description = "Construct a building onto the column";
                                    q.steps[0] = "Column constructed ";
                                    q.steps[1] = "Building over it completed ";
                                    break;
                                case ProgressQuestID.Progress_FoodStocks:
                                    name = "Food reserve";
                                    description = "Gather food enough for a month";
                                    q.steps[0] = "Food on stocks: ";
                                    break;
                                default: return;
                            }
                            break;
                        case QuestType.Endgame:
                            switch ((Knowledge.ResearchRoute)q.subIndex)
                            {
                                case Knowledge.ResearchRoute.Foundation:
                                    {
                                        int b = Knowledge.R_F_QUEST_POPULATION_COND;
                                        name = "Foundation Route - Ending";
                                        description = "For gaining victory raise population to " + b.ToString();
                                        q.steps[0] = "Current population: " + GameMaster.realMaster.colonyController.citizenCount.ToString() + " / " + b.ToString();
                                        break;
                                    }
                                default: return;
                            }
                            break;
                        case QuestType.Foundation:
                            {
                                var colony = GameMaster.realMaster.colonyController;
                                switch ((Knowledge.FoundationRouteBoosters)q.subIndex)
                                {
                                    case Knowledge.FoundationRouteBoosters.HappinessBoost:
                                        name = "Foundation Route";
                                        description = "Citizens happiness and unity is the only one that keeps us together. Make sure our bounds will not be ripped!";
                                        q.steps[0] = "Satisfying level: ";
                                        break;
                                    case Knowledge.FoundationRouteBoosters.HotelBoost:
                                        {
                                            name = "Foundation Route";
                                            description = "For those who came from far lands our gates shall be open. Let the Hotel be built!";
                                            q.steps[0] = GetStructureName(Structure.HOTEL_BLOCK_6_ID) + " built ";
                                            break;
                                        }
                                    case Knowledge.FoundationRouteBoosters.HousingMastBoost:
                                        name = "Foundation Route";
                                        description = "We need more living space - higher, brighter, wider.";
                                        q.steps[0] = GetStructureName(Structure.HOUSING_MAST_6_ID) + " built ";
                                        break;
                                    case Knowledge.FoundationRouteBoosters.ImmigrantsBoost:
                                        name = "Foundation Route";
                                        description = "A lot of people waiting to join our city far away. Lets accept them as fast, as we can.";
                                        q.steps[0] = "Incomers count: ";
                                        break;
                                    case Knowledge.FoundationRouteBoosters.PointBoost:
                                        name = "Foundation Route";
                                        description = "";
                                        q.steps[0] = "Find another colony ";
                                        break;
                                    case Knowledge.FoundationRouteBoosters.PopulationBoost:
                                        name = "Foundation Route";
                                        description = "";
                                        q.steps[0] = "Current population: ";
                                        break;
                                    case Knowledge.FoundationRouteBoosters.QuestBoost:
                                        name = "Foundation Route";
                                        description = "";
                                        q.steps[0] = "";
                                        break;
                                    case Knowledge.FoundationRouteBoosters.SettlementBoost:
                                        name = "Foundation Route";
                                        description = "";
                                        q.steps[0] = GetStructureName(Structure.SETTLEMENT_CENTER_ID) + " level 6 built.";
                                        break;
                                }
                                break;
                            }
                        case QuestType.CloudWhale:
                            {
                                switch ((Knowledge.CloudWhaleRouteBoosters)q.subIndex)
                                {
                                    case Knowledge.CloudWhaleRouteBoosters.StreamGensBoost:
                                        name = "Cloud Whale Route";
                                        description = "Build " + Knowledge.R_CW_STREAMGENS_COUNT_COND.ToString() + " " + GetStructureName(Structure.WIND_GENERATOR_1_ID) + "s";
                                        q.steps[0] = "Stream gens: ";
                                        break;
                                    case Knowledge.CloudWhaleRouteBoosters.CrewsBoost:
                                        name = "Cloud Whale Route";
                                        description = "Prepare " + Knowledge.R_CW_CREWS_COUNT_COND.ToString() + " or more crews level " + Knowledge.R_CW_CREW_LEVEL_COND.ToString() + " for island guard.";
                                        q.steps[0] = "Crews count: ";
                                        break;
                                    case Knowledge.CloudWhaleRouteBoosters.ArtifactBoost:
                                        name = "Cloud Whale Route";
                                        description = "Find an artifact of the Tech Path";
                                        q.steps[0] = "Artifact found ";
                                        break;
                                    case Knowledge.CloudWhaleRouteBoosters.XStationBoost:
                                        name = "Cloud Whale Route";
                                        description = "Build " + GetStructureName(Structure.XSTATION_3_ID);
                                        q.steps[0] = "Station completed ";
                                        break;
                                    case Knowledge.CloudWhaleRouteBoosters.StabilityEnforcerBooster:
                                        name = "Cloud Whale Route";
                                        description = "Build " + GetStructureName(Structure.STABILITY_ENFORCER_ID);
                                        q.steps[0] = "Stabilizer built ";
                                        break;
                                        //case Knowledge.CloudWhaleRouteBoosters.QuestBoost:
                                }
                                break;
                            }
                        case QuestType.Engine:
                            {
                                switch ((Knowledge.EngineRouteBoosters)q.subIndex)
                                {
                                    case Knowledge.EngineRouteBoosters.EnergyBoost:
                                        name = "Engine Route";
                                        description = "Try to gain enough energy for your colony's effectiveness test.";
                                        q.steps[0] = "Energy stored: ";
                                        break;
                                    case Knowledge.EngineRouteBoosters.CityMoveBoost:
                                        name = "Engine Route";
                                        description = "Try to change city's positions in the Last Sector";
                                        q.steps[0] = "Island moved ";
                                        break;
                                    case Knowledge.EngineRouteBoosters.GearsBoost:
                                        name = "Engine Route";
                                        description = "Improve your island's industry gears";
                                        q.steps[0] = "Gears level ";
                                        break;
                                    case Knowledge.EngineRouteBoosters.FactoryBoost:
                                        name = "Engine Route";
                                        description = "We need massive industry for journey preparing";
                                        q.steps[0] = "Cube-smelteries built: ";
                                        break;
                                    case Knowledge.EngineRouteBoosters.IslandEngineBoost:
                                        name = "Engine Route";
                                        description = string.Empty;
                                        q.steps[0] = "Engine built ";
                                        break;
                                    case Knowledge.EngineRouteBoosters.ControlCenterBoost:
                                        name = "Engine Route";
                                        description = string.Empty;
                                        q.steps[0] = "Control center built ";
                                        break;
                                }
                                break;
                            }
                        case QuestType.Pipes:
                            {
                                switch ((Knowledge.PipesRouteBoosters)q.subIndex)
                                {
                                    case Knowledge.PipesRouteBoosters.FarmsBoost:
                                        name = "Pipes Route";
                                        description = "As a preparation, replace all your traditional farms to covered ones.";
                                        q.steps[0] = "Basic farms left: ";
                                        q.steps[1] = "Covered farms count: ";
                                        break;
                                    case Knowledge.PipesRouteBoosters.SizeBoost:
                                        {
                                            name = "Pipes Route";
                                            description = "A massive island obviously can not come through the Pipes Gate. Make sure you cut the edges. (Need two of three conditions met)";
                                            q.steps[0] = "Width: ";
                                            q.steps[1] = "Height: ";
                                            q.steps[2] = "Length: ";
                                            break;
                                        }
                                    case Knowledge.PipesRouteBoosters.FuelBoost:
                                        {
                                            name = "Pipes Route";
                                            description = "We need huge fuel reserves for the journey";
                                            q.steps[0] = "Fuel collected: ";
                                            break;
                                        }
                                    case Knowledge.PipesRouteBoosters.BiomesBoost:
                                        {
                                            name = "Pipes Route";
                                            description = "Prepare island for a long flight - visit four different biomes.";
                                            q.steps[0] = "Ocean visited ";
                                            q.steps[1] = "Flames visited ";
                                            q.steps[2] = "Outer space visited ";
                                            q.steps[3] = "Meadows visited ";
                                            break;
                                        }
                                    case Knowledge.PipesRouteBoosters.QETBoost:
                                        {
                                            name = "Pipes Route";
                                            description = "Crystals always will be useful. Build " + GetStructureName(Structure.QUANTUM_ENERGY_TRANSMITTER_5_ID);
                                            q.steps[0] = "Constructed ";
                                            break;
                                        }
                                    case Knowledge.PipesRouteBoosters.CapacitorMastBoost:
                                        {
                                            name = "Pipes Route";
                                            description = "Massive capacitor will be useful. Construct " + GetStructureName(Structure.CAPACITOR_MAST_ID);
                                            q.steps[0] = "Capacitor built ";
                                            break;
                                        }
                                }
                                break;
                            }
                        case QuestType.Crystal:
                            {
                                switch ((Knowledge.CrystalRouteBoosters)q.subIndex)
                                {
                                    case Knowledge.CrystalRouteBoosters.MoneyBoost:
                                        name = "Crystal Route";
                                        description = "Collect " + Knowledge.R_C_MONEY_COND.ToString() + " energy crystals.";
                                        q.steps[0] = "Crystals collected: ";
                                        break;
                                    case Knowledge.CrystalRouteBoosters.PinesBoost:
                                        name = "Crystal Route";
                                        description = "Grow up crystal pines on your island.";
                                        q.steps[0] = "First pine grown ";
                                        break;
                                    case Knowledge.CrystalRouteBoosters.GCubeBoost:
                                        name = "Crystal Route";
                                        description = "Build a block full of Graphonium as an catalyst.";
                                        q.steps[0] = "Graphonium block built ";
                                        break;
                                    case Knowledge.CrystalRouteBoosters.BiomeBoost:
                                        name = "Crystal Route";
                                        description = "Move your island to the Crystal biome.";
                                        q.steps[0] = "City is in the Crytal biome: ";
                                        break;
                                    case Knowledge.CrystalRouteBoosters.CrystalliserBoost:
                                        name = "Crystal Route";
                                        description = "Build " + GetStructureName(Structure.CRYSTALLISER_ID);
                                        q.steps[0] = "Constructed: ";
                                        break;
                                    case Knowledge.CrystalRouteBoosters.CrystalMastBoost:
                                        name = "Crystal Route";
                                        description = "Build " + GetStructureName(Structure.CRYSTAL_MAST_ID);
                                        q.steps[0] = "Constructed: ";
                                        break;
                                    case Knowledge.CrystalRouteBoosters.PointBoost:
                                        break;
                                }
                                break;
                            }
                        case QuestType.Monument:
                            {
                                switch ((Knowledge.MonumentRouteBoosters)q.subIndex)
                                {
                                    case Knowledge.MonumentRouteBoosters.MonumentAffectionBoost:
                                        {
                                            name = "Monument Route";
                                            description = "Build " + Knowledge.R_M_MONUMENTS_COUNT_COND.ToString() + " monuments of a Tech Path with affection value more than " + Knowledge.R_M_MONUMENTS_AFFECTION_CONDITION.ToString();
                                            q.steps[0] = "Monuments count: ";
                                            q.steps[1] = "With suitable affection: ";
                                            break;
                                        }
                                    case Knowledge.MonumentRouteBoosters.LifesourceBoost:
                                        {
                                            name = "Monument Route";
                                            description = "Add one more Lifepower Source to your island.";
                                            q.steps[0] = "Lifesources: ";
                                            break;
                                        }
                                    case Knowledge.MonumentRouteBoosters.BiomeBoost:
                                        {
                                            name = "Monument Route";
                                            description = "Move your city to a Ruins Biome.";
                                            q.steps[0] = "City moved: ";
                                            break;
                                        }
                                    case Knowledge.MonumentRouteBoosters.ExpeditionsBoost:
                                        {
                                            name = "Monument Route";
                                            description = "Successfully end more expeditions to gain this boost";
                                            q.steps[0] = "Expeditions succeed: ";
                                            break;
                                        }
                                    case Knowledge.MonumentRouteBoosters.MonumentConstructionBoost:
                                        {
                                            name = "Monument Route";
                                            description = "Build at least one " + GetStructureName(Structure.MONUMENT_ID);
                                            q.steps[0] = "Monument built: ";
                                            break;
                                        }
                                    case Knowledge.MonumentRouteBoosters.AnchorMastBoost:
                                        {
                                            name = "Monument Route";
                                            description = "Build an " + GetStructureName(Structure.ANCHOR_MAST_ID);
                                            q.steps[0] = "Anchor built: ";
                                            break;
                                        }
                                    case Knowledge.MonumentRouteBoosters.PointBoost:
                                        break;
                                }
                                break;
                            }
                        case QuestType.Blossom:
                            {
                                switch ((Knowledge.BlossomRouteBoosters)q.subIndex)
                                {
                                    case Knowledge.BlossomRouteBoosters.GrasslandsBoost:
                                        {
                                            name = "Blossom Route";
                                            description = "A lot of nature entities must be";
                                            q.steps[0] = "Grasslands percentage: ";
                                            break;
                                        }
                                    case Knowledge.BlossomRouteBoosters.ArtifactBoost:
                                        {
                                            name = "Blossom Route";
                                            description = "We need a special item";
                                            q.steps[0] = "Artifact of the Secret Path found: ";
                                            break;
                                        }
                                    case Knowledge.BlossomRouteBoosters.BiomeBoost:
                                        {
                                            name = "Blossom Route";
                                            description = "Our way to Blossom is foing through Forest Biome";
                                            q.steps[0] = "Biome found: ";
                                            break;
                                        }
                                    case Knowledge.BlossomRouteBoosters.Unknown:
                                        break;
                                    case Knowledge.BlossomRouteBoosters.GardensBoost:
                                        {
                                            break;
                                        }
                                    case Knowledge.BlossomRouteBoosters.HTowerBoost:
                                        {
                                            name = "Blossom Route";
                                            description = "Build " + GetStructureName(Structure.HANGING_TMAST_ID);
                                            q.steps[0] = "Constructed: ";
                                            break;
                                        }
                                    case Knowledge.BlossomRouteBoosters.PointBoost:
                                        break;
                                }
                                break;
                            }
                        case QuestType.Pollen:
                            {
                                switch ((Knowledge.PollenRouteBoosters)q.subIndex)
                                {
                                    case Knowledge.PollenRouteBoosters.FlowersBoost:
                                        {
                                            name = "Pollen Route";
                                            description = "Let flowers grow";
                                            q.steps[0] = "Pollen flowers on our island: ";
                                            break;
                                        }
                                    case Knowledge.PollenRouteBoosters.AscensionBoost:
                                        {
                                            name = "Pollen Route";
                                            description = "Achieve High Ascension level.";
                                            q.steps[0] = "Ascension level: ";
                                            break;
                                        }
                                    case Knowledge.PollenRouteBoosters.BiomeBoost:
                                        {
                                            name = "Pollen Route";
                                            description = "Move the city to the Pollen Biome";
                                            q.steps[0] = "On position ";
                                            break;
                                        }
                                    case Knowledge.PollenRouteBoosters.FilterBoost:
                                        {
                                            name = "Pollen Route";
                                            description = "Build " + GetStructureName(Structure.RESOURCE_FILTER_ID);
                                            q.steps[0] = "Constructed ";
                                            break;
                                        }
                                    case Knowledge.PollenRouteBoosters.ProtectorCoreBoost:
                                        {
                                            name = "Pollen Route";
                                            description = "Build " + GetStructureName(Structure.PROTECTION_CORE_ID);
                                            q.steps[0] = "Constructed ";
                                            break;
                                        }
                                    case Knowledge.PollenRouteBoosters.PointBoost:
                                        break;
                                }
                                break;
                            }
                      
                        default: return;
                    }
                }
                break;
        }
        q.FillText(name, description);
    }
    
}

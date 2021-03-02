public static partial class Localization
{
    public static void FillQuestData(Quest q)
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
                                    q.name = "Экспериментальный прогноз";
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
                                    q.description = "Попробуйте новую гидропонную ферму! Так вы стабилизируете производство пищи и снизите потребление жизненной энергии острова.";
                                    q.steps[0] = GetStructureName(Structure.COVERED_FARM) + " построена";
                                    break;
                                case ProgressQuestID.Progress_CoveredLumbermill:
                                    q.name = "Лес под крышей";
                                    q.description = "Внезапно, гидропонная лесопилк! Стабилизирует поток древесины и снизит расход жизненной силы острова.";
                                    q.steps[0] = GetStructureName(Structure.COVERED_LUMBERMILL) + " построена";
                                    break;
                                case ProgressQuestID.Progress_Reactor:
                                    q.name = "Источник энергии";
                                    q.description = "Постройте полноценный графониевый реактор.";
                                    q.steps[0] = GetStructureName(Structure.GRPH_REACTOR_4_ID) + " построен";
                                    break;
                                case ProgressQuestID.Progress_FirstExpedition:
                                    q.name = "Храбрые исследователи";
                                    q.description = "Подготовьте и успешно завершите экспедицию в загадочный Последний Сектор. Для этого вам нужно набрать команду, произвести для неё челнок, и сформировать экспедицию в Экспедиционном корпусе.";
                                    q.steps[0] = "Команда подготовлена ";
                                    q.steps[1] = "Челнок собран ";
                                    q.steps[2] = "Обсерватория построена";
                                    q.steps[3] = "Передатчик построен ";
                                    q.steps[4] = "Экспедиция запущена ";
                                    q.steps[5] = "Миссия завершена ";
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
                            switch ((Knowledge.ResearchRoute)q.subIndex)
                            {
                                case Knowledge.ResearchRoute.Foundation:
                                    {
                                        int b = Knowledge.R_F_QUEST_POPULATION_COND;
                                        q.name = "Путь Основания - Завершение";
                                        q.description = "Для победы по Пути Основания доведите популяцию до " + b.ToString();
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
                                        q.name = "Путь Основания";
                                        q.description = "";
                                        q.steps[0] = "Уровень довольства: ";
                                        break;
                                    case Knowledge.FoundationRouteBoosters.HotelBoost:
                                        {
                                            q.name = "Путь Основания";
                                            q.description = "";
                                            q.steps[0] = GetStructureName(Structure.HOTEL_BLOCK_6_ID) + " построен ";
                                            break;
                                        }
                                    case Knowledge.FoundationRouteBoosters.HousingMastBoost:
                                        q.name = "Путь Основания";
                                        q.description = "";
                                        q.steps[0] = GetStructureName(Structure.HOUSING_MAST_6_ID) + " построена ";
                                        break;
                                    case Knowledge.FoundationRouteBoosters.ImmigrantsBoost:
                                        q.name = "Путь Основания";
                                        q.description = "";
                                        q.steps[0] = "Количество прибывших: ";
                                        break;
                                    case Knowledge.FoundationRouteBoosters.PointBoost:
                                        q.name = "Путь Основания";
                                        q.description = "";
                                        q.steps[0] = "Найти другую Колонию ";
                                        break;
                                    case Knowledge.FoundationRouteBoosters.PopulationBoost:
                                        q.name = "Путь Основания";
                                        q.description = "";
                                        q.steps[0] = "Текущее население: ";
                                        break;
                                    case Knowledge.FoundationRouteBoosters.QuestBoost:
                                        q.name = "Путь Основания - Квест";
                                        q.description = "";
                                        q.steps[0] = "";
                                        break;
                                    case Knowledge.FoundationRouteBoosters.SettlementBoost:
                                        q.name = "Путь Основания";
                                        q.description = "";
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
                                        q.name = "Путь Облачного Кита";
                                        q.description = "Постройте " + GetStructureName(Structure.WIND_GENERATOR_1_ID) + " в количестве " + Knowledge.R_CW_STREAMGENS_COUNT_COND.ToString();
                                        q.steps[0] = "Потоковые генераторы: ";
                                        break;
                                    case Knowledge.CloudWhaleRouteBoosters.CrewsBoost:
                                        q.name = "Путь Облачного Кита";
                                        q.description = "Набрать не менее " + Knowledge.R_CW_CREWS_COUNT_COND.ToString() + " команд не ниже уровня " + Knowledge.R_CW_CREW_LEVEL_COND.ToString() + " для охраны острова.";
                                        q.steps[0] = "Команд собрано: ";
                                        break;
                                    case Knowledge.CloudWhaleRouteBoosters.ArtifactBoost:
                                        q.name = "Путь Облачного Кита";
                                        q.description = "Найдите артефакт Тайного Пути";
                                        q.steps[0] = "Артефакт найден ";
                                        break;
                                    case Knowledge.CloudWhaleRouteBoosters.XStationBoost:
                                        q.name = "Путь Облачного Кита";
                                        q.description = "Постройте следующее сооружение: " + GetStructureName(Structure.XSTATION_3_ID);
                                        q.steps[0] = "Станция построена ";
                                        break;
                                    case Knowledge.CloudWhaleRouteBoosters.StabilityEnforcerBooster:
                                        q.name = "Путь Облачного Кита";
                                        q.description = "Постройте следующее сооружение: " + GetStructureName(Structure.STABILITY_ENFORCER_ID);
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
                                        q.name = "Путь Движителя";
                                        q.description = "Попробуйте накопить достаточно энергии, чтобы оценить накопительный потенциал вашей колонии.";
                                        q.steps[0] = "Энергии накоплено: ";
                                        break;
                                    case Knowledge.EngineRouteBoosters.CityMoveBoost:
                                        q.name = "Путь Движителя";
                                        q.description = "Попробуйте изменить положение острова в Последнем Секторе.";
                                        q.steps[0] = "Остров сдвинулся ";
                                        break;
                                    case Knowledge.EngineRouteBoosters.GearsBoost:
                                        q.name = "Путь Движителя";
                                        q.description = "Улучшите оснащение своих фабрик и мастерских";
                                        q.steps[0] = "Уровень оснащения ";
                                        break;
                                    case Knowledge.EngineRouteBoosters.FactoryBoost:
                                        q.name = "Путь Движителя";
                                        q.description = "Нам потребуются немалые мощности, чтобы подготовить путешествие.";
                                        q.steps[0] = "Построено фабрик-кубов: ";
                                        break;
                                    case Knowledge.EngineRouteBoosters.IslandEngineBoost:
                                        q.name = "Путь Движителя";
                                        q.description = string.Empty;
                                        q.steps[0] = "Движитель построен ";
                                        break;
                                    case Knowledge.EngineRouteBoosters.ControlCenterBoost:
                                        q.name = "Путь Движителя";
                                        q.description = string.Empty;
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
                                        q.name = "Путь Труб";
                                        q.description = "В качестве подготовки, замените все оычные фермы крытыми";
                                        q.steps[0] = "Обычные фермы: ";
                                        q.steps[1] = "Крытые фермы: ";
                                        break;
                                    case Knowledge.PipesRouteBoosters.SizeBoost:
                                        {
                                            q.name = "Путь Труб";
                                            q.description = "Крупный объект не сможет пройти сквозь Врата Труб. Убедитесь, что ваш остров достаточно компактный (Достаточно выполнить два условия из трех).";
                                            q.steps[0] = "Ширина: ";
                                            q.steps[1] = "Высота: ";
                                            q.steps[2] = "Длина: ";
                                            break;
                                        }
                                    case Knowledge.PipesRouteBoosters.FuelBoost:
                                        {
                                            q.name = "Путь Труб";
                                            q.description = "Нас понадобится большой запас топлива для путешествия.";
                                            q.steps[0] = "Собрано топлива: ";
                                            break;
                                        }
                                    case Knowledge.PipesRouteBoosters.BiomesBoost:
                                        {
                                            q.name = "Путь Труб";
                                            q.description = "Подготовьте остров к дальнему путешествию - посетите 4 биома";
                                            q.steps[0] = "Видели океан ";
                                            q.steps[1] = "Видели огненный биом ";
                                            q.steps[2] = "Посетили внешний космос ";
                                            q.steps[3] = "Посетили луга ";
                                            break;
                                        }
                                    case Knowledge.PipesRouteBoosters.QETBoost:
                                        {
                                            q.name = "Путь Труб";
                                            q.description = "Энергокристаллы будут нужны всегда. Постройте " + GetStructureName(Structure.QUANTUM_ENERGY_TRANSMITTER_5_ID);
                                            q.steps[0] = "Строение готово ";
                                            break;
                                        }
                                    case Knowledge.PipesRouteBoosters.CapacitorMastBoost:
                                        {
                                            q.name = "Путь Труб";
                                            q.description = "Нам понадобится крупный накопитель энергии - " + GetStructureName(Structure.CAPACITOR_MAST_ID);
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
                                    q.description = "Digging left a lot of stone, still contained ores inside. Build an Ore Enricher to extract maximum resources amount.";
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
                                    q.description = "Try out new covered hydroponics farm! It stabilizes food supply and reduces lifepower consumption.";
                                    q.steps[0] = "Build " + GetStructureName(Structure.COVERED_FARM); ;
                                    break;
                                case ProgressQuestID.Progress_CoveredLumbermill:
                                    q.name = "Covered forest";
                                    q.description = "Suddenly hydroponics lumbermill! Stabilizaes lumber supply and reduces lifepower consumption.";
                                    q.steps[0] = "Build " + GetStructureName(Structure.COVERED_LUMBERMILL); ;
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
                                    q.steps[1] = "Shuttle constructed ";
                                    q.steps[2] = "Observatory built ";
                                    q.steps[3] = "Transmitter built ";
                                    q.steps[4] = "Expedition launched ";
                                    q.steps[5] = "Expedition succeed ";
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
                            switch ((Knowledge.ResearchRoute)q.subIndex)
                            {
                                case Knowledge.ResearchRoute.Foundation:
                                    {
                                        int b = Knowledge.R_F_QUEST_POPULATION_COND;
                                        q.name = "Foundation Route - Ending";
                                        q.description = "For gaining victory raise population to " + b.ToString();
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
                                        q.name = "Foundation Route";
                                        q.description = "Citizens happiness and unity is the only one that keeps us together. Make sure our bounds will not be ripped!";
                                        q.steps[0] = "Satisfying level: ";
                                        break;
                                    case Knowledge.FoundationRouteBoosters.HotelBoost:
                                        {
                                            q.name = "Foundation Route";
                                            q.description = "For those who came from far lands our gates shall be open. Let the Hotel be built!";
                                            q.steps[0] = GetStructureName(Structure.HOTEL_BLOCK_6_ID) + " built ";
                                            break;
                                        }
                                    case Knowledge.FoundationRouteBoosters.HousingMastBoost:
                                        q.name = "Foundation Route";
                                        q.description = "We need more living space - higher, brighter, wider.";
                                        q.steps[0] = GetStructureName(Structure.HOUSING_MAST_6_ID) + " built ";
                                        break;
                                    case Knowledge.FoundationRouteBoosters.ImmigrantsBoost:
                                        q.name = "Foundation Route";
                                        q.description = "A lot of people waiting to join our city far away. Lets accept them as fast, as we can.";
                                        q.steps[0] = "Incomers count: ";
                                        break;
                                    case Knowledge.FoundationRouteBoosters.PointBoost:
                                        q.name = "Foundation Route";
                                        q.description = "";
                                        q.steps[0] = "Find another colony ";
                                        break;
                                    case Knowledge.FoundationRouteBoosters.PopulationBoost:
                                        q.name = "Foundation Route";
                                        q.description = "";
                                        q.steps[0] = "Current population: ";
                                        break;
                                    case Knowledge.FoundationRouteBoosters.QuestBoost:
                                        q.name = "Foundation Route";
                                        q.description = "";
                                        q.steps[0] = "";
                                        break;
                                    case Knowledge.FoundationRouteBoosters.SettlementBoost:
                                        q.name = "Foundation Route";
                                        q.description = "";
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
                                        q.name = "Cloud Whale Route";
                                        q.description = "Build " + Knowledge.R_CW_STREAMGENS_COUNT_COND.ToString() + " " + GetStructureName(Structure.WIND_GENERATOR_1_ID) + "s";
                                        q.steps[0] = "Stream gens: ";
                                        break;
                                    case Knowledge.CloudWhaleRouteBoosters.CrewsBoost:
                                        q.name = "Cloud Whale Route";
                                        q.description = "Prepare " + Knowledge.R_CW_CREWS_COUNT_COND.ToString() + " or more crews level " + Knowledge.R_CW_CREW_LEVEL_COND.ToString() + " for island guard.";
                                        q.steps[0] = "Crews count: ";
                                        break;
                                    case Knowledge.CloudWhaleRouteBoosters.ArtifactBoost:
                                        q.name = "Cloud Whale Route";
                                        q.description = "Find an artifact of the Tech Path";
                                        q.steps[0] = "Artifact found ";
                                        break;
                                    case Knowledge.CloudWhaleRouteBoosters.XStationBoost:
                                        q.name = "Cloud Whale Route";
                                        q.description = "Build " + GetStructureName(Structure.XSTATION_3_ID);
                                        q.steps[0] = "Station completed ";
                                        break;
                                    case Knowledge.CloudWhaleRouteBoosters.StabilityEnforcerBooster:
                                        q.name = "Cloud Whale Route";
                                        q.description = "Build " + GetStructureName(Structure.STABILITY_ENFORCER_ID);
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
                                        q.name = "Engine Route";
                                        q.description = "Try to gain enough energy for your colony's effectiveness test.";
                                        q.steps[0] = "Energy stored: ";
                                        break;
                                    case Knowledge.EngineRouteBoosters.CityMoveBoost:
                                        q.name = "Engine Route";
                                        q.description = "Try to change city's positions in the Last Sector";
                                        q.steps[0] = "Island moved ";
                                        break;
                                    case Knowledge.EngineRouteBoosters.GearsBoost:
                                        q.name = "Engine Route";
                                        q.description = "Improve your island's industry gears";
                                        q.steps[0] = "Gears level ";
                                        break;
                                    case Knowledge.EngineRouteBoosters.FactoryBoost:
                                        q.name = "Engine Route";
                                        q.description = "We need massive industry for journey preparing";
                                        q.steps[0] = "Cube-smelteries built: ";
                                        break;
                                    case Knowledge.EngineRouteBoosters.IslandEngineBoost:
                                        q.name = "Engine Route";
                                        q.description = string.Empty;
                                        q.steps[0] = "Engine built ";
                                        break;
                                    case Knowledge.EngineRouteBoosters.ControlCenterBoost:
                                        q.name = "Engine Route";
                                        q.description = string.Empty;
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
                                        q.name = "Pipes Route";
                                        q.description = "As a preparation, replace all your traditional farms to covered ones.";
                                        q.steps[0] = "Basic farms left: ";
                                        q.steps[1] = "Covered farms count: ";
                                        break;
                                    case Knowledge.PipesRouteBoosters.SizeBoost:
                                        {
                                            q.name = "Pipes Route";
                                            q.description = "A massive island obviously can not come through the Pipes Gate. Make sure you cut the edges. (Need two of three conditions met)";
                                            q.steps[0] = "Width: ";
                                            q.steps[1] = "Height: ";
                                            q.steps[2] = "Length: ";
                                            break;
                                        }
                                    case Knowledge.PipesRouteBoosters.FuelBoost:
                                        {
                                            q.name = "Pipes Route";
                                            q.description = "We need huge fuel reserves for the journey";
                                            q.steps[0] = "Fuel collected: ";
                                            break;
                                        }
                                    case Knowledge.PipesRouteBoosters.BiomesBoost:
                                        {
                                            q.name = "Pipes Route";
                                            q.description = "Prepare island for a long flight - visit four different biomes.";
                                            q.steps[0] = "Ocean visited ";
                                            q.steps[1] = "Flames visited ";
                                            q.steps[2] = "Outer space visited ";
                                            q.steps[3] = "Meadows visited ";
                                            break;
                                        }
                                    case Knowledge.PipesRouteBoosters.QETBoost:
                                        {
                                            q.name = "Pipes Route";
                                            q.description = "Crystals always will be useful. Build " + GetStructureName(Structure.QUANTUM_ENERGY_TRANSMITTER_5_ID);
                                            q.steps[0] = "Constructed ";
                                            break;
                                        }
                                    case Knowledge.PipesRouteBoosters.CapacitorMastBoost:
                                        {
                                            q.name = "Pipes Route";
                                            q.description = "Massive capacitor will be useful. Construct " + GetStructureName(Structure.CAPACITOR_MAST_ID);
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
                                        q.name = "Crystal Route";
                                        q.description = "Collect " + Knowledge.R_C_MONEY_COND.ToString() + " energy crystals.";
                                        q.steps[0] = "Crystals collected: ";
                                        break;
                                    case Knowledge.CrystalRouteBoosters.PinesBoost:
                                        q.name = "Crystal Route";
                                        q.description = "Grow up crystal pines on your island.";
                                        q.steps[0] = "First pine grown ";
                                        break;
                                    case Knowledge.CrystalRouteBoosters.GCubeBoost:
                                        q.name = "Crystal Route";
                                        q.description = "Build a block full of Graphonium as an catalyst.";
                                        q.steps[0] = "Graphonium block built ";
                                        break;
                                    case Knowledge.CrystalRouteBoosters.BiomeBoost:
                                        q.name = "Crystal Route";
                                        q.description = "Move your island to the Crystal biome.";
                                        q.steps[0] = "City is in the Crytal biome: ";
                                        break;
                                    case Knowledge.CrystalRouteBoosters.CrystalliserBoost:
                                        q.name = "Crystal Route";
                                        q.description = "Build " + GetStructureName(Structure.CRYSTALLISER_ID);
                                        q.steps[0] = "Constructed: ";
                                        break;
                                    case Knowledge.CrystalRouteBoosters.CrystalMastBoost:
                                        q.name = "Crystal Route";
                                        q.description = "Build " + GetStructureName(Structure.CRYSTAL_MAST_ID);
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
                                            q.name = "Monument Route";
                                            q.description = "Build " + Knowledge.R_M_MONUMENTS_COUNT_COND.ToString() + " monuments of a Tech Path with affection value more than " + Knowledge.R_M_MONUMENTS_AFFECTION_CONDITION.ToString();
                                            q.steps[0] = "Monuments count: ";
                                            q.steps[1] = "With suitable affection: ";
                                            break;
                                        }
                                    case Knowledge.MonumentRouteBoosters.LifesourceBoost:
                                        {
                                            q.name = "Monument Route";
                                            q.description = "Add one more Lifepower Source to your island.";
                                            q.steps[0] = "Lifesources: ";
                                            break;
                                        }
                                    case Knowledge.MonumentRouteBoosters.BiomeBoost:
                                        {
                                            q.name = "Monument Route";
                                            q.description = "Move your city to a Ruins Biome.";
                                            q.steps[0] = "City moved: ";
                                            break;
                                        }
                                    case Knowledge.MonumentRouteBoosters.ExpeditionsBoost:
                                        {
                                            q.name = "Monument Route";
                                            q.description = "Successfully end more expeditions to gain this boost";
                                            q.steps[0] = "Expeditions succeed: ";
                                            break;
                                        }
                                    case Knowledge.MonumentRouteBoosters.MonumentConstructionBoost:
                                        {
                                            q.name = "Monument Route";
                                            q.description = "Build at least one " + GetStructureName(Structure.MONUMENT_ID);
                                            q.steps[0] = "Monument built: ";
                                            break;
                                        }
                                    case Knowledge.MonumentRouteBoosters.AnchorMastBoost:
                                        {
                                            q.name = "Monument Route";
                                            q.description = "Build an " + GetStructureName(Structure.ANCHOR_MAST_ID);
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
                                            q.name = "Blossom Route";
                                            q.description = "A lot of nature entities must be";
                                            q.steps[0] = "Grasslands percentage: ";
                                            break;
                                        }
                                    case Knowledge.BlossomRouteBoosters.ArtifactBoost:
                                        {
                                            q.name = "Blossom Route";
                                            q.description = "We need a special item";
                                            q.steps[0] = "Artifact of the Secret Path found: ";
                                            break;
                                        }
                                    case Knowledge.BlossomRouteBoosters.BiomeBoost:
                                        {
                                            q.name = "Blossom Route";
                                            q.description = "Our way to Blossom is foing through Forest Biome";
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
                                            q.name = "Blossom Route";
                                            q.description = "Build " + GetStructureName(Structure.HANGING_TMAST_ID);
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
                                            q.name = "Pollen Route";
                                            q.description = "Let flowers grow";
                                            q.steps[0] = "Pollen flowers on our island: ";
                                            break;
                                        }
                                    case Knowledge.PollenRouteBoosters.AscensionBoost:
                                        {
                                            q.name = "Pollen Route";
                                            q.description = "Ascend and Rise";
                                            q.steps[0] = "Ascension level: ";
                                            break;
                                        }
                                    case Knowledge.PollenRouteBoosters.BiomeBoost:
                                        {
                                            q.name = "Pollen Route";
                                            q.description = "Move the city to the Pollen Biome";
                                            q.steps[0] = "On position ";
                                            break;
                                        }
                                    case Knowledge.PollenRouteBoosters.FilterBoost:
                                        {
                                            q.name = "Pollen Route";
                                            q.description = "Build " + GetStructureName(Structure.RESOURCE_FILTER_ID);
                                            q.steps[0] = "Constructed ";
                                            break;
                                        }
                                    case Knowledge.PollenRouteBoosters.ProtectorCoreBoost:
                                        {
                                            q.name = "Pollen Route";
                                            q.description = "Build " + GetStructureName(Structure.PROTECTION_CORE_ID);
                                            q.steps[0] = "Constructed ";
                                            break;
                                        }
                                    case Knowledge.PollenRouteBoosters.PointBoost:
                                        break;
                                }
                                break;
                            }
                        case QuestType.Tutorial:
                            {
                                switch ((TutorialUI.TutorialStep)q.subIndex)
                                {
                                    case TutorialUI.TutorialStep.QuestShown:
                                        {
                                            q.name = "Quest System";
                                            q.description = "Yes, right there! When you will play, you can see other quests here. Now close this window to proceed - click the small cross in the right upper corner first time to close" +
                            " this quest description and second time to close the quests window. You can return here anytime in this tutorial to read the current step instruction again.";
                                            q.steps[0] = "Close quests window to proceed";
                                            break;
                                        }
                                    case TutorialUI.TutorialStep.CameraMovement:
                                        {
                                            q.name = "Camera movement";
                                            q.description = "If you have keyboard: \n" +
                                                "Use WASD to move, Ctrl to down, Space to up.\n" +
                                                "If you use touchscreen: \n" +
                                                "Use Control Cross in the bottom left corner to move, double-arrow buttons nearby to up and down.";
                                            q.steps[0] = "Try camera controls and then press Proceed button.";
                                            break;
                                        }
                                    case TutorialUI.TutorialStep.CameraRotation:
                                        {
                                            q.name = "Camera rotation";
                                            q.description = "If you have keyboard: \n" +
                                                "Hold middle mouse button and move the mouse\n" +
                                                "If you use touchscreen: \n" +
                                                "Slide through screen to rotate horizontally and vertically";
                                            q.steps[0] = "Try camera controls and then press Proceed button.";
                                            break;
                                        }
                                    case TutorialUI.TutorialStep.CameraSlicing:
                                        {
                                            q.name = "Slicing";
                                            q.description = "Use the Slice button to look through blocks. Click it second time to return to normal state.";
                                            q.steps[0] = "Try slicing and then press Proceed button.";
                                            break;
                                        }
                                    case TutorialUI.TutorialStep.Landing:
                                        {
                                            q.name = "Landing";
                                            q.description = "Select a three-blocks surface and then click on Land button.";
                                            q.steps[0] = "Colony founded";
                                            break;
                                        }
                                    case TutorialUI.TutorialStep.BuildWindmill_0:
                                        {
                                            q.name = "Build Stream generator";
                                            q.description = "Select highest surface possible, then click on BUILD button at right panel. In the list find " +
                                                 "Stream Generator and build it.";
                                            q.steps[0] = "Stream generator built ";
                                            break;
                                        }
                                    case TutorialUI.TutorialStep.GatherLumber:
                                        {
                                            q.name = "Gather lumber";
                                            q.description = "Select a surface with trees and click the GATHER button to take all lumber for that cell. You can add more worker to a worksite, just pressing the " +
                        "plus buttons in the right part of the appeared worksite window.";
                                            q.steps[0] = "Lumber collected ";
                                            break;
                                        }
                                    case TutorialUI.TutorialStep.BuildFarm:
                                        {
                                            q.name = "Build farm";
                                            q.description = "Build farm and assign no less than " + TutorialUI.FARM_QUEST_WORKERS_COUNT.ToString() + " workers.";
                                            q.steps[0] = "Farm built";
                                            q.steps[1] = "Workers assigned ";
                                            break;
                                        }
                                    case TutorialUI.TutorialStep.StoneDigging:
                                        {
                                            q.name = "Stone digging";
                                            q.description = "Click on any side of a stone block and the press the DIG button";
                                            q.steps[0] = "Stone collected ";
                                            break;
                                        }
                                    case TutorialUI.TutorialStep.SmelteryBuilding:
                                        {
                                            q.name = "Smeltery building";
                                            q.description = "Build a Smeltery for an access to crafting recipes. Also you can build second Stream generator for proper smeltery function.";
                                            q.steps[0] = "Smeltery built ";
                                            q.steps[1] = "(Additional) Stream generator built";
                                            break;
                                        }
                                    case TutorialUI.TutorialStep.RecipeExplaining_A:
                                        {
                                            q.name = "Resource producing";
                                            q.description = "Set in newfound Smeltery Stone-to-Lconcrete recipe and then power it up";
                                            q.steps[0] = "Recipe set ";
                                            q.steps[1] = "Powered up ";
                                            break;
                                        }
                                    case TutorialUI.TutorialStep.CollectConcrete:
                                        {
                                            q.name = "Collect concrete";
                                            q.description = "Collect enough l-concrete for dock building. Advice: increase smeltery's workers count and sure that there is enough power to operate.";
                                            q.steps[0] = "Concrete collected ";
                                            break;
                                        }
                                    case TutorialUI.TutorialStep.BuildDock:
                                        {
                                            q.name = "Dock building";
                                            q.description = "Find a place on a island shore with " + Dock.SMALL_SHIPS_PATH_WIDTH.ToString() + 'x' + Dock.SMALL_SHIPS_PATH_WIDTH.ToString() +
                                                " space to allow trade ships to dock. Rotate dock manually if it faces wrong, using small arrows at the top of its observer panel. /n" +
                                                "If you placed dock wrong, use button in top-right corner to demolish it - in this scenario your resources will be returned.";
                                            q.steps[0] = "Dock built ";
                                            q.steps[1] = "Dock is working ";
                                            break;
                                        }
                                }
                                break;
                            }
                        default: return;
                    }
                }
                break;
        }
    }
    public static void GetTutorialInfo(TutorialUI.TutorialStep step, ref string label, ref string description)
    {
        switch (step)
        {
            case TutorialUI.TutorialStep.Intro:
                {
                    label = "Welcome to Limited Worlds!";
                    description = "Glad to see you in my real-time city building game. Here will be a short instruction how it is works. In this tutorial mode " +
                        "advices and goals will be doubled in the quests menu for your convenience. Click on quests button after this window closes!";
                    return;
                }
            case TutorialUI.TutorialStep.CameraMovement:
                {
                    label = "Camera controls";
                    if (FollowingCamera.touchscreen)
                    {
                        description = "Before we start the main tutorial lets talk about camera controls. My game supports both touchscreen and keyboard input." +
                            "At this moment works touchscreen input - note the controlling cross in the left bottom corner. It is using for shift camera. To up and down use the double-arrow buttons nearby. Press the Proceed butoon when you'll be ready to continue.";
                    }
                    else
                    {
                        description = "Before we start the main tutorial lets talk about camera controls. My game supports both touchscreen and keyboard input." +
                          "At this moment works keyboard input. Use standart WASD controls to shift camera, Ctrl to down and Space to up. Press the Proceed button when you'll be ready to continue.";
                    }
                    return;
                }
            case TutorialUI.TutorialStep.CameraRotation:
                {
                    label = "Camera controls";
                    if (FollowingCamera.touchscreen)
                    {
                        description = "You can rotate camera just sliding your finger across the screen horizontally and vertically.";
                    }
                    else
                    {
                        description = "Rotate camera by holding middle mouse button and move the mouse itself.";
                    }
                    return;
                }
            case TutorialUI.TutorialStep.CameraSlicing:
                {
                    label = "Camera controls";
                    description = "You may also need to look through blocks. You can slice terrain by using specified button. A number displays the height of current block. Click the button second time to off the slicing mode.";
                    return;
                }
            case TutorialUI.TutorialStep.Landing:
                {
                    label = "First landing";
                    description = "And now we will found a new colony on this island. At first, you shall select a place to land your colony's zeppelin. " +
                        "Where it lands will be a main colony building, a storage building, and a residential area. Click on selected surface and then press Land button.";
                    break;
                }
            case TutorialUI.TutorialStep.Interface_People:
                {
                    label = "Interface";
                    description = "Numbers in upper left upper part of the screen displays main colony characteristics. On the up as citizens count, represented by three numbers: " +
                        "free workers count / total citizens count / housing space. If housing space is not enough, your citizens moral will decrease.";
                    break;
                }
            case TutorialUI.TutorialStep.Interface_Electricity:
                {
                    label = "Interface";
                    description = "This values displays Power situation. They are power stored / power capacity / power surplus. All buildings require power supply, and it will be not enough, some of your structures go malfunction." +
                        " Power generators and power capacitors marked in buildings list with a small lighting icon.";
                    break;
                }
            case TutorialUI.TutorialStep.BuildWindmill_0:
                {
                    label = "Electricity producing";
                    description = "So now we will build first colony's generator - a Stream Generator, looking like an ordinary wind turbine. Select highest surface possible, then click on BUILD button at right panel. In the list find " +
                        "Stream Generator and build it.";
                    break;
                }
            case TutorialUI.TutorialStep.BuildWindmill_1:
                {
                    label = "Electricity producing";
                    description = "Now you can notice that electricity supply value is positive. Stream generators is not very reliable sources however - power of Stream may be lower or higher sometimes, but it doesnt need any workers to operate. Just dont forget to place it as" +
                        " high as possible.";
                    break;
                }
            case TutorialUI.TutorialStep.GatherLumber:
                {
                    label = "Gather resources";
                    description = "After had constructed the power generator, you shall build a food source for colonists. But start resources as for Stream Generator not contain enough raw materials, " +
                        "because you can gather it on island. Select a surface with trees and click the GATHER button to take all lumber for that cell. You can add more worker to a worksite, just pressing the " +
                        "plus buttons in the right part of the appeared worksite window.";
                    break;
                }
            case TutorialUI.TutorialStep.BuildFarm:
                {
                    label = "Farm construction";
                    description = "Now, using collected lumber, build a farm and assign at least " + TutorialUI.FARM_QUEST_WORKERS_COUNT.ToString() + " workers on it. Note that farm cannot be built on stone surfaces.";
                    break;
                }
            case TutorialUI.TutorialStep.StoneDigging:
                {
                    label = "Farm construction completed";
                    description = "Good, the farm is now working! After lumber and food we shall develop underground deposits. Select any side of a stone blocks and click the DIG button.";
                    break;
                }
            case TutorialUI.TutorialStep.StorageLook:
                {
                    label = "Storage system";
                    description = "Rocks, drilled by your colonist, consist not only of stone, but a different ores too. Click on the Storage Button to see all of your colony's resources. Click again to close it after.";
                    break;
                }
            case TutorialUI.TutorialStep.SmelteryBuilding:
                {
                    label = "Ores";
                    description = "Note all resources with an Ores word. For using them as a component of your buildings and machines, they firstly need to be smelted. So our next step is to build Smeltery!";
                    break;
                }
            case TutorialUI.TutorialStep.RecipeExplaining_A:
                {
                    label = "Crafting";
                    description = "Now open the factory window and at the lower part of right window click the No Recipe button and change it to Stone - L-Concrete.";
                    break;
                }
            case TutorialUI.TutorialStep.RecipeExplaining_B:
                {
                    label = "Crafting II";
                    description = "Now press the small lightning icon on the top of the right panel to start processing!;";
                    break;
                }
            case TutorialUI.TutorialStep.CollectConcrete:
                {
                    label = "Prepare to build a dock";
                    description = "Concrete production is first need for building a dock. It is very important building, allows you to trade, increase population and upgrade to tier 2. " +
                        "Collect " + ResourcesCost.DOCK_CONCRETE_COSTVOLUME.ToString() + " L-concrete to build a new dock!";
                    break;
                }
            case TutorialUI.TutorialStep.BuildDock:
                {
                    label = "Dock building conditions";
                    var sz = Dock.SMALL_SHIPS_PATH_WIDTH.ToString();
                    description = "For proper functionality your dock must be build on the edge to be able to serve ships. For first level ships it needs " + sz + 'x' + sz + " corridor. If dock " +
                        "is placed correctly, all path will be filled with little stars, otherwise you will see a red box when select the dock. If dock is rotated wrongly, you can rotate it manually, use " +
                        "small arrows at the top of right panel.";
                    break;
                }
            case TutorialUI.TutorialStep.Immigration:
                {
                    label = "Immigration";
                    description = "Functional dock allows you to bring new colonists to your island. Use input field at the immigration tab to set how many colonists you want to invite. However, new people wont come," +
                        " if your colony haven't got enough houses or have low morale.";
                    break;
                }
            case TutorialUI.TutorialStep.Trade:
                {
                    label = "Trading";
                    description = "And this is a trading tab. You can select any of available resources and set its trade status - no action, sell (arrow down), buy (arrow up). The number means limit " +
                        "for buy and sell operations - dock will buy/sell until resources count wont reach it";
                    break;
                }
        }
    }
}

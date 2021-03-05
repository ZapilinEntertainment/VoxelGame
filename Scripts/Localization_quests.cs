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
                      
                        default: return;
                    }
                }
                break;
        }
    }
    
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Language{English, Russian};

public static class Localization {
	public static string detail_shuttleHull, detail_navigatingSystem, detail_smallPropulsionSystem, detail_smallThruster;
	public static string rtype_nothing_name, rtype_nothing_descr, rtype_lumber_name, rtype_lumber_descr, rtype_stone_name, rtype_stone_descr,
	rtype_dirt_name, rtype_dirt_descr, rtype_food_name, rtype_food_descr, rtype_metalK_ore_name, rtype_metalK_descr, rtype_metalM_ore_name, rtype_metalM_descr,
	rtype_metalE_ore_name, rtype_metalE_descr, rtype_metalN_ore_name, rtype_metalN_descr, rtype_metalP_ore_name, rtype_metalP_descr,
	rtype_metalS_ore_name, rtype_metalS_descr, rtype_mineralF_descr, rtype_mineralL_descr, rtype_elasticMass_descr, rtype_concrete_name, rtype_concrete_descr,
	rtype_fertileSoil_name, rtype_fertileSoil_descr, rtype_fuel_name, rtype_fuel_descr;
	public static string ui_build, ui_dig_block, ui_pourIn, ui_clear, ui_storage_name, ui_stopWork, 
	ui_accept_destruction_on_clearing, ui_accept, ui_decline, ui_choose_block_action, ui_toPlain, ui_toGather, ui_cancelGathering, ui_workers, 
	ui_dig_in_progress, ui_clean_in_progress, ui_gather_in_progress, ui_pouring_in_progress, ui_activeSelf, ui_immigration, ui_trading, ui_buy, ui_sell,
	ui_selectResource, ui_immigrationEnabled, ui_immigrationDisabled, ui_immigrationMonthLimit, ui_close, ui_reset, ui_add_transaction, ui_setMode, ui_currentMode;
	public static string menu_colonyInfo, menu_gameMenuButton, menu_cancel;
	public static string info_housing, info_population, info_level, info_gearsCoefficient, info_hospitalsCoverage, info_happiness, info_health,
	info_birthrate;
	public static string announcement_powerFailure, announcement_starvation, announcement_peopleArrived, announcement_notEnoughResources;
	public static string objects_left, extracted, work_has_stopped, sales_volume, min_value_to_trade;
	public static string[] structureName;
	public static string hq_refuse_reason_1, hq_refuse_reason_2, lowered_birthrate, normal_birthrate, improved_birthrate, material_required, no_activity;
	public static string rollingShop_gearsProduction, rollingShop_boatPartsProduction;

	static Localization() {
		structureName = new string[28];
		structureName[0] = "<empty>";
		ChangeLanguage(Language.English);
	}

	public static void ChangeLanguage(Language lan ) {
		switch (lan) {
		case Language.English:
			rtype_nothing_name = "Nothing";
			rtype_nothing_descr = "You shouldn't see this, it's a bug(";
			rtype_dirt_name = "Dirt";
			rtype_dirt_descr = "Organic cover of floating islands.";
			rtype_food_name = "Food";
			rtype_food_descr = "Organic fuel for your citizens.";
			rtype_lumber_name = "Wood";
			rtype_lumber_descr = "Different elastic wood, growing only in Last Sector Dominion. Used for building and sometimes decorating.";
			rtype_stone_name = "Stone";
			rtype_stone_descr = "Nature material used in construction. Processing into L-Concrete.";
			rtype_metalK_ore_name = "Metal K (ore)";
			rtype_metalK_descr = "Used in construction.";
			rtype_metalM_ore_name = "Metal M (ore)";
			rtype_metalM_descr = "Used in  machinery building.";
			rtype_metalE_ore_name = "Metal E (ore)";
			rtype_metalE_descr = "Used in electronic components production.";
			rtype_metalN_ore_name = "Metal N (ore)";
			rtype_metalN_descr = "Rare and expensive metal.";
			rtype_metalP_ore_name = "Metal P (ore)";
			rtype_metalP_descr = "Used in mass-production.";
			rtype_metalS_ore_name = "Metal S (ore)";
			rtype_metalS_descr = "Used in ship building.";
			rtype_mineralF_descr = "Used as fuel.";
			rtype_mineralL_descr = "Used to create elastic mass.";
			rtype_elasticMass_descr = "Easy-forming by special influence relatively tough material, used for building and manufacturing";
			rtype_concrete_name = "L-Concrete"; rtype_concrete_descr = "Comfortable and easy-forming building material.";
			rtype_fertileSoil_name = "Fertile Soil"; rtype_fertileSoil_descr = "Soil, appliable for growing edibles.";
			rtype_fuel_name = "Fuel"; rtype_fuel_descr = "Standart fuel for spaceship engine";


			structureName[1] = "Tree sapling"; structureName[2] = "Tree";
			structureName[3] = "Dead tree"; structureName[4] = "Wheat crop";
			structureName[5] = "Landed Zeppelin"; structureName[6] = "Tree of Life";
			structureName[7] = "Storage"; structureName[8] = "Pile";
			structureName[9] = "Mine elevator"; structureName[10] = "Lifestone";
			structureName[11] = "House";structureName[12] = "Boulder";
			structureName[13] = "Dock";structureName[14] = "Energy capacitor";
			structureName[15] = "Farm";structureName[16] = "Head Quarters";
			structureName[17] = "Lumbermill";structureName[18] = "Mine";
			structureName[19] ="Smeltery";structureName[20] = "Wind generator";
			structureName[21] = "Hospital";structureName[22] = "Biogenerator";
			structureName[23] = "Mineral F Powerplant"; structureName[24] = "Ore enricher";
			structureName[25] = "Rolling shop"; structureName[26] = "Mini Grph-reactor";
			structureName[27] = "Fuel facility";

			detail_navigatingSystem = "Navigating system"; detail_shuttleHull = "Shuttle hull";
			detail_smallPropulsionSystem = "Small propulsion system"; detail_smallThruster = "Small thruster"; 

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
			ui_immigrationEnabled = "Immigration allowed"; ui_immigrationDisabled = "Immigration is not possible"; ui_immigrationMonthLimit = "Immigrants month limit";
			ui_close = "Close"; ui_reset = "Reset";
			ui_setMode = "Set mode"; ui_currentMode = "Current mode";

			menu_colonyInfo = "Colony info"; menu_gameMenuButton = "Game menu"; menu_cancel = "Cancel";
			info_housing = "Housing";
			info_population = "Population";
			info_level = " lvl.";
			info_gearsCoefficient = "Gears";
			info_happiness = "Happiness";
			info_hospitalsCoverage = "Hospitals coverage";
			info_health = "Health situation";
			info_birthrate = "Birthrate";

			announcement_powerFailure = "Power Failure";
			announcement_starvation = "People are starving!";
			announcement_peopleArrived = "New colonists arrived";
			announcement_notEnoughResources = "Not enough resources";

			objects_left = "objects left";
			extracted = "extracted";
			work_has_stopped = "Work has stopped";
			sales_volume = "Sales volume";
			min_value_to_trade = "Limit";
			hq_refuse_reason_1 = "No docks built";
			hq_refuse_reason_2 = "No rolling shops built";
			lowered_birthrate = "Low birthrate";
			normal_birthrate = "Normal birthrate";
			improved_birthrate = "Stimulate birthrate";
			material_required = "Required material: ";
			no_activity = "No activity";

			rollingShop_gearsProduction = "Gears production";
			rollingShop_boatPartsProduction = "Boat parts production";
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
			ui_immigrationEnabled = "Иммиграция разрешена"; ui_immigrationDisabled = "Въезд в город закрыт"; ui_immigrationMonthLimit = "Максимум поселенцев в месяц";
			ui_close = "Закрыть"; ui_reset = "Сброс";
			ui_setMode = "Изменить режим"; ui_currentMode = "Текущий режим";

			menu_colonyInfo = "Состояние";
			info_housing = "Свободное жильё";
			info_population = "Население"; info_level = " ур.";
			info_gearsCoefficient = "Техническое оснащение";
			menu_gameMenuButton = "Меню"; menu_cancel = "Отмена";

			announcement_powerFailure = "Энергоснабжение нарушено!";
			announcement_starvation = "Закончилась провизия!";
			announcement_peopleArrived = "Прибыли новые поселенцы";
			announcement_notEnoughResources = "Недостаточно ресурсов!";

			objects_left = "осталось";
			extracted = "извлечено";
			work_has_stopped = "Работы остановлены";
			sales_volume = "Объём продажи";
			min_value_to_trade = "Ограничение";
			no_activity = "Бездействует"; // not a developer status!!!

			rollingShop_gearsProduction = "Производство оборудования";
			rollingShop_boatPartsProduction = "Производство комплектующих для шаттлов";
			break;	
		}
	}
}

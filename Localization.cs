using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Language{English, Russian};

public static class Localization {
	public static string rtype_nothing_name, rtype_nothing_descr, rtype_lumber_name, rtype_lumber_descr, rtype_stone_name, rtype_stone_descr,
	rtype_dirt_name, rtype_dirt_descr, rtype_food_name, rtype_food_descr, rtype_metalK_ore_name, rtype_metalK_descr, rtype_metalM_ore_name, rtype_metalM_descr,
	rtype_metalE_ore_name, rtype_metalE_descr, rtype_metalN_ore_name, rtype_metalN_descr, rtype_metalP_ore_name, rtype_metalP_descr,
	rtype_metalS_ore_name, rtype_metalS_descr, rtype_mineralF_descr, rtype_mineralL_descr, rtype_elasticMass_descr;
	public static string ui_build, ui_dig_block, ui_pourIn, ui_clear, ui_storage_name, ui_cancel_clearing, ui_cancel_digging, ui_cancel_pouring, ui_clear_and_dig,
	ui_accept_destruction_on_clearing, ui_accept, ui_decline, ui_choose_block_action, ui_toPlain, ui_toGather, ui_cancelGathering;
	public static string menu_colonyInfo, menu_gameMenuButton, menu_cancel;
	public static string info_housing, info_population, info_level;

	static Localization() {
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
			rtype_food_descr = "Organic fuel for your citizens";
			rtype_lumber_name = "Wood";
			rtype_lumber_descr = "Different elastic wood, growing only in Last Sector Dominion. Used for building and decorating";
			rtype_stone_name = "Stone";
			rtype_stone_descr = "Nature material used in construction";
			rtype_metalK_ore_name = "Metal K (ore)";
			rtype_metalK_descr = "Used in construction";
			rtype_metalM_ore_name = "Metal M (ore)";
			rtype_metalM_descr = "Used in  machinery building";
			rtype_metalE_ore_name = "Metal E (ore)";
			rtype_metalE_descr = "Used in electronic components production";
			rtype_metalN_ore_name = "Metal N (ore)";
			rtype_metalN_descr = "Rare and expensive metal";
			rtype_metalP_ore_name = "Metal P (ore)";
			rtype_metalP_descr = "Used in mass-production";
			rtype_metalS_ore_name = "Metal S (ore)";
			rtype_metalS_descr = "Used in ship building";
			rtype_mineralF_descr = "Used as fuel";
			rtype_mineralL_descr = "Used to create elastic mass";
			rtype_elasticMass_descr = "Easy-forming by special influence relatively tough material, used for building and manufacturing";

			ui_build = "Build"; ui_clear = "Clear"; ui_dig_block = "Dig block"; ui_pourIn = "Pour in";
			ui_storage_name = "Storage"; 
			ui_cancel_clearing = "Cancel clearing"; ui_cancel_digging = "Stop digging"; ui_cancel_pouring = "Stop pouring";
			ui_clear_and_dig = "Clear and dig"; ui_accept_destruction_on_clearing = "Destruct all buildings up there?";
			ui_accept = "Yes"; ui_decline = "No";
			ui_choose_block_action = "Choose block action";
			ui_toPlain = "Plain ground"; 
			ui_toGather = "Gather resources"; ui_cancelGathering = "Stop gathering";

			menu_colonyInfo = "Colony info"; menu_gameMenuButton = "Game menu"; menu_cancel = "Cancel";
			info_housing = "Housing";
			info_population = "Population";
			info_level = "lvl.";
			break;
		case  Language.Russian: 
			ui_storage_name = "Склад";
			ui_accept_destruction_on_clearing = "Снести все здания на поверхности?";
			ui_choose_block_action = "Выберите действие с блоком";
			ui_dig_block = "Выкопать блок";
			ui_pourIn = "Засыпать блок";
			ui_toPlain = "Разровнять";
			ui_toGather = "Собрать ресурсы"; ui_cancelGathering = "Остановить сбор ресурсов";
			menu_colonyInfo = "Состояние";
			info_housing = "Свободное жильё";
			info_population = "Население"; info_level = "ур.";
			menu_gameMenuButton = "Меню"; menu_cancel = "Отмена";
			break;	
		}
	}
}

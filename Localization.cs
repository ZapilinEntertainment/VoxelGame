using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Language{English, Russian};

public static class Localization {
	public static string rtype_nothing_name, rtype_nothing_descr, rtype_lumber_name, rtype_lumber_descr, rtype_stone_name, rtype_stone_descr,
	rtype_dirt_name, rtype_dirt_descr, rtype_food_name, rtype_food_descr, rtype_metalC_ore_name, rtype_metalC_descr, rtype_metalM_ore_name, rtype_metalM_descr,
	rtype_metalE_ore_name, rtype_metalE_descr, rtype_metalN_ore_name, rtype_metalN_descr, rtype_metalP_ore_name, rtype_metalP_descr,
	rtype_metalS_ore_name, rtype_metalS_descr, rtype_mineralF_descr, rtype_mineralL_descr, rtype_elasticMass_descr;
	public static string ui_build, ui_dig, ui_pourIn, ui_clear;

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
			rtype_lumber_name = "Used for building and decorating";
			rtype_stone_name = "Stone";
			rtype_stone_descr = "Nature material used in construction";
			rtype_metalC_ore_name = "Metal C (ore)";
			rtype_metalC_descr = "Used in construction";
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

			ui_build = "Build"; ui_clear = "Clear"; ui_dig = "Dig"; ui_pourIn = "Pour in";
			break;
		case  Language.Russian: 
			
			break;	
		}
	}
}

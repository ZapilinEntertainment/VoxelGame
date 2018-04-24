using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detail  {
	public string name{get;private set;}
	public int ID {get; private set;}
	public byte requiredLevel {get; private set;}
	public Texture icon{get; private set;}

	public static List<Detail> detailsList{get; private set;}

	public Detail( string f_name, int f_id, byte f_reqLevel, Texture f_icon ) {
		name = f_name;
		ID = f_id;
		requiredLevel = f_reqLevel;
		icon = f_icon;
	}

	static Detail() {
		detailsList = new List<Detail>();
		detailsList.Add( new Detail("Nothing", 0, 0, PoolMaster.empty_tx) );
		detailsList.Add ( new Detail ( Localization.detail_navigatingSystem, 1, 2, Resources.Load<Texture>("Textures/detail_navSystem")));
		detailsList.Add ( new Detail ( Localization.detail_shuttleHull, 2, 2, Resources.Load<Texture>("Textures/detail_shuttleHull")));
		detailsList.Add ( new Detail ( Localization.detail_smallPropulsionSystem, 3, 2, Resources.Load<Texture>("Textures/detail_propSystem_small")));
		detailsList.Add ( new Detail ( Localization.detail_smallThruster, 4, 2, Resources.Load<Texture>("Textures/detail_thruster_small")));
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour {
	 static  GameMaster realMaster;

	public GameMaster () {
		if (realMaster != null) realMaster = null;
		realMaster = this;
	}
		
}

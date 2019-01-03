using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineElevator : Structure {
	//--------SAVE  SYSTEM-------------
	public override List<byte> Save() { // cause serializing throw mine
		return null;
	} 
}

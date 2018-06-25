using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineElevator : Structure {
	//--------SAVE  SYSTEM-------------
	public override StructureSerializer Save() { // cause serializing throw mine
		return null;
	} 
	public StructureSerializer GetSerializer() { // for mine serializing
		return GetStructureSerializer();
	}
	// ------------------------------------------
}

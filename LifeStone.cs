using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeStone : Structure {

	override public void SetBasement(Block b) {
		basement = b;
		b.myChunk.AddLifePower(GameMaster.START_LIFEPOWER);
	}
}

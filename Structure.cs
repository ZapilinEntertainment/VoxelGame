using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure : MonoBehaviour {
	protected Block basement;
	protected float hp = 1, maxHp = 1;
	public int height = 1;
	int [,,] innerBlocks;


	virtual public void SetBasement(Block b) {basement = b;}
}

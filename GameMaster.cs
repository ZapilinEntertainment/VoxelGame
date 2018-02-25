using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour {
	 public static  GameMaster realMaster;
	public static float gameSpeed {get;private set;} 
	List<Block> daytimeUpdates_blocks;
	public const int LIFEPOWER_SPREAD_SPEED = 10, START_LIFEPOWER = 6144;
	public float lifeGrowCoefficient {get;private set;}

	public GameMaster () {
		if (realMaster != null) realMaster = null;
		realMaster = this;
		daytimeUpdates_blocks = new List<Block>();
		lifeGrowCoefficient = 1;
	}

	void Awake() {
		gameSpeed = 1;
	}

	public void AddBlockToDaytimeUpdateList(Block b) {
		if (b == null) return;
		daytimeUpdates_blocks.Add(b);
		b.daytimeUpdatePosition = daytimeUpdates_blocks.Count - 1;
	}

	public void RemoveBlockFromDaytimeUpdateList (int index) {
		if (daytimeUpdates_blocks[index] == null) return;
		daytimeUpdates_blocks[index].daytimeUpdatePosition = -1;
		daytimeUpdates_blocks[index] = daytimeUpdates_blocks[daytimeUpdates_blocks.Count - 1];
		daytimeUpdates_blocks.RemoveAt(daytimeUpdates_blocks.Count - 1);
	}
}

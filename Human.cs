using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum Activity {Idle,Moving,SearchingPlace, Working, Obtaining, Delivering, Resting}

public class Human : Entity {
	Structure home;
	Point currentPos, targetPos;
	Activity activity;
	byte a = 0, b = 0, x= 0, y= 0, z = 0;
	const float HUMAN_STAMINA = 100;

	void Awake() {
		GameMaster.realMaster.everydayUpdateList.Add(this);
		RaycastHit rh;
		if (Physics.Raycast(transform.position, Vector3.down, out rh)) {
			SurfaceBlock sb = rh.collider.transform.parent.GetComponent<SurfaceBlock>();
			if (sb != null) currentPos = new Point(sb, sb.WorldToLocalPosition(transform.position));
			else currentPos = Point.Empty;
		}
		stamina = HUMAN_STAMINA;
	}

	void EverydayUpdate() {
		if (home == null) {FindHome();}
	}

	void Update() {
		if (GameMaster.gameSpeed == 0) return;
		float t = Time.deltaTime * GameMaster.gameSpeed;
		if (currentPos == null) {
			transform.Translate(Vector3.down * 9.8f,Space.World);
			if (transform.position.y < GameMaster.CRITICAL_DEPTH) Destroy(gameObject);
		}
		else {
			switch (activity) {
			case Activity.SearchingPlace:
				Vector3 target = targetPos.block.transform.position;
				transform.position = Vector3.MoveTowards(transform.position, target, speed * t);
				if (transform.position == target && a == 1) {print ("house finding");FindHome();}
				break;
			}
		}
	}

	public void FindHome() {
		if (currentPos != Point.Empty) {
			PixelPosByte ps = currentPos.block.GetRandomPosition(2,2);
			print (ps.x.ToString() + ' ' + ps.y.ToString());
			if (ps == PixelPosByte.Empty) {
				List<SurfaceBlock> candidats = new List<SurfaceBlock>();
				int x = currentPos.block.pos.x, z = currentPos.block.pos.z;
				SurfaceBlock sb = currentPos.block.myChunk.GetSurfaceBlock(x+1,z);
				if (sb != null) candidats.Add(sb);
				sb = currentPos.block.myChunk.GetSurfaceBlock(x-1,z);
				if (sb != null) candidats.Add(sb);
				sb = currentPos.block.myChunk.GetSurfaceBlock(x,z+1);
				if (sb != null) candidats.Add(sb);
				sb = currentPos.block.myChunk.GetSurfaceBlock(x,z-1);
				if (sb != null) candidats.Add(sb);
				int ppos= (int)(Random.value * (candidats.Count - 1));
				//отправить в соседний чанк
				if (candidats.Count != 0) {
					activity = Activity.SearchingPlace;
					targetPos = new Point(candidats[ppos], new PixelPosByte((byte)(Random.value * (SurfaceBlock.INNER_RESOLUTION - 1)), (byte)(Random.value * (SurfaceBlock.INNER_RESOLUTION - 1))));
				}
				a = 1;
			}
			else {
				GameObject g = Instantiate(Resources.Load<GameObject>("Structures/House_level_0")) as GameObject;
				SurfaceRect sr = new SurfaceRect(ps.x,ps.y,2,2,Content.Structure,g);
				currentPos.block.AddStructure(sr);
				home = g.GetComponent<Structure>();
				activity = Activity.Idle;
				a= 0;
			}
		}
	}

	override public  void Die() {
		RaycastHit rh;
		if (Physics.Raycast(transform.position, Vector3.down, out rh)) {
			Block b = rh.collider.transform.parent.GetComponent<Block>();
			if (b != null) {
				Grassland gl = b.GetComponent<Grassland>();
				if (gl == null) b.myChunk.AddLifePower((int)lifepower);
				else gl.AddLifepower((int)lifepower);
			}
		}
		GameMaster.realMaster.everyYearUpdateList.Remove(this);

	}
}

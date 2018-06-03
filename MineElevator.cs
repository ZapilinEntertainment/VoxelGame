using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineElevator : Structure {
	//--------SAVE  SYSTEM-------------
	public virtual string Save() {
		return string.Empty;
	}
	//loading by mine object
	public virtual void Load(string s_data, Chunk c, SurfaceBlock surface) {
		byte x = byte.Parse(s_data.Substring(0,2));
		byte z = byte.Parse(s_data.Substring(2,2));
		Prepare();
		SetBasement(surface, new PixelPosByte(x,z));
		transform.localRotation = Quaternion.Euler(0, 45 * int.Parse(s_data[7].ToString()), 0);
		hp = int.Parse(s_data.Substring(8,3)) / 100f * maxHp;
	}
	// ------------------------------------------
}

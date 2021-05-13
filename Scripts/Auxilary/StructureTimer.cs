using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureTimer : MonoBehaviour {
    public float timer;
	
	
	// Update is called once per frame
	void Update () {
        timer -= Time.deltaTime * GameMaster.gameSpeed;
        if (timer <= 0)
        {
            Structure s = gameObject.GetComponent<Structure>();
            if (s != null) s.Annihilate(StructureAnnihilationOrder.DecayDestruction);
            else Destroy(this);
        }
	}
}

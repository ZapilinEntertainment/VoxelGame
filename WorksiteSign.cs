using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorksiteSign : MonoBehaviour {
	public Worksite worksite;

	void Update() {
        if (worksite == null)
        {
            Destroy(gameObject);
        }
	}
}

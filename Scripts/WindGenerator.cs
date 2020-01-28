using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindGenerator : Building {
	public Transform head, screw;
	Vector2 windDirection;
    bool rotateHead = false, rotateScrew = true, subscribedToWindUpdate = false;
	const float HEAD_ROTATE_SPEED = 1, SCREW_ROTATE_SPEED = 90;
	float height_coefficient = 1;
    const int STANDART_SURPLUS = 10;

	override public void Prepare() {
		PrepareBuilding();
	}

    override public void SetBasement(Plane b, PixelPosByte pos) {
		if (b == null) return;
		SetBuildingData(b, pos);
        subscribedToUpdate = true;
        if (!subscribedToWindUpdate)
        {
            GameMaster.realMaster.environmentMaster.WindUpdateEvent += this.WindUpdate;            
            subscribedToWindUpdate = true;
            head = transform.GetChild(0).GetChild(0);
            screw = head.transform.GetChild(0);
        }
        float hf = Chunk.CHUNK_SIZE / 2f;
		height_coefficient = (basement.pos.y - hf) / hf;
        if (height_coefficient < 0) height_coefficient /= 4f;
		WindUpdate(GameMaster.realMaster.environmentMaster.windVector);
	}

	void Update() {
        if (!subscribedToUpdate) return;
		if ( rotateHead ) {
            Vector3 newDir = windDirection.magnitude != 0 ? new Vector3(-windDirection.x, 0, -windDirection.y).normalized : Vector3.zero;
            if (newDir != Vector3.zero)
            {
                head.transform.forward = Vector3.MoveTowards(head.transform.forward, newDir, HEAD_ROTATE_SPEED * Time.deltaTime);
                if (head.transform.forward == newDir) rotateHead = false;
            }
            else
            {
                rotateHead = false;
                rotateScrew = false;
            }
		}
		if (rotateScrew) {
			screw.transform.Rotate( Vector3.forward * windDirection.magnitude * SCREW_ROTATE_SPEED * Time.deltaTime * GameMaster.gameSpeed);
		}
	}

	public void WindUpdate( Vector2 direction) {
		windDirection = direction;
		if (windDirection == Vector2.zero) {
            rotateScrew = false;
            if ( rotateScrew ) {
				energySurplus = 0;
                GameMaster.realMaster.colonyController.RecalculatePowerGrid();
			}
			rotateHead = false;
		}
		else {
			if (rotateScrew == false) {
				rotateScrew = true;
                GameMaster.realMaster.colonyController.RecalculatePowerGrid();
			}
            float newSurplus = windDirection.magnitude * (STANDART_SURPLUS * (1 + height_coefficient));
            if (newSurplus != energySurplus)
            {
                energySurplus = newSurplus;
                GameMaster.realMaster.colonyController.RecalculatePowerGrid();
            }
            else energySurplus = newSurplus;
            if (head.transform.forward != new Vector3(windDirection.x, 0, windDirection.y).normalized) rotateHead = true; else rotateHead = false;
		}
	}

    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        if (!clearFromSurface) { UnsetBasement(); }
        PrepareBuildingForDestruction(clearFromSurface, returnResources, leaveRuins);
        if (subscribedToWindUpdate)
        {
            GameMaster.realMaster.environmentMaster.WindUpdateEvent -= this.WindUpdate;
            subscribedToWindUpdate = false;
        }
        Destroy(gameObject);
    }
}

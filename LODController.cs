using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LODController : MonoBehaviour {
    public Sprite[] lodSprites;
    float lodDistance = 5;

    public void CameraUpdate(Transform cam)
    {
        int count = transform.childCount;
        Transform me = transform;
        Vector3 camPos = cam.transform.position;
        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                Transform t = me.GetChild(i);
                Vector3 treePos = t.position;
                Transform spriteTransform = t.GetChild(2);
                if ((treePos - camPos).magnitude > lodDistance)
                {                    
                    if ( !spriteTransform.gameObject.activeSelf )
                    {
                        t.GetChild(0).gameObject.SetActive(false);
                        t.GetChild(1).gameObject.SetActive(false);
                        spriteTransform.gameObject.SetActive(true);
                    }
                    byte spriteStatus = 0;
                    float angle = Vector3.Angle(Vector3.up, camPos - treePos);
                    if (angle < 30)
                    {
                        if (angle < 5) spriteStatus = 3;
                        else spriteStatus = 2;
                    }
                    else
                    {
                        if (angle > 60) spriteStatus = 0;
                        else spriteStatus = 1;
                    }
                    spriteTransform.GetComponent<SpriteRenderer>().sprite = lodSprites[spriteStatus];                                       
                }
                else
                {
                    if (spriteTransform.gameObject.activeSelf)
                    {
                        t.GetChild(0).gameObject.SetActive(true);
                        t.GetChild(1).gameObject.SetActive(true);
                        spriteTransform.gameObject.SetActive(false);
                    }
                }

               
            }
        }
    }
}

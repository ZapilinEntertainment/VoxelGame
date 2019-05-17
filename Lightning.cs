using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lightning : MonoBehaviour
{
    private static LineRenderer[] drawers;
    private static float[] speeds;
    private static bool activeLRs = false;
    private static GameObject executor;

    public const float fallspeed = 1f, disappearSpeed = 5f;

    public static void Strike(Vector3 pos)
    {
        if (executor == null)
        {
            executor = new GameObject("Lightning executor");
            executor.AddComponent<Lightning>();
        }
        LineRenderer d = null;
        int index = -1;
        if (drawers == null)
        {
            drawers = new LineRenderer[5];
            speeds = new float[5];
            d = Instantiate(Resources.Load<GameObject>("Prefs/LightningLR")).GetComponent<LineRenderer>();
            drawers[0] = d;
            index = 0;
        }
        else
        {
            if (!drawers[0].enabled)
            {
                d = drawers[0];
                index = 0;
            }
            else
            {
                if (drawers[1] == null)
                {
                    d = Instantiate(Resources.Load<GameObject>("Prefs/LightningLR")).GetComponent<LineRenderer>();
                    drawers[1] = d;
                    index = 1;
                }
                else
                {
                    if (!drawers[1].enabled)
                    {
                        d = drawers[1];
                        index = 1;
                    }
                    else
                    {
                        if (drawers[2] == null)
                        {
                            d = Instantiate(Resources.Load<GameObject>("Prefs/LightningLR")).GetComponent<LineRenderer>();
                            drawers[2] = d;
                            index = 2;
                        }
                        else
                        {
                            if (!drawers[2].enabled)
                            {
                                d = drawers[2];
                                index = 2;
                            }
                            else
                            {
                                if (drawers[3] == null)
                                {
                                    d = Instantiate(Resources.Load<GameObject>("Prefs/LightningLR")).GetComponent<LineRenderer>();
                                    drawers[3] = d;
                                    index = 3;
                                }
                                else
                                {
                                    if (!drawers[3].enabled)
                                    {
                                        d = drawers[3];
                                        index = 3;
                                    }
                                    else
                                    {
                                        if (drawers[4] == null)
                                        {
                                            d = Instantiate(Resources.Load<GameObject>("Prefs/LightningLR")).GetComponent<LineRenderer>();
                                            drawers[4] = d;
                                            index = 4;
                                        }
                                        else
                                        {
                                            if (!drawers[4].enabled)
                                            {
                                                d = drawers[4];
                                                index = 4;
                                            }
                                            else return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        var lr = drawers[index];
        speeds[index] = 0f;
        lr.colorGradient.alphaKeys[1].time = 0.001f;
        var positions = new Vector3[5];
        lr.GetPositions(positions);
        positions[4] = pos;
        var uppos = new Vector3(pos.x, GameConstants.GetUpperBorder(), pos.z);
        positions[0] = uppos;
        var p = (uppos - pos) * 0.25f;
        float m = p.sqrMagnitude;
        positions[3] = pos + Vector3.up ;
        positions[2] = pos + 2 *p;
        positions[1] = pos + 3 * p ;
        lr.SetPositions(positions);
        var g = lr.colorGradient;
        var ckeys = g.colorKeys;
        var akeys = g.alphaKeys;
        akeys[1].time = 0.01f;
        akeys[1].alpha = 1f;
        akeys[2].alpha = 0f;
        g.SetKeys(ckeys, akeys);
        lr.colorGradient = g;
        lr.enabled = true;
        activeLRs = true;        
    }

    private void Update()
    {
        if (activeLRs)
        {
            int activeLRsCount = 0;
            float up = GameConstants.GetUpperBorder(), speed;
            Gradient g;
            GradientAlphaKey[] akeys;
            GradientColorKey[] ckeys;
            LineRenderer lr;
            for (int i = 0; i < drawers.Length; i++)
            {
                lr = drawers[i];
                if (lr != null)
                {
                    if (lr.enabled)
                    {
                        g = lr.colorGradient;
                        akeys = g.alphaKeys;
                        ckeys = g.colorKeys;                        
                        if (akeys[1].time < 1)
                        {
                            speeds[i] += fallspeed * Time.deltaTime;
                            speed = speeds[i];
                            float f = akeys[1].time + speed * Time.deltaTime;
                            akeys[1].time = f;
                            akeys[2].alpha = f > 0.5f ? (f - 0.5f) * 2f : 0f;
                            activeLRsCount++;
                        }
                        else
                        {
                            speed = speeds[i];
                            akeys[1].alpha -= speed / 2f * Time.deltaTime;
                            akeys[2].alpha -= speed / 2f * Time.deltaTime; ;
                            if (akeys[2].alpha <= 0) lr.enabled = false;
                            else activeLRsCount++;
                        }
                        g.SetKeys(ckeys, akeys);
                        lr.colorGradient = g;
                    }
                }
                else break;
            }
            if (activeLRsCount == 0) activeLRs = false;
        }
    }
}

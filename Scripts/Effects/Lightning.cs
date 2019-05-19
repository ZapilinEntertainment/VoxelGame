using UnityEngine;

public sealed class Lightning : MonoBehaviour
{
    private static LineRenderer[] drawers;
    private static float[] speeds;
    private static bool activeLRs = false;
    private static GameObject executor;

    private const float fallspeed = 3f ;
    private const float LIGHTNING_BASIC_DAMAGE = 10f;

    public static void Strike(Vector3 startPos, Vector3 endpos)
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
        positions[0] = startPos;
        positions[4] = endpos;
        var p = (startPos - endpos).normalized;
        positions[3] = endpos + p + Vector3.Project(new Vector3((Random.value - 0.5f) * 0.4f, 0, (Random.value - 0.5f)), p);
        positions[2] = endpos + 2 * p + Vector3.Project(new Vector3((Random.value - 0.5f) * 0.8f, 0, (Random.value - 0.5f) * 0.8f), p);
        positions[1] = endpos + 3 * p + Vector3.Project(new Vector3((Random.value - 0.5f) * 1.6f, 0, (Random.value - 0.5f) * 1.6f),p);
        lr.SetPositions(positions);
        var g = lr.colorGradient;
        var ckeys = g.colorKeys;
        var akeys = g.alphaKeys;
        akeys[1].time = 0.01f;
        akeys[1].alpha = 1f;
        akeys[2].alpha = 0f;
        ckeys[0].color = Color.Lerp(Color.red, Color.cyan, GameMaster.realMaster.stability);
        g.SetKeys(ckeys, akeys);
        lr.colorGradient = g;
        lr.enabled = true;
        if (GameMaster.soundEnabled) GameMaster.audiomaster.MakeSoundEffect(SoundEffect.Thunder);
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
                            speeds[i] += fallspeed * Time.deltaTime * (1.5f - GameMaster.realMaster.stability);
                            speed = speeds[i];
                            float f = akeys[1].time + speed * Time.deltaTime;
                            akeys[1].time = f;
                            akeys[2].alpha = f > 0.5f ? (f - 0.5f) * 2f : 0f;
                            if (f >= 1 & GameMaster.soundEnabled)
                            {
                                GameMaster.audiomaster.MakeSoundEffect(SoundEffect.Lightning, lr.GetPosition(4));
                            }
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

    public static float CalculateDamage()
    {
        switch (GameMaster.realMaster.difficulty)
        {
            case Difficulty.Utopia: return LIGHTNING_BASIC_DAMAGE * 0.5f;
            case Difficulty.Easy: return LIGHTNING_BASIC_DAMAGE * 0.8f;
            case Difficulty.Hard: return LIGHTNING_BASIC_DAMAGE * 2f;
            case Difficulty.Torture: return LIGHTNING_BASIC_DAMAGE * 10f;
            default: return LIGHTNING_BASIC_DAMAGE;
        }
    }
}

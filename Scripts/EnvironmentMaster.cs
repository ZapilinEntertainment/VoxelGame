using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class EnvironmentMaster : MonoBehaviour {
    [SerializeField] private Vector2 newWindVector;

    public bool positionChanged = false; // может быть отмечена другими скриптами
    public float environmentalConditions { get; private set; } // 0 is hell, 1 is very favourable
    public Vector2 windVector { get; private set; }
    public Light sun;
    public delegate void WindChangeHandler(Vector2 newVector);
    public event WindChangeHandler WindUpdateEvent;

    private bool prepared = false, showCelestialBodies = true;
    private int vegetationShaderWindPropertyID, lastDrawnMapActionHash;
    private float windTimer = 0, prevSkyboxSaturation = 1, environmentEventTimer = 0, lastSpawnDistance = 0, effectsTimer = 10;
    private Environment currentEnvironment;
    private GlobalMap gmap;
    private Material skyboxMaterial;
    private List<Transform> decorations;
    private Dictionary<MapPoint, Transform> celestialBodies;
    
    private const float WIND_CHANGE_STEP = 1, WIND_CHANGE_TIME = 120, DECORATION_PLANE_WIDTH = 6;
    private const int SKY_SPHERE_RADIUS = 9, CLOUD_LAYER_INDEX = 9;

    public void Prepare()
    {
        if (prepared) return; else prepared = true;
        vegetationShaderWindPropertyID = Shader.PropertyToID("_Windpower");
        windVector = Random.insideUnitCircle;
        newWindVector = windVector;

        Shader.SetGlobalFloat(vegetationShaderWindPropertyID, 1);
        sun = FindObjectOfType<Light>();
        decorations = new List<Transform>();

        skyboxMaterial = RenderSettings.skybox;
        var rm = GameMaster.realMaster;
        gmap = rm.globalMap;
        if (rm.gameMode != GameMode.Editor)
        {
            RefreshEnvironment();
            RecalculateCelestialDecorations();
        }
        else
        {
            SetEnvironment(Environment.defaultEnvironment);
        }
    }
    public void PrepareIslandBasis(ChunkGenerationMode cmode)
    { // его придётся сохранять!
        switch (cmode) {
            case ChunkGenerationMode.Peak:
                int resolution = 15;
                GameObject g = Constructor.CreatePeakBasis(resolution, ResourceType.STONE_ID);
                float cs = Chunk.CHUNK_SIZE * Block.QUAD_SIZE;
                g.transform.localScale = Vector3.one * cs / 2f;
                g.transform.position = new Vector3(cs / 2f - 0.5f * Block.QUAD_SIZE , g.transform.localScale.y / 2f - 0.5f * Block.QUAD_SIZE, cs / 2f - 0.5f * Block.QUAD_SIZE);
                break;
            default: return;
        }        
    }


    public void SetEnvironment(Environment e)
    {
        currentEnvironment = e;       
        prevSkyboxSaturation = 1f;
        //# setting environment
        environmentalConditions = currentEnvironment.conditions;

        var ls = currentEnvironment.lightSettings;
        sun.color = ls.sunColor;
        sun.transform.forward = ls.sunDirection;
        float s = 0.75f;
        sun.intensity = ls.maxIntensity * s;

        var skyColor = Color.Lerp(Color.black, ls.sunColor, s);
        var horColor = Color.Lerp(Color.cyan, ls.horizonColor, s);
        var bottomColor = Color.Lerp(Color.white, ls.bottomColor, s);
        RenderSettings.ambientGroundColor = bottomColor;
        RenderSettings.ambientEquatorColor = horColor;
        RenderSettings.ambientGroundColor = bottomColor;
        skyboxMaterial.SetColor("_BottomColor", bottomColor);
        skyboxMaterial.SetColor("_HorizonColor", horColor);
        //
        environmentEventTimer = currentEnvironment.GetInnerEventTime();
    }

    public void AddDecoration(float size, GameObject dec)
    {
        var mv = gmap.cityFlyDirection;
        var v = new Vector3(mv.x, 0, mv.z).normalized;
        v *= -1;
        dec.layer = CLOUD_LAYER_INDEX;
        dec.transform.position = Quaternion.AngleAxis((0.5f - Random.value) * 180f, Vector3.up) * (v * SKY_SPHERE_RADIUS) + v * lastSpawnDistance;
        
        decorations.Add(dec.transform);
        lastSpawnDistance += size;
    }
    public void RecalculateCelestialDecorations()
    {
        var gm = GameMaster.realMaster.globalMap;
        var pts = gm.mapPoints;
        if (celestialBodies != null)
        {
            //проверка существующих спрайтов звезд
            var destroyList = new List<MapPoint>();
            foreach (var cb in celestialBodies)
            {
                if (!pts.Contains(cb.Key))
                {
                    destroyList.Add(cb.Key);
                }
            }
            if (destroyList.Count > 0) {
                foreach (var d in destroyList)
                {
                    Transform t = celestialBodies[d];
                    Destroy(t.gameObject);
                    celestialBodies.Remove(d);
                }
            }
            // ищем добавившиеся точки
            foreach (var pt in pts)
            {
                if (pt.type == MapMarkerType.Star && !celestialBodies.ContainsKey(pt)) AddVisibleStar(pt as SunPoint);
            }
        }
        else
        {
            celestialBodies = new Dictionary<MapPoint, Transform>();
            foreach (var pt in pts)
            {
                if (pt.type == MapMarkerType.Star) AddVisibleStar(pt as SunPoint);
            }
        }
        lastDrawnMapActionHash = gm.actionsHash;
    }
    private void AddVisibleStar(SunPoint sp)
    {
        var g = new GameObject("star");
        g.layer = GameConstants.CELESTIAL_LAYER;
        var sr = g.AddComponent<SpriteRenderer>();
        sr.sprite = PoolMaster.GetStarSprite(false);
        sr.sharedMaterial = PoolMaster.celestialBillboardMaterial;
        sr.color = sp.color;
        celestialBodies.Add(sp, g.transform);
        if (!showCelestialBodies) g.SetActive(false);
        Vector3 cpoint = Quaternion.AngleAxis(gmap.cityPoint.angle, Vector3.back) * (Vector3.up * gmap.cityPoint.height),
            mpoint = Quaternion.AngleAxis(sp.angle, Vector3.back) * (Vector3.up * sp.height);
        mpoint -= cpoint;
        mpoint.z = mpoint.y;
        mpoint.y = 0.2f;
        g.transform.position = mpoint * SKY_SPHERE_RADIUS;
    }
    public void RefreshEnvironment()
    {
        var rs = gmap.GetCurrentSector();
        currentEnvironment = rs.environment;
        //# setting environment
        environmentalConditions = currentEnvironment.conditions;

        var ls = currentEnvironment.lightSettings;
        sun.color = ls.sunColor;
        sun.transform.forward = ls.sunDirection;
        float s = rs.GetVisualSaturationValue();
        float li = ls.maxIntensity * s;
        sun.intensity = li;
        Color scolor = sun.color * li * 1.1f;
        PoolMaster.billboardMaterial.SetColor("_MainColor", scolor);
        PoolMaster.verticalBillboardMaterial.SetColor("_MainColor", scolor);

        var skyColor = Color.Lerp(Color.black, ls.sunColor, s);
        var horColor = Color.Lerp(Color.cyan, ls.horizonColor, s);
        var bottomColor = Color.Lerp(Color.white, ls.bottomColor, s);
        RenderSettings.ambientGroundColor = bottomColor;
        RenderSettings.ambientEquatorColor = horColor;
        RenderSettings.ambientGroundColor = bottomColor;
        skyboxMaterial.SetColor("_BottomColor", bottomColor);
        skyboxMaterial.SetColor("_HorizonColor", horColor);
        //
        prevSkyboxSaturation = s;
    }

    private void LateUpdate()
    {
        float t = Time.deltaTime * GameMaster.gameSpeed;
        windTimer -= t;
        if (windTimer <= 0)
        {
            windTimer = WIND_CHANGE_TIME * (0.1f + Random.value * 1.9f);
            newWindVector = Quaternion.AngleAxis((0.5f - Random.value) * 30, Vector3.up) * windVector;
            newWindVector = newWindVector * ( 1 + (0.5f - Random.value) * 0.5f );
        }

        if (windVector != newWindVector)
        {
            float st = WIND_CHANGE_STEP * t;            
            windVector = Vector3.RotateTowards(windVector, newWindVector, st, st);
            float windpower = windVector.magnitude;
            Shader.SetGlobalFloat(vegetationShaderWindPropertyID, windpower);
            if (WindUpdateEvent != null) WindUpdateEvent(windVector);
        }

        if (GameMaster.realMaster.gameMode != GameMode.Editor & !GameMaster.loading)
        {
            // меняем краски, если город начинает двигаться внутри сектора
            if (positionChanged)
            {
                RefreshEnvironment();
                positionChanged = false;
            }

            //if (currentEnvironment.presetType != Environment.EnvironmentPresets.Default)
            //{
            if (environmentEventTimer > 0)
            {
                environmentEventTimer -= t;
                if (environmentEventTimer <= 0)
                {
                    currentEnvironment.InnerEvent();
                    environmentEventTimer = currentEnvironment.GetInnerEventTime();
                }
            }
            //}

            var dir = gmap.cityFlyDirection / 500f;
            dir.y = 0;
            if (decorations.Count > 0)
            {
                int i = 0;
                Vector3 pos;
                float sqrRadius = SKY_SPHERE_RADIUS * SKY_SPHERE_RADIUS;
                while (i < decorations.Count)
                {
                    pos = decorations[i].position;
                    pos += dir;
                    decorations[i].position = pos;
                    if (Vector3.SqrMagnitude(pos) > sqrRadius * 4f)
                    {
                        Destroy(decorations[i].gameObject);
                        decorations.RemoveAt(i);
                    }
                    else i++;
                }
            }
            if (lastSpawnDistance > 0) lastSpawnDistance -= dir.magnitude;

            if (showCelestialBodies)
            {
                if (celestialBodies != null && celestialBodies.Count > 0)
                {
                    Vector3 cpoint = Quaternion.AngleAxis(gmap.cityPoint.angle, Vector3.back) * (Vector3.up * gmap.cityPoint.height);
                    Vector3 mpoint;
                    foreach (var sb in celestialBodies)
                    {
                        mpoint = Quaternion.AngleAxis(sb.Key.angle, Vector3.back) * (Vector3.up * sb.Key.height);
                        mpoint -= cpoint;
                        mpoint.z = mpoint.y * 10f; mpoint.x *= 10f;
                        mpoint.y = 1f;
                        sb.Value.position = mpoint;
                    }
                }
            }

            //test lightnings
            if (false)
            {
                effectsTimer -= t;
                if (effectsTimer < 0)
                {
                    float f = Chunk.CHUNK_SIZE;
                    var center = GameMaster.sceneCenter;
                    var pos = Random.onUnitSphere * f + center;
                    dir = center - pos;
                    dir += Random.onUnitSphere;
                    RaycastHit rh;
                    if (Physics.Raycast(pos, dir, out rh, 2 * f))
                    {
                        Lightning.Strike(pos, rh.point);
                        var hitobject = rh.collider;
                        float damage = Lightning.CalculateDamage();
                        if (hitobject.tag == Structure.STRUCTURE_COLLIDER_TAG)
                        {
                            hitobject.transform.parent.GetComponent<Structure>().ApplyDamage(damage);
                        }
                        else
                        {
                            if (hitobject.tag == Chunk.BLOCK_COLLIDER_TAG)
                            {
                                var crh = GameMaster.realMaster.mainChunk.GetBlock(rh.point, rh.normal);
                                Block b = crh.block;
                                if (b != null)
                                {
                                    if (b.type == BlockType.Cube) (b as CubeBlock).Dig((int)damage, true);
                                    else
                                    {
                                        var sb = b as SurfaceBlock;
                                        if (sb != null)
                                        {
                                            sb.EnvironmentalStrike(rh.point, 2, damage);
                                        }
                                        else
                                        {
                                            if (b.blockingStructure != null) b.blockingStructure.ApplyDamage(damage);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        var end = pos + Random.onUnitSphere * 5f;
                        end.y = GameConstants.GetBottomBorder();
                        Lightning.Strike(pos, end);
                    }
                    effectsTimer = 1f;
                }
            }
        }
    }

    public void EnableDecorations()
    {
        showCelestialBodies = true;
        if (celestialBodies !=null && celestialBodies.Count > 0)
        {
            foreach (var sb in celestialBodies)
            {
                sb.Value.gameObject.SetActive(true);
            }
        }
    }
    public void DisableDecorations()
    {
        showCelestialBodies = false;
        if (celestialBodies != null && celestialBodies.Count > 0)
        {
            foreach (var sb in celestialBodies)
            {
                sb.Value.gameObject.SetActive(false);
            }
        }
    }


    public void Save( System.IO.FileStream fs)
    {
        fs.Write(System.BitConverter.GetBytes(newWindVector.x),0,4); // 0 - 3
        fs.Write(System.BitConverter.GetBytes(newWindVector.y), 0, 4);// 4 - 7
        fs.Write(System.BitConverter.GetBytes(windVector.x), 0, 4);   // 8 - 11
        fs.Write(System.BitConverter.GetBytes(windVector.y), 0, 4);  // 12 - 15
        fs.Write(System.BitConverter.GetBytes(environmentalConditions), 0, 4); // 16 - 19
        fs.Write(System.BitConverter.GetBytes(windTimer), 0, 4);  // 20 - 23
        //сохранение декораций?
        //save environment
    }
    public void Load(System.IO.FileStream fs)
    {
        var data = new byte[24];
        fs.Read(data, 0, data.Length);
        newWindVector = new Vector2(System.BitConverter.ToSingle(data, 0), System.BitConverter.ToSingle(data, 4));
        windVector = new Vector2(System.BitConverter.ToSingle(data, 8), System.BitConverter.ToSingle(data, 12));
        
        windTimer = System.BitConverter.ToSingle(data, 20);

        vegetationShaderWindPropertyID = Shader.PropertyToID("_Windpower");
        float windPower = windVector.magnitude;
        Shader.SetGlobalFloat(vegetationShaderWindPropertyID, windPower);        
        if (WindUpdateEvent != null) WindUpdateEvent(windVector);

        skyboxMaterial = RenderSettings.skybox;
        gmap = GameMaster.realMaster.globalMap;
        SetEnvironment(gmap.GetCurrentEnvironment());
        environmentalConditions = System.BitConverter.ToSingle(data, 16);
        RecalculateCelestialDecorations();

        prepared = true;
    }
}

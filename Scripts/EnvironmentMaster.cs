using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class EnvironmentMaster : MonoBehaviour {
    [SerializeField] private Vector2 newWindVector;

    public bool positionChanged = false; // hot

    public float environmentalConditions { get { return currentEnvironment.conditions; } } 
    public float lifepowerSupport { get { return currentEnvironment.lifepowerSupport; } }
    public float envRichness { get { return currentEnvironment.richness; } }
    public float envStability { get { return currentEnvironment.stability; } }
    public float islandStability { get; private set; }

    public Vector2 windVector { get; private set; }    
    public delegate void WindChangeHandler(Vector2 newVector);
    public event WindChangeHandler WindUpdateEvent;
    public event System.Action<Environment> environmentChangingEvent;

    private bool prepared = false, showCelestialBodies = true, lightning = false;
    private int vegetationShaderWindPropertyID, lastDrawnMapActionHash;
    private float windTimer = 0, prevSkyboxSaturation = 1, environmentEventTimer = 0, lastSpawnDistance = 0, effectsTimer = 10,
        targetStability = DEFAULT_ISLAND_STABILITY;
    private ColonyController colonyController;
    private Environment currentEnvironment = Environment.defaultEnvironment;
    private GameMaster gm;
    private GlobalMap gmap;
    private Material skyboxMaterial;
    private List<Transform> decorations;
    private Light sun;
    private Transform sunTransform;
    private Dictionary<MapPoint, Transform> celestialBodies;
    
    private Dictionary<int, float> stabilityModifiers;
    private int nextSModifiersID;

    private const float WIND_CHANGE_STEP = 1, WIND_CHANGE_TIME = 120, DECORATION_PLANE_WIDTH = 6, BASIC_SUN_INTENSITY = 0.5f, 
        DEFAULT_ISLAND_STABILITY = 0.5f, CITY_CHANGE_HEIGHT_STEP = 0.001f, POPULATION_STABILITY_EFFECT_1 = 0.1f, 
        POPULATION_STABILITY_EFFECT_2 = 0.25f, POPULATION_STABILITY_EFFECT_3 = 0.5f;
    private const int POPULATION_CONDITION_1 = 2500, POPULATION_CONDITION_2 = 10000, POPULATION_CONDITION_3 = 25000;
    private const int SKY_SPHERE_RADIUS = 9, CLOUD_LAYER_INDEX = 9;

    public void Prepare()
    {
        if (prepared) return; else prepared = true;
        vegetationShaderWindPropertyID = Shader.PropertyToID("_Windpower");
        if (!GameMaster.loading)
        {
            windVector = Random.insideUnitCircle;
            newWindVector = windVector;
        }

        Shader.SetGlobalFloat(vegetationShaderWindPropertyID, 1);
        sun = FindObjectOfType<Light>();
        if (sun == null)
        {
            sun = new GameObject("sun").AddComponent<Light>();
            sun.type = LightType.Directional;
        }
        sunTransform = sun.transform;
        decorations = new List<Transform>();

        skyboxMaterial = RenderSettings.skybox;
        gm = GameMaster.realMaster;
        gmap = gm.globalMap;
        if (gm.gameMode != GameMode.Editor)
        {
            RefreshEnvironment();
            RecalculateCelestialDecorations();
        }
        else
        {
            SetEnvironment(Environment.defaultEnvironment);
        }       
        islandStability = DEFAULT_ISLAND_STABILITY;
        gmap?.LinkEnvironmentMaster(this);
        if (GameMaster.realMaster.testMode) AnnouncementCanvasController.MakeAnnouncement("environment master loaded");
    }
    public void LinkColonyController(ColonyController cc)
    {
        colonyController = cc;
    }

    public void PrepareIslandBasis(ChunkGenerationMode cmode)
    { // его придётся сохранять!
        switch (cmode) {
            case ChunkGenerationMode.Peak:
                int resolution = 15;
                GameObject g = Constructor.CreatePeakBasis(resolution, ResourceType.STONE_ID);
                float cs = Chunk.chunkSize * Block.QUAD_SIZE;
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
        SetLightParameters(BASIC_SUN_INTENSITY * e.lightIntensityMultiplier, 1f);
        environmentChangingEvent?.Invoke(currentEnvironment);
        positionChanged = false;
    }
    public void RefreshEnvironment()
    {
        var rs = gmap.GetCurrentSector();
        if (currentEnvironment != rs.environment)
        {
            currentEnvironment = rs.environment;
            environmentChangingEvent(currentEnvironment);
        }
        float s = rs.GetVisualSaturationValue();
        float li = BASIC_SUN_INTENSITY * currentEnvironment.lightIntensityMultiplier * s;
        SetLightParameters(li, s);        
        positionChanged = false;
    }
    private void SetLightParameters(float sunIntesity, float ambientColorSaturation)
    {        
        sun.intensity = sunIntesity;
        Color scolor = sun.color * (0.5f + 0.5f * sunIntesity);
        PoolMaster.billboardMaterial.SetColor("_MainColor", scolor);
        PoolMaster.verticalBillboardMaterial.SetColor("_MainColor", scolor);

        var skyColor = Color.Lerp(Color.white, currentEnvironment.skyColor, ambientColorSaturation);
        float h = gmap?.cityPoint?.height ?? 0.5f;
        var horColor = Color.Lerp(Color.cyan * Mathf.Cos(h * Mathf.PI / 2f), currentEnvironment.horizonColor, ambientColorSaturation);
        var bottomColor = Color.Lerp(Color.white, currentEnvironment.bottomColor, ambientColorSaturation);
        RenderSettings.ambientSkyColor = skyColor;
        RenderSettings.ambientEquatorColor = horColor;
        RenderSettings.ambientGroundColor = bottomColor;
        skyboxMaterial.SetColor("_BottomColor", bottomColor);
        skyboxMaterial.SetColor("_HorizonColor", horColor);
        prevSkyboxSaturation = ambientColorSaturation;
        //Debug.Log("light recalc");
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

    public int AddStabilityModifier(float val)
    {
        if (stabilityModifiers == null) stabilityModifiers = new Dictionary<int, float>();
        int id = nextSModifiersID++;
        stabilityModifiers.Add(id, val);
        return id;
    }
    public void ChangeStabilityModifierValue(int id, float val)
    {
        if (stabilityModifiers != null && stabilityModifiers.Count > 0)
        {
            if (stabilityModifiers.Remove(id))
            {
                if (val != 0) stabilityModifiers.Add(id, val);
            }
        }
    }
    public void RemoveStabilityModifier(int id)
    {
        if (stabilityModifiers != null)
        {
            stabilityModifiers.Remove(id);
            if (id == nextSModifiersID - 1)
            {
                nextSModifiersID = id;
            }
        }
    }

    private void LateUpdate()
    {
        if (gmap == null) return;
        float t = Time.deltaTime * GameMaster.gameSpeed,
            ascension = gmap.ascension;
            ;

        //wind:
        {
            windTimer -= t;
            if (windTimer <= 0)
            {
                windTimer = WIND_CHANGE_TIME * (0.1f + Random.value * 1.9f);
                newWindVector = Quaternion.AngleAxis((0.5f - Random.value) * 30, Vector3.up) * windVector;
                newWindVector = newWindVector * (1 + (0.5f - Random.value) * 0.5f);
            }
            if (windVector != newWindVector)
            {
                float st = WIND_CHANGE_STEP * t;
                windVector = Vector3.RotateTowards(windVector, newWindVector, st, st);
                float windpower = windVector.magnitude;
                Shader.SetGlobalFloat(vegetationShaderWindPropertyID, windpower);
                if (WindUpdateEvent != null) WindUpdateEvent(windVector);
            }
        }

        if (GameMaster.realMaster.gameMode != GameMode.Editor & !GameMaster.loading)
        {          
            //if (currentEnvironment.presetType != Environment.EnvironmentPresets.Default)
            //{
            if (environmentEventTimer > 0)
            {
                environmentEventTimer -= t;
                if (environmentEventTimer <= 0)
                {
                    //currentEnvironment.InnerEvent();
                   // environmentEventTimer = currentEnvironment.GetInnerEventTime();
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

            #region  test lightnings
            if (lightning){
                effectsTimer -= t;
                if (effectsTimer < 0)
                {
                    float f = Chunk.chunkSize;
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
                                crh.block?.EnvironmentalStrike(crh.faceIndex, rh.point, 2, damage);
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
            #endregion
            //sunlight changing
            var clv = gmap.cityLookVector * -1f;
            clv.y = -0.3f;
            //var rightVector = Vector3.Cross(clv, Vector3.down);
            // sunTransform.forward = Quaternion.AngleAxis(15f + ascension * 75f ,rightVector) * clv;
            sunTransform.forward = clv;

            #region stability            
            if (colonyController != null)
            {
                float hc = 1f, gc = 0f;
                hc = colonyController.happiness_coefficient;
                if (colonyController.storage != null)
                {
                    gc = colonyController.storage.standartResources[ResourceType.GRAPHONIUM_ID] / GameConstants.GRAPHONIUM_CRITICAL_MASS;

                    // + блоки?
                }
                float pc = 0f;
                int pop = colonyController.citizenCount;
                if (pop > POPULATION_CONDITION_1)
                {
                    if (pop >= POPULATION_CONDITION_3) pc = POPULATION_STABILITY_EFFECT_3;
                    else
                    {
                        if (pop > POPULATION_CONDITION_2) pc = POPULATION_STABILITY_EFFECT_2;
                        else pc = POPULATION_STABILITY_EFFECT_1;
                    }
                }

                float structureStabilizersEffect = 1f;
                if (stabilityModifiers != null && stabilityModifiers.Count > 0)
                {
                    structureStabilizersEffect = 1f;
                    foreach (var sm in stabilityModifiers)
                    {
                        structureStabilizersEffect *= (1f + sm.Value);
                    }
                }
                targetStability =
                    (
                    0.5f * hc // happiness
                    - gc // graphonium reserves
                    + pc // population
                    + (1f - ascension) * structureStabilizersEffect
                    )
                    *
                    envStability;
                if (targetStability > 1f) targetStability = 1f;
                else
                {
                    if (targetStability < 0f) targetStability = 0f;
                }
                if (islandStability != targetStability)
                {
                    islandStability = Mathf.MoveTowards(islandStability, targetStability, GameConstants.STABILITY_CHANGE_SPEED * Time.deltaTime);
                }
                if (islandStability < 1f)
                {
                    float lcf = colonyController.GetLevelCf();
                    float step = lcf > 1f ? 0f : CITY_CHANGE_HEIGHT_STEP * (1f - islandStability) * t * (1f - lcf);
                    if (step != 0f)
                    {
                        if (ascension < 0.5f)
                        {
                            if (gmap.cityPoint.height != 1f)
                            {
                                gmap.ChangeCityPointHeight(step);
                                positionChanged = true;
                            }
                        }
                        else
                        {
                            if (gmap.cityPoint.height != 0f)
                            {
                                gmap.ChangeCityPointHeight(-step);
                                positionChanged = true;
                            }
                        }
                    }
                }
            }
            // обновление освещения при движении города
            if (positionChanged) RefreshEnvironment();
            #endregion
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

    #region save-load
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
        RecalculateCelestialDecorations();

        prepared = true;
    }
    #endregion
}

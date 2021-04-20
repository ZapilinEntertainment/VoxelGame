using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public sealed class AnchorBasement : WorkBuilding
{
    private enum ActivityStage:byte { BasementBuilt, AwaitingPowering,PoweringUp, InnerRingBuilding,
        PierPreparing, OuterRingBuilding  }
    private enum ShipStatus { Disabled, ComingToPier, Docking, Leaving, WaitingForNextShip}
    // dependencies in update and saves!
    private GameObject hexPref;
    private Transform endCrystal, hexBasement, bigGear, smallGear, colonistsShip;
    private LineRenderer mainLine;
    private Vector3 pierPosition;
    private Vector3 endCrystalEndPosition { get { return transform.position + Vector3.down * OUTER_RING_HEIGHT; } }
    private Vector3 hexBasementEndPosition { get { return transform.position + Vector3.down * INNER_RING_HEIGHT;  } }
    private ActivityStage currentStage;
    private bool liftObject = false, transportColonists = false;
    private FoundationRouteScenario scenario;
    private ShipStatus shipStatus;
    private Action colonistsInfoUpdate;
    private float poweringProgress = 0f, shipSpeed = 0f, distanceToPier = 0f, shipTimer = 0f;
    public int colonistsArrived { get; private set; }
    public const float POWER_CONSUMPTION = -1500f, MAX_EXCESS_POWER = 15000f;
    private const float INNER_RING_HEIGHT = 20f, OUTER_RING_HEIGHT = 23f, HEXSIZE = 4f, PIER_HEIGHT = 10f,
        PADS_LIFT_SPEED = 2f, SHIP_WIDTH = 4f, SHIP_SPAWN_DISTANCE = 200f, SHIP_MAX_SPEED = 50f, SHIP_ACCELERATION = 2f, SHIP_AWAITING_TIME = 10f,
        SHIP_DOCKING_TIME = 10f, POWERING_SPEED = 0.1f;
    private const int MIN_COLONISTS_COUNT = 400, MAX_COLONISTS_COUNT = 2000;
    private readonly Color maxColor = new Color(1f, 0.74f, 0f), minColor = new Color(0f, 1f,1f);

    public static bool CheckSpecialBuildingCondition(Plane p, ref string refusalReason)
    {
        if (p.faceIndex == Block.DOWN_FACE_INDEX) return true;
        return false;
    }

    public override bool CanBeRotated() { return false; }


    override public void SetBasement(Plane b, PixelPosByte pos)
    {
        if (b == null) return;
        SetBuildingData(b, pos, false);
        currentStage = ActivityStage.BasementBuilt;
        poweringProgress = 0f;
        endCrystal = Instantiate(Resources.Load<GameObject>("Prefs/Special/fd_endCrystal")).transform;
        endCrystal.position = transform.position;
        endCrystal.GetComponent<Rotator>().SetRotationVector(Vector3.up * 25f);
        pierPosition = transform.position + Vector3.down * PIER_HEIGHT;
        PrepareMainLine();
    }
    public void LinkScenario(FoundationRouteScenario frs)
    {
        scenario = frs;
    }
    private void PrepareMainLine()
    {
        if (mainLine == null)
        {
            mainLine = gameObject.AddComponent<LineRenderer>();
            mainLine.sharedMaterial = Resources.Load<Material>("Materials/Lightning");
            mainLine.SetPositions(new Vector3[2] { transform.position + Vector3.down * 2.2f, transform.position});
        }
        switch(currentStage)
        {
            case ActivityStage.BasementBuilt:
            case ActivityStage.AwaitingPowering:
                mainLine.startColor = minColor;
                mainLine.endColor = minColor;
                mainLine.startWidth = 0.3f;
                mainLine.endWidth = 0.3f;
                break;
            case ActivityStage.PoweringUp:
                mainLine.startColor = Color.Lerp(minColor, maxColor, 0.5f);
                mainLine.endColor = Color.Lerp(minColor, maxColor, 0.8f);
                mainLine.startWidth = 0.6f;
                mainLine.endWidth = 0.6f;
                mainLine.SetPosition(1, endCrystal.transform.position);
                break;
            case ActivityStage.InnerRingBuilding:
            case ActivityStage.PierPreparing:
            case ActivityStage.OuterRingBuilding:
                mainLine.startColor = maxColor;
                mainLine.endColor = maxColor;
                mainLine.startWidth = 1.2f;
                mainLine.endWidth = 1.2f;
                mainLine.SetPosition(1, endCrystal.transform.position);
                break;
        }
    }

    public void Update()
    {
        var t = Time.deltaTime * GameMaster.gameSpeed;
        switch (currentStage)
        {
            case ActivityStage.BasementBuilt:
                {
                    poweringProgress = Mathf.MoveTowards(poweringProgress, 1f, PADS_LIFT_SPEED * t * 0.02f);
                    endCrystal.position = Vector3.Lerp(transform.position, endCrystalEndPosition, poweringProgress);
                    mainLine.SetPosition(1, endCrystal.transform.position);
                    if (poweringProgress >= 1f)
                    {                        
                        scenario.Next();
                        currentStage = ActivityStage.AwaitingPowering;
                        endCrystal.GetComponent<Rotator>().SetRotationVector(Vector3.up * 5f);
                    }
                    break;
                }
            case ActivityStage.PoweringUp:
                if (poweringProgress < 1f)
                {
                    float energyBoost = colony.energySurplus / MAX_EXCESS_POWER;
                    if (energyBoost >= 1f) energyBoost = 1f;
                    poweringProgress += (POWERING_SPEED + energyBoost) * t;
                    hexBasement.position = Vector3.Lerp(transform.position, hexBasementEndPosition, poweringProgress);
                    if (poweringProgress >= 1f)
                    {
                        // special sound?
                        currentStage = ActivityStage.InnerRingBuilding;
                        LoadBigGear();
                        bigGear.position = transform.position;
                        scenario.AnchorPoweredUp();
                        poweringProgress = 0f;
                        liftObject = true;
                    }
                }
                break;
            case ActivityStage.InnerRingBuilding:
                {
                    if (liftObject && poweringProgress < 1f)
                    {
                        float energyBoost = colony.energySurplus / MAX_EXCESS_POWER;
                        if (energyBoost >= 1f) energyBoost = 1f;
                        poweringProgress += (POWERING_SPEED + energyBoost) * t * 0.25f;
                        bigGear.position = Vector3.Lerp(transform.position, hexBasementEndPosition, poweringProgress);
                        if (poweringProgress >= 1f)
                        {
                            liftObject = false;
                            scenario.AnchorBigGearReady();
                        }
                    }
                    break;
                }
            case ActivityStage.PierPreparing:
                {
                    if (liftObject && poweringProgress <1f)
                    {
                        float energyBoost = colony.energySurplus / MAX_EXCESS_POWER;
                        if (energyBoost >= 1f) energyBoost = 1f;
                        poweringProgress += (POWERING_SPEED + energyBoost) * t;
                        smallGear.position = Vector3.Lerp(transform.position, pierPosition, poweringProgress);
                        if (poweringProgress >= 1f)
                        {
                            liftObject = false;
                            scenario.OKButton();
                        }
                    }
                    break;
                }
            case ActivityStage.OuterRingBuilding:
                {
                    float step = 0f;
                    if (transportColonists)
                    {
                        if (shipStatus != ShipStatus.Disabled)
                        {
                            switch (shipStatus)
                            {
                                case ShipStatus.ComingToPier:
                                    float brakingDistances = shipSpeed * shipSpeed / SHIP_ACCELERATION / 2f;
                                    if (brakingDistances >= distanceToPier) shipSpeed = Mathf.MoveTowards(shipSpeed, 0f, SHIP_ACCELERATION * 10f * t);
                                    step = shipSpeed * t;
                                    distanceToPier -= step;
                                    if (distanceToPier > 1f) colonistsShip.Translate(Vector3.forward * step, Space.Self);
                                    else
                                    {
                                        shipSpeed = 0f;
                                        shipStatus = ShipStatus.Docking;
                                        shipTimer = SHIP_DOCKING_TIME;
                                    }
                                    break;
                                case ShipStatus.Docking:
                                    shipTimer -= t;
                                    if (shipTimer <= 0f)
                                    {
                                        ShipDocked();
                                        shipStatus = ShipStatus.Leaving;
                                        distanceToPier = 0f;
                                    }
                                    break;
                                case ShipStatus.Leaving:
                                    if (shipSpeed < SHIP_MAX_SPEED)
                                    {
                                        shipSpeed += SHIP_ACCELERATION * t;
                                        step = shipSpeed * t;
                                        distanceToPier += step;
                                        if (distanceToPier < SHIP_SPAWN_DISTANCE) colonistsShip.Translate(Vector3.forward * step, Space.Self);
                                        else
                                        {
                                            colonistsShip.gameObject.SetActive(false);
                                            shipSpeed = 0f;
                                            shipStatus = ShipStatus.WaitingForNextShip;
                                            shipTimer = SHIP_AWAITING_TIME;
                                        }
                                    }
                                    break;
                                case ShipStatus.WaitingForNextShip:
                                    shipTimer -= t;
                                    if (shipTimer <= 0f) SetShipStartPosition();
                                    break;
                            }
                        }
                    }
                    break;
                }
        }       
    }  

    public bool IsReadyToContinue()
    {
        switch (currentStage)
        {
            case ActivityStage.PoweringUp:
            case ActivityStage.BasementBuilt: return poweringProgress == 1f;
            case ActivityStage.AwaitingPowering: return false;
            case ActivityStage.InnerRingBuilding: return poweringProgress == 0f && !liftObject;
            default:return true;
        }
    }   
    public void SetColonistUIUpdateFunction(Action ui_update)
    {
        colonistsInfoUpdate = ui_update;
        ui_update();
    }
    
    public void StartActivating()
    {
        if (GameMaster.loading) return;
        if (currentStage == ActivityStage.AwaitingPowering)
        {
            currentStage = ActivityStage.PoweringUp;
            poweringProgress = 0f;
            GameMaster.audiomaster.MakeSoundEffect(SoundEffect.FD_anchorLaunch);
            PrepareMainLine();
            LoadHexBasement();
            // добавить влияние на global map и вывод сообщения о том, что оcтров заякорен
        }
    }  
    public void ActivatePier()
    {
        if (GameMaster.loading) return;
        if (currentStage == ActivityStage.InnerRingBuilding)
        {
            currentStage = ActivityStage.PierPreparing;
            LoadSmallGear();
            liftObject = true;
            poweringProgress = 0f;            
            PrepareMainLine();
        }
    }
    public void StartTransportingColonists(Action ui_function)
    {
        if (GameMaster.loading) return;
        if (currentStage == ActivityStage.PierPreparing)
        {
            currentStage = ActivityStage.OuterRingBuilding;
            colonistsInfoUpdate = ui_function;
            transportColonists = true;
            if (colonistsShip == null) LoadColonistsShip();
            SetShipStartPosition();
        }
    }
    private void LoadHexBasement()
    {
        hexBasement = Instantiate(Resources.Load<GameObject>("Prefs/Special/fd_hexBasement"), transform.position + Vector3.down * 2f, Quaternion.identity).transform;
    }
    private void LoadSmallGear()
    {
        smallGear = Instantiate(Resources.Load<GameObject>("Prefs/Special/fd_smallGear"), transform.position, Quaternion.identity).transform;
    }
    private void LoadBigGear()
    {
        bigGear = Instantiate(Resources.Load<GameObject>("Prefs/Special/fd_bigGear")).transform;
    }
    private void LoadColonistsShip()
    {
        colonistsShip = Instantiate(Resources.Load<GameObject>("Prefs/Ships/fd_colonyShip")).transform;
    }
    private void SetShipStartPosition()
    {
        byte x = 0;
        float r = UnityEngine.Random.value;
        if (r > 0.5f)
        {
            if (r > 0.75f) x = 3; else x = 2;
        }
        else
        {
            if (r > 0.25f) r = 1;
        }
        Vector3 d1,d2;
        switch (x)
        {
            case 1:
                d1 = Vector3.left;
                d2 =Vector3.forward * SHIP_WIDTH;
                break;
            case 2:
                d1 = Vector3.right;
                d2 = Vector3.forward * SHIP_WIDTH;
                break;
            case 3:
                d1 = Vector3.right ;
                d2 =  Vector3.back * SHIP_WIDTH;
                break;
            default:
                d1 = Vector3.left;
                d2 = Vector3.back * SHIP_WIDTH;
                break;
        }
        d1 = transform.TransformDirection(d1);
        d2 = transform.TransformDirection(d2);
        colonistsShip.position = pierPosition + d1 * SHIP_SPAWN_DISTANCE + d2;
        colonistsShip.forward = -d1;
        distanceToPier = SHIP_SPAWN_DISTANCE;
        shipSpeed = SHIP_MAX_SPEED;
        shipStatus = ShipStatus.ComingToPier;
        colonistsShip.gameObject.SetActive(true);
    }
    private void ShipDocked()
    {
        var x = UnityEngine.Random.Range(MIN_COLONISTS_COUNT, MAX_COLONISTS_COUNT);
        colonistsArrived += x;
        colony.AddCitizens(x, false);
        colonistsInfoUpdate?.Invoke();
    }

    public void AddSector(byte ringIndex, byte ringPosition)
    {
        if (hexPref == null)
        {
            hexPref = Resources.Load<GameObject>("Prefs/Special/foundationHex");
        }
        Transform t = Instantiate(hexPref).transform;
        switch (ringIndex)
        {
            case 0: // INNER
                {
                    t.position = transform.position + Vector3.down * INNER_RING_HEIGHT + Quaternion.AngleAxis(60f * ringPosition, Vector3.up) * (Vector3.forward * HEXSIZE * 2f);
                    t.localScale = Vector3.one * 0.5f;
                    break;
                }
            case 1: // OUTER
                {
                    t.position = transform.position + Vector3.down * OUTER_RING_HEIGHT + Quaternion.AngleAxis(60f * ringPosition, Vector3.up) * (Vector3.forward * HEXSIZE * 4f);
                    
                    break;
                }
        }
    }

    private void OnDestroy()
    {
        if (colonistsShip != null) Destroy(colonistsShip.gameObject);
    }

    #region save-load system
    override public List<byte> Save()
    {
        var data = base.Save();
        data.Add((byte)currentStage);
        switch (currentStage)
        {
            case ActivityStage.BasementBuilt:
            case ActivityStage.PoweringUp:
                data.AddRange(BitConverter.GetBytes(poweringProgress));
                break;
            case ActivityStage.InnerRingBuilding:
            case ActivityStage.PierPreparing:
                data.Add(liftObject ? (byte)1 : (byte)0);
                data.AddRange(BitConverter.GetBytes(poweringProgress));
                break;
            case ActivityStage.OuterRingBuilding:
                data.Add(transportColonists ? (byte)1 : (byte)0);
                data.Add((byte)shipStatus);
                void SaveShipPosition()
                {
                    var v = colonistsShip.position;
                    data.AddRange(BitConverter.GetBytes(v.x));
                    data.AddRange(BitConverter.GetBytes(v.y));
                    data.AddRange(BitConverter.GetBytes(v.z));
                    v = colonistsShip.forward;
                    data.AddRange(BitConverter.GetBytes(v.x));
                    data.AddRange(BitConverter.GetBytes(v.y));
                    data.AddRange(BitConverter.GetBytes(v.z));
                }
                switch(shipStatus)
                {
                    case ShipStatus.ComingToPier:
                    case ShipStatus.Leaving:
                        SaveShipPosition();
                        break;
                    case ShipStatus.Docking:
                        SaveShipPosition();
                        data.AddRange(BitConverter.GetBytes(shipTimer));
                        break;
                    case ShipStatus.WaitingForNextShip:
                        data.AddRange(BitConverter.GetBytes(shipTimer));
                        break;                   
                }
                data.AddRange(BitConverter.GetBytes(colonistsArrived));
                break;
        }
        return data;
    }

    override public void Load(System.IO.FileStream fs, Plane sblock)
    {
        base.Load(fs, sblock);
        var x = fs.ReadByte();
        currentStage = (ActivityStage)x;
        byte[] data;       
        switch (currentStage)
        {
            case ActivityStage.BasementBuilt:
            case ActivityStage.PoweringUp:
                data = new byte[4];
                fs.Read(data, 0, data.Length);
                poweringProgress = BitConverter.ToSingle(data, 0);
                break;
            case ActivityStage.InnerRingBuilding:
            case ActivityStage.PierPreparing:
                data = new byte[5];
                fs.Read(data, 0, data.Length);
                liftObject = data[0] == 1;
                poweringProgress = BitConverter.ToSingle(data, 1);
                break;
            case ActivityStage.OuterRingBuilding:
                transportColonists = (fs.ReadByte() == 1);
                shipStatus = (ShipStatus)fs.ReadByte();
                void LoadShipPosition()
                {
                    LoadColonistsShip();
                    colonistsShip.position = new Vector3(
                        BitConverter.ToSingle(data, 0),
                        BitConverter.ToSingle(data, 4),
                        BitConverter.ToSingle(data, 8)
                        );
                    colonistsShip.forward = new Vector3(
                        BitConverter.ToSingle(data, 12),
                        BitConverter.ToSingle(data, 16),
                        BitConverter.ToSingle(data, 20)
                        );
                }
                switch (shipStatus)
                {
                    case ShipStatus.ComingToPier:
                    case ShipStatus.Leaving:
                        data = new byte[24]; 
                        fs.Read(data, 0, data.Length);
                        LoadShipPosition();
                        break;
                    case ShipStatus.Docking:
                        data = new byte[28];
                        fs.Read(data, 0, data.Length);
                        LoadShipPosition();
                        shipTimer = BitConverter.ToSingle(data, 24);
                        break;
                    case ShipStatus.WaitingForNextShip:
                        data = new byte[4]; 
                        fs.Read(data, 0, data.Length);
                        shipTimer = BitConverter.ToSingle(data, 0);
                        break;
                }
                data = new byte[4];
                fs.Read(data, 0, data.Length);
                colonistsArrived = BitConverter.ToInt32(data, 0);
                break;
        }
        // SAVING HEX ARRAYS
        if (currentStage > ActivityStage.BasementBuilt)
        {
            endCrystal.position = endCrystalEndPosition;
            LoadHexBasement();
            if (currentStage > ActivityStage.PoweringUp)
            {
                LoadBigGear();
                var p = hexBasementEndPosition;
                hexBasement.position = p;
                if (currentStage > ActivityStage.InnerRingBuilding)
                {
                    LoadSmallGear();
                    bigGear.position = p;
                    if (currentStage > ActivityStage.InnerRingBuilding)
                    {
                        smallGear.position = pierPosition;
                    }
                }
            }
        }
        PrepareMainLine();
    }
    #endregion
}

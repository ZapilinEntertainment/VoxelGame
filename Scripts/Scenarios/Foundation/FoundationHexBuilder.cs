﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FoundationRoute
{
    public sealed class HexBuilder : MyObject
    {
        public readonly FoundationRouteScenario scenario;

        public float totalPowerConsumption { get; private set; }
        public float totalIncome { get; private set; }
        public float totalFoodProduction { get; private set; }
        public float totalLifepower { get; private set; }
        public int totalPersonnelInvolved { get; private set; }
        public int totalPersonnelSlots { get; private set; }
        public int colonistsCount { get; private set; }
        public int freeColonists { get; private set; }
        public int totalHousing { get; private set; }

        private readonly GameObject hexMaquetteExample;
        public HexCanvasUIC uic { get; private set; }
        private ColonyController colony;
        private AnchorBasement anchor;
        private Dictionary<(byte, byte), Hex> hexList;        
        private Dictionary<HexPosition, GameObject> maquettesList;
        private List<GameObject> maquettesPool;
        private Vector3 zeroPoint;
        private int poolLength = 0;
        private bool firstHex = true;
        private readonly float innerRadius, outerRadius;
        private const float CONST_0 = 1.73205f, ENERGY_CF = 1000f;
        private const int RING_LIMIT = 4;


        private HexBuilder() { }// reserved
        public HexBuilder(FoundationRouteScenario frs)
        {
            scenario = frs;
            hexList = new Dictionary<(byte, byte), Hex>();
            hexMaquetteExample = Resources.Load<GameObject>(FoundationRouteScenario.resourcesPath + "hexMaquette");
            maquettesPool = new List<GameObject>();
            poolLength = 0;
            maquettesList = new Dictionary<HexPosition, GameObject>();
            innerRadius = 8f;
            outerRadius = innerRadius * 2f / CONST_0;
            if (uic == null)
            {
                var t = Object.Instantiate(Resources.Load<GameObject>("UIPrefs/hexCanvas")).transform;
                UIController.GetCurrent().AddSpecialCanvasToHolder(t);
                uic = t.GetComponentInChildren<HexCanvasUIC>();
                uic.Prepare(this);
            }
            var rm = GameMaster.realMaster;
            colony = rm.colonyController;
            anchor = scenario.anchorBasement;
            zeroPoint = anchor?.outerRingZeroPoint ?? Vector3.zero ;
            rm.everydayUpdate += this.EverydayUpdate;
        }

        public void CountBuildings(ref int[] buildingsCount)
        {
            for (int i = 0; i < (int)HexType.TotalCount; i++)
            {
                buildingsCount[i] = 0;
            }
            if (hexList.Count > 0)
            {
                foreach (var h in hexList.Values)
                {
                    if (h.type < HexType.TotalCount)  buildingsCount[(int)h.type]++;
                }
            }
        }
        

        public void CreateHexMaquette(HexPosition hpos)
        {
            if (maquettesList.ContainsKey(hpos) || hexList.ContainsKey(hpos.ToBytes())) return;
            GameObject g;
            if (poolLength > 0)
            {
                g = maquettesPool[poolLength - 1];
                poolLength -= 1;
            }
            else g = Object.Instantiate(hexMaquetteExample);
            g.transform.position = GetHexWorldPosition(hpos);
            var cl = g.GetComponentInChildren<ClickableObject>();

            var npos = hpos.Copy;
            cl.AssignFunction(() => uic.OpenConstructionWindow(npos), uic.CloseButton);
            maquettesList.Add(npos, g);
        }
        public void CreateHex(HexPosition hpos, HexType htype)
        {
            CreateHex(hpos, new HexBuildingStats(htype));
        }
        public void CreateHex(HexPosition hpos, HexBuildingStats stats)
        {
            var bb = hpos.ToBytes();
            if (hexList.ContainsKey(bb))
            {
                Debug.Log("hex create error - key already exists");
                return;
            }
            else
            {
                if (maquettesList.ContainsKey(hpos))
                {
                    var m = maquettesList[hpos];
                    m.SetActive(false);
                    maquettesList.Remove(hpos);
                    maquettesPool.Add(m);
                }
                //
                var hex = LoadHexPref();
                hex.Initialize(hpos, this, stats);
                if (hpos.ringIndex > 0) hex.transform.position = GetHexWorldPosition(hpos) + Vector3.down * (Random.value * 2f);
                else hex.transform.position = GetHexWorldPosition(hpos);
                // модуляции высоты?
                hexList.Add(hpos.ToBytes(), hex);                
                uic.RecalculateAvailabilityMask();
                //+spread affection on neighbours:
                HexPosition npos;
                if (hexList.Count > 1) {
                    bool?[] aff;
                    for (byte i = 0; i < 6; i++)
                    {
                        npos = hpos.GetNeighbour(i);
                        bb = npos.ToBytes();
                        if (hexList.ContainsKey(bb))
                        {
                            hexList[bb].hexStats.ApplyNeighboursAffection(GetNeighboursHexTypes(npos), out aff);
                        }
                    }
                }
                //
                RecalculateTotalParameters();
                if (firstHex)
                {
                    uic.EnableTotalStatsPanel();
                    firstHex = false;
                }
                //
                npos = hpos.GetNextPosition();
                if (npos.ringIndex < RING_LIMIT) CreateHexMaquette(npos);
            }
        }

        public Hex GetHex(HexPosition hpos)
        {
            var b = hpos.ToBytes();
            if (hexList.ContainsKey(b)) return hexList[b];
            else return null;
        }       
        public void RecalculateTotalParameters()
        {
            totalPowerConsumption = 0f;
            totalIncome = 0f;
            totalLifepower = 0f;
            totalFoodProduction = 0f;
            totalPersonnelInvolved = 0;
            totalPersonnelSlots = 0;
            totalHousing = 0;
            if (hexList.Count > 0)
            {
                HexBuildingStats stats;
                foreach(var h in hexList.Values)
                {
                    stats = h.hexStats;
                    totalPowerConsumption += stats.powerConsumption;
                    totalIncome += stats.income;
                    totalLifepower += stats.lifepower;
                    totalFoodProduction += stats.foodProduction;
                    totalPersonnelInvolved += stats.personnelInvolved;
                    totalPersonnelSlots += stats.maxPersonnel;
                    totalHousing += stats.housing;
                }
            }
            freeColonists = colonistsCount - totalPersonnelInvolved;
            anchor?.SetEnergySurplus(totalPowerConsumption * ENERGY_CF);
            uic.RedrawStatsPanel();
        }

        private void EverydayUpdate()
        {
            const float moneyCf = 100f;            
            float x = colony.FORCED_GetEnergyCrystals(totalIncome * moneyCf);
        }

        #region positioning
        private Vector3 GetHexWorldPosition(HexPosition hpos)
        {
            return GetHexLocalPosition(hpos) + zeroPoint; 
        }
        private Vector3 GetHexLocalPosition(HexPosition hpos)
        {
            switch (hpos.ringIndex)
            {
                case 0: return GetMainDirectionVector(hpos.ringPosition) * 2f * innerRadius;

                default:
                    {
                        byte sector = hpos.DefineSector();
                        var dir = GetMainDirectionVector(sector) * (innerRadius * 2f * (hpos.ringIndex + 1));
                        dir += GetInsectorDirection(sector) * hpos.DefineInsectorPosition() * 2f * innerRadius;
                        return dir;
                    }
            }
        }
        private Vector3 GetMainDirectionVector(byte x)
        {
            return (Quaternion.AngleAxis(60f * x, Vector3.up) * Vector3.forward);
        }
        private Vector3 GetInsectorDirection(byte x)
        {
            // normalized
            // на больших расстояниях будет сказываться погрешность!
            switch (x)
            {
                case 0: return new Vector3(0.866025f, 0f, -0.5f).normalized;
                case 1: return Vector3.back;
                case 2: return new Vector3(-0.866025f, 0f, -0.5f).normalized;
                case 3: return new Vector3(-0.866025f, 0f, 0.5f).normalized;
                case 4: return Vector3.forward;
                case 5: return new Vector3(0.866025f, 0f, 0.5f).normalized;
                default: return Vector3.zero;
            }
        }
        public List<HexType> GetNeighboursHexTypes(HexPosition hpos)
        {
            var list = new List<HexType>();
            if (hexList.Count != 0)
            {
                var np = hpos.GetNeighbour(0).ToBytes();
                if (hexList.ContainsKey(np)) list.Add(hexList[np].type);
                np = hpos.GetNeighbour(1).ToBytes();
                if (hexList.ContainsKey(np)) list.Add(hexList[np].type);
                np = hpos.GetNeighbour(2).ToBytes();
                if (hexList.ContainsKey(np)) list.Add(hexList[np].type);
                np = hpos.GetNeighbour(3).ToBytes();
                if (hexList.ContainsKey(np)) list.Add(hexList[np].type);
                np = hpos.GetNeighbour(4).ToBytes();
                if (hexList.ContainsKey(np)) list.Add(hexList[np].type);
                np = hpos.GetNeighbour(5).ToBytes();
                if (hexList.ContainsKey(np)) list.Add(hexList[np].type);
            }
            return list;
        }
        #endregion

        private Hex LoadHexPref()
        {
            return Object.Instantiate(Resources.Load<GameObject>(FoundationRouteScenario.resourcesPath + "foundationHex")).AddComponent<Hex>();
        }
        public bool SendColonistsForWork(Hex h)
        {
            if (freeColonists > 0)
            {
                if (h.AddWorker())
                {
                    freeColonists--;
                    return true;
                }
            }
            return false;
        }
        public bool TryAddColonist()
        {
            if (totalHousing < colonistsCount)
            {
                colonistsCount++;
                freeColonists++;
                return true;
            }
            else return false;
        }
        public void FreeColonistFromWork()
        {
            freeColonists++;
        }

        #region save-load
        public void Save(System.IO.FileStream fs)
        {
            if (hexList.Count > 0)
            {
                fs.WriteByte(1);
                fs.Write(System.BitConverter.GetBytes(hexList.Count), 0, 4);
                foreach (var h in hexList.Values)
                {
                    h.Save(fs);
                }
            }
            else fs.WriteByte(0);
        }
        public void Load(System.IO.FileStream fs)
        {
            if (anchor == null)
            {
                anchor = scenario.anchorBasement;
            }
            if (hexList == null || hexList.Count != 0) hexList = new Dictionary<(byte, byte), Hex>();
            if (fs.ReadByte() == 1)
            {
                var data = new byte[4];
                fs.Read(data, 0, 4);
                int count = System.BitConverter.ToInt32(data, 0);                
                Hex h;
                for (int i = 0; i < count; i++)
                {
                    h = LoadHexPref();
                    h.Load(fs, this);
                    hexList.Add(h.hexPosition.ToBytes(), h);
                    h.transform.position = GetHexWorldPosition(h.hexPosition);
                }
                if (firstHex)
                {
                    uic.EnableTotalStatsPanel();
                    firstHex = false;
                }
            }
            RecalculateTotalParameters();
        }
        #endregion
    }
}

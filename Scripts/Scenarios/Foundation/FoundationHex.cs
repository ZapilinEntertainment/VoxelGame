using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FoundationRoute
{
    public struct HexPosition
    {
        public byte ringIndex, ringPosition;
        public static readonly HexPosition zer0 = new HexPosition(0, 0), unexist = new HexPosition(255, 255);
        public HexPosition Copy { get { return new HexPosition(ringIndex, ringPosition); } }

        public HexPosition(byte index, byte position)
        {
            ringIndex = index;
            ringPosition = position;
        }
        public HexPosition(int index, int position)
        {
            if (index < 0 || index > 255) { index = 255; Debug.Log("error in hexpositon index"); }
            if (position < 0 || position > 255) { position = 255; Debug.Log("error in hexposition pos"); }
            ringIndex = (byte)index;
            ringPosition = (byte)position;
        }
        public byte DefineSector()
        {
            if (ringIndex == 0) return ringPosition;
            else return (byte)(ringPosition / (ringIndex + 1));
        }
        public byte DefineInsectorPosition()
        {
            if (ringIndex == 0) return ringPosition;
            else
            {
                int segmentsCount = ringIndex + 1;
                int a = ringPosition / segmentsCount;
                if (ringPosition % segmentsCount == 0) return 0;
                else return (byte)(ringPosition - a * segmentsCount);
            }
        }
        public int InsectorToOnring(int ring, int sector, int pos)
        {
            return sector * (ring + 1) + pos;
        }
        public HexPosition GetNextPosition()
        {
            var sectors = ringIndex + 1;
            var inpos = DefineInsectorPosition();
            if (inpos == sectors - 1) return new HexPosition(ringIndex + 1, DefineSector() * (ringIndex + 2));
            else return new HexPosition(ringIndex, ringPosition + 1);
        }
        public HexPosition GetNeighbour(byte direction)
        {
            byte sector = DefineSector();
            var p = DefineInsectorPosition();
            int ring = 0, position = 0;
            switch (direction)
            {
                case 0: // up
                    {
                        switch (sector)
                        {
                            case 0:
                                ring = ringIndex + 1;
                                position = p;
                                break;
                            case 1:
                                {
                                    if (p == 0)
                                    {
                                        ring = ringIndex + 1;
                                        position = InsectorToOnring(ring, 0, ring);
                                    }
                                    else
                                    {
                                        ring = ringIndex;
                                        position = ringPosition - 1;
                                    }
                                    break;
                                }
                            case 2:
                                {
                                    if (p == 0)
                                    {
                                        ring = ringIndex;
                                        position = ringPosition - 1;
                                    }
                                    else
                                    {
                                        ring = ringIndex - 1;
                                        position = InsectorToOnring(ringIndex - 1, sector, p - 1);
                                    }
                                    break;
                                }
                            case 3:
                                {
                                    ring = ringIndex - 1;
                                    if (p == ringIndex)
                                    {
                                        position = InsectorToOnring(ringIndex - 1, sector + 1, 0);
                                    }
                                    else
                                    {
                                        position = InsectorToOnring(ringIndex - 1, 3, p);
                                    }
                                    break;
                                }
                            case 4:
                                ring = ringIndex;
                                position = ringPosition + 1;
                                break;
                            case 5:
                                ring = ringIndex + 1;
                                position = InsectorToOnring(ringIndex + 1, sector, p + 1);
                                break;
                        }
                        break;
                    }
                case 1: //up-right
                    {
                        switch (sector)
                        {
                            case 0:
                                ring = ringIndex + 1;
                                position = p + 1;
                                break;
                            case 1:
                                ring = ringIndex + 1;
                                position = InsectorToOnring(ringIndex + 1, sector, p);
                                break;
                            case 2:
                                if (p == 0)
                                {
                                    ring = ringIndex + 1;
                                    position = InsectorToOnring(ringIndex + 1, sector - 1, ringIndex + 1);
                                }
                                else
                                {
                                    ring = ringIndex;
                                    position = ringPosition - 1;
                                }
                                break;
                            case 3:
                                if (p == 0)
                                {
                                    ring = ringIndex;
                                    position = ringPosition - 1;
                                }
                                else
                                {
                                    ring = ringIndex - 1;
                                    position = InsectorToOnring(ring, sector, p - 1);
                                }
                                break;
                            case 4:
                                {
                                    ring = ringIndex - 1;
                                    if (p == ringIndex) position = InsectorToOnring(ring, sector + 1, 0);
                                    else position = InsectorToOnring(ring, sector, p);
                                    break;
                                }
                            case 5:
                                {
                                    ring = ringIndex;
                                    if (p == ringIndex) position = 0;
                                    else position = ringPosition + 1;
                                    break;
                                }
                        }
                        break;
                    }
                case 2: // right - down
                    {
                        switch (sector)
                        {
                            case 0:
                                {
                                    ring = ringIndex;
                                    position = ringPosition + 1;
                                    break;
                                }
                            case 1:
                                {
                                    ring = ringIndex + 1;
                                    position = InsectorToOnring(ring, sector, p + 1);
                                    break;
                                }
                            case 2:
                                {
                                    ring = ringIndex + 1;
                                    position = InsectorToOnring(ring, sector, p);
                                    break;
                                }
                            case 3:
                                {
                                    if (p == 0)
                                    {
                                        ring = ringIndex + 1;
                                        position = InsectorToOnring(ring, sector - 1, ring);
                                    }
                                    else
                                    {
                                        ring = ringIndex;
                                        position = ringPosition - 1;
                                    }
                                    break;
                                }
                            case 4:
                                {
                                    if (p == 0)
                                    {
                                        ring = ringIndex;
                                        position = ringPosition - 1;
                                    }
                                    else
                                    {
                                        ring = ringIndex - 1;
                                        position = InsectorToOnring(ring, sector, p - 1);
                                    }
                                    break;
                                }
                            case 5:
                                {
                                    ring = ringIndex - 1;
                                    if (p == ringIndex)
                                    {
                                        position = 0;
                                    }
                                    else
                                    {
                                        position = InsectorToOnring(ring, sector, p);
                                    }
                                    break;
                                }
                        }
                        break;
                    }
                case 3: // down 
                    {
                        switch (sector)
                        {
                            case 0:
                                {
                                    ring = ringIndex - 1;
                                    if (p == ringIndex) position = InsectorToOnring(ring, 1, 0);
                                    else position = InsectorToOnring(ring, 0, p);
                                    break;
                                }
                            case 1:
                                {
                                    ring = ringIndex;
                                    position = ringPosition + 1;
                                    break;
                                }
                            case 2:
                                {
                                    ring = ringIndex + 1;
                                    position = InsectorToOnring(ring, sector, p + 1);
                                    break;
                                }
                            case 3:
                                {
                                    ring = ringIndex + 1;
                                    position = InsectorToOnring(ring, sector, p);
                                    break;
                                }
                            case 4:
                                {
                                    if (p == 0)
                                    {
                                        ring = ringIndex + 1;
                                        position = InsectorToOnring(ring, sector - 1, ring);
                                    }
                                    else
                                    {
                                        ring = ringIndex;
                                        position = ringPosition - 1;
                                    }
                                    break;
                                }
                            case 5:
                                {
                                    if (p == 0)
                                    {
                                        ring = ringIndex;
                                        position = ringPosition - 1;
                                    }
                                    else
                                    {
                                        ring = ringIndex - 1;
                                        position = InsectorToOnring(ring, sector, p - 1);
                                    }
                                    break;
                                }
                        }
                        break;
                    }
                case 4: // left - down
                    {
                        switch (sector)
                        {
                            case 0:
                                {
                                    if (p == 0)
                                    {
                                        ring = ringIndex;
                                        position = (ringIndex + 1) * 6 - 1;
                                    }
                                    else
                                    {
                                        ring = ringIndex - 1;
                                        position = ringPosition - 1;
                                    }
                                    break;
                                }
                            case 1:
                                {
                                    ring = ringIndex - 1;
                                    if (p == ringIndex) position = InsectorToOnring(ring, sector + 1, 0);
                                    else position = InsectorToOnring(ring, sector, p);
                                    break;
                                }
                            case 2:
                                {
                                    ring = ringIndex;
                                    position = ringPosition + 1;
                                    break;
                                }
                            case 3:
                                {
                                    ring = ringIndex + 1;
                                    position = InsectorToOnring(ring, sector, p + 1);
                                    break;
                                }
                            case 4:
                                {
                                    ring = ringIndex + 1;
                                    position = InsectorToOnring(ring, sector, p);
                                    break;
                                }
                            case 5:
                                {
                                    if (p == 0)
                                    {
                                        ring = ringIndex + 1;
                                        position = InsectorToOnring(ring, sector - 1, ring);
                                    }
                                    else
                                    {
                                        ring = ringIndex;
                                        position = ringPosition - 1;
                                    }
                                    break;
                                }
                        }
                        break;
                    }
                case 5: // left - up
                    {
                        switch (sector)
                        {
                            case 0:
                                {
                                    if (p == 0)
                                    {
                                        ring = ringIndex + 1;
                                        position = InsectorToOnring(ring, 5, ring);
                                    }
                                    else
                                    {
                                        ring = ringIndex;
                                        position = ringPosition - 1;
                                    }
                                    break;
                                }
                            case 1:
                                {
                                    if (p == 0)
                                    {
                                        ring = ringIndex;
                                        position = ringPosition - 1;
                                    }
                                    else
                                    {
                                        ring = ringIndex - 1;
                                        position = InsectorToOnring(ring, sector, p - 1);
                                    }
                                    break;
                                }
                            case 2:
                                {
                                    ring = ringIndex - 1;
                                    if (p == ringIndex) position = InsectorToOnring(ring, sector + 1, 0);
                                    else position = InsectorToOnring(ring, sector, p);
                                    break;
                                }
                            case 3:
                                {
                                    ring = ringIndex;
                                    position = ringPosition + 1;
                                    break;
                                }
                            case 4:
                                {
                                    ring = ringIndex + 1;
                                    position = InsectorToOnring(ring, sector, p + 1);
                                    break;
                                }
                            case 5:
                                {
                                    ring = ringIndex + 1;
                                    position = InsectorToOnring(ring, sector, p);
                                    break;
                                }
                        }
                        break;
                    }
            }
            if (ring < 0 || position < 0) return unexist;
            else return new HexPosition(ring, position);
        }
        public (byte, byte) ToBytes()
        {
            return (ringIndex, ringPosition);
        }

        public static byte GetOppositeDirection(byte x)
        {
            int d = x + 3;
            if (d > 5) d -= 6;
            return (byte)d;
        }
    }
    
    public enum HexType : byte { Residential,ResidentialDense, ResidentialEco, Commercial, CommercialDense,
    Fields, AdvancedFields, Forest, Mountain, Lake, Industrial, IndustrialExperimental, Powerplant, TotalCount }

    public sealed class HexBuildingStats : MyObject
    {
        public readonly float powerConsumption;
        public readonly int personnel;
        public readonly float income;
        public readonly float lifepower;
        public readonly float foodProduction;
        public readonly int housing;
        private readonly ResourceContainer[] cost;
        public readonly string name;
        public readonly string description;
        private readonly HexType htype;

        protected override bool IsEqualNoCheck(object obj)
        {
            return htype == (obj as HexBuildingStats).htype;
        }

        private static HexBuildingStats[] allstats;

        public static HexBuildingStats GetStats(HexType type)
        {
            bool loadStat;
            int index = (int)type;
            if (allstats == null)
            {
                allstats = new HexBuildingStats[(int)HexType.TotalCount];
                loadStat = true;
            }
            else loadStat = allstats[index] == null;
            if (loadStat)
            {
                allstats[index] = new HexBuildingStats(type);                
            }
            return allstats[index];
        }

        private HexBuildingStats() { }
        private HexBuildingStats(HexType type) 
        {
            const float pwr = 1f, m = 1f, lp = 1f, f = 1f;
            const int ppl = 1, h = 1;
            switch (type)
            {
                case HexType.Residential:
                    powerConsumption = 4f * pwr;
                    personnel = 1 * ppl;
                    income = -3f * m;
                    lifepower = -1f * lp;
                    foodProduction = -10f * f;
                    housing = 10 * h;
                    break;
                case HexType.ResidentialDense:
                    powerConsumption = 10f * pwr;
                    personnel = 1 * ppl;
                    income = -8f * m;
                    lifepower = -2f * lp;
                    foodProduction = -22f * f;
                    housing = 15 * h;
                    break;
                case HexType.ResidentialEco:
                    powerConsumption = 2f * pwr;
                    personnel = 1 * ppl;
                    income = -4f * m;
                    lifepower = 0f;
                    foodProduction = -9f * f;
                    housing = 8 * h;
                    break;
                case HexType.Commercial:
                    powerConsumption = 15f * pwr;
                    personnel = 4 * ppl;
                    income = 10f * m;
                    lifepower = -5f * lp;
                    foodProduction = -4f * f;
                    housing = 0;
                    break;
                case HexType.CommercialDense:
                    powerConsumption = 30f * pwr;
                    personnel = 7 * ppl;
                    income = 40f * m;
                    lifepower = -8f * lp;
                    foodProduction = -8f * f;
                    housing = 0;
                    break;
                case HexType.Fields:
                    powerConsumption = 1f * pwr;
                    personnel = 3 * ppl;
                    income = 2f * m;
                    lifepower = 2f * lp;
                    foodProduction = 25f * f;
                    housing = 1 * h;
                    break;
                case HexType.AdvancedFields:
                    powerConsumption = 1.5f * pwr;
                    personnel = 4 * ppl;
                    income = 5f * m;
                    lifepower = 5f * lp;
                    foodProduction = 20f * f;
                    housing = 1* h;
                    break;
                case HexType.Forest:
                    powerConsumption = 1.5f * pwr;
                    personnel = 1 * ppl;
                    income = -2f * m;
                    lifepower = 10f * lp;
                    foodProduction = 5f * f;
                    housing = 1 * h;
                    break;
                case HexType.Mountain:
                    powerConsumption = 0f;
                    personnel = 1 * ppl;
                    income = -2f * m;
                    lifepower = 7f * lp;
                    foodProduction = f;
                    housing = 1 * h;
                    break;
                case HexType.Lake:
                    powerConsumption = 1f;
                    personnel = 1 * ppl;
                    income = -2f * m;
                    lifepower = 12f * lp;
                    foodProduction = 7f * f;
                    housing = 1 * h;
                    break;
                case HexType.Industrial:
                    powerConsumption = 30f * pwr;
                    personnel = 10 * ppl;
                    income = 5f * m;
                    lifepower = -30f * lp;
                    foodProduction = -2f * f;
                    housing = 0;
                    break;
                case HexType.IndustrialExperimental:
                    powerConsumption = 52f * pwr;
                    personnel = 12 * ppl;
                    income = 20f * m;
                    lifepower = -21f * lp;
                    foodProduction = -2f * f;
                    housing = 0;
                    break;
                case HexType.Powerplant:
                    powerConsumption = -70f * pwr;
                    personnel = 4 * ppl;
                    income = -10f * m;
                    lifepower = -14f * lp;
                    foodProduction = -1f * f;
                    housing = 1 * h;
                    break;
            }
        }
    }

    public sealed class Hex
    {
        public HexType type { get; private set; }
    }
}
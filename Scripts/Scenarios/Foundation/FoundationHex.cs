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
    public static class HexTypeExtension
    {
        public static HexSubtype DefineSubtype(this HexType htype)
        {
            switch (htype)
            {
                case HexType.Residential:
                case HexType.ResidentialDense:
                case HexType.ResidentialEco:
                    return HexSubtype.Residential;
                case HexType.Commercial:
                case HexType.CommercialDense:
                    return HexSubtype.Commercial;
                case HexType.Fields:
                case HexType.AdvancedFields:
                case HexType.Forest:
                case HexType.Mountain:
                case HexType.Lake:
                    return HexSubtype.Nature;
                case HexType.Industrial:
                case HexType.IndustrialExperimental:
                case HexType.Powerplant:
                    return HexSubtype.Industrial;
                default: return HexSubtype.NoSubtype;
            }
        }
        public static ColorCode DefineColorCode(this HexType htype)
        {
            switch (htype)
            {
                case HexType.Residential:
                case HexType.ResidentialDense:
                case HexType.ResidentialEco:
                case HexType.Commercial:
                case HexType.CommercialDense:
                    return ColorCode.Blue;
                case HexType.Fields:
                case HexType.AdvancedFields:
                case HexType.Forest:
                case HexType.Mountain:
                case HexType.Lake:
                    return ColorCode.Green;
                case HexType.Industrial:
                case HexType.IndustrialExperimental:
                case HexType.Powerplant:
                    return ColorCode.Red;
                default: return ColorCode.NoColor;
            }
        }
    }
    public enum HexSubtype: byte { NoSubtype, Residential, Commercial, Industrial, Nature}
    public enum ColorCode : byte { NoColor, Red, Green,Blue}

    public struct HexBuildingStats
    {
        // нумерация нужна для ui-подсветки строчек
        public float powerConsumption { get; private set; } // 0
        public int personnel { get; private set; }//1
        public float income { get; private set; } // 2
        public float lifepower { get; private set; }//3
        public float foodProduction { get; private set; }//4
        public int housing { get; private set; }//5
        private readonly ResourceContainer[] cost;
        private readonly HexType htype;
        public const byte POWER_INDEX = 0, PERSONNEL_INDEX = 1, INCOME_INDEX = 2, LIFEPOWER_INDEX = 3, FOOD_INDEX = 4, HOUSING_INDEX = 5;

        public HexBuildingStats(HexType type)
        {
            const float pwr = 1f, m = 1f, lp = 1f, f = 1f;
            const int ppl = 1, h = 1;
            htype = type;
            switch (type)
            {
                case HexType.Residential:
                    powerConsumption = 4f * pwr;
                    personnel = 1 * ppl;
                    income = -3f * m;
                    lifepower = -1f * lp;
                    foodProduction = -10f * f;
                    housing = 10 * h;
                    cost = new ResourceContainer[3]
                    {
                        new ResourceContainer(ResourceType.Concrete, 1000f),
                        new ResourceContainer(ResourceType.metal_K, 180f),
                        new ResourceContainer(ResourceType.Plastics, 1500f)
                    };
                    break;
                case HexType.ResidentialDense:
                    powerConsumption = 10f * pwr;
                    personnel = 1 * ppl;
                    income = -8f * m;
                    lifepower = -2f * lp;
                    foodProduction = -22f * f;
                    housing = 15 * h;
                    cost = new ResourceContainer[3]
                    {
                        new ResourceContainer(ResourceType.metal_P, 250f),
                        new ResourceContainer(ResourceType.metal_K, 300f),
                        new ResourceContainer(ResourceType.Plastics, 2000f)
                    };
                    break;
                case HexType.ResidentialEco:
                    powerConsumption = 2f * pwr;
                    personnel = 1 * ppl;
                    income = -4f * m;
                    lifepower = 0f;
                    foodProduction = -9f * f;
                    housing = 8 * h;
                    cost = new ResourceContainer[5]
                    {
                        new ResourceContainer(ResourceType.Concrete, 800f),
                        new ResourceContainer(ResourceType.metal_K, 100f),
                        new ResourceContainer(ResourceType.Plastics, 800f),
                        new ResourceContainer(ResourceType.metal_E, 200f),
                        new ResourceContainer(ResourceType.Graphonium, 25f),
                    };
                    break;
                case HexType.Commercial:
                    powerConsumption = 15f * pwr;
                    personnel = 4 * ppl;
                    income = 10f * m;
                    lifepower = -5f * lp;
                    foodProduction = -4f * f;
                    housing = 0;
                    cost = new ResourceContainer[4]
                    {
                        new ResourceContainer(ResourceType.Concrete, 2000f),
                        new ResourceContainer(ResourceType.metal_K, 700f),
                        new ResourceContainer(ResourceType.Plastics, 4000f),
                        new ResourceContainer(ResourceType.metal_E, 250f)
                    };
                    break;
                case HexType.CommercialDense:
                    powerConsumption = 30f * pwr;
                    personnel = 7 * ppl;
                    income = 40f * m;
                    lifepower = -8f * lp;
                    foodProduction = -8f * f;
                    housing = 0;
                    cost = new ResourceContainer[4]
                    {
                        new ResourceContainer(ResourceType.Concrete, 3000f),
                        new ResourceContainer(ResourceType.metal_K, 1000f),
                        new ResourceContainer(ResourceType.Plastics, 6500f),
                        new ResourceContainer(ResourceType.metal_E, 700f)
                    };
                    break;
                case HexType.Fields:
                    powerConsumption = 1f * pwr;
                    personnel = 3 * ppl;
                    income = 2f * m;
                    lifepower = 2f * lp;
                    foodProduction = 25f * f;
                    housing = 1 * h;
                    cost = new ResourceContainer[3]
                    {
                        new ResourceContainer(ResourceType.Dirt, 5000f),
                        new ResourceContainer(ResourceType.metal_K, 200f),
                        new ResourceContainer(ResourceType.metal_M, 300f)
                    };
                    break;
                case HexType.AdvancedFields:
                    powerConsumption = 1.5f * pwr;
                    personnel = 4 * ppl;
                    income = 5f * m;
                    lifepower = 5f * lp;
                    foodProduction = 15f * f;
                    housing = 1 * h;
                    cost = new ResourceContainer[5]
                    {
                        new ResourceContainer(ResourceType.Dirt, 5000f),
                        new ResourceContainer(ResourceType.metal_K, 400f),
                        new ResourceContainer(ResourceType.metal_M, 300f),
                        new ResourceContainer(ResourceType.metal_N_ore, 500f),
                        new ResourceContainer(ResourceType.mineral_F, 500f)
                    };
                    break;
                case HexType.Forest:
                    powerConsumption = 1.5f * pwr;
                    personnel = 1 * ppl;
                    income = -2f * m;
                    lifepower = 10f * lp;
                    foodProduction = 5f * f;
                    housing = 1 * h;
                    cost = new ResourceContainer[3]
                    {
                        new ResourceContainer(ResourceType.Dirt, 4000f),
                        new ResourceContainer(ResourceType.Lumber, 800f),
                        new ResourceContainer(ResourceType.mineral_F, 300f)
                    };
                    break;
                case HexType.Mountain:
                    powerConsumption = 0f;
                    personnel = 1 * ppl;
                    income = -2f * m;
                    lifepower = 7f * lp;
                    foodProduction = f;
                    housing = 1 * h;
                    cost = new ResourceContainer[3]
                    {
                        new ResourceContainer(ResourceType.Dirt, 1000f),
                        new ResourceContainer(ResourceType.Stone, 6000f),
                        new ResourceContainer(ResourceType.mineral_L, 700f)
                    };
                    break;
                case HexType.Lake:
                    powerConsumption = 1f;
                    personnel = 1 * ppl;
                    income = -2f * m;
                    lifepower = 12f * lp;
                    foodProduction = 7f * f;
                    housing = 1 * h;
                    cost = new ResourceContainer[3]
                    {
                        new ResourceContainer(ResourceType.Dirt, 1000f),
                        new ResourceContainer(ResourceType.Stone, 1000f),
                        new ResourceContainer(ResourceType.metal_N, 200f)
                    };
                    break;
                case HexType.Industrial:
                    powerConsumption = 30f * pwr;
                    personnel = 10 * ppl;
                    income = 5f * m;
                    lifepower = -30f * lp;
                    foodProduction = -2f * f;
                    housing = 0;
                    cost = new ResourceContainer[4]
                    {
                        new ResourceContainer(ResourceType.Concrete, 6000f),
                        new ResourceContainer(ResourceType.metal_K, 2200f),
                        new ResourceContainer(ResourceType.metal_M, 1500f),
                        new ResourceContainer(ResourceType.metal_E, 600f)
                    };
                    break;
                case HexType.IndustrialExperimental:
                    powerConsumption = 52f * pwr;
                    personnel = 12 * ppl;
                    income = 20f * m;
                    lifepower = -21f * lp;
                    foodProduction = -2f * f;
                    housing = 0;
                    cost = new ResourceContainer[5]
                    {
                        new ResourceContainer(ResourceType.Concrete, 5000f),
                        new ResourceContainer(ResourceType.metal_K, 2000f),
                        new ResourceContainer(ResourceType.metal_M, 2000f),
                        new ResourceContainer(ResourceType.metal_E, 1000f),
                        new ResourceContainer(ResourceType.Graphonium, 200f),
                    };
                    break;
                case HexType.Powerplant:
                    powerConsumption = -70f * pwr;
                    personnel = 4 * ppl;
                    income = -10f * m;
                    lifepower = -14f * lp;
                    foodProduction = -1f * f;
                    housing = 1 * h;
                    cost = new ResourceContainer[5]
                    {
                        new ResourceContainer(ResourceType.Concrete, 3000f),
                        new ResourceContainer(ResourceType.metal_K, 1000f),
                        new ResourceContainer(ResourceType.metal_M, 900f),
                        new ResourceContainer(ResourceType.metal_E, 1000f),
                        new ResourceContainer(ResourceType.metal_N, 500f),
                    };
                    break;
                default:
                    {
                        powerConsumption = 0f;
                        personnel = 0;
                        income = 0f;
                        lifepower = 0f;
                        foodProduction = 0f;
                        housing = 0;
                        cost = new ResourceContainer[0];
                        break;
                    }
            }
        }

        public ResourceContainer[] GetCost()
        {
            return cost;
        }


        public void ApplyNeighboursAffection(List<HexType> neitypes, out bool?[] affections)
        {
            HexSubtype nsubtype;
            const float lifepowerBoost = 0.25f, powerReducing = 0.8f, indPowerReducing = 0.9f,
                incomeBoost = 1.5f, incomeIndBoost = 1.2f;
            float[] multipliers = new float[6];
            multipliers[0] = 1f; multipliers[1] = 1f; multipliers[2] = 1f;
            multipliers[3] = 1f; multipliers[4] = 1f; multipliers[5] = 1f;

            if (neitypes.Count > 0)
            {
                switch (htype.DefineSubtype())
                {
                    case HexSubtype.Residential:
                        foreach (var n in neitypes)
                        {
                            if (n == HexType.Powerplant)
                            {
                                multipliers[POWER_INDEX] *= powerReducing;
                            }
                            else
                            {
                                nsubtype = n.DefineSubtype();
                                switch (nsubtype)
                                {
                                    case HexSubtype.Residential:
                                        multipliers[HOUSING_INDEX] *= 1.1f;
                                        break;
                                    case HexSubtype.Industrial:
                                        multipliers[LIFEPOWER_INDEX] *= (1f - lifepowerBoost / 2f);
                                        break;
                                    case HexSubtype.Nature:
                                        multipliers[LIFEPOWER_INDEX] *= (htype == HexType.ResidentialEco) ? (1f + lifepowerBoost * 1.5f) : (1f + lifepowerBoost);
                                        break;
                                }
                            }
                        }
                        break;
                    case HexSubtype.Commercial:
                        foreach (var n in neitypes)
                        {
                            if (n == HexType.Powerplant)
                            {
                                multipliers[POWER_INDEX] *= powerReducing;
                            }
                            else
                            {
                                nsubtype = n.DefineSubtype();
                                switch (nsubtype)
                                {
                                    case HexSubtype.Residential:
                                        multipliers[PERSONNEL_INDEX] *= 0.8f;
                                        break;
                                    case HexSubtype.Industrial:
                                        multipliers[INCOME_INDEX] *= incomeBoost;
                                        break;
                                }
                            }
                        }
                        break;
                    case HexSubtype.Industrial:
                        foreach (var n in neitypes)
                        {
                            if (n == HexType.Powerplant)
                            {
                                multipliers[POWER_INDEX] *= indPowerReducing;
                            }
                            else
                            {
                                if (n.DefineColorCode() == ColorCode.Blue)
                                {
                                    multipliers[INCOME_INDEX] *= incomeIndBoost;
                                }
                            }
                        }
                        break;
                    case HexSubtype.Nature:
                        ColorCode cc;
                        foreach (var n in neitypes)
                        {
                            cc = n.DefineColorCode();
                            if (cc == ColorCode.Red)
                            {
                                multipliers[LIFEPOWER_INDEX] *= 0.5f;
                                if (htype == HexType.AdvancedFields) multipliers[INCOME_INDEX] *= 1.2f;
                            }
                            else
                            {
                                if (cc == ColorCode.Green)
                                {
                                    multipliers[FOOD_INDEX] *= 1.2f;
                                    if (htype == HexType.Fields) multipliers[INCOME_INDEX] *= 1.1f;
                                }
                                else // blue
                                {
                                    if (n == HexType.ResidentialEco) multipliers[LIFEPOWER_INDEX] *= 1.1f;
                                }
                            }
                        }
                        break;

                }
            }
            affections = new bool?[6];
            float m = multipliers[0];
            if (m == 1f) affections[0] = null;
            else
            {
                if (m > 1f)
                {
                    affections[0] = true;
                    
                }
                else affections[0] = false;
                powerConsumption *= m;
            }
            //
            m = multipliers[1];
            if (m == 1f) affections[1] = null;
            else
            {
                if (m > 1f)
                {
                    affections[1] = true;

                }
                else affections[1] = false;
                personnel =(int)(personnel * m);
            }
            //
            m = multipliers[2];
            if (m == 1f) affections[2] = null;
            else
            {
                if (m > 1f)
                {
                    affections[2] = true;

                }
                else affections[2] = false;
                income *= m;
            }
            //
            m = multipliers[3];
            if (m == 1f) affections[3] = null;
            else
            {
                if (m > 1f)
                {
                    affections[3] = true;

                }
                else affections[3] = false;
                lifepower *= m;
            }
            //
            m = multipliers[4];
            if (m == 1f) affections[4] = null;
            else
            {
                if (m > 1f)
                {
                    affections[4] = true;

                }
                else affections[4] = false;
                foodProduction *= m;
            }
            //
            m = multipliers[5];
            if (m == 1f) affections[5] = null;
            else
            {
                if (m > 1f)
                {
                    affections[5] = true;

                }
                else affections[5] = false;
                housing = (int)(housing *m);
            }
        }

    }


    public sealed class Hex : MyObject
    {
        public HexType type { get; private set; }
        private readonly int ID;

        protected override bool IsEqualNoCheck(object obj)
        {
            return ID == (obj as Hex).ID;
        }


    }
}
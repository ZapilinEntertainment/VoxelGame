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
            if (ringIndex != 0)
            {
                var sectors = ringIndex + 1;
                var inpos = DefineInsectorPosition();
                if (inpos == sectors - 1) return new HexPosition(ringIndex + 1, DefineSector() * (ringIndex + 2));
                else return new HexPosition(ringIndex, ringPosition + 1);
            }
            else return new HexPosition(1, 2 * ringPosition);
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

    public class HexBuildingStats : MyObject
    {
        // нумерация нужна для ui-подсветки строчек
        public float powerConsumption { get; private set; } // 0
        private float _maxIncome; // 1
        private float _maxFoodProduction; // 2
        private float _maxLifepower; // 3
        public int personnelInvolved { get; private set; } // 4a
        public int maxPersonnel { get; private set; }      // 4b
        public int housing { get; private set; } // 5


        public float income { get { return _maxIncome * personnel_cf; } private set { _maxIncome = value; } }         
        public float lifepower { get { return _maxLifepower * personnel_cf; } private set { _maxLifepower = value; } }        
        public float foodProduction { get { return _maxFoodProduction * personnel_cf; } private set { _maxFoodProduction = value; } }      
        private float personnel_cf { get {
                if (maxPersonnel == 0) return 1f;
                else return (float)personnelInvolved / (float)maxPersonnel;
            } }
        private readonly ResourceContainer[] cost;
        private readonly HexType htype;
        public const byte POWER_INDEX = 0, PERSONNEL_INDEX = 1, INCOME_INDEX = 2, LIFEPOWER_INDEX = 3, FOOD_INDEX = 4, HOUSING_INDEX = 5;

        protected override bool IsEqualNoCheck(object obj)
        {
            var a = (HexBuildingStats)obj;
            return (htype == a.htype) && (powerConsumption == a.powerConsumption) && (personnelInvolved == a.personnelInvolved) &&
                (maxPersonnel == a.maxPersonnel) && (_maxIncome == a._maxIncome) && (_maxLifepower == a._maxLifepower)
                && (_maxFoodProduction == a._maxFoodProduction) && (housing == a.housing);
        }

        public HexBuildingStats(HexType type) : this(type, false) { }
        public HexBuildingStats(HexType type, bool fullPersonnel)
        {
            const float pwr = 1f, m = 1f, lp = 1f, f = 1f, costCf = 1000f;
            const int ppl = 1, h = 1;
            htype = type;
            personnelInvolved = 0;
            maxPersonnel = 0;
            switch (type)
            {
                case HexType.Residential:
                    maxPersonnel = 1 * ppl;
                    powerConsumption = 4f * pwr;                    
                    income = -3f * m;
                    lifepower = -1f * lp;
                    foodProduction = -10f * f;
                    housing = 10 * h;
                    cost = new ResourceContainer[3]
                    {
                        new ResourceContainer(ResourceType.Concrete, 5f * costCf),
                        new ResourceContainer(ResourceType.metal_K, 0.8f * costCf),
                        new ResourceContainer(ResourceType.Plastics, 2f * costCf)
                    };
                    break;
                case HexType.ResidentialDense:
                    maxPersonnel = 1 * ppl;
                    powerConsumption = 10f * pwr;                    
                    income = -8f * m;
                    lifepower = -2f * lp;
                    foodProduction = -22f * f;
                    housing = 15 * h;
                    cost = new ResourceContainer[3]
                    {
                        new ResourceContainer(ResourceType.metal_P, 0.25f * costCf),
                        new ResourceContainer(ResourceType.metal_K, 0.3f * costCf),
                        new ResourceContainer(ResourceType.Plastics, 4f * costCf)
                    };
                    break;
                case HexType.ResidentialEco:
                    maxPersonnel = 1 * ppl;
                    powerConsumption = 2f * pwr;                    
                    income = -4f * m;
                    lifepower = 0f;
                    foodProduction = -9f * f;
                    housing = 8 * h;
                    cost = new ResourceContainer[5]
                    {
                        new ResourceContainer(ResourceType.Concrete, 0.8f * costCf),
                        new ResourceContainer(ResourceType.metal_K, 0.3f * costCf),
                        new ResourceContainer(ResourceType.Plastics, 1.8f * costCf),
                        new ResourceContainer(ResourceType.metal_E, 0.3f * costCf),
                        new ResourceContainer(ResourceType.Graphonium, 0.025f * costCf),
                    };
                    break;
                case HexType.Commercial:
                    maxPersonnel = 4 * ppl;
                    powerConsumption = 15f * pwr;                    
                    income = 10f * m;
                    lifepower = -5f * lp;
                    foodProduction = -4f * f;
                    housing = 0;
                    cost = new ResourceContainer[4]
                    {
                        new ResourceContainer(ResourceType.Concrete, 2f * costCf),
                        new ResourceContainer(ResourceType.metal_K, 1.1f * costCf),
                        new ResourceContainer(ResourceType.Plastics, 4f * costCf),
                        new ResourceContainer(ResourceType.metal_E, 0.5f* costCf)
                    };
                    break;
                case HexType.CommercialDense:
                    maxPersonnel = 7 * ppl;
                    powerConsumption = 30f * pwr;                    
                    income = 40f * m;
                    lifepower = -8f * lp;
                    foodProduction = -8f * f;
                    housing = 0;
                    cost = new ResourceContainer[4]
                    {
                        new ResourceContainer(ResourceType.Concrete, 5f * costCf),
                        new ResourceContainer(ResourceType.metal_K, 1.5f * costCf),
                        new ResourceContainer(ResourceType.Plastics, 7f * costCf),
                        new ResourceContainer(ResourceType.metal_E, 0.7f * costCf)
                    };
                    break;
                case HexType.Fields:
                    maxPersonnel = 3 * ppl;
                    powerConsumption = 1f * pwr;                    
                    income = 2f * m;
                    lifepower = 2f * lp;
                    foodProduction = 25f * f;
                    housing = 1 * h;
                    cost = new ResourceContainer[3]
                    {
                        new ResourceContainer(ResourceType.Dirt, 5f * costCf),
                        new ResourceContainer(ResourceType.metal_K, 0.2f * costCf),
                        new ResourceContainer(ResourceType.metal_M, 0.3f * costCf)
                    };
                    break;
                case HexType.AdvancedFields:
                    maxPersonnel = 4 * ppl;
                    powerConsumption = 1.5f * pwr;                    
                    income = 5f * m;
                    lifepower = 5f * lp;
                    foodProduction = 15f * f;
                    housing = 1 * h;
                    cost = new ResourceContainer[5]
                    {
                        new ResourceContainer(ResourceType.Dirt, 5f * costCf),
                        new ResourceContainer(ResourceType.metal_K, 0.4f * costCf),
                        new ResourceContainer(ResourceType.metal_M, 0.3f * costCf),
                        new ResourceContainer(ResourceType.metal_N_ore, 0.5f * costCf),
                        new ResourceContainer(ResourceType.mineral_F, 0.5f * costCf)
                    };
                    break;
                case HexType.Forest:
                    maxPersonnel = 1 * ppl;
                    powerConsumption = 1.5f * pwr;                    
                    income = -2f * m;
                    lifepower = 10f * lp;
                    foodProduction = 5f * f;
                    housing = 1 * h;
                    cost = new ResourceContainer[3]
                    {
                        new ResourceContainer(ResourceType.Dirt, 4f * costCf),
                        new ResourceContainer(ResourceType.Lumber, 2f * costCf),
                        new ResourceContainer(ResourceType.mineral_F, 0.5f * costCf)
                    };
                    break;
                case HexType.Mountain:
                    maxPersonnel = 1 * ppl;
                    powerConsumption = 0f;                    
                    income = -2f * m;
                    lifepower = 7f * lp;
                    foodProduction = f;
                    housing = 1 * h;
                    cost = new ResourceContainer[3]
                    {
                        new ResourceContainer(ResourceType.Dirt, 3f * costCf),
                        new ResourceContainer(ResourceType.Stone, 7f * costCf),
                        new ResourceContainer(ResourceType.mineral_L, 0.7f * costCf)
                    };
                    break;
                case HexType.Lake:
                    maxPersonnel = 1 * ppl;
                    powerConsumption = 1f;                    
                    income = -2f * m;
                    lifepower = 12f * lp;
                    foodProduction = 7f * f;
                    housing = 1 * h;
                    cost = new ResourceContainer[3]
                    {
                        new ResourceContainer(ResourceType.Dirt, 3f * costCf),
                        new ResourceContainer(ResourceType.Stone, 2f * costCf),
                        new ResourceContainer(ResourceType.metal_N, 0.4f * costCf)
                    };
                    break;
                case HexType.Industrial:
                    maxPersonnel = 10 * ppl;
                    powerConsumption = 30f * pwr;                    
                    income = 5f * m;
                    lifepower = -30f * lp;
                    foodProduction = -2f * f;
                    housing = 0;
                    cost = new ResourceContainer[4]
                    {
                        new ResourceContainer(ResourceType.Concrete, 6f * costCf),
                        new ResourceContainer(ResourceType.metal_K, 3f * costCf),
                        new ResourceContainer(ResourceType.metal_M, 2f * costCf),
                        new ResourceContainer(ResourceType.metal_E, 0.8f * costCf)
                    };
                    break;
                case HexType.IndustrialExperimental:
                    maxPersonnel = 12 * ppl;
                    powerConsumption = 52f * pwr;                    
                    income = 20f * m;
                    lifepower = -21f * lp;
                    foodProduction = -2f * f;
                    housing = 0;
                    cost = new ResourceContainer[5]
                    {
                        new ResourceContainer(ResourceType.Concrete, 5f * costCf),
                        new ResourceContainer(ResourceType.metal_K, 3.2f * costCf),
                        new ResourceContainer(ResourceType.metal_M, 2.5f * costCf),
                        new ResourceContainer(ResourceType.metal_E, 1f * costCf),
                        new ResourceContainer(ResourceType.Graphonium, 0.2f * costCf),
                    };
                    break;
                case HexType.Powerplant:
                    maxPersonnel = 4 * ppl;
                    powerConsumption = -70f * pwr;                    
                    income = -10f * m;
                    lifepower = -14f * lp;
                    foodProduction = -1f * f;
                    housing = 1 * h;
                    cost = new ResourceContainer[5]
                    {
                        new ResourceContainer(ResourceType.Concrete, 3f * costCf),
                        new ResourceContainer(ResourceType.metal_K, 1f * costCf),
                        new ResourceContainer(ResourceType.metal_M, 1.3f * costCf),
                        new ResourceContainer(ResourceType.metal_E, 1f * costCf),
                        new ResourceContainer(ResourceType.metal_N, 0.5f * costCf),
                    };
                    break;
                default:
                    {
                        powerConsumption = 0f;
                        maxPersonnel = 0;
                        income = 0f;
                        lifepower = 0f;
                        foodProduction = 0f;
                        housing = 0;
                        cost = new ResourceContainer[0];
                        break;
                    }
            }
            if (fullPersonnel) personnelInvolved = maxPersonnel;
        }
        public HexBuildingStats(ICollection<Hex> collection) : this(HexType.TotalCount)
        {
            if (collection.Count > 0)
            {
                HexBuildingStats stats;
                foreach (var v in collection)
                {
                    stats = v.hexStats;
                    powerConsumption += stats.powerConsumption;
                    maxPersonnel += stats.maxPersonnel;
                    personnelInvolved += stats.personnelInvolved;
                    income += stats.income;
                    lifepower += stats.lifepower;
                    foodProduction += stats.foodProduction;
                    housing += stats.housing;
                }
            }
        }

        public ResourceContainer[] GetCost()
        {
            return cost;
        }

        public bool AddPersonnel()
        {
            if (personnelInvolved < maxPersonnel)
            {
                personnelInvolved++;
                return true;
            }
            else return false;
        }
        public bool RemovePersonnel()
        {
            if (maxPersonnel == 0) return false;
            else
            {
                personnelInvolved--;
                return true;
            }
        }
        public HexBuildingStats GetNoPersonnelCopy()
        {
            var ns = new HexBuildingStats(htype, false);
            ns.powerConsumption = powerConsumption;
            ns.maxPersonnel = maxPersonnel;
            ns._maxIncome = _maxIncome;
            ns._maxLifepower = _maxLifepower;
            ns._maxFoodProduction = _maxFoodProduction;
            ns.housing = housing;
            return ns;
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
                                if (htype != HexType.Powerplant) multipliers[POWER_INDEX] *= indPowerReducing;
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
                if (m > 1f && powerConsumption > 0f)
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
                maxPersonnel = (int)(maxPersonnel * m);
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
                housing = (int)(housing * m);
            }
        }
    }


    public sealed class Hex : MonoBehaviour
    {
        public HexType type { get; private set; }
        public HexBuildingStats hexStats { get; private set; }
        public HexPosition hexPosition { get; private set; }
        private HexBuilder hexBuilder;
        public int personnel { get; private set; }
        private float personnel_cf { get { return (float)personnel / (float)hexStats.maxPersonnel; } }
       
        private int ID = -1;
        private static int nextID = 1;
        
        public void Initialize(HexType htype, HexPosition i_hpos, HexBuilder i_hexBuilder, HexBuildingStats i_stats)
        {
            if (ID == -1)
            {
                ID = nextID++;
            }
            type = htype;
            hexPosition = i_hpos;
            hexBuilder = i_hexBuilder;
            hexStats = i_stats;

            GameObject model;
            Transform t;
            switch (type)
            {
                case HexType.Residential:
                    {
                        GetComponentInChildren<Renderer>().sharedMaterial = Resources.Load<Material>("Materials/Roads");
                        model = Instantiate(Resources.Load<GameObject>("Prefs/Special/residentialTower"));
                        t = model.transform;
                        t.parent = transform;
                        t.localPosition = Vector3.zero;
                        t.localRotation = Quaternion.Euler(0f, 60f * ((hexPosition.ringPosition + hexPosition.ringIndex) / 6), 0f);

                        Vector3 pos;
                        for (int r = 0; r < 4; r++)
                        {
                            int c = (r + 1) * 6;
                            float angle;
                            for (int a = 0; a < c; a++)
                            {
                                angle = 360f / c;
                                pos = Quaternion.AngleAxis(angle * a, Vector3.up) * Vector3.forward * (2 * r + 1f);
                                pos += new Vector3(Random.value, 0f, Random.value);
                                float scale = Random.value * 1.5f + 0.5f, scale2 = 1f + Random.value;
                                model = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                t = model.transform;
                                t.parent = transform;
                                t.localPosition = pos + Vector3.up * scale / 2f;
                                t.localScale = new Vector3(0.5f * scale2, scale, 0.5f * scale2);
                                t.localRotation = Quaternion.Euler(0f, Random.value * 180f, 0f);
                                //model.GetComponentInChildren<Renderer>().sharedMaterial = PoolMaster.glassMaterial;
                            }
                        }
                    }
                    break;
                case HexType.ResidentialDense:
                    {
                        GetComponentInChildren<Renderer>().sharedMaterial = Resources.Load<Material>("Materials/Roads");
                        model = Instantiate(Resources.Load<GameObject>("Prefs/Special/residentialDenseTower"));
                        t = model.transform;
                        t.parent = transform;
                        t.localPosition = Vector3.zero;
                        t.localRotation = Quaternion.Euler(0f, 60f * ((hexPosition.ringPosition + hexPosition.ringIndex) / 6), 0f);

                        Vector3 pos;
                        for (int r = 0; r < 4; r++)
                        {
                            int c = (r + 1) * 6;
                            float angle;
                            for (int a = 0; a < c; a++)
                            {
                                angle = 360f / c;
                                pos = Quaternion.AngleAxis(angle * a, Vector3.up) * Vector3.forward * (2 * r + 1f);
                                pos += new Vector3(Random.value, 0f, Random.value);
                                float scale = Random.value * 4f + 2f, scale2 = 1f + 2f *Random.value;
                                model = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                t = model.transform;
                                t.parent = transform;
                                t.localPosition = pos + Vector3.up * scale / 2f;
                                t.localScale = new Vector3(0.5f * scale2, scale, 0.5f * scale2);
                                t.localRotation = Quaternion.Euler(0f, Random.value * 180f, 0f);
                                //model.GetComponentInChildren<Renderer>().sharedMaterial = PoolMaster.glassMaterial;
                            }
                        }
                    }
                    break;
                case HexType.Commercial:
                    {
                        GetComponentInChildren<Renderer>().sharedMaterial = Resources.Load<Material>("Materials/Roads");

                        Vector3 pos;
                        for (int r = 0; r < 4; r++)
                        {
                            int c = (r + 1) * 6;
                            float angle;
                            for (int a = 0; a < c; a++)
                            {
                                model = GameObject.CreatePrimitive(PrimitiveType.Cube);                              

                                angle = 360f / c;
                                pos = Quaternion.AngleAxis(angle * a, Vector3.up) * Vector3.forward * (2 * r + 1f);
                                pos += new Vector3(Random.value, 0f, Random.value);
                                float scale = Random.value * 1.5f + 0.5f, scale2 = 1f + Random.value;
                                t = model.transform;
                                t.parent = transform;
                                t.localPosition = pos + Vector3.up * scale / 2f;
                                t.localScale = new Vector3(0.5f * scale2, scale, 0.5f * scale2);
                                t.localRotation = Quaternion.Euler(0f, Random.value * 180f, 0f);
                                if (Random.value > 0.5f)
                                {
                                    model.GetComponentInChildren<Renderer>().sharedMaterial = PoolMaster.glassMaterial;
                                    scale *= 2f;
                                }                               
                                
                            }
                        }
                        break;
                    }
                case HexType.CommercialDense:
                    {
                        GetComponentInChildren<Renderer>().sharedMaterial = Resources.Load<Material>("Materials/Roads");
                        Vector3 pos;
                        for (int r = 0; r < 4; r++)
                        {
                            int c = (r + 1) * 6;
                            float angle;
                            for (int a = 0; a < c; a++)
                            {
                                if (Random.value > 0.6f)
                                {
                                    model = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                    angle = 360f / c;
                                    pos = Quaternion.AngleAxis(angle * a, Vector3.up) * Vector3.forward * (2 * r + 1f);
                                    pos += new Vector3(Random.value, 0f, Random.value);
                                    float scale = Random.value * 5f + 2f, scale2 = 2f + Random.value;
                                    model.GetComponentInChildren<Renderer>().sharedMaterial = PoolMaster.glassMaterial;
                                    
                                    t = model.transform;
                                    t.parent = transform;
                                    t.localPosition = pos + Vector3.up * scale / 2f;
                                    t.localScale = new Vector3(0.5f * scale2, scale, 0.5f * scale2);
                                    t.localRotation = Quaternion.Euler(0f, Random.value * 180f, 0f);
                                }
                            }
                        }
                        break;
                    }
                case HexType.Industrial:
                case HexType.IndustrialExperimental:
                    {
                        bool simple = htype == HexType.Industrial;
                        GetComponentInChildren<Renderer>().sharedMaterial = Resources.Load<Material>("Materials/Roads");

                        const float R = 8f, height = R * 0.5f, hwidth = R * 0.6f;

                        model = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        var mf = model.GetComponentInChildren<MeshFilter>();
                        var mr = model.GetComponentInChildren<MeshRenderer>();
                        PoolMaster.SetMaterialByID(ref mf, ref mr, ResourceType.METAL_K_ID, 255);
                        float angle = Random.value * 360f;
                        var dir = Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward;
                        var npos = transform.position;

                        t = model.transform;
                        t.parent = transform;
                        var hangarpos = dir * R * 0.5f + Vector3.up * height * 0.5f;
                        t.localPosition = hangarpos;
                        t.LookAt(new Vector3(npos.x, t.position.y, npos.z), Vector3.up);
                        t.localScale = new Vector3(R * 0.5f, height, R * 0.8f);
                        if (!simple)
                        {
                            model = Instantiate(Resources.Load<GameObject>("Prefs/Special/industrialSpire"));
                            t = model.transform;
                            t.parent = transform;
                            t.localPosition = hangarpos + Quaternion.AngleAxis(Random.value * 360f, Vector3.up) * (Vector3.forward * (Random.value * 1f));
                            t.localRotation = Quaternion.Euler(0f, Random.value * 180f, 0f);
                        }
                        //
                        float step = simple ? 60f : 45f;
                        angle += 90f;
                        void MakeHangar()
                        {
                            angle += step * (0.8f + 0.8f * Random.value);
                            model = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            t = model.transform;
                            t.parent = transform;
                            t.localPosition = Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward * (hwidth * 0.5f);
                            t.LookAt(new Vector3(npos.x, t.position.y, npos.z), Vector3.up);
                            t.localScale = new Vector3(2f, 2f, hwidth);
                        }
                        MakeHangar();
                        MakeHangar();
                        if (!simple) MakeHangar();
                        for (int i = 0; i < 4+ Random.Range(0, 8); i++)
                        {
                            model = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            t = model.transform;
                            t.parent = transform;
                            t.localPosition = Quaternion.AngleAxis(Random.value * 360f, Vector3.up) * Vector3.forward * (Random.value * R);
                            t.rotation = Quaternion.Euler(0f, Random.value * 180f, 0f);
                            t.localScale = new Vector3(1f, 15f, 1f);
                        }                        
                        break;
                    }
                case HexType.Powerplant:
                    {
                        var mf = GetComponentInChildren<MeshFilter>();
                        var mr = GetComponentInChildren<MeshRenderer>();
                        PoolMaster.SetMaterialByID(ref mf, ref mr, ResourceType.CONCRETE_ID, 255);
                        //
                        model = Instantiate(Resources.Load<GameObject>("Prefs/Special/powerplant"));
                        t = model.transform;
                        t.parent = transform;
                        t.localPosition = Vector3.zero;
                        t.rotation = Quaternion.Euler(0f, Random.value * 180f, 0f);
                        break;
                    }
                case HexType.Forest:
                    {
                        break;
                        GetComponentInChildren<Renderer>().sharedMaterial = Resources.Load<Material>("Materials/Roads");
                        Vector3 pos;
                        for (int r = 0; r < 4; r++)
                        {
                            int c = (r + 1) * 6;
                            float angle;
                            for (int a = 0; a < c; a++)
                            {
                                    angle = 360f / c;
                                    pos = Quaternion.AngleAxis(angle * a, Vector3.up) * Vector3.forward * (2 * r + 1f);
                                    pos += new Vector3(Random.value, 0f, Random.value);
                                    float scale = Random.value * 5f + 2f, scale2 = 2f + Random.value;                                   
                                    //model = OakTree.GetModel();
                                    t = model.transform;
                                    t.parent = transform;
                                    t.localPosition = pos + Vector3.up * scale / 2f;
                                    t.localScale = new Vector3(0.5f * scale2, scale, 0.5f * scale2);
                                    t.localRotation = Quaternion.Euler(0f, Random.value * 180f, 0f);
                            }
                        }
                        break;
                    }

            }
            personnel = 0;
        }

        public float GetPowerConsumption()
        {
            return personnel_cf * hexStats.powerConsumption;
        }
        public float GetIncome()
        {
            return personnel_cf * hexStats.income;
        }
        public float GetLifepower()
        {
            return personnel_cf * hexStats.lifepower;
        }
        public float GetFoodProduction()
        {
            return personnel_cf * hexStats.foodProduction;
        }
    }
}
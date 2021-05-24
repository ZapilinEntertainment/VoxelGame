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

        public HexPosition(int index, byte position)
        {
            if (index < 0 || index > 255) { index = 255; Debug.Log("error in hexpositon index"); }
            ringIndex = (byte)index;
            ringPosition = position;
        }
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

        public override string ToString()
        {
            return "ring: " + ringIndex + ", position: " + ringPosition.ToString();
        }

        public void Save(System.IO.FileStream fs)
        {
            fs.WriteByte(ringIndex);
            fs.WriteByte(ringPosition);
        }
        public static HexPosition Load(System.IO.FileStream fs)
        {
            return new HexPosition(fs.ReadByte(), fs.ReadByte());
        }
    }
    
    public enum HexType : byte {
        Fields,AdvancedFields,Lake,Forest,Mountain,
        Residential,ResidentialDense, ResidentialEco, Commercial, CommercialDense,
        Industrial, IndustrialExperimental, Powerplant,
        TotalCount, DummyRed, DummyBlue, DummyGreen }
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
                case HexType.DummyBlue:
                    return ColorCode.Blue;
                case HexType.Fields:
                case HexType.AdvancedFields:
                case HexType.Forest:
                case HexType.Mountain:
                case HexType.Lake:
                case HexType.DummyGreen:
                    return ColorCode.Green;
                case HexType.Industrial:
                case HexType.IndustrialExperimental:
                case HexType.Powerplant:
                case HexType.DummyRed:
                    return ColorCode.Red;
                default: return ColorCode.NoColor;
            }
        }
        public static Rect GetHexIconRect(this HexType htype)
        {
            float x = 0.25f;
            switch (htype)
            {
                case HexType.Powerplant: return new Rect(x, 3f * x, x, x);
                case HexType.IndustrialExperimental: return new Rect(0f, 3f * x, x, x);
                case HexType.Industrial: return new Rect(3f * x, 2f * x, x, x);
                case HexType.CommercialDense: return new Rect(2f * x, 2f * x, x, x);
                case HexType.Commercial: return new Rect(x, 2f * x, x, x);
                case HexType.ResidentialEco: return new Rect(0f, 2f * x, x, x);
                case HexType.ResidentialDense: return new Rect(3f * x, x, x, x);
                case HexType.Residential: return new Rect(2f * x, x, x, x);
                case HexType.AdvancedFields: return new Rect(x, x, x, x);
                case HexType.Fields: return new Rect(0f, x, x, x);
                case HexType.Mountain: return new Rect(3f * x, 0f, x, x);
                case HexType.Lake: return new Rect(2f * x, 0f, x,x);
                case HexType.Forest: return new Rect(x, 0f, x, x);
                default: return new Rect(0f, 0, x, x);
            }
        }
    }
    public enum HexSubtype: byte { NoSubtype, Residential, Commercial, Industrial, Nature}
    public enum ColorCode : byte { NoColor, Red, Green,Blue}

    public class HexBuildingStats : MyObject
    {
        // нумерация нужна для ui-подсветки строчек
        
        private float _maxPowerConsumption;// 0
        private float _maxIncome; // 1
        private float _maxFoodProduction; // 2
        private float _maxLifepower; // 3
        public int personnelInvolved { get; private set; } // 4a
        public int maxPersonnel { get; private set; }      // 4b
        public int housing { get; private set; } // 5

        public float powerConsumption
        {
            get { if (maxPersonnel != 0) return _maxPowerConsumption * (0.2f +0.8f * personnel_cf); else return _maxPowerConsumption; }
            private set { _maxPowerConsumption = value; }
        }
        public float income {
            get { if (_maxIncome > 0f) return _maxIncome * personnel_cf; else return _maxIncome; }
            private set { _maxIncome = value; } }         
        public float lifepower {
            get { if (_maxLifepower > 0f) return _maxLifepower * personnel_cf; else return _maxLifepower; }
            private set { _maxLifepower = value; } }        
        public float foodProduction {
            get { if (_maxFoodProduction > 0f) return _maxFoodProduction * personnel_cf; else return _maxFoodProduction; }
            private set { _maxFoodProduction = value; } }      
        private float personnel_cf { get {
                if (maxPersonnel == 0) return 1f;
                else return (float)personnelInvolved / (float)maxPersonnel;
            } }
        private readonly ResourceContainer[] cost;
        public readonly HexType htype;
        public enum Stats : byte { PowerConsumption, Income, FoodProduction, LifepowerProduction, Personnel, Housing}
        public static int POWER_INDEX { get { return (int)Stats.PowerConsumption; } }
        public static int INCOME_INDEX { get { return (int)Stats.Income; } }
        public static int FOOD_INDEX { get { return (int)Stats.FoodProduction; } }
        public static int LIFEPOWER_INDEX { get { return (int)Stats.LifepowerProduction; } }
        public static int PERSONNEL_INDEX { get { return (int)Stats.Personnel; } }
        public static int HOUSING_INDEX { get { return (int)Stats.Housing; } }

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
                    powerConsumption = 4f * pwr;                    
                    income = -3f * m;
                    lifepower = -1f * lp;
                    foodProduction = -10f * f;
                    housing = 5 * h;
                    cost = new ResourceContainer[3]
                    {
                        new ResourceContainer(ResourceType.Concrete, 5f * costCf),
                        new ResourceContainer(ResourceType.metal_K, 0.8f * costCf),
                        new ResourceContainer(ResourceType.Plastics, 2f * costCf)
                    };
                    break;
                case HexType.ResidentialDense:
                    powerConsumption = 10f * pwr;                    
                    income = -8f * m;
                    lifepower = -2f * lp;
                    foodProduction = -22f * f;
                    housing = 8 * h;
                    cost = new ResourceContainer[3]
                    {
                        new ResourceContainer(ResourceType.metal_P, 0.25f * costCf),
                        new ResourceContainer(ResourceType.metal_K, 0.3f * costCf),
                        new ResourceContainer(ResourceType.Plastics, 4f * costCf)
                    };
                    break;
                case HexType.ResidentialEco:
                    powerConsumption = 2f * pwr;                    
                    income = -4f * m;
                    lifepower = 0f;
                    foodProduction = -9f * f;
                    housing = 3 * h;
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
                    powerConsumption = 0f;                    
                    income = -2f * m;
                    lifepower = 10f * lp;
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
                    powerConsumption = 1f;                    
                    income = -2f * m;
                    lifepower = 10f * lp;
                    foodProduction = 10f * f;
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
                    lifepower = -25f * lp;
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
                    powerConsumption = -100f * pwr;                    
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
            var basicStats = new HexBuildingStats(htype);

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
                                        multipliers[LIFEPOWER_INDEX] *= (htype == HexType.ResidentialEco) ? 0.7f : 0.5f;
                                        break;
                                    default:
                                        {
                                            if (n == HexType.DummyBlue)
                                            {
                                                multipliers[HOUSING_INDEX] *= 1.5f;
                                                multipliers[FOOD_INDEX] *= 0.8f;
                                            }
                                            break;
                                        }
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
                                    default:
                                        {
                                            if (n == HexType.DummyBlue)
                                            {
                                                multipliers[INCOME_INDEX] *= 1.5f;
                                            }
                                            break;
                                        }
                                }
                            }
                        }
                        break;
                    case HexSubtype.Industrial:
                        if (htype != HexType.Powerplant)
                        {
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
                                    else
                                    {
                                        if (n == HexType.DummyRed)
                                        {
                                            multipliers[POWER_INDEX] *= 0.5f;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (var n in neitypes)
                            {
                                if (n == HexType.Powerplant) continue;
                                else
                                {
                                    if (n == HexType.DummyRed)
                                    {
                                        multipliers[POWER_INDEX] *= 1.5f;
                                    }
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
                                    if (htype == HexType.Fields | htype == HexType.AdvancedFields)
                                    {
                                        multipliers[FOOD_INDEX] *= 1.2f;
                                        multipliers[INCOME_INDEX] *= 1.1f;
                                    }
                                    else multipliers[LIFEPOWER_INDEX] *= 1.2f;
                                    if (n == HexType.DummyGreen)
                                    {
                                        multipliers[LIFEPOWER_INDEX] *= 1.5f;
                                    }
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
            int i = POWER_INDEX;
            float m = multipliers[i];
            bool x;
            if (m == 1f || _maxPowerConsumption == 0f) affections[i] = null;
            else
            {
                x = _maxPowerConsumption > 0f;
                if (m > 1f) affections[i] = !x;
                else affections[i] = x;                
            }
            _maxPowerConsumption = basicStats._maxPowerConsumption * m;
           //
            i = INCOME_INDEX;
            m = multipliers[i];
            if (m == 1f || _maxIncome == 0f) affections[i] = null;
            else
            {
                x = _maxIncome > 0;
                if (m > 1f) affections[i] = x;
                else affections[i] = !x;               
            }
            _maxIncome = basicStats._maxIncome * m;
            //
            i = FOOD_INDEX;
            m = multipliers[i];
            if (m == 1f || _maxFoodProduction == 0f) affections[i] = null;
            else
            {
                x = _maxFoodProduction > 0;
                if (m > 1f) affections[i] = x;
                else affections[i] = !x;
            }
            _maxFoodProduction = basicStats._maxFoodProduction * m;
            //
            i = LIFEPOWER_INDEX;
            m = multipliers[i];
            if (m == 1f || _maxLifepower == 0f) affections[i] = null;
            else
            {
                x = _maxLifepower > 0;
                if (m > 1f) affections[i] = x;
                else affections[i] = !x;
            }
            _maxLifepower = basicStats._maxLifepower * m;
            //
            i = PERSONNEL_INDEX;
            m = multipliers[i];
            if (m == 1f || maxPersonnel == 0) affections[i] = null;
            else
            {
                if (m > 1f)
                {
                    affections[i] = false;
                }
                else affections[i] = true;
            }
            maxPersonnel = (int)(basicStats.maxPersonnel * m);
            //
            i = HOUSING_INDEX;
            m = multipliers[i];
            if (m == 1f || housing == 0) affections[i] = null;
            else
            {
                if (m > 1f)
                {
                    affections[i] = true;
                }
                else affections[i] = false;
            }
            housing = (int)(basicStats.housing * m);
        }

        public float GetMaxPowerConsumption() { return _maxPowerConsumption; }
        public float GetMaxIncome() { return _maxIncome; }
        public float GetMaxLifepower() { return _maxLifepower; }
        public float GetMaxFoodProduction() { return _maxFoodProduction; }

        #region save-load
        public void Save(System.IO.FileStream fs)
        {
            fs.WriteByte((byte)htype);
            fs.Write(System.BitConverter.GetBytes(_maxPowerConsumption),0,4);
            fs.Write(System.BitConverter.GetBytes(_maxIncome), 0, 4);
            fs.Write(System.BitConverter.GetBytes(_maxFoodProduction), 0, 4);
            fs.Write(System.BitConverter.GetBytes(_maxLifepower), 0, 4);
            fs.Write(System.BitConverter.GetBytes(personnelInvolved), 0, 4);
            fs.Write(System.BitConverter.GetBytes(maxPersonnel), 0, 4);
            fs.Write(System.BitConverter.GetBytes(housing), 0, 4);
        }
        public static HexBuildingStats Load(System.IO.FileStream fs)
        {
            const int length = 29;
            var data = new byte[length];
            fs.Read(data, 0, length);
            var hbs = new HexBuildingStats((HexType)data[0]);
            hbs._maxPowerConsumption = System.BitConverter.ToSingle(data, 1);
            hbs._maxIncome = System.BitConverter.ToSingle(data, 5);
            hbs._maxFoodProduction = System.BitConverter.ToSingle(data, 9);
            hbs._maxLifepower = System.BitConverter.ToSingle(data, 13);
            hbs.personnelInvolved = System.BitConverter.ToInt32(data, 17);
            hbs.maxPersonnel = System.BitConverter.ToInt32(data, 21);
            hbs.housing = System.BitConverter.ToInt32(data, 25);
            return hbs;
        }
        #endregion
    }


    public sealed class Hex : MonoBehaviour
    {
        public HexType type { get { return hexStats.htype; } }
        public HexBuildingStats hexStats { get; private set; }
        public HexPosition hexPosition { get; private set; }
        private HexBuilder hexBuilder;
       
        private int ID = -1;
        private static int nextID = 1;
        
        public void Initialize( HexPosition i_hpos, HexBuilder i_hexBuilder, HexBuildingStats i_stats)
        {
            if (ID == -1)
            {
                ID = nextID++;
            }
            hexPosition = i_hpos;
            hexBuilder = i_hexBuilder;
            hexStats = i_stats;

            bool clickable = true;
            GameObject model, collider = gameObject;
            Transform t, mytransform = transform; 
            GameObject GetCube()
            {
                return Resources.Load<GameObject>("Prefs/5cube");
            }

            switch (type)
            {
                case HexType.Residential:
                    {
                        GetComponentInChildren<Renderer>().sharedMaterial = Resources.Load<Material>("Materials/Roads");
                        transform.GetChild(0).localRotation = Quaternion.Euler(0f, Random.Range(0, 6) * 60f, 0f);
                        model = Instantiate(Resources.Load<GameObject>(FoundationRouteScenario.resourcesPath + "residentialTower"));
                        t = model.transform;
                        t.parent = transform;
                        t.localPosition = Vector3.zero;
                        t.localRotation = Quaternion.Euler(0f, 60f * ((hexPosition.ringPosition + hexPosition.ringIndex) / 6), 0f);

                        Vector3 pos;
                        var example = GetCube();
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
                                model = Instantiate(example, mytransform);
                                t = model.transform;
                                t.localPosition = pos + Vector3.up * scale / 2f;
                                t.localScale = new Vector3(0.5f * scale2, scale, 0.5f * scale2);
                                t.localRotation = Quaternion.Euler(0f, Random.value * 180f, 0f);
                                // не добавляем тени
                            }
                        }
                        break;
                    }
                case HexType.ResidentialEco:
                    {
                        GetComponentInChildren<Renderer>().sharedMaterial = Resources.Load<Material>("Materials/Roads");
                        transform.GetChild(0).localRotation = Quaternion.Euler(0f, Random.Range(0, 6) * 60f, 0f);
                        model = Instantiate(Resources.Load<GameObject>(FoundationRouteScenario.resourcesPath + "residentialTower"));
                        t = model.transform;
                        t.parent = transform;
                        t.localPosition = Vector3.zero;
                        t.localRotation = Quaternion.Euler(0f, 60f * ((hexPosition.ringPosition + hexPosition.ringIndex) / 6), 0f);

                        var example = GetCube();
                        void CreateHouse(Vector3 pos)
                        {
                            model = Instantiate(example, mytransform) ;
                            t = model.transform;
                            float scale = Random.value * 1.5f + 0.5f, scale2 = 1f + Random.value;
                            t.localPosition = pos + Vector3.up * scale / 2f;
                            t.localScale = new Vector3(0.5f * scale2, scale, 0.5f * scale2);
                            t.localRotation = Quaternion.Euler(0f, Random.value * 180f, 0f);
                        }
                        for (int i = 0; i< 6; i++)
                        {                           
                            if (Random.value > 0.5f)
                            {
                                var dir = Quaternion.AngleAxis(i * 60f, Vector3.up) * Vector3.forward * 1.5f;
                                CreateHouse(dir);
                                dir *= 2f;
                                var x = Quaternion.AngleAxis(90f, Vector3.up) * dir.normalized * 1.5f;
                                CreateHouse(dir + x); CreateHouse(dir - x);
                                dir *= 2f;
                                CreateHouse(dir + x);CreateHouse(dir + 2f * x);
                                CreateHouse(dir - x); CreateHouse(dir - 2f * x);
                            }
                            else
                            {
                                t = Instantiate(Resources.Load<GameObject>(FoundationRouteScenario.resourcesPath + "forestSector")).transform;
                                t.parent = transform;
                                t.localPosition = Vector3.zero;
                                t.localRotation = Quaternion.Euler(0f, i * 60f, 0f);
                            }
                        }
                    }
                    break;
                case HexType.ResidentialDense:
                    {
                        GetComponentInChildren<Renderer>().sharedMaterial = Resources.Load<Material>("Materials/Roads");
                        transform.GetChild(0).localRotation = Quaternion.Euler(0f, Random.Range(0, 6) * 60f, 0f);
                        model = Instantiate(Resources.Load<GameObject>(FoundationRouteScenario.resourcesPath + "residentialDenseTower"));
                        t = model.transform;
                        t.parent = transform;
                        t.localPosition = Vector3.zero;
                        t.localRotation = Quaternion.Euler(0f, 60f * ((hexPosition.ringPosition + hexPosition.ringIndex) / 6), 0f);

                        Vector3 pos;
                        var example = GetCube();
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
                                model = Instantiate(example, mytransform);
                                t = model.transform;
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
                        transform.GetChild(0).localRotation = Quaternion.Euler(0f, Random.Range(0, 6) * 60f, 0f);

                        Vector3 pos;
                        var example = GetCube();
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
                                model = Instantiate(example, mytransform);
                                t = model.transform;
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
                        transform.GetChild(0).localRotation = Quaternion.Euler(0f, Random.Range(0, 6) * 60f, 0f);
                        Vector3 pos;
                        var example = GetCube();
                        for (int r = 0; r < 4; r++)
                        {
                            int c = (r + 1) * 6;
                            float angle;
                            for (int a = 0; a < c; a++)
                            {
                                if (Random.value > 0.6f)
                                {
                                    model = Instantiate(example, mytransform);
                                    angle = 360f / c;
                                    pos = Quaternion.AngleAxis(angle * a, Vector3.up) * Vector3.forward * (2 * r + 1f);
                                    pos += new Vector3(Random.value, 0f, Random.value);
                                    float scale = Random.value * 5f + 2f, scale2 = 2f + Random.value;
                                    model.GetComponentInChildren<Renderer>().sharedMaterial = PoolMaster.glassMaterial;
                                    
                                    t = model.transform;
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
                        var example = GetCube();
                        bool simple = type == HexType.Industrial;
                        GetComponentInChildren<Renderer>().sharedMaterial = Resources.Load<Material>("Materials/Roads");
                        transform.GetChild(0).localRotation = Quaternion.Euler(0f, Random.Range(0, 6) * 60f, 0f);

                        const float R = 8f, height = R * 0.35f, hwidth = R * 0.6f;

                        model = Instantiate(example, mytransform);
                        var mf = model.GetComponentInChildren<MeshFilter>();
                        var mr = model.GetComponentInChildren<MeshRenderer>();
                        PoolMaster.SetMaterialByID(ref mf, ref mr, ResourceType.METAL_K_ID, 255);
                        float angle = Random.value * 360f;
                        var dir = Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward;
                        var npos = transform.position;

                        t = model.transform;
                        var hangarpos = dir * R * 0.5f + Vector3.up * height * 0.5f;
                        t.localPosition = hangarpos;
                        t.LookAt(new Vector3(npos.x, t.position.y, npos.z), Vector3.up);
                        t.localScale = new Vector3(R * 0.5f, height, R * 0.8f);
                        if (!simple)
                        {
                            model = Instantiate(Resources.Load<GameObject>(FoundationRouteScenario.resourcesPath + "industrialSpire"));
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
                            model = Instantiate(example, mytransform);
                            t = model.transform;
                            t.localPosition = Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward * (hwidth * 0.5f);
                            t.LookAt(new Vector3(npos.x, t.position.y, npos.z), Vector3.up);
                            t.localScale = new Vector3(2f, height * 0.8f, hwidth);
                        }
                        MakeHangar();
                        MakeHangar();
                        if (!simple) MakeHangar();
                        for (int i = 0; i < 4+ Random.Range(0, 8); i++)
                        {
                            model = Instantiate(example, mytransform);
                            mf = model.GetComponentInChildren<MeshFilter>();
                            mr = model.GetComponentInChildren<MeshRenderer>();
                            PoolMaster.SetMaterialByID(ref mf, ref mr, ResourceType.METAL_M_ID, 255);
                            t = model.transform;
                            t.localPosition = Quaternion.AngleAxis(Random.value * 360f, Vector3.up) * Vector3.forward * (Random.value * R*0.8f);
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
                        model = Instantiate(Resources.Load<GameObject>(FoundationRouteScenario.resourcesPath + "powerplant"));
                        t = model.transform;
                        t.parent = transform;
                        t.localPosition = Vector3.zero;
                        t.rotation = Quaternion.Euler(0f, Random.value * 180f, 0f);
                        break;
                    }
                case HexType.Forest:
                    {
                        RemoveStandartModel();
                        collider = PrepareHexWalls();
                        var g = PrepareHexRoof();
                        var mf = g.GetComponentInChildren<MeshFilter>();
                        var mr = g.GetComponentInChildren<MeshRenderer>();
                        PoolMaster.SetMaterialByID(ref mf, ref mr, PoolMaster.MATERIAL_GRASS_100_ID, 255);
                        //
                        t = Instantiate(Resources.Load<GameObject>(FoundationRouteScenario.resourcesPath + "forest")).transform;
                        t.parent = transform;
                        t.localPosition = Vector3.zero;
                        t.localRotation = Quaternion.Euler(0f, Random.Range(0, 6) * 60f, 0f);
                        break;
                    }
                case HexType.Fields:
                    {
                        RemoveStandartModel();
                        collider = PrepareHexWalls();
                        PrepareHexRoof().GetComponentInChildren<Renderer>().sharedMaterial = Resources.Load<Material>("Materials/fields");
                        t = InstantiateSiloModel().transform;
                        t.parent = transform;
                        t.localPosition = Vector3.zero;
                        t.localRotation = Quaternion.Euler(0f, Random.value * 360f, 0f);
                        break;
                    }
                case HexType.AdvancedFields:
                    {
                        RemoveStandartModel();
                        collider = PrepareHexWalls();
                        PrepareHexRoof().GetComponentInChildren<Renderer>().sharedMaterial = Resources.Load<Material>("Materials/experimentalFields");
                        t = InstantiateSiloModel().transform;
                        t.parent = transform;
                        t.localPosition = Vector3.zero;
                        t.localRotation = Quaternion.Euler(0f, Random.value * 360f, 0f);
                        t = Instantiate(Resources.Load<GameObject>(FoundationRouteScenario.resourcesPath + "advancedFieldLab")).transform;
                        t.parent = transform;
                        t.localPosition = Quaternion.AngleAxis(Random.value * 360f, Vector3.up) * Vector3.forward * 2f;
                        t.localRotation = Quaternion.Euler(0f, Random.value * 360f, 0f);
                        break;
                    }
                case HexType.Mountain:
                    {
                        RemoveStandartModel();
                        collider = PrepareHexWalls();
                        t = Instantiate(Resources.Load<GameObject>(FoundationRouteScenario.resourcesPath + "mountain")).transform;
                        t.parent = transform;
                        t.localPosition = Vector3.zero;
                        t.localRotation = Quaternion.Euler(0f, 60f * Random.Range(0, 6), 0f);
                        break;
                    }
                case HexType.Lake:
                    {
                        RemoveStandartModel();
                        collider = PrepareHexWalls();
                        t = Instantiate(Resources.Load<GameObject>(FoundationRouteScenario.resourcesPath + "lake")).transform;
                        t.parent = transform;
                        t.localPosition = Vector3.zero;
                        t.localRotation = Quaternion.Euler(0f, 60f * Random.Range(0, 6), 0f);
                        break;
                    }
                case HexType.DummyBlue:
                case HexType.DummyGreen:
                case HexType.DummyRed:
                    {
                        clickable = false;
                        model = Instantiate(Resources.Load<GameObject>(FoundationRouteScenario.resourcesPath + "nullhex"), mytransform);
                        t = model.transform;
                        t.localPosition = Vector3.zero;
                        t.localRotation = Quaternion.Euler(0f, 60f * hexPosition.ringPosition, 0f);
                        var rrs = model.GetComponentsInChildren<Renderer>();
                        var replacingMaterial = Instantiate(Resources.Load<Material>("Materials/Coloured"));
                        Color col;
                        if (type == HexType.DummyBlue) col = Color.blue;
                        else
                        {
                            if (type == HexType.DummyGreen) col = Color.green;
                            else col = Color.red;
                        }
                        replacingMaterial.color = Color.Lerp(col, Color.white, 0.5f);
                        var clr_name = PoolMaster.GetColouredMaterialName();
                        int i,l;
                        foreach (var r in rrs)
                        {
                            l = r.sharedMaterials.Length;
                            if (l== 1)
                            {
                                if (r.sharedMaterial.name == clr_name) r.sharedMaterial = replacingMaterial;
                            }
                            else
                            {
                                for (i = 0; i< l; i++)
                                {
                                    if (r.sharedMaterials[i].name == clr_name)
                                    {
                                        var nmats = new Material[l];
                                        for (int j = 0; j< l; j++)
                                        {
                                            nmats[j] = r.sharedMaterials[j];
                                        }
                                        nmats[i] = replacingMaterial;
                                        r.sharedMaterials = nmats;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    break;
                default:
                    {
                        clickable = false;
                        break;
                    }
            }

            var me = this;
            if (clickable) collider.GetComponentInChildren<ClickableObject>().AssignFunction(() => hexBuilder.uic.OpenHexWindow(me), hexBuilder.uic.CloseButton);
            else
            {
                var co = collider.GetComponentInChildren<ClickableObject>();
                if (co != null) Destroy(co);
            }
        }
        private GameObject PrepareHexWalls()
        {
            var g =  Instantiate(Resources.Load<GameObject>(FoundationRouteScenario.resourcesPath + "hexWalls"));
            var t = g.transform;
            t.parent = transform;
            t.localPosition = Vector3.zero;
            return g;
        }
        private GameObject PrepareHexRoof()
        {
            var g = Instantiate(Resources.Load<GameObject>(FoundationRouteScenario.resourcesPath + "hexRoof"));
            var t = g.transform;
            t.parent = transform;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.Euler(0f, Random.Range(0, 6) * 60f, 0f);
            return g;
        }
        private GameObject InstantiateSiloModel()
        {
            return Instantiate(Resources.Load<GameObject>(FoundationRouteScenario.resourcesPath + "basicSilo"));
        }
        private void RemoveStandartModel()
        {
            Destroy(transform.GetChild(0).gameObject);
        }

        public bool AddWorker()
        {
            if (hexStats.personnelInvolved != hexStats.maxPersonnel)
            {
                hexStats.AddPersonnel();
                hexBuilder.RecalculateTotalParameters();
                return true;
            }
            return false;
        }
        public bool RemoveWorker()
        {
            if (hexStats.personnelInvolved != 0)
            {
                hexStats.RemovePersonnel();
                hexBuilder.RecalculateTotalParameters();
                return true;
            }
            return false;
        }

        #region save-load
        public void Save(System.IO.FileStream fs)
        {
            hexPosition.Save(fs);
            hexStats.Save(fs);            
        }
        public void Load(System.IO.FileStream fs, HexBuilder i_builder)
        {
            Initialize(HexPosition.Load(fs), i_builder, HexBuildingStats.Load(fs));
        }
        #endregion
    }
}
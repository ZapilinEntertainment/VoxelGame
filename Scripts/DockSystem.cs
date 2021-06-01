using System.Collections.Generic;
using UnityEngine;
public sealed class DockSystem 
{
    private static DockSystem current;
    public bool?[] isForSale { get; private set; }
    public int[] minValueForTrading { get; private set; }
    public bool immigrationEnabled { get; private set; }
    public int immigrationPlan { get; private set; }    
    public uint immigrantsArrived = 0, emigrantsGone = 0;

    #region save-load system
    public static void SaveDockSystem(System.IO.Stream fs)
    {
        if (current == null)
        {
            fs.WriteByte(0);
            return;
        }
        else
        {
            fs.WriteByte(1);
            current.Save(fs);
        }
    }
    public static void LoadDockSystem(System.IO.Stream fs, int saveVersion)
    {
        current = null;
        int x = fs.ReadByte();
        if (x == 0) {
            return;
        }
        else
        {
            current = new DockSystem();
            current.Load(fs);
        }
    }
    private void Save(System.IO.Stream fs)
    {
        byte truebyte = 1, falsebyte = 0, nullbyte = 2;        
        byte resCount = ResourceType.TYPES_COUNT;
        fs.WriteByte(resCount);
        for (int i = 0; i < resCount; i++)
        {
            if (isForSale[i] == null)
            {
                fs.WriteByte(nullbyte);
                continue;
            }
            else
            {
                fs.WriteByte(isForSale[i] == true ? truebyte : falsebyte);
                fs.Write(System.BitConverter.GetBytes(minValueForTrading[i]),0,4);
            }
        }
        //
        fs.WriteByte(immigrationEnabled ? truebyte : falsebyte);
        fs.Write(System.BitConverter.GetBytes(immigrationPlan),0,4);
        fs.Write(System.BitConverter.GetBytes(immigrantsArrived), 0, 4);
        fs.Write(System.BitConverter.GetBytes(emigrantsGone), 0, 4);
    }   
    private void Load(System.IO.Stream fs)
    {
        int resCount = fs.ReadByte();
        isForSale = new bool?[resCount];
        minValueForTrading = new int[resCount];
        int x;
        var data = new byte[4];
        for (int i = 0; i < resCount; i++)
        {
            x = fs.ReadByte();
            if (x == 2) {
                isForSale[i] = null;
                minValueForTrading[i] = 0;
            }
            else
            {
                isForSale[i] = x == 1 ? true : false;
                fs.Read(data, 0, 4);
                minValueForTrading[i] = System.BitConverter.ToInt32(data, 0);
            }
        }
        //
        data = new byte[13];
        fs.Read(data, 0, data.Length);
        immigrationEnabled = data[0] == 1;
        immigrationPlan = System.BitConverter.ToInt32(data, 1);
        immigrantsArrived = System.BitConverter.ToUInt32(data, 5);
        emigrantsGone = System.BitConverter.ToUInt32(data, 9);
    }
    #endregion

    private DockSystem()
    {
        SYSTEM_RESET();
    }
    private void SYSTEM_RESET()
    {
        var resCount = ResourceType.TYPES_COUNT;
        isForSale = new bool?[resCount];
        for (int i = 0; i < resCount; i++) isForSale[i] = null;
        minValueForTrading = new int[resCount];
        immigrationPlan = 0;
        immigrationEnabled = true;
    }
    public static void ResetRequest()
    {
        current?.SYSTEM_RESET();
    }

    public static DockSystem GetCurrent()
    {
        if (current == null)
        {
            current = new DockSystem();
        }
        return current;
    }
    public static uint GetImmigrantsTotalCount()
    {
        if (current == null) return 0;
        else return current.immigrantsArrived;
    }

    public void HandleShip(Dock d, Ship s, ColonyController colony) {		
		int peopleBefore = immigrationPlan;
        float efficientcy = (float)d.workersCount / (float)d.maxWorkers; 
        float tradeVolume = s.volume * (0.05f + 0.95f * efficientcy);
        float rewardValue = 1f;
        var storage = colony.storage;
        switch (s.type) {
		case ShipType.Passenger:
                {
                    float vol = s.volume * (Random.value * 0.5f * colony.happinessCoefficient + 0.5f);
                    if (immigrationEnabled && immigrationPlan > 0)
                    {
                        if (vol > immigrationPlan)
                        {
                            colony.AddCitizens(immigrationPlan, true);
                            immigrationPlan = 0;
                            vol -= immigrationPlan;
                        }
                        else
                        {
                            int x = (int)vol;
                            colony.AddCitizens(x, true); immigrationPlan -= x;
                            vol = 0;
                        }
                    }
                    else
                    {
                        vol = 0f;
                        if (Random.value < colony.happinessCoefficient * 0.25f)
                        {
                            int x = (int)(Random.value * colony.hq.level);
                            if (x != 0) colony.AddCitizens(x, true);
                        }
                    }

                    if (vol > 0)
                    {
                        vol *= colony.happinessCoefficient;
                        if (vol > 1f) Hotel.DistributeLodgers((int)vol);
                        rewardValue += 0.5f;
                    }

                    if (isForSale[ResourceType.FOOD_ID] != null)
                    {
                        if (isForSale[ResourceType.FOOD_ID] == true) d.SellResource(ResourceType.Food, s.volume * 0.1f);
                        else d.BuyResource(ResourceType.Food, s.volume * 0.1f);
                        rewardValue += 0.3f * Random.value;
                    }
                    break;
                }
		case ShipType.Cargo:
                {
                    int totalPositions = 0;
                    List<int> buyPositions = new List<int>(), sellPositions = new List<int>();
                    for (int i = 0; i < ResourceType.TYPES_COUNT; i++)
                    {
                        if (isForSale[i] == null) continue;
                        if (isForSale[i] == true) // продаваемый островом ресурс
                        {                            
                            if (storage.GetResourceCount(i) > minValueForTrading[i])
                            {
                                sellPositions.Add(i);
                                totalPositions++;
                            }
                        }
                        else // покупаемый островом ресурс
                        {
                            if (storage.GetResourceCount(i) <= minValueForTrading[i])
                            {
                                buyPositions.Add(i);
                                totalPositions++;
                            }
                        }
                    }

                    float pc =  tradeVolume * 1f / (float)totalPositions;
                    float v, a;
                    if (buyPositions.Count > 0)
                    {
                        foreach (int id in buyPositions)
                        {
                            v = pc * (0.9f + 0.2f * Random.value);
                            a = storage.GetResourceCount(id);
                            if (a + v > minValueForTrading[id]) v = minValueForTrading[id] - a;
                            if (v > 0)
                            {
                                d.BuyResource(ResourceType.GetResourceTypeById(id), v);
                                rewardValue += 0.2f;
                            }
                        }
                    }
                    else rewardValue += 0.1f;

                    if (sellPositions.Count > 0)
                    {
                        foreach (int id in sellPositions)
                        {
                            v = pc * (0.9f + 0.2f * Random.value);
                            a = storage.GetResourceCount(id);
                            if (a - v <= minValueForTrading[id]) v = a - minValueForTrading[id];
                            if (v > 0)
                            {
                                d.SellResource(ResourceType.GetResourceTypeById(id), v);
                                rewardValue += 0.2f;
                            }
                        }
                    }
                    else rewardValue += 0.1f;

                    if (d.ID != Structure.DOCK_ID)
                    {
                        if (d.ID == Structure.DOCK_2_ID) rewardValue *= 2f;
                        else
                        {
                            if (d.ID == Structure.DOCK_3_ID) rewardValue *= 3f;
                        }
                    }
                    break;
                }
		case ShipType.Military:
                {
                    rewardValue += 1f;
                    if (GameMaster.realMaster.warProximity < 0.5f && Random.value < 0.1f && immigrationPlan > 0)
                    {
                        int veterans = (int)(s.volume * 0.02f);
                        if (veterans > immigrationPlan) veterans = immigrationPlan;
                        colony.AddCitizens(veterans, true);
                    }
                    if (isForSale[ResourceType.FUEL_ID] == true) {
                        float tv = (float)(tradeVolume * 0.5f * (Random.value * 0.5f + 0.5f));
                        if (tv != 0) d.SellResource(ResourceType.Fuel, tv);
                            };
                    if (GameMaster.realMaster.warProximity > 0.5f)
                    {
                        if (isForSale[ResourceType.METAL_S_ID] == true) d.SellResource(ResourceType.metal_S, s.volume * 0.1f);
                        if (isForSale[ResourceType.METAL_K_ID] == true) d.SellResource(ResourceType.metal_K, s.volume * 0.05f);
                        if (isForSale[ResourceType.METAL_M_ID] == true) d.SellResource(ResourceType.metal_M, s.volume * 0.1f);
                    }
                    break;
                }
		case ShipType.Private:
                rewardValue += 0.1f;
			if ( isForSale[ResourceType.FUEL_ID] == true) d.SellResource(ResourceType.Fuel, (float)(tradeVolume * 0.8f));
			if ( isForSale[ResourceType.FOOD_ID] == true) d.SellResource(ResourceType.Fuel, (float)(tradeVolume * 0.15f));
			break;
		}		

		int newPeople = peopleBefore - immigrationPlan;
        if (newPeople != 0)
        {
            if (newPeople > 0)
            {
                AnnouncementCanvasController.MakeAnnouncement(Localization.GetPhrase(LocalizedPhrase.ColonistsArrived) + " (" + newPeople.ToString() + ')');
                immigrantsArrived += (uint)newPeople;
                Knowledge.GetCurrent()?.ImmigrantsCheck(immigrantsArrived);
            }
            else
            {
                emigrantsGone += (uint)(newPeople * (-1));
            }
        }

        rewardValue += d.workersCount * 0.1f ;
        colony.AddEnergyCrystals(rewardValue * GameConstants.PER_DOCKED_SHIP_BASIC_REWARD * GameMaster.realMaster.GetDifficultyCoefficient());
	}

    public void SetImmigrationStatus(bool x, int count)
    {
        immigrationEnabled = x;
        immigrationPlan = count;
    }
    public void ChangeMinValue(int index, int val)
    {
        minValueForTrading[index] = val;
    }
    public void ChangeSaleStatus(int index, bool? val)
    {
        if (isForSale[index] == val) return;
        else
        {
            isForSale[index] = val;
        }
    }   
}

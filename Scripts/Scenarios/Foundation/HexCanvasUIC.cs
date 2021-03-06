﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FoundationRoute
{
    public sealed class HexCanvasUIC : MonoBehaviour
    {
        [SerializeField] private GameObject constructionWindow, statsPanel, upPanel, workersPanel;
        [SerializeField] private GameObject[] costLines;
        [SerializeField] private Button buildButton;
        [SerializeField] private Button[] buildingButtons;
        [SerializeField] private Text[] stats;
        [SerializeField] private Text nameField, descriptionField;
        [SerializeField] private Transform conditionLine0, conditionLine1;
        private GameObject conditionPanel { get { return conditionLine0.parent.gameObject; } }
        private GameObject costPanel { get { return costLines[0].transform.parent.gameObject; } }
        private Button workersPanel_minusButton { get { return workersPanel.transform.GetChild(0).GetComponent<Button>(); } }
        private Button workersPanel_plusButton { get { return workersPanel.transform.GetChild(1).GetComponent<Button>(); } }
        private Text workersPanel_label { get { return workersPanel.transform.GetChild(2).GetComponent<Text>(); } }
        private bool buildmode = true;
        private HexBuilder hexBuilder;
        private HexPosition selectedPosition;
        private HexType selectedType = HexType.Residential;
        private Hex selectedHex;
        private ColonyController colony;
        private HexBuildingStats selectedStats;
        private int[] buildingsCount = new int[(int)HexType.TotalCount];
        private int livingQuartersCount, commCount, natureCount, indCount;
        private BitArray availabilityMask = new BitArray((int)HexType.TotalCount, true);
        private Rect taskIncompletedRect, taskCompletedRect;
        private string[] conditionStrings, buildingsInfo;
        private const byte RES2_LQ_COUNT = 2, RES3_NAT_COUNT = 2, COM2_COM_COUNT = 2, PP_LQ_COUNT = 3, IND_EXP_NAME_INDEX = 20,
            MOUNTAIN_NAME_INDEX = 15, POWERPLANT_NAME_INDEX = 22,RESIDENTIAL_ECO_NAME_INDEX = 4;


        public void Prepare(HexBuilder hb)
        {
            hexBuilder = hb;
            colony = GameMaster.realMaster.colonyController;
            constructionWindow.SetActive(false);
            statsPanel.SetActive(false);
            buildingsCount = new int[(int)HexType.TotalCount];
            RecalculateAvailabilityMask();
            hexBuilder.scenario.LoadHexInfo(out conditionStrings, out buildingsInfo);
            taskIncompletedRect = UIController.GetIconUVRect(Icons.TaskFrame);
            taskCompletedRect = UIController.GetIconUVRect(Icons.TaskCompleted);
        }
        public void RecalculateAvailabilityMask()
        {
            hexBuilder.CountBuildingsByTypes(ref buildingsCount);
            livingQuartersCount = buildingsCount[(int)HexType.Residential] + buildingsCount[(int)HexType.ResidentialDense] + buildingsCount[(int)HexType.ResidentialEco];
            commCount = buildingsCount[(int)HexType.Commercial] + buildingsCount[(int)HexType.CommercialDense];
            natureCount = buildingsCount[(int)HexType.Forest] + buildingsCount[(int)HexType.Mountain] + buildingsCount[(int)HexType.Lake];
            indCount = buildingsCount[(int)HexType.Industrial] + buildingsCount[(int)HexType.IndustrialExperimental];
            //
            bool? a, b;
            void CheckConditions(HexType htype)
            {
                CheckBuildingConditions(htype, out a, out b);
                availabilityMask[(int)htype] = (a == null) || ((a == true) & (b == true | b == null));
            }
            //
            if (hexBuilder.GetBuildingsCount() >= hexBuilder.hexLimit)
            {
                for (int i = 0; i < (int)HexType.TotalCount; i++)
                {
                    availabilityMask[i] = false;
                }
                availabilityMask[(int)HexType.Industrial] = true;
                CheckConditions(HexType.IndustrialExperimental);
            }
            else
            {
                availabilityMask[(byte)HexType.Residential] = true;
                CheckConditions(HexType.ResidentialDense);
                CheckConditions(HexType.ResidentialEco);
                availabilityMask[(byte)HexType.Commercial] = true;
                CheckConditions(HexType.CommercialDense);
                availabilityMask[(byte)HexType.Fields] = true;
                CheckConditions(HexType.AdvancedFields);
                availabilityMask[(byte)HexType.Lake] = true;
                availabilityMask[(byte)HexType.Forest] = true;
                CheckConditions(HexType.Mountain);
                availabilityMask[(byte)HexType.Industrial] = true;
                CheckConditions(HexType.IndustrialExperimental);
                CheckConditions(HexType.Powerplant);
            }
        }
        private void CheckBuildingConditions(HexType type, out bool? a, out bool? b)
        {
            //dependency - WriteConditionsText
            switch (type)
            {
                case HexType.ResidentialDense:
                    a = (livingQuartersCount >= RES2_LQ_COUNT);
                    b = (commCount > 0);
                    break;
                case HexType.ResidentialEco:
                    a = natureCount >= RES3_NAT_COUNT;
                    b = null;
                    break;
                case HexType.CommercialDense:
                    a = (commCount >= COM2_COM_COUNT);
                    b = (indCount >0);
                    break;
                case HexType.AdvancedFields:
                    a = buildingsCount[(int)HexType.IndustrialExperimental] > 0;
                    b = buildingsCount[(int)HexType.Mountain] > 0;
                    break;
                case HexType.Mountain:
                    a = indCount > 0;
                    b = null;
                    break;
                case HexType.IndustrialExperimental:
                    a = buildingsCount[(int)HexType.Powerplant] > 0;
                    b = buildingsCount[(int)HexType.ResidentialEco] > 0;
                    break;
                case HexType.Powerplant:
                    a = livingQuartersCount >= PP_LQ_COUNT;
                    b = null;
                    break;
                default: a = null; b = null; break;
            }
        }
        private void WriteConditionsText(Text t, HexType htype, byte stage)
        {
            switch (htype)
            {
                case HexType.ResidentialDense:
                    if (stage == 0) t.text = conditionStrings[0] + ": " + livingQuartersCount.ToString() + '/' + RES2_LQ_COUNT.ToString();
                    else t.text = conditionStrings[1] + ": " + commCount.ToString() +"/1";
                    break;
                case HexType.ResidentialEco:
                    t.text = conditionStrings[2] + ": " + natureCount.ToString() + '/' + RES3_NAT_COUNT.ToString();
                    break;
                case HexType.CommercialDense:
                    if (stage == 0) t.text = conditionStrings[1] + ": " + commCount.ToString() + '/' + COM2_COM_COUNT.ToString();
                    else t.text = conditionStrings[3] + ": " + indCount.ToString() + "/1";
                    break;
                case HexType.AdvancedFields:
                    if (stage == 0) t.text = buildingsInfo[IND_EXP_NAME_INDEX];
                    else t.text = buildingsInfo[MOUNTAIN_NAME_INDEX];
                    break;
                case HexType.Mountain:
                    t.text = conditionStrings[3] + ": " + indCount.ToString() + "/1";
                    break;
                case HexType.IndustrialExperimental:
                    if (stage == 0) t.text = buildingsInfo[POWERPLANT_NAME_INDEX];
                    else t.text = buildingsInfo[RESIDENTIAL_ECO_NAME_INDEX];
                    break;
                case HexType.Powerplant:
                    t.text = conditionStrings[0] + ": " + livingQuartersCount.ToString() + '/' + PP_LQ_COUNT.ToString();
                    break;
            }
        }
        private void WriteBuildingInfo(HexType type, Text name, Text description)
        {
            switch (type)
            {
                case HexType.Residential:
                    name.text = buildingsInfo[0];
                    description.text = buildingsInfo[1];
                    break;
                case HexType.ResidentialDense:
                    name.text = buildingsInfo[2];
                    description.text = buildingsInfo[3];
                    break;
                case HexType.ResidentialEco:
                    name.text = buildingsInfo[RESIDENTIAL_ECO_NAME_INDEX];
                    description.text = buildingsInfo[5];
                    break;
                case HexType.Commercial:
                    name.text = buildingsInfo[6];
                    description.text = buildingsInfo[7];
                    break;
                case HexType.CommercialDense:
                    name.text = buildingsInfo[8];
                    description.text = buildingsInfo[9];
                    break;
                case HexType.Fields:
                    name.text = buildingsInfo[10];
                    description.text = buildingsInfo[11];
                    break;
                case HexType.AdvancedFields:
                    name.text = buildingsInfo[12];
                    description.text = buildingsInfo[13];
                    break;
                case HexType.Forest:
                    name.text = buildingsInfo[14];
                    description.text = buildingsInfo[17];
                    break;
                case HexType.Mountain:
                    name.text = buildingsInfo[MOUNTAIN_NAME_INDEX];
                    description.text = buildingsInfo[17];
                    break;
                case HexType.Lake:
                    name.text = buildingsInfo[16];
                    description.text = buildingsInfo[17];
                    break;
                case HexType.Industrial:
                    name.text = buildingsInfo[18];
                    description.text = buildingsInfo[19];
                    break;
                case HexType.IndustrialExperimental:
                    name.text = buildingsInfo[IND_EXP_NAME_INDEX];
                    description.text = buildingsInfo[21];
                    break;
                case HexType.Powerplant:
                    name.text = buildingsInfo[POWERPLANT_NAME_INDEX];
                    description.text = buildingsInfo[23];
                    break;
                case HexType.TotalCount:
                    name.text = buildingsInfo[24];
                    if (description != null) { description.text = buildingsInfo[24]; }
                    break;
            }

        }

        public void OpenHexWindow(Hex h)
        {
            SwitchBuildmode(false);
            selectedStats = h.hexStats;
            selectedPosition = h.hexPosition;
            selectedHex = h;
            var affectionsList = new bool?[6] { null, null, null, null, null,null};            
            FillStatsPanel(affectionsList);
            WriteBuildingInfo(h.type, nameField, descriptionField);
            var hs = h.hexStats;
            if (hs.maxPersonnel == 0)
            {
                workersPanel.SetActive(false);
            }
            else
            {
                int pi = hs.personnelInvolved, mi = hs.maxPersonnel ;
                workersPanel_label.text = pi.ToString() + " / " + mi.ToString();
                workersPanel_minusButton.interactable = (pi != 0);
                workersPanel_plusButton.interactable = (pi != mi);
                workersPanel.SetActive(true);
            }
            if (!constructionWindow.activeSelf) constructionWindow.SetActive(true);
        }
        public void OpenConstructionWindow(HexPosition hpos)
        {
            selectedHex = null;
            selectedPosition = hpos;
            
            SwitchBuildmode(true);
            RedrawConstructionWindow();
            if (!constructionWindow.activeSelf) constructionWindow.SetActive(true);
        }
        private void SwitchBuildmode(bool x)
        {
            if (x != buildmode)
            {
                buildmode = x;
                upPanel.SetActive(buildmode);
                conditionPanel.SetActive(buildmode);
                costPanel.SetActive(buildmode);
                buildButton.gameObject.SetActive(buildmode);
                workersPanel.SetActive(!buildmode);
            }
        }
        public void EnableTotalStatsPanel()
        {
            statsPanel.SetActive(true);
        }

        private void FillStatsPanel(bool?[] affectionsList)
        {          
            int POWER_INDEX = HexBuildingStats.POWER_INDEX, INCOME_INDEX = HexBuildingStats.INCOME_INDEX,
                FOOD_INDEX = HexBuildingStats.FOOD_INDEX, LIFEPOWER_INDEX = HexBuildingStats.LIFEPOWER_INDEX,
                PERSONNEL_INDEX = HexBuildingStats.PERSONNEL_INDEX, HOUSING_INDEX = HexBuildingStats.HOUSING_INDEX;
            //
            Text s;
            void DisplayAffection(int i)
            {
                if (affectionsList[i] == null) s.color = Color.white;
                else
                {
                    if (affectionsList[i] == false) s.color = Color.red;
                    else s.color = Color.green;
                }
            }
            //
            float pc = selectedStats.powerConsumption;            
            if (pc == 0f) stats[POWER_INDEX].enabled = false;
            else
            {
                s = stats[POWER_INDEX];
                if (pc > 0f) s.text = '-' + string.Format("{0:0.##}", pc);
                else s.text = '+' + string.Format("{0:0.##}", pc * (-1f));
                DisplayAffection(POWER_INDEX);
                if (!s.enabled) s.enabled = true;
            }
            // 1 - income
            pc = selectedStats.income;
            s = stats[INCOME_INDEX];
            if (pc > 0f) s.text = '+' + string.Format("{0:0.##}", pc);
            else s.text = string.Format("{0:0.##}", pc);
            DisplayAffection(INCOME_INDEX);
            // 2 - food
            pc = selectedStats.foodProduction;
            s = stats[FOOD_INDEX];
            if (pc > 0f) s.text = '+' + string.Format("{0:0.##}", pc);
            else s.text = string.Format("{0:0.##}", pc);
            DisplayAffection(FOOD_INDEX);
            // 3 - lifepower
            pc = selectedStats.lifepower;
            s = stats[LIFEPOWER_INDEX];
            if (pc > 0f) s.text = '+' + string.Format("{0:0.##}", pc);
            else s.text = string.Format("{0:0.##}", pc);
            DisplayAffection(LIFEPOWER_INDEX);
            // 4 - personnel & maxpersonnel
            s = stats[PERSONNEL_INDEX];
            s.text = selectedStats.maxPersonnel.ToString();
            DisplayAffection(PERSONNEL_INDEX);
            // 5 - housing           
            s = stats[HOUSING_INDEX];
            if (selectedStats.housing == 0) s.enabled = false;
            else
            {
                s.text = selectedStats.housing.ToString();
                DisplayAffection(HOUSING_INDEX);
                if (!s.enabled) s.enabled = true;
            }
        }
        private void RedrawConstructionWindow()
        {
            RecalculateAvailabilityMask();
            int selected = (int)selectedType;
            bool buildingAvailable = availabilityMask[selected], buildConditionsMet = true;
            for (int i = 0; i< (int)HexType.TotalCount; i++)
            {
                if (availabilityMask[i])
                {
                    buildingButtons[i].GetComponent<Image>().color = i == selected ? Color.cyan : Color.white;
                }
                else
                {
                    buildingButtons[i].GetComponent<Image>().color = Color.grey;
                }
            }
            selectedStats = new HexBuildingStats(selectedType, true);

            #region left panel
            bool?[] affectionsList;
            selectedStats.ApplyNeighboursAffection(hexBuilder.GetNeighboursHexTypes(selectedPosition), out affectionsList);
            FillStatsPanel(affectionsList);
            //-conditions
            if (buildingAvailable)
            {
                conditionLine0.gameObject.SetActive(false);
                conditionLine1.gameObject.SetActive(false);
            }
            else
            {
                bool? a; bool? b;
                Text t;
                CheckBuildingConditions(selectedType, out a, out b);
                if (a == null) { conditionLine0.gameObject.SetActive(false); }
                else
                {
                    if (a == true)
                    {
                        conditionLine0.GetChild(0).GetComponent<RawImage>().uvRect = taskCompletedRect;
                        t = conditionLine0.GetChild(1).GetComponent<Text>();
                        t.color = Color.grey;
                    }
                    else
                    {
                        conditionLine0.GetChild(0).GetComponent<RawImage>().uvRect = taskIncompletedRect;
                        t = conditionLine0.GetChild(1).GetComponent<Text>();
                        t.color = Color.white;
                    }
                    WriteConditionsText(t, selectedType, 0);
                    conditionLine0.gameObject.SetActive(true);
                }
                
                if (b == null)
                {
                    conditionLine1.gameObject.SetActive(false);
                }
                else
                {
                    if (b == true)
                    {
                        conditionLine1.GetChild(0).GetComponent<RawImage>().uvRect = taskCompletedRect;
                        t = conditionLine1.GetChild(1).GetComponent<Text>();
                        t.color = Color.grey;
                    }
                    else
                    {
                        conditionLine1.GetChild(0).GetComponent<RawImage>().uvRect = taskIncompletedRect;
                        t = conditionLine1.GetChild(1).GetComponent<Text>();
                        t.color = Color.white;
                    }
                    WriteConditionsText(t, selectedType, 1);
                    conditionLine1.gameObject.SetActive(true);
                }

                buildConditionsMet = (a == null) || ((a == true) & ((b == null) | (b == true)));
                #endregion
            
            }
            //right panel:
            {
                WriteBuildingInfo(selectedType, nameField, descriptionField);
                var cost = selectedStats.GetCost();
                int rcount = cost.GetLength(0), rid, costConditionsMet = 0;
                GameObject g;
                Transform t;
                Text label;
                Storage storage = colony.storage;
                ResourceContainer rc;
                void FillCostString(int i)
                {
                    g = costLines[i];
                    rc = cost[i];
                    rid = rc.resourceID;
                    t = g.transform;
                    t.GetChild(0).GetComponent<RawImage>().uvRect = ResourceType.GetResourceIconRect(rid);
                    label = t.GetChild(1).GetComponent<Text>();
                    label.text =((int)rc.volume).ToString() + ' '+  Localization.GetResourceName(rid);
                    if (storage.GetResourceCount(rid) >= rc.volume)
                    {
                        label.color = Color.white;
                        costConditionsMet++;
                    }
                    else label.color = Color.red;
                    g.SetActive(true);
                }
                FillCostString(0);
                FillCostString(1);
                FillCostString(2);
                if (rcount > 3)
                {
                    FillCostString(3);
                    if (rcount > 4) FillCostString(4);
                    else
                    {
                        costLines[4].gameObject.SetActive(false);
                    }
                }
                else
                {
                    costLines[3].gameObject.SetActive(false);
                    costLines[4].gameObject.SetActive(false);
                }

                if (buildConditionsMet & (costConditionsMet == rcount) & buildingAvailable == true)
                {
                    buildButton.interactable = true;
                    buildButton.GetComponentInChildren<Text>().color = Color.white;
                }
                else
                {
                    buildButton.interactable = false;
                    buildButton.GetComponentInChildren<Text>().color = Color.grey;
                }
            }
        }
        public void RedrawStatsPanel()
        {
            Text GetText(int i)
            {
                return statsPanel.transform.GetChild(i).GetChild(1).GetComponent<Text>();
            }
            Text t;
            float x = hexBuilder.totalPowerConsumption;
            t = GetText(0);
            if (x > 0) t.text = '-' + string.Format("{0:0.##}", x);
            else t.text = '+' + string.Format("{0:0.##}", -x);
            t.color = x > 0 ? Color.red : Color.white;
            //
            void FillFloatVal(int i)
            {
                t = GetText(i);
                if (x > 0) t.text = '+' + string.Format("{0:0.##}", x);
                else t.text = string.Format("{0:0.##}", x);
                t.color = x < 0 ? Color.red : Color.white;
            }
            
            //
            x = hexBuilder.totalIncome;
            FillFloatVal(1);
            // 2 - food
            x = hexBuilder.totalFoodProduction;
            FillFloatVal(2);
            // 3 - lifepower
            x = hexBuilder.totalLifepower;
            FillFloatVal(3);
            // 4 - personnel & maxpersonnel
            t = GetText(4);
            int a = hexBuilder.freeColonists,b = hexBuilder.colonistsCount,  c = hexBuilder.totalHousing;
            string sh, sa;
            if (a == 0) sa = "<color=yellow>" + a.ToString() + " / " + b.ToString() + "</color>";
            else sa = a.ToString() + " / " + b.ToString();

            if (c == b) sh = "<color=yellow>" + c.ToString() + "</color>";
            else
            {
                if (c < b) sh = "<color=red>" + c.ToString() + "</color>";
                else sh = c.ToString();
            }
            t.text = sa  + " / " + sh;
            // 5 - hexes
            t = GetText(5);
            a = hexBuilder.GetBuildingsCount();
            b = hexBuilder.hexLimit;
            t.text = a.ToString() + '/' + b.ToString();
            if (a == b) t.color = Color.yellow;
            else t.color = a > b ? Color.red : Color.white;
        }

        public void BuildButton(int i)
        {
            selectedType = (HexType)i;
            RedrawConstructionWindow();
        }
        public void BuildButton()
        {
            if (colony.storage.TryGetResources(selectedStats.GetCost()))
            {
                var ns = selectedStats.GetNoPersonnelCopy();
                hexBuilder.CreateHex(selectedPosition, ns);
                constructionWindow.SetActive(false);
            }
            else RedrawConstructionWindow();
        }
        public void CloseButton() {
            constructionWindow.SetActive(false);
        }

        public void AddWorker()
        {
            if (selectedHex != null && hexBuilder.SendColonistsForWork(selectedHex))
            {
                hexBuilder.RecalculateTotalParameters();
                OpenHexWindow(selectedHex);               
            }
        }
        public void RemoveWorker()
        {
            if (selectedHex != null && selectedHex.RemoveWorker())
            {
                hexBuilder.FreeColonistFromWork();
                OpenHexWindow(selectedHex);
            }
        }
    }
}

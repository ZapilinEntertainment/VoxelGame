using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FoundationRoute
{
    public sealed class HexCanvasUIC : MonoBehaviour
    {
        [SerializeField] private GameObject constructionWindow, statsPanel;
        [SerializeField] private GameObject[] costLines;
        [SerializeField] private Button buildButton;
        [SerializeField] private Button[] buildingButtons;
        [SerializeField] private Text[] stats;
        [SerializeField] private Text nameField, descriptionField;
        [SerializeField] private Transform conditionLine0, conditionLine1;
        private HexBuilder hexBuilder;
        private HexPosition selectedPosition;
        private HexType selectedType = HexType.Residential;
        private ColonyController colony;
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
        private void RecalculateAvailabilityMask()
        {
            hexBuilder.CountBuildings(ref buildingsCount);
            livingQuartersCount = buildingsCount[(int)HexType.Residential] + buildingsCount[(int)HexType.ResidentialDense] + buildingsCount[(int)HexType.ResidentialEco];
            commCount = buildingsCount[(int)HexType.Commercial] + buildingsCount[(int)HexType.CommercialDense];
            natureCount = buildingsCount[(int)HexType.Forest] + buildingsCount[(int)HexType.Mountain] + buildingsCount[(int)HexType.Lake];
            indCount = buildingsCount[(int)HexType.Industrial] + buildingsCount[(int)HexType.IndustrialExperimental];
            bool? a, b;
            void CheckConditions(HexType htype)
            {
                CheckBuildingConditions(htype, out a, out b);
                availabilityMask[(int)htype] = (a == null) || (( a == true) & (b == true | b == null));
            }
            CheckConditions(HexType.ResidentialDense);
            CheckConditions(HexType.ResidentialEco);
            CheckConditions(HexType.CommercialDense);
            CheckConditions(HexType.AdvancedFields);
            CheckConditions(HexType.Mountain);
            CheckConditions(HexType.IndustrialExperimental);
            CheckConditions(HexType.Powerplant);
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

        public void OpenConstructionWindow(HexPosition hpos)
        {
            if (!constructionWindow.activeSelf) constructionWindow.SetActive(true);
            selectedPosition = hpos;
            RedrawConstructionWindow();
        }

        private void RedrawConstructionWindow()
        {
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
            HexBuildingStats hbs = new HexBuildingStats(selectedType);
            bool?[] affectionsList;
            hbs.ApplyNeighboursAffection(hexBuilder.GetNeighboursHexTypes(selectedPosition), out affectionsList);
            #region left panel
            float pc = hbs.powerConsumption;
            Text s;
            if (pc == 0f) stats[0].enabled = false;
            else {
                s = stats[0];
                s.text = (pc > 0f ? '-' : '+') + string.Format("{0:0.##}", pc);
                if (affectionsList[0] == null) s.color = Color.white;
                else
                {
                    if (affectionsList[0] == true) s.color = Color.green;
                    else s.color = Color.red;
                }
                if (!s.enabled) s.enabled = true;
            }
            //
            s = stats[1];
            s.text = hbs.personnel.ToString();
            if (affectionsList[1] == null) s.color = Color.white;
            else
            {
                if (affectionsList[1] == true) s.color = Color.green;
                else s.color = Color.red;
            }
            //
            pc = hbs.income;
            s = stats[2];
            if (pc > 0f) s.text = '+' + string.Format("{0:0.##}", pc);
            else s.text = string.Format("{0:0.##}", pc);
            if (affectionsList[2] == null) s.color = Color.white;
            else
            {
                if (affectionsList[2] == true) s.color = Color.green;
                else s.color = Color.red;
            }
            //
            pc = hbs.lifepower;
            s = stats[3];
            if (pc > 0f) s.text = '+' + string.Format("{0:0.##}", pc);
            else s.text = string.Format("{0:0.##}", pc);
            if (affectionsList[3] == null) s.color = Color.white;
            else
            {
                if (affectionsList[3] == true) s.color = Color.green;
                else s.color = Color.red;
            }
            //
            pc = hbs.foodProduction;
            s = stats[4];
            if (pc > 0f) s.text = '+' + string.Format("{0:0.##}", pc);
            else s.text = string.Format("{0:0.##}", pc);
            if (affectionsList[4] == null) s.color = Color.white;
            else
            {
                if (affectionsList[4] == true) s.color = Color.green;
                else s.color = Color.red;
            }
            //
            s = stats[5];
            if (hbs.housing == 0) s.enabled = false;
            else
            {
                s.text = hbs.housing.ToString();
                if (affectionsList[5] == null) s.color = Color.white;
                else
                {
                    if (affectionsList[5] == true) s.color = Color.green;
                    else s.color = Color.red;
                }
                if (!s.enabled) s.enabled = true;
            }
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
                var cost = hbs.GetCost();
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
                    label.text = Localization.GetResourceName(rid);
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

                if (buildConditionsMet & (costConditionsMet == rcount))
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

        public void BuildButton(int i)
        {
            selectedType = (HexType)i;
            RedrawConstructionWindow();
        }
        public void BuildButton()
        {

        }
        public void CloseButton() {
            constructionWindow.SetActive(false);
        }
    }
}

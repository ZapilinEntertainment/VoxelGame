using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FoundationRoute
{
    public sealed class HexCanvasUIC : MonoBehaviour
    {
        [SerializeField] private GameObject constructionWindow, statsPanel;
        [SerializeField] private Button[] buildingButtons;
        [SerializeField] private Text[] stats;
        [SerializeField] private Transform conditionLine0, conditionLine1;
        private HexBuilder hexBuilder;
        private HexPosition selectedPosition;
        private HexType selectedType = HexType.Residential;
        private int[] buildingsCount = new int[(int)HexType.TotalCount];
        private int livingQuartersCount, commCount, natureCount, indCount;
        private BitArray availabilityMask = new BitArray((int)HexType.TotalCount, true);
        private readonly Rect taskIncompletedRect = UIController.GetIconUVRect(Icons.TaskFrame),
            taskCompletedRect = UIController.GetIconUVRect(Icons.TaskCompleted);
        private string[] conditionStrings;
        private const byte RES2_LQ_COUNT = 2, RES3_NAT_COUNT = 2, COM2_COM_COUNT = 2, PP_LQ_COUNT = 3;


        public void Prepare(HexBuilder hb)
        {
            hexBuilder = hb;
            constructionWindow.SetActive(false);
            statsPanel.SetActive(false);
            buildingsCount = new int[(int)HexType.TotalCount];
            RecalculateAvailabilityMask();
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
                    if (stage == 0) t.text = conditionStrings[0] + ' ' + livingQuartersCount.ToString() + '/' + RES2_LQ_COUNT.ToString();
                    else t.text = conditionStrings[1];
                    break;
                case HexType.ResidentialEco:
                    t.text = conditionStrings[2] + ' ' + natureCount.ToString() + '/' + RES3_NAT_COUNT.ToString();
                    break;
                case HexType.CommercialDense:
                    if (stage == 0) t.text = conditionStrings[3] + ' ' + commCount.ToString() + '/' + COM2_COM_COUNT.ToString();
                    else t.text = conditionStrings[4];
                    break;
                case HexType.AdvancedFields:
                    if (stage == 0) t.text = conditionStrings[5];
                    else t.text = conditionStrings[6];
                    break;
                case HexType.Mountain:
                    t.text = conditionStrings[7];
                    break;
                case HexType.IndustrialExperimental:
                    if (stage == 0) t.text = conditionStrings[8];
                    else t.text = conditionStrings[9];
                    break;
                case HexType.Powerplant:
                    t.text = conditionStrings[10] + ' ' + livingQuartersCount.ToString() + '/' + PP_LQ_COUNT.ToString();
                    break;
            }
        }

        public void OpenConstructionWindow(HexPosition hpos)
        {
            if (!constructionWindow.activeSelf) constructionWindow.SetActive(true);
            selectedPosition = hpos;
        }

        private void RedrawConstructionWindow()
        {
            int selected = (int)selectedType;
            bool buildingAvailable = availabilityMask[selected];
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
            HexBuildingStats hbs = HexBuildingStats.GetStats(selectedType);
            #region left panel
            float pc = hbs.powerConsumption;
            Text s;
            if (pc == 0f) stats[0].enabled = false;
            else {
                s = stats[0];
                s.text = (pc > 0f ? '-' : '+') + string.Format("{0:0.##}", pc);
                if (!s.enabled) s.enabled = true;
            }
            stats[1].text = hbs.personnel.ToString();
            pc = hbs.income;
            if (pc > 0f) stats[2].text = '+' + string.Format("{0:0.##}", pc);
            else stats[2].text = string.Format("{0:0.##}", pc);
            pc = hbs.lifepower;
            if (pc > 0f) stats[3].text = '+' + string.Format("{0:0.##}", pc);
            else stats[3].text = string.Format("{0:0.##}", pc);
            pc = hbs.foodProduction;
            if (pc > 0f) stats[4].text = '+' + string.Format("{0:0.##}", pc);
            else stats[4].text = string.Format("{0:0.##}", pc);
            s = stats[5];
            if (hbs.housing == 0) s.enabled = false;
            else
            {
                s.text = hbs.housing.ToString();
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
                #endregion
            
            }
            //right panel:


        }
    }
}

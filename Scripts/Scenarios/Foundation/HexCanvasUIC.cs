using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FoundationRoute
{
    public sealed class HexCanvasUIC : MonoBehaviour
    {
        [SerializeField] private GameObject constructionWindow, statsPanel;
        private HexPosition selectedPosition;
        private HexType selectedType = HexType.Residential;

        public void Prepare()
        {
            constructionWindow.SetActive(false);
            statsPanel.SetActive(false);
        }

        public void OpenConstructionWindow(HexPosition hpos)
        {
            if (!constructionWindow.activeSelf) constructionWindow.SetActive(true);
            selectedPosition = hpos;
        }

        private void RedrawConstructionWindow()
        {

        }
    }
}

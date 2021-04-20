using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FoundationRoute
{
    public sealed class HexBuilder
    {        
        private GameObject hexMaquetteExample;
        private HexCanvasUIC uic;
        private Dictionary<(byte, byte), Hex> hexList;        
        private Dictionary<HexPosition, GameObject> maquettesList;
        private List<GameObject> maquettesPool;
        private int poolLength = 0;


        public void Prepare()
        {
            hexList = new Dictionary<(byte, byte), Hex>();
            hexMaquetteExample = Resources.Load<GameObject>("Prefs/Special/hexMaquette");
            maquettesPool = new List<GameObject>();
            poolLength = 0;
            maquettesList = new Dictionary<HexPosition, GameObject>();
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
            var cl = g.AddComponent<ClickableObject>();

            var npos = hpos.Copy;
            cl.AssignFunction(() => this.MaquetteClicked(npos));
            maquettesList.Add(npos, g);
        }
        public void MaquetteClicked(HexPosition hpos)
        {
            if (uic == null)
            {
                uic = Object.Instantiate(Resources.Load<GameObject>("UIPrefs/hexCanvas")).transform.GetChild(0).GetChild(0).GetComponent<HexCanvasUIC>();
            }
            uic.OpenConstructionWindow(hpos);
        }

    }
}

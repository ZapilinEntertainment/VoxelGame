using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FoundationRoute
{
    public sealed class HexBuilder
    {
        public readonly FoundationRouteScenario scenario;
        private readonly GameObject hexMaquetteExample;
        private HexCanvasUIC uic;
        private Dictionary<(byte, byte), Hex> hexList;        
        private Dictionary<HexPosition, GameObject> maquettesList;
        private List<GameObject> maquettesPool;
        private int poolLength = 0;
        private readonly float innerRadius, outerRadius;
        private const float CONST_0 = 1.73205f;


        private HexBuilder() { }// reserved
        public HexBuilder(FoundationRouteScenario frs)
        {
            scenario = frs;
            hexList = new Dictionary<(byte, byte), Hex>();
            hexMaquetteExample = Resources.Load<GameObject>("Prefs/Special/hexMaquette");
            maquettesPool = new List<GameObject>();
            poolLength = 0;
            maquettesList = new Dictionary<HexPosition, GameObject>();
            innerRadius = 8f;
            outerRadius = innerRadius * 2f / CONST_0;
            if (uic == null)
            {
                var t = Object.Instantiate(Resources.Load<GameObject>("UIPrefs/hexCanvas")).transform;
                UIController.GetCurrent().AddSpecialCanvasToHolder(t);
                uic = t.GetComponentInChildren<HexCanvasUIC>();
                uic.Prepare(this);
            }
        }

        public void CountBuildings(ref int[] buildingsCount)
        {
            for (int i = 0; i < (int)HexType.TotalCount; i++)
            {
                buildingsCount[i] = 0;
            }
            if (hexList.Count > 0)
            {
                foreach (var h in hexList.Values)
                {
                    buildingsCount[(int)h.type]++;
                }
            }
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
            g.transform.position = GetHexLocalPosition(hpos) + new Vector3(8f, -21f, 8f);
            var cl = g.GetComponentInChildren<ClickableObject>();

            var npos = hpos.Copy;
            cl.AssignFunction(() => uic.OpenConstructionWindow(npos), uic.CloseButton);
            maquettesList.Add(npos, g);
        }

        public Hex GetHex(HexPosition hpos)
        {
            var b = hpos.ToBytes();
            if (hexList.ContainsKey(b)) return hexList[b];
            else return null;
        }
        public List<HexType> GetNeighboursHexTypes(HexPosition hpos)
        {
            var list = new List<HexType>();
            if (hexList.Count != 0) 
            {
                var np = hpos.GetNeighbour(0).ToBytes();
                if (hexList.ContainsKey(np)) list.Add(hexList[np].type);
                np = hpos.GetNeighbour(1).ToBytes();
                if (hexList.ContainsKey(np)) list.Add(hexList[np].type);
                np = hpos.GetNeighbour(2).ToBytes();
                if (hexList.ContainsKey(np)) list.Add(hexList[np].type);
                np = hpos.GetNeighbour(3).ToBytes();
                if (hexList.ContainsKey(np)) list.Add(hexList[np].type);
                np = hpos.GetNeighbour(4).ToBytes();
                if (hexList.ContainsKey(np)) list.Add(hexList[np].type);
                np = hpos.GetNeighbour(5).ToBytes();
                if (hexList.ContainsKey(np)) list.Add(hexList[np].type);                
            }
            return list;
        }

        #region positioning
        private Vector3 GetHexLocalPosition(HexPosition hpos)
        {
            switch (hpos.ringIndex)
            {
                case 0: return GetMainDirectionVector(hpos.ringPosition) * 2f * innerRadius;

                default:
                    {
                        byte sector = hpos.DefineSector();
                        var dir = GetMainDirectionVector(sector) * (innerRadius * 2f * (hpos.ringIndex + 1));
                        dir += GetInsectorDirection(sector) * hpos.DefineInsectorPosition() * 2f * innerRadius;
                        return dir;
                    }
            }
        }
        private Vector3 GetMainDirectionVector(byte x)
        {
            return (Quaternion.AngleAxis(60f * x, Vector3.up) * Vector3.forward);
        }
        private Vector3 GetInsectorDirection(byte x)
        {
            // normalized
            // на больших расстояниях будет сказываться погрешность!
            switch (x)
            {
                case 0: return new Vector3(0.866025f, 0f, -0.5f).normalized;
                case 1: return Vector3.back;
                case 2: return new Vector3(-0.866025f, 0f, -0.5f).normalized;
                case 3: return new Vector3(-0.866025f, 0f, 0.5f).normalized;
                case 4: return Vector3.forward;
                case 5: return new Vector3(0.866025f, 0f, 0.5f).normalized;
                default: return Vector3.zero;
            }
        }
        #endregion
    }
}

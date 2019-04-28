using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UIMonumentObserver : UIObserver
{
#pragma warning disable 0649
    [SerializeField] private RawImage affectionIcon;
    [SerializeField] private Text affectionText;
    [SerializeField] private Transform[] slots;
#pragma warning restore 0649
    private Monument observingMonument;
    private const int IMAGE_CHILD_INDEX = 0, TEXT_CHILD_INDEX = 1, CLOSEBUTTON_CHILD_INDEX = 2;

    //отключать слоты артефактов когда нет питания

    public void SetObservingMonument(Monument m)
    {
        if (m == null)
        {
            SelfShutOff();
            return;
        }
        else
        {
            var at = observingMonument.affectionType;
            affectionIcon.uvRect = Artifact.GetAffectionIconRect(at);
            affectionText.text = Localization.GetAffectionTitle(at);
            if (observingMonument.artifacts != null)
            {
                //0
                Transform t = slots[0];
                Artifact a = observingMonument.artifacts[0];
                if (a == null)
                {
                    t.GetChild(IMAGE_CHILD_INDEX).gameObject.SetActive(false);
                    t.GetChild(TEXT_CHILD_INDEX).gameObject.SetActive(false);
                    t.GetChild(CLOSEBUTTON_CHILD_INDEX).gameObject.SetActive(false);
                    t.GetComponent<Button>().interactable = true;
                }
                else
                {
                    var t2 = t.GetChild(IMAGE_CHILD_INDEX);
                    t2.GetComponent<RawImage>().texture = a.GetTexture();
                    t2.gameObject.SetActive(true);
                    t2 = t.GetChild(TEXT_CHILD_INDEX);
                    t2.GetComponent<Text>().text = '"' + a.name + '"';
                    t2.gameObject.SetActive(true);
                    t.GetChild(CLOSEBUTTON_CHILD_INDEX).gameObject.SetActive(true);
                    t.GetComponent<Button>().interactable = false;
                }
                // 1
                t = slots[1]; a = observingMonument.artifacts[1];
                if (a == null)
                {
                    t.GetChild(IMAGE_CHILD_INDEX).gameObject.SetActive(false);
                    t.GetChild(TEXT_CHILD_INDEX).gameObject.SetActive(false);
                    t.GetChild(CLOSEBUTTON_CHILD_INDEX).gameObject.SetActive(false);
                    t.GetComponent<Button>().interactable = true;
                }
                else
                {
                    var t2 = t.GetChild(IMAGE_CHILD_INDEX);
                    t2.GetComponent<RawImage>().texture = a.GetTexture();
                    t2.gameObject.SetActive(true);
                    t2 = t.GetChild(TEXT_CHILD_INDEX);
                    t2.GetComponent<Text>().text = '"' + a.name + '"';
                    t2.gameObject.SetActive(true);
                    t.GetChild(CLOSEBUTTON_CHILD_INDEX).gameObject.SetActive(true);
                    t.GetComponent<Button>().interactable = false;
                }
                // 2
                t = slots[2]; a = observingMonument.artifacts[2];
                if (a == null)
                {
                    t.GetChild(IMAGE_CHILD_INDEX).gameObject.SetActive(false);
                    t.GetChild(TEXT_CHILD_INDEX).gameObject.SetActive(false);
                    t.GetChild(CLOSEBUTTON_CHILD_INDEX).gameObject.SetActive(false);
                    t.GetComponent<Button>().interactable = true;
                }
                else
                {
                    var t2 = t.GetChild(IMAGE_CHILD_INDEX);
                    t2.GetComponent<RawImage>().texture = a.GetTexture();
                    t2.gameObject.SetActive(true);
                    t2 = t.GetChild(TEXT_CHILD_INDEX);
                    t2.GetComponent<Text>().text = '"' + a.name + '"';
                    t2.gameObject.SetActive(true);
                    t.GetChild(CLOSEBUTTON_CHILD_INDEX).gameObject.SetActive(true);
                    t.GetComponent<Button>().interactable = false;
                }
                // 3
                t = slots[3]; a = observingMonument.artifacts[3];
                if (a == null)
                {
                    t.GetChild(IMAGE_CHILD_INDEX).gameObject.SetActive(false);
                    t.GetChild(TEXT_CHILD_INDEX).gameObject.SetActive(false);
                    t.GetChild(CLOSEBUTTON_CHILD_INDEX).gameObject.SetActive(false);
                    t.GetComponent<Button>().interactable = true;
                }
                else
                {
                    var t2 = t.GetChild(IMAGE_CHILD_INDEX);
                    t2.GetComponent<RawImage>().texture = a.GetTexture();
                    t2.gameObject.SetActive(true);
                    t2 = t.GetChild(TEXT_CHILD_INDEX);
                    t2.GetComponent<Text>().text = '"' + a.name + '"';
                    t2.gameObject.SetActive(true);
                    t.GetChild(CLOSEBUTTON_CHILD_INDEX).gameObject.SetActive(true);
                    t.GetComponent<Button>().interactable = false;
                }
            }
            else
            {
                //0
                Transform t = slots[0];
                Artifact a = observingMonument.artifacts[0];
                t.GetChild(IMAGE_CHILD_INDEX).gameObject.SetActive(false);
                t.GetChild(TEXT_CHILD_INDEX).gameObject.SetActive(false);
                t.GetChild(CLOSEBUTTON_CHILD_INDEX).gameObject.SetActive(false);
                t.GetComponent<Button>().interactable = true;
                //1
                t = slots[1];
                a = observingMonument.artifacts[1];
                t.GetChild(IMAGE_CHILD_INDEX).gameObject.SetActive(false);
                t.GetChild(TEXT_CHILD_INDEX).gameObject.SetActive(false);
                t.GetChild(CLOSEBUTTON_CHILD_INDEX).gameObject.SetActive(false);
                t.GetComponent<Button>().interactable = true;
                //2
                t = slots[2];
                a = observingMonument.artifacts[2];
                t.GetChild(IMAGE_CHILD_INDEX).gameObject.SetActive(false);
                t.GetChild(TEXT_CHILD_INDEX).gameObject.SetActive(false);
                t.GetChild(CLOSEBUTTON_CHILD_INDEX).gameObject.SetActive(false);
                t.GetComponent<Button>().interactable = true;
                //3
                t = slots[3];
                a = observingMonument.artifacts[3];
                t.GetChild(IMAGE_CHILD_INDEX).gameObject.SetActive(false);
                t.GetChild(TEXT_CHILD_INDEX).gameObject.SetActive(false);
                t.GetChild(CLOSEBUTTON_CHILD_INDEX).gameObject.SetActive(false);
                t.GetComponent<Button>().interactable = true;
            }
        }
    }

    override protected void StatusUpdate()
    {

    }

    public void ClearSlot(int x)
    {
        if (observingMonument == null)
        {
            SelfShutOff();
            return;
        }
        else
        {
            observingMonument.RemoveArtifact(x);
            StatusUpdate();
        }
    }
}

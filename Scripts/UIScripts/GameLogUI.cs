using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameLogUI : MonoBehaviour {
#pragma warning disable 0649
    [SerializeField] private GameObject logButton,logWindow, lastMessagePanel, blockingMask, decisionPanel, decisionLeftButton, 
        decisionRightButton,decisionMonoButton, importantAnnouncePanel;
    [SerializeField] private Text lastMessageText, decisionWindowText;
    [SerializeField] private Text[] messages;
#pragma warning restore 0649
    private static GameLogUI current;

    public delegate void DecisionAction();
    private bool activeAnnouncement = false, importantAnnouncementEnabled = false;
    private float lastMessageTimer = 0;
    private int lastMessageIndex = 0;
    private DecisionAction leftDecision, rightDecision, monoDecision;    
    private const byte MAX_MESSAGES = 10;
    private const float IMPORTANT_ANNOUNCE_DISSAPPEAR_SPEED = 1f, MESSAGE_DISSAPPEAR_TIME = 3f, INNER_LOG_CLEAR_TIME = 15;

    public static void MakeAnnouncement(string s)
    {
        MakeAnnouncement(s, Color.white);
    }
    public static void MakeAnnouncement(string s, Color col)
    {
        if (current == null) InitializeCurrent();
        current.AddAnnouncement(s, col);
    }
    public static void MakeImportantAnnounce(string s)
    {
        if (current == null) InitializeCurrent();
        current.importantAnnouncePanel.transform.GetChild(0).GetComponent<Text>().text = s;
        current.importantAnnouncePanel.GetComponent<Image>().color = PoolMaster.gameOrangeColor;
        current.importantAnnouncePanel.SetActive(true);
        current.importantAnnouncementEnabled = true;
    }

    public static void NotEnoughResourcesAnnounce()
    {
        MakeImportantAnnounce(Localization.GetAnnouncementString(GameAnnouncements.NotEnoughResources));
        if (GameMaster.soundEnabled) GameMaster.audiomaster.Notify(NotificationSound.NotEnoughResources);
    }
    public static void NotEnoughMoneyAnnounce()
    {
        MakeImportantAnnounce(Localization.GetAnnouncementString(GameAnnouncements.NotEnoughEnergyCrystals));
        if (GameMaster.soundEnabled) GameMaster.audiomaster.Notify(NotificationSound.NotEnoughMoney);
    }

    public static void EnableDecisionWindow(DecisionAction monoaction, string text)
    {
        if (current == null) InitializeCurrent();
        current.PrepareDecisionWindow(monoaction,text);
    }
    public static void EnableDecisionWindow(DecisionAction leftDecision, DecisionAction rightDecision, string text)
    {
        if (current == null) InitializeCurrent();
        current.PrepareDecisionWindow(leftDecision, rightDecision,text);
    }

    private static void InitializeCurrent()
    {
        current = Instantiate(Resources.Load<GameObject>("UIPrefs/logCanvas")).GetComponent<GameLogUI>();
    }
    public static void DeactivateLogWindow()
    {
        if (current == null) return;
        else
        {
            if (current.logWindow.activeSelf) current.LogButton();
        }
    }
    // =====================
    private void Update()
    {
        float t = Time.deltaTime * GameMaster.gameSpeed;
        if (importantAnnouncementEnabled )
        {
            Image i = importantAnnouncePanel.GetComponent<Image>();
            float x = i.color.a;
            x = Mathf.MoveTowards(x, 0, IMPORTANT_ANNOUNCE_DISSAPPEAR_SPEED * t);
            if (x > 0) i.color = new Color(i.color.r, i.color.g, i.color.b, x);
            else
            {
                importantAnnouncePanel.SetActive(false);
                importantAnnouncementEnabled = false;
            }
        }
        if (activeAnnouncement)
        {
            Image i = lastMessagePanel.GetComponent<Image>();
            float x = Mathf.MoveTowards(i.color.a, 0, 1/MESSAGE_DISSAPPEAR_TIME * t);
            if (x > 0) i.color = new Color(i.color.r, i.color.g, i.color.b, x);
            else
            {
                lastMessagePanel.SetActive(false);
                activeAnnouncement = false;
            }
        }
        if (lastMessageTimer > 0)
        {
            lastMessageTimer -= t;
            if (lastMessageTimer <= 0 & lastMessageIndex > 0)
            {
                lastMessageIndex--;
                for (int i = 0; i < lastMessageIndex; i++)
                {
                    messages[i].text = messages[i + 1].text;
                    messages[i].color = messages[i + 1].color;
                }
                messages[lastMessageIndex].gameObject.SetActive(false);
                
                lastMessageTimer = INNER_LOG_CLEAR_TIME;
            }
        }
    }

    private void AddAnnouncement(string s, Color c)
    {      
       if (lastMessageIndex == 0)
        {
            var t = messages[0];
            t.text = s;
            t.color = c;
            t.gameObject.SetActive(true);
            lastMessageIndex++;
        }
       else
        {
            int l = messages.Length;
            if (lastMessageIndex == l)
            {
                int i = 0;
                for (; i < l - 1; i++)
                {
                    messages[i].text = messages[i + 1].text;
                    messages[i].color = messages[i + 1].color;
                }
                messages[i].text = s;
                messages[i].color = c;
            }
            else
            {
                var t = messages[lastMessageIndex];
                t.text = s;
                t.color = c;
                t.gameObject.SetActive(true);
                lastMessageIndex++;
            }
        }
        lastMessageText.text = s;
        lastMessageText.color = c;        
        lastMessagePanel.GetComponent<Image>().color = Color.white;
        lastMessagePanel.gameObject.SetActive(true);
        activeAnnouncement = true;
        lastMessageTimer = INNER_LOG_CLEAR_TIME;
    }
    private void PrepareDecisionWindow(DecisionAction monoDecision, string text)
    {
        blockingMask.SetActive(true);
        decisionWindowText.text = text;
        decisionRightButton.SetActive(false);
        decisionLeftButton.SetActive(false);
        decisionMonoButton.SetActive(true);
        decisionPanel.SetActive(true);
    }
    private void PrepareDecisionWindow(DecisionAction leftDecision, DecisionAction rightDecision, string text)
    {
        blockingMask.SetActive(true);
        decisionWindowText.text = text;
        decisionRightButton.SetActive(true);
        decisionLeftButton.SetActive(true);
        decisionMonoButton.SetActive(false);
        decisionPanel.SetActive(true);
    }
    //====
    public void LogButton()
    {
        if (logWindow.activeSelf)
        {
            current.logWindow.SetActive(false);
            if (current.activeAnnouncement) current.lastMessagePanel.SetActive(true);
            if (UIController.current.currentActiveWindowMode == ActiveWindowMode.LogWindow) UIController.current.DropActiveWindow(ActiveWindowMode.LogWindow);
        }
        else
        {
            if (current.activeAnnouncement) current.lastMessagePanel.SetActive(false);
            current.logWindow.SetActive(true);
            UIController.current.ChangeActiveWindow(ActiveWindowMode.LogWindow);
        }
    } 

    //==== DECISION  PANEL
    public void DecisionLeft()
    {
        leftDecision();
        CloseDecisionPanel();
    }
    public void DecisionRight()
    {
        rightDecision();
        CloseDecisionPanel();
    }
    public void MonoDecision()
    {
        monoDecision();
        CloseDecisionPanel();
    }
    private void CloseDecisionPanel()
    {
        decisionPanel.SetActive(false);
        blockingMask.SetActive(false);        
    }
}

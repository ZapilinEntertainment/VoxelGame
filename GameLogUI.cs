using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameLogUI : MonoBehaviour {
#pragma warning disable 0649
    [SerializeField] private GameObject logButton,logWindow, lastMessagePanel, blockingMask, decisionPanel, decisionLeftButton, 
        decisionRightButton,decisionMonoButton, importantAnnouncePanel;
    [SerializeField] private Text lastMessageText, decisionWindowText;
    [SerializeField] private RectTransform contentHolder, exampleText;
#pragma warning restore 0649
    private static GameLogUI current;

    public delegate void DecisionAction();
    private bool logPrepared = false, activeAnnouncement = false, importantAnnouncementEnabled = false;
    private int lastMessageIndex = 0;
    private DecisionAction leftDecision, rightDecision, monoDecision;
    private List<Text> messages;
    private const byte MAX_MESSAGES = 30;
    private const float IMPORTANT_ANNOUNCE_DISSAPPEAR_SPEED = 0.3f, LAST_MESSAGE_DISSAPPEAR_TIME = 0.3f;


    public static void MakeAnnouncement(string s)
    {
        MakeAnnouncement(s, Color.white);
    }
    public static void MakeAnnouncement(string s, Color col)
    {
        if (current == null) InitializeCurrent();
        if (!current.logPrepared)
        {
            current.logButton.SetActive(true);
            current.messages = new List<Text>();
            current.contentHolder.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, current.exampleText.rect.height * MAX_MESSAGES);
            current.logPrepared = true;
        }
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
        if (current == null || !current.logPrepared) return;
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
            float x = Mathf.MoveTowards(i.color.a, 0, LAST_MESSAGE_DISSAPPEAR_TIME * t);
            if (x > 0) i.color = new Color(i.color.r, i.color.g, i.color.b, x);
            else
            {
                lastMessagePanel.SetActive(false);
                activeAnnouncement = false;
            }
        }
    }

    private void AddAnnouncement(string s, Color c)
    {
        bool activated = contentHolder.gameObject.activeSelf;
        if (activated) contentHolder.gameObject.SetActive(false);        
        if (lastMessageIndex >= MAX_MESSAGES)
        {
            Vector3 upv = Vector3.up * exampleText.rect.height;
            for (int i = 1; i < MAX_MESSAGES; i++)
            {
                messages[i].rectTransform.transform.position += upv;
            }
            Text m = messages[MAX_MESSAGES - 1];
            m.text = s;
            m.color = c;
            m.rectTransform.position += Vector3.down * m.rectTransform.rect.height * (MAX_MESSAGES - 1);
        }
        else
        {
            if (messages.Count < lastMessageIndex + 1)
            {
                Text m = Instantiate(exampleText).GetComponent<Text>();
                m.gameObject.SetActive(true);
                lastMessageIndex++;
                m.transform.parent = contentHolder;
                m.transform.position = Vector3.up * lastMessageIndex * m.rectTransform.rect.height;
                m.text = s;
                m.color = c;
            }
        }
        if (activated)
        {
            contentHolder.gameObject.SetActive(true);
            if (activeAnnouncement)
            {
                activeAnnouncement = false;
                lastMessagePanel.SetActive(false);
            }
        }
        else
        {
            Text t = lastMessagePanel.transform.GetChild(0).GetComponent<Text>();
            t.text = s;
            t.color = c;
            lastMessagePanel.GetComponent<Image>().color = Color.black;
            if (!activeAnnouncement)
            {
                activeAnnouncement = true;
                lastMessagePanel.SetActive(true);
            }
        }
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
        if (logPrepared)
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

enum UIMode{ViewMode, QuestsWindow}

public class UIController : MonoBehaviour {
	public RectTransform questPanel; // fill in Inspector
	public RectTransform[] questButtons; // fill in Inspector
	public GameObject returnToQuestList_button; // fill in Inspector
	UIMode mode;
	byte submode = 0;
	bool transformingRectInProgress = false;
	float rectTransformingSpeed = 0.8f, transformingProgress;
	RectTransform transformingRect; Vector2 resultingAnchorMin, resultingAnchorMax;
	int openedQuest = -1;

	void Update() {
		if (transformingRectInProgress) {
			transformingProgress = Mathf.MoveTowards(transformingProgress, 1, rectTransformingSpeed * Time.deltaTime);
			transformingRect.anchorMin = Vector2.Lerp (transformingRect.anchorMin, resultingAnchorMin, transformingProgress);
			transformingRect.anchorMax = Vector2.Lerp (transformingRect.anchorMax, resultingAnchorMax, transformingProgress);
			if (transformingProgress == 1) {
				transformingProgress = 0;
				transformingRectInProgress = false;
			}
		}			
	}

	public void OpenQuestWindow() {
		mode = UIMode.QuestsWindow;
		questPanel.gameObject.SetActive(true);
	}

	#region quest window
	public void QuestButton_OpenQuest(int index) {
		transformingRect = questButtons[index];
		transformingRectInProgress = true;
		transformingProgress = 0;
		resultingAnchorMin = Vector2.zero;
		resultingAnchorMax= Vector2.one;
		for (int i = 0; i < questButtons.Length; i++) {
			if (i == index) continue;
			else {
				questButtons[i].gameObject.SetActive(false);
			}
		}
		openedQuest = index;
		returnToQuestList_button.SetActive(true);
	}

	public void QuestButton_ReturnToQuestList() {
		if (openedQuest == - 1) return;
		QuestButton_RestoreButtonRect(openedQuest);
		transformingRectInProgress = true;
		transformingProgress = 0;
		for (int i = 0; i < questButtons.Length; i++) {
			if (i == openedQuest) continue;
			else {
				questButtons[i].gameObject.SetActive(true);
			}
		}
	}

	void QuestButton_RestoreButtonRect(int i) {
		switch (i) {
		case 0:
			resultingAnchorMin = new Vector2(0, 0.66f);
			resultingAnchorMax = new Vector2(0.33f, 1);
			break;
		case 1:
			resultingAnchorMin = new Vector2(0.33f, 0.66f);
			resultingAnchorMax = new Vector2(0.66f, 1);
			break;
		case 2:
			resultingAnchorMin = new Vector2(0.66f, 0.66f);
			resultingAnchorMax = new Vector2(1, 1);
			break;
		case 3:
			resultingAnchorMin = new Vector2(0, 0.33f);
			resultingAnchorMax = new Vector2(0.33f, 0.66f);
			break;
		case 4:
			resultingAnchorMin = new Vector2(0.33f, 0.33f);
			resultingAnchorMax= new Vector2(0.66f, 0.66f);
			break;
		case 5:
			resultingAnchorMin = new Vector2(0.66f, 0.33f);
			resultingAnchorMax = new Vector2(1, 0.66f);
			break;
		case 6:
			resultingAnchorMin = new Vector2(0, 0);
			resultingAnchorMax = new Vector2(0.33f, 0.33f);
			break;
		case 7:
			resultingAnchorMin = new Vector2(0.33f, 0);
			resultingAnchorMax = new Vector2(0.66f, 0.33f);
			break;
		case 8:
			resultingAnchorMin = new Vector2(0.66f, 0);
			resultingAnchorMax = new Vector2(1, 0.33f);
			break;
		}
	}
	#endregion
}

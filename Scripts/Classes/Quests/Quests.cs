using Bayat.Json;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Quest;

/// <summary>
/// TODO Summary of Quest-Component
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class Quests : MonoBehaviour {

    #region Vars

    /// <summary>
    /// The Index of the QuestList where the User is
    /// </summary>
    [JsonProperty(PropertyName = "currentQuestIndex")]
    private int currentQuestIndex = 0;

    /// <summary>
    /// The current State of the currentQuest
    /// </summary>
    [JsonProperty(PropertyName = "currentQuestState")]
    private QuestStates currentQuestState = QuestStates.NotStarted;

    /// <summary>
    /// Quest Infos about the current Quest
    /// </summary>
    public Quest currentQuest = new Quest();

    /// <summary>
    /// the List of Quests of the World
    /// </summary>
    //[HideInInspector] 
    public List<Quest> questList;

    public UnityEvent allCurrentQuestsAccepted;
    public UnityEvent rewardedWithItems;



    // Performance Optimization
    string rewardString, questDescription;
    Transform PopUp, PopUpContent, PopUpHeading;
    TMPro.TextMeshProUGUI InProgressInfo;
    GameObject clonedQuest, buttonInfoOk, buttonInfoOkText, QuestListContent, QuestBlueprint, QuestBlueprintInactive;
    Button buttonStartQuest;
    int iterator, questListCount;
    List<Quest> questListTurned;


    #endregion

    /// <summary>
    /// loads the currentQuest information and prepares it for opening the PopUp
    /// </summary>
    /// <param name="loadOutOfSaveGame">if true, the information for the currentQuest comes out of the PlayerPrefs</param>
    public void loadCurrentQuest(bool loadOutOfSaveGame) {

        if (loadOutOfSaveGame) {
            setStatusQuestInfoCircle(false);
        }

        if (currentQuestIndex < questList.Count && questList.Count > 0) {

            if (currentQuestIndexExits() && currentQuestIndex >= 0) {

                currentQuest = questList[currentQuestIndex];

                if (loadOutOfSaveGame) {
                    currentQuest.questState = currentQuestState;
                }

                loadQuest(currentQuest);

            } else {
                Debug.Log("All Quests fulfilled in this World");
            }

        } else {
            setCurrentQuestState(QuestStates.Finished);
            Debug.LogError("ERROR: PlayerPrefs currentQuestIndex out of Range!");
        }  
    }

    public void loadQuest(Quest aQuest) {
        // Load QuestInfos Into PopUp
        switch (aQuest.questType) {
            case Quest.QuestTypes.Quest:
            default:
                manageQuest(aQuest);
                break;
            case Quest.QuestTypes.InfoPopUp:
                fillQuestPopUp(aQuest, aQuest.rewardList.Count > 0, true);
                break;
            case Quest.QuestTypes.InfoPopUpWithReward:
                fillQuestPopUp(aQuest, true, true);
                break;
        }
    }

    public void loadQuest(int questIndex) {
        loadQuest(questList[questIndex]);
    }


    /// <summary>
    /// Managing a Quest with its States (an InfoPopUp don't have to be managed)
    /// </summary>
    private void manageQuest(Quest aQuest) {

        switch (aQuest.questState) {
            case QuestStates.NotStarted:
            default:
                setStatusQuestInfoCircle(true);
                fillQuestPopUp(aQuest, aQuest.rewardList.Count > 0, false);
                break;
            case QuestStates.InProgress:
                fillQuestPopUp(aQuest, aQuest.rewardList.Count > 0, false);
                break;
            case QuestStates.Finished:
                setStatusQuestInfoCircle(true);
                fillQuestPopUp(aQuest, aQuest.rewardList.Count > 0, true);
                break;
        }

    }

    /// <summary>
    /// Checks if the Questcondition
    /// </summary>
    /// <returns></returns>
    public bool checkCurrentQuestAlreadyFulfilled() {

        bool questFullfilled = false;

        switch (currentQuest.finishCondition.conditionType) {
            case QuestCondition.ConditionTypes.Building:
            default:
                questFullfilled = currentQuest.finishCondition.building.getLevel() >= currentQuest.finishCondition.buildingLevel;
                break;
        }    

        if (questFullfilled) {
            currentQuestSetFinished();
        }

        return questFullfilled;
    }

    /// <summary>
    /// Sets the currentQuest to finished and shows the User that he can get a Reward
    /// </summary>
    public void currentQuestSetFinished() {

        setCurrentQuestState(QuestStates.Finished);

        setStatusQuestInfoCircle(true);

        // If PopUp is open, update the Content
        if (Globals.UICanvas.uiElements.PopUpQuests.activeSelf) {
            loadCurrentQuest(false);
        }

        // Log Firebase Event
        KeyValuePair<string, object>[] valuePairArray = {
            new KeyValuePair<string, object>("currentQuestIndex", currentQuestIndex),
            new KeyValuePair<string, object>("World", Globals.Game.currentWorld.worldName),
            new KeyValuePair<string, object>("FirstGameStart", Globals.Game.currentUser.stats.FirstGameLoad.ToString()),
            new KeyValuePair<string, object>("DaysWithGameOpening", Globals.Game.currentUser.stats.DaysWithGameOpening),
            new KeyValuePair<string, object>("MinutesPlayedOverall", (int)(Globals.Game.currentUser.stats.SecondsPlayedOverall + Time.time)/60),
        };

        Globals.Controller.Firebase.IncrementFirebaseEventWithParameters(
        "quest_finished", valuePairArray);

        Globals.Controller.Firebase.IncrementFirebaseEventOnce("quest_finished_" + Globals.Game.currentWorld.worldName + "_" + currentQuestIndex);
    }


    /// <summary>
    ///  Claims the Rewards of currentQuest
    /// </summary>
    public void currentQuestClaimRewards() {
        currentQuestSetFinished();
        foreach (QuestReward reward in currentQuest.rewardList) {
            switch (reward.rewardType) {
                case QuestReward.RewardTypes.Emeralds:
                    Globals.UICanvas.uiElements.ParticleEffects.addEmeralds(reward.amountEmeralds);
                    break;
                case QuestReward.RewardTypes.Coins:
                    Globals.Game.currentWorld.addCoins(reward.amountCoins);
                    break;
                case QuestReward.RewardTypes.ItemForInventory:
                    Globals.Game.currentUser.inventory.createProperty(reward.itemForInventory);
                    rewardedWithItems.Invoke();
                    break;
            }
        }
    }

    /// <summary>
    /// Check if there is a Quest on CurrentIndex
    /// </summary>
    /// <returns></returns>
    public bool currentQuestIndexExits() {
        return (currentQuestIndex >= 0 && currentQuestIndex < questList.Count);
    }

    /// <summary>
    /// Raises the Index of the currentQuest and Manages to get the Next Quest or to finish the Quest-System of the current World
    /// </summary>
    public void loadNextQuest() {
        if (currentQuestIndex < questList.Count - 1) {
            currentQuestIndex++;
            if (currentQuestIndexExits()) {
                loadCurrentQuest(false);
                // Next QuestPopUp
                Globals.UICanvas.uiElements.PopUpQuests.SetActive(true);
                // Deactivate HUD Buttons
                UIElements.setHUDVisibility_LeftAndRight(false);
            } 
        } else {
            // Close QuestPopUp
            ButtonScripts.staticCLoseQuestPanel(true);
        }
    }

    /// <summary>
    /// Fills the Information of the currentQuest into the Quest-PopUp
    /// </summary>
    /// <param name="hasReward"></param>
    private void fillQuestPopUp(Quest aQuest, bool hasReward, bool finished) {

        questDescription = "";
        PopUp = Globals.UICanvas.uiElements.PopUpQuests.transform;
        PopUpContent = PopUp.Find("LayoutContent/Content");
        PopUpHeading = PopUp.Find("LayoutHeading/Heading");

        PopUpHeading.GetComponent<TMPro.TextMeshProUGUI>().text
                = Globals.Controller.Language.translateString(aQuest.langKeyHeading);

        InProgressInfo = PopUpContent.Find("InProgressInfo").gameObject.GetComponent<TMPro.TextMeshProUGUI>();

        if (finished) {
            // Finished Layout
            questDescription = aQuest.langKeyTextFinish;

            if (aQuest == currentQuest && !currentQuestIsLastQuestAndFinished() ) {
                // User clicked current Quest
                buttonInfoOk = PopUpContent.Find("LayoutFinished/Button_CollectQReward").gameObject;
                buttonInfoOkText = buttonInfoOk.transform.Find("Text").gameObject;

                if (aQuest.questType == QuestTypes.Quest) {
                    PopUpHeading.GetComponent<TMPro.TextMeshProUGUI>().text += Globals.Controller.Language.translateString("quest_finished");
                }

                //Reward?
                if (hasReward) {
                    buttonInfoOkText.GetComponent<TMPro.TextMeshProUGUI>().text
                    = Globals.Controller.Language.translateString("offlinecoins_button_claim");

                    PopUpContent.Find("LayoutFinished/RewardInfo").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text
                    = printRewards(aQuest);
                } else {
                    buttonInfoOkText.GetComponent<TMPro.TextMeshProUGUI>().text
                    = Globals.Controller.Language.translateString("quest_info_gotit");

                    PopUpContent.Find("LayoutFinished/RewardInfo").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = "";
                }

                // manage layouts
                PopUpContent.Find("LayoutFinished").gameObject.SetActive(true);
                PopUpContent.Find("LayoutQuest").gameObject.SetActive(false);
            } else {
                // User clicked an other than the current quest -> no interaction allowed
                PopUpContent.Find("LayoutFinished").gameObject.SetActive(false);
                PopUpContent.Find("LayoutQuest").gameObject.SetActive(false);
                InProgressInfo.text = "";
            }

        } else {
            // Start Layout
            questDescription = aQuest.langKeyTextStart;

            if (aQuest == currentQuest) {
                // User clicked current Quest
                buttonStartQuest = PopUpContent.Find("LayoutQuest/Button/Button_StartQuest").gameObject.GetComponent<Button>();

                if (aQuest.questState == QuestStates.InProgress) {
                    buttonStartQuest.interactable = false;
                    InProgressInfo.text = Globals.Controller.Language.translateString("quest_buttoninfo_inProgress");
                } else {
                    buttonStartQuest.interactable = true;
                    InProgressInfo.text = "";
                }

                //Reward?
                if (hasReward) {
                    PopUpContent.Find("LayoutQuest/Rewards/RewardInfo").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text
                    = printRewards(aQuest);
                } else {
                    PopUpContent.Find("LayoutQuest/Rewards/RewardInfo").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = "";
                }

                // manage layouts
                PopUpContent.Find("LayoutQuest").gameObject.SetActive(true);
                PopUpContent.Find("LayoutFinished").gameObject.SetActive(false);

            } else {
                // User clicked an other than the current quest -> no interaction allowed
                PopUpContent.Find("LayoutQuest").gameObject.SetActive(false);
                PopUpContent.Find("LayoutFinished").gameObject.SetActive(false);
                InProgressInfo.text = "";
            }

        }

        PopUpContent.Find("QuestDescription").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text
                = Globals.Controller.Language.translateString(questDescription);

        fillQuestList();

    }

    /// <summary>
    /// Fills the Quest List with the Quests of the currentWorld
    /// </summary>
    public void fillQuestList() {

        QuestListContent = Globals.UICanvas.uiElements.PopUpQuests.transform.Find("LayoutContent/Content/QuestList/Viewport/Content").gameObject;
        QuestBlueprint = Globals.UICanvas.uiElements.PopUpQuests.transform.Find("LayoutContent/Content/QuestBlueprint").gameObject;
        QuestBlueprintInactive = Globals.UICanvas.uiElements.PopUpQuests.transform.Find("LayoutContent/Content/QuestBlueprintInactive").gameObject;

        // Delete All Quest-Clones
        foreach (Transform child in QuestListContent.transform) {
            Destroy(child.gameObject);
        }

        QuestBlueprint.SetActive(false);
        QuestBlueprintInactive.SetActive(false);

        // Foreach Quest it will be a Quest-Panel in the QuestList
        iterator = Globals.Game.currentWorld.QuestsComponent.questList.Count-1;
        questListCount = 0;
        questListTurned = new List<Quest>(Globals.Game.currentWorld.QuestsComponent.questList);
        questListTurned.Reverse();
        foreach (Quest aQuest in questListTurned) {

            if (iterator <= currentQuestIndex) {
                if (iterator == currentQuestIndex && !currentQuestIsLastQuestAndFinished() ) {
                    clonedQuest = Instantiate(QuestBlueprint, QuestListContent.transform);
                } else {
                    clonedQuest = Instantiate(QuestBlueprintInactive, QuestListContent.transform);
                }

                clonedQuest.transform.Find("Heading").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text
                    = Globals.Controller.Language.translateString(aQuest.langKeyHeading);

                clonedQuest.SetActive(true);
                questListCount++;


                clonedQuest.GetComponent<QuestListPoint>().QuestListIterator = iterator;
            }

            iterator--;
        }

    }

    /// <summary>
    /// Returns if the World Quests are finished
    /// </summary>
    /// <returns></returns>
    public bool currentQuestIsLastQuestAndFinished() {
        return currentQuest.questState == QuestStates.Finished && currentQuestIndex == questList.Count - 1;
    }

    /// <summary>
    ///  Prints out the Rewards for a Quest as String
    /// </summary>
    /// <param name="quest"></param>
    public string printRewards(Quest quest) {
        rewardString = "";
        foreach (QuestReward reward in quest.rewardList) {
            switch (reward.rewardType) {
                case QuestReward.RewardTypes.Emeralds:
                    if (reward.amountEmeralds > 0) {
                        rewardString += "<sprite=\"Icon_Emerald\" name=\"Icon_Emerald\"> <COLOR=#00DA00>" + reward.amountEmeralds + "</COLOR>";
                    }
                    break;
                case QuestReward.RewardTypes.Coins:
                    if (reward.amountCoins > new IdleNum(0)) {
                        rewardString += "<sprite=\"Icon_Coins\" name=\"Icon_Coins\"> <COLOR=#000000>" + reward.amountCoins.toRoundedString() + "</COLOR>";
                    }
                    break;
                case QuestReward.RewardTypes.ItemForInventory:
                    rewardString += "Item: " + reward.itemForInventory.getTranslatedItemName();
                    break;
            }
            rewardString += "\n";
        }
        return rewardString;
    }


    /// <summary>
    /// Resets the whole Quest Component like at GameStart
    /// </summary>
    public void resetQuests() {
        currentQuestIndex = 0;
        currentQuestState = QuestStates.NotStarted;

        loadCurrentQuest(false);
    }



    /// <summary>
    /// Sets the Look of the Quest-Info-Button at the Main-GUI<br></br>
    /// If true, the User will be informed, that there is something new at the Quests
    /// </summary>
    /// <param name="isShown"></param>
    public void setStatusQuestInfoCircle(bool isShown) {
        Globals.UICanvas.uiElements.QuestInfoCircle.SetActive(isShown);

        // TODO Bring die Feder zum wackeln
    }

    /// <summary>
    /// Refers to the current Quest
    /// </summary>
    /// <returns></returns>
    public Quest getCurrentQuest() {
        return currentQuest;
    }

    /// <summary>
    /// Returns the current Quest Index, where the User is in the Quest-List
    /// </summary>
    /// <returns></returns>
    public int getCurrentQuestIndex() {
        return currentQuestIndex;
    }

    /// <summary>
    /// Set the current Quest Index
    /// </summary>
    /// <param name="i"></param>
    public void setCurrentQuestIndex(int i) {
        currentQuestIndex = i;
    }


    /// <summary>
    /// Sets the current Quest state
    /// </summary>
    /// <param name="newState"></param>
    public void setCurrentQuestState(QuestStates newState) {
        currentQuestState = newState;
        currentQuest.questState = newState;
    }



}

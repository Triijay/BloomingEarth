using UnityEngine;
using HardCodeLab.TutorialMaster;
using System.Collections.Generic;

public class TutorialController : MonoBehaviour {

    // Variables
    public TutorialMasterManager tmManager;

    public int desiredTutorialID = 0;
    public int desiredStageID = 0;

    public bool isInitialized = false;


    // IMPORTANT: TutorialController must be Inited on Start() otherwise the Modules will not work
    void Start() {

        isInitialized = checkTutorialStart();

    }

    public bool checkTutorialStart() {
        // Start Tutorial only when we want it
        // at the very beginning
        // or when a Tutorial already started
        if (!Globals.KaloaSettings.skipTutorial) {
            if ((Globals.Game.currentWorld.buildingsProgressArray[0].getLevel() == 0 && Globals.Game.currentUser.tutorialIDAndStageID.Equals(new KeyValuePair<int, int>()))
            || Globals.Game.currentUser.tutorialIsRunning) {
                // Set User Tutorial started
                setTutorialStatus(true);

                // Check where the User is in the Tutorial
                if (Globals.Game.currentUser.tutorialIDAndStageID.Equals(new KeyValuePair<int, int>())) {
                    Globals.Controller.Firebase.IncrementFirebaseEventOnce(Firebase.Analytics.FirebaseAnalytics.EventTutorialBegin);
                    // User never started Tutorial
                    tmManager.StartTutorial(0);
                } else {
                    startGameWithSpecificTutorialStage(desiredTutorialID, desiredStageID);
                }
            }
        }

        return true;
    }
    

    /// <summary>
    /// Starts the first Tutorial on the First Stage
    /// </summary>
    public void startTutorial(int index) {
        // Stop the currently running Tut
        tmManager.StopTutorial();
        // Start the desired Tutorial
        if (!Globals.KaloaSettings.skipTutorial) {
            tmManager.StartTutorial(index);
        }
    }

    /// <summary>
    /// Starts a specific Tutorial on a Specific Stage
    /// </summary>
    /// <param name="desiredTutorialID"></param>
    /// <param name="desiredStageID"></param>
    public void startGameWithSpecificTutorialStage(int desiredTutorialID, int desiredStageID = 0) {
        
        // Stops the active Tutorial
        tmManager.StopTutorial();

        // Set Active Stage on Desired Tutorial (because tmManager.ActiveTutorial.SetStage causes an error after Starting Tutorial, that nothing works)
        tmManager.Tutorials[desiredTutorialID].SetStage(desiredStageID);
        // Start Desired Tutorial
        tmManager.StartTutorial(desiredTutorialID);
        if(Globals.Controller.Language != null) {
            tmManager.SetLanguage(Globals.Controller.Language.currentLanguageIndex);
        } else {
            tmManager.SetLanguage(0);
        }

        // Invoke the Tutorial Events
        tmManager.ActiveTutorial.Events.OnTutorialEnter.Invoke(tmManager.ActiveTutorial);
        tmManager.ActiveTutorial.Events.OnTutorialStart.Invoke(tmManager.ActiveTutorial);

        // Invoke all Previous Stage Events, to unhide all necessary Buttons
        for ( int stageIndex = 0; stageIndex < tmManager.ActiveTutorial.Stages.Count; stageIndex++ ) {
            Stage stage = tmManager.ActiveTutorial.Stages[stageIndex];
            if (stageIndex <= desiredStageID) {
                stage.Events.OnStageEnter.Invoke(stage);
                stage.Events.OnStagePlay.Invoke(stage);
                if (stageIndex != desiredStageID) {
                    // Invoke all onStageExits except the currentStage's
                    stage.Events.OnStageExit.Invoke(stage);
                }
            }
        }

    }

    /// <summary>
    /// Saves Active Tutorial Index and Active Stage Index into UserData
    /// </summary>
    public void saveTutorialIDAndStageID() {
        // Save Active Tutorial Index and Active Stage Index
        if (tmManager.ActiveTutorial != null && tmManager.ActiveTutorial.ActiveStage != null) {

            int activeTut = tmManager.ActiveTutorialIndex;
            int activeStage = tmManager.ActiveTutorial.ActiveStageIndex;

            // Prevent to Save "nonStartable" Stages
            while (tmManager.ActiveTutorial.Stages[activeStage].Name.Contains("nonStartable") ) {
                // When the Name contains "nonStartable"-Tag, decrement the active Stage
                activeStage--;
            }

            Globals.Game.currentUser.tutorialIDAndStageID = new KeyValuePair<int, int>(activeTut, activeStage);
        } else {
            // Tutorial Ended
            setTutorialStatus(false);
            Firebase.Analytics.FirebaseAnalytics.LogEvent(Firebase.Analytics.FirebaseAnalytics.EventTutorialComplete);
        }
    }


    /// <summary>
    /// Sets the Tutorial Language<br></br>
    /// Note - To work properly, the LanguageList in TutorialManager must be in the Same Order as our LanguageList in Language.cs!
    /// </summary>
    /// <param name="language"></param>
    public void setTutorialLanguage(int languageIndex) {
        tmManager.SetLanguage(languageIndex);
    }

    /// <summary>
    /// Sets the User Tutorial Status for SaveGame
    /// </summary>
    public void setTutorialStatus(bool status) {
        // Set User Tutorial ended
        Globals.Game.currentUser.tutorialIsRunning = status;
    }


    /* Specials */


    /// <summary>
    /// Checks if the "-- get XX to Unlock nxt Building" is already Fulfilled
    /// </summary>
    public void skipStageIfBuildingAlreadyUnlockedNextBuilding(Building building) {
        if (building != null && building.getLevel() >= building.levelsToUnlockNextBuilding) {
            tmManager.NextStage();
        }
    }

    /// <summary>
    /// Checks if the "-- get XX to Level 1" is already Fulfilled
    /// </summary>
    public void skipStageIfBuildingIsAlreadyBuilt(Building building) {
        if (building != null && building.getLevel() >= 1) {
            tmManager.NextStage();
        }
    }

    /// <summary>
    /// Checks if the "-- get XXX to DelayUnlock" is already Fulfilled
    /// </summary>
    public void skipStageIfBuildingAlreadyInDelayUnlock(Building building) {
        if (building != null && building.getIsCheckingDelayUnlockProcess()) {
            tmManager.NextStage();
        }
    }

    /// <summary>
    /// Checks if the Inventory has already Items applicable for the Building
    /// </summary>
    public void skipStageIfInventoryHasItemsForBuilding(Building building) {
        if (building != null && building.hasUsableBuildingUpgrades()) {
            tmManager.NextStage();
        }
    }

}

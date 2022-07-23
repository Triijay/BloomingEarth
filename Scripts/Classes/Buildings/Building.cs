using UnityEngine;
using System.Collections.Generic;
using System;
using System.Globalization;
using UnityEngine.Serialization;
using Bayat.Json;
using UnityEngine.Events;

/// <summary>
/// The Building class wich will never be used Directly
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public abstract class Building : MonoBehaviour, IUpgradable {

    #region Vars

    /// <summary>
    /// The Current level of the Building
    /// </summary>
    [JsonProperty(PropertyName = "buildingLevel")]
    protected int level = 0;

    /// <summary>
    /// Name of the Building
    /// </summary>
    [JsonProperty(PropertyName = "buildingName")]
    protected string buildingName;

    /// <summary>
    /// Indicates wether the building was constructed before
    /// </summary>
    [JsonProperty(PropertyName = "wasConstructed")]
    public bool wasConstructed = false;

    /// <summary>
    /// Name of the World where the Building is placed
    /// </summary>
    protected string worldName;

    /// <summary>
    /// The Base Upgrade costs
    /// </summary>
    [Tooltip("The Base Upgrade costs")]
    public IdleNum baseCost = new IdleNum(10, "");


    /// <summary>
    /// The Factor at witch the cost increase each level (1.1 = 10% each level)
    /// </summary>
    [Tooltip("The Factor at witch the cost increase each level (1.1 = 10% each level)")]
    [Range(1.0f, 2.0f)]
    public float costFactor = 1.1f;

    /// <summary>
    /// The Current Upgrade costs for this Building
    /// </summary>
    [JsonProperty(PropertyName = "upgradeCost")]
    protected IdleNum upgradeCost = new IdleNum(0);

    /// <summary>
    /// The Current Upgrade costs to upgrade this Building 10 times
    /// so we dont have to Calculate this that often
    /// </summary>
    protected IdleNum upgradeCost5;

    /// <summary>
    /// The Current Upgrade costs to upgrade this Building 10 times
    /// so we dont have to Calculate this that often
    /// </summary>
    protected IdleNum upgradeCost10;

    /// <summary>
    /// The Current Upgrade costs to upgrade this based on the slider
    /// so we dont have to Calculate this that often
    /// </summary>
    protected IdleNum upgradeCostNext;

    /// <summary>
    /// The LevelUpIndicator for this Building
    /// </summary>
    protected Transform levelUpIndicator;

    /// <summary>
    /// The Renderer for the LevelUpIndicator
    /// </summary>
    protected Renderer renderer_levelUpIndicator;

    /// <summary>
    /// The LevelUpIndicator Renderer for the LevelUpIndicator
    /// </summary>
    private bool levelUppable = false;

    /// <summary>
    /// Level Shield
    /// </summary>
    private Transform SignLabel;

    /// <summary>
    /// The individual offset in Landscape-Mode, from where the Camera in the BuildingMenu stays<br></br>
    /// X = Zoom<br></br>
    /// Y = Pan Height<br></br>
    /// Z = Pan Left/Right
    /// </summary>
    [Tooltip("Offset BuildingMenu: X=Zoom(+=further,-=closer), Y=PanHeight(+=up,-=down), Z=PanRL(+=ObjMoreRight,-=ObjMoreLeft)")]
    public Vector3 buildingMenuOffsetLandscape = new Vector3(0, 0, 0);



    // * BuildingProgress
    // *    the Buildings can be unlocked only in a specific way (ordered by baseCost) - thats what we call the BuildingProgress

    /// <summary>
    /// Last Building in BuildingProgress
    /// </summary>
    protected Building LastBuilding;

    /// <summary>
    /// How many levels must be unlocked on this Building to unlock the next one
    /// </summary>
    [Tooltip("How many levels from this Building are required to unlock the next one")]
    public int levelsToUnlockNextBuilding = 25;

    /// <summary>
    /// Is the Building the next to build
    /// </summary>
    private bool isNextInBuildingProgress = false;



    // * DelayUnlocking-Process
    // *    From Level 0 to Level 1 the Building needs time

    /// <summary>
    /// DEFAULT Time delay to get from lvl 0 to lvl 1 - in seconds -> DO NOT OVERRIDE (only get)
    /// </summary>
    [Tooltip("Time delay (s) to get from lvl 0 to lvl 1")]
    public int level0To1DelaySeconds = 5;

    /// <summary>
    /// Time delay to get from lvl 0 to lvl 1 - in seconds -> Use this!
    /// </summary>
    private int delayUnlockProcessSeconds = 5;

    /// <summary>
    /// The Timestamp when the Building is Unlocked for LevelUps<br></br>
    /// </summary>
    [JsonProperty(PropertyName = "DelayUnlockTimestampWhenUnlocked")]
    private DateTime DelayUnlockTimestampWhenUnlocked = new DateTime();

    /// <summary>
    /// Bool if DelayUnlockCheck is running
    /// </summary>
    private bool isCheckingDelayUnlockProcess = false;

    /// <summary>
    /// How much percent can the DelayUnlock-Process reduced with Ads
    /// </summary>
    public const int DelayUnlockProcessMaxReducePercent = 60;

    /// <summary>
    /// DelayUnlock-Process Info
    /// </summary>
    private Transform DelayUnlockProcessIndicator;

    /// <summary>
    /// DelayUnlock-Process Info in Percent (Part of DelayUnlockProcessIndicator)
    /// </summary>
    private Transform percentBar;

    /// <summary>
    /// DelayUnlock-Process Info  Time-Left (Part of DelayUnlockProcessIndicator)
    /// </summary>
    private Transform TimeLeftLabel;



    // * BuildingUpgrade Slots
    // * 

    /// <summary>
    /// Slots for a BuildingUpgrade-Property to affect the behaviour of the Building
    /// </summary>
    protected Property[] upgradeSlots = new Property[4];

    /// <summary>
    /// Says what Level is needed to unlock the Slot of the Building
    /// </summary>
    [Tooltip("Says what Level is needed to unlock the Slot of the Building")]
    public int upgradeSlotUnlockLvl1st = 30, upgradeSlotUnlockLvl2nd = 60, upgradeSlotUnlockLvl3rd = 100;
    private int[] upgradeSlotUnlockLvlList = new int[3];


    // * Construction Mode
    // * 

    /// <summary>
    /// The Phases of a Building<br></br>
    /// 0 = Building not Constructed<br></br>
    /// 1 = Building in Construction<br></br>
    /// 2 = Building completed<br></br>
    /// </summary>
    public enum ConstructionPhases {
        ConstructionSite = 0,
        InConstruction = 1,
        Finished = 2
    }

    /// <summary>
    /// Array of ConstructionLevels<br></br>
    /// </summary>
    protected SortedDictionary<int, List<Transform>> ConstructionLevels = new SortedDictionary<int, List<Transform>>();

    /// <summary>
    /// Boolean if the Building is in ConstructionMode
    /// </summary>
    protected ConstructionPhases currentConstructionPhase = ConstructionPhases.ConstructionSite;




    // * LevelUpExtensions
    // *   When you are Levelling the Building, the Building can evolve with its Levels

    /// <summary>
    /// Array of LevelUpExtensions<br></br>
    /// LevelUpExtensions are additional Meshes of the Building that can be unlocked due LevelingUp
    /// </summary>
    private SortedDictionary<int, List<Transform>> LevelUpExtensions = new SortedDictionary<int, List<Transform>>();

    /// <summary>
    /// On what Level is the next LevelUpExtension (if 0, there is no Next)
    /// </summary>
    private int nextLevelUpExtensionsLevel = 0;

    // Unity Events for Levelling
    public UnityEvent onStartDelayUnlock;
    public UnityEvent onReachLevel1;
    public UnityEvent onReachLevel10;
    public UnityEvent onUnlockNextBuilding;


    // Performance optimization
    IdleNum cost, previous_lvl_costs;
    int levelCount, initLevel, newLevel, levelUntilUnlockExtension, temp_key;
    DateTime tempdatetime;
    TimeSpan du_timeRemaining;
    float du_percentUnlocked;
    Material mat;
    string someString;
    ConstructionPhases tempPhase;
    protected Dictionary<string, string> stringDict;

    #endregion


    /// <summary>
    /// Loads the Values of the Building <br></br>
    /// Values: level, upgradeCost, upgradeCost10
    /// </summary>
    public virtual void loadBuilding() {

        buildingName = transform.name;
        worldName = transform.parent.parent.name;
        isNextInBuildingProgress = false;

        // Sets the GameObject visible, if the Level is more than 0
        gameObject.SetActive(level > 0);

        // DelayUnlock-Process Seconds
        //if (wasConstructed) {
        //    delayUnlockProcessSeconds = Globals.Game.currentUser.prestige.prestigeConstructionTime;
        //} else {

            // Admin Google Settings for Delay
            if (Application.platform == RuntimePlatform.WindowsEditor || Globals.Controller.GPiOS.isAdm()) {
                delayUnlockProcessSeconds = Globals.KaloaSettings.adminUnlockProcessSec;
            } else {
                // Set Default Seconds to DelayUnlock-Process
                delayUnlockProcessSeconds = level0To1DelaySeconds; 
            }

        //}

        if (upgradeCost == new IdleNum(0)) {
            upgradeCost = baseCost;
        }

        upgradeCost5 = getUpgradeCost(5);
        upgradeCost10 = getUpgradeCost(10);

        // ConstructionMode
        initConstructionMode();

        // LevelUpExtensions
        initLevelUpExtensions();

    }





    /// <summary>
    /// Fires at the Creation of the Object
    /// </summary>
    public virtual void initIntegratedUIElements() {

        // Load Shield
        SignLabel = gameObject.transform.Find("Sign/Label");
        updateLabel();

        levelUpIndicator = gameObject.transform.Find("LevelUpIndicator");
        if (levelUpIndicator != null) {
            renderer_levelUpIndicator = levelUpIndicator.GetComponent<Renderer>();
        }

        // Load PercentBar
        DelayUnlockProcessIndicator = gameObject.transform.Find("DelayUnlock-Process");
        percentBar = gameObject.transform.Find("DelayUnlock-Process/Bar/PercentBarPivot");
        TimeLeftLabel = gameObject.transform.Find("DelayUnlock-Process/Bar/TimeLeftLabel");    

    }



    /// <summary>
    /// Get the upgradeCost for x amount of upgrades and add them up so we
    /// get the total amount it would cost to upgrade
    /// so : costs = costs + previous_lvl_costs * costfactor
    /// </summary>
    /// <param name="amount">Amount to upgrade</param>
    /// <returns>Cost of Upgrading 'amount' times</returns>
    public IdleNum getUpgradeCost(int amount) {

        // Init cost to The current upgradecost of this building
        cost = upgradeCost;
        previous_lvl_costs = upgradeCost;

        // Go over how often we want to Upgrade
        for (int i = 1; i < amount; i++) {
            // This will be needed for the next calculation in this loop if there is one
            previous_lvl_costs = previous_lvl_costs * costFactor;

            // Add on top of the costs
            cost += previous_lvl_costs;
        }

        return cost;
    }

    /// <summary>
    /// Calculate the Upgradecosts after lvlup without adding them up
    /// so : costs = costs*costfactor
    /// </summary>
    /// <param name="amount">The amount to check for</param>
    public void calcNextUpgradeCost(int amount) {
        // Go over how often we want to Upgrade
        for (int i = 0; i < amount; i++) {
            // And Calculate the Costs
            upgradeCost *= costFactor;
        }
        // After upgradeCost is set, we can evaluate the upgradeCost for next 10 lvls
        upgradeCost5 = getUpgradeCost(5);
        upgradeCost10 = getUpgradeCost(10);
    }

    /// <summary>
    /// Calculate the Upgradecosts after lvlup without adding them up
    /// so : costs = costs*costfactor
    /// </summary>
    /// <param name="amount">The amount to check for</param>
    public void setUpgradeCostNext(int sliderValue) {
        // Define the levelamount to update based on the slider (DelayUnlock-Process level==0)
        if (sliderValue == 1 || level == 0) {
            upgradeCostNext = upgradeCost;
        } else if (sliderValue == 2) {
            upgradeCostNext = upgradeCost5;
        } else if (sliderValue == 3) {
            upgradeCostNext = upgradeCost10;
        } else {
            Debug.LogError("LevelUpAmountSlider->SliderOutOfRangeError");
        }
    }

    /// <summary>
    /// Returns the Name of the Building
    /// </summary>
    /// <returns></returns>
    public string getName() {
        return buildingName;
    }

    /// <summary>
    /// Returns the Infos of the Building
    /// </summary>
    /// <returns></returns>
    public virtual Dictionary<string, string> getInfo(int levelUpAmount) {
        
        stringDict = new Dictionary<string, string>();

        // Levels
        stringDict.Add("Level", level.ToString());

        if (level == 0) {
            // DelayUnlock-Process - you can only go from level 0 to 1
            levelUpAmount = 1;
        }
        stringDict.Add("LevelUpAmount", levelUpAmount.ToString());
        stringDict.Add("UpgrCost", getUpgradeCost(levelUpAmount).toRoundedString());

        stringDict.Add("UpgrCostHasEnoughMoney", (Globals.Game.currentWorld.getCoins() >= upgradeCostNext).ToString());


        // BuildingProgress
        stringDict.Add("LevelsToUnlockNextBuilding", levelsToUnlockNextBuilding.ToString());
        if (LastBuilding != null) {
            bool levelLock = LastBuilding.getNextBuildingIsLocked();
            stringDict.Add("LastBuildingLevelLock", levelLock.ToString());
            stringDict.Add("LastBuildingLevel", LastBuilding.getLevelsToUnlockNextBuilding().ToString());
            stringDict.Add("LastBuildingName", Globals.Controller.Language.translateString("building_name_" + LastBuilding.getName()));
        } else {
            stringDict.Add("LastBuildingLevelLock", "False");
            stringDict.Add("LastBuildingLevel", "False");
            stringDict.Add("LastBuildingName", "False");
        }

        // DelayUnlockProcess
        stringDict.Add("level0to1TimestampUntilUnlock", DelayUnlockTimestampWhenUnlocked.ToString());

        // LevelUpExtension
        stringDict.Add("nextLevelUpExtensionsLevel", nextLevelUpExtensionsLevel.ToString());

        //Slots
        stringDict.Add("upgradeSlotUnlockLvl1st", upgradeSlotUnlockLvl1st.ToString());
        stringDict.Add("upgradeSlotUnlockLvl2nd", upgradeSlotUnlockLvl2nd.ToString());
        stringDict.Add("upgradeSlotUnlockLvl3rd", upgradeSlotUnlockLvl3rd.ToString());


        return stringDict;
    }

    /// <summary>
    /// Returns the Level
    /// </summary>
    /// <returns></returns>
    public int getLevel() {
        return level;
    }

    /// <summary>
    /// Checks if the Building is LevelUppable
    /// </summary>
    public bool isLevelUppable() {
        return levelUppable;
    }


    /// <summary>
    /// Triggers the LevelUp Function of the Building <br></br>
    /// decides how much it has to LevelUp because of the LevelUpAmountSlider
    /// </summary>
    public void triggerLevelUp() {
        if (Globals.UICanvas.uiElements.LevelUpAmountSlider.value == 1 || level == 0) {
            // Level it up once  - When the building is Level 0 it will always level up 1 level
            levelUp(1);
        } else if (Globals.UICanvas.uiElements.LevelUpAmountSlider.value == 2) {
            // Level it up 5 times
            levelUp(5);
        } else if (Globals.UICanvas.uiElements.LevelUpAmountSlider.value == 3) {
            // Level it up 10 times
            levelUp(10);
        } else {
            Debug.LogError("LevelUpAmountSlider->SliderOutOfRangeError");
        }

        // Update ALL LevelUpIndicators, because BankRoll changed
        Globals.Game.currentWorld.checkLevelUpIndicatorsForced();
    }

    /// <summary>
    /// Update the Level of the Building <br></br>
    /// Check if we have enough money to upgrade <br></br>
    /// If the Coins where prepayed, the levelUp will be executed without removing Coins from the Bank Account
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="prepayed"></param>
    public virtual bool levelUp(int amount, bool prepayed = false) {

        initLevel = level;

        // Check if we have enough Coins to Upgrade AND the last Building in progress allows it
        if (amount > 0 &&
            (prepayed || Globals.Game.currentWorld.getCoins() >= getUpgradeCost(amount)) && // User not enough coins
            (Globals.Game.currentWorld.buildingsProgressArray[0] == this || !LastBuilding.getNextBuildingIsLocked())) { // Last Building is not leveled up enough

            if (isCheckingDelayUnlockProcess) {
                // If this Building is currently in DelayUnlockprocess, we are doing nothing
                return false;
            }
            
            // Remove the Cost from the Global Coins if not prepayed
            if (!prepayed) {
                Globals.Game.currentWorld.removeCoins(getUpgradeCost(amount));
            }

            // Feedback
            Vibration.Vibrate(20);

            if (!prepayed && initLevel == 0 && delayUnlockProcessSeconds != 0  && // When Building is Level 0, the only way to get to Level 1 is over the DelayUnlock-Process
                (DelayUnlockTimestampWhenUnlocked == new DateTime() || System.DateTime.Now < System.DateTime.FromBinary(System.Convert.ToInt64(DelayUnlockTimestampWhenUnlocked)))
                ) {

                initDelayUnlockProcess();
                onStartDelayUnlock.Invoke();

                // Update BuildingMenu
                if (Globals.UICanvas.uiElements.BuildingMenu.activeSelf) {
                    Globals.UICanvas.uiElements.InitGameObject.GetComponent<BuildingMenu>().updateBuildingMenu();
                }

                return false;
            }

            // Calculate new UpgradeCost
            calcNextUpgradeCost(amount);

            // Level Up the Building
            level += amount;


            // Check BuildingProgress
            if (initLevel <= 0 || initLevel == levelsToUnlockNextBuilding - 1) {
                Globals.Game.currentWorld.checkNextBuildingProgress();
            }

            // Play UnityEvent onUnlockNextBuilding
            if (level >= levelsToUnlockNextBuilding && level - amount <= levelsToUnlockNextBuilding) {
                onUnlockNextBuilding.Invoke();
            }

            // Update BuildingMenu
            if (Globals.UICanvas.uiElements.BuildingMenu.activeSelf) {
                Globals.UICanvas.uiElements.InitGameObject.GetComponent<BuildingMenu>().updateBuildingMenu();
            }

            // Update OverlayMenu
            try {
                Globals.UICanvas.uiElements.OverlayMenu.updateOverlayInfos();
            }
            catch (Exception e){
                Globals.UICanvas.DebugLabelAddText(e, true);
            }

            // Update local Score Total Levels
            Globals.Game.currentUser.scoreTotalLevels += amount;
            // Update level in current run
            //Globals.Game.currentUser.prestige.levelInCurrentRun += amount;
            // Update Prestige
            //Globals.Game.currentUser.prestige.updatePrestigeLevels();

            // Achievements
            checkBuildingAchievements(initLevel, amount);

            // LevelUpExtensions
            checkLevelUpExtensions(level);

            // Quests
            checkQuests();

            // Saving
            if (initLevel == 0) {
                // Save Game Data
                _ = SavingSystem.saveGameData();
            } else {
                // Update Save Count
                SavingSystem.updateSaveCount(amount);
            }


            return true;
        } else {
            // If we haven´t enough money to upgrade we have to stop the function
            // so the child method functionality will not be executed
            return false;
        }

    }

    /// <summary>
    /// Checks if Building LvlUp triggered a Quest.Finish
    /// </summary>
    private void checkQuests() {
        // Only Finish Quests if they are started
        if (Globals.Game.currentWorld.QuestsComponent.getCurrentQuest().questState == Quest.QuestStates.InProgress) {
            // If this building is the currentQuestCondition
            if (this == Globals.Game.currentWorld.QuestsComponent.getCurrentQuest().finishCondition.building) {
                Globals.Game.currentWorld.QuestsComponent.checkCurrentQuestAlreadyFulfilled();
            }
        }
    }

    /// <summary>
    ///  Checks if the new Level of a Building should trigger an Achievement or an Event
    /// </summary>
    private void checkBuildingAchievements(int initLevel, int amountLevelledUp) {


        // Log Firebase Event
        KeyValuePair<string, object>[] valuePairArray = {
                new KeyValuePair<string, object>("BuildingName", buildingName),
                new KeyValuePair<string, object>("amountLevelledUp", amountLevelledUp),
                new KeyValuePair<string, object>("newLevel", initLevel+amountLevelledUp),
                new KeyValuePair<string, object>("FirstGameStart", Globals.Game.currentUser.stats.FirstGameLoad.ToString()),
                new KeyValuePair<string, object>("DaysWithGameOpening", Globals.Game.currentUser.stats.DaysWithGameOpening),
                new KeyValuePair<string, object>("MinutesPlayedOverall", (int)(Globals.Game.currentUser.stats.SecondsPlayedOverall + Time.time)/60),
            };

        Globals.Controller.Firebase.IncrementFirebaseEventWithParameters(
            "level_up", valuePairArray);
       

        // We use these if-Formulas because when you are raising 10 or MAX Levels, you could skip the achievements otherwise 
        // OR we would sent the achievements on EVERY LevelUp what would cause massive traffic
        if (initLevel == 0) {
            // Check for Achievements on Construction
            // Building Progress Event
            Globals.Controller.GPiOS.IncrementEventOnce(GPGSIds.event_buildingprogress);
            Globals.Controller.Firebase.IncrementFirebaseEventOnce("building_progress_firstLevel_" + buildingName, "level");
            Globals.Controller.Firebase.IncrementFirebaseEventWithParameters(
            "building_progress_firstLevel", valuePairArray);

            // Check EventsAndAchievement Dictionary
            if (Globals.Game.currentWorld.achievementsAndEventsLevel1.ContainsKey(buildingName)) {
                if (Globals.Game.currentWorld.achievementsAndEventsLevel1[buildingName]["achievement"] != "") {
                    // Globals.UICanvas.DebugLabelAddText("Achievement " + Globals.Game.currentWorld.achievementsAndEventsLevel1[buildingName]["achievement"]);
                    Globals.Controller.GPiOS.sentAchievement100Percent(Globals.Game.currentWorld.achievementsAndEventsLevel1[buildingName]["achievement"]);
                }
            }

            // UnityEvents
            onReachLevel1.Invoke();

        } else if (initLevel < 10 && initLevel + amountLevelledUp >= 10) {
            // UnityEvents
            onReachLevel10.Invoke();
        } else if (initLevel < 30 && initLevel + amountLevelledUp >= 30) {
            // Check for Achievements - Specials
            switch (buildingName) {
                case "House":
                    Globals.Controller.GPiOS.sentAchievement100Percent(GPGSIds.achievement_diligent_worker);
                    break;
            }
        }

    }

    /// <summary>
    /// Initiate the DelayUnlocking-Process if not initiated yet
    /// </summary>
    public void initDelayUnlockProcess() {

        Debug.Log("Initiating the UnlockProcess");

        // Admin Google Settings for Delay
        if (Application.platform == RuntimePlatform.WindowsEditor || Globals.Controller.GPiOS.isAdm()) {
            delayUnlockProcessSeconds = Globals.KaloaSettings.adminUnlockProcessSec;
        }

        if (DelayUnlockTimestampWhenUnlocked == new DateTime()) {

            // get the current Time
            System.DateTime CurrentTime = System.DateTime.Now;

            // Add the UnlockTime and save it into the building var
            DelayUnlockTimestampWhenUnlocked = CurrentTime.AddSeconds(delayUnlockProcessSeconds);
        }


        // Start Delayunlock, if its not running
        if (!isCheckingDelayUnlockProcess) {

            changeConstructionPhase(ConstructionPhases.InConstruction);

            Debug.Log("Invoke the UnlockProcess Checkfunction");

            // Play Sound
            Globals.Controller.Sound.PlaySound("StartBuilding");

            // Set Building into Globals, so that the Game knows that a Building is in Process
            Globals.Game.currentWorld.buildingInDelayUnlock = this;

            isCheckingDelayUnlockProcess = true;

            // Update Time Label
            updateDelayUnlockTimeLeftLabel();

            DelayUnlockProcessIndicator.gameObject.SetActive(true);

            InvokeRepeating("checkDelayUnlockProcess", 1f, 1f);

        }
    }

    /// <summary>
    /// Check wether the DelayUnlocking-Process is finished
    /// </summary>
    protected virtual void checkDelayUnlockProcess() {

        // Check if time for Level1 has come
        if (System.DateTime.Now >= DelayUnlockTimestampWhenUnlocked) {

            //** Building is ready to Level Up

            this.isCheckingDelayUnlockProcess = false;

            levelUp(1, true);

            // Set the wasConstructed bool var to true
            this.wasConstructed = true;

            resetDelayUnlockProcess();

            Debug.Log("Building " + buildingNameIdentifier() + ": automated Level Up to Level 1");

        } else {

            //** Building is not ready

            // Update Time Label
            updateDelayUnlockTimeLeftLabel();
        }
    }

    public void resetDelayUnlockProcess() {
        isCheckingDelayUnlockProcess = false;

        // Set the Building Status to the usual values and cancel the checking
        DelayUnlockTimestampWhenUnlocked = new DateTime();
        CancelInvoke("checkDelayUnlockProcess");

        // Fade Out the Indicator
        DelayUnlockProcessIndicator.gameObject.SetActive(false);
        Globals.UICanvas.uiElements.BuildingMenuAdButtonMinus30.SetActive(false);

        // Tell the World that no Building is in the Process anymore
        Globals.Game.currentWorld.buildingInDelayUnlock = null;
    }


    /// <summary>
    /// Updates the Timeleft Label from DelayUnlockProcess - rounds the Seconds
    /// </summary>
    public void updateDelayUnlockTimeLeftLabel() {
        this.du_timeRemaining = DelayUnlockTimestampWhenUnlocked - DateTime.Now;

        // Get the Percentage how much the Building is Unlocked - but not from Timestamp, only from the delayUnlockProcessDelay, because otherwise the Percentage would be faked, when -30 Button pressed
        this.du_percentUnlocked = 1 - (float)this.du_timeRemaining.TotalSeconds / delayUnlockProcessSeconds;
        if (this.du_percentUnlocked < 0) this.du_percentUnlocked = 0;
        if (this.du_percentUnlocked > 1) this.du_percentUnlocked = 1;

        // Scale the Percentbar from 0..0.5..1
        this.percentBar.localScale = new Vector3(1, 1, this.du_percentUnlocked);
        if (du_timeRemaining.TotalSeconds >= 0) {
            someString = Convert.ToInt32(Math.Round(du_timeRemaining.TotalSeconds % 60, 0)).ToString("D2");
            TimeLeftLabel.GetComponent<TMPro.TextMeshPro>().text = du_timeRemaining.ToString(@"hh\:mm", new CultureInfo("en-US")) + ":" + someString;
        }
    }

    // Reduce the Waiting Time from DelayUnlock-Process
    public void ReduceWaitingTimeMinus30() {
        DelayUnlockTimestampWhenUnlocked = DelayUnlockTimestampWhenUnlocked.AddSeconds(-30 * 60);
    }

    /// <summary>
    /// Get the Information if Building is currently in DelayUnlock-Process
    /// </summary>
    /// <returns></returns>
    public DateTime getDelayUnlockProcessTimestampUntilUnlock() {
        return DelayUnlockTimestampWhenUnlocked;
    }

    public bool getIsCheckingDelayUnlockProcess() {
        return isCheckingDelayUnlockProcess;
    }

    public bool delayUnlockCanBeReduced() {
        return du_percentUnlocked < DelayUnlockProcessMaxReducePercent/100f; 
    }





    /// <summary>
    /// Update the Level of the Building <br></br>
    /// Check if we have enough money to upgrade
    /// </summary>
    /// <param name="amount"></param>
    public virtual void levelUpMax() {
        // Check if we have enough Coins to Upgrade
        levelUp(checkLevelsMaxLvlUp());
    }


    /// <summary>
    /// Check how much Levels the Building can upgraded
    /// </summary>
    /// <param name="amount"></param>
    public int checkLevelsMaxLvlUp() {
        // Init cost to The current upgradecost of this building
        cost = upgradeCost;
        previous_lvl_costs = upgradeCost;
        levelCount = 0;

        // Check if we have enough Coins to Upgrade
        while (Globals.Game.currentWorld.getCoins() >= cost) {
            // This will be needed for the next calculation in this loop if there is one
            previous_lvl_costs = previous_lvl_costs * costFactor;

            // Add on top of the costs
            cost += previous_lvl_costs;

            levelCount++;
        }

        return levelCount;
    }


    /// <summary>
    /// Level up this Building for free with the Editor for Debugging<br></br>
    /// ALSO IT SHOWS A HIDDEN BUILDING
    /// </summary>
    [ExposeMethodInEditorAttribute]
    public void levelUpDebug1() {
        gameObject.SetActive(true);
        levelUp(1, true);
    }

    /// <summary>
    /// Level up this Building 10 times for free with the Editor<br></br>
    /// ALSO IT SHOWS A HIDDEN BUILDING
    /// </summary>
    [ExposeMethodInEditorAttribute]
    public void levelUpDebug10() {
        gameObject.SetActive(true);
        levelUp(10, true);
    }

    /// <summary>
    /// Level up All Buildings to the Min Value to get the next Building
    /// </summary>
    [ExposeMethodInEditorAttribute]
    public void levelUpAllBuildings() {
        foreach (Building aBuilding in Globals.Game.currentWorld.buildingsProgressArray) {
            aBuilding.gameObject.SetActive(true);
            Debug.LogWarning("Levelling up building " + aBuilding .getName() + " to " + aBuilding.levelsToUnlockNextBuilding);
            aBuilding.levelUp(aBuilding.levelsToUnlockNextBuilding, true);
        }
    }

    /// <summary>
    /// Level up this Building X times for free with the Editor<br></br>
    /// ALSO IT SHOWS A HIDDEN BUILDING
    /// </summary>
    public void levelUpDebug(int amount) {
        gameObject.SetActive(true);
        levelUp(amount, true);
    }

    /// <summary>
    /// Updates the Label on the Level-Shield
    /// </summary>
    public void updateLabel() {
        // Also Try Updating the Label
        SignLabel.GetComponent<TMPro.TextMeshPro>().text = level.ToString();
    }

    public string buildingNameIdentifier() {
        return buildingName + "_" + worldName;
    }


    /// <summary>
    /// Sets the Levelindicator for the Building
    /// </summary>
    public virtual void setLevelUpIndicator() {
        // Check if Amount could be updated AND the LastBuilding has reached the unlock-Level in BuildingProgress
        if (Globals.Game.currentWorld.getCoins() >= upgradeCostNext && (Globals.Game.currentWorld.buildingsProgressArray[0] == this || !LastBuilding.getNextBuildingIsLocked()) && !isCheckingDelayUnlockProcess) {
            // only trigger Material change if levelUppable is changes
            if (!levelUppable) {
                // Paint LevelIndicator - ready for Update
                renderer_levelUpIndicator.material = Globals.Game.currentWorld.levelUpIndicator_updateable;
                levelUppable = true;
            }
        } else {
            // only trigger Material change if levelUppable is changes
            if (levelUppable) {
                // Paint LevelIndicator transparent
                renderer_levelUpIndicator.material = Globals.Game.currentWorld.levelUpIndicator_transparent;
                levelUppable = false;
            }
        }

        // Set the button state of the level up button in overlay menu as the indicator state
        Globals.UICanvas.uiElements.OverlayMenu.checkLevelUpButtonState(this, levelUppable);
    }

    /// <summary>
    /// Sets the next Building in BuildingProgress
    /// </summary>
    public void setLastBuilding(Building b) {
        LastBuilding = b;
    }

    /// <summary>
    /// sets the Building Status in BuildingProgress
    /// </summary>
    public void setIsNextInBuildingProgress(bool isNext) {
        if (isNext) {
            // Fade In the Building and Set it to Construction Modus
            gameObject.SetActive(true);
            if (isCheckingDelayUnlockProcess) {
                changeConstructionPhase(ConstructionPhases.InConstruction);
            } else {
                changeConstructionPhase(ConstructionPhases.ConstructionSite);
            }
        } else {
            if (isNextInBuildingProgress) {
                // If the Building was the last Construction, it will not displayed finished
                changeConstructionPhase(ConstructionPhases.Finished);
            }
        }
        isNextInBuildingProgress = isNext;
    }

    /// <summary>
    /// Returns if this Building is next in the Buildingprogress
    /// </summary>
    /// <returns></returns>
    public bool getIsNextInBuildingProgress() {
        return isNextInBuildingProgress;
    }


    /// <summary>
    /// How many levels from th is Building are required to unlock the next one
    /// </summary>
    public int getLevelsToUnlockNextBuilding() {
        return levelsToUnlockNextBuilding;
    }

    public bool getNextBuildingIsLocked() {
        return (level < levelsToUnlockNextBuilding);
    }




    /// <summary>
    /// Inits the BuildingUpgradeSlots
    /// </summary>
    public void initUpgradeSlots() {

        upgradeSlotUnlockLvlList = new int[3] { upgradeSlotUnlockLvl1st, upgradeSlotUnlockLvl2nd, upgradeSlotUnlockLvl3rd };

        int slot = 1;
        foreach (Property prop in Globals.Game.currentUser.inventory.getPropertyList()) {
            if (prop.getWhereInUse() == buildingNameIdentifier()) {
                // Lay down the BuildingUpgrades into the Slots
                setUpgradeSlot(slot, prop);
                slot++;
            }
        }
    }

    /// <summary>
    /// Adds an specific Upgrade 
    /// </summary>
    /// <param name="upgr"></param>
    /// <param name="building"></param>
    /// <param name="slotToPushIn"></param>
    public bool addBuildingUpgrade(Property propToPushIn, int slotToPushIn) {

        // If Item was really in Inventory
        if (Globals.Game.currentUser.inventory.containsProperty(propToPushIn)) {

            // Check if Item is Compatible to the Building
            if (((BuildingUpgrade)propToPushIn.ReferencedItem).isBuildingUpgradeCompatibleTo(this)) {

                // If level high enough for the slot
                if (level >= upgradeSlotUnlockLvlList[slotToPushIn - 1]) {

                    if (checkUpgradeSlotFree(slotToPushIn)) {
                        // If Prop is switched into another slot, remove it there
                        int i = 0;
                        foreach (Property itemHoldByBuilding in upgradeSlots) {
                            if (itemHoldByBuilding == propToPushIn) {
                                resetUpgradeSlot(i);
                            }
                            i++;
                        }

                        // Adds the Item to the desired UpgradeSlot
                        setUpgradeSlot(slotToPushIn, propToPushIn);

                        // Flag Item in Inventory as Used
                        propToPushIn.setInUseAt(this.buildingNameIdentifier());

                        return true;
                    } else {
                        Debug.LogError("Item Slot is not free");
                    }

                } else {
                    Debug.LogError("Item Slot is not unlocked yet");
                }

            } else {
                Debug.LogError("Item is not a compatible with Building");
            }
        } else {
            Debug.LogError("Item is not owned by User");
        }

        // item was not compatible
        return false;
    }

    /// <summary>
    /// Removes a specific Upgrade from a building
    /// </summary>
    /// <param name="building"></param>
    public void removeBuildingUpgrade(Property prop) {
        // Find Prop and remove from specific slot
        int i = 0;
        foreach (Property item in upgradeSlots) {
            if (item == prop) {
                resetUpgradeSlot(i);
            }
            i++;
        }

        // Add item back to inventory
        prop.setNotInUse();
    }

    /// <summary>
    /// Adds the BuildingUpgrade to the desired UpgradeSlot
    /// </summary>
    protected virtual void setUpgradeSlot(int slot, Property upgr) {
        upgradeSlots[slot] = upgr;
    }

    /// <summary>
    /// Nulls the UpgradeSlot
    /// </summary>
    /// <param name="slot"></param>
    private void resetUpgradeSlot(int slot) {
        setUpgradeSlot(slot, null);
    }

    /// <summary>
    /// Returns the Property of the UpgradeSlot
    /// </summary>
    /// <returns></returns>
    public Property getUpgradeSlot(int slot) {
        return upgradeSlots[slot];
    }

    /// <summary>
    /// Returns the List of Properties holded by the Building
    /// </summary>
    /// <returns></returns>
    public Property[] getUpgradeSlots() {
        return upgradeSlots;
    }

    /// <summary>
    /// Returns if the UpgradeSlot is free
    /// </summary>
    /// <param name="slot">SlotNumber</param>
    /// <returns></returns>
    public bool checkUpgradeSlotFree(int slot) {
        return (getUpgradeSlot(slot) == null);
    }

    /// <summary>
    /// Returns if there are BuildingUpgrades in the Inventory, that the User can use on this Building
    /// </summary>
    /// <returns></returns>
    public bool hasUsableBuildingUpgrades() {
        foreach (Property prop in Globals.Game.currentUser.inventory.getPropertyListOnlyUnused()) {
            if (((BuildingUpgrade)prop.ReferencedItem).isBuildingUpgradeCompatibleTo(this)) {
                return true;
            }
        }
        return false;
    }




    /// <summary>
    /// Returns the ConstructionModePhase Dictionary
    /// </summary>
    public SortedDictionary<int, List<Transform>> getConstructionLevels() {
        return ConstructionLevels;
    }

    /// <summary>
    /// Check for the Buildings ConstructionMode
    /// </summary>
    private void initConstructionMode() {
        try {
            addValidConstructionLevels(this.transform.Find("ConstructionLevels"));
        }
        catch {
            Debug.LogWarning("Warning: Building " + this.buildingName + " has no GameObject called 'ConstructionLevels' (should be built in Cinema4D)");
        }

        try {
            addValidConstructionLevels(this.transform.Find("ConstructionLevelsEngine"));
        }
        catch {
            Debug.LogWarning("Warning: Building " + this.buildingName + " has no GameObject called 'ConstructionLevelsEngine' (should be built in Unity)");
        }

        // See what ConstructionPhase should be activated
        if (level == 0) {
            if (isCheckingDelayUnlockProcess) {
                changeConstructionPhase(ConstructionPhases.InConstruction);
            } else {
                changeConstructionPhase(ConstructionPhases.ConstructionSite);
            }
        } else {
            changeConstructionPhase(ConstructionPhases.Finished);
        }

        // If there is any valid LevelUpExtension
        checkConstructionLevels();
    }

    /// <summary>
    /// Displays the whole building in a specific ConstructionPhase<br></br>
    /// 0 = Building not Constructed<br></br>
    /// 1 = Building in Construction<br></br>
    /// 2 = Building completed<br></br>
    /// </summary>
    public void changeConstructionPhase(ConstructionPhases phase) {
        currentConstructionPhase = phase;
        checkConstructionLevels();
    }

    /// <summary>
    /// Displays or Hides the right Objects for Construction
    /// </summary>
    public void checkConstructionLevels() {
        if (ConstructionLevels.Count > 0) {
            foreach (KeyValuePair<int, List<Transform>> ConstructionLevel in ConstructionLevels) {
                // Go through the Extensions from the List of this Level
                foreach (Transform Extension in ConstructionLevel.Value) {
                    // Set the Objects of current ConstructionMode to visible, others to false
                    if ((int)currentConstructionPhase == ConstructionLevel.Key) {
                        Extension.gameObject.SetActive(true);
                    } else {
                        Extension.gameObject.SetActive(false);
                    }
                }
            }
        }
    }



    /// <summary>
    /// Adds all the ConstructionLevels from the given ConstructionLevel-RootObject<br></br>
    /// Only direct childs of this Object are valid, and they must be names like ...
    /// </summary>
    /// <param name="root"></param>
    public void addValidConstructionLevels(Transform root) {
        Transform[] ConstructionLevelsTransforms = root.GetComponentsInChildren<Transform>(true);
        foreach (Transform ConstructionLevel in ConstructionLevelsTransforms) {

            if (ConstructionLevel.parent == root) {
                // Transform is a direct child of root-Object
                Enum.TryParse<ConstructionPhases>(ConstructionLevel.name, true, out tempPhase);
                temp_key = (int)tempPhase;
                if (!ConstructionLevels.ContainsKey(temp_key)) {
                    ConstructionLevels.Add(temp_key, new List<Transform> { ConstructionLevel });
                } else {
                    ConstructionLevels[temp_key].Add(ConstructionLevel);
                }

            }
        }
    }

    /// <summary>
    /// Returns the LevelUpExtensions Dictionary
    /// </summary>
    public SortedDictionary<int, List<Transform>> getLevelUpExtensions() {
        return LevelUpExtensions;
    }

    /// <summary>
    /// Check for the Buildings LevelUpExtensions and activate them, if the Level is high enough
    /// </summary>
    private void initLevelUpExtensions() {

        try {
            addValidLevelExtensions(this.transform.Find("LevelUpExtensions"));
        }
        catch {
            Debug.LogWarning("Warning: Building "+this.buildingName+" has no GameObject called 'LevelUpExtensions' (should be built in Cinema4D)");
        }

        try {
            addValidLevelExtensions(this.transform.Find("LevelUpExtensionsEngine"));
        } 
        catch {
            Debug.LogWarning("Warning: Building " + this.buildingName + " has no GameObject called 'LevelUpExtensionsEngine' (should be built in Unity)");
        }

        // If there is any valid LevelUpExtension
        checkLevelUpExtensions(level);
    }



    /// <summary>
    /// Adds all the LevelExtensions from the given LevelExtension-RootObject<br></br>
    /// Only direct childs of this Object are valid, and they must be names like an int
    /// </summary>
    /// <param name="root"></param>
    public void addValidLevelExtensions(Transform root) {
        Transform[] LevelUpExtensionsTransforms = root.GetComponentsInChildren<Transform>(true);
        foreach (Transform LevelUpExtension in LevelUpExtensionsTransforms) {

            if (LevelUpExtension.parent == root) { 
                // Transform is a direct child of root-Object
                levelUntilUnlockExtension = int.Parse(LevelUpExtension.name);
                if (!LevelUpExtensions.ContainsKey(levelUntilUnlockExtension)) {
                    LevelUpExtensions.Add(levelUntilUnlockExtension, new List<Transform>{ LevelUpExtension } );
                } else {
                    LevelUpExtensions[levelUntilUnlockExtension].Add(LevelUpExtension);
                }

            }
        }
    }

    /// <summary>
    /// Check if there is a LevelUpExtension in this Building for the currently reached Level
    /// </summary>
    public void checkLevelUpExtensions(int reachedLevel) {

        bool settedNextLevelUpExtensionsLevel = false;

        if (LevelUpExtensions.Count > 0) {
            foreach (KeyValuePair<int, List<Transform>> LevelUpExtensionList in LevelUpExtensions) {
                // Go through the Extensions from the List of this Level
                foreach (Transform Extension in LevelUpExtensionList.Value) {
                    // Set the Extension active if Building-Level is high enough
                    if (reachedLevel >= LevelUpExtensionList.Key) {
                        Extension.gameObject.SetActive(true);
                    } else {
                        Extension.gameObject.SetActive(false);
                        // If not setted the Information of the Next LevelUpExtension, we set it on the first encounter when the level is not high enough
                        if (!settedNextLevelUpExtensionsLevel) {
                            nextLevelUpExtensionsLevel = LevelUpExtensionList.Key;
                            settedNextLevelUpExtensionsLevel = true;
                            //Debug.Log("LevelUpExtension: " + this.buildingName + ": on lvl "+ nextLevelUpExtensionsLevel);
                        }
                    }
                    

                    LevelUpExtension comp = Extension.GetComponent<LevelUpExtension>();

                    // If Materials to change
                    if (comp != null) {
                        if (!comp.isAlreadyExecuted()) {
                            if (reachedLevel >= LevelUpExtensionList.Key) {
                                Globals.HelperFunctions.swapMaterials(this.gameObject, comp.materialToChange, comp.materialTarget);
                                comp.setExecuted(true);
                            } else {
                                Globals.HelperFunctions.swapMaterials(this.gameObject, comp.materialTarget, comp.materialToChange);
                            }
                        }
                    }
                }
            }

            if (!settedNextLevelUpExtensionsLevel) {
                nextLevelUpExtensionsLevel = 0;
            }
        }
    }

    public void resetBuilding() {

        // Reset values
        level = 0;
        upgradeCost = new IdleNum(0);
        DelayUnlockTimestampWhenUnlocked = new DateTime();
        gameObject.SetActive(false);
        upgradeSlots = new Property[4];

        // reLoad the Building
        loadBuilding();
        // Set the Sign Label to the current level
        initIntegratedUIElements();
        // Reset Delay Unlock if it is running
        resetDelayUnlockProcess();
    }




    // For Tests:
    public Transform getDelayUnlockProcessIndicator() {
        return DelayUnlockProcessIndicator;
    }
    public Transform getPercentBar() {
        return percentBar;
    }
    public Transform getTimeLeftLabel() {
        return TimeLeftLabel;
    }

    public Building getLastBuildingInBuildingProgress() {
        return LastBuilding;
    }

    public ConstructionPhases getCurrentConstructionPhase() {
        return currentConstructionPhase;
    }

    public void setDelayUnlockProcessDelay(int newDelay) {
        delayUnlockProcessSeconds = newDelay;
    }

    public int getDelayUnlockProcessSeconds() {
        return delayUnlockProcessSeconds;
    }

}

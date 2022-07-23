using Bayat.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// TODO Description
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class World : MonoBehaviour{


    #region Vars
    /// <summary>
    /// Name of the World
    /// </summary>
    public string worldName;

    /// <summary>
    /// Says if World is the currentWorld loaded by User
    /// </summary>
    [JsonProperty(PropertyName = "isCurrentWorld")]
    public bool isCurrentWorld;

    /// <summary>
    /// The Coins in World (Start at 10 so you can buy the first Building)
    /// </summary>
    [JsonProperty(PropertyName = "Coins")]
    private IdleNum coins = new IdleNum(10);

    /// <summary>
    /// The global enviGlass
    /// </summary>
    public EnviGlass enviGlass;

    /// <summary>
    /// The global BankAccount Income-Manager
    /// </summary>
    [JsonProperty(PropertyName = "CoinIncomeManager")]
    public CoinIncome CoinIncomeManager;

    /// <summary>
    /// The Quests-Component of the Worlds
    /// </summary>
    [JsonProperty(PropertyName = "Quests")]
    public Quests QuestsComponent;

    /// <summary>
    /// The default Material for the LevelUp Indicators
    /// </summary>
    public Material levelUpIndicator_transparent;

    /// <summary>
    /// A Material for the LevelUp Indicators
    /// Building is ready for LevelUp
    /// </summary>
    public Material levelUpIndicator_updateable;

    /// <summary>
    /// A Material for the LevelUp Indicators
    /// Building is ready for Upgrade
    /// </summary>
    public Material levelUpIndicator_upgradeable;

    /// <summary>
    /// All Buildings of World ordered by their BaseCost
    /// </summary>
    [JsonProperty(PropertyName = "buildingsProgressArray")]
    public Building[] buildingsProgressArray;

    /// <summary>
    /// Building in DelayUnlock-Process, is null if no Building is in the Process 
    /// </summary>
    public Building buildingInDelayUnlock;

    /// <summary>
    /// AchievementDictionary with Achievements and Events from Level 0->1
    /// </summary>
    public Dictionary<string, Dictionary<string, string>> achievementsAndEventsLevel1;


    public UnityEvent onLevelUpSlider1;
    public UnityEvent onLevelUpSlider10;
    public UnityEvent onLevelUpSliderMax;

    public Monument monument;
    public Transform monumentSlot;

    #endregion



    /// <summary>
    /// Get the current Amount of Coins in the World BankAccount
    /// </summary>
    public IdleNum getCoins() {
        return coins;
    }

    /// <summary>
    /// Sets coins to the Global coins BankAccount
    /// </summary>
    /// <param name="amount">Amount to add</param>
    public void setCoins(IdleNum amount) {
        coins = amount;
    }

    /// <summary>
    /// Add coins to the Global coins BankAccount
    /// </summary>
    /// <param name="amount">Amount to add</param>
    public void addCoins(IdleNum amount) {
        coins += amount;
    }

    /// <summary>
    /// Remove coins from the Global coins BankAccount
    /// </summary>
    /// <param name="amount">Amount to remove</param>
    public void removeCoins(IdleNum amount) {
        coins = coins - amount;
    }


    /// <summary>
    /// Update the LevelIndicators, but only if its not already upgradable (Performance)
    /// </summary>
    public void checkLevelIndicators() {
        foreach (Building building in buildingsProgressArray) {
            // if the Building is already marked as levelUppable, we dont have to check it regulary
            // because the Coin Account is regulary only raising
            // The only time we have to force all Buildings to check, is when the User is buying something or changing the LevelUpSlider (see TapGameObject.cs)
            if (!building.isLevelUppable()) {
                building.setUpgradeCostNext((int)Globals.UICanvas.uiElements.LevelUpAmountSlider.value);
                building.setLevelUpIndicator();
            }
        }
    }

    /// <summary>
    /// Sets the next Upgradecosts for all se Buildings and
    /// Check and set Levelindicator for each building
    /// </summary>
    public void checkLevelUpIndicatorsForced() {

        switch (Globals.UICanvas.uiElements.LevelUpAmountSlider.value) {

            case 0:
            default:
                onLevelUpSliderMax.Invoke();
                break;

            case 1:
                onLevelUpSlider1.Invoke();
                break;

            case 2:
                onLevelUpSlider10.Invoke();
                break;
        }

        foreach (Building building in buildingsProgressArray) {
            building.setUpgradeCostNext((int)Globals.UICanvas.uiElements.LevelUpAmountSlider.value);
            building.setLevelUpIndicator();
        }
    }

    /// <summary>
    /// Sorts an Array of Buildings by their BaseCost
    /// </summary>
    public static Building[] SortBuildingsByBaseCost(Building[] buildingsToSort) {
        Building[] buildingArray = new Building[buildingsToSort.Length];
        Building TempBuilding = null;
        bool buildingIsSorted = false;

        // Go over the buildings to sort
        foreach (Building toSortBuilding in buildingsToSort) {
            // Set the Building to not sorted
            buildingIsSorted = false;
            for (int i = 0; i < buildingArray.Length; i++) {
                if (buildingArray[i] == null) {
                    if (!buildingIsSorted) {
                        // If the toSortBuilding is on the end of the Array, append it
                        buildingArray[i] = toSortBuilding;
                    } else {
                        // If the toSortBuilding was sorted in, there is the last TempBuilding to append at the end of the array
                        buildingArray[i] = TempBuilding;
                    }
                    break;
                } else {

                    Building alreadySortedBuilding = buildingArray[i];

                    if (!buildingIsSorted && toSortBuilding.baseCost <= alreadySortedBuilding.baseCost) {
                        // If the Building is not already sorted in and --the baseCost are minor than the currently checked Building
                        // set the toSortBuilding to this position
                        buildingArray[i] = toSortBuilding;
                        buildingIsSorted = true;
                    } else if (buildingIsSorted) {
                        buildingArray[i] = TempBuilding;
                    }
                    // Save the old Building on this Position
                    TempBuilding = alreadySortedBuilding;
                }
            }
        }

        return buildingArray;
    }

    /// <summary>
    /// Check the Building wich is the Next Building in Progress
    /// </summary>
    public void checkNextBuildingProgress() {
        // Inits
        Building LastBuilding = null;
        bool nextOneFound = false;

        foreach (Building checkBuilding in buildingsProgressArray) {
            // Setting the Building as the Last Building in BuildingProgress to checkBuilding
            if (LastBuilding != null) {
                checkBuilding.setLastBuilding(LastBuilding);
            }

            // Check if the Level is high enough
            if (!nextOneFound && checkBuilding.getLevel() == 0) {

                // Check if Building is in DelayUnlock-Process
                if (checkBuilding.getDelayUnlockProcessTimestampUntilUnlock() != new DateTime() && !checkBuilding.getIsCheckingDelayUnlockProcess()) {
                    // Invoke the Checkfunction
                    checkBuilding.initDelayUnlockProcess();
                    Debug.Log("Globals.cs: " + checkBuilding.getName() + " is currently in DelayUnlock-Process - Start Checking.");
                }

                checkBuilding.setIsNextInBuildingProgress(true);
                nextOneFound = true;
                Debug.Log("Globals.cs: " + checkBuilding.getName() + " is next in the Building Progress");
                if (LastBuilding != null) {
                    Debug.Log("Globals.cs: " + checkBuilding.getName() + " needs " + LastBuilding.getName() + " to be lvl " + LastBuilding.getLevelsToUnlockNextBuilding() + ".");
                }
            } else {
                checkBuilding.setIsNextInBuildingProgress(false);
            }

            LastBuilding = checkBuilding;
        }
    }

    public void initAchievementsBuildings() {

        // Dictionary Achievements
        achievementsAndEventsLevel1 = new Dictionary<string, Dictionary<string, string>>();
        achievementsAndEventsLevel1.Add("House", new Dictionary<string, string>{
            {"achievement", GPGSIds.achievement_our_house_in_the_middle_of_our_street}
        });
        achievementsAndEventsLevel1.Add("FieldWheat", new Dictionary<string, string>{
            {"achievement", ""}
        });
        achievementsAndEventsLevel1.Add("Windturbine", new Dictionary<string, string>{
            {"achievement", GPGSIds.achievement_helping_nature}
        });
        achievementsAndEventsLevel1.Add("Windmill", new Dictionary<string, string>{
            {"achievement", GPGSIds.achievement_environmental_friendly_production}
        });
        achievementsAndEventsLevel1.Add("Bakery", new Dictionary<string, string>{
            {"achievement", ""}
        });
        achievementsAndEventsLevel1.Add("FieldAnimals", new Dictionary<string, string>{
            {"achievement", ""}
        });
        achievementsAndEventsLevel1.Add("Butcher", new Dictionary<string, string>{
            {"achievement", ""}
        });
        achievementsAndEventsLevel1.Add("Beekeeper", new Dictionary<string, string>{
            {"achievement", GPGSIds.achievement_save_the_bees}
        });
        achievementsAndEventsLevel1.Add("Sawmill", new Dictionary<string, string>{
            {"achievement", ""}
        });
        achievementsAndEventsLevel1.Add("Carpenter", new Dictionary<string, string>{
            {"achievement", ""}
        });
        achievementsAndEventsLevel1.Add("Forestry", new Dictionary<string, string>{
            {"achievement", GPGSIds.achievement_into_the_woods}
        });
    }


    
}

using Bayat.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Prestige System - resetting the BuildingProgress for Income Bonus
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class Prestige : MonoBehaviour
{
    /// <summary>
    /// Prestige Bonus in percent
    /// </summary>
    [JsonProperty(PropertyName = "prestigeBonus")]
    public double prestigeBonus = 1.0f;

    /// <summary>
    /// Level we can use prestige on
    /// </summary>
    [JsonProperty(PropertyName = "prestigeLevel")]
    private int prestigeLevel = 0;

    /// <summary>
    /// Level we reached in the current prestige run
    /// </summary>
    [JsonProperty(PropertyName = "levelInCurrentRun")]
    public int levelInCurrentRun = 0;

    /// <summary>
    /// Bonus to add after user prestiged
    /// </summary>
    [JsonProperty(PropertyName = "possiblePrestigeBonus")]
    private double possiblePrestigeBonus = 0.0f;

    /// <summary>
    /// Level the user has already prestige in previous runs
    /// </summary>
    [JsonProperty(PropertyName = "alreadyPrestigedLevel")]
    private int alreadyPrestigedLevel = 0;

    /// <summary>
    /// The Construction time for the buildings i had reached in a prior run <br></br>
    /// so i dont have to wait again for this building
    /// </summary>
    public int prestigeConstructionTime = 5;


    /// <summary>
    /// Min Bonus in percent, when user can use prestige
    /// </summary>
    public double minPrestigeBonus = 8.0f;

    /// <summary>
    /// The minimum progress i need to reach to unlock prestige (7=Beekeeper currently)
    /// </summary>
    public int minPrestigeProgress = 7;

    /// <summary>
    /// (minPrestigeBonus / bonusPerLevelFactor) -> berechnet die Anzahl der Level die benötigt werden, 
    /// um den Mindestbonus zu erreichen
    /// </summary>
    private double minLevelforPrestige;

    /// <summary>
    /// Prestige Reset Button
    /// </summary>
    public Button prestigeResetButton;
    public TMPro.TextMeshProUGUI prestigeLabelBonusNow;
    public TMPro.TextMeshProUGUI prestigeLabelBonusNext;
    public GameObject prestigeHint;

    public const float bonusPerLevelFactor = 0.02f;


    public void Awake() {
        calcMinLevelForPrestige();
    }

    /// <summary>
    /// (minPrestigeBonus / bonusPerLevelFactor) -> berechnet die Anzahl der Level die benötigt werden, 
    /// um den Mindestbonus zu erreichen
    /// </summary>
    public void calcMinLevelForPrestige() {
        minLevelforPrestige = (minPrestigeBonus / bonusPerLevelFactor);
    }


    public void checkButtonState() {
        // Check Prestige Button State
        if (prestigeBonus != 1.0f || Globals.Game.currentUser.worldProgressIndex >= minPrestigeProgress) {
            showPrestigeButton();
        } else {
            //showPrestigeButton();
            hidePrestigeButton();
        }
    }

    /// <summary>
    /// Show Prestige Button
    /// </summary>
    private void showPrestigeButton() {
        Globals.UICanvas.uiElements.ButtonPrestige.SetActive(true);
    }

    /// <summary>
    /// Hide Prestige Button
    /// </summary>
    private void hidePrestigeButton() {
        Globals.UICanvas.uiElements.ButtonPrestige.SetActive(false);
    }



    /// <summary>
    /// Get the prestige from the PlayerPrefs
    /// </summary>
    public void loadPrestige() {
        updatePrestigePopUp();
    }


    private void updatePrestigeBonus() {
        alreadyPrestigedLevel += prestigeLevel;
        prestigeBonus = possiblePrestigeBonus;

        prestigeLevel = 0;
        possiblePrestigeBonus = 0.0f;
        levelInCurrentRun = 0;
    }

    /// <summary>
    /// Update the number of potential prestige levels
    /// </summary>
    public void updatePrestigeLevels() {
        //if (levelInCurrentRun > alreadyPrestigedLevel) {
        //    prestigeLevel = levelInCurrentRun - alreadyPrestigedLevel; ;
        //    calcPossiblePrestigeBonus();     
        //} else {
        //    prestigeResetButton.interactable = false;
        //    prestigeHint.GetComponent<TMPro.TextMeshProUGUI>().text =
        //        Globals.Wrapper.LanguageWrapper.translateString(
        //            "prestige_keep_playing",
        //            new string[] { getPossiblePrestigeBonus_Rounded(), getMinPrestigeBonusNext_Rounded() }
        //            );
        //    prestigeHint.SetActive(true);
        //}

        //// Check if Button needs to be shown
        //checkButtonState();
    }

    private void calcPossiblePrestigeBonus() {  
        
        if(minLevelforPrestige == 0) {
            calcMinLevelForPrestige();
        }

        // die momentan erreichten Level im Verhältnis zu den Anzahl der Mindestlevel
        double levelfactor = levelInCurrentRun / minLevelforPrestige;
        // Mindest Prestige Bonus hoch den levelfactor
        double bonus = Math.Pow(minPrestigeBonus, levelfactor);

        possiblePrestigeBonus = (float)bonus;
        updatePrestigePopUp();
    }

    public void updatePrestigePopUp() {

        // Prestige Now      
        prestigeLabelBonusNow.text = "x " + getPrestigeBonus_Rounded();

        // Set Button interactability
        if (possiblePrestigeBonus < (getMinPrestigeBonusNext())) {
            prestigeResetButton.interactable = false;
            prestigeLabelBonusNext.text = "x " + getMinPrestigeBonusNext_Rounded();
            prestigeHint.GetComponent<TMPro.TextMeshProUGUI>().text =
                Globals.Controller.Language.translateString(
                    "prestige_keep_playing",
                    new string[] { getPossiblePrestigeBonus_Rounded(), getMinPrestigeBonusNext_Rounded() }
                    );
            prestigeHint.SetActive(true);
        } else {
            prestigeResetButton.interactable = true;
            prestigeLabelBonusNext.text = "x " + getPossiblePrestigeBonus_Rounded();
            prestigeHint.SetActive(false);
        }
    }


    /// <summary>
    /// Reset Game with Prestige Bonus Updated
    /// </summary>
    public void prestigeResetGame() { 

        //if(possiblePrestigeBonus >= minPrestigeBonus) {
        //    updatePrestigeBonus();

        //    usualResetGame();

        //    // Set Button not interactable
        //    prestigeResetButton.interactable = false;

        //    // Close PopUp
        //    Globals.UICanvas.uiElements.PopUpPrestige.SetActive(false);
        //    Globals.UICanvas.uiElements.PopUpBG.SetActive(false);
        //    updatePrestigePopUp();
        //}

    }

    /// <summary>
    /// Resets the Game
    /// </summary>
    public void usualResetGame() {
        // Reset Coins
        prestigeResetCoins();

        // Reset Envi
        prestigeResetEnvi();

        // Reset Worlds and Buildings
        prestigeResetBuildings();

        // Check BuildingProgess
        Globals.Game.currentWorld.checkNextBuildingProgress();
    }

    private void prestigeResetBuildings() {

        // Reset Buildings in current Worlds
        foreach (Building building in Globals.Game.currentWorld.buildingsProgressArray) {
            building.resetBuilding();
        }

    }

    ///// <summary>
    ///// Set Construction time of all already built Buildings to <see cref="prestigeConstructionTime"/>
    ///// </summary>
    //public void overWriteConstructionTime() {
    //    for (int i = 0; i < Globals.Game.WorldAcrossBuildingOrder.Count; i++) {
    //        Building building = Globals.Game.WorldAcrossBuildingOrder[i];
    //        if (building.wasConstructed) {
    //            building.setDelayUnlockProcessDelay(prestigeConstructionTime);
    //        }
    //    }
    //}

    private void prestigeResetCoins() {
        // Reset Coins in current Worlds
        Globals.Game.currentWorld.setCoins(new IdleNum(10, ""));
        // Update Label
        Globals.UICanvas.uiElements.HUDCoinDisplay.text = Globals.Game.currentWorld.getCoins().toRoundedString();

        // Reset Coin Affectors in current Worlds
        Globals.Game.currentWorld.CoinIncomeManager.reset();
    }

    private void prestigeResetEnvi() {
        // Reset Envi Glasses in current Worlds
        Globals.Game.currentWorld.enviGlass = new EnviGlass();
    }


    /// <summary>
    /// Reset all relevant Presige Values to Defaults
    /// </summary>
    public void ResetPrestigeValuesForAdmins() {
        prestigeBonus = 1.0f;
        prestigeLevel = 0;
        possiblePrestigeBonus = 0;
        alreadyPrestigedLevel = 0;
        levelInCurrentRun = 0;
    }










    /* GETTER */
    public int getPrestigeLevel() {
        return prestigeLevel;
    }

    public double getPossiblePrestigeBonus() {
        return possiblePrestigeBonus;
    }

    public int getAlreadyPrestigedLevel() {
        return alreadyPrestigedLevel;
    }

    public double getMinPrestigeBonusNext() {
        return minPrestigeBonus * prestigeBonus;
    }

    public string getMinPrestigeBonusNext_Rounded() {
        IdleNum num = new IdleNum(getMinPrestigeBonusNext());
        num = num * 1;
        return num.toRoundedString();
    }

    public string getPrestigeBonus_Rounded() {
        IdleNum num = new IdleNum(prestigeBonus);
        num = num * 1;
        return num.toRoundedString();
    }

    public string getPossiblePrestigeBonus_Rounded() {
        IdleNum num = new IdleNum(possiblePrestigeBonus);
        num = num * 1;
        return num.toRoundedString();
    }
}

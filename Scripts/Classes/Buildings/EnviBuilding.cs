using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bayat.Json;

/// <summary>
/// Building that only affects the Environment
/// </summary>
public class EnviBuilding : Building, IEnvironmentable  {

    

    /// <summary>
    /// The Base Factor how strong the Environment is affected by this Building (DONT MODIFY!)
    /// </summary>
    public float environmentFactorOR = 3.23f;

    /// <summary>
    /// The current EnvironmentFactor of the Building (includes BuildingUpgrades)
    /// </summary>
    private float currentEnvironmentFactor = 3.23f;

    /// <summary>
    /// The current EnvironmentFactor of the holded Items
    /// </summary>
    private float currentItemEnvironmentFactor = 1;

    /// <summary>
    /// represents the Affector of the Building
    /// </summary>
    protected Affector<float> buildingsEnviAffector;


    // Performance Optimization
    float enviNext;
    int newLevelUpAmount;
    string enviPlus;


    /// <summary>
    /// Loads the Values of the Building <br></br>
    /// Values: level, upgradeCost, upgradeCost10, environmentAffection
    /// </summary>
    public override void loadBuilding() {
        base.loadBuilding();
        influenceEnvironment();
        UpdateCurrentEnvironmentFactor();
    }

    /// <summary>
    /// Levels up the Environment Building for a specific amount
    /// </summary>
    /// <param name="amount"></param>
    public override bool levelUp(int amount, bool prepayed = false) {
        // Use Parent levelUp Function
        if (base.levelUp(amount, prepayed)) {

            // Update EnviGlass
            Globals.Game.currentWorld.enviGlass.updateSpecificAffector(buildingsEnviAffector.getID(), currentEnvironmentFactor * level);

            updateLabel();

            return true;
        } else {
            return false;
        }

    }

    /// <summary>
    /// Updates the currentEnvironmentFactor and calculates it together with the BuildingUpgrades
    /// </summary>
    public void UpdateCurrentEnvironmentFactor() {

        // Check out the Item Affection
        currentItemEnvironmentFactor = 1;
        for (int i = 0; i < getUpgradeSlots().Length; i++) {
            if (!checkUpgradeSlotFree(i)) {
                currentItemEnvironmentFactor += ((BuildingUpgrade)upgradeSlots[i].ReferencedItem).factorEnvironmentAffection - 1;
            }
        }

        // Add the Envi Factors of the Items
        currentEnvironmentFactor = environmentFactorOR * currentItemEnvironmentFactor;

        Globals.Game.currentWorld.enviGlass.updateSpecificAffector(buildingsEnviAffector.getID(), currentEnvironmentFactor * level);
    }

    /// <summary>
    /// Returns the current EnvironmentFactor
    /// </summary>
    /// <returns></returns>
    public float getCurrentEnvironmentFactor() {
        return currentEnvironmentFactor;
    }

    /// <summary>
    /// Returns the current Item-EnvironmentFactor
    /// </summary>
    /// <returns></returns>
    public float getCurrentItemEnvironmentFactor() {
        return currentItemEnvironmentFactor;
    }


    protected override void setUpgradeSlot(int slot, Property upgr) {
        base.setUpgradeSlot(slot, upgr);

        // Update EnviGlass
        UpdateCurrentEnvironmentFactor();
    }

    /// <summary>
    /// Override the general Information function. This prints Information for the Labels
    /// </summary>
    /// <returns>String Containing the Information</returns>
    public override Dictionary<string, string> getInfo(int levelUpAmount) {
        stringDict = base.getInfo(levelUpAmount);
        stringDict.Add("Envi", buildingsEnviAffector.getAffection().ToString());

        enviPlus = "";

        newLevelUpAmount = int.Parse(stringDict["LevelUpAmount"]); // Because levelUpAmount can change at base.getInfo(levelUpAmount)

        if (newLevelUpAmount != 0) {
            // Calculate the next environmental Cost/Buff
            enviNext = currentEnvironmentFactor * newLevelUpAmount;
            if (enviNext > 0) {
                // If next Envi Value is positive we prepend a + on the PopUp
                enviPlus = "+";
            }
            stringDict.Add("EnviNext", System.Math.Round(enviNext, 2).ToString());
        } else {
            stringDict.Add("EnviNext", "0");
        }

        stringDict.Add("EnviPlus", enviPlus);

        // Item Affection (in percent)
        stringDict.Add("currentItemEnvironmentFactor", ((1 - getCurrentItemEnvironmentFactor()) * 100).ToString("N0").ToString());
        


        return stringDict;
    }  

    /// <summary>
    /// Add EnviAffectors to the Building
    /// Should be called in Awake Function
    /// </summary>
    public void influenceEnvironment() {
        buildingsEnviAffector = new Affector<float>(worldName + "_" + buildingName, currentEnvironmentFactor * level);

        Globals.Game.currentWorld.enviGlass.addAffector(buildingsEnviAffector);
    }
  
}


using UnityEngine;
using System;
using System.Collections.Generic;
using System.Timers;
using System.Collections;
using Random = System.Random;
using Bayat.Json;

/// <summary>
/// Coins per Second and a List of Affectors
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class CoinIncome: MonoBehaviour {

    /// <summary>
    /// Shows the total income / s
    /// </summary>
    [JsonProperty(PropertyName = "totalIncome")]
    public IdleNum totalIncome = new IdleNum(0);

    /// <summary>
    /// List of Affectors that generate Coins
    /// </summary>
    public static List<Affector<IdleNum>> affectors = new List<Affector<IdleNum>>();

    /// <summary>
    /// How often the Coins/s should be added to the total Coins in miliseconds (wont change the amount of coins added /s)
    /// </summary>
    [Tooltip("How often the Coins/s should be added to the total Coins in miliseconds (wont change the amount of coins added /s). NEEDS RESTART!")]
    public int incomeInterval = 100;

    private float incomeWaitInterval;

    /// <summary>
    /// Identifier
    /// </summary>
    private int coinIncomeID = 0;

    // Performance Optimization
    IdleNum incomeBuildings = new IdleNum(0);

    /// <summary>
    /// Boost Timer for normal Boost
    /// </summary>
    [JsonProperty(PropertyName = "adBoostTimer")]
    public BoostTimer adBoostTimer;

    /// <summary>
    /// Boost Timer for normal Boost
    /// </summary>
    [JsonProperty(PropertyName = "itemBoostTimer")]
    public BoostShopTimer itemBoostTimer;


    /// <summary>
    /// Inits CoinIncome to manage the IdleNum Incomes
    /// </summary>
    public void InitCoinIncome() {
        totalIncome = new IdleNum(0);
        incomeWaitInterval = (float)incomeInterval/ (float)1000;
        Random my_rnd = new Random();
        coinIncomeID = my_rnd.Next(0, 1000000);
    }

    /// <summary>
    /// Starts the Producing of Income in this CoinTimer
    /// </summary>
    public void startProduceIncome() {
        if (!produceIncomeIsRunning()) {
            InvokeRepeating("produceIncome", 0, incomeWaitInterval);
        }
    }

    /// <summary>
    /// Stops the Producing of Income in this CoinTimer
    /// </summary>
    public void stopProduceIncome() {
        if (produceIncomeIsRunning()) {
            CancelInvoke("produceIncome");
        }
    }

    /// <summary>
    /// Returns wether the producing of Income is Running
    /// </summary>
    /// <returns></returns>
    public bool produceIncomeIsRunning() {
        return IsInvoking("produceIncome");
    }

    /// <summary>
    /// Add the Income to the global Coins
    /// </summary>
    public void produceIncome() {
        try {
            Globals.Game.currentWorld.addCoins(totalIncome * (incomeInterval / 1000.0f));
            //Debug.Log(coinIncomeID + " Added " + (totalIncome * (incomeInterval / 1000.0f)).toRoundedString(2, true) + " Coins");
            Globals.UICanvas.uiElements.HUDCoinDisplay.text = Globals.Game.currentWorld.getCoins().toRoundedString();
        } catch (Exception e) {
            Globals.UICanvas.DebugLabelAddText(e, true);
        }
    }



    /// <summary>
    /// Add a new Affector to Coins/s
    /// Checks wether the Affector is already in the List
    /// ALSO UPDATES THE totalIncome !
    /// </summary>
    /// <param name="NewAffector"></param>
    public void addAffector(Affector<IdleNum> NewAffector) {
        if (!isAlreadyAffector(NewAffector)) {
            affectors.Add(NewAffector);
        }
        updateTotalIncome();
    }

    /// <summary>
    /// Updates the AffectionValue of a specified Affector<br></br>
    /// and updates the CoinIncome of the Glass
    /// </summary>
    /// <param name="affectorID"></param>
    /// <param name="newAffectionValue"></param>
    public void updateSpecificAffector(string affectorID, IdleNum newAffectionValue) {
        foreach (Affector<IdleNum> Affector in affectors) {
            if (Affector.getID() == affectorID) {
                Affector.setAffection(newAffectionValue);
            }
        }
        updateTotalIncome();
    }


    /// <summary>
    /// Updates the totalIncome with all its Affectors
    /// </summary>
    public void updateTotalIncome() {
        // Add Up the Buildings
        incomeBuildings = new IdleNum(0);
        foreach (Affector<IdleNum> Affector in affectors) {
            incomeBuildings += Affector.getAffection();
        }

        // Backup
        //if (Globals.Game.currentUser.prestige.prestigeBonus < 1) {
        //    Globals.Game.currentUser.prestige.prestigeBonus = 1;
        //}       


        // Add the Boosts
        try {
            totalIncome = incomeBuildings * adBoostTimer.getBoost() * itemBoostTimer.getBoost();// * Globals.Game.currentUser.prestige.prestigeBonus;
        } catch (Exception e) {
            Globals.UICanvas.DebugLabelAddText(e, true);
        }
    }

    /// <summary>
    /// Gets the incomeMultiplier from the Boost-Function
    /// </summary>
    /// <param name="multiplier"></param>
    public IdleNum getIncomeBuildings() {
        return incomeBuildings;
    }

    
    



    /// <summary>
    /// Resets the CoinIncome to 0
    /// ALSO UPDATES THE totalIncome !
    /// </summary>
    /// <param name="NewAffector"></param>
    public void reset() {
        affectors.Clear();
        updateTotalIncome();
    }

    /// <summary>
    /// Returns the current total income
    /// </summary>
    public IdleNum getTotalIncome() {
        // Prestige Bonus is essential, so we add it to the Basic income (so it affects OfflineCoins, MiniGames etc)
        return totalIncome;
    }

    /// <summary>
    /// Gets the Base Income -> all Incomes of Building + current Prestige Bonus<br></br>
    /// without timed Boosts!
    /// </summary>
    /// <returns></returns>
    public IdleNum getBaseIncome() {
        return incomeBuildings;// * Globals.Game.currentUser.prestige.prestigeBonus;
    }

    /// <summary>
    /// Searches the AffectorList and returns wether the Affector is already in the List
    /// </summary>
    /// <param name="Affector"></param>
    /// <returns></returns>
    private bool isAlreadyAffector(Affector<IdleNum> Affector) {
        foreach (Affector<IdleNum> ListAffector in affectors) {
            if (ListAffector.getID() == Affector.getID()) {
                return true;
            }
        }
        return false;
    }
}

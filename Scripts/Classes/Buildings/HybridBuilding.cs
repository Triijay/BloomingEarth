using Bayat.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class HybridBuilding : EnviBuilding, ICoinable {

    /// <summary>
    /// The base Income of this Building.
    /// </summary>
    [Tooltip("The base Income of this Building")]
    public IdleNum baseIncome = new IdleNum(1);

    /// <summary>
    /// The time after the collectable Coin respawns in Seconds
    /// </summary>
    public int coinRespawnTime = 1;

    /// <summary>
    /// The current Income Factor (without Level) of the Building (includes BuildingUpgrades)
    /// </summary>
    private IdleNum currentIncomeFactor = new IdleNum(1);

    /// <summary>
    /// The current Item Income Factor of the Building 
    /// </summary>
    private float currentItemIncomeFactor = 1;

    /// <summary>
    /// The current Income of the Building (includes BuildingUpgrades)
    /// </summary>
    private IdleNum currentIncome = new IdleNum(1);

    /// <summary>
    /// The current Income of this Building.
    /// </summary>
    private Affector<IdleNum> buildingsCoinAffector;

    /// <summary>
    /// The tappable Coin to collect extra coins
    /// </summary>
    private Transform TapCoin;

    // Calculate the Game Off Time
    private TimeSpan TimeDifferenceGameOff;


    // UnityEvents
    public UnityEvent redeemedTapCoin;


    /// <summary>
    /// Loads the Values of the Building <br></br>
    /// Values: level, upgradeCost, upgradeCost10, environmentAffection
    /// </summary>
    public override void loadBuilding() {
        
        base.loadBuilding();

        initProduceCoins();
    
    }


    /// <summary>
    /// Get the Income Var
    /// </summary>
    /// <returns>income</returns>
    public IdleNum getIncome() {
        return buildingsCoinAffector.getAffection();
    }

    protected override void setUpgradeSlot(int slot, Property upgr) {
        base.setUpgradeSlot(slot, upgr);

        // Update Income
        UpdateCurrentIncome();
    }

    /// <summary>
    /// Initiaties the coin-producing and sets the coin-Affector
    /// </summary>
    public void initProduceCoins() {

        // Set new CoinAffector
        buildingsCoinAffector = new Affector<IdleNum>(worldName + "_" + buildingName + "_coin", currentIncomeFactor * level);
        //Debug.Log("LoadBuilding: CoinAffector " + worldName + "_" + buildingName + "(lvl" + level + "): Setted producing/s " + (currentIncomeFactor * level).toRoundedString() );

        // Init the Affector in CoinIncome
        Globals.Game.currentWorld.CoinIncomeManager.addAffector(buildingsCoinAffector);

        // Update CurrentIncome
        UpdateCurrentIncome();

        // Sets the Buildings Affection on CoinIncome
        buildingsCoinAffector.setAffection(currentIncome);
    }


    public override void initIntegratedUIElements() {
        base.initIntegratedUIElements();

        TapCoin = gameObject.transform.Find("TapCoin");

        // Calculate the Game Off Time
        TimeDifferenceGameOff = Globals.HelperFunctions.getGameOfflineTime(DateTime.Now);

        if (TimeDifferenceGameOff.TotalSeconds > coinRespawnTime) {
            // GameStart: If Long enough offline
            spawnCoin();
        } else {
            // Trigger the Coroutine as usual
            if (level > 0) {
                StartCoroutine(WaitForCoinRespawn());
            }
        }
    }


    public override bool levelUp(int amount, bool prepayed = false) {
        // base updated EnviGlass already
        if (base.levelUp(amount, prepayed)) {

            // Calculate new Income
            UpdateCurrentIncome();

            updateLabel();

            return true;
        } else {
            return false;
        }

    }


    /// <summary>
    /// Updates the income and calculates it together with the BuildingUpgrades
    /// </summary>
    public void UpdateCurrentIncome() {

        // Calculate Item Factor
        currentItemIncomeFactor = 1;
        for (int i = 0; i < getUpgradeSlots().Length; i++) {
            if (!checkUpgradeSlotFree(i)) {
                currentItemIncomeFactor += ((BuildingUpgrade)upgradeSlots[i].ReferencedItem).factorCoinProducing - 1;
            }
        }

        // Add the Income Factors of the Items
        currentIncomeFactor = baseIncome * currentItemIncomeFactor;

        // Multiply it with the Level
        currentIncome = currentIncomeFactor * level;
        Globals.Game.currentWorld.CoinIncomeManager.updateSpecificAffector(buildingsCoinAffector.getID(), currentIncome);
    }

    public override Dictionary<string, string> getInfo(int levelUpAmount) {
        stringDict = base.getInfo(levelUpAmount);
        stringDict.Add("Income", getIncome().toRoundedString());

        int newLevelUpAmount = int.Parse(stringDict["LevelUpAmount"]); // Because levelUpAmount can change at base.getInfo(levelUpAmount)
        if (newLevelUpAmount != 0) {
            IdleNum incomeNext = currentIncomeFactor * int.Parse(stringDict["LevelUpAmount"]);
            // Calculate the IncomeAddition on the disired Level
            stringDict.Add("IncomeNext", incomeNext.toRoundedString());
        } else {
            stringDict.Add("IncomeNext", "0");
        }

        // Add Coin Spawn Time
        stringDict.Add("CoinRespawnTime", coinRespawnTime.ToString());
        stringDict.Add("CoinTapCash", getSpawnedCoinAmount().toRoundedString());
        if (newLevelUpAmount != 0) {
            stringDict.Add("CoinTapCashNext", getSpawnedCoinAmount(level + int.Parse(stringDict["LevelUpAmount"])).toRoundedString());
        } else {
            stringDict.Add("CoinTapCashNext", "0");
        }

        // Item Affection (in percent)
        stringDict.Add("currentItemIncomeFactor", ((getCurrentItemIncomeFactor() - 1) * 100).ToString("N0").ToString());

        return stringDict;
    }

    protected override void checkDelayUnlockProcess() {
        base.checkDelayUnlockProcess();

        if (level > 0) {
            StartCoroutine(WaitForCoinRespawn());
        }
    }

    public IdleNum getSpawnedCoinAmount(int alevel) {
        return currentIncomeFactor * alevel * coinRespawnTime;
    }

    public IdleNum getSpawnedCoinAmount() {
        return currentIncomeFactor * level * coinRespawnTime;
    }

    private IEnumerator WaitForCoinRespawn() {

        yield return new WaitForSeconds(coinRespawnTime);
        spawnCoin();
    }

    /// <summary>
    /// Set the spinning coin to active
    /// </summary>
    public void spawnCoin() {
        if (level > 0) {
            TapCoin.gameObject.SetActive(true);
        }
    }

    public void handleCoinTap() {
        // Add the Coins to the world coins
        Globals.Game.currentWorld.addCoins(getSpawnedCoinAmount());
        // Play Sound
        Globals.Controller.Sound.PlaySound("CoinReward");
        // Disable TapCoin
        TapCoin.gameObject.SetActive(false);
        // Start Coroutine, wait for next Coin Spawn
        StartCoroutine(WaitForCoinRespawn());

        redeemedTapCoin.Invoke();
    }

    /// <summary>
    /// Returns the current Income per Sec from the Building
    /// </summary>
    /// <returns></returns>
    public IdleNum getCurrentIncome() {
        return currentIncome;
    }


    /// <summary>
    /// Returns the affection from the Items to the Income
    /// </summary>
    /// <returns></returns>
    public float getCurrentItemIncomeFactor() {
        return currentItemIncomeFactor;
    }
}


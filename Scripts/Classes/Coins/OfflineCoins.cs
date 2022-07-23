using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class OfflineCoins {
    /// <summary>
    /// inits the essential Functions of the Game like Offline Coins
    /// </summary>
    public static void initOfflineCoins(bool showPopUp, DateTime CurrentTime) {

        try {

            // Set Offline Coins unclaimed
            Globals.HelperFunctions.claimedOfflineCoins = false;

            string coinsBeforePrefs = Globals.Game.currentWorld.getCoins().toRoundedString();

            // Debug Wrong Offline Coins 
            //Globals.UICanvas.DebugLabelAddText("InitGame: \nCoin Amount from Prefs: " + new IdleNum(w1_coins, w1_suffix).toRoundedString() +
            //                    "\nCoins Before Prefs " + coinsBeforePrefs + " coins." +
            //                    "\nCurrentWorld coins now: " + Globals.Game.currentWorld.getCoins().toRoundedString());


            // Calculate the Game Off Time
            TimeSpan TimeDifferenceGameOff = Globals.HelperFunctions.getGameOfflineTime(CurrentTime);


            // TimeDifferenceGameOff => negative
            if (TimeDifferenceGameOff.TotalHours < -1 && TimeDifferenceGameOff.TotalHours >= -24) {
                // User could faked his time or he traveled to another country with other Timezone
                Globals.Controller.GPiOS.IncrementEventOnce(GPGSIds.event_user_offlinetime_negative_low);
                Globals.Controller.Firebase.IncrementFirebaseEventOnce("user_offlinetime_negative_low", "times");
            }
            if (TimeDifferenceGameOff.TotalHours < -24) {
                // User is suspected to cheat
                Globals.Controller.GPiOS.IncrementEventOnce(GPGSIds.event_user_offlinetime_negative_high);
                Globals.Controller.Firebase.IncrementFirebaseEventOnce("user_offlinetime_negative_high", "times");

                // TODO Should we do a Malus for him? - after 2 Times?
            }

            // Now calculate the Game-Off Coins
            IdleNum GameOffCoins = calculateOffGameCoins(TimeDifferenceGameOff);


            // If Minimum offline time not reached, or no IdleCash was made, or the Welcome-Tutorial is Running (what would affect bad PopUp-Stack)
            // dont show PopUp (forced)
            if (TimeDifferenceGameOff.TotalSeconds < Globals.Game.initGame.PopupMinOfflineTime
                || (Globals.Game.currentUser.tutorialIsRunning && Globals.Controller.Tutorial.desiredTutorialID == 0)
                || GameOffCoins == new IdleNum(0)) {
                showPopUp = false;
            }

            // Decide wether to show PopUp
            if (showPopUp) {

                // Calculate double Offline Coins
                IdleNum DoubleGameOffCoins = GameOffCoins * 2;

                // Insert values in strings
                Globals.UICanvas.translatedTMProElements.PopUpOfflineCoinsEarned.text = "<sprite=\"Icon_Coins\" name=\"Icon_Coins\"> " + GameOffCoins.toRoundedString();

                // Offline Time Display
                TimeSpan maximumOfflineTimespan = new TimeSpan(Globals.Game.initGame.limitOfflineCoinsHours, 0, 0);
                if (TimeDifferenceGameOff < maximumOfflineTimespan) {
                    // Offline Time under maximumOfflineTimespan
                    Globals.UICanvas.translatedTMProElements.PopUpOfflineCoinsBeenAwaySince.text =
                        Globals.Controller.Language.translateString("offlinecoins_beenawaysince",
                            new string[] { TimeDifferenceGameOff.ToString(@"dd\d\ hh\h\ mm\m\ ss\s\ ").TrimStart(' ', 'd', 'h', 'm', 's', '0') }
                        );
                } else {
                    // Offline Time over maximumOfflineTimespan
                    Globals.UICanvas.translatedTMProElements.PopUpOfflineCoinsBeenAwaySince.text =
                        Globals.Controller.Language.translateString("offlinecoins_beenawaysince_too_long",
                            new string[] { TimeDifferenceGameOff.ToString(@"dd\d\ hh\h\ mm\m\ ss\s\ ").TrimStart(' ', 'd', 'h', 'm', 's', '0'), Globals.Game.initGame.limitOfflineCoinsHours.ToString() }
                        );
                }

                // Get Buttons
                Button btn_Ok = Globals.UICanvas.uiElements.PopUpOfflineCoinsButtonOK.GetComponent<Button>();
                Button btn_X2 = Globals.UICanvas.uiElements.PopUpOfflineCoinsAdButtonX2.GetComponent<Button>();

                // Show doubled coins
                TMPro.TextMeshProUGUI btnText = btn_X2.transform.Find("doubleCoins").GetComponent<TMPro.TextMeshProUGUI>();
                btnText.text = "<sprite=\"Icon_Coins\" name=\"Icon_Coins\"> " + DoubleGameOffCoins.toRoundedString();

                // RemoveListener so there is only one listener on a button
                btn_Ok.onClick.RemoveAllListeners();
                btn_X2.onClick.RemoveAllListeners();

                // AddListener Method
                btn_Ok.onClick.AddListener(() => Globals.HelperFunctions.popUpButtonListener(GameOffCoins, false));
                btn_X2.onClick.AddListener(() => Globals.HelperFunctions.popUpButtonListener(DoubleGameOffCoins, true));

                // Zoom out of Building if Buildinmenu is open
                if (Globals.UICanvas.uiElements.BuildingMenu.activeSelf) {
                    Globals.UICanvas.uiElements.MainCamera.GetComponent<CameraControls>().moveOutOfBuilding();
                }

                // Show popup
                Globals.UICanvas.uiElements.PopUpOfflineCoins.SetActive(true);


            } else {
                // Add the Coin Income to the Worlds coins
                Globals.Game.currentWorld.addCoins(GameOffCoins);

                // Save Claim Timestamp
                Globals.HelperFunctions.setOfflineCoinsClaimed();

                // Show Feedback PopUp after 10 Gamestarts (if offlineCoins where made, the User would be asked, after he clamed the coins)
                ButtonScripts.checkifToActivateFeedbackPopUp();
            }
        }
        catch (Exception e) {
            Globals.UICanvas.DebugLabelAddText("OfflineCoins--" + e.ToString(), true);
        }


    }


    /// <summary>
    /// Calculate Game-Off Coins
    /// Game-Off Coins are the amount of Coins which would have been produced while the User turned of our game
    /// </summary>
    private static IdleNum calculateOffGameCoins(System.TimeSpan TimeDifference) {

        IdleNum GameOffCoins = new IdleNum(0, "");

        // Get the total Coin income per second
        IdleNum IncomePerSec = Globals.Game.currentWorld.CoinIncomeManager.getBaseIncome();

        // Load Ad Boost
        int secondsOfflineTimeBoost = SavingSystem.loadBoost(TimeDifference,
            Globals.Game.currentWorld.CoinIncomeManager.adBoostTimer
            );

        // Load Item Boost
        int secondsOfflineTimeItemBoost = SavingSystem.loadBoost(TimeDifference,
            Globals.Game.currentWorld.CoinIncomeManager.itemBoostTimer
            );

        if (IncomePerSec > new IdleNum(0)) {

            // Get TimeDifference in Seconds
            float timeDifferenceSeconds = (float)TimeDifference.TotalSeconds;

            // Clamp the GameOffCoins to the Limit
            int limitOfflineCoinsSeconds = Globals.Game.initGame.limitOfflineCoinsHours * 60 * 60;
            timeDifferenceSeconds = Mathf.Clamp(timeDifferenceSeconds, 0, limitOfflineCoinsSeconds);

            // Get the Total Amount of OfflineCoins
            GameOffCoins = IncomePerSec * (int)timeDifferenceSeconds;

            Debug.Log("InitGame: Basic Offline-Coin Income: " + GameOffCoins.toRoundedString() + " coins in " + (int)timeDifferenceSeconds + " Seconds, with a speed of " + IncomePerSec.toRoundedString() + "/s");


            // Now we have to Add the Boosts to the Income
            // We cannot simply use the Globals total Income, beuace the Boosts can end seperately while User is offline

            // Add the Boosted Coin Amounts
            if (secondsOfflineTimeBoost > 0 && Globals.Game.currentWorld.CoinIncomeManager.adBoostTimer.getBoost() > 1) {
                // the Income Per Sec runned on Boost [secondsOfflineTimeBoost]seconds, so we add this
                // eG Double Bonus -> We add IncomePerSec*secondsOfflineTimeBoost*1, because the usual Income was already added to the Offline Coins
                // eG Triple Bonus -> We add IncomePerSec*secondsOfflineTimeBoost*2, because 1 part of the Triple Income was already added
                GameOffCoins += IncomePerSec * secondsOfflineTimeBoost * (Globals.Game.currentWorld.CoinIncomeManager.adBoostTimer.getBoost() - 1);
                Debug.Log("InitGame: Added " + (IncomePerSec * secondsOfflineTimeBoost * (Globals.Game.currentWorld.CoinIncomeManager.adBoostTimer.getBoost() - 1)).toRoundedString() + " coins to Basic Offline-Coin Income" +
                    ", because of " + secondsOfflineTimeBoost + " Seconds of x" + Globals.Game.currentWorld.CoinIncomeManager.adBoostTimer.getBoost() + " boosted Offline Time");
            }

            // Add the item-Boosted Coin Amounts
            if (secondsOfflineTimeItemBoost > 0 && Globals.Game.currentWorld.CoinIncomeManager.itemBoostTimer.getBoost() > 1) {
                // the Income Per Sec runned on Boost [secondsOfflineTimeItemBoost]seconds, so we add this
                // eG Double Bonus -> We add IncomePerSec*secondsOfflineTimeItemBoost*1, because the usual Income was already added to the Offline Coins
                // eG Triple Bonus -> We add IncomePerSec*secondsOfflineTimeItemBoost*2, because 1 part of the Triple Income was already added
                GameOffCoins += IncomePerSec * secondsOfflineTimeItemBoost * (Globals.Game.currentWorld.CoinIncomeManager.itemBoostTimer.getBoost() - 1);
                Debug.Log("InitGame: Added " + (IncomePerSec * secondsOfflineTimeItemBoost * (Globals.Game.currentWorld.CoinIncomeManager.itemBoostTimer.getBoost() - 1)).toRoundedString() + " coins to Basic Offline-Coin Income," +
                    " because of " + secondsOfflineTimeItemBoost + " Seconds of x" + Globals.Game.currentWorld.CoinIncomeManager.itemBoostTimer.getBoost() + " item-boosted Offline Time");
            }

        }

        return GameOffCoins;
    }
}

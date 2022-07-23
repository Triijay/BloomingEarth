using Bayat.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script to Count Down a Boost
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class BoostTimer : MonoBehaviour {

    /// <summary>
    /// Time Left in Seconds
    /// </summary>
    [JsonProperty(PropertyName = "timeLeftSeconds")]
    public int timeLeftSeconds = 0;

    /// <summary>
    /// The Multiplicator how much the Boost affects the Income
    /// </summary>
    [JsonProperty(PropertyName = "boostMultiplicator")]
    protected float boostMultiplicator = 1;

    /// <summary>
    /// says wether the Timer is already started
    /// </summary>
    protected bool timerStarted = false;

    // Performance  Vars
    protected int maxSecondsOfBoost;
    protected float boostPercentLeft;
    protected GameObject boostPopUp;
    protected TMPro.TextMeshProUGUI BoostPopUpTextRemaining, BoostPopUpWarningText;


    /// <summary>
    /// Inits the BoostTimer
    /// </summary>
    public virtual void initBoostTimer() {
        // read Variables, so that they not are read every second in Update Function
        maxSecondsOfBoost = Globals.Game.initGame.boostMaxSeconds;
        boostPopUp = Globals.UICanvas.uiElements.BoostPopUp;
        BoostPopUpTextRemaining = Globals.UICanvas.uiElements.BoostPopUpTextRemaining;
        BoostPopUpWarningText = Globals.UICanvas.uiElements.BoostPopUpWarningText;

        BoostPopUpTextRemaining.text = "0s";

        //Globals.UICanvas.uiElements.BoostBatteryStatus.transform.localScale = new Vector3(1, 0, 1);
        Globals.UICanvas.uiElements.BoostPopUpBatteryStatus.transform.localScale = new Vector3(1, 0, 1);
    }



    /// <summary>
    /// Fills the time remaining of the boost, if the maxSecondsOfBoost from the InitGame-Object is not full
    /// </summary>
    /// <param name="addTimeInSeconds"></param>
    /// /// <param name="sendEvent">should the function sending the Tracking Event?</param>
    public void activateBoost(float multiplier, int addTimeInSeconds, bool sendEvent = true) {
        try {
            if (multiplier > 1) {
                if (addTimeInSeconds > 0) {
                    //Globals.UICanvas.DebugLabelAddText(addTimeInSeconds);
                    //Globals.UICanvas.DebugLabelAddText(maxSecondsOfBoost);
                    setBoost(multiplier);

                    // to prevent that User isnt getting the complete amount of Boost
                    if (timeLeftSeconds < 0) {
                        timeLeftSeconds = 0;
                    }

                    // The time of incomeMultiplierBoostedSecondsLeft is capped by our InitGame Option
                    if (timeLeftSeconds + addTimeInSeconds <= maxSecondsOfBoost || maxSecondsOfBoost < 0) {
                        if (timeLeftSeconds == 0 && sendEvent) {
                            Globals.Controller.GPiOS.IncrementEventOnce(GPGSIds.event_boosted_once);
                            Globals.Controller.Firebase.IncrementFirebaseEventOnce("boosted_once", "times");
                        }
                        if (maxSecondsOfBoost > 0 && timeLeftSeconds + addTimeInSeconds >= maxSecondsOfBoost - 3600 && sendEvent) {
                            // Increment Max Boost Tracking if the new amount is short to maxSecondsOfBoost
                            Globals.Controller.GPiOS.IncrementEventOnce(GPGSIds.event_boosted_max);
                            Globals.Controller.Firebase.IncrementFirebaseEventOnce("boosted_max", "times");
                        }
                        timeLeftSeconds += addTimeInSeconds;
                        startBoostTimer();
                    } else {
                        timeLeftSeconds = maxSecondsOfBoost;
                        Debug.Log("Boost was capped by maxSecondsOfBoost.");

                        if (sendEvent) {
                            // Increment Max Boost Tracking
                            Globals.Controller.GPiOS.IncrementEventOnce(GPGSIds.event_boosted_max);
                            Globals.Controller.Firebase.IncrementFirebaseEventOnce("boosted_max", "times");
                        }
                    }

                    Debug.Log("Boost started -> added " + addTimeInSeconds + " Seconds - " + (timeLeftSeconds + addTimeInSeconds) + " Seconds left");
                }
            } else {
                Debug.LogError("Income Multipliers must not be under 1!, was " + multiplier);
            }
        } catch (Exception e) {
            Globals.UICanvas.DebugLabelAddText(e);
        }

    }


    /// <summary>
    /// reduces the time of the Boost every Frame and updates all necessary Infos for the User every Second
    /// </summary>
    IEnumerator UpdateBoostTimer() {

        while (true) {

            if (timeLeftSeconds > 0) {

                yield return new WaitForSeconds(1);

                timeLeftSeconds -= 1;


                if (boostPopUp != null) {
                    // GUI
                    boostPercentLeft = (float)timeLeftSeconds / (float)maxSecondsOfBoost;
                    // Clamp Boost (safeguarding)

                    boostPercentLeft = (float)Math.Round( Mathf.Clamp(boostPercentLeft, 0, 1), 2);
                    // refresh the scale of the green Battery-Fill
                    //Globals.UICanvas.uiElements.BoostBatteryStatus.transform.localScale = new Vector3(1, boostPercentLeft, 1);
                    Globals.UICanvas.uiElements.BoostPopUpBatteryStatus.transform.localScale = new Vector3(1, boostPercentLeft, 1);
                    //formatAndOutputBoostTimeleft();

                    // refresh the boost-popup-countdown
                    BoostPopUpTextRemaining.text = (DateTime.Now - DateTime.Now.AddSeconds(-timeLeftSeconds)).ToString(@"hh\h\ mm\m\ ss\s\ ").TrimStart(' ', 'd', 'h', 'm', 's', '0');

                    if (timeLeftSeconds >= maxSecondsOfBoost - (Globals.Game.initGame.boostSecPerVid / 2) ) {

                        // Deactivate BoostButton to prevent AdSpam
                        Globals.UICanvas.uiElements.BoostPopUpAdButtonInvokeBoost.GetComponent<Button>().interactable = false;

                        if (timeLeftSeconds >= maxSecondsOfBoost - (Globals.Game.initGame.boostSecPerVid / 4) ) {
                            BoostPopUpWarningText.text = Globals.Controller.Language.translateString("boostmenu_warning");
                        }
                    } else {
                        // Boost is no Prob to Use
                        Globals.UICanvas.uiElements.BoostPopUpAdButtonInvokeBoost.GetComponent<Button>().interactable = true;
                        BoostPopUpWarningText.text = "";
                    }
                } 
                

            } else {
                stopBoostTimer();
                yield return null;
            }

        }


    }

    /// <summary>
    /// Outputs the TimeLeft to the Text of the BoostButton (rounds up the hours left)
    /// </summary>
    private void formatAndOutputBoostTimeleft(TMPro.TextMeshProUGUI textmeshObject) {

        if (timeLeftSeconds > 3600) {
            // hours left (added half an hour to round up)
            textmeshObject.text = ((timeLeftSeconds+1800) / 3600).ToString() + "h";
        } else if (timeLeftSeconds > 60) {
            // minutes left
            textmeshObject.text = (timeLeftSeconds / 60).ToString() + "m";
        } else {
            // seconds left
            textmeshObject.text = timeLeftSeconds.ToString() + "s";
        }
    }


    /// <summary>
    /// returns the Time left of the Boost in Seconds
    /// </summary>
    /// <returns></returns>
    public int getTimeLeftSec() {
        return timeLeftSeconds;
    }

    /// <summary>
    /// Reduces the Time left in Seconds
    /// </summary>
    /// <param name="newTimeInSec"></param>
    public void reduceTimeLeftSec(int timeInSec) {
        if (timeInSec > 0) {
            timeLeftSeconds = timeLeftSeconds - timeInSec;
        }
    }

    /// <summary>
    /// Reduces the Time left in Seconds to 0
    /// </summary>
    /// <param name="newTimeInSec"></param>
    public void reduceTimeLeftSecTo0() {
         timeLeftSeconds = 0;
    }

    /// <summary>
    /// Starts the Boost timer, if Game was closed
    /// </summary>
    public void startBoostTimer() {
        if (!timerStarted) {
            StartCoroutine(UpdateBoostTimer());
            timerStarted = true;
        }
    }

    /// <summary>
    /// Stops the boost and resets the values
    /// </summary>
    public void stopBoostTimer() {
        Debug.Log("The Boost has stopped.");
        timerStarted = false;
        timeLeftSeconds = 0;
        setBoost(1); // Boost to x1
        StopAllCoroutines();

        // Reset UI
        Globals.UICanvas.uiElements.BoostPopUpBatteryStatus.transform.localScale = new Vector3(1, 0, 1);
        //Globals.UICanvas.uiElements.BoostButtonText.text = "0s";
        Globals.UICanvas.uiElements.BoostPopUpTextRemaining.text = "0s";
    }

    /// <summary>
    /// Is the Timer running?
    /// </summary>
    /// <returns></returns>
    public bool isTimerStarted() {
        return timerStarted;
    }


    /// <summary>
    /// Gets the boostMultiplicator
    /// </summary>
    public float getBoost() {
        return boostMultiplicator;
    }

    /// <summary>
    /// Sets the incomeMultiplier from the Boost-Function
    /// </summary>
    public void setBoost(float multiplier) {
        // Make sure that there is no Bonus that gives a Malus
        if (multiplier < 1) {
            multiplier = 1;
        }
        boostMultiplicator = multiplier;
        Globals.Game.currentWorld.CoinIncomeManager.updateTotalIncome();
    }


}
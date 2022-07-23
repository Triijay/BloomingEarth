using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class CoinsPerSecond : MonoBehaviour {

    /// <summary>
    /// The TextMeshPro UI Component
    /// </summary>
    public TMPro.TextMeshProUGUI IncomeText;
    public TMPro.TextMeshProUGUI PrestigeText;
    public TMPro.TextMeshProUGUI BoostText;
    public TMPro.TextMeshProUGUI ItemBoostText;
    public TMPro.TextMeshProUGUI TotalIncomeText;
    BoostTimer boostTimer;
    BoostTimer itemBoostTimer;

    // Start is called before the first frame update
    public void InitCoinsPerSecondsPopup() {
        // Get the Boost Timers
        boostTimer = Globals.Game.currentWorld.CoinIncomeManager.adBoostTimer;
        itemBoostTimer = Globals.Game.currentWorld.CoinIncomeManager.itemBoostTimer;
    }

    public void startUpdating() {
        StartCoroutine(UpdateCoinsPerSecondPopUp());
    }

    public void stopUpdating() {
        StopAllCoroutines();
    }

    // Update is called once per frame (only when CPS PopUp is open)
    IEnumerator UpdateCoinsPerSecondPopUp() {
        while (true) {

            // Check Boost Timer
            if (boostTimer.getTimeLeftSec() < 0) {
                boostTimer.reduceTimeLeftSecTo0();
            }
            if (itemBoostTimer.getTimeLeftSec() < 0) {
                itemBoostTimer.reduceTimeLeftSecTo0();
            }


            try {
                // Update Income Text
                IncomeText.text = Globals.Controller.Language.translateString("coinsPerSec_Income",
                    new string[] { "<sprite=\"Icon_Coins\" name=\"Icon_Coins\"> " + Globals.Game.currentWorld.CoinIncomeManager.getIncomeBuildings().toRoundedString() + " / s" });
                PrestigeText.text = "Prestige Bonus: x " + Globals.Game.currentUser.prestige.getPrestigeBonus_Rounded();

                // Update Boost Text
                if (boostTimer.getTimeLeftSec() > 0) {
                    BoostText.text = Globals.Controller.Language.translateString("coinsPerSec_Boost",
                    new string[] { boostTimer.getBoost().ToString(), (DateTime.Now - DateTime.Now.AddSeconds(-boostTimer.getTimeLeftSec())).ToString(@"hh\h\ mm\m\ ss\s\ ").TrimStart(' ', 'd', 'h', 'm', 's', '0') });
                } else {
                    BoostText.text = Globals.Controller.Language.translateString("coinsPerSec_Boost",
                    new string[] { boostTimer.getBoost().ToString(), "0s" });
                }
                if (itemBoostTimer.getTimeLeftSec() > 0) {
                    ItemBoostText.text = Globals.Controller.Language.translateString("coinsPerSec_ItemBoost",
                    new string[] { itemBoostTimer.getBoost().ToString(), (DateTime.Now - DateTime.Now.AddSeconds(-itemBoostTimer.getTimeLeftSec())).ToString(@"hh\h\ mm\m\ ss\s\ ").TrimStart(' ', 'd', 'h', 'm', 's', '0') });
                } else {
                    ItemBoostText.text = Globals.Controller.Language.translateString("coinsPerSec_ItemBoost",
                    new string[] { itemBoostTimer.getBoost().ToString(), "0s" });
                }

                // Update Total Text
                TotalIncomeText.text = Globals.Controller.Language.translateString("coinsPerSec_TotalIncome",
                    new string[] { "<sprite=\"Icon_Coins\" name=\"Icon_Coins\"> " + Globals.Game.currentWorld.CoinIncomeManager.totalIncome.toRoundedString() + " / s" });
            } catch (Exception e) {
                Globals.UICanvas.DebugLabelAddText(e);
            }

            yield return new WaitForSeconds(1);
        }
    }
}


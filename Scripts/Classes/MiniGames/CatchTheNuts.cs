using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public class CatchTheNuts : MonoBehaviour {

    public int coinSecondsAdd = 1;
    public int secondsPlaytime = 20;
    /// <summary>
    /// The Factor how many seconds-Income of the current IncomeBuildings will be added per Nut-Catched as Reward
    /// </summary>
    [Tooltip("The Factor how many seconds of the current IncomeBuildings will be added per Nut-Catched as Reward")]
    [Range(1.0f, 10.0f)]
    public float rewardCashFactor;
    public GameObject Squirrel;
    public GameObject Nut;
    public Transform NutSpawn;
    public GameObject BGPanel;
    public GameObject PopUpStart;
    public GameObject PopUpFinished;
    public GameObject PopUpClaimButton;
    public GameObject PopUpClaimShareButton;
    public GameObject Timer;
    public GameObject Score;
    public GameObject FinishText;

    int score = 0;
    int gameSecondsLeft = 0;

    bool minigameRunning = false;

    Random my_rnd = new Random();
    int randomDelay, randomX, randomY;

    const int XYpaddingNutspawn = 120;

    private GameObject cloneNut;
    private RectTransform nutTransform;
    private float waitforNextNut;
    private IdleNum scorePerNut;

    TMPro.TextMeshProUGUI timerlabel;
    TMPro.TextMeshProUGUI scorelabel;

    // disables the spawn of the squirrel
    bool catchTheNutsBlocked;
    bool preventSquirrelHide;

    // Performance
    RectTransform bgPanelRect;
    int width, height;


    // Init the CatchTheNuts Button
    void Start() {
        BGPanel.SetActive(false);
        Nut.SetActive(false);
        timerlabel = Timer.GetComponent<TMPro.TextMeshProUGUI>();
        scorelabel = Score.GetComponent<TMPro.TextMeshProUGUI>();
        PopUpClaimButton.GetComponent<Button>().interactable = false;
        StartCoroutine(ShowSquirrelAfterSeconds());
    }

    public IEnumerator ShowSquirrelAfterSeconds() {

        hideSquirrel();

        while (true) {

            // Get Random Time to Squirrel Disapear (seconds)
            if (!Globals.KaloaSettings.debugMode) {
                randomDelay = my_rnd.Next(70, 100);
            } else {
                randomDelay = my_rnd.Next(7, 10);
            }

            // Wait for DelayTime
            yield return new WaitForSeconds(randomDelay);

            
            // Only show the Squirrel, when its okay for us:
            if (!Globals.UICanvas.uiElements.MiniGamePopUp.activeSelf // If MinigamePopUp is not Open
                && !minigameRunning // If Minigame is not Running
                && Globals.Game.currentWorld.CoinIncomeManager.getIncomeBuildings() > new IdleNum(0) // If Buildings has Income
                && !catchTheNutsBlocked
                ) {
                // Squirrel Shows up
                showSquirrel();
            }

            // Get Random Time to Squirrel Disapear (seconds)
            if (!Globals.KaloaSettings.debugMode) {
                randomDelay = my_rnd.Next(30, 45);
            } else {
                randomDelay = my_rnd.Next(7, 10);
            }
            yield return new WaitForSeconds(randomDelay);

            // Squirrel disappears
            if (!preventSquirrelHide) {
                hideSquirrel();
            }
        }

    }

    /// <summary>
    /// Shows the Squirrel
    /// </summary>
    public void showSquirrel() {
        Squirrel.SetActive(true);
    }

    /// <summary>
    /// Hides the Squirrel
    /// </summary>
    public void hideSquirrel() {
        Squirrel.SetActive(false);
    }

    /// <summary>
    /// Gets the Description of the PopUp and fills in the current score Per Nut
    /// </summary>
    public void openMiniGamePopUp() {

        PanelOpener.closeAllPopUpsStatic();

        scorePerNut = Globals.Game.currentWorld.CoinIncomeManager.getBaseIncome() * rewardCashFactor;
        Globals.UICanvas.translatedTMProElements.MiniGameDescription.text = Globals.Controller.Language.translateString(
            "minigame_start_information",
            new string[] { "<sprite=\"Icon_Coins\" name=\"Icon_Coins\"> " + scorePerNut.toRoundedString() }
            );
        PopUpStart.SetActive(true);
    }



    public void StartMiniGame() {
        
        // Fade Out MiniGameStartPopUp
        Globals.UICanvas.uiElements.MiniGamePopUp.SetActive(false);

        // Fade In CatchTheNuts Elements
        Timer.SetActive(true);
        Score.SetActive(true);
        FinishText.SetActive(false);
        hideSquirrel();

        BGPanel.SetActive(true);

        // Set new PlayTime
        gameSecondsLeft = secondsPlaytime;
        minigameRunning = true;

        // Set Score to 0
        score = 0;
        scorelabel.text = "Score: " + score;

        StopAllCoroutines();
        StartCoroutine(GameTimer());
        StartCoroutine(GameActions());
    }

    public IEnumerator GameTimer() {

        while (true) {
            if (gameSecondsLeft > 0) {
                timerlabel.text = "00:" + gameSecondsLeft.ToString("D2");
                yield return new WaitForSeconds(1);
                gameSecondsLeft -= 1;

            } else {
                StopAllCoroutines();
                StartCoroutine(ComeToRest());
                yield return null;
            }
        }

    }

    public IEnumerator GameActions() {

        while (gameSecondsLeft > 0) {

            cloneNut = Instantiate(Nut, NutSpawn);
            nutTransform = (RectTransform)cloneNut.transform;

            // get the BG Panel Size
            bgPanelRect = BGPanel.transform as RectTransform;
            width = Convert.ToInt32(bgPanelRect.rect.width);
            height = Convert.ToInt32(bgPanelRect.rect.height);

            // Spawn Nut random on Screen 
            nutTransform.anchoredPosition = new Vector2(
                my_rnd.Next(XYpaddingNutspawn, width - XYpaddingNutspawn), 
                my_rnd.Next(XYpaddingNutspawn, height - XYpaddingNutspawn));
            nutTransform.localRotation = Quaternion.Euler(0,0, (float)my_rnd.Next(-20, 110));
            cloneNut.SetActive(true);
            
            int secondsPlayed = secondsPlaytime - gameSecondsLeft;
            //Debug.Log("Min" + (int)(2 + secondsPlayed / 4));
            //Debug.Log("Max" + (int)(4 + secondsPlayed / 4));
            randomDelay = my_rnd.Next(2 + secondsPlayed/4, 4 + secondsPlayed/4);

            waitforNextNut = (2f / (float)randomDelay);
            //Debug.Log(waitforNextNut);

            yield return new WaitForSeconds(waitforNextNut);
        }

        yield return null;
    }


    /// <summary>
    /// When Clicked a Nut, raise the score and destroy the Nut
    /// </summary>
    /// <param name="nut"></param>
    public void clickedNut(GameObject nut) {
        // raise Score
        ++score;

        scorelabel.text = "Score: " + score;

        Destroy(nut);
    }

    /// <summary>
    /// Player gets "Finished" and should come to Rest to prevent that he accidently clicks a Button in FinishPopUp
    /// </summary>
    /// <returns></returns>
    public IEnumerator ComeToRest() {

        Timer.SetActive(false);
        FinishText.SetActive(true);
        
        // Delete All Nut-Clones
        foreach (Transform child in NutSpawn) {
            Destroy(child.gameObject);
        }

        yield return new WaitForSeconds(1.5f);

        // Finish the Game
        FinishMiniGame();

        yield return null;
    }


    private void FinishMiniGame() {

        // Set Game Running to false
        minigameRunning = false;
        BGPanel.SetActive(false);

        // Achievements (must be before highscore is updated!)
        manageCatchTheNutsAchievements(score);

        // Get Highscore
        manageCatchTheNutsHighScore();

        Globals.UICanvas.translatedTMProElements.MiniGameFinishedDescription.text = Globals.Controller.Language.translateString(
            "minigame_end_information",
            new string[] { "<sprite=\"Nut\" name=\"Nut\"> " + score.ToString() }
            );
        Globals.UICanvas.translatedTMProElements.MiniGameFinishedRewardCoins.text = "<sprite=\"Icon_Coins\" name=\"Icon_Coins\"> " + (scorePerNut * score).toRoundedString();

        PopUpClaimButton.transform.Find("Text").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = Globals.Controller.Language.translateString("offlinecoins_button_claim");
        PopUpClaimShareButton.transform.Find("Text").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = Globals.Controller.Language.translateString("minigame_button_claim_and_share");

        PopUpClaimButton.GetComponent<Button>().interactable = true;
        PopUpFinished.SetActive(true);
        

        // Post Score to Leaderboard
        Globals.Controller.GPiOS.ScoreToLeaderBoard(score, GPGSIds.leaderboard_catch_the_nuts);

    }


    public void claimReward() {

        Globals.Controller.Sound.PlaySound("CoinReward");

        Globals.UICanvas.uiElements.ParticleEffects.addCoins(scorePerNut*score, true);

        CatchTheNuts mgame = Globals.UICanvas.uiElements.InitGameObject.GetComponent<CatchTheNuts>();
        
        // Close PopUp
        mgame.PopUpFinished.SetActive(false);

        // Start Counter for Squirrel
        mgame.PopUpClaimButton.GetComponent<Button>().interactable = false;
        mgame.Start();

    }

    public void claimRewardAndShare() {

        claimReward();

        StartCoroutine(ShareScore(score));

    }

    public IEnumerator ShareScore(int score) {

        yield return new WaitForSeconds(1);

        NativeShare ns = new NativeShare();
        Globals.Controller.Firebase.IncrementFirebaseEventOnce("shared_function_minigame_nuts");
        ns.AddFile(Application.dataPath + "/Resources/Icons/IconHouse2.png");
        ns.SetTitle("Blooming Earth");
        ns.SetText(Globals.Controller.Language.translateString(
            "minigame_nuts_pressed_share", 
            new string[] {score.ToString(), Globals.KaloaSettings.linkPlayStore }));
        ns.Share();
    }


    /// <summary>
    /// After the Minigame the User can get Achievements
    /// </summary>
    public void manageCatchTheNutsAchievements(int aScore) {
        if (aScore >= 75) {
            // Pr0
            if (Globals.Game.currentUser.stats.HighscoreCatchTheNuts > 65) {
                Globals.Controller.GPiOS.sentAchievements100Percent(new string[] {
                    GPGSIds.achievement_catch_the_nuts__like_a_pro,
                    GPGSIds.achievement_catch_the_nuts__rookie,
                    GPGSIds.achievement_catch_the_nuts__semi_pro,
                }); Globals.Controller.GPiOS.sentAchievement100Percent(GPGSIds.achievement_catch_the_nuts__like_a_pro);
            } else if (Globals.Game.currentUser.stats.HighscoreCatchTheNuts > 50) {
                // get first the new semi-pro, then the pro (after that, the rookie, if he wasnt online when he got the rookie achievement)
                Globals.Controller.GPiOS.sentAchievements100Percent(new string[] {
                    GPGSIds.achievement_catch_the_nuts__semi_pro,
                    GPGSIds.achievement_catch_the_nuts__like_a_pro,
                    GPGSIds.achievement_catch_the_nuts__rookie,
                });
            } else {
                // first try pro gets all Achievements after another
                Globals.Controller.GPiOS.sentAchievements100Percent(new string[] {
                    GPGSIds.achievement_catch_the_nuts__rookie,
                    GPGSIds.achievement_catch_the_nuts__semi_pro,
                    GPGSIds.achievement_catch_the_nuts__like_a_pro,
                });
            }
        } else if (aScore >= 65) {
            // Semi Pro
            if (Globals.Game.currentUser.stats.HighscoreCatchTheNuts > 50) {
                // get first the new semi-pro (after that, the rookie, if he wasnt online when he got the rookie achievement)
                Globals.Controller.GPiOS.sentAchievements100Percent(new string[] {
                    GPGSIds.achievement_catch_the_nuts__semi_pro,
                    GPGSIds.achievement_catch_the_nuts__rookie,
                });
            } else {
                // first try semi-pro gets both Achievements after another
                Globals.Controller.GPiOS.sentAchievements100Percent(new string[] {
                    GPGSIds.achievement_catch_the_nuts__rookie,
                    GPGSIds.achievement_catch_the_nuts__semi_pro,
                });
            }
        } else if (aScore >= 50) {
            // Rookie
            Globals.Controller.GPiOS.sentAchievement100Percent(GPGSIds.achievement_catch_the_nuts__rookie);
        }
    }

    /// <summary>
    /// Every User has its own HighScore at the specific Minigame
    /// </summary>
    private void manageCatchTheNutsHighScore() {
        if (score > Globals.Game.currentUser.stats.HighscoreCatchTheNuts) {
            // New Highscore !

            // Set new Highscore
            Globals.Game.currentUser.stats.HighscoreCatchTheNuts = score;

            // Read the setted Highscore
            Globals.UICanvas.translatedTMProElements.MiniGameFinishedHighscore.text = Globals.Controller.Language.translateString(
                "minigame_new_highscore",
                new string[] { "<sprite=\"Nut\" name=\"Nut\"> " + Globals.Game.currentUser.stats.HighscoreCatchTheNuts.ToString() }
            );
        } else {
            // no new Highscore
            // read last Highscore
            Globals.UICanvas.translatedTMProElements.MiniGameFinishedHighscore.text = Globals.Controller.Language.translateString(
                "minigame_no_highscore",
                new string[] { "<sprite=\"Nut\" name=\"Nut\"> " + Globals.Game.currentUser.stats.HighscoreCatchTheNuts.ToString() }
            );
        }
    }


    /// <summary>
    /// Enables/Disables the Spawn of the Squirrel
    /// </summary>
    /// <param name="newStatus"></param>
    public void setCatchTheNutsBlocked(bool newStatus) {
        catchTheNutsBlocked = newStatus;
    }


    public void setPreventSquirrelHide(bool newStatus) {
        preventSquirrelHide = newStatus;
    }

}

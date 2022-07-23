using System;
using UnityEngine;
using System.Collections;
using GoogleMobileAds.Api;

/// <summary>
/// Wrapper for Google AdMob Services <br></br>
/// Callback Functions - EventHandler
/// </summary>
public partial class AdWrapper : MonoBehaviour {

    // Development Mode and Script Debugging has to be unchecked
    // otherwise the events are not fired

    #region Rewarded Boost callback handlers

    bool check_boost_started = false;

    // Called when an ad request has successfully loaded.
    private void HandleRewardedBoostAdLoaded(object sender, EventArgs args) {

        // Reset load Count
        reloadCount_boost = 0;
        
        // Enable the Buttons where the Ad can be called
        setButtonLoadingStatus(false, Globals.UICanvas.uiElements.BoostPopUpAdButtonInvokeBoost, reloadCount_boost == MAXIMUM_RELOADCOUNT);
        
        // If BoostAd was successfully loaded and the BuildingProgressAd not, try to reload once again
        if (forwardBuildingProgressAd != null && !forwardBuildingProgressAd.IsLoaded()) {
            reloadCount_forwardBuildingProgress = 0;
            this.CreateAndLoadForwardBuildingProgressAd();
        }

        checkButtonLoadingStatus();
    }

    // Called when an ad request failed to load.
    private void HandleRewardedBoostAdFailedToLoad(object sender, AdFailedToLoadEventArgs args) {
        Globals.UICanvas.DebugLabelAddText("Boost Ad failed to load with message: "
                             + args.LoadAdError);

        if (!check_boost_started) {
            UnityMainThreadDispatcher.Instance().Enqueue(checkIfBoostAdCanBeLoaded());
        }

        checkButtonLoadingStatus();
    }

    //Retries constantly to load the failed Ad
    private IEnumerator checkIfBoostAdCanBeLoaded() {

        check_boost_started = true;

        //Globals.UICanvas.DebugLabelAddText("Boost Ad: Check started");
        
        while (!boostAd.IsLoaded()) {

            try {
                CreateAndLoadBoostAd();
            }
            catch (Exception e){ Globals.UICanvas.DebugLabelAddText(e.Message);  }
            //Globals.UICanvas.DebugLabelAddText(boostAd.IsLoaded().ToString()+ " Checking BoostAd, Tried " + reloadCount_boost +" times so far");

            if (reloadCount_boost <= MAXIMUM_RELOADCOUNT) {
                reloadCount_boost++;
                yield return new WaitForSeconds(1);
            } else {
                yield return new WaitForSeconds(15);
            }
            
        }

        check_boost_started = false;
        yield return null;
    }

    // Called when an ad is shown.
    private void HandleRewardedBoostAdOpening(object sender, EventArgs args) {
    }

    // Called when the user should be rewarded for interacting with the ad.
    public void HandleUserEarnedBoostReward(object sender, Reward args) {
        // Handle Reward in Main Thread
        UnityMainThreadDispatcher.Instance().Enqueue(rewardUserEarnedBoost());        
    }

    // Called when the ad is closed.
    private void HandleRewardedBoostAdClosed(object sender, EventArgs args) {

        // Load new Ad
        CreateAndLoadBoostAd();

        // Disable the Button until new Ad is loaded
        setButtonLoadingStatus(true, Globals.UICanvas.uiElements.BoostPopUpAdButtonInvokeBoost, reloadCount_boost == MAXIMUM_RELOADCOUNT);
    }

    /// <summary>
    /// Mainthread: Increment Events and activate Boost
    /// </summary>
    /// <returns></returns>
    public IEnumerator rewardUserEarnedBoost() {
        Globals.Controller.GPiOS.IncrementEventOnce(GPGSIds.event_video_watched);
        Globals.Controller.GPiOS.IncrementEventOnce(GPGSIds.event_video_watched_boost);
        Globals.Controller.Firebase.IncrementFirebaseEventOnce("ad_video_watched", "times");
        Globals.Controller.Firebase.IncrementFirebaseEventOnce("ad_video_watched_boost", "times");

        Globals.Game.currentUser.stats.adsWatched++;
        Globals.Game.currentUser.stats.adsWatchedBoost++;

        // Add (currently 2) Hours to the World Boost
        Globals.Game.currentWorld.CoinIncomeManager.adBoostTimer.activateBoost(
            Globals.Game.initGame.boostMultiplier, //setted in InitGame Object
            Globals.Game.initGame.boostSecPerVid
        );
        yield return null;
    }

    #endregion







    #region Rewarded OfflineCoins callback handlers

    // Called when an ad request has successfully loaded.
    private void HandleRewardedOfflineCoinsAdLoaded(object sender, EventArgs args) {

        if (LoadingScreen.loadingScreenIsActive) {
            LoadingScreen.setAdmobInitialized();
        }

        // Globals.UICanvas.DebugLabelAddText("Offline Coins Ad Loaded");
        Debug.Log("Offline Coins Ad Loaded");
        
        // Reset load Count
        reloadCount_offlineCoins = 0;

        // Enable the Buttons where the Ad can be called
        setButtonLoadingStatus(false, Globals.UICanvas.uiElements.PopUpOfflineCoinsAdButtonX2, reloadCount_offlineCoins <= MAXIMUM_RELOADCOUNT);

        // Load Other Ads
        CreateAndLoadBoostAd();
        CreateAndLoadForwardBuildingProgressAd();

        checkButtonLoadingStatus();

    }

    // Called when an ad request failed to load.
    private void HandleRewardedOfflineCoinsAdFailedToLoad(object sender, AdFailedToLoadEventArgs args) {

        LoadingScreen.setAdmobInitialized();

        if (reloadCount_offlineCoins <= MAXIMUM_RELOADCOUNT) {
            this.CreateAndLoadOfflineCoinsAd();
            reloadCount_offlineCoins++;
        } else {
            Globals.UICanvas.DebugLabelAddText("Offline Coins Ad failed to load - event received with message: " + args.LoadAdError);
            //Globals.Wrapper.FirebaseWrapper.IncrementFirebaseEventOnce("ad_video_failed_offlinecoins", "times");
        }

        // Load Other Ads
        CreateAndLoadBoostAd();
        CreateAndLoadForwardBuildingProgressAd();

        checkButtonLoadingStatus();
    }


    // Called when an ad is shown.
    private void HandleRewardedOfflineCoinsAdOpening(object sender, EventArgs args) {
    }

    // Called when the user should be rewarded for interacting with the ad.
    public void HandleUserEarnedOfflineCoinsReward(object sender, Reward args) {
        // Handle Reward in Main Thread
        UnityMainThreadDispatcher.Instance().Enqueue(rewardEarnedOfflineCoins());
    }

    // Called when the ad is closed.
    public void HandleRewardedOfflineCoinsAdClosed(object sender, EventArgs args) {


        // Handle 1x Coins in Main Thread
        UnityMainThreadDispatcher.Instance().Enqueue(adClosedOfflineCoins());

        // Disable the Button until new Ad is loaded
        setButtonLoadingStatus(true, Globals.UICanvas.uiElements.PopUpOfflineCoinsAdButtonX2, reloadCount_offlineCoins <= MAXIMUM_RELOADCOUNT);

        // Load new Ad
        this.CreateAndLoadOfflineCoinsAd();  
    }

    /// <summary>
    /// Mainthread: Increment Events and add Coins to CurrentWorld
    /// </summary>
    /// <returns></returns>
    public IEnumerator rewardEarnedOfflineCoins() {
        Globals.Controller.GPiOS.IncrementEventOnce(GPGSIds.event_video_watched);
        Globals.Controller.GPiOS.IncrementEventOnce(GPGSIds.event_video_watched_offlinecoins_doubled);
        Globals.Controller.Firebase.IncrementFirebaseEventOnce("ad_video_watched", "times");
        Globals.Controller.Firebase.IncrementFirebaseEventOnce("ad_video_watched_offlinecoins_doubled", "times");

        Globals.Game.currentUser.stats.adsWatched++;
        Globals.Game.currentUser.stats.adsWatchedOfflineCoins++;

        // Add half the coins if vid is watched (The other half is added when Video closes, to prevent that the User gains nothing)
        Globals.UICanvas.uiElements.ParticleEffects.addCoins(rewardingOfflineCoins / 2, true);

        yield return null;
    }

    /// <summary>
    /// Mainthread: Increment Events and add Coins to World1
    /// </summary>
    /// <returns></returns>
    public IEnumerator adClosedOfflineCoins() {
        // Add half the coins anyway (Regardless the user Closes the Ad before the min-time)
        Globals.Game.currentWorld.addCoins(rewardingOfflineCoins / 2);

        yield return null;
    }

    #endregion








    #region Rewarded Forward Building Progress callback handlers

    bool check_forwardbp_started = false;

    // Called when an ad request has successfully loaded.
    private void HandleRewardedForwardBuildingProgressAdLoaded(object sender, EventArgs args) {
        // Reset load Count
        reloadCount_forwardBuildingProgress = 0;

        // Enable the Buttons where the Ad can be called
        setButtonLoadingStatus(false, Globals.UICanvas.uiElements.BuildingMenuAdButtonMinus30, reloadCount_forwardBuildingProgress == MAXIMUM_RELOADCOUNT);

        // If BuildingProgressAd was successfully loaded and the BoostAd not, try to reload once again
        if (boostAd != null && !boostAd.IsLoaded()) {
            reloadCount_boost = 0;
            CreateAndLoadBoostAd();
        }

        checkButtonLoadingStatus();

    }

    // Called when an ad request failed to load.
    private void HandleRewardedForwardBuildingProgressAdFailedToLoad(object sender, AdFailedToLoadEventArgs args) {
        Globals.UICanvas.DebugLabelAddText("ForwardBuildingProgress Ad failed to load with message: "
                             + args.LoadAdError);

        if (!check_forwardbp_started) {
            UnityMainThreadDispatcher.Instance().Enqueue(checkIfForwardAdCanBeLoaded());
        }

        checkButtonLoadingStatus();
    }

    //Retries constantly to load the failed Ad
    private IEnumerator checkIfForwardAdCanBeLoaded() {

        check_forwardbp_started = true;

        //Globals.UICanvas.DebugLabelAddText("Check started");

        while (!forwardBuildingProgressAd.IsLoaded()) {

            try {
                CreateAndLoadForwardBuildingProgressAd();
            }
            catch (Exception e) { Globals.UICanvas.DebugLabelAddText(e.Message); }
            //Globals.UICanvas.DebugLabelAddText(forwardBuildingProgressAd.IsLoaded().ToString() + reloadCount_forwardBuildingProgress + " Checking ForwardAd ");

            if (reloadCount_forwardBuildingProgress <= MAXIMUM_RELOADCOUNT) {
                reloadCount_forwardBuildingProgress++;
                yield return new WaitForSeconds(1);
            } else {
                yield return new WaitForSeconds(15);
            }

        }

        //Globals.UICanvas.DebugLabelAddText("ForwardAd loaded");
        check_forwardbp_started = false;
        yield return null;
    }

    // Called when an ad is shown.
    private void HandleRewardedForwardBuildingProgressAdOpening(object sender, EventArgs args) {
    }

    // Called when the user should be rewarded for interacting with the ad.
    public void HandleUserEarnedForwardBuildingProgressReward(object sender, Reward args) {
        // Handle Reward in Main Thread
        UnityMainThreadDispatcher.Instance().Enqueue(rewardForwardBuildingProgress());
    }

    // Called when the ad is closed.
    private void HandleRewardedForwardBuildingProgressAdClosed(object sender, EventArgs args) {

        // Load new Ad
        this.CreateAndLoadForwardBuildingProgressAd();

        // Disable the Button until new Ad is loaded
        setButtonLoadingStatus(true, Globals.UICanvas.uiElements.BuildingMenuAdButtonMinus30, reloadCount_forwardBuildingProgress == MAXIMUM_RELOADCOUNT);
    }

    /// <summary>
    /// Mainthread: Increment Events and reduce the waiting time of the Building
    /// </summary>
    /// <returns></returns>
    public IEnumerator rewardForwardBuildingProgress() {
        Globals.Controller.GPiOS.IncrementEventOnce(GPGSIds.event_video_watched);
        Globals.Controller.GPiOS.IncrementEventOnce(GPGSIds.event_video_watched_buildingdelay);
        Globals.Controller.Firebase.IncrementFirebaseEventOnce("ad_video_watched", "times");
        Globals.Controller.Firebase.IncrementFirebaseEventOnce("ad_video_watched_buildingdelay", "times");

        Globals.Game.currentUser.stats.adsWatched++;
        Globals.Game.currentUser.stats.adsWatchedBuildingProgress++;

        // Reduce Waiting Time
        if (rewardingBuilding != null) {
            rewardingBuilding.ReduceWaitingTimeMinus30();
        }
        yield return null;
    }

    #endregion
}


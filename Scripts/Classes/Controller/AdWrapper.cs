using System;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Wrapper for Google AdMob Services <br></br>
/// Call a Function and it will decide what to do on any device
/// </summary>
public partial class AdWrapper : MonoBehaviour {

    #region Variables
    /// <summary>
    /// Rewarded Ad Object for Boost Ad
    /// </summary>
    private RewardedAd boostAd;

    /// <summary>
    /// Rewarded Ad Object for Offline Coins Ad
    /// </summary>
    private RewardedAd offlineCoinsAd;

    /// <summary>
    /// Rewarded Ad Object for Forward Building Progress Ad
    /// </summary>
    private RewardedAd forwardBuildingProgressAd;

    /// <summary>
    /// AdUnitId for Rewarded Boost Ad
    /// </summary>
    private string adUnitId_boost = "";

    /// <summary>
    /// AdUnitId for Rewarded Offline Coins Ad
    /// </summary>
    private string adUnitId_offlineCoins = "";

    /// <summary>
    /// AdUnitId for Rewarded Forward Building Progress Ad
    /// </summary>
    private string adUnitId_forwardBuildingProgress = "";

    /// <summary>
    /// Counts how often the BOOST ad is reloaded
    /// </summary>
    private int reloadCount_boost = 0;

    /// <summary>
    /// Counts how often the OFFLINECOINS ad is reloaded
    /// </summary>
    private int reloadCount_offlineCoins = 0;

    /// <summary>
    /// Counts how often the FORWARDBUILDINGPROGRESS ad is reloaded
    /// </summary>
    private int reloadCount_forwardBuildingProgress = 0;

    /// <summary>
    /// Maximum tries to load an Ad
    /// </summary>
    private const int MAXIMUM_RELOADCOUNT = 2;

    /// <summary>
    /// OfflineCoins from PopUp
    /// </summary>
    private IdleNum rewardingOfflineCoins = new IdleNum(0);

    /// <summary>
    /// active Building from Building Menu
    /// </summary>
    private Building rewardingBuilding = null;


    // Performance Optimization
    static NetworkReachability networkReachability;

    #endregion


    

    //* -- Google Ads
    /// <summary>
    /// Initialize the Mobile Ads SDK
    /// this needs to be done only one, ideally at app launch
    /// </summary>
    public void initMobileAds() {

        // Initialize the Google Mobile Ads SDK
        MobileAds.Initialize(initStatus => { });
        List<string> deviceIds = new List<string>();
        deviceIds.Add("789AE109B8816CE7AF91257292E52F82");
        RequestConfiguration requestConfiguration = new RequestConfiguration
            .Builder()
            .SetTestDeviceIds(deviceIds)
            .build();
        MobileAds.SetRequestConfiguration(requestConfiguration);

        this.GetAdIdsAndCreateRewardedAds();
    }

    /// <summary>
    /// Get the Ad IDs for all Rewarded Ads
    /// and Create and Load OfflineCoinAd (other Ads will be loaded after OfflineCoinAd is ready) <br></br>
    /// This needs to be done only once at the beginning
    /// </summary>
    private void GetAdIdsAndCreateRewardedAds() {
        // get the Ad ID for the rewarded Ad
        adUnitId_boost = getAdUnitID(Globals.KaloaSettings.AdType.Boost);
        adUnitId_offlineCoins = getAdUnitID(Globals.KaloaSettings.AdType.OfflineCoins);
        adUnitId_forwardBuildingProgress = getAdUnitID(Globals.KaloaSettings.AdType.ForwardBuildingProgress);

        Globals.UICanvas.DebugLabelAddText("Boost Ad ID: "+ adUnitId_boost);

        // Create Rewarded Ad with adUnit Id
        CreateAndLoadOfflineCoinsAd();
        // Other Ads will be Loaded after OfflineCoinsAd is ready - see HandleRewardedBoostAdLoaded
        // Thats because after start we need OfflineCoinsAd as fast as we can
    }

    /// <summary>
    /// Create and Load Boost Ad, Add Boost EventHandler
    /// </summary>
    private void CreateAndLoadBoostAd() {
        try {
            if (adUnitId_boost != "") {
                // Create Rewarded Ad with ID
                this.boostAd = new RewardedAd(adUnitId_boost);

                //* Add Event handler for ad's lifecycle
                // Called when an ad request has successfully loaded.
                this.boostAd.OnAdLoaded += HandleRewardedBoostAdLoaded;
                // Called when an ad request failed to load.
                this.boostAd.OnAdFailedToLoad += HandleRewardedBoostAdFailedToLoad;
                // Called when an ad request failed to show.
                //this.boostAd.OnAdFailedToShow += HandleRewardedAdFailedToShow;
                // Called when an ad is shown.
                this.boostAd.OnAdOpening += HandleRewardedBoostAdOpening;
                // Called when the user should be rewarded for interacting with the ad.
                this.boostAd.OnUserEarnedReward += HandleUserEarnedBoostReward;
                // Called when the ad is closed.
                this.boostAd.OnAdClosed += HandleRewardedBoostAdClosed;
                
                // Create an empty ad request.
                AdRequest request = new AdRequest.Builder().AddExtra("npa", "1").Build();
                // Load the rewarded ad with the request.
                this.boostAd.LoadAd(request);

            } else {
                Globals.UICanvas.DebugLabelAddText("CreateAndLoadBoostAd: No adUnitId_boost set");
            }

        } catch (Exception e) {
            Globals.UICanvas.DebugLabelAddText(e);
        }

    }

    /// <summary>
    /// Create and Load Offline Coins Ad, Add Offline Coins EventHandler
    /// </summary>
    private void CreateAndLoadOfflineCoinsAd() {

        try {
            if (adUnitId_offlineCoins != "") {
                // Create Rewarded Ad with ID
                this.offlineCoinsAd = new RewardedAd(adUnitId_offlineCoins);

                Debug.Log("CreateRewardedAd");

                //* Add Event handler for ad's lifecycle
                // Called when an ad request has successfully loaded.
                this.offlineCoinsAd.OnAdLoaded += HandleRewardedOfflineCoinsAdLoaded;
                // Called when an ad request failed to load.
                this.offlineCoinsAd.OnAdFailedToLoad += HandleRewardedOfflineCoinsAdFailedToLoad;
                // Called when an ad is shown.
                this.offlineCoinsAd.OnAdOpening += HandleRewardedOfflineCoinsAdOpening;
                // Called when the user should be rewarded for interacting with the ad.
                this.offlineCoinsAd.OnUserEarnedReward += HandleUserEarnedOfflineCoinsReward;
                // Called when the ad is closed.
                this.offlineCoinsAd.OnAdClosed += HandleRewardedOfflineCoinsAdClosed;

                // Create an empty ad request.
                AdRequest request = new AdRequest.Builder().AddExtra("npa", "1").Build();
                // Load the rewarded ad with the request.
                this.offlineCoinsAd.LoadAd(request);
            } else {
                Globals.UICanvas.DebugLabelAddText("CreateAndLoadOfflineCoinsAd: No adUnitId_offlineCoins set");
            }

        } catch (Exception e) {
            Globals.UICanvas.DebugLabelAddText("Error in AdWrapper.CreateAndLoadOfflineCoinsAd: " + e.ToString(), true);
        }

    }

    /// <summary>
    /// Create and Load Forward Building Progress Ad, Add Forward Building Progress EventHandler
    /// </summary>
    private void CreateAndLoadForwardBuildingProgressAd() {

        try {
            if (adUnitId_forwardBuildingProgress != "") {
                // Create Rewarded Ad with ID
                this.forwardBuildingProgressAd = new RewardedAd(adUnitId_forwardBuildingProgress);

                //* Add Event handler for ad's lifecycle
                // Called when an ad request has successfully loaded.
                this.forwardBuildingProgressAd.OnAdLoaded += HandleRewardedForwardBuildingProgressAdLoaded;
                // Called when an ad request failed to load.
                this.forwardBuildingProgressAd.OnAdFailedToLoad += HandleRewardedForwardBuildingProgressAdFailedToLoad;
                // Called when an ad is shown.
                this.forwardBuildingProgressAd.OnAdOpening += HandleRewardedForwardBuildingProgressAdOpening;
                // Called when the user should be rewarded for interacting with the ad.
                this.forwardBuildingProgressAd.OnUserEarnedReward += HandleUserEarnedForwardBuildingProgressReward;
                // Called when the ad is closed.
                this.forwardBuildingProgressAd.OnAdClosed += HandleRewardedForwardBuildingProgressAdClosed;

                // Create an empty ad request.
                AdRequest request = new AdRequest.Builder().AddExtra("npa", "1").Build();
                // Load the rewarded ad with the request.
                this.forwardBuildingProgressAd.LoadAd(request);
            } else {
                Globals.UICanvas.DebugLabelAddText("CreateAndLoadForwardBuildingProgressAd: No adUnitId_forwardBuildingProgress set");
            }

        }
        catch (Exception e) {
            Globals.UICanvas.DebugLabelAddText("Error in AdWrapper.CreateAndLoadForwardBuildingProgressAd: " + e.ToString());
        }

    }

    /// <summary>
    /// Get the AdUnitId for the specified AdType
    /// regardless of Device Platform
    /// </summary>
    private string getAdUnitID(Globals.KaloaSettings.AdType type) {
        // Unit ID to return
        string adUnitId = "";

        // Play test Ad when Admin is playing in debug mode
        if (Globals.KaloaSettings.preventRealAds) {
            if (Application.platform == RuntimePlatform.Android) {
                adUnitId = "ca-app-pub-3940256099942544/5224354917";
            } else if (Application.platform == RuntimePlatform.IPhonePlayer) {
                adUnitId = "ca-app-pub-3940256099942544/1712485313";
            } else {
                adUnitId = "unexpected_platform";
            }
            Globals.Controller.Firebase.IncrementFirebaseEventOnce("adwarning_testAd_watched", "times");
        } else {

            // Check which type is required and generate ID
            switch (type) {
                case Globals.KaloaSettings.AdType.Boost:
                    if (Application.platform == RuntimePlatform.Android) {
                        adUnitId = "ca-app-pub-6274265840337023/4162404811";
                    } else if (Application.platform == RuntimePlatform.IPhonePlayer) {
                        adUnitId = "ca-app-pub-3940256099942544/1712485313";
                    } else {
                        adUnitId = "unexpected_platform";
                    }
                    break;
                case Globals.KaloaSettings.AdType.OfflineCoins:
                    if (Application.platform == RuntimePlatform.Android) {
                        adUnitId = "ca-app-pub-6274265840337023/4922580423";
                    } else if (Application.platform == RuntimePlatform.IPhonePlayer) {
                        adUnitId = "ca-app-pub-3940256099942544/1712485313";
                    } else {
                        adUnitId = "unexpected_platform";
                    }
                    break;
                case Globals.KaloaSettings.AdType.ForwardBuildingProgress:
                    if (Application.platform == RuntimePlatform.Android) {
                        adUnitId = "ca-app-pub-6274265840337023/4465895798";
                    } else if (Application.platform == RuntimePlatform.IPhonePlayer) {
                        adUnitId = "ca-app-pub-3940256099942544/1712485313";
                    } else {
                        adUnitId = "unexpected_platform";
                    }
                    break;
                default:
                    if (Application.platform == RuntimePlatform.Android) {
                        adUnitId = "ca-app-pub-3940256099942544/5224354917";
                    } else if (Application.platform == RuntimePlatform.IPhonePlayer) {
                        adUnitId = "ca-app-pub-3940256099942544/1712485313";
                    } else {
                        adUnitId = "unexpected_platform";
                    }

                    Globals.Controller.Firebase.IncrementFirebaseEventOnce("adwarning_testAd_watched", "times");
                    break;
            }

        }

        return adUnitId;

    }

    /// <summary>
    /// Show the referenced Ad
    /// </summary>
    public void ShowRewardedAd(Globals.KaloaSettings.AdType type, IdleNum coins = null, Building building = null) {
        // Check which type is required and load appropriate Ad
        switch (type) {
            case Globals.KaloaSettings.AdType.Boost:
                if (this.boostAd.IsLoaded()) {
                    this.boostAd.Show();
                }
                break;
            case Globals.KaloaSettings.AdType.OfflineCoins:
                if(coins != new IdleNum(0)) {
                    rewardingOfflineCoins = coins;
                }

                if (this.offlineCoinsAd.IsLoaded()) {
                    this.offlineCoinsAd.Show();
                }
                break;
            case Globals.KaloaSettings.AdType.ForwardBuildingProgress:
                if (building != null) {
                    rewardingBuilding = building;
                }
                if (this.forwardBuildingProgressAd.IsLoaded()) {
                    this.forwardBuildingProgressAd.Show();
                }
                break;
            default:
                Debug.Log("ShowRewardedAd: No Appropriate Ad Type!");
                break;
        }
    }

    public static void checkButtonLoadingStatus() {
        try {
            // Check OfflineCoins Ad
            if (Globals.Controller.Ads != null && Globals.UICanvas.uiElements.PopUpOfflineCoinsAdButtonX2 != null) {
                setButtonLoadingStatus(!Globals.Controller.Ads.isOfflineCoinsAdLoaded(), Globals.UICanvas.uiElements.PopUpOfflineCoinsAdButtonX2, false);
            } 
        } catch (Exception e) {
            Globals.UICanvas.DebugLabelAddText(e, true);
        }

        try {
            // Check Boost Ad
            if (Globals.Controller.Ads != null && Globals.UICanvas.uiElements.BoostPopUpAdButtonInvokeBoost != null) {
                setButtonLoadingStatus(!Globals.Controller.Ads.isBoostAdLoaded(), Globals.UICanvas.uiElements.BoostPopUpAdButtonInvokeBoost, false);
            }
        }
        catch (Exception e) {
            Globals.UICanvas.DebugLabelAddText(e, true);
        }

        try {
            // Check Forward Building Progress Ad
            if (Globals.Controller.Ads != null && Globals.UICanvas.uiElements.BuildingMenuAdButtonMinus30 != null) {
                setButtonLoadingStatus(!Globals.Controller.Ads.isForwardBuildingProgressAdLoaded(), Globals.UICanvas.uiElements.BuildingMenuAdButtonMinus30, false);
            }
        }
        catch (Exception e) {
            Globals.UICanvas.DebugLabelAddText(e, true);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="isLoading"></param>
    /// <param name="adButton"></param>
    public static void setButtonLoadingStatus(bool isLoading, GameObject adButton, bool maximum_reloads_reached) {
        try {
            if(adButton != null) {
                Debug.Log(Application.internetReachability);

                networkReachability = Application.internetReachability;

                adButton.GetComponent<Button>().interactable = (!isLoading && networkReachability != NetworkReachability.NotReachable);

                adButton.transform.Find("VideoSymbolLoading").gameObject.SetActive(isLoading && networkReachability != NetworkReachability.NotReachable);

                adButton.transform.Find("VideoInfoNoConnection").gameObject.SetActive(
                    !adButton.GetComponent<Button>().interactable &&
                    (maximum_reloads_reached || networkReachability == NetworkReachability.NotReachable));
            }

        } catch (Exception e) {
            Globals.UICanvas.DebugLabelAddText("ERROR Button State: " + e.ToString());
        }
    }

    /// <summary>
    /// Sets the OfflineCoins that will rewarded after watching the Ad
    /// </summary>
    public void setRewardingOfflineCoinAmount(IdleNum Amount) {
        rewardingOfflineCoins = Amount;
    }

    /// <summary>
    /// Sets the BuildingProgress that will rewarded after watching the Ad
    /// </summary>
    public void setRewardingBuilding(Building aBuilding) {
        rewardingBuilding = aBuilding;
    }

    /// <summary>
    /// Returns the ForwardBuildingProgressAd Loading status
    /// </summary>
    /// <returns></returns>
    public bool isForwardBuildingProgressAdLoaded() {
        return forwardBuildingProgressAd != null && forwardBuildingProgressAd.IsLoaded();
    }

    /// <summary>
    /// Returns the BoostAd Loading status
    /// </summary>
    /// <returns></returns>
    public bool isBoostAdLoaded() {
        return boostAd != null && boostAd.IsLoaded();
    }

    /// <summary>
    /// Returns the OfflineCoinsAd Loading status
    /// </summary>
    /// <returns></returns>
    public bool isOfflineCoinsAdLoaded() {
        return offlineCoinsAd != null && offlineCoinsAd.IsLoaded();
    }

}




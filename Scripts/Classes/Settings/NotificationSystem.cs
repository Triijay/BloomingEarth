using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Notifications.Android;
using UnityEngine;
using Random = System.Random;

public class NotificationSystem : MonoBehaviour {

    #region Vars

    /// <summary>
    /// Indicates if the Notifications was already sent to prevent double-sending
    /// </summary>
    static bool notificationsAlreadySent = false;

    #endregion


    /// <summary>
    /// Initiats the Timers for Notifications to get the Users back to our game
    /// </summary>
    public static void sendNotifications() {

        if (!notificationsAlreadySent) {

            Debug.Log("Sending Notifications");

            Random my_rnd = new Random();

            // Chance 1-X to get a Random Notification
            int randomChance = 6;
            int randomNumberRandomNotification = my_rnd.Next(1, randomChance + 1);

            if (randomNumberRandomNotification == 1) {
                // Random Shop Notification
                // 2-4h Random nur bei manchen Spiel-Öffnungen
                int randomNumberNotificationShop = my_rnd.Next(2 * 60 * 60, 4 * 60 * 60);
                Notification notificationShop = new Notification(
                    101,
                    Globals.Controller.Language.translateString("notifications_shop_header"),
                    Globals.Controller.Language.translateString("notifications_shop1"),
                    randomNumberNotificationShop,
                    Notification.NotificationChannels.Shop,
                    "{\"title\": \"Shop\", \"data\": \"101\"}"
                    );
            } else if (randomNumberRandomNotification == 2) {
                // Random Come Back Notification
                // 10-12h Random nur bei manchen Spiel-Öffnungen
                int randomNumberNotificationComeBack = my_rnd.Next(10 * 60 * 60, 12 * 60 * 60);
                Notification notificationComeBack = new Notification(
                    102,
                    Globals.Controller.Language.translateString("notifications_comeback_header"),
                    Globals.Controller.Language.translateString("notifications_comeback1"),
                    randomNumberNotificationComeBack,
                    Notification.NotificationChannels.DefaultBloomingEarth,
                    "{\"title\": \"Comeback\", \"data\": \"102\"}"
                    );
            }


            // Building is built
            if (Globals.Game.currentWorld.buildingInDelayUnlock != null && Globals.Game.currentWorld.buildingInDelayUnlock.getDelayUnlockProcessTimestampUntilUnlock() != new DateTime()) {
                Building buildingInProcess = Globals.Game.currentWorld.buildingInDelayUnlock;

                List<Building> buildingList = Globals.Game.currentWorld.buildingsProgressArray.ToList();
                int index = buildingList.FindIndex(a => a.Equals(buildingInProcess));

                // Calculate the time, how long the remaining DelayUnlock-Process Time is
                TimeSpan timeRemaining = buildingInProcess.getDelayUnlockProcessTimestampUntilUnlock() - DateTime.Now;

                Notification notificationBuildingReady = new Notification(
                    500 + index,
                    Globals.Controller.Language.translateString(
                        "notifications_building_ready_header",
                        new string[] { Globals.Controller.Language.translateString("building_possessiveCapital_name_" + buildingInProcess.getName()) }
                        ),
                    Globals.Controller.Language.translateString(
                        "notifications_building_ready1",
                        new string[] { Globals.Controller.Language.translateString("building_possessiveCapital_name_" + buildingInProcess.getName()) }
                        ),
                    (int)timeRemaining.TotalSeconds,
                    Notification.NotificationChannels.BuildingReady,
                    "{\"title\": \"Building\", \"data\": \"500" + index + "\"}"
                    );
            }


            // Rate Us Notification
            if ( (Globals.Game.currentUser.stats.GameOpeningsRaw == 20 || Globals.Game.currentUser.stats.GameOpeningsRaw == 50) && Globals.UserSettings.hasRated == false) {
                // Wenn der User schon mehrere Tage unser Spiel gespielt hat
                Notification notificationComeBack = new Notification(
                    201,
                    Globals.Controller.Language.translateString("notifications_rateus_header"),
                    Globals.Controller.Language.translateString("notifications_rateus1"),
                    2 * 60 * 60,
                    Notification.NotificationChannels.DefaultBloomingEarth,
                    "{\"title\": \"RateUs\", \"data\": \"201\"}"
                    );
            }



            // Test Notification Short Time (DO NOT RELEASE!)
            if (Globals.Game.initGame.testNotificationOn) {
                Notification notificationLongTermWeMissUTEST = new Notification(6301, "TEST TEST!", "Are you shitting me? Turn this Notification out!", 10);
            }


            // Treasure Full Notification
            Notification notificationTreasure = new Notification(
                301,
                Globals.Controller.Language.translateString("notifications_treasure_full_header"),
                Globals.Controller.Language.translateString("notifications_treasure_full1"),
                Globals.Game.initGame.limitOfflineCoinsHours * 60 * 60,
                Notification.NotificationChannels.TreasureFull
                );

            // Long Therm reactivation - after 1 Week "Deine Welt vermisst dich - Schau doch mal wieder rein"
            Notification notificationLongTermWeMissU = new Notification(
                401,
                Globals.Controller.Language.translateString("notifications_longtherm_header"),
                Globals.Controller.Language.translateString("notifications_longtherm1"),
                7 * 24 * 60 * 60
                );

            // Long Therm reactivation 2  - after 2 Weeks "Deine Welt vermisst dich - Schau doch mal wieder rein"
            Notification notificationLongTermWeMissU2 = new Notification(
                402,
                Globals.Controller.Language.translateString("notifications_longtherm_header"),
                Globals.Controller.Language.translateString("notifications_longtherm2"),
                14 * 24 * 60 * 60
                );

            // Long Therm reactivation 3  - after 4 Weeks "Deine Welt vermisst dich - Schau doch mal wieder rein"
            Notification notificationLongTermWeMissU3 = new Notification(
                403,
                Globals.Controller.Language.translateString("notifications_longtherm_header"),
                Globals.Controller.Language.translateString("notifications_longtherm3"),
                28 * 24 * 60 * 60
                );

            notificationsAlreadySent = true;

        }

    }


    /// <summary>
    /// Notifications will be cancelled and initiated
    /// </summary>
    public static void initNotifications() {

        try {

            // Cancel all displayed notifications if game is openend
            AndroidNotificationCenter.CancelAllDisplayedNotifications();

            string notificationChannelID;
            AndroidNotificationChannel notificationCenter;

            // Go Through all EnviStates
            foreach (Notification.NotificationChannels channel in Notification.getChannelsAsArray()) {

                // Notification Inits
                notificationChannelID = ((int)channel).ToString();

                // Delete old Notification Channel (because when game was Paused, it must not double the Channels!)
                AndroidNotificationCenter.DeleteNotificationChannel(notificationChannelID);

                // Init new Android Notification Channel
                notificationCenter = AndroidNotificationCenter.GetNotificationChannel(notificationChannelID);

                notificationCenter = new AndroidNotificationChannel() {
                    Id = notificationChannelID,
                    Name = Globals.Controller.Language.translateString(Notification.LanguageKeyNamePrefix + channel),
                    Importance = Importance.High,
                    Description = Globals.Controller.Language.translateString(Notification.LanguageKeyDescriptionPrefix + channel),
                };

                AndroidNotificationCenter.RegisterNotificationChannel(notificationCenter);

                //Globals.UICanvas.DebugLabelAddText(notificationChannelID, true);
                //Globals.UICanvas.DebugLabelAddText(notificationCenter.Name, true);

            }


            // Check if User started app with Notification
            var notificationIntentData = AndroidNotificationCenter.GetLastNotificationIntent();
            if (notificationIntentData != null && Globals.HelperFunctions.initialGameStart) {
                int id = notificationIntentData.Id;
                string channel = notificationIntentData.Channel;
                var notification = notificationIntentData.Notification;

                // Remove opened Notification on System-StatusBar
                AndroidNotificationCenter.CancelDisplayedNotification(notificationIntentData.Id);

                Globals.Controller.GPiOS.IncrementEventOnce(GPGSIds.event_opened_game_through_notification);
                Globals.Controller.Firebase.IncrementFirebaseEventOnce("opened_game_through_notification", "times");

                Globals.UICanvas.DebugLabelAddText("User Started App by Notification");

                // the IDs of the different Notifications can be found at InitGame->initReactivationNotifications
                if (id == 101) {
                    // Tested for notificationTreasure! 
                    // ToDo open the Shop
                    Globals.UICanvas.uiElements.ShopPopUp.SetActive(true);
                } else if (id == 201) {
                    // Rate Us was clicked
                    Globals.UICanvas.uiElements.PopUpFeedback.SetActive(true);
                } else if (id >= 500 && id <= 500) {
                    // Building is ready
                    // ToDo pan to new Building
                }

            }

            notificationsAlreadySent = false;

            // Cancel all Notifications that are Scheduled to not upset the User
            AndroidNotificationCenter.CancelAllScheduledNotifications();
        }
        catch (Exception e) {
            Globals.UICanvas.DebugLabelAddText("InitGame InitNotifications--" + e.ToString(), true);
        }


    }
}

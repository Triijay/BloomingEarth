using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Notifications.Android;
using UnityEngine;

class Notification {

    /// <summary>
    /// Headline of the Notification
    /// </summary>
    public string title;

    /// <summary>
    /// Text of Notification
    /// </summary>
    public string text;


    public string img;

    /// <summary>
    /// Senconds until the Notification will be executed to User
    /// </summary>
    public int secondsToSend;

    /// <summary>
    /// When the Notification is sent to Android (iOS?), we get an identifier<br></br>
    /// With this identifiert we can edit or remove the Notification from the System
    /// </summary>
    public int identifier;

    /// <summary>
    /// We can sent data together with the Notification<br></br>
    /// That data we can receive whan the User has opened out App with a click on that Notification -> we can give benefits or link to shop<br></br>
    /// eG "{\"title\": \"Notification 1\", \"data\": \"200\"}"
    /// </summary>
    public string intentData;

    /// <summary>
    /// in wich Channel the Notification should be posted
    /// </summary>
    public NotificationChannels notificationChannel;

    /// <summary>
    /// ChannelIDs and ChannelNames for Notifications<br></br>
    /// Look at App-Info at Smartphone, there you can see the Channels
    /// </summary>
    public enum NotificationChannels {
         DefaultBloomingEarth = 13372763,
         TreasureFull = 513375742,
         BuildingReady = 32133735,
         Shop = 723513377,
    }

    public const string LanguageKeyNamePrefix = "notification_channel_name_";
    public const string LanguageKeyDescriptionPrefix = "notification_channel_description_";

    // Vars
    AndroidNotification notification;

    /// <summary>
    /// Constructor for a Notification<br></br><br></br>
    /// </summary>
    /// <param name="title"></param>
    /// <param name="text"></param>
    /// <param name="secondsToSend"></param>
    /// <param name="channelToSend">Wich NotificationChannel the Notification is sent</param>
    /// <param name="intentData"></param>
    /// <param name="image"></param>
    public Notification(int id, string title, string text, int secondsToSend,  NotificationChannels channelToSend = NotificationChannels.DefaultBloomingEarth, string intentData = "", string image = "" ) {

        if (Globals.UserSettings.hasNotifications) {

            // TODO if Android / iOS

            notification = new AndroidNotification();
            notification.Title = this.title = title;
            notification.Text = this.text = text;
            notification.FireTime = System.DateTime.Now.AddSeconds(secondsToSend);
            this.secondsToSend = secondsToSend;

            notification.IntentData = this.intentData = intentData;
            notification.SmallIcon = "icon_1";
            notification.LargeIcon = "icon_0";
            identifier = id;

            img = image;
            //Globals.UICanvas.DebugLabelAddText(notification.ToString(), true);
            //Globals.UICanvas.DebugLabelAddText(notification.Title.ToString(), true);

            // Sent the Notification to Android System
            try {
                AndroidNotificationCenter.SendNotificationWithExplicitID(notification, ((int)channelToSend).ToString(), id);
            } catch (Exception e) {
                Globals.UICanvas.DebugLabelAddText(e.Message, true);
            }

            //Globals.UICanvas.DebugLabelAddText(notification.Title.ToString(), true);
            Debug.Log("Android Notification will be sent with ID " + identifier);
            Debug.Log("Android Notification will be sent in " + secondsToSend + " Seconds with ID " + identifier + "\nTitle: " + title + " -- Text: " + text + "\n\n");
        } else {
            Debug.Log("User turned off Notifications. No Notification was generated");
        }
    }

    /// <summary>
    /// Gets the ID of the current Notification
    /// </summary>
    /// <returns></returns>
    public int getIdentifier() {
        return identifier;
    }

    /// <summary>
    /// Returns the Existent Notification Channels as an Array of Notification.NotificationChannels
    /// </summary>
    /// <returns></returns>
    public static NotificationChannels[] getChannelsAsArray() {
        return (Notification.NotificationChannels[])System.Enum.GetValues(typeof(Notification.NotificationChannels));
    }
}


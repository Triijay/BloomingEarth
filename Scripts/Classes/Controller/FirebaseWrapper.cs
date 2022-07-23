using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Firebase;
using Firebase.Analytics;
using Firebase.Crashlytics;
using Firebase.Extensions;
using UnityEngine;
using Random = System.Random;

/// <summary>
/// Wrapper Class for Google Firebase<br></br>
/// Firebase runs on Android and iOS
/// </summary>
public class FirebaseWrapper {

    /// <summary>
    /// FirebaseApp
    /// </summary>
    FirebaseApp app;

    /// <summary>
    /// Is Firebase completely initialized?
    /// </summary>
    bool firebaseInitialized = false;

    /// <summary>
    /// unique User ID
    /// </summary>
    string userID = "";

    /// <summary>
    /// Storage for Events that are Called before connecting to Firebase. After connect, they will be called again
    /// </summary>
    private static List<KeyValuePair<string, object>> IncrementEventsAfterActivate = new List<KeyValuePair<string, object>>();


    // Handle initialization of the necessary firebase modules:
    public void InitializeFirebase() {
        try {

            app = FirebaseApp.DefaultInstance;
            Debug.Log(app.Name);

            Debug.Log("Enabling data collection.");
            FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);

            Debug.Log("Set user properties.");
            // Set the user's sign up method.
            FirebaseAnalytics.SetUserProperty(
              FirebaseAnalytics.UserPropertySignUpMethod,
              "Google");


            // Set the user ID.
            userID = getOrGenerateUID();
            FirebaseAnalytics.SetUserId(userID);

            //
            //triggerFirebaseAfterActivateTests();

            // Add the user ID to Crashlytics Report
            Crashlytics.SetUserId(Globals.Game.currentUser.userID);

            // Set default session duration values.
            FirebaseAnalytics.SetSessionTimeoutDuration(new TimeSpan(0, 30, 0));

            firebaseInitialized = true;
            if (LoadingScreen.loadingScreenIsActive) {
                LoadingScreen.setFirebaseInitialized();
            }
            IncrementFirebaseEventOnce("user_firebase_initialized");

            // Resent Events if stored
            if (IncrementEventsAfterActivate != null) {
                foreach (KeyValuePair<string, object> EventToSentPair in IncrementEventsAfterActivate) {
                    if(EventToSentPair.Value is string) {
                        //Globals.UICanvas.DebugLabelAddText("ResendingEvent " + EventToSentPair.Key + " with value " + EventToSentPair.Value + ".");
                        IncrementFirebaseEventOnce(EventToSentPair.Key, EventToSentPair.Value.ToString());
                        //Globals.UICanvas.DebugLabelAddText(EventToSentPair.Key + " - event resent");
                    } else {
                        IncrementFirebaseEventWithParameters(EventToSentPair.Key, (Parameter[])EventToSentPair.Value);
                    }
                }
                IncrementEventsAfterActivate.Clear();
            }
        }
        catch (Exception e) {
            Globals.UICanvas.DebugLabelAddText(e, true);
        }
    }

    /// <summary>
    /// Increments a Tracking Event on one<br></br>
    /// First Param is: Our Name for the Firebase-Event (we can choose it by ourselfes)<br></br>
    /// Param Name is not really relevant
    /// </summary>
    /// <param name="eventName">Our Name for the Firebase-Event</param>
    /// <param name="eventParamName">What param should be displayed in Firebase, eg Times, Hours, Level ...</param>
    public void IncrementFirebaseEventOnce(string eventName, string eventParamName = "times") {
        if (firebaseInitialized) {
            try {
                Globals.UICanvas.DebugLabelAddText("FireBaseEvent triggered: " + eventName);
                FirebaseAnalytics.LogEvent(eventName, eventParamName, 1);
            } 
            catch (Exception e) {
                Globals.UICanvas.DebugLabelAddText(e, true);
            }
        } else {
            Globals.UICanvas.DebugLabelAddText("FireBaseEvent for Later: " + eventName +"," + eventParamName);
            // If Firebase not Initialized, Store Events for Later
            IncrementEventsAfterActivate.Add(new KeyValuePair<string, object>(eventName, eventParamName));
        }
    }


    /// <summary>
    /// Trigger a Firebase Event with Parameters -> Can be KeyValuePair<string, object> and will be converted to Param[]
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="paramList"></param>
    public void IncrementFirebaseEventWithParameters(string eventName, KeyValuePair<string, object>[] paramList) {
        // Create Firebase Parameter List from KeyValuePair Array
        Parameter[] parameters = CreateParameterList(paramList, paramList.Count());

        IncrementFirebaseEventWithParameters(eventName, parameters);
    }

    /// <summary>
    /// Trigger a Firebase Event with Parameters
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="paramList"></param>
    public void IncrementFirebaseEventWithParameters(string eventName, Parameter[] paramList) {
        if (firebaseInitialized) {
            try {
                Globals.UICanvas.DebugLabelAddText("FireBaseEvent triggered: " + eventName);
                FirebaseAnalytics.LogEvent(eventName, paramList);
            }
            catch (Exception e) {
                Globals.UICanvas.DebugLabelAddText(e, true);
            }

        } else {
            Globals.UICanvas.DebugLabelAddText("FireBaseEvent for Later: " + eventName);
            // If Firebase not Initialized, Store Events for Later
            IncrementEventsAfterActivate.Add(new KeyValuePair<string, object>(eventName, paramList));
        }
    }

    /// <summary>
    /// Converts a KeyValuePair<string, object> to a Param[]
    /// </summary>
    /// <param name="paramList"></param>
    /// <param name="paramCount"></param>
    /// <returns>ParameterList for Firebase</returns>
    public Parameter[] CreateParameterList(KeyValuePair<string, object>[] paramList, int paramCount) {
        Parameter[] Parameters = new Parameter[paramCount];
        int cnt = 0;

        try {
            foreach (KeyValuePair<string, object> valuePair in paramList) {

                if (valuePair.Value is string) {
                    Parameters[cnt] = new Parameter(valuePair.Key, valuePair.Value.ToString());
                } else {
                    Parameters[cnt] = new Parameter(valuePair.Key, Convert.ToDouble(valuePair.Value));
                } 
            
                cnt++;
            }
        } catch (Exception e) {
            Globals.UICanvas.DebugLabelAddText(e, true);
        }

        return Parameters;
    }


    /// <summary>
    /// Generates a unique User ID
    /// </summary>
    private string getOrGenerateUID() {

        if (Globals.Game.currentUser.userID == "") {

            // Generate new UserID
            Random my_rnd = new Random();

            // Timestamp + Random Number
            userID = Math.Abs(System.DateTime.Now.ToBinary()).ToString("X4") + my_rnd.Next(10000, 99999) + my_rnd.Next(10000, 99999).ToString("X2") + "x" + my_rnd.Next(10000, 99999).GetHashCode();
            
            // Save New userID to Globals User
            Globals.Game.currentUser.userID = userID;

        } else {
            // User has UserID
            userID = Globals.Game.currentUser.userID;
        }

        // Debug UserID into AdmLabel
        Globals.UICanvas.DebugLabelAddText("UserID: " + userID);

        return userID;

    }


    /// <summary>
    /// With this function you can test the Event-Triggering if Firebase is not activated yet<br></br>
    /// The Events should be stored and fired later, when Firebase is fully Initialized
    /// </summary>
    public void triggerFirebaseAfterActivateTests() {
        IncrementFirebaseEventOnce("firebase_notinitialized");
        // Log Firebase Event
        KeyValuePair<string, object>[] valuePairArray = {
            new KeyValuePair<string, object>("rrrr", 2),
            new KeyValuePair<string, object>("World", Globals.Game.currentWorld.worldName),
            new KeyValuePair<string, object>("FirstGameStart", Globals.Game.currentUser.stats.FirstGameLoad.ToString()),
            new KeyValuePair<string, object>("DaysWithGameOpening", Globals.Game.currentUser.stats.DaysWithGameOpening),
            new KeyValuePair<string, object>("MinutesPlayedOverall", (int)(Globals.Game.currentUser.stats.SecondsPlayedOverall + Time.time)/60),
        };
        Globals.Controller.Firebase.IncrementFirebaseEventWithParameters(
            "firebase_notinitialized2", valuePairArray);
    }


    /// <summary>
    /// Returns the Firebase UserID
    /// </summary>
    /// <returns></returns>
    public string getUserID() {
        return userID;
    }
}


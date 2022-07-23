using Firebase.Analytics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Firebase Test --> TAKES AROUND 30 SECONDS
/// </summary>
public class FirebaseTester : MonoBehaviour {


    private void Awake() {
        StartCoroutine(InitFirebaseTest()); 
    }

    private IEnumerator InitFirebaseTest() {
        yield return new WaitForSeconds(15);

        StartFirebaseTest();
    }



    private void StartFirebaseTest() {

        Globals.UICanvas.DebugLabelAddText("Starting Firebase Test");

        Globals.UICanvas.DebugLabelAddText("Sending");

        // Test 1 direct
        FirebaseAnalytics.LogEvent("ztest_1", "testparam", 1);

        // Test 2 direct
        Parameter[] LevelUpParameters = {
            new Parameter(
                "test", 2),
            new Parameter(
                "World", Globals.Game.currentWorld.gameObject.name),
            new Parameter(
                "FirstGameStart", Globals.Game.currentUser.stats.FirstGameLoad.ToString()),
            new Parameter(
                "DaysWithGameOpening", Globals.Game.currentUser.stats.DaysWithGameOpening),
            new Parameter(
                "MinutesPlayedOverall", (int)(Globals.Game.currentUser.stats.SecondsPlayedOverall + Time.time)/60),
        };

        FirebaseAnalytics.LogEvent(
                  "ztest_2",
                  LevelUpParameters);

        // Test 3 over our function
        Globals.Controller.Firebase.IncrementFirebaseEventOnce("ztest_3");

        // Test 4 over our function
        KeyValuePair<string, object>[] valuePairArray = {
            new KeyValuePair<string, object>("test", 3),
            new KeyValuePair<string, object>("World", Globals.Game.currentWorld.worldName),
            new KeyValuePair<string, object>("FirstGameStart", Globals.Game.currentUser.stats.FirstGameLoad.ToString()),
            new KeyValuePair<string, object>("DaysWithGameOpening", Globals.Game.currentUser.stats.DaysWithGameOpening),
            new KeyValuePair<string, object>("MinutesPlayedOverall", (int)(Globals.Game.currentUser.stats.SecondsPlayedOverall + Time.time)/60),
        };

        Globals.Controller.Firebase.IncrementFirebaseEventWithParameters(
        "ztest_4", valuePairArray);

    }

}

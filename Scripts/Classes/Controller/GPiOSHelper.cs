using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPiOSHelper : MonoBehaviour {

    public void triggerAchievementsArray(string[] achievementsToProcess) {
        StartCoroutine(processAchievementsArray(achievementsToProcess));
    }

    IEnumerator processAchievementsArray(string[] achievementsToProcess) {

        foreach (string achievementID in achievementsToProcess) {
            Globals.Controller.GPiOS.sentAchievement100Percent(achievementID);
            yield return new WaitForSeconds(4);
        }

        yield return null;
    }

}

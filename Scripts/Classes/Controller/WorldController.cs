using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldController : MonoBehaviour {


    public Animator levelChangeTransition;
    public CanvasGroup LoadingSceneLoadingCircle;

    public async void changeWorld(string worldName) {

        // Set currentworld to be not current anymore
        Globals.Game.currentWorld.isCurrentWorld = false;

        // SaveGame current World to Disk
        if (!Globals.KaloaSettings.preventSaving) {
            await SavingSystem.saveGameData(false);
        }

        // Stop the current running Coroutines
        Globals.HelperFunctions.stopAllRunningCoroutines();

        // Set Status from scene change to true
        Globals.Game.isSceneChanging = true;

        StartCoroutine(animateToScene(worldName));
    }

    /// <summary>
    /// Animates to the MainScene
    /// </summary>
    private IEnumerator animateToScene(string worldName) {

        LoadingSceneLoadingCircle.alpha = 1;

        // Start Animation
        levelChangeTransition.SetTrigger("StartShowMainscene");

        yield return new WaitForSeconds(2f);

        // Set current World
        Globals.Game.saveGame.currentWorldName = worldName;

        // Load desired Scene
        SceneManager.LoadScene("WorldScene_" + worldName);

        yield return new WaitForSeconds(1f);

        // Stop the Coroutines of WorldController.cs
        StopAllCoroutines();
        yield return null;
    }


}

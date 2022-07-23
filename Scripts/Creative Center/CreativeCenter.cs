using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Center to make cool gifs
/// </summary>
public class CreativeCenter : MonoBehaviour {


    public Transform PictureCamera;
    public Transform CreativeCamera;
    public Transform CreativeProjects;


    /// <summary>
    /// If true, Creative Center is enabled
    /// </summary>
    [Tooltip("If true, Creative Center is enabled")]
    public bool letsBeCreative = false;

    /// <summary>
    /// If true, we can Render Pictures- if false, we make Gifs
    /// </summary>
    [Tooltip("If true, we cam Render Pictures- if false, we make Gifs")]
    public bool letsMakePictures = false;

    public Transform creativeProject;
    private GameObject creativeProjectGameObject;



    public bool displayAllBuildingUpgrades = false;
    public bool displaySigns = false;
    public bool displayTapCoins = false;
    [Range(-360, 360)]
    public int enviValLiveUpdated = 360;


    // Start is called before the first frame update
    void Start() {

        if (letsBeCreative) {
            // CreativeCenter is ON

            // Camera Management
            Globals.UICanvas.uiElements.MainCamera.SetActive(false);
            Globals.UICanvas.uiElements.uiCamera.SetActive(false);

            // Settings for GameStart
            Globals.KaloaSettings.preventPlayfabCommunication = true;
            Globals.KaloaSettings.preventIAPCommunication = true;
            Globals.KaloaSettings.preventGoogleCommunication = true;
            Globals.KaloaSettings.preventSaving = true;
            Globals.KaloaSettings.skipTutorial = true;

            // Delete all PlayerPrefs and start a new Game
            SavingSystem.saveOrLoadPlayfab = false;
            Globals.Game.initGame.resetGameForAdmins();

            // Buildings
            foreach (Building b in Globals.Game.currentWorld.buildingsProgressArray) {
                b.gameObject.SetActive(true);
                b.gameObject.transform.Find("LevelUpIndicator").gameObject.SetActive(false);
                b.gameObject.transform.Find("Sign").gameObject.SetActive(displaySigns);
                b.changeConstructionPhase(Building.ConstructionPhases.Finished);
                if (displayAllBuildingUpgrades) {
                    b.levelUp(10000, true);
                } else if (displayTapCoins) {
                    b.levelUp(1, true);
                }
                if (b as HybridBuilding) {
                    ((HybridBuilding)b).gameObject.transform.Find("TapCoin").gameObject.SetActive(displayTapCoins);
                }
            }


            if (letsMakePictures) {
                // We want to make Renderings
                PictureCamera.gameObject.SetActive(true);
                CreativeCamera.gameObject.SetActive(false);
                CreativeProjects.gameObject.SetActive(false);
            } else {

                // We want to make GIFS
                PictureCamera.gameObject.SetActive(false);
                CreativeCamera.gameObject.SetActive(true);
                CreativeProjects.gameObject.SetActive(true);



                if (creativeProject != null) {
                    creativeProjectGameObject = creativeProject.gameObject;

                    // Set All Projects to invisible
                    foreach (Transform transform in CreativeProjects) {
                        if (transform.name == creativeProjectGameObject.name) {
                            creativeProjectGameObject.SetActive(true);
                        } else {
                            transform.gameObject.SetActive(false);
                        }
                    }

                    // Execute Creative

                } else {
                    Debug.LogError("CreativeCenter: Please set the right CreativeName, if you want to display a specific creative!");
                }
            }
        }
        
    }


    private void Update() {
        // Variable Environment
        Globals.Game.currentWorld.enviGlass.enviValue = enviValLiveUpdated;
        Globals.Game.currentWorld.enviGlass.transformNeedle();
    }

}

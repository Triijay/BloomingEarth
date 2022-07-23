using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Notifications.Android;
using UnityEditorInternal.VersionControl;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Tests {
    public class TestSuiteUI {

        public InitGame Game;

        [UnitySetUp]
        public IEnumerator UnitySetUp() {
            // TestSettings
            Globals.KaloaSettings.preventPlayfabCommunication = true;
            Globals.KaloaSettings.preventIAPCommunication = true;
            Globals.KaloaSettings.preventGoogleCommunication = true;
            Globals.KaloaSettings.preventSaving = true;
            Globals.KaloaSettings.skipTutorial = true;

            // Ensure that all components will be loaded
            Globals.Game.isGameStarting = true;

            // Load the MainScene
            SceneManager.LoadScene("WorldScene_Village1");

            // Wait one Frame until Scene is loaded
            yield return null;

            // Shut Up Google Sign In
            PlayerPrefs.SetString("global_settings_wasSignedIn", "false"); PlayerPrefs.SetString("global_stat_firstGameLoad", "222");

            // Get Game-Object and Init the Game
            Game = GameObject.Find("/_BaseObjects/InitGame").GetComponent<InitGame>();

            // Wait for one Frame until Component is loaded
            yield return null;

            // Delete all PlayerPrefs and start a new Game
            SavingSystem.saveOrLoadPlayfab = false;
            Game.resetGameForAdmins();

            Globals.UICanvas.uiElements.PopUpQuests.SetActive(false);

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown() {
            // Destroy the GameObject to not affect other tests
            Object.Destroy(Game.gameObject);
            // Reset outside communication
            Globals.KaloaSettings.preventPlayfabCommunication = false;
            Globals.KaloaSettings.preventIAPCommunication = false;
            Globals.KaloaSettings.preventGoogleCommunication = false;
            Globals.KaloaSettings.preventSaving = false;
            Globals.KaloaSettings.skipTutorial = false;

            yield return null;
        }







        // Test for: Tests are Running properly
        // -> This Test must always be true
        [UnityTest]
        public IEnumerator CheckAreWorkingProperly() {
            // Test Assert
            Assert.AreEqual(1, 1);
            yield return null;
        }



        // Test for: Important UI Elements are there
        [UnityTest]
        public IEnumerator CheckImportantUIElementsExistAndNotRenamed() {

            Assert.IsNotNull(GameObject.Find("CanvasMain"));
            Assert.IsNotNull(GameObject.Find("InitGame"));
            Assert.IsNotNull(GameObject.Find("Sounds"));
            Assert.IsNotNull(GameObject.Find("Items"));
            Assert.IsNotNull(GameObject.Find("Cameras"));
            Assert.IsNotNull(GameObject.Find("World"));
            Assert.IsNotNull(GameObject.Find("AdWrapper"));
            Assert.IsNotNull(GameObject.Find("LoadingScreenCrossfade"));

            yield return null;
        }



        // Test for: Admin Stuff inactive on start
        [UnityTest]
        public IEnumerator CheckAdminStuffInactive() {

            // Debug Label
            Assert.IsFalse(Globals.UICanvas.uiElements.DebugLabelObj.activeSelf, "Warning: Admin DebugLabel will be seen by Users!");
            

            // Creative Center
            Assert.IsFalse(Globals.UICanvas.uiElements.CreativeCenter.GetComponent<CreativeCenter>().letsBeCreative, "Warning: CreativeCenter.letsBeCreative MUST be unchecked");
            Assert.IsFalse(Globals.UICanvas.uiElements.CreativeCenter.activeSelf, "Critical Warning: CreativeCenter.letsBeCreative is unchecked, but CreativeCenter is still active!");

            yield return null;
        }


        // Test For: Check Boost Battery Scale
        [UnityTest]
        public IEnumerator CheckBoostBatteryScale() {

            // Set Boost PopUp active
            Globals.UICanvas.uiElements.BoostPopUp.SetActive(true);

            yield return null;

            // BoostBattery must be 0, when no Boost is active
            //Assert.AreEqual(0, Globals.UICanvas.uiElements.BoostBatteryStatus.transform.localScale.y, "BoostMenuBattery should be scale 0");
            Assert.AreEqual(0, Globals.UICanvas.uiElements.BoostPopUpBatteryStatus.transform.localScale.y, "BoostMenuBattery in PopUp should be scale 0");

            Globals.Game.currentWorld.CoinIncomeManager.adBoostTimer.activateBoost(2, 1000);

            yield return new WaitForSeconds(1.5f);

            // BoostBattery must be over 0, when Boost is active
            //Assert.Greater(Globals.UICanvas.uiElements.BoostBatteryStatus.transform.localScale.y, 0, "BoostMenuBattery should be scale > 0");
            Assert.Greater(Globals.UICanvas.uiElements.BoostPopUpBatteryStatus.transform.localScale.y, 0, "BoostMenuBattery in PopUp should be scale > 0");

            yield return null;
        }


        // Test for: Coins in Label
        [UnityTest]
        public IEnumerator CheckCoinsLikeLabel() {

            yield return null;
            TMPro.TextMeshProUGUI coinLabel = GameObject.Find("MainCoinDisplay").GetComponent<TMPro.TextMeshProUGUI>();

            // Check if OfflineCoins are loaded
            Assert.IsNotNull(coinLabel, "Coinlabel not found");

            yield return null;

            // Get Coins and Label Coins
            IdleNum coinsOnStart = Globals.Game.currentWorld.getCoins();
            string coinsFromLabel = coinLabel.text;

            Debug.Log("TestSuite: Coins from Label: " + coinsFromLabel);
            Debug.Log("TestSuite: Coins from Globals: " + Globals.Game.currentWorld.getCoins().toRoundedString());

            Assert.AreEqual(coinsFromLabel, Globals.Game.currentWorld.getCoins().toRoundedString(), "The Coins in saved CurrentWorld's Bank Account are not the same as the User sees in UI");

            yield return null;

        }

        // Test for: Check all Buttons on Canvas have a onClickListener
        [UnityTest]
        public IEnumerator CheckAllButtonsHaveFunction() {
            Button[] buttonsInCanvas = Globals.UICanvas.uiElements.Canvas.GetComponentsInChildren<Button>(true);

            foreach (Button buttonInCanvas in buttonsInCanvas) {

                try {
                    if (buttonInCanvas.transform.parent.name + "/" + buttonInCanvas.transform.name == "ButtonGroupOfflineCoins/Button_OK" ||
                        buttonInCanvas.transform.parent.name + "/" + buttonInCanvas.transform.name == "ButtonGroupOfflineCoins/AdButton_X2" ||
                        buttonInCanvas.transform.name == "InventoryItemTemplate"
                        ) {
                        // These Buttons will get their EventListeners via Script and should not be tested
                        continue;
                    }
                } catch { }


                Assert.GreaterOrEqual(buttonInCanvas.onClick.GetPersistentEventCount(), 1, "Button " + buttonInCanvas.transform.parent.name + "/" + buttonInCanvas.transform.name + " has no onClick Event");
            }

            yield return null;
        }


        // Test for: Code convention - Every Button should have a maximum of 1 Listener
        [UnityTest]
        public IEnumerator CheckAllButtonsHasOnlyOneListener() {

            Button[] buttons = Globals.UICanvas.uiElements.Canvas.GetComponentsInChildren<Button>(true);

            foreach (Button buttonInCanvas in buttons) {
                if (
                    buttonInCanvas.transform.parent.name + "/" + buttonInCanvas.transform.name == "PopUpOfflineCoins/Button_OK" ||
                    buttonInCanvas.transform.parent.name + "/" + buttonInCanvas.transform.name == "PopUpOfflineCoins/AdButton_X2"
                    ) {
                    // These Buttons will get their EventListeners via Script and should not be tested
                    Assert.LessOrEqual(buttonInCanvas.onClick.GetPersistentEventCount(), 0,
                        "Button '" + buttonInCanvas.transform.parent.name + "/" + buttonInCanvas.transform.name + "' has more than one Listener. Thats deprecated.");
                } else if (
                    buttonInCanvas.transform.parent.name + "/" + buttonInCanvas.transform.name == "BuildingMenu/Button_Close"
                    ) {
                    Assert.LessOrEqual(buttonInCanvas.onClick.GetPersistentEventCount(), 2, 
                        "Button '" + buttonInCanvas.transform.parent.name + "/" + buttonInCanvas.transform.name + "' has more than one Listener. Thats deprecated.");
                } else {
                    Assert.LessOrEqual(buttonInCanvas.onClick.GetPersistentEventCount(), 1, 
                        "Button '" + buttonInCanvas.transform.parent.name + "/" + buttonInCanvas.transform.name + "' has more than one Listener. Thats deprecated.");
                }


            }

            yield return null;
        }


        // Test for: Achievement Deer
        [UnityTest]
        public IEnumerator CheckAchievementDeerWorking() {

            string achievementObjectname = "Deco/Forest/DeerAchievement";

            GameObject deer = Globals.Game.currentWorld.gameObject.transform.Find(achievementObjectname).gameObject;
            Assert.IsNotNull(deer, "No GameObject in CurrentWorld called " + achievementObjectname+" - Achievement will not work or is not the equal to the real code");

            yield return null;
        }

        
        // Test for: exactly Two AudioListener (Game + CreativeCenter)
        [UnityTest]
        public IEnumerator CheckTwoAudioListener() {

            int audioListers = 0;

            for (int i = 0; i < SceneManager.sceneCount; i++) {
                // In each scene there have to be one AudioListener
                audioListers = 0;
                GameObject[] rootObjs = SceneManager.GetSceneAt(i).GetRootGameObjects();
                foreach (GameObject obj in rootObjs) {
                    if (obj.GetComponent<AudioListener>() != null || obj.GetComponentInChildren<AudioListener>() != null) {
                        audioListers++;
                    }
                }
                Assert.AreEqual(2, audioListers);
            }


            yield return null;
        }



        [UnityTest]
        public IEnumerator TestEnviGlassNeedleWorking() {

            // No Building is affecting -> EnviNeedle should be exactly in the middle
            GameObject needle = Globals.UICanvas.uiElements.enviNeedle;
            Assert.IsNotNull(needle, "There is no valid EnviNeedle-GameObject");

            // Check that the Position without PlayerPrefs is exactly in the Middle of the EnviGlass
            Assert.AreEqual(0, needle.transform.localPosition.x);

            // Positive EnviAffection -> Needle rotates to right (z=negative)
            Globals.Game.currentWorld.enviGlass.addAffector(new Affector<float>("testaffector1", 5));
            Assert.AreNotEqual(0, needle.transform.localPosition.x, "EnviNeedle not moved");
            Assert.Less(0, needle.transform.localPosition.x, "EnviNeedle moved in the false direction");
            Assert.Less(needle.transform.localPosition.x, 10, "EnviNeedle moved too much");

            // Negative EnviAffection -> Needle rotates to left (z=positive)
            Globals.Game.currentWorld.enviGlass.addAffector(new Affector<float>("testaffector2", -10));
            Assert.Less(needle.transform.localPosition.x, 0, "EnviNeedle is still Positive");
            Assert.Less(-10, needle.transform.localPosition.x, "EnviNeedle moved too much");

            RectTransform rt = (RectTransform)Globals.UICanvas.uiElements.enviGlassContainer.transform;
            float glassValueToTransformMAX = EnviGlass.GLASS_VALUE_MAX * rt.rect.max.x * 0.75f / EnviGlass.GLASS_VALUE_MAX;

            // Positive EnviAffection with extreme Value should be capped
            Globals.Game.currentWorld.enviGlass.addAffector(new Affector<float>("testaffector3", 50000));
            Assert.AreEqual(glassValueToTransformMAX, needle.transform.localPosition.x);

            // Negative EnviAffection with extreme Value should be capped
            Globals.Game.currentWorld.enviGlass.addAffector(new Affector<float>("testaffector4", -100000));
            Assert.AreEqual(-glassValueToTransformMAX, needle.transform.localPosition.x);

            yield return null;
        }

        [UnityTest]
        public IEnumerator TestEnviGlassPopUpTextsExist() {


            // Go Through all EnviStates
            foreach (EnviGlass.EnviStates enviState in (EnviGlass.EnviStates[]) System.Enum.GetValues(typeof(EnviGlass.EnviStates))) {

                // Check in each Language
                foreach (KeyValuePair<string, string> lang in Language.supportedLanguages) {

                    Globals.Controller.Language.setLanguage(lang.Key);
                    Hashtable fileOtherLang = Globals.Controller.Language.getXmlFileStrings();

                    // TODO Images are there

                    // Check if necessary LangKeys exists
                    Assert.IsTrue(fileOtherLang.ContainsKey("enviglass_status_" + enviState.ToString()));
                    Assert.IsTrue(fileOtherLang.ContainsKey("enviglass_status_consequences_" + enviState.ToString()));
                    Assert.IsTrue(fileOtherLang.ContainsKey("enviglass_tips_" + enviState.ToString()));

                    yield return null;

                }

            }

            yield return null;
        }

        // Test for: MiniGame Opens at click on Squirrel
        [UnityTest]
        public IEnumerator CheckMiniGameOpensAtClickOnSquirrel() {

            Assert.IsNotNull(Globals.Game.initGame.GetComponent<CatchTheNuts>());

            Button buttonSquirrel = Globals.Game.initGame.GetComponent<CatchTheNuts>().Squirrel.GetComponent<Button>();

            buttonSquirrel.onClick.Invoke();

            yield return null;

            Assert.IsTrue(Globals.UICanvas.uiElements.MiniGamePopUp);

            yield return null;
        }


        // Test for: PopUpBG closes all PopUps
        [UnityTest]
        public IEnumerator CheckPopUpBGclosesAllPopUps() {

            Globals.UICanvas.uiElements.PopUpBG.SetActive(true);

            Globals.UICanvas.uiElements.PopUpEnviGlass.SetActive(true);

            Assert.IsNotNull(Globals.UICanvas.uiElements.PopUpBG.GetComponent<Button>());

            Globals.UICanvas.uiElements.PopUpBG.GetComponent<Button>().onClick.Invoke();

            yield return null;

            Assert.IsFalse(Globals.UICanvas.uiElements.PopUpEnviGlass.activeSelf);

            yield return null;
        }

        // Test for: Every AdButton must have specific Children Elements to work properly
        [UnityTest]
        public IEnumerator CheckAllAdButtonsHasImportantChildrenElements() {

            Button[] buttons = Globals.UICanvas.uiElements.Canvas.GetComponentsInChildren<Button>(true);
            string buttonName;

            foreach (Button buttonInCanvas in buttons) {
                buttonName = buttonInCanvas.transform.name;
                if (buttonName.StartsWith("AdButton") ) {
                    Assert.IsNotNull(buttonInCanvas.gameObject.transform.Find("VideoSymbol"), buttonName + " has no VideoSymbol");
                    Assert.IsNotNull(buttonInCanvas.gameObject.transform.Find("VideoSymbolLoading"), buttonName + " has no VideoSymbolLoading");
                    Assert.IsNotNull(buttonInCanvas.gameObject.transform.Find("VideoInfoNoConnection"), buttonName + " has no VideoInfoNoConnection");
                }
            }

            yield return null;
        }

        // Test for: Sounds
        [UnityTest]
        public IEnumerator TestSoundsPlaying() {

            Globals.UserSettings.hasSound = true;

            // Test a Safely-There-And-Will-Not-Be-Renamed Sound
            Globals.Controller.Sound.PlaySound("ClosePanel");
            LogAssert.Expect(LogType.Log, "Sound ClosePanel played.");

            // Test a Safely-Never-A-Sound-In-Our-Game Sound
            Globals.Controller.Sound.PlaySound("LustigesEinhornFurztAusversehen");
            LogAssert.Expect(LogType.Error, "No Audio Source in GameObject Sounds with Name LustigesEinhornFurztAusversehen");

            // Test all Sounds that are in The SoundWrapper
            foreach (string SoundName in Globals.Controller.Sound.getAudioFilesAsStringArray()) {
                Globals.Controller.Sound.PlaySound(SoundName);
                LogAssert.Expect(LogType.Log, "Sound " + SoundName + " played.");
                yield return new WaitForSeconds(0.5f);
            }

            yield return null;
        }

        // Test for: Notifications
        [UnityTest]
        public IEnumerator TestNotifications() {

            Globals.UserSettings.hasNotifications = true;

            // Building Notification: When Building is not ready when game closes, the Notification of the Building has to be sent
            Game.adminUnlockProcessSec = 200;
            Globals.Game.currentWorld.buildingsProgressArray[0].levelUp(1);

            // Check Notifications
            NotificationSystem.sendNotifications();
            LogAssert.Expect(LogType.Log, "Sending Notifications");
            // Building Notification
            LogAssert.Expect(LogType.Log, "Android Notification will be sent with ID 500");
            // Standard Notifications
            LogAssert.Expect(LogType.Log, "Android Notification will be sent with ID 301");
            LogAssert.Expect(LogType.Log, "Android Notification will be sent with ID 401");
            LogAssert.Expect(LogType.Log, "Android Notification will be sent with ID 402");
            LogAssert.Expect(LogType.Log, "Android Notification will be sent with ID 403");



            yield return null;
        }

        // Test for: TextMesh and TextMesh U GUI has different Materials
        // when this Test fails, there will be some devices, where one System fails: No Sign Letters or No GUI Letters
        [UnityTest]
        public IEnumerator PreventTextMeshProNotVisibleBug() {

            TMPro.TextMeshPro[] proObjects = Resources.FindObjectsOfTypeAll(typeof(TMPro.TextMeshPro)) as TMPro.TextMeshPro[];
            Debug.Log("Found " + proObjects.Length + " TextMeshPro instances with this script attached");
            TMPro.TextMeshProUGUI[] proGUIObjects = Resources.FindObjectsOfTypeAll(typeof(TMPro.TextMeshProUGUI)) as TMPro.TextMeshProUGUI[];
            Debug.Log("Found " + proGUIObjects.Length + " TextMeshProUGUI instances with this script attached");

            Transform parent;


            foreach (TMPro.TextMeshPro proObject in proObjects) {
                Debug.Log(proObject.gameObject.transform.parent.name + "/" + proObject.gameObject.name);
                var mr = proObject.gameObject.GetComponent<MeshRenderer>();
                foreach (TMPro.TextMeshProUGUI proGUIObject in proGUIObjects) {
                    Assert.IsNotNull(mr.sharedMaterials[0], proObject.gameObject.transform.parent.name + "/" + proObject.gameObject.name);
                    parent = proGUIObject.gameObject.transform.parent;
                    if (proGUIObject.materialForRendering != null) {
                        Assert.IsNotNull(proGUIObject.materialForRendering, proObject.gameObject.transform.parent.name + "/" + proObject.gameObject.name);

                        Assert.AreNotEqual(mr.sharedMaterials[0].name, proGUIObject.materialForRendering.name,
                            proObject.gameObject.transform.parent.name + "/" + proObject.gameObject.name +" material == " +
                            proGUIObject.gameObject.transform.parent.name + "/" + proGUIObject.gameObject.name +" material " +
                            " => CRITICAL:TMPro(UI) must use the Default Liberation Sans, TMPro InGameLabels must use the Material Preset 'LiberationSans SDF - InGame'! (Placed in our UnityMats Folder)");
                    }
                }
            }

            yield return null;
        }


        // Test for: CatchTheNuts after Start
        [UnityTest]
        public IEnumerator TestCatchTheNuts() {

            Assert.IsFalse(Globals.UICanvas.uiElements.InitGameObject.GetComponent<CatchTheNuts>().Squirrel.activeSelf);
            Assert.IsFalse(Globals.UICanvas.uiElements.InitGameObject.GetComponent<CatchTheNuts>().PopUpStart.activeSelf);
            Assert.IsFalse(Globals.UICanvas.uiElements.InitGameObject.GetComponent<CatchTheNuts>().PopUpFinished.activeSelf); 
            Assert.IsFalse(Globals.UICanvas.uiElements.InitGameObject.GetComponent<CatchTheNuts>().PopUpClaimButton.GetComponent<Button>().interactable);

            yield return null;
        }

        // Test for: CatchTheNuts after Start
        [UnityTest]
        public IEnumerator FindMissingScripts() {
            GameObject[] go = Object.FindObjectsOfType<GameObject>();
            int go_count = 0, components_count = 0, missing_count = 0;
            foreach (GameObject gameObject in go) {
                go_count++;
                Component[] components = gameObject.GetComponents<Component>();
                for (int i = 0; i < components.Length; i++) {
                    components_count++;
                    if (components[i] == null) {
                        missing_count++;
                        string s = gameObject.name;
                        Transform t = gameObject.transform;
                        while (t.parent != null) {
                            s = t.parent.name + "/" + s;
                            t = t.parent;
                        }
                        Debug.Log(s + " has an empty script attached in position: " + i, gameObject);
                    }
                }
            }

            Debug.Log(string.Format("Searched {0} GameObjects, {1} components, found {2} missing", go_count, components_count, missing_count));

            Assert.AreEqual(0, missing_count);

            yield return null;
        }

        
        /// Test For: BuildingMenu UI - Close Button works and Camera Moves out
        [UnityTest]
        public IEnumerator TestBuildingMenu() {

            // get first Building
            Building firstBuilding = Globals.Game.currentWorld.buildingsProgressArray[0];

            // Test Button in OverlayMenu
            Globals.UICanvas.uiElements.OverlayMenu.openOverlayMenu(firstBuilding);

            yield return null;

            Assert.IsTrue(Globals.UICanvas.uiElements.PopUpOverlayMenu.activeSelf);

            // Check Button and Open BuildingMenu via Buttonclick
            GameObject bmButtonObj = Globals.UICanvas.uiElements.PopUpOverlayMenu.transform.Find("LayoutContent/Content/ButtonInfo").gameObject;
            Assert.IsNotNull(bmButtonObj, "Button for BuildingMenu not found - possible rename?");
            Button bmButton = bmButtonObj.GetComponent<Button>();
            Assert.IsNotNull(bmButton);
            Assert.IsTrue(bmButton.interactable);
            bmButton.onClick.Invoke();

            yield return null;

            // Assert that BuildingMenu is open
            Assert.IsTrue(Globals.UICanvas.uiElements.BuildingMenu.activeSelf, "BuildingMenu didnt open!");

            //yield return new WaitForSeconds(3f);

            yield return null;

            Globals.KaloaSettings.adminUnlockProcessSec = 2;

            yield return null;

            string levelText = Game.GetComponent<BuildingMenu>().level.text;

            // Make a normal LevelUp -> DelayUnlock Process must work for this Test after here!
            firstBuilding.levelUp(1);

            yield return null;

            // Not -30 Min Button must be visible
            Assert.IsTrue(Globals.UICanvas.uiElements.BuildingMenuAdButtonMinus30.activeSelf);

            yield return new WaitForSeconds(Globals.KaloaSettings.adminUnlockProcessSec + 1.3f);

            // After LevelUp the Text must change
            Assert.AreNotEqual(levelText, Game.GetComponent<BuildingMenu>().level.text, "LevelText didnt change");

            // Save original Camera Position
            Vector3 orCameraPos = Globals.UICanvas.uiElements.MainCamera.transform.position;

            // Close Button Works
            GameObject closeButton = Globals.UICanvas.uiElements.BuildingMenu.transform.Find("LayoutHeading/Button_Close").gameObject;
            Assert.IsNotNull(closeButton);
            closeButton.GetComponent<Button>().onClick.Invoke();

            yield return new WaitForSeconds(0.5f);

            Assert.IsFalse(Globals.UICanvas.uiElements.BuildingMenu.activeSelf, "BuildingMenu didnt close");

            // Camera Moves Out when closing
            Assert.AreNotEqual(Globals.UICanvas.uiElements.MainCamera.transform.position, orCameraPos, "Camera didnt move on BuildingMenu.close");

            yield return null;
        }


    }
}

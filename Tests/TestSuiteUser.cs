using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using DateTime = System.DateTime;

namespace Tests
{
    public class TestSuiteUser {

        public InitGame Game;

        IdleNum zeroIdle = new IdleNum(0);


        [UnitySetUp]
        public IEnumerator UnitySetUp() {
            // TestSettings
            Globals.KaloaSettings.preventPlayfabCommunication = true;
            Globals.KaloaSettings.preventIAPCommunication = true;
            Globals.KaloaSettings.preventGoogleCommunication = true;
            Globals.KaloaSettings.preventSaving = false;
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
            Globals.Game.currentUser.wasSignedIn = false;

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




        // Test for: Stats are functional
        [UnityTest]
        public IEnumerator CheckUserBasics() {

            yield return new WaitForSeconds(5);

            // Emeralds
            Assert.IsNotNull(Globals.Game.currentUser.Emeralds);
            Assert.AreEqual(0, Globals.Game.currentUser.Emeralds);

            // worldProgressIndex
            Assert.IsNotNull(Globals.Game.currentUser.worldProgressIndex);
            Assert.AreEqual(0, Globals.Game.currentUser.worldProgressIndex);

            // Prestige
            Assert.IsNotNull(Globals.Game.currentUser.prestige);
            
            yield return null;
        }


        // Test for: Stats are functional
        [UnityTest]
        public IEnumerator TestUserStats() {

            // Inits
            Game.PopupMinOfflineTime = 1;
            System.DateTime l8r = DateTime.Now;
            DateTime firstGameLoad = l8r;


            //*  Test Statistics at a new Game
            Assert.AreEqual(DateTime.Now.Date, Globals.Game.currentUser.stats.FirstGameLoad.Date);
            Assert.AreEqual(DateTime.Now.Hour, Globals.Game.currentUser.stats.FirstGameLoad.Hour);
            Assert.AreEqual(DateTime.Now.Date, Globals.Game.currentUser.stats.LastDateGameOpen.Date);
            Assert.AreEqual(DateTime.Now.Hour, Globals.Game.currentUser.stats.LastDateGameOpen.Hour);
            Assert.AreEqual(1, Globals.Game.currentUser.stats.GameOpeningsRaw);
            Assert.AreEqual(1, Globals.Game.currentUser.stats.DaysWithGameOpening);
            Assert.AreEqual(1, Globals.Game.currentUser.stats.DaysWithConsecutiveGameOpening);



            //*  Test  1 day later - FirstGameload still the same, other stats are all raising

            // Manipulate Time
            l8r = l8r.AddHours(24); // 1 day

            // Start new Game
            Game.initGameStartUp(false, l8r);
            DateTime secondGameLoad = l8r;

            Assert.AreEqual(firstGameLoad.Date, Globals.Game.currentUser.stats.FirstGameLoad.Date, "FirstGameLoad must not be changed at any Day");
            Assert.AreEqual(firstGameLoad.Hour, Globals.Game.currentUser.stats.FirstGameLoad.Hour, "FirstGameLoad must not be changed at any Day");
            Assert.AreEqual(secondGameLoad.Date, Globals.Game.currentUser.stats.LastDateGameOpen.Date, "LastDateGameOpen must be the this Day, bcse its updated at GameStart");
            Assert.AreEqual(secondGameLoad.Hour, Globals.Game.currentUser.stats.LastDateGameOpen.Hour, "LastDateGameOpen must be the this Day, bcse its updated at GameStart");
            Assert.AreEqual(2, Globals.Game.currentUser.stats.GameOpeningsRaw);
            Assert.AreEqual(2, Globals.Game.currentUser.stats.DaysWithGameOpening);
            Assert.AreEqual(2, Globals.Game.currentUser.stats.DaysWithConsecutiveGameOpening);


            //*  Test  1 day later - FirstGameload still the same, other stats are all raising

            // Manipulate Time
            l8r = l8r.AddHours(24); // 1 day

            // Start new Game
            Game.initGameStartUp(false, l8r);
            DateTime thirdGameLoad = l8r;

            Assert.AreEqual(firstGameLoad.Date, Globals.Game.currentUser.stats.FirstGameLoad.Date, "FirstGameLoad must not be changed at any Day");
            Assert.AreEqual(firstGameLoad.Hour, Globals.Game.currentUser.stats.FirstGameLoad.Hour, "FirstGameLoad must not be changed at any Day");
            Assert.AreEqual(thirdGameLoad.Date, Globals.Game.currentUser.stats.LastDateGameOpen.Date, "LastDateGameOpen must be the this Day, bcse its updated at GameStart");
            Assert.AreEqual(thirdGameLoad.Hour, Globals.Game.currentUser.stats.LastDateGameOpen.Hour, "LastDateGameOpen must be the this Day, bcse its updated at GameStart");
            Assert.AreEqual(3, Globals.Game.currentUser.stats.GameOpeningsRaw);
            Assert.AreEqual(3, Globals.Game.currentUser.stats.DaysWithGameOpening);
            Assert.AreEqual(3, Globals.Game.currentUser.stats.DaysWithConsecutiveGameOpening);



            //*  Test  30 seconds later (same day) - only the GameOpeningsRaw should move one up

            // Manipulate Time (hint, will fail 30 Seconds before midnight)
            l8r = l8r.AddSeconds(30); // 30 Seconds

            // Start new Game
            Game.initGameStartUp(false, l8r);
            DateTime fourthGameLoad = l8r;

            Assert.AreEqual(firstGameLoad.Date, Globals.Game.currentUser.stats.FirstGameLoad.Date, "FirstGameLoad must not be changed at any Day");
            Assert.AreEqual(firstGameLoad.Hour, Globals.Game.currentUser.stats.FirstGameLoad.Hour, "FirstGameLoad must not be changed at any Day");
            Assert.AreEqual(fourthGameLoad.Date, Globals.Game.currentUser.stats.LastDateGameOpen.Date, "LastDateGameOpen must be the this Day, bcse its updated at GameStart");
            Assert.AreEqual(fourthGameLoad.Hour, Globals.Game.currentUser.stats.LastDateGameOpen.Hour, "LastDateGameOpen must be the this Day, bcse its updated at GameStart");
            Assert.AreEqual(4, Globals.Game.currentUser.stats.GameOpeningsRaw);
            Assert.AreEqual(3, Globals.Game.currentUser.stats.DaysWithGameOpening);
            Assert.AreEqual(3, Globals.Game.currentUser.stats.DaysWithConsecutiveGameOpening);


            //*  Test  1 day no game opening

            // Manipulate Time (hint, will fail 30 Seconds before midnight)
            l8r = l8r.AddDays(2); // 30 Seconds

            // Start new Game
            Game.initGameStartUp(false, l8r);
            DateTime fifthGameLoad = l8r;

            Assert.AreEqual(firstGameLoad.Date, Globals.Game.currentUser.stats.FirstGameLoad.Date, "FirstGameLoad must not be changed at any Day");
            Assert.AreEqual(firstGameLoad.Hour, Globals.Game.currentUser.stats.FirstGameLoad.Hour, "FirstGameLoad must not be changed at any Day");
            Assert.AreEqual(fifthGameLoad.Date, Globals.Game.currentUser.stats.LastDateGameOpen.Date, "LastDateGameOpen must be the this Day, bcse its updated at GameStart");
            Assert.AreEqual(fifthGameLoad.Hour, Globals.Game.currentUser.stats.LastDateGameOpen.Hour, "LastDateGameOpen must be the this Day, bcse its updated at GameStart");
            Assert.AreEqual(5, Globals.Game.currentUser.stats.GameOpeningsRaw);
            Assert.AreEqual(4, Globals.Game.currentUser.stats.DaysWithGameOpening);
            Assert.AreEqual(1, Globals.Game.currentUser.stats.DaysWithConsecutiveGameOpening);


            yield return null;

        }


        // Test for: LastDateGameClosed are functional
        [UnityTest]
        public IEnumerator TestUserStatsLastDateGameClosed() {

            // Inits
            Game.PopupMinOfflineTime = 1;
            System.DateTime l8r = DateTime.Now;
            DateTime firstGameLoad = l8r;


            Assert.AreEqual(DateTime.Now.Date, Globals.Game.currentUser.stats.LastDateGameClosed.Date);
            Assert.AreEqual(DateTime.Now.Hour, Globals.Game.currentUser.stats.LastDateGameClosed.Hour);
            SavingSystem.savePlayerPrefs(l8r);


            //*  Test  1 day later - FirstGameload still the same, other stats are all raising

            // Manipulate Time
            l8r = l8r.AddHours(24); // 1 day

            // Start new Game
            DateTime secondGameLoad = l8r;

            Assert.AreEqual(firstGameLoad.Date, Globals.Game.currentUser.stats.LastDateGameClosed.Date, "LastDateGameClosed must be the last Day, bcse its updated at GameClose");
            Assert.AreEqual(firstGameLoad.Hour, Globals.Game.currentUser.stats.LastDateGameClosed.Hour, "LastDateGameClosed must be the last Day, bcse its updated at GameClose");
            SavingSystem.savePlayerPrefs(l8r);


            //*  Test  1 day later - FirstGameload still the same, other stats are all raising

            // Manipulate Time
            l8r = l8r.AddHours(24); // 1 day

            // Start new Game
            DateTime thirdGameLoad = l8r;

            Assert.AreEqual(secondGameLoad.Date, Globals.Game.currentUser.stats.LastDateGameClosed.Date, "LastDateGameClosed must be the last Day, bcse its updated at GameClose");
            Assert.AreEqual(secondGameLoad.Hour, Globals.Game.currentUser.stats.LastDateGameClosed.Hour, "LastDateGameClosed must be the last Day, bcse its updated at GameClose");
            SavingSystem.savePlayerPrefs(l8r);

            //*  Test  30 seconds later (same day) - only the GameOpeningsRaw should move one up

            // Manipulate Time (hint, will fail 30 Seconds before midnight)
            l8r = l8r.AddSeconds(30); // 30 Seconds

            // Start new Game
            DateTime fourthGameLoad = l8r;

            Assert.AreEqual(thirdGameLoad.Date, Globals.Game.currentUser.stats.LastDateGameClosed.Date, "LastDateGameClosed must be the last Day, bcse its updated at GameClose");
            Assert.AreEqual(thirdGameLoad.Hour, Globals.Game.currentUser.stats.LastDateGameClosed.Hour, "LastDateGameClosed must be the last Day, bcse its updated at GameClose");
            SavingSystem.savePlayerPrefs(l8r);


            //*  Test  1 day no game opening

            // Manipulate Time (hint, will fail 30 Seconds before midnight)
            l8r = l8r.AddDays(2); // 30 Seconds

            // Start new Game
            DateTime fifthGameLoad = l8r;

            Assert.AreEqual(thirdGameLoad.Date, Globals.Game.currentUser.stats.LastDateGameClosed.Date, "LastDateGameClosed must be the last Day, bcse its updated at GameClose");
            Assert.AreEqual(thirdGameLoad.Hour, Globals.Game.currentUser.stats.LastDateGameClosed.Hour, "LastDateGameClosed must be the last Day, bcse its updated at GameClose");
            Assert.AreEqual(fourthGameLoad.Date, Globals.Game.currentUser.stats.LastDateGameClosed.Date, "LastDateGameClosed must be the last Day, bcse its updated at GameClose");
            Assert.AreEqual(fourthGameLoad.Hour, Globals.Game.currentUser.stats.LastDateGameClosed.Hour, "LastDateGameClosed must be the last Day, bcse its updated at GameClose");


            yield return null;

        }


    }
}

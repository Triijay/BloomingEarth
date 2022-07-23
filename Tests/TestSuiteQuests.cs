using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Tests
{
    public class TestSuiteQuests {

        public InitGame Game;

        IdleNum zeroIdle = new IdleNum(0);
        HybridBuilding hybridbuilding;


        [UnitySetUp]
        public IEnumerator UnitySetUp() {
            // TestSettings
            Globals.KaloaSettings.preventPlayfabCommunication = true;
            Globals.KaloaSettings.preventIAPCommunication = true;
            Globals.KaloaSettings.preventSaving = true;
            Globals.KaloaSettings.preventGoogleCommunication = true;
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
            Globals.Game.currentUser.wasSignedIn = false; PlayerPrefs.SetString("global_stat_firstGameLoad", "222");


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




        // Test for: Quests Basics
        [UnityTest]
        public IEnumerator CheckQuestBasics() {

            int i = 0;
            foreach (Quest aQuest in Globals.Game.currentWorld.QuestsComponent.questList) {

                // Lang Keys da
                Assert.IsNotEmpty(aQuest.langKeyHeading, "Quest LangKey missing in: Quest" + i);
                if (aQuest.questType == Quest.QuestTypes.Quest) {
                    Assert.IsNotEmpty(aQuest.langKeyTextStart, "Quest LangKey missing in: Quest" + i);
                }
                Assert.IsNotEmpty(aQuest.langKeyTextFinish, "Quest LangKey missing in: Quest" + i);

                // Quests haben Finish Conditions
                if (aQuest.questType == Quest.QuestTypes.Quest && aQuest.finishCondition.conditionType == QuestCondition.ConditionTypes.Building) {
                    Assert.Less(0, aQuest.finishCondition.buildingLevel, "Quest Problem in: Quest" + i);
                    Assert.IsNotNull(aQuest.finishCondition.building, "Quest Problem in: Quest" + i);
                }

                i++;
            }

            yield return null;
        }



        // Test for: Quests Rewards
        [UnityTest]
        public IEnumerator CheckQuestRewards() {

            foreach (Quest aQuest in Globals.Game.currentWorld.QuestsComponent.questList) {

                // User has to be rewarded!
                Assert.Less(0, aQuest.rewardList.Count, "Quest no Rewards: " + aQuest.langKeyTextFinish);

                foreach (QuestReward reward in aQuest.rewardList) {

                    if (reward.rewardType == QuestReward.RewardTypes.ItemForInventory) {
                        Assert.IsNotNull(reward.itemForInventory, "Quest Problem in: " + aQuest.langKeyTextFinish );
                    } else if (reward.rewardType == QuestReward.RewardTypes.Emeralds) {
                        Assert.Less(0, reward.amountEmeralds, "Quest Problem in: " + aQuest.langKeyTextFinish);
                    } else if (reward.rewardType == QuestReward.RewardTypes.Coins) {
                        Assert.Less(0, reward.amountCoins.getAmount(), "Quest Problem in: " + aQuest.langKeyTextFinish);
                    }

                }
            }

            yield return null;
        }



    }
}

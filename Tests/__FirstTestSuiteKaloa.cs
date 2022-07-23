using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Tests 
{ 
    public class __FirstTestSuiteKaloa
    {

        [SetUp]
        public void SetUp() {
        }

        [TearDown]
        public void TearDown() {
        }
        
        [Test]
        public void SettingsTestDebugMode() {
            // Check if playfab Communication is not disabled
            Assert.IsFalse(Globals.KaloaSettings.debugMode, "CRITICAL WARNING: debugMode must be FALSE for release");
        }

        [Test]
        public void SettingsTestPlayFab() {
            // Check if playfab Communication is not disabled
            Assert.IsFalse(Globals.KaloaSettings.preventPlayfabCommunication, "CRITICAL WARNING: preventPlayfabCommunication must be FALSE for release");
        }

        [Test]
        public void SettingsTestIAP() {
            // Check if IAP Communication is not disabled
            Assert.IsFalse(Globals.KaloaSettings.preventIAPCommunication, "CRITICAL WARNING: preventIAPCommunication must be FALSE for release");
        }

        [Test]
        public void SettingsTestGoogleComm() {
            // Check if google Communication is not disabled
            Assert.IsFalse(Globals.KaloaSettings.preventGoogleCommunication, "CRITICAL WARNING: preventGoogleCommunication must be FALSE for release");
        }

        [Test]
        public void SettingsTestAds() {
            // Check if preventRealAds is off
            Assert.IsFalse(Globals.KaloaSettings.preventRealAds, "CRITICAL WARNING: preventRealAds must be turned to FALSE for release");
        }

        [Test]
        public void SettingsTestTutorialSkip() {
            // Check if skipTutorial is false
            Assert.IsFalse(Globals.KaloaSettings.skipTutorial, "CRITICAL WARNING: skipTutorial must be turned to FALSE for release");
        }

        [Test]
        public void SettingsTestSavingSystem() {
            // Check if preventSaving is false
            Assert.IsFalse(Globals.KaloaSettings.preventSaving, "CRITICAL WARNING: preventSaving must be turned to FALSE for release");
        }

        [Test]
        public void SettingsTestPlayAmbientMusicInEditor() {
            // Check if preventSaving is false
            Assert.IsFalse(Globals.KaloaSettings.playAmbientMusicInEditor, "CRITICAL WARNING: playAmbientMusicInEditor must be turned to FALSE for release");
        }
        

    }
}

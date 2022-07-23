using Bayat.SaveSystem;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Internal;
using GooglePlayGames;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using Application = UnityEngine.Application;

public class PlayFabFileSync
{
    public static string playFabEntityID = ""; // Id representing the logged in player
    public static string playFabEntityType = ""; // entityType representing the logged in player
    private static readonly Dictionary<string, byte[]> _entityFileJson = new Dictionary<string, byte[]>();
    private static string ActiveUploadFileName;
    private static readonly string playFabFileName = SavingSystem.fileNameZipfile + ".zip";
    // GlobalFileLock provides is a simplistic way to avoid file collisions, specifically designed for this example.
    private static int GlobalFileLock = 0;




    /*--------------------------------------------------------------
      -------------------- HELPER FUNCTIONS ------------------------
      --------------------------------------------------------------*/

    private static void OnSharedFailure(PlayFabError error) {
        Globals.UICanvas.DebugLabelAddText("Error while file sync: " + error.GenerateErrorReport());
        Debug.LogError(error.GenerateErrorReport());
        GlobalFileLock -= 1;
        PlayFabAccountMngmt.reloadGoogleSave = false;
    }

    private static string getPlayfabZIPSaveGameLocation() {
        return SavingSystem.getRootSaveGameLocation() + SavingSystem.fileNameZipfile + "_PF.zip";
    }

    private static async void getDataFromSaveFile() {
        try {
            _entityFileJson.Clear();

            byte[] data;

            string foldernameToZipLocal = SavingSystem.getRootSaveGameLocation() + SavingSystem.fileNameSaveGameFolderLocal;
            string filenameZipTarget = SavingSystem.getRootSaveGameLocation() + SavingSystem.fileNameZipfile + ".zip";

            if (Directory.Exists(foldernameToZipLocal)) {

                // If Zip File already exists, delete it
                if (File.Exists(filenameZipTarget)) {
                    File.Delete(filenameZipTarget);
                }

                // Zip Data
                ZipFile.CreateFromDirectory(
                    foldernameToZipLocal,
                    filenameZipTarget
                    );

                // Check if Zip was created properly
                if (File.Exists(filenameZipTarget)) {

                    // Read data from File as text
                    data = await SaveSystemAPI.ReadAllBytesAsync(filenameZipTarget);

                    // Save in dictionary
                    _entityFileJson.Add(playFabFileName, data);
                } else {
                    Globals.UICanvas.DebugLabelAddText("PlayFabFileSync Error: Zip File wasn't created: " + filenameZipTarget, true);
                }

            } else {
                Globals.UICanvas.DebugLabelAddText("PlayFabFileSync: Folder doesn't exist: " + foldernameToZipLocal);
            }

        } catch (Exception e) {
            Globals.UICanvas.DebugLabelAddText("Error: " + e);
        }
    }

    /*--------------------------------------------------------------
      ------------------------ DOWNLOAD ----------------------------
      --------------------------------------------------------------*/
    public static void LoadAllFiles() {

        if (PlayFabClientAPI.IsClientLoggedIn()) {

            if (GlobalFileLock != 0) {
                Globals.UICanvas.DebugLabelAddText("You should never download two Files twice! ");
                return;
            }

            Globals.UICanvas.DebugLabelAddText("Loading files...");
            GlobalFileLock += 1; // Start GetFiles
            var request = new PlayFab.DataModels.GetFilesRequest { Entity = new PlayFab.DataModels.EntityKey { Id = playFabEntityID, Type = playFabEntityType } };
            PlayFabDataAPI.GetFiles(request, OnGetFileMeta, OnSharedFailure);
        }

    }

    private static void OnGetFileMeta(PlayFab.DataModels.GetFilesResponse result) {
        Globals.UICanvas.DebugLabelAddText("Loading " + result.Metadata.Count + " files");

        _entityFileJson.Clear();
        foreach (var eachFilePair in result.Metadata) {
            _entityFileJson.Add(eachFilePair.Key, null);
            GetActualFile(eachFilePair.Value);
        }
        GlobalFileLock -= 1; // Finish GetFiles
    }

    private static void GetActualFile(PlayFab.DataModels.GetFileMetadata fileData) {
        GlobalFileLock += 1; // Start Each SimpleGetCall
        PlayFabHttp.SimpleGetCall(fileData.DownloadUrl,
            result => {
                _entityFileJson[fileData.FileName] = result;
                GlobalFileLock -= 1;
                onDownloadSuccess(); }, // Finish Each SimpleGetCall
            error => { Globals.UICanvas.DebugLabelAddText(error); PlayFabAccountMngmt.reloadGoogleSave = false; }
        );
    }

    private static async void onDownloadSuccess() {

        Globals.UICanvas.DebugLabelAddText("PlayFabFileSync Download Success");

        // Save file local
        string savingPath = getPlayfabZIPSaveGameLocation();
        string savingTargetFolder = SavingSystem.getRootSaveGameLocation() + SavingSystem.fileNameSaveGameFolderPlayfab;

        byte[] data;
        _entityFileJson.TryGetValue(playFabFileName, out data);

        // Write ZIP
        await SaveSystemAPI.WriteAllBytesAsync(savingPath, data);

        // Delete old PlayFab Data
        if (Directory.Exists(savingTargetFolder)) {
            Directory.Delete(savingTargetFolder, true); // recursive Delete
        }

        // Try to unpack Zip
        try {
            ZipFile.ExtractToDirectory(
                savingPath,
                savingTargetFolder);
        } catch (Exception e) {
            Globals.UICanvas.DebugLabelAddText("PlayFabFileSync Error: wasn't able to unpack zip " + e);
        }

        // Set PlayFab Initialized
        try {
            if (LoadingScreen.loadingScreenIsActive) {
                LoadingScreen.setPlayFabInitialized();
            }
        } catch {
            Debug.Log("Scene was already loaded.");
        }

        // If User trys to restore Data from Cloud, load Game Data
        if (PlayFabAccountMngmt.reloadGoogleSave) {
            Globals.UICanvas.DebugLabelAddText("PlayFabFileSync loading Game Data... ");
            SavingSystem.loadGameData();
        }
        
    }

    /*--------------------------------------------------------------
      ------------------------- UPLOAD -----------------------------
      --------------------------------------------------------------*/

    public static void UploadFile() {

        getDataFromSaveFile();

        if (GlobalFileLock != 0) {
            Globals.UICanvas.DebugLabelAddText("You should never Upload two Files twice!");
            return;
        }

        if (PlayFabClientAPI.IsClientLoggedIn()) {

            ActiveUploadFileName = playFabFileName;

            GlobalFileLock += 1; // Start InitiateFileUploads
            var request = new PlayFab.DataModels.InitiateFileUploadsRequest {
                Entity = new PlayFab.DataModels.EntityKey { Id = playFabEntityID, Type = playFabEntityType },
                FileNames = new List<string> { ActiveUploadFileName },
            };

            PlayFabDataAPI.InitiateFileUploads(request, OnInitFileUpload, OnInitFailed);
        }

    }

    private static void OnInitFailed(PlayFabError error) {
        if (error.Error == PlayFabErrorCode.EntityFileOperationPending) {
            // This is an error you should handle when calling InitiateFileUploads, but your resolution path may vary
            GlobalFileLock += 1; // Start AbortFileUploads
            var request = new PlayFab.DataModels.AbortFileUploadsRequest {
                Entity = new PlayFab.DataModels.EntityKey { Id = playFabEntityID, Type = playFabEntityType },
                FileNames = new List<string> { ActiveUploadFileName },
            };
            PlayFabDataAPI.AbortFileUploads(request, (result) => { GlobalFileLock -= 1; UploadFile(); }, OnSharedFailure); GlobalFileLock -= 1; // Finish AbortFileUploads
            GlobalFileLock -= 1; // Failed InitiateFileUploads
        } else
            OnSharedFailure(error);
    }

    private static void OnInitFileUpload(PlayFab.DataModels.InitiateFileUploadsResponse response) {
        byte[] payload;
        _entityFileJson.TryGetValue(ActiveUploadFileName, out payload);

        GlobalFileLock += 1; // Start SimplePutCall
        PlayFabHttp.SimplePutCall(response.UploadDetails[0].UploadUrl,
            payload,
            FinalizeUpload,
            error => { Debug.Log(error); }
        );
        GlobalFileLock -= 1; // Finish InitiateFileUploads
    }

    private static void FinalizeUpload(byte[] data) {
        GlobalFileLock += 1; // Start FinalizeFileUploads
        var request = new PlayFab.DataModels.FinalizeFileUploadsRequest {
            Entity = new PlayFab.DataModels.EntityKey { Id = playFabEntityID, Type = playFabEntityType },
            FileNames = new List<string> { ActiveUploadFileName },
        };
        PlayFabDataAPI.FinalizeFileUploads(request, OnUploadSuccess, OnSharedFailure);
        GlobalFileLock -= 1; // Finish SimplePutCall
    }

    private static void OnUploadSuccess(PlayFab.DataModels.FinalizeFileUploadsResponse result) {
        Debug.Log("File upload success: " + ActiveUploadFileName);
        Globals.UICanvas.DebugLabelAddText("File playfab upload success: " + ActiveUploadFileName);
        GlobalFileLock -= 1; // Finish FinalizeFileUploads
    }

}

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Random = System.Random;

/// <summary>
/// Steers the Sound of Smartphones 
/// </summary>
public class Sound : MonoBehaviour {

    /// <summary>
    /// List of Audio Tracks currently in Memory
    /// </summary>
    private Dictionary<string, AudioSource> audioFiles;

    Random ambient_rnd = new Random();

    AudioSource[] AudioSources;
    AudioSource audioSource;

    AudioSource[] AmbientAudioSources;
    AudioSource[] AnimalAudioSources;

    public string currentAmbientMusic = "";


    /// <summary>
    /// Searches the Sound-Root-GameObject and creates a Dictionary of Sounds
    /// </summary>
    public void InitAudioFiles() {
        audioFiles = new Dictionary<string, AudioSource>();
        try {
            // Search for all AudioSources in our Sounds GameObject
            AudioSources = Globals.UICanvas.uiElements.Sounds.GetComponentsInChildren<AudioSource>(true);

            // Ambient Sounds
            AmbientAudioSources = Globals.UICanvas.uiElements.Sounds.transform.Find("Ambient").gameObject.GetComponentsInChildren<AudioSource>(true);

            // Animal Sounds
            AnimalAudioSources = Globals.UICanvas.uiElements.Sounds.transform.Find("Animals").gameObject.GetComponentsInChildren<AudioSource>(true);

            // Go Through all AudioSources and add them to audioFiles
            foreach (AudioSource audioSource in AudioSources) {
                audioFiles.Add(audioSource.name, audioSource);
            }
        } catch (Exception e) {
            Debug.LogError("No Audio Sources in GameObject Sounds or initialize-error: " + e);
        }

        // Play Ambient Music not in our Test Env
        if (Globals.KaloaSettings.playAmbientMusicInEditor || Application.platform != RuntimePlatform.WindowsEditor) {

            // Play Ambient Music, only if User had the Settings on
            if (Globals.UserSettings.hasMusic) {
                // Generate Random Ambient Music
                currentAmbientMusic = "AmbientMusic" + ambient_rnd.Next(0, AmbientAudioSources.Length);

                if (Globals.Game.currentUser.stats.GameOpeningsRaw < 2) {
                    // At first Gamestarts, we always want to have the Original AmbientMusic
                    currentAmbientMusic = "AmbientMusic0";
                }

                // Start Playing Ambient Sound
                PlaySound(currentAmbientMusic, true);
            }
        }

    }

    /// <summary>
    /// Play a specific Named Sound once<br></br>
    /// You can find a List of the acessable Sounds in GameObjects Root > Sounds
    /// </summary>
    /// <param name="SoundName"></param>
    public void PlaySound(string SoundName, bool force = false) {

        if (force || Globals.UserSettings.hasSound) {
            if (audioFiles.TryGetValue(SoundName, out audioSource)) {
                audioSource.Play();

                Debug.Log("Sound " + SoundName + " played.");

            } else {
                Debug.LogError("No Audio Source in GameObject Sounds with Name " + SoundName);
            }
        }
    }


    /// <summary>
    /// Play a specific Named Sound once<br></br>
    /// You can find a List of the acessable Sounds in GameObjects Root > Sounds
    /// </summary>
    /// <param name="SoundName"></param>
    public void StopSound(string SoundName) {
        if (audioFiles.TryGetValue(SoundName, out audioSource)) {
        audioSource.Stop();

            Debug.Log("Sound " + SoundName + " stopped.");

        } else {
            Debug.LogError("No Audio Source in GameObject Sounds with Name " + SoundName);
        }
    }


    public string[] getAudioFilesAsStringArray() {
        return audioFiles.Keys.ToArray<string>();
    }

}
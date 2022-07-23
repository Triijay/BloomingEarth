using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class ParticleEffects : MonoBehaviour {

    public GameObject EmeraldParticles;
    private ParticleSystem EmeraldParticleSystem;

    public GameObject CoinParticles;
    private ParticleSystem CoinParticleSystem;
    private MinMaxCurve defaultCoinEmission;

    // Start is called before the first frame update
    void Start() {
        EmeraldParticleSystem = EmeraldParticles.GetComponent<ParticleSystem>();
        EmeraldParticles.GetComponent<ParticleSystemRenderer>().material.renderQueue = 4502;

        CoinParticleSystem = CoinParticles.GetComponent<ParticleSystem>();
        CoinParticles.GetComponent<ParticleSystemRenderer>().material.renderQueue = 4501;
        defaultCoinEmission = CoinParticleSystem.emission.rateOverTime;
    }

    /// <summary>
    /// Add Emeralds to Users Account<br></br>
    /// And Plays a ParticleSystem with spreading Emeralds at Point of Touch
    /// </summary>
    /// <param name="amount"></param>
    public void addEmeralds(int amount) {
        StartCoroutine(SpawnParticlesEmeralds(amount));
    }

    public IEnumerator SpawnParticlesEmeralds(int emeraldAmount) {
        // Configure the Emission
        var emission = EmeraldParticleSystem.emission;
        if (emeraldAmount >= 99) {
            emission.rateOverTime = emeraldAmount/10 + 1;
        } else {
            emission.rateOverTime = emeraldAmount + 1;
        }


        if (!EmeraldParticleSystem.isPlaying) {
            var main = EmeraldParticleSystem.main;
            main.duration = emeraldAmount / 25;
            if (main.duration > 4) {
                main.duration = 4;
            }
            if (main.duration < 1) {
                main.duration = 1;
            }
        }

        // get ParticleSystem in Position
        var rect = (RectTransform)EmeraldParticles.transform;
        if (Input.touchCount >= 1) {
            rect.anchoredPosition = new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y, EmeraldParticles.transform.localPosition.z);
        } else {
            rect.anchoredPosition = new Vector3(Screen.currentResolution.width / 2, Screen.currentResolution.height / 2, CoinParticles.transform.localPosition.z);
        }

        // Play Particles
        EmeraldParticles.SetActive(true);
        EmeraldParticleSystem.Play();

        // Add Emeralds to Bank Account
        Globals.Game.currentUser.raiseEmeralds(emeraldAmount);

        yield return null;
    }


    /// <summary>
    /// Add Coins to Users Account<br></br>
    /// And Plays a ParticleSystem with spreading Emeralds at Point of Touch
    /// </summary>
    /// <param name="amount"></param>
    public void addCoins(IdleNum amount, bool doubledEmission = false, bool useTouchPosition = true) {
        StartCoroutine(SpawnParticlesCoins(amount, doubledEmission, useTouchPosition));
    }

    public IEnumerator SpawnParticlesCoins(IdleNum coinAmount, bool doubledEmission, bool useTouchPosition) {
        // Configure the Emission
        var emission = CoinParticleSystem.emission;
        if (doubledEmission) {
            emission.rateOverTime = defaultCoinEmission.constant * 2;
        } else {
            emission.rateOverTime = defaultCoinEmission.constant;
        }


        if (!CoinParticleSystem.isPlaying) {
            var main = CoinParticleSystem.main;
        }

        // get ParticleSystem in Position
        var rect = (RectTransform)CoinParticles.transform;

        Debug.Log(useTouchPosition);
        if (Input.touchCount >= 1 && useTouchPosition) {
            rect.anchoredPosition = new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y, CoinParticles.transform.localPosition.z);
        } else {
            rect.anchoredPosition = new Vector3(Screen.currentResolution.width/2, Screen.currentResolution.height/2, CoinParticles.transform.localPosition.z); 
        }

        // Play Particles
        CoinParticles.SetActive(true);
        CoinParticleSystem.Play();

        // Add Coins to Bank Account
        Globals.Game.currentWorld.addCoins(coinAmount);

        yield return null;
    }
}

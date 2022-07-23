
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Environmental Class wich represents an environmental Health Value and a List of Affectors
/// </summary>
public class EnviGlass {

    #region Vars

    /// <summary>
    /// Shows how healthy the Environment is
    /// Positive Values will have a positive impact for the humanity
    /// Negative Values will have a bad impact for the humanity
    /// </summary>
    public float enviValue = 0;

    /// <summary>
    /// List of Affectors that affects the Environment
    /// </summary>
    public List<Affector<float>> affectors = new List<Affector<float>>();


    /// <summary>
    /// The EnviGlass has not a fixed Width, so we must transform our current GlassValue into the Needle-Position on the EnviGlass
    /// </summary>
    public float glassValueToTransform = 3.75f;


    public const int GLASS_VALUE_MAX = 90;
    public const int GLASS_VALUE_MIN = -90;


    private EnviStates lastEnviStatus;
    public EnviStates currentEnviStatus = EnviStates.Neutral;
    public enum EnviStates {
        Perfect,
        VeryGood,
        Good,
        Neutral,
        Poor,
        VeryPoor,
        Destructive
    }

    // Performance Optimization
    float summary, glassValue, glassValueToScale;
    string enviColor;

    #endregion

    /// <summary>
    /// Constructor
    /// Creates a new EnviGlass to manage the Environmental Health
    /// </summary>
    public EnviGlass() {
        
    }

    /// <summary>
    /// gets the environmental Health
    /// </summary>
    /// <returns></returns>
    public float getEnviValue() {
        return enviValue;
    }


    /// <summary>
    /// Add a new Affector to the Environment
    /// Checks wether the Affector is already in the List
    /// ALSO UPDATES THE enviValue !
    /// </summary>
    /// <param name="NewAffector"></param>
    public void addAffector(Affector<float> NewAffector) {
        if (!isAlreadyAffector(NewAffector)) {
            affectors.Add(NewAffector);
            updateEnviValue();
        }
    }

    /// <summary>
    /// Updates the AffectionValue of a specified Affector<br></br>
    /// and updates the EnviValue of the Glass
    /// </summary>
    /// <param name="affectorID"></param>
    /// <param name="newAffectionValue"></param>
    public void updateSpecificAffector(string affectorID, float newAffectionValue) {
        foreach (Affector<float> Affector in affectors) {
            if (Affector.getID() ==  affectorID) {
                Affector.setAffection(newAffectionValue);
            }
        }
        updateEnviValue();
    }

    /// <summary>
    /// Updates the enviValue with all its Affectors
    /// </summary>
    private void updateEnviValue() {
        summary = 0;
        foreach (Affector<float> Affector in affectors) {
            summary += Affector.getAffection();
        }
        enviValue = summary;

        // Transform Needle
        transformNeedle();
    }



    /// <summary>
    /// Searches the AffectorList and returns wether the Affector is already in the List
    /// </summary>
    /// <param name="Affector"></param>
    /// <returns></returns>
    private bool isAlreadyAffector(Affector<float> Affector) {
        foreach (Affector<float> ListAffector in affectors) {
            if (ListAffector.getID() == Affector.getID()) {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Transforms the needle of the EnviGlass based on the enviValue
    /// </summary>
    public void transformNeedle() {

        // Get the degree Value from our envivalue
        // We clamped the value to (-)360 each, so 4 times 90
        glassValue = Mathf.Clamp(enviValue / 4, GLASS_VALUE_MIN, GLASS_VALUE_MAX);

        //The EnviGlass has not a fixed Width, so we must transform our current GlassValue into the Needle-Position on the EnviGlass
        RectTransform rt = (RectTransform)Globals.UICanvas.uiElements.enviGlassContainer.transform;
        glassValueToTransform = glassValue * rt.rect.max.x * 0.75f / GLASS_VALUE_MAX;

        // Transform the Needle Pivot with the above values
        Globals.UICanvas.uiElements.enviNeedle.transform.localPosition = new Vector3(glassValueToTransform, Globals.UICanvas.uiElements.enviNeedle.transform.localPosition.y,
            Globals.UICanvas.uiElements.enviNeedle.transform.localPosition.z);


        // Get the scale value based on our glassvalue
        // TODO: Function to maintain the best value
        // 20.45 is just a number i tried - with glassValue 90 it should be 15
        // Tested: Scale Value must vary between 1.6f and 6f (mom to 4.4f)
        glassValueToScale = glassValue / 20.45f;
        glassValueToScale = Mathf.Clamp(Math.Abs(glassValueToScale), 1f, 1.5f);

        Globals.UICanvas.uiElements.enviNeedle.transform.localScale = new Vector3(1f, glassValueToScale, 1f);

        // Sky dependend on Envi
        //RenderSettings.skybox.SetColor("_SkyColor", new Color32(200, 50, 50, 255));
        //RenderSettings.skybox.SetColor("_EquatorColor", new Color32(209, 151, 88, 255));
        //RenderSettings.skybox.SetColor("_CloudsShadowColor", new Color32(209, 151, 88, 255));

        // Check EnviFauna after Needle Tranformed
        EnviFauna.checkEnviFauna(glassValue);

        // Update the Information in the PopUp
        updateEnviPopUp();

    }


    /// <summary>
    /// Updates the Information
    /// </summary>
    public void updateEnviPopUp(bool force = false) {
        
        if (glassValue == GLASS_VALUE_MAX) {
            // Perfect Environment
            enviColor = "#70C919";
            currentEnviStatus = EnviStates.Perfect;
        } else if (glassValue >= 60) {
            // Very Good Environment
            enviColor = "#9DC919";
            currentEnviStatus = EnviStates.VeryGood;
        } else if (glassValue >= 30) {
            // Good Environment
            enviColor = "#AFC919";
            currentEnviStatus = EnviStates.Good;
        } else if (glassValue >= 0) {
            // Neutral Environment
            enviColor = "#C9C919";
            currentEnviStatus = EnviStates.Neutral;
        } else if (glassValue >= -30) {
            // Poor Environment
            enviColor = "#C97019";
            currentEnviStatus = EnviStates.Poor;
        } else if (glassValue >= -60) {
            // Very Poor Environment
            enviColor = "#C94419";
            currentEnviStatus = EnviStates.VeryPoor;
        } else {
            // Destructive Environment
            enviColor = "#C91819";
            currentEnviStatus = EnviStates.Destructive;
        }

        // Change Texts of the PopUp if the Status has Changed or its forced (eg at language-switch)
        if (lastEnviStatus != currentEnviStatus || force) {
            Globals.UICanvas.uiElements.PopUpEnviGlassStatus.text =
                "<color="+ enviColor + ">" + 
                Globals.Controller.Language.translateString(
                    "enviglass_status_" + currentEnviStatus.ToString(),
                    new string[] { Globals.Controller.Language.translateString(Globals.Game.currentWorld.worldName + "_possessive") }) +
                "</color>";
            Globals.UICanvas.uiElements.PopUpEnviGlassStatusConsequences.text =
                Globals.Controller.Language.translateString(
                    "enviglass_status_consequences_" + currentEnviStatus.ToString(),
                    new string[] { Globals.Controller.Language.translateString(Globals.Game.currentWorld.worldName + "_possessive") });
            Globals.UICanvas.uiElements.PopUpEnviGlassTips.text =
                Globals.Controller.Language.translateString(
                    "enviglass_tips_" + currentEnviStatus.ToString(),
                    new string[] { Globals.Controller.Language.translateString(Globals.Game.currentWorld.worldName + "_possessive") });
            // Set new lastEnviStatus
            lastEnviStatus = currentEnviStatus;
        }


    }

}

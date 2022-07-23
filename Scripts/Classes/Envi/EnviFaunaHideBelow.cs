using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// The <see cref="EnviFauna"/> can be used to hide/display Objects by the current Envi Value<br></br>
/// <see cref="EnviFaunaHideBelow"/> hides an Object when the Envi-GlassValue is higher than a specific amount<br></br>
/// Useful for e.g. rotten Trees
/// </summary>
public class EnviFaunaHideBelow : EnviFauna
{

    /// <summary>
    /// Int from -90..0..90 where the EnviNeedle is positioned
    /// </summary>
    [Tooltip("Int from -90..0..90 where the EnviNeedle is positioned")]
    [Range(-90.0f, 90.0f)]
    public int needleposMaxDisplayed = 0;

    /// <summary>
    /// Int from -90..0..90 where the EnviNeedle is positioned
    /// </summary>
    public int getNeedleposMaxDisplayed () {
        return needleposMaxDisplayed;
    }


    /// <summary>
    /// Checks this EnviFaunaObject wether to display or Not
    /// </summary>
    protected override void ShowOrHideEnviFauna(float glassValue) {
        gameObject.SetActive(glassValue < needleposMaxDisplayed);
    }


}

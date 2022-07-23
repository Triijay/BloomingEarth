using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// The EnviFauna can be used to hide/display Objects by the current Envi Value
/// </summary>
public abstract class EnviFauna : MonoBehaviour {


    /// <summary>
    /// Checks this EnviFaunaObject wether to display or Not
    /// </summary>
    protected virtual void ShowOrHideEnviFauna(float glassValue) {}


    /// <summary>
    /// Checks all EnviFaunaObjects wether to display or Not
    /// </summary>
    public static void checkEnviFauna(float glassValue) {
        foreach (EnviFauna fauna in Globals.Game.currentWorld.gameObject.GetComponentsInChildren<EnviFauna>(true)) {
            fauna.ShowOrHideEnviFauna(glassValue);
        }
    }

}

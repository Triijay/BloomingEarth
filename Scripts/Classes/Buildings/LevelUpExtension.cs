using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelUpExtension : MonoBehaviour {

    public Material materialToChange;
    public Material materialTarget;
    private bool isExecuted = false;


    /// <summary>
    /// GETTER for Boolean
    /// </summary>
    public bool isAlreadyExecuted() {
        return isExecuted;
    }

    /// <summary>
    /// SETTER for Boolean
    /// </summary>
    /// <param name="b"></param>
    public void setExecuted(bool b) {
        isExecuted = b;
    }

}

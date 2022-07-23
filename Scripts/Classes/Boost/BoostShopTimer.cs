using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostShopTimer : BoostTimer
{

    /// <summary>
    /// Inits the BoostTimer
    /// </summary>
    public override void initBoostTimer() {
        // read Variables, so that they not are read every second in Update Function
        maxSecondsOfBoost = -1;

        boostPopUp = null;
        BoostPopUpTextRemaining = null;
        BoostPopUpWarningText = null;
    }
}

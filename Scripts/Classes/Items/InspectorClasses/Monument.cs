using Bayat.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Monument can be placed in specific Slots at a World
/// </summary>
public class Monument : ItemTemplate {

    /// <summary>
    /// The Template GameObject wich will be shown at the Monument Spot
    /// </summary>
    public GameObject monumentObject;


    // Performance Optimization
    GameObject cloneMonument;
    Monument templateItem;


    public void placeMonumentInMonumentSlot() {

        // Place Instance of Monument Object in the current World
        cloneMonument = Instantiate(monumentObject, Globals.Game.currentWorld.monumentSlot);
        cloneMonument.SetActive(true);
    }


}

using Bayat.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// User Settings of the Game
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class User {

    /// <summary>
    /// Identifier for this User
    /// </summary>
    [JsonProperty(PropertyName = "userID")]
    public string userID = "";

    /// <summary>
    /// Emeralds the User owns
    /// </summary>
    private int emeralds = 0;

    /// <summary>
    /// Encapsulation for Emeralds
    /// </summary>
    [JsonProperty(PropertyName = "PremiumCurrency")]
    public int Emeralds {
        get { return emeralds; }   // get method
        set {
            emeralds = value;
            // Update HUD Emerald Display
            if(Globals.UICanvas.uiElements.HUDEmeraldDisplay != null) {
                Globals.UICanvas.uiElements.HUDEmeraldDisplay.text = value.ToString();
            }
            // Update Shop Emerald Display
            if (Globals.UICanvas.uiElements.ShopEmeraldDisplay != null) {
                Globals.UICanvas.uiElements.ShopEmeraldDisplay.text = "<sprite=\"Icon_Emerald\" name=\"Icon_Emerald\"> " + value.ToString();
            }
            // Check ShopItems
            if (Globals.Controller.IAP != null) {
                Globals.Controller.IAP.checkButtonsShouldBeInteractible();
            }

        }  // set method
    }
    

    /// <summary>
    /// Game Stats of the User
    /// </summary>
    [JsonProperty(PropertyName = "Stats")]
    public Stats stats = new Stats();

    /// <summary>
    /// Total Levels of all Buildings achieved by User
    /// </summary>
    [JsonProperty(PropertyName = "scoreTotalLevels")]
    public long scoreTotalLevels = 0;

    /// <summary>
    /// Documents if the User was signed in at last session
    /// </summary>
    [JsonProperty(PropertyName = "wasSignedIn")]
    public bool wasSignedIn = true;

    /// <summary>
    /// Index which indicates the progress of the User Across the worlds
    /// Defines the key in <see cref="WorldAcrossBuildingOrder"/>
    /// </summary>
    [JsonProperty(PropertyName = "worldProgressIndex")]
    public int worldProgressIndex = 0;

    /// <summary>
    /// Current Prestige of the Game
    /// </summary>
    //[JsonProperty(PropertyName = "Prestige")]
    public Prestige prestige;

    /// <summary>
    /// Items the User Owns
    /// </summary>
    [JsonProperty(PropertyName = "inventory")]
    public Inventory inventory = new Inventory();

    /// <summary>
    /// List of Items the User bought in our Shop
    /// </summary>
    [JsonProperty(PropertyName = "buyedItemsList")]
    public List<IAPBuyedItem> buyedItemsList = new List<IAPBuyedItem>();

    /// <summary>
    /// Saves if the User is in our Tutorial
    /// </summary>
    [JsonProperty(PropertyName = "tutorialIsRunning")]
    public bool tutorialIsRunning = false;

    /// <summary>
    /// Saves where the User is in our Tutorial
    /// </summary>
    [JsonProperty(PropertyName = "tutorialIDAndStageID")]
    public KeyValuePair<int, int> tutorialIDAndStageID = new KeyValuePair<int, int>();


    /// <summary>
    /// Constructor
    /// </summary>
    public User() {

        // Init his Inventory
        if (Globals.UICanvas.uiElements != null) {
            inventory.InitItemTemplates();
            inventory.InitProperties();
        }

        // Init Prestive if its activated
        if (Globals.Game.initGame != null) {
            prestige = Globals.Game.initGame.GetComponent<Prestige>();
        }
    }


    /// <summary>
    /// Raises the Emeralds by a specific amount
    /// </summary>
    /// <param name="amount"></param>
    public void raiseEmeralds(int amount) {
        Emeralds += amount;

        // Log Firebase Event
        KeyValuePair<string, object>[] valuePairArray = {
            new KeyValuePair<string, object>("amount", amount),
        };

        Globals.Controller.Firebase.IncrementFirebaseEventWithParameters(
        "earn_virtual_currency", valuePairArray);
    }

    /// <summary>
    /// Raises the Emeralds by a specific amount
    /// </summary>
    /// <param name="amount"></param>
    public void reduceEmeralds(int amount) {
        Emeralds -= amount;
    }

}
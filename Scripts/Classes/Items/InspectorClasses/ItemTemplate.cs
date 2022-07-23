using Bayat.Json;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.ProceduralImage;

/// <summary>
/// Holder for all Items, the User can have in his Inventory
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public abstract class ItemTemplate : MonoBehaviour {

    /// <summary>
    /// Name of the Item -> same as the Name of the GameObject in "Items"<br></br>
    /// Represents the template of the Item<br></br>
    /// Used for Translation<br></br>
    /// NEVER CHANGE THE NAME OF THE GAMEOBJECT AFTER RELEASING AN ITEM !
    /// </summary>
    [Tooltip("Identifier of the Item - NEVER CHANGE THE NAME OF THE GAMEOBJECT AFTER RELEASING AN ITEM")]
    [JsonProperty(PropertyName = "itemName")]
    private string itemName = "";

    /// <summary>
    /// Rarity of the Item
    /// Shouldn't be changed after release, but would be ok
    /// </summary>
    public Rarities rarity = Rarities.Usual;

    /// <summary>
    /// Defines how rare the Item is
    /// </summary>
    public enum Rarities {
        Usual,  // Silver
        Uncommon, // Green
        Rare,   // Blue
        Epic, // Violet
    }

    /// <summary>
    /// ConvertTable for Rarity to Color
    /// </summary>
    public static Dictionary<Rarities, Color32> itemRaritiesToColors = new Dictionary<Rarities, Color32>{
        { Rarities.Usual, new Color32(90, 90, 90, 255) },
        { Rarities.Uncommon, new Color32(97, 128, 6, 255) },
        { Rarities.Rare, new Color32(6, 97, 128, 255) },
        { Rarities.Epic, new Color32(166, 6, 128, 255) },
    };


    /// <summary>
    /// Returns the itemName to Identify the Item
    /// </summary>
    public string getItemName() {
        if(itemName == "") {
            itemName = gameObject.name;
        }
        return itemName;
    }


    /// <summary>
    /// Returns the TRANSLATED itemName to Identify the Item
    /// </summary>
    public string getTranslatedItemName() {
        return Globals.Controller.Language.translateString("itemname_" + getItemName()); ;
    }


    /// <summary>
    /// Fills a (Transform)GameObject with the item Parameters
    /// </summary>
    /// <param name="objectToFill"></param>
    public virtual void fillItemTemplateWithInfos(Transform objectToFill) {
        try {

            // Color HeadingBG
            objectToFill.Find("HeadingBG").GetComponent<ProceduralImage>().color
                = itemRaritiesToColors[rarity];

            // Color Border
            objectToFill.Find("Border").GetComponent<ProceduralImage>().color
                = itemRaritiesToColors[rarity];

            // Name of the Item
            objectToFill.Find("HeadingBG/Heading").GetComponent<TMPro.TextMeshProUGUI>().text
                = getTranslatedItemName();

            // its a virtual function, if needed the BuildingUpgrade will be activated again
            objectToFill.Find("BuildingUpgrade").gameObject.SetActive(false);
        } catch { }
    }
}

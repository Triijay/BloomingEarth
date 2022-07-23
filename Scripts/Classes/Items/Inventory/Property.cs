using UnityEngine;
using UnityEditor;
using Bayat.Json;
using Math = System.Math;

/// <summary>
/// A Property can be placed in Users Inventory and holds an Item
/// </summary>
[JsonObject(MemberSerialization.OptIn, IsReference = true)]
public class Property {

    /// <summary>
    /// Unique ID wich identifies the Item Instance which the User owns
    /// </summary>
    [JsonProperty(PropertyName = "uniqueItemInstanceID")]
    protected string uniqueItemInstanceID = "";

    /// <summary>
    /// The Name of the referencedItemTemplate in "Items"
    /// </summary>
    [JsonProperty(PropertyName = "referencedItemTemplateName")]
    private string referencedItemTemplateName;


    /// <summary>
    /// The referenced Item in GameObject "Items"<br></br>
    /// We cannot save this via Bayat
    /// </summary>
    private ItemTemplate referencedItemTemplate;
    /// <summary>
    /// The referenced Item in GameObject "Items"
    /// </summary>
    public ItemTemplate ReferencedItem {
        get { return referencedItemTemplate; }
        set { referencedItemTemplate = value; }
    }

    /// <summary>
    /// Says if the Property is already in use, eg a referenced BuildingUpgrade on a Building
    /// </summary>
    [JsonProperty(PropertyName = "usedAt")]
    private string usedAt = "";


    /// <summary>
    /// Creates an empty Property
    /// </summary>
    public Property() {}

    /// <summary>
    /// Creates a new Property with a specific referenced Item
    /// </summary>
    /// <param name="item"></param>
    public Property(ItemTemplate item) {
        referencedItemTemplate = item;
        referencedItemTemplateName = item.getItemName();
        if (uniqueItemInstanceID == "") {
            generateAndSetUniqueItemInstanceID();
        }
    }


    /// <summary>
    /// Loads the necessary Information of the Property
    /// </summary>
    public void initProperty() {
        if (referencedItemTemplate == null) {
            // Search for the ItemTemplate
            referencedItemTemplate = Globals.Game.currentUser.inventory.availableItems[referencedItemTemplateName];
        }
    }

    

    /// <summary>
    /// Sets if the Property is in Use
    /// </summary>
    /// <param name="aString"></param>
    public void setInUseAt(string aString) {
        usedAt = aString;
    }

    /// <summary>
    /// Sets if the Property is in Use
    /// </summary>
    /// <param name="aBool"></param>
    public void setNotInUse() {
        usedAt = "";
    }

    /// <summary>
    /// Returns if the Property is in Use
    /// </summary>
    /// <returns></returns>
    public bool isInUse() {
        return (usedAt != "");
    }

    /// <summary>
    /// Returns where the Property is in Use
    /// </summary>
    /// <returns></returns>
    public string getWhereInUse() {
        return usedAt;
    }


    /// <summary>
    /// Generates and Sets a new Unique ID to the Item
    /// </summary>
    public void generateAndSetUniqueItemInstanceID() {
        // Generate a unique ID
        Random.InitState(System.DateTime.Now.Millisecond + GetHashCode());

        uniqueItemInstanceID = Math.Abs(System.DateTime.Now.ToBinary()).ToString("X4") + "_" + Random.Range(10000000,99999999).ToString("X");
    }

}
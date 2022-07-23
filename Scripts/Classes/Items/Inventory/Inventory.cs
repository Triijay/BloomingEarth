using Bayat.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Inventory is a List of Properties the User Owns
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class Inventory {
    /// <summary>
    /// Items the User owns
    /// </summary>
    [JsonProperty(PropertyName = "inventory")]
    private List<Property> propertyList;


    /// <summary>
    /// List of Items currently in Memory
    /// </summary>
    public Dictionary<string, ItemTemplate> availableItems;


    // Performance Optimization
    ItemTemplate[] tempItems;
    ItemTemplate tempItem;
    bool returnBool;
    List<Property> TempPropertyList;





    /// <summary>
    /// Constructor
    /// </summary>
    public Inventory() {
        propertyList = new List<Property>(); 
    }


    

    
    /// <summary>
    /// Creates a Property with a refrerenced Item
    /// </summary>
    /// <param name="item"></param>
    public void createProperty(ItemTemplate item) {

        if (item != null) {
            Property newProperty = new Property(item);
            propertyList.Add(newProperty);

            Debug.Log("Item " + item.name + " created and sent to inventory.");
        }
    }

    /// <summary>
    /// Creates a Property with a refrerenced Item out of the available Items
    /// </summary>
    /// <param name="itemName"></param>
    public void createProperty(string itemName) {

        if (availableItems.TryGetValue(itemName, out tempItem)) {

            Property newProperty = new Property(tempItem);
            propertyList.Add(newProperty);

            Debug.Log("Item " + itemName + " created and sent to inventory.");
        } else {
            Debug.LogError("No Item in GameObject Items with name " + itemName);
        }

    }

    /// <summary>
    /// Removes a Property out of the Inventory
    /// </summary>
    /// <param name="item"></param>
    public void removeProperty(ItemTemplate item) {
        containsItem(item, true);
    }

    /// <summary>
    /// Removes a Property out of the Inventory
    /// </summary>
    /// <param name="item"></param>
    public void removeProperty(Property prop) {
        propertyList.Remove(prop);
    }


    /// <summary>
    /// Returns the complete PropertyList
    /// </summary>
    /// <returns></returns>
    public List<Property> getPropertyList() {
        return propertyList;
    }

    /// <summary>
    /// Returns the PropertyList, but only the Properties which are unused by sth
    /// </summary>
    /// <returns></returns>
    public List<Property> getPropertyListOnlyUnused() {
        TempPropertyList = new List<Property>();
        foreach (Property prop in propertyList) {
            if (!prop.isInUse()) {
                TempPropertyList.Add(prop);
            }
        }
        return TempPropertyList;
    }


    /// <summary>
    /// Check, if Inventory contains a copy of a specific Property<br></br> 
    /// </summary>
    /// <param name="prop"></param>
    /// <returns></returns>
    public bool containsProperty(Property prop) {
        return (propertyList.Contains(prop));
    }

    /// <summary>
    /// Check, if Inventory contains a copy of a specific Item in any Property<br></br>
    /// is able to delete the Property holding it, if found
    /// </summary>
    /// <returns></returns>
    public bool containsItem(ItemTemplate item, bool deleteIt = false) {
        returnBool = false;

        foreach (Property invProperty in propertyList) {
            if (invProperty.ReferencedItem.name == item.name) {
                returnBool = true;
                if (deleteIt) {
                    propertyList.Remove(invProperty);
                }
            }
        }

        return returnBool;
    }





    /// <summary>
    /// Searches the Item-Root-GameObject and creates a Dictionary of Items
    /// </summary>
    public void InitItemTemplates() {
        availableItems = new Dictionary<string, ItemTemplate>();
        try {
            // Search for all Items in our Items-GameObject
            tempItems = Globals.UICanvas.uiElements.Items.GetComponentsInChildren<ItemTemplate>(true);

            // Go Through all Items and add them to availableItems
            foreach (ItemTemplate currentItem in tempItems) {
                availableItems.Add(currentItem.name, currentItem);
            }
        }
        catch (System.Exception e){
            Debug.LogError(e);
        }
    }

    /// <summary>
    /// Inits all Properties in the Inventory
    /// </summary>
    public void InitProperties() {
        try {

            foreach (Property prop in propertyList) {
                prop.initProperty();
            }
        }
        catch {
            Debug.LogError("No Items in GameObject Items");
        }

    }

}

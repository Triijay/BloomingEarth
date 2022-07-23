using Bayat.Json;
using System;
using UnityEngine;

/// <summary>
/// With BuildingUpgrades User can modify his Buildings to be eg. more productive or more environment-friendly
/// </summary>
public class BuildingUpgrade : ItemTemplate {


    public enum CompatibleBuildingTypes {
        AllBuildings,
        EnviBuildingOnly,
        HybridBuilding,
        HybridBuildingOnlyNegativeForEnvironment,
        HybridBuildingPositiveOrNeutralForEnvironment,
    }

    public CompatibleBuildingTypes compatibleBuildingTypes;

    /// <summary>
    /// What Buildings can get this Upgrade<br></br>
    /// Not Saved 
    /// </summary>
    [Tooltip("What Buildings can get this Upgrade\n- When this list is empty, it can affect every Building")]
    public Building[] compatibleBuildings;

    public float factorEnvironmentAffection = 1;

    public float factorCoinProducing = 1;

    bool returnBool, buildingIsInCompatibleBuildings;

    BuildingUpgrade templateItem;




    /// <summary>
    /// Checks if this BuildingUpgrade can be applied to a specific building
    /// </summary>
    /// <param name="building"></param>
    /// <returns></returns>
    public bool isBuildingUpgradeCompatibleTo(Building building) {

        returnBool = false;

        // Check Compatible Building Types
        switch (compatibleBuildingTypes) {
            case CompatibleBuildingTypes.HybridBuilding:
                if (building as HybridBuilding != false) {
                    returnBool = true;
                }
                break;
            case CompatibleBuildingTypes.HybridBuildingOnlyNegativeForEnvironment:
                if (building as HybridBuilding != false && ((HybridBuilding)building).environmentFactorOR < 0 ) {
                    returnBool = true;
                }
                break;
            case CompatibleBuildingTypes.HybridBuildingPositiveOrNeutralForEnvironment:
                if (building as HybridBuilding != false && ((HybridBuilding)building).environmentFactorOR >= 0) {
                    returnBool = true;
                }
                break;
            case CompatibleBuildingTypes.EnviBuildingOnly:
                if (building as EnviBuilding != false && building as HybridBuilding == false) {
                    returnBool = true;
                }
                break;
            case CompatibleBuildingTypes.AllBuildings:
            default:
                returnBool = true;
                break;
        }

        // Check Compatible Building List

        if (compatibleBuildings.Length > 0) {
            // if compatibleBuildings has buildings in it, we check, if the building is in that list
            buildingIsInCompatibleBuildings = false;
            foreach (Building checkedBuilding in compatibleBuildings) {
                if (checkedBuilding == building) {
                    buildingIsInCompatibleBuildings = true;
                }
            }

            returnBool = returnBool && buildingIsInCompatibleBuildings;
        }


        return returnBool;
    }



    /// <summary>
    /// Fills a (Transform)GameObject with the item Parameters
    /// </summary>
    /// <param name="objectToFill"></param>
    public override void fillItemTemplateWithInfos(Transform objectToFill) {
        base.fillItemTemplateWithInfos(objectToFill);

        try { 
            if (factorCoinProducing > 1) {
                objectToFill.Find("BuildingUpgrade/CoinBonus").GetComponent<TMPro.TextMeshProUGUI>().text = ((factorCoinProducing - 1) * 100).ToString("N0") + "%";
                objectToFill.Find("BuildingUpgrade/CoinBonus").GetComponent<TMPro.TextMeshProUGUI>().color = new Color32(117, 176, 17, 255);
            } else if (factorCoinProducing < 1) {
                objectToFill.Find("BuildingUpgrade/CoinBonus").GetComponent<TMPro.TextMeshProUGUI>().text = "-" + ((1 - factorCoinProducing) * 100).ToString("N0") + "%";
                objectToFill.Find("BuildingUpgrade/CoinBonus").GetComponent<TMPro.TextMeshProUGUI>().color = new Color32(190, 39, 8, 255);
            } else {
                objectToFill.Find("BuildingUpgrade/CoinBonus").GetComponent<TMPro.TextMeshProUGUI>().text = "0%";
                objectToFill.Find("BuildingUpgrade/CoinBonus").GetComponent<TMPro.TextMeshProUGUI>().color = new Color32(166, 166, 166, 255);
            }

            if (factorEnvironmentAffection > 1) {
                objectToFill.Find("BuildingUpgrade/EnviBonus").GetComponent<TMPro.TextMeshProUGUI>().text = ((factorEnvironmentAffection - 1) * 100).ToString("N0") + "%";
                objectToFill.Find("BuildingUpgrade/EnviBonus").GetComponent<TMPro.TextMeshProUGUI>().color = new Color32(190, 39, 8, 255);
            } else if (factorEnvironmentAffection < 1) {
                objectToFill.Find("BuildingUpgrade/EnviBonus").GetComponent<TMPro.TextMeshProUGUI>().text = "-" + ((1 - factorEnvironmentAffection) * 100).ToString("N0") + "%";
                objectToFill.Find("BuildingUpgrade/EnviBonus").GetComponent<TMPro.TextMeshProUGUI>().color = new Color32(117, 176, 17, 255);
            } else {
                objectToFill.Find("BuildingUpgrade/EnviBonus").GetComponent<TMPro.TextMeshProUGUI>().text = "0%";
                objectToFill.Find("BuildingUpgrade/EnviBonus").GetComponent<TMPro.TextMeshProUGUI>().color = new Color32(166, 166, 166, 255);
            }

            objectToFill.Find("BuildingUpgrade").gameObject.SetActive(true);
        } catch { }
    }
}

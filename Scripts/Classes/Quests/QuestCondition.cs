using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[System.Serializable]
public class QuestCondition {

    /// <summary>
    /// Finish-Condition for this Quest
    /// </summary>
    public ConditionTypes conditionType;
    /// <summary>
    ///  A Quest can have several Conditions to finish it. Current Contitions are:<br></br>
    ///  Building - a specific Level of a Building
    /// </summary>
    public enum ConditionTypes {
        Building,
    }

    /// <summary>
    /// If Condition is a Building Level
    /// </summary>
    public int buildingLevel = 1;
    /// <summary>
    /// If Condition is a Building Level
    /// </summary>
    public Building building;



}
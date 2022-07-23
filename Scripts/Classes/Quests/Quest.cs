using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bayat.Json;

/// <summary>
/// A Quest in the Game - User has to fulfil a Quest to get a Reward
/// </summary>
[System.Serializable]
public class Quest {

    /// <summary>
    /// There are different Quest Types in our Game
    /// </summary>
    public QuestTypes questType;
    public enum QuestTypes {
        Quest,
        InfoPopUpWithReward,
        InfoPopUp,
    }

    /// <summary>
    /// Current State of a Quest
    /// </summary>
    public QuestStates questState;
    public enum QuestStates {
        NotStarted,
        InProgress,
        Finished,
    }

    /// <summary>
    /// Key in the Language-Files for the PopUp-Heading
    /// </summary>
    public string langKeyHeading;

    /// <summary>
    /// Key in the Language-Files for the PopUp: Start Description (only for Quests)
    /// </summary>
    public string langKeyTextStart;

    /// <summary>
    /// Key in the Language-Files for the PopUp: Finish Description (for all Quests and Info-PopUps)
    /// </summary>
    public string langKeyTextFinish;

    /// <summary>
    /// the Condition to finish the Quest (eG a specific Level of a Building)
    /// </summary>
    public QuestCondition finishCondition;

    /// <summary>
    /// A List of Rewards, what the User gets, when he finished the Quest
    /// </summary>
    public List<QuestReward> rewardList;

}


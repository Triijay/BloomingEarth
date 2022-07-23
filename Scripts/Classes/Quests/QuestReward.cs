using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[System.Serializable]
public class QuestReward {


    public RewardTypes rewardType;
    /// <summary>
    ///  A Quest can have several Types of Rewards. Current Reward types are:<br></br>
    ///  Emeralds - an amount of Premium-Currency<br></br>
    ///  ItemForInventory - a Item (like a Statue)<br></br>
    ///  Coins - an amount of Coins<br></br>
    /// </summary>
    public enum RewardTypes {
        Emeralds,
        ItemForInventory,
        Coins,
    }

    /// <summary>
    /// If RewardType == Coins
    /// </summary>
    public IdleNum amountCoins;
    /// <summary>
    /// If RewardType == Emeralds
    /// </summary>
    public int amountEmeralds;

    /// <summary>
    /// If RewardType == ItemForInventory
    /// </summary>
    public ItemTemplate itemForInventory;



}
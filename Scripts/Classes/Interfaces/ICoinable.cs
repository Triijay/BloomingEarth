using UnityEngine;
using UnityEditor;

public interface ICoinable {

    /// <summary>
    /// Generate Coins and add them to the Global IdleNum Var
    /// </summary>
    void initProduceCoins();
}
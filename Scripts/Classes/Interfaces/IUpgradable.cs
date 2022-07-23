using System.Collections.Generic;
using UnityEditor;

public interface IUpgradable {

    /// <summary>
    /// Print Information for the Labels
    /// </summary>
    /// <returns>String with building Information</returns>
    Dictionary<string, string> getInfo(int levelUpAmount);

    /// <summary>
    /// Function to Level up the Building and set some Vars
    /// </summary>
    /// <param name="amount">How much levels we should level up</param>
    bool levelUp(int amount, bool prepayed);
}
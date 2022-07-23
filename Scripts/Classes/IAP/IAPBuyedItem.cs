using UnityEngine;
using UnityEditor;
using Bayat.Json;

/// <summary>
/// Holds the Information about an Item the User bought
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class IAPBuyedItem {

    [JsonProperty(PropertyName = "productID")]
    public string productID;

    [JsonProperty(PropertyName = "shopName")]
    public string shopName;

    [JsonProperty(PropertyName = "localizedPriceString")]
    public string localizedPriceString;

    [JsonProperty(PropertyName = "utcDateTimestamp")]
    public string utcDateTimestamp;

    /// <summary>
    /// Amount of Emeralds, the User spent, -1 if bought for real money
    /// </summary>
    [JsonProperty(PropertyName = "emeraldAmount")]
    public int emeraldAmount;

    /// <summary>
    ///  Constructor with all necessary Information
    /// </summary>
    /// <param name="productID"></param>
    /// <param name="shopName"></param>
    /// <param name="localizedPriceString"></param>
    /// <param name="utcDateTimestamp">Amount of Emeralds, the User spent, -1 if bought for real money</param>
    public IAPBuyedItem(string productID, string shopName, string localizedPriceString, string utcDateTimestamp, int emeraldAmount) {
        this.productID = productID;
        this.shopName = shopName;
        this.localizedPriceString = localizedPriceString;
        this.utcDateTimestamp = utcDateTimestamp;
        this.emeraldAmount = emeraldAmount;
    }

}
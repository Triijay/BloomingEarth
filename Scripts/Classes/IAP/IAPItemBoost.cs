using UnityEngine.Purchasing;

public class IAPItemBoost : IAPItem {

    public float multiplier;
    public int hours;

    /// <summary>
    /// Gets the Translated Texts out of Google
    /// </summary>
    public override void translateShopItem(IStoreController m_StoreController) {

        base.translateShopItem(m_StoreController);


    }

    /// <summary>
    /// Post-Inits the Shop Item by InternetConnection<br></br>
    /// Locks Buttons, Descriptions etc, when no Connection
    /// </summary>
    public override void reactToInternetConnection() {
        base.reactToInternetConnection();
    }

    public override void ExecuteBuyedItem() {
        Globals.Game.currentWorld.CoinIncomeManager.itemBoostTimer.activateBoost(multiplier, hours * 60 * 60);
    }

}

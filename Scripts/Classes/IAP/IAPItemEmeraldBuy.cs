using UnityEngine.Purchasing;

public class IAPItemEmeraldBuy : IAPItem {

    public int emeraldsToUser;


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
        Globals.UICanvas.uiElements.ParticleEffects.addEmeralds(emeraldsToUser);
    }

}

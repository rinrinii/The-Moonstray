using UnityEngine;

public class ShopInteractionResponse : MonoBehaviour, IInteractionResponse
{
    public void OnInteract()
    {
        if (GameplayUIManager.Instance == null ||
            GameplayUIManager.Instance.Shop == null)
        {
            Debug.LogError("ShopInteractionResponse could not find ShopUI.");
            return;
        }

        GameplayUIManager.Instance.Shop.OpenShop();
    }
}

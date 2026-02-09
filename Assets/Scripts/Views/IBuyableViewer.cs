using Managers;
using Models.Shop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Views
{
    public class IBuyableViewer : MonoBehaviour
    {
        [SerializeField] private Button buyButton;
        [SerializeField] private TextMeshProUGUI priceLabel;

        private ShopItemBundle _bundle;

        public void Initialize(ShopItemBundle bundle)
        {
            _bundle = bundle;

            priceLabel.text = $"{bundle.Price}$";

            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnBuyClicked);
            buyButton.interactable = bundle.CanBuy();
        }

        private void OnBuyClicked()
        {
            if (_bundle.Purchase())
            {
                buyButton.interactable = false;
            }
            else
            {
                // => You're poor. 
            }
        }
    }
}
using CleanArchitecture.UseCaseLayer;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace CleanArchitecture.OuterLayer.ViewLayer
{
    public class ShopController : MonoBehaviour
    {
        [SerializeField]
        private Button _buyButton;

        [SerializeField] private InputField _itemIdInputField;
        [SerializeField] private InputField _itemAmountInputField;

        private IBuyItemInShopInputBoundary _buyItemInShopInputBoundary;


        public void Initialize(IBuyItemInShopInputBoundary buyItemInShopInputBoundary)
        {
            _buyItemInShopInputBoundary = buyItemInShopInputBoundary;
        }

        public void Start()
        {
            _buyButton.OnClickAsObservable().Subscribe(_ =>
            {
                var itemId = _itemIdInputField.text;
                var itemAmount = _itemAmountInputField.text;

                var inputData = new ShopUseCaseInputData
                {
                    //use better processing
                    SelectedItemId = int.Parse(itemId),
                    BuyQuantity = int.Parse(itemAmount)
                };

                _buyItemInShopInputBoundary.ProcessPurchase(inputData);
            });
        }
    }
}
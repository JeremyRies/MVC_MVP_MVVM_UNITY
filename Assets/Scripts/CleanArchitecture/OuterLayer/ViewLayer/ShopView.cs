using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace CleanArchitecture.OuterLayer.ViewLayer
{
    public class ShopView : MonoBehaviour
    {
        private ShopViewModel _shopViewModel;

        [SerializeField] private Text _itemCountText;

        public void Initialize(ShopViewModel shopViewModel)
        {
            _shopViewModel = shopViewModel;

            _shopViewModel.BoughtCount.Subscribe(count =>
            {
                _itemCountText.text = "Bought: " + count;
            }).AddTo(this);
        }
    }
}
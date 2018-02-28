using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class MVVMCode
    {
        public void EntryPoint()
        {
            var model = new BuyItemViewModel(new MockStore(), new MockItemType(), new MockInventory());
            //normally you would instantiate a prefab here - when mocking you can use a proper constructor though
            var view = new BuyItemViewMock(model);
            view.ExcecuteBuy();
        }
    }

    public class BuyItemViewMock
    {
        private IBuyItemViewModel _viewModel;

        public BuyItemViewMock(IBuyItemViewModel viewModel)
        {
            this._viewModel = viewModel;
        }

        public void ExcecuteBuy()
        {
            _viewModel.BuyCommand.Execute();
        }
    }

    public class BuyItemView : MonoBehaviour
    {
        public Button BuyButton;
        public Text ItemNameText;
        public Text ItemCountText;

        public void Initialize(IBuyItemViewModel viewModel)
        {
            ItemNameText.text = viewModel.Name;
            viewModel.BuyCommand.BindTo(BuyButton);
            viewModel.Amount.Subscribe(val =>
            {
                ItemCountText.text = "count: " + val;
            });
        }
    }

    public interface IBuyItemViewModel
    {
        string Name { get; }
        ReactiveProperty<int> Amount { get; }
        ReactiveCommand BuyCommand { get; }
    }

    public class BuyItemViewModel : IBuyItemViewModel
    {
        private readonly IStore2 _store;
        private readonly IItemType _item;
        private readonly IInventory _inventory;
        private readonly ReactiveCommand _buyCommand;

        public BuyItemViewModel(IStore2 store, IItemType item, IInventory inventory)
        {
            _store = store;
            _item = item;
            _inventory = inventory;
            _buyCommand = new ReactiveCommand(_store.CanBuy(item));
            _buyCommand.Subscribe(_ => _store.Buy(item));
        }

        public string Name
        {
            get { return _item.Name; }
        }

        public ReactiveProperty<int> Amount
        {
            get { return _inventory.AmountOf(_item); }
        }

        public ReactiveCommand BuyCommand
        {
            get { return _buyCommand; }
        }
    }

    public interface IStore2
    {
        IObservable<bool> CanBuy(IItemType item);
        void Buy(IItemType item);
    }

    public interface IInventory
    {
        ReactiveProperty<int> AmountOf(IItemType item);
    }

    public interface IItemType
    {
        string Name { get; }
    }


    class MockItemType : IItemType
    {
        public string Name
        {
            get { throw new System.NotImplementedException(); }
        }
    }

    class MockInventory : IInventory
    {
        public ReactiveProperty<int> AmountOf(IItemType item)
        {
            throw new System.NotImplementedException();
        }
    }

    class MockStore : IStore2
    {
        public IObservable<bool> CanBuy(IItemType item)
        {
            throw new System.NotImplementedException();
        }

        public void Buy(IItemType item)
        {
            throw new System.NotImplementedException();
        }
    }
}
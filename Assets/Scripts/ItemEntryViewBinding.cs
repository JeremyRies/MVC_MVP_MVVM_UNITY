using System;
using NUnit.Framework;
using UniRx;
using UnityEngine;
using UnityEngine.Purchasing.Extension;
using UnityEngine.UI;

public class MVCItemEntryView : MonoBehaviour
{
    [SerializeField] private ItemEntryViewBinding _viewBinding;
    private ButtonView _buyButtonView;
    private IItemModel _model;

    public string NameText
    {
        set { _viewBinding.ItemNameText.text = value; }
    }

    public int ItemCount
    {
        set { _viewBinding.ItemCountText.text = "Count: " + value; }
    }

    public AbstractButtonView ButtonView
    {
        get { return _buyButtonView; }
    }

    public void Initialize(IItemModel itemModel)
    {
        _model = itemModel;
        _buyButtonView = new ButtonView(_model.Buy, _viewBinding.BuyButton);
        NameText = _model.ItemName;

        _model.ItemCount.Subscribe(val =>
        {
            ItemCount = val;
            ButtonView.ButtonActive = val < 5;
        });
    }
}

public class ItemEntryViewBinding : MonoBehaviour
{
    public Button BuyButton;

    public Text ItemNameText;
    public Text ItemCountText;
}

public class ItemEntryView : MonoBehaviour, IItemEntryView
{
    [SerializeField] private ItemEntryViewBinding _viewBinding;
    private ButtonView _buyButtonView;

    public string NameText
    {
        set { _viewBinding.ItemNameText.text = value; }
    }

    public int ItemCount
    {
        set { _viewBinding.ItemCountText.text = "Count: "+ value; }
    }

    public AbstractButtonView ButtonView
    {
        get { return _buyButtonView; }
    }

    public void Initialize(Action action)
    {
        _buyButtonView = new ButtonView(action,_viewBinding.BuyButton);
    }
}

public interface IItemEntryView
{
    string NameText { set; }
    int ItemCount { set; }
    AbstractButtonView ButtonView { get; }
    void Initialize(Action action);
}

public class ItemEntryViewMock : IItemEntryView
{
    private AbstractButtonView _buttonView;
    public string NameText { get; set; }

    public int ItemCount { get; set; }

    public AbstractButtonView ButtonView
    {
        get { return _buttonView; }
    }

    public void Initialize(Action action)
    {
        _buttonView = new MockButtonView(action);
    }
}

public class ItemModel : IItemModel
{
    private string _itemName = "Life Blob";
    public ReactiveProperty<int> ItemCount { get; private set; }

    public string ItemName
    {
        get { return _itemName; }
    }

    public void Buy()
    {
        ItemCount.Value++;
    }
}

public interface IItemModel
{
    string ItemName { get; }
    ReactiveProperty<int> ItemCount { get; }
    void Buy();
}

public class ItemEntryController
{
    private IItemEntryView _view;
    private readonly IItemModel _model;

    public ItemEntryController(IItemEntryView view, IItemModel model)
    {
        _view = view;
        _model = model;
    }

    public void Setup()
    {
        _view.Initialize(() => _model.Buy());
        _view.NameText = _model.ItemName;

        _model.ItemCount.Subscribe(val =>
        {
            _view.ItemCount = val;
            _view.ButtonView.ButtonActive = val < 5;
        });
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


public class BuyItemViewTests
{
    [Test]
    public void AmountShouldMatchInventoryAmount()
    {
        var model = new BuyItemViewModel(null, null, MockInventory.WithCountForType(type, 2));
        Assert.AreEqual(2, model.Amount);
        MockInventory.IncreaseAmount(type, 1);
        Assert.AreEqual(3, model.Amount);
    }
}


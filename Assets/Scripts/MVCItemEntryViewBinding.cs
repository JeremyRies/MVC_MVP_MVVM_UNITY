using System;
using NUnit.Framework;
using UniRx;
using UnityEngine;
using UnityEngine.Purchasing.Extension;
using UnityEngine.UI;

public class MVCItemEntryViewBinding : MonoBehaviour
{
    public Button BuyButton;

    public Text ItemNameText;
    public Text ItemCountText;
}

public class ItemEntryView : MonoBehaviour, IItemEntryView
{
    [SerializeField] private MVCItemEntryViewBinding _viewBinding;
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


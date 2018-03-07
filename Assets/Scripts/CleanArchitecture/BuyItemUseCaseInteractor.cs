using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace CleanArchitecture
{
    public class ViewPrefab : MonoBehaviour
    {
        public ShopInputSystem ShopInputSystem;
        public ShopView ShopView;
    }

    public class DependencyRoot : MonoBehaviour
    {
        [SerializeField] private ViewPrefab _viewPrefab;

        void Awake()
        {
            var viewPrefab = Instantiate(_viewPrefab);
            var input = viewPrefab.ShopInputSystem;
            var view = viewPrefab.ShopView;

            var database = new Database();
            var dataAccess = new DataAccess(database);

            var viewModel = new ShopViewModel();
            var outPutBoundary = new Presenter(viewModel);

            var useCaseInteractor = new BuyItemUseCaseInteractor(dataAccess, outPutBoundary);

            var controller = new ShopController(useCaseInteractor);

            view.Initialize(viewModel);
            input.Initialize(controller);
        }
    }



    public class ShopInputSystem : MonoBehaviour
    {
        [SerializeField]
        private Button _buyButton;

        [SerializeField] private InputField _itemIdInputField;
        [SerializeField] private InputField _itemAmountInputField;

        private ShopController _shopController;

        public void Initialize(ShopController shopController)
        {
            _shopController = shopController;
        }

        public void Start()
        {
            _buyButton.OnClickAsObservable().Subscribe(_ =>
            {
                var itemId = _itemIdInputField.text;
                var itemAmount = _itemAmountInputField.text;
                _shopController.TryPurchase(itemId,itemAmount);
            });
        }
    }

    public class ShopController
    {
        private readonly IInputBoundary _inputBoundary;

        public ShopController(IInputBoundary inputBoundary)
        {
            _inputBoundary = inputBoundary;
        }

        public void TryPurchase(string itemId, string count)
        {
            var inputData = new ShopUseCaseInputData
            {
                //use better processing
                SelectedItemId = int.Parse(itemId),
                BuyQuantity = int.Parse(count)
            };
            _inputBoundary.ProcessPurchase(inputData);
        }
    }

    public struct ShopUseCaseInputData
    {
        public int SelectedItemId;
        public int BuyQuantity;
    }


    public interface IInputBoundary
    {
        void ProcessPurchase(ShopUseCaseInputData inputData);
    }

    public class BuyItemUseCaseInteractor : IInputBoundary
    {
        private readonly IDataAccessInterface _dataAccessInterface;
        private readonly IOutPutBoundary _outPutBoundary;

        public BuyItemUseCaseInteractor(IDataAccessInterface dataAccessInterface, IOutPutBoundary outPutBoundary)
        {
            _dataAccessInterface = dataAccessInterface;
            _outPutBoundary = outPutBoundary;
        }

        public void ProcessPurchase(ShopUseCaseInputData inputData)
        {
            try
            {
                var currentCount = _dataAccessInterface.GetItemCount(inputData.SelectedItemId);
                if (inputData.BuyQuantity + currentCount > _dataAccessInterface.MaxPossibleItemCount())
                {
                    var outPutData = new FailedAttemptOutputData
                    {
                        BuyQuantity = inputData.BuyQuantity,
                        SelectedItemId = inputData.SelectedItemId,
                        InventoryQuantity = currentCount,
                        FailureReason = FailureReason.TooManyItems
                    };
                    _outPutBoundary.DisplayFailedAttempt(outPutData);
                }
                else
                {
                    _dataAccessInterface.StoreItemCount(inputData.SelectedItemId, inputData.BuyQuantity + currentCount);
                    var newCount = _dataAccessInterface.GetItemCount(inputData.SelectedItemId);
                    _outPutBoundary.DisplaySuccessfullAttempt(new SuccessfullAttemptOutputData
                    {
                        BuyQuantity = inputData.BuyQuantity,
                        NewInventoryQuantity = newCount,
                        SelectedItemId = inputData.SelectedItemId
                    });
                }
            }
            catch (Exception dataBaseAccessException)
            {
                var outPutData = new FailedAttemptOutputData
                {
                    BuyQuantity = inputData.BuyQuantity,
                    SelectedItemId = inputData.SelectedItemId,
                    FailureReason = FailureReason.NoDatabaseAccess
                };
                _outPutBoundary.DisplayFailedAttempt(outPutData);
            }

        }
    }

    public interface IOutPutBoundary
    {
        void DisplayFailedAttempt(FailedAttemptOutputData outputData);
        void DisplaySuccessfullAttempt(SuccessfullAttemptOutputData outputData);
    }

    public class Presenter : IOutPutBoundary
    {
        private readonly ShopViewModel _shopViewModel;

        public Presenter(ShopViewModel shopViewModel)
        {
            _shopViewModel = shopViewModel;
        }

        public void DisplayFailedAttempt(FailedAttemptOutputData outputData)
        {
            _shopViewModel.BoughtCount.Value = outputData.BuyQuantity;

            switch (outputData.FailureReason)
            {
                case FailureReason.NoDatabaseAccess:
                    _shopViewModel.ErrorMessage.Value = "No Database Access possible";
                    _shopViewModel.InventoryCount.Value = 0;
                    _shopViewModel.BoughtCount.Value = 0;
                    break;
                case FailureReason.TooManyItems:
                    _shopViewModel.ErrorMessage.Value = "Too Many Items";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void DisplaySuccessfullAttempt(SuccessfullAttemptOutputData outputData)
        {
            throw new NotImplementedException();
        }
    }

    public class ShopViewModel
    {
        public ReactiveProperty<int> BoughtCount = new ReactiveProperty<int>();
        public ReactiveProperty<int> InventoryCount = new ReactiveProperty<int>();
        public ReactiveProperty<int> ItemId = new ReactiveProperty<int>();

        public ReactiveProperty<string> ErrorMessage = new ReactiveProperty<string>();
    }

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

    public struct FailedAttemptOutputData
    {
        public int SelectedItemId;
        public int BuyQuantity;
        public int? InventoryQuantity;

        public FailureReason FailureReason;
    }

    public enum FailureReason
    {
        NoDatabaseAccess,
        TooManyItems
    }

    public struct SuccessfullAttemptOutputData
    {
        public int SelectedItemId;
        public int BuyQuantity;
        public int NewInventoryQuantity;
    }


    public interface IDataAccessInterface
    {
        void StoreItemCount(int itemId, int count);
        int GetItemCount(int itemId);
        int MaxPossibleItemCount();
    }

    //should actually write sql code
    public class DataAccess : IDataAccessInterface
    {
        private Database _database;

        public DataAccess(Database database)
        {
            _database = database;
        }

        public void StoreItemCount(int itemId, int count)
        {
            if(itemId > 5)
                throw new ArgumentException();
            _database.Items[itemId] = count;
        }

        public int GetItemCount(int itemId)
        {
            return _database.Items[itemId];
        }

        public int MaxPossibleItemCount()
        {
            return 10;
        }
    }

    //should be a real mysql database
    public class Database
    {
        public Dictionary<int, int> Items = new Dictionary<int, int>();
    }
}

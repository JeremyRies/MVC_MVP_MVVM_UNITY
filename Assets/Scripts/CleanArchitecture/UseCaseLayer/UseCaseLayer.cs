using System;
using System.Collections.Generic;
using UniRx;

namespace CleanArchitecture.UseCaseLayer
{
    public class InternetDependantItemCreator : IItemCreator
    {
        public ItemEntity CreateItemEntity(int id)
        {
            throw new NotImplementedException();
        }
    }

    public struct ShopUseCaseInputData
    {
        public int SelectedItemId;
        public int BuyQuantity;
    }

    public class BuyItemUseCase : IBuyItemInShopInputBoundary
    {
        private readonly IShopDataAccess _shopDataAccess;
        private readonly IShopBuyItemOutput _shopBuyItemOutput;
        private readonly ShopEntity _shop;
        private readonly InventoryEntity _inventory;

        public BuyItemUseCase(IShopDataAccess shopDataAccess, IShopBuyItemOutput shopBuyItemOutput, ShopEntity shop, InventoryEntity inventory)
        {
            _shopDataAccess = shopDataAccess;
            _shopBuyItemOutput = shopBuyItemOutput;
            _shop = shop;
            _inventory = inventory;
        }

        public void ProcessPurchase(ShopUseCaseInputData inputData)
        {
            var id = inputData.SelectedItemId;
            var quantity = inputData.BuyQuantity;

            try
            {
                var currentCount = _shopDataAccess.GetItemCount(id);
                if (!_shop.CanBuyItem(id,quantity))
                {
                    var outPutData = new FailedAttemptOutputData
                    {
                        BuyQuantity = quantity,
                        SelectedItemId = id,
                        InventoryQuantity = currentCount,
                        FailureReason = FailureReason.NotEnoughMoney
                    };
                    _shopBuyItemOutput.DisplayFailedAttempt(outPutData);
                }
                else
                {
                    _shop.BuyItem(id,quantity);

                    _shopDataAccess.StoreItemCount(id, _inventory.GetItemCount(id));
                    var newCount = _shopDataAccess.GetItemCount(id);

                    _shopBuyItemOutput.DisplaySuccessfullAttempt(new SuccessfullAttemptOutputData
                    {
                        BuyQuantity = quantity,
                        NewInventoryQuantity = newCount,
                        SelectedItemId = id
                    });
                }
            }
            catch (Exception dataBaseAccessException)
            {
                var outPutData = new FailedAttemptOutputData
                {
                    BuyQuantity = quantity,
                    SelectedItemId = id,
                    FailureReason = FailureReason.NoDatabaseAccess
                };
                _shopBuyItemOutput.DisplayFailedAttempt(outPutData);
            }
        }
    }

    public interface IShopDataAccess
    {
        void StoreItemCount(int itemId, int count);
        int GetItemCount(int itemId);

        Dictionary<int, double> ItemPrices();
    }

    public interface IBuyItemInShopInputBoundary
    {
        void ProcessPurchase(ShopUseCaseInputData inputData);
    }


    public interface IShopBuyItemOutput
    {
        void DisplayFailedAttempt(FailedAttemptOutputData outputData);
        void DisplaySuccessfullAttempt(SuccessfullAttemptOutputData outputData);
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
        NotEnoughMoney
    }

    public struct SuccessfullAttemptOutputData
    {
        public int SelectedItemId;
        public int BuyQuantity;
        public int NewInventoryQuantity;
    }

}

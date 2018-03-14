using System;
using CleanArchitecture.UseCaseLayer;
using UniRx;

namespace CleanArchitecture.OuterLayer.ViewLayer
{
    public class ShopViewModel : IShopBuyItemOutput
    {
        public ReactiveProperty<int> BoughtCount = new ReactiveProperty<int>();
        public ReactiveProperty<int> InventoryCount = new ReactiveProperty<int>();
        public ReactiveProperty<int> ItemId = new ReactiveProperty<int>();

        public ReactiveProperty<string> ErrorMessage = new ReactiveProperty<string>();

        public void DisplayFailedAttempt(FailedAttemptOutputData outputData)
        {
            BoughtCount.Value = outputData.BuyQuantity;

            switch (outputData.FailureReason)
            {
                case FailureReason.NoDatabaseAccess:
                    ErrorMessage.Value = "No Database Access possible";
                    InventoryCount.Value = 0;
                    BoughtCount.Value = 0;
                    break;
                case FailureReason.NotEnoughMoney:
                    ErrorMessage.Value = "Too Many Items";
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
}
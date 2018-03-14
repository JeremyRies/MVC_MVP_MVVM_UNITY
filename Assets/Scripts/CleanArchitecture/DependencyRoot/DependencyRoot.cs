using System.ComponentModel;
using CleanArchitecture.OuterLayer.DataLayer;
using CleanArchitecture.OuterLayer.ViewLayer;
using CleanArchitecture.UseCaseLayer;
using JetBrains.Annotations;
using UnityEngine;

namespace CleanArchitecture.OuterLayer
{

    public class DependencyRoot : MonoBehaviour
    {
        [SerializeField] private ViewPrefab _viewPrefab;

        void Awake()
        {
            var viewPrefab = Instantiate(_viewPrefab);
            var input = viewPrefab.ShopController;
            var view = viewPrefab.ShopView;

            var database = new Database();
            var dataAccess = new ShopDataAccess(database);

            var viewModel = new ShopViewModel();

            // var itemCreator = new StandardLazyItemCreator( new List<ItemBluePrint>{new ItemBluePrint{Id = 1, UsageAction =() => { } }} );

            var itemCreator = new InternetDependantItemCreator();
            var inventory = new InventoryEntity(itemCreator);

            var prices = dataAccess.ItemPrices();
            var resources = new ResourceEntity();
            var shop = new ShopEntity(inventory, resources, new ItemPriceData { Prices = prices });


            var useCaseInteractor = new BuyItemUseCase(dataAccess, viewModel, shop, inventory);

            view.Initialize(viewModel);
            input.Initialize(useCaseInteractor);
        }
    }

}
using System;
using System.Collections.Generic;
using CleanArchitecture.UseCaseLayer;

namespace CleanArchitecture.OuterLayer.DataLayer
{
    //should actually write sql code
    public class ShopDataAccess : IShopDataAccess
    {
        private Database _database;

        public ShopDataAccess(Database database)
        {
            _database = database;
        }

        public void StoreItemCount(int itemId, int count)
        {
            if (itemId > 5)
                throw new ArgumentException();
            _database.Items[itemId] = count;
        }

        public int GetItemCount(int itemId)
        {
            return _database.Items[itemId];
        }

        public Dictionary<int, double> ItemPrices()
        {
            throw new NotImplementedException();
        }
    }

    //should be a real mysql database
    public class Database
    {
        public Dictionary<int, int> Items = new Dictionary<int, int>();
    }
}
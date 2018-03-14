using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking.NetworkSystem;

public class ItemEntity
{
    public int Id;

    private Action _activationAction;

    public ItemEntity(int id, Action activationAction)
    {
        _activationAction = activationAction;
    }

    public void Activate()
    {
        _activationAction();
    }
}

public class ItemEntry
{
    public ItemEntity Entity;
    public int ItemAmount;

    public ItemEntry(ItemEntity entity, int itemAmount)
    {
        Entity = entity;
        ItemAmount = itemAmount;
    }

    public static ItemEntry operator ++(ItemEntry entry)
    {
        entry.ItemAmount++;
        return entry;
    }

    public static ItemEntry operator +(ItemEntry entry, int amount)
    {
        entry.ItemAmount+= amount;
        return entry;
    }

    public static ItemEntry operator --(ItemEntry entry)
    {
        entry.ItemAmount--;
        return entry;
    }
}

public class Items : IEnumerable
{
    private Dictionary<int, ItemEntry> _items;

    public Items(Dictionary<int, ItemEntry> items)
    {
        _items = items;
    }

    public IEnumerator GetEnumerator()
    {
        return new ItemsEnumerator(_items.Values.ToArray());
    }

    public ItemEntry this[int itemId]
    {
        get { return _items[itemId]; }
        set { _items[itemId] = value; }
    }
}

public class ItemsEnumerator : IEnumerator
{
    private readonly ItemEntry[] _items;
    private int _position = -1;

    public ItemsEnumerator(ItemEntry[] items)
    {
        _items = items;
    }

    public bool MoveNext()
    {
        _position++;
        return (_position < _items.Length);
    }

    public void Reset()
    {
        _position = -1;
    }

    public object Current
    {
        get {
            try
            {
                return _items[_position];
            }
            catch (IndexOutOfRangeException)
            {
                throw new InvalidOperationException();
            }
        }
    }
}

public class InventoryEntity
{
    private readonly Items _items;
    private readonly IItemCreator _itemCreator;

    public InventoryEntity(IItemCreator itemCreator)
    {
        _itemCreator = itemCreator;
        _items = new Items(new Dictionary<int, ItemEntry>()); //todo get from database
    }

    public void AddItem(int itemId,int amount)
    {
        var itemEntry = _items[itemId];
        if (itemEntry != null)
        {
            _items[itemId] += amount;
        }
        else
        {
            _items[itemId] = new ItemEntry (_itemCreator.CreateItemEntity(itemId), 1);
        }
    }

    public void UseItem(int itemId)
    {
        _items[itemId].Entity.Activate();
        _items[itemId]--;
    }

    public int GetItemCount(int id)
    {
       return _items[id].ItemAmount;
    }
}

public struct ItemBluePrint
{
    public int Id;
    public Action UsageAction;
}

public interface IItemCreator
{
    ItemEntity CreateItemEntity(int id);
}

public class StandardLazyItemCreator : IItemCreator
{
    private readonly List<ItemBluePrint> _bluePrints;

    public StandardLazyItemCreator(List<ItemBluePrint> bluePrints)
    {
        _bluePrints = bluePrints;
    }

    public ItemEntity CreateItemEntity(int id)
    {
        return new ItemEntity(id, _bluePrints.First(bluePrint => bluePrint.Id == id).UsageAction);
    }
}

public class ItemPriceData
{
    public Dictionary<int, double> Prices;
}

public class ResourceEntity
{
    public double CurrentMoney;
}

public class ShopEntity
{
    private InventoryEntity _inventory;
    private readonly ResourceEntity _resources;
    private ItemPriceData _itemPriceData;

    public ShopEntity(InventoryEntity inventory, ResourceEntity resources, ItemPriceData itemPriceData)
    {
        _inventory = inventory;
        _resources = resources;
        _itemPriceData = itemPriceData;
    }

    public void BuyItem(int id, int quantity)
    {
        var cost = _itemPriceData.Prices[id] * quantity;
        if (_resources.CurrentMoney < cost)
            throw new InvalidOperationException();

        _resources.CurrentMoney -= cost;
        _inventory.AddItem(id,quantity);
    }

    public bool CanBuyItem(int id, int quantity)
    {
        var cost = _itemPriceData.Prices[id] * quantity;
        return _resources.CurrentMoney >= cost;
    }
}

/*using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    private List<string> _collectedItems = new List<string>();
    //private int _maxItems = 3; // Максимум предметов за раз

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public bool HasItem(string itemType)
    {
        return _collectedItems.Contains(itemType);
    }

    public bool AddItem(string itemType)
    {
        if (_collectedItems.Count >= _maxItems)
        {
            Debug.Log("Инвентарь полон!");
            return false;
        }

        _collectedItems.Add(itemType);
        Debug.Log($"Добавлен предмет: {itemType}. Всего: {_collectedItems.Count}");
        return true;
    }

    public bool RemoveItem(string itemType)
    {
        return _collectedItems.Remove(itemType);
    }

    public void ClearInventory()
    {
        _collectedItems.Clear();
    }

    public List<string> GetItems()
    {
        return new List<string>(_collectedItems);
    }
}*/
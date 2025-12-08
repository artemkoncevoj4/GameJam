using System.Collections.Generic;
using UnityEngine;
using System;
namespace Player {
    public class PlayerInventory : MonoBehaviour
    {
        public static PlayerInventory Instance { get; private set; }

        private List<string> _collectedItems = new List<string>();
        [SerializeField] private int _maxItems = 3; // �������� ��������� �� ���
        public event Action OnInventoryChanged;

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
                Debug.Log("��������� �����!");
                return false;
            }

            _collectedItems.Add(itemType);
            Debug.Log($"�������� �������: {itemType}. �����: {_collectedItems.Count}");
            OnInventoryChanged?.Invoke();
            return true;
        }

        public bool RemoveItem(string itemType)
        {
            bool removed = _collectedItems.Remove(itemType);
            if (removed)
            {
                Debug.Log($"������ �������: {itemType}");
                OnInventoryChanged?.Invoke();
            }
            return removed;
        }

        public void ClearInventory()
        {
            _collectedItems.Clear();
            OnInventoryChanged?.Invoke();
            Debug.Log("��������� ��������� ������.");
        }

        public List<string> GetItems()
        {
            return new List<string>(_collectedItems);
        }

        public int GetitemCount()
        {
            return _collectedItems.Count;
        }
    }
}
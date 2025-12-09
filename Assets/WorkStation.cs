using UnityEngine;
using System.Collections.Generic;
using Player;
using TaskSystem;
using System;
namespace InteractiveObjects
{
    public class Workstation : InteractObject
    {
        [Header("Станция для предмета")]
        [SerializeField] private string _stationType = "desk"; 
        [SerializeField] private List<string> _requiredItems = new List<string>();
        [SerializeField] private Transform _itemPlacementPoint; 
        public Action<string> OnWorkStationInteraction;

        private GameObject _placedItem;

        public override void Interact()
        {


            if (PlayerInventory.Instance == null) return;

            string itemToUse = null;
            foreach (string item in _requiredItems)
            {
                if (PlayerInventory.Instance.HasItem(item))
                {
                    itemToUse = item;
                    break;
                }
            }

            if (itemToUse != null)
            {
                Interact();
                UseStation(itemToUse);
            }
            else
            {
                Debug.Log($"��� ������������� ������� {_stationType} ����� ���� �� ���������: {string.Join(", ", _requiredItems)}");
            }
        }

        private void UseStation(string stationType)
        {
            List<string> _playerIventory = PlayerInventory.Instance.GetItems();

            OnWorkStationInteraction?.Invoke(stationType);
            
            foreach (string item in _playerIventory)
            {
                break;
            }
        }
    }
}
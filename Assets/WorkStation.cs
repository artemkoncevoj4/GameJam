using UnityEngine;
using System.Collections.Generic;
using Player;
using TaskSystem;
namespace InteractiveObjects
{
    public class Workstation : InteractObject
    {
        [Header("Станция для предмета")]
        [SerializeField] private string _stationType = "desk"; 
        [SerializeField] private List<string> _requiredItems = new List<string>();
        [SerializeField] private Transform _itemPlacementPoint; 

        private bool _isUsed = false;
        private GameObject _placedItem;

        public override void Interact()
        {
            if (_isUsed)
            {
                Debug.Log($"������� {_stationType} ��� ������������.");
                return;
            }

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

        private void UseStation(string item)
        {
            // ����� ���������� ������� �� �������
            if (PlayerInventory.Instance.RemoveItem(item))
            {
                _isUsed = true;

                // TODO: �������� ������ ������������ ��������
                // if (_itemPlacementPoint != null)
                // {
                //     _placedItem = Instantiate(ItemPrefab, _itemPlacementPoint.position, Quaternion.identity);
                // }

                // �������� TaskManager �� �������������
                if (TaskManager.Instance != null)
                {
                    // � �������� ���� ��� ����������� ���������� ID �������
                    //TaskManager.Instance.ReportStationUsed(_stationType, item, ID);
                }

                Debug.Log($"������� {_stationType} ������������ � ��������� {item}");
            }
        }

        public void ResetStation()
        {
            _isUsed = false;

            // TODO: ���������� ��� ������ _placedItem
            if (_placedItem != null)
            {
                Destroy(_placedItem);
                _placedItem = null;
            }
        }
    }
}
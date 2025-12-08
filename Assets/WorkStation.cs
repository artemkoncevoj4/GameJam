using UnityEngine;
using System.Collections.Generic;
using Player;
using TaskSystem;
namespace InteractiveObjects
{
    public class Workstation : InteractObject
    {
        [Header("��������� �������")]
        [SerializeField] private string _stationType = "desk"; // ��� �������
        [SerializeField] private List<string> _requiredItems = new List<string>();
        [SerializeField] private Transform _itemPlacementPoint; // ��� ���������� �������

        private bool _isUsed = false;
        private GameObject _placedItem;

        protected override void Start()
        {
            base.Start();
            State = "������� �������";
        }

        public override void Interact()
        {
            if (_isUsed)
            {
                Debug.Log($"������� {_stationType} ��� ������������.");
                return;
            }

            if (PlayerInventory.Instance == null) return;

            // ���������, ���� �� � ������ **����** �� ������ ���������
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
                OnInteractionStarted();
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
                State = "������������";

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
            State = "������� �������";

            // TODO: ���������� ��� ������ _placedItem
            if (_placedItem != null)
            {
                Destroy(_placedItem);
                _placedItem = null;
            }
        }
    }
}
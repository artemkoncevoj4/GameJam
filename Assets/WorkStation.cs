using UnityEngine;
using System.Collections.Generic;

namespace InteractiveObjects
{
    public class Workstation : InteractObject
    {
        [Header("Настройки станции")]
        [SerializeField] private string _stationType = "desk"; // Тип станции
        [SerializeField] private List<string> _requiredItems = new List<string>();
        [SerializeField] private Transform _itemPlacementPoint; // Где разместить предмет

        private bool _isUsed = false;
        private GameObject _placedItem;

        protected override void Start()
        {
            base.Start();
            State = "Ожидает предмет";
        }

        public override void Interact()
        {
            if (_isUsed)
            {
                Debug.Log($"Станция {_stationType} уже используется.");
                return;
            }

            if (PlayerInventory.Instance == null) return;

            // Проверяем, есть ли у игрока **один** из нужных предметов
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
                Debug.Log($"Для использования станции {_stationType} нужен один из предметов: {string.Join(", ", _requiredItems)}");
            }
        }

        private void UseStation(string item)
        {
            // Игрок использует предмет на станции
            if (PlayerInventory.Instance.RemoveItem(item))
            {
                _isUsed = true;
                State = "Использована";

                // TODO: Добавить логику визуализации предмета
                // if (_itemPlacementPoint != null)
                // {
                //     _placedItem = Instantiate(ItemPrefab, _itemPlacementPoint.position, Quaternion.identity);
                // }

                // Сообщаем TaskManager об использовании
                if (TaskManager.Instance != null)
                {
                    // В реальной игре вам потребуется передавать ID станции
                    //TaskManager.Instance.ReportStationUsed(_stationType, item, ID);
                }

                Debug.Log($"Станция {_stationType} использована с предметом {item}");
            }
        }

        public void ResetStation()
        {
            _isUsed = false;
            State = "Ожидает предмет";

            // TODO: Уничтожить или скрыть _placedItem
            if (_placedItem != null)
            {
                Destroy(_placedItem);
                _placedItem = null;
            }
        }
    }
}
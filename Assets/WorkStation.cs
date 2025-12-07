/*using UnityEngine;

namespace InteractiveObjects
{
    public class Workstation : InteractObject
    {
        [Header("Настройки станции")]
        [SerializeField] private string _stationType = "desk"; // Тип станции
        [SerializeField] private List<GameObject> _requiredItems = []; // Нужнie предметi //task manager
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
            if (_isUsed) return;

            // Проверяем, есть ли у игрока нужный предмет
            if (PlayerInventory.Instance != null)
            {
                OnInteractionStarted();
                foreach (item in _requiredItems)
                {
                    if (PlayerInventory.Instance.HasItem(item))
                    {
                        UseStation(item);
                    }
                }
                _isUsed = true;
                State = "Использована";
            }
            else
            {
                Debug.Log($"Для использования нужен: {_requiredItems}");
                // Можно показать сообщение игроку
            }
        }

        private void UseStation(string item)
        {
            // Игрок использует предмет на станции
            PlayerInventory.Instance?.RemoveItem(item);
            _requiredItemType.Remove(item)

            // Показываем визуализацию предмета
            if (item != null)
            {
                item.SetActive(true);
            }

            // Сообщаем TaskManager об использовании
            if (TaskManager.Instance != null)
            {
                TaskManager.Instance.ReportStationUsed(_stationType, item, id);
            }

            Debug.Log($"Станция {_stationType} использована с предметом {item}");
        }

        public void ResetStation()
        {
            _isUsed = false;
            State = "Ожидает предмет";

            if (_itemVisualization != null)
                _itemVisualization.SetActive(false);
        }
    }
}*/
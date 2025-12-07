/*using UnityEngine;

namespace InteractiveObjects
{
    public class CollectibleItem : InteractObject
    {
        [Header("Настройки предмета")]
        [SerializeField] private string _itemType = "ink_black"; // Тип предмета
        [SerializeField] private AudioClip _pickupSound;
        [SerializeField] private GameObject _pickupEffect;

        private bool _isCollected = false;

        protected override void Start()
        {
            base.Start();
            State = "Готов к сбору";
        }

        public override void Interact()
        {
            if (_isCollected) return;

            OnInteractionStarted();

            // Сообщаем TaskManager о взятии предмета
            if (TaskManager.Instance != null)
            {
                TaskManager.Instance.ReportItemCollected(_itemType, id); //task manager
            }

            // Визуальные и звуковые эффекты
            PlayPickupEffects();

            // Делаем предмет невидимым/неактивным
            _isCollected = true;
            State = "Собран";

            // Отключаем коллайдер и рендерер
            GetComponent<SpriteRenderer>().enabled = false;

            Debug.Log($"Предмет {_itemType} собран (ID: {id})");
        }

        private void PlayPickupEffects()
        {
            if (_pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(_pickupSound, transform.position);
            }

            if (_pickupEffect != null)
            {
                Instantiate(_pickupEffect, transform.position, Quaternion.identity);
            }
        }

        // Для отладки
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}*/
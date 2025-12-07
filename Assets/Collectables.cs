using UnityEngine;

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

            if (PlayerInventory.Instance != null && PlayerInventory.Instance.AddItem(_itemType))
            {
                // Сообщаем TaskManager о взятии предмета (для возможной внутренней логики)
                if (TaskManager.Instance != null)
                {
                    // TaskManager.Instance.ReportItemCollected(_itemType, ID); // ID из InteractObject
                }

                // Визуальные и звуковые эффекты
                PlayPickupEffects();

                // Делаем предмет невидимым/неактивным
                _isCollected = true;
                State = "Собран";

                // Отключаем коллайдер и рендерер (предполагаем наличие)
                var renderer = GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.enabled = false;
                }

                // Деактивируем на время, пока не будет респавна
                gameObject.SetActive(false);

                Debug.Log($"Предмет {_itemType} собран (ID: {ID})");
            }
            else
            {
                // Не удалось добавить предмет (инвентарь полон)
                _isCollected = false;
                State = "Готов к сбору";
            }
        }

        private void PlayPickupEffects()
        {
            if (_pickupSound != null)
            {
                // Предполагается наличие AudioSource
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
}
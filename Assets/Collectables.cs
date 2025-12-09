using UnityEngine;
using Player;
using TaskSystem;
namespace InteractiveObjects
{
    public class CollectibleItem : InteractObject
    {
        [Header("Подбор предмета")]
        [SerializeField] private AudioClip _pickupSound;
        [SerializeField] private GameObject _pickupEffect;

        private bool _isCollected = false;


        public override void Interact()
        {
        //     if (_isCollected) return;

        //     Interact();

        //     if (PlayerInventory.Instance != null && PlayerInventory.Instance.AddItem(_itemType))
        //     {
        //         // �������� TaskManager � ������ �������� (��� ��������� ���������� ������)
        //         if (TaskManager.Instance != null)
        //         {
        //             // TaskManager.Instance.ReportItemCollected(_itemType, ID); // ID �� InteractObject
        //         }

        //         // ���������� � �������� �������
        //         PlayPickupEffects();

        //         // ������ ������� ���������/����������
        //         _isCollected = true;

        //         // ��������� ��������� � �������� (������������ �������)
        //         var renderer = GetComponent<SpriteRenderer>();
        //         if (renderer != null)
        //         {
        //             renderer.enabled = false;
        //         }

        //         // ������������ �� �����, ���� �� ����� ��������
        //         gameObject.SetActive(false);

        //         Debug.Log($"������� {_itemType} ������ (ID: {ID})");
        //     }
        //     else
        //     {
        //         // �� ������� �������� ������� (��������� �����)
        //         _isCollected = false;
        //     }
        }

        public void Pickup()
        {
            if (_isCollected) return;
            PlayerInventory.Instance.AddItemToInventory(this.Type);
        }

        public bool PassToTask(string type)
        {
            if (!_isCollected) return false;
            if (this.Type == type)
            {
                PlayerInventory.Instance.RemoveItem(this.Type);
                return true;
            }
            return false;
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

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}
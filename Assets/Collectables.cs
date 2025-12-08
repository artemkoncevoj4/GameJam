using UnityEngine;
using Player;
using TaskSystem;
namespace InteractiveObjects
{
    public class CollectibleItem : InteractObject
    {
        [Header("��������� ��������")]
        [SerializeField] private string _itemType = "ink_black"; // ��� ��������
        [SerializeField] private AudioClip _pickupSound;
        [SerializeField] private GameObject _pickupEffect;

        private bool _isCollected = false;

        protected override void Start()
        {
            base.Start();
            State = "����� � �����";
        }

        public override void Interact()
        {
            if (_isCollected) return;

            OnInteractionStarted();

            if (PlayerInventory.Instance != null && PlayerInventory.Instance.AddItem(_itemType))
            {
                // �������� TaskManager � ������ �������� (��� ��������� ���������� ������)
                if (TaskManager.Instance != null)
                {
                    // TaskManager.Instance.ReportItemCollected(_itemType, ID); // ID �� InteractObject
                }

                // ���������� � �������� �������
                PlayPickupEffects();

                // ������ ������� ���������/����������
                _isCollected = true;
                State = "������";

                // ��������� ��������� � �������� (������������ �������)
                var renderer = GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.enabled = false;
                }

                // ������������ �� �����, ���� �� ����� ��������
                gameObject.SetActive(false);

                Debug.Log($"������� {_itemType} ������ (ID: {ID})");
            }
            else
            {
                // �� ������� �������� ������� (��������� �����)
                _isCollected = false;
                State = "����� � �����";
            }
        }

        private void PlayPickupEffects()
        {
            if (_pickupSound != null)
            {
                // �������������� ������� AudioSource
                AudioSource.PlayClipAtPoint(_pickupSound, transform.position);
            }

            if (_pickupEffect != null)
            {
                Instantiate(_pickupEffect, transform.position, Quaternion.identity);
            }
        }

        // ��� �������
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}
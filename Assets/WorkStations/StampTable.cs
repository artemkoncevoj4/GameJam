using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using TaskSystem;

namespace InteractiveObjects
{
    public class StampTable : Workstation
    {
        [Header("Stamp Table Settings")]
        [SerializeField] private ParticleSystem _stampParticleEffect; // Визуальный эффект
        [SerializeField] private AudioClip _stampSound; // Звук штампа
        [SerializeField] private GameObject documentModel;
        public GameObject _movementEmpty;
        
        private bool _isDoucmenPresent = false;
        private Document currentDocument;

        void Start()
        {
            
        }
        public string GetInteractionHint()
        {
            return "Нажмите E для использования штампа";
        }

        public bool CanInteract()
        {
            return true;
        }
        
        // Переопределяем метод UseStation для конкретной логики штампа
        public new void UseStation()
        {
            Debug.Log("StampTable: Checking for interaction");
            
            if (!_isDoucmenPresent) return;

            OpenStampWindow();
            
        }
        
        // Размещение документа на столе
        private void OpenStampWindow()
        {
            Debug.Log($"Open stamp window");
            Document _currentDocument = TaskManager.Instance.GetCurrentDocument();
            ChangeEmptyPos(1);
            //MakeStamp();
            StampType stampType = StampType.Одобрено; //!   Change later
            StampDocument(_currentDocument, stampType);
            Debug.Log($"Теперь нажмите E еще раз для штамповки документа");
            ResetTable();
        }
        
        // Штамповка документа
        private void StampDocument(Document document, StampType stampType)
        {
            Debug.Log($"StampTable: Stamping document {document}");
            
            // Визуальные эффекты
            if (_stampParticleEffect != null)
                _stampParticleEffect.Play();
            
            // Звуковой эффект
            if (_stampSound != null)
                AudioSource.PlayClipAtPoint(_stampSound, transform.position);
            
            // Выдаем штампованный документ
            document.IsStamped = true;
            document.StampType = stampType;

            Debug.Log($"Документ {document} был штампован с типом {stampType}");
        }
        
        private void ResetTable()
        {
            ChangeEmptyPos(-1);
            currentDocument = null;
            
            if (documentModel != null)
                documentModel.SetActive(false);
        }

        private void ChangeEmptyPos(int direction)
        {
            Vector2 tempPos = _movementEmpty.transform.position;
            tempPos.y = _movementEmpty.transform.position.y + 10 * direction;
            _movementEmpty.transform.position = tempPos;
        }

    }
}
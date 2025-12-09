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
        
        
        private bool _isDoucmenPresent = false;
        
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
            
            StampType stampType = StampType.Одобрено; //!   Change later
            StampDocument(_currentDocument, stampType);
            Debug.Log($"Теперь нажмите E еще раз для штамповки документа");
        }
        
        // Штамповка документа
        private void StampDocument(Document _currentDocument, StampType stampType)
        {
            Debug.Log($"StampTable: Stamping document {_currentDocument}");
            
            // Визуальные эффекты
            if (_stampParticleEffect != null)
                _stampParticleEffect.Play();
            
            // Звуковой эффект
            if (_stampSound != null)
                AudioSource.PlayClipAtPoint(_stampSound, transform.position);
            
            // Выдаем штампованный документ
            _currentDocument.IsStamped = true;
            _currentDocument.StampType = stampType;
            
        }
    }
}
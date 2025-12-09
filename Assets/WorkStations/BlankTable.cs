using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using TaskSystem;

namespace InteractiveObjects
{
    public class BlankTable : Workstation
    {
        [Header("Stamp Table Settings")]
        [SerializeField] private AudioClip _paperSound; // Звук 
        [SerializeField] private GameObject documentModel;
        private Document currentDocument;
        
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
            Debug.Log("BlankTable: Checking for interaction");
            
            if (currentDocument == null) return;

            OpenBlankWindow();
            
        }
        
        // Размещение документа на столе
        private void OpenBlankWindow()
        {
            Debug.Log($"Open stamp window");
            Document _currentDocument = TaskManager.Instance.GetCurrentDocument();
            
            StampType stampType = StampType.Одобрено; //!   Change later
            StampDocument(_currentDocument, stampType);
            Debug.Log($"Теперь нажмите E еще раз для штамповки документа");
            ResetTable();
        }
        
        // Штамповка документа
        private void StampDocument(Document document, StampType stampType)
        {
            Debug.Log($"StampTable: Stamping document {document}");
            
            // Звуковой эффект
            if (_paperSound != null)
                AudioSource.PlayClipAtPoint(_paperSound, transform.position);
            
            // Выдаем штампованный документ
            document.IsStamped = true;
            document.StampType = stampType;

            Debug.Log($"Документ {document} был штампован с типом {stampType}");
        }
        
        private void ResetTable()
        {
            currentDocument = null;
            
            if (documentModel != null)
                documentModel.SetActive(false);
        }

    }
}
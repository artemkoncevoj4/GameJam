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
            Debug.Log($"Open blank window");
            Document _currentDocument = TaskManager.Instance.GetCurrentDocument();
            
            PaperType paperType = PaperType.Бланк_формы_7_Б; //!   Change later

            GetBlank(_currentDocument, paperType);
            Debug.Log($"Теперь нажмите E еще раз для штамповки документа");
            ResetTable();
        }
        
        // Штамповка документа
        private void GetBlank(Document document, PaperType paperType)
        {
            Debug.Log($"StampTable: Blank document {document}");
            
            // Звуковой эффект
            if (_paperSound != null)
                AudioSource.PlayClipAtPoint(_paperSound, transform.position);
            
            // Выдаем штампованный документ
            document.PaperType = paperType;

            Debug.Log($"Документ {document} был штампован с типом {paperType}");
        }
        
        private void ResetTable()
        {
            currentDocument = null;
            
            if (documentModel != null)
                documentModel.SetActive(false);
        }

    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using TaskSystem;

namespace InteractiveObjects
{
    public class BlankTable : Workstation
    {
        [Header("Blank Table Settings")]
        [SerializeField] private AudioClip _paperSound;
        [SerializeField] private GameObject documentModel;
        public static PaperType paperType;
        [Header("Empty")]
        public GameObject _movementEmpty;
        
        private bool _isDocumentPresent = false;
        private bool isCoroutineRunning = false;
        public static bool shouldCoroutineStop = false;
        private Document currentDocument;
        private Coroutine currCoroutine;
        private Vector3 originalEmptyPosition;
        
        void Start()
        {
            originalEmptyPosition = _movementEmpty.transform.localPosition;
        }
        
        void Update()
        {
            if (TaskManager.Instance.GetCurrentDocument() != null)
            {
                _isDocumentPresent = true;
                currentDocument = TaskManager.Instance.GetCurrentDocument();
            }
            else
            {
                _isDocumentPresent = false;
                currentDocument = null;
            }
        }

        public string GetInteractionHint()
        {
            return "Выберите бланк";
        }

        public bool CanInteract()
        {
            return true;
        }
        
        // Переопределяем метод UseStation
        public override void UseStation()
        {
            Debug.Log("BlankTable: Checking for interaction");
            
            // Если станция уже активна - закрываем её
            if (isActive)
            {
                Debug.Log("Closing blank window");
                ResetTable();
                return;
            }
            
            if (!_isDocumentPresent) 
            {
                Debug.LogWarning("No document present on BlankTable");
                return;
            }

            OpenBlankWindow();
        }
        
        private void OpenBlankWindow()
        {
            Debug.Log($"Open blank window");
            isActive = true;
            ChangeEmptyPos(-1);
            
            if (!isCoroutineRunning)
            {
                currCoroutine = StartCoroutine(GetBlank(currentDocument));
            }
            Debug.Log($"Нажмите E еще раз для закрытия окна");
        }
        
        private IEnumerator GetBlank(Document document)
        {
            Debug.Log($"BlankTable: Getting blank for document {document}");
            isCoroutineRunning = true;
            
            while (!shouldCoroutineStop)
            {
                // Если станция стала неактивной, прерываем корутину
                if (!isActive)
                {
                    Debug.Log("Blank window was closed, stopping coroutine");
                    break;
                }
                yield return new WaitForSeconds(0.1f);
            }
            
            // Если корутина завершилась нормально (не была отменена)
            if (shouldCoroutineStop && isActive)
            {
                if (_paperSound != null)
                    AudioSource.PlayClipAtPoint(_paperSound, transform.position);
                
                document.PaperType = paperType;
                Debug.Log($"Документ {document} получил тип бумаги {paperType}");
            }
            
            ResetTable();
        }
        
        public override void ResetTable()
        {
            // Возвращаем empty в исходную позицию
            _movementEmpty.transform.localPosition = originalEmptyPosition;
            
            if (documentModel != null)
                documentModel.SetActive(false);
            
            if (currCoroutine != null)
            {
                StopCoroutine(currCoroutine);
                currCoroutine = null;
            }
            
            isCoroutineRunning = false;
            shouldCoroutineStop = false;
            isActive = false;
            
            Debug.Log("Blank table reset to original state");
        }

        private void ChangeEmptyPos(int direction)
        {
            _movementEmpty.transform.localPosition += new Vector3(0, 10 * direction, 0);
        }
        
        void OnDestroy()
        {
            if (currCoroutine != null)
            {
                StopCoroutine(currCoroutine);
            }
        }
    }
}
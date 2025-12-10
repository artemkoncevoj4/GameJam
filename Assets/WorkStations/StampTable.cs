using UnityEngine;
using System.Collections;
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
        
        private bool _isDocumentPresent = false;
        private bool isCoroutineRunning = false;
        public static bool shouldCoroutineStop = false;
        private Document currentDocument;
        private Coroutine currCoroutine;
        public static StampPosition stampPos;
        public static StampType stampType;
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
            return "Выберите штамп и поставьте на бумагу";
        }

        public bool CanInteract()
        {
            return true;
        }
        
        // Переопределяем метод UseStation для конкретной логики штампа
        public override void UseStation()
        {
            Debug.Log("StampTable: Checking for interaction");
            
            // Если станция уже активна - закрываем её
            if (isActive)
            {
                Debug.Log("Closing stamp window");
                ResetTable();
                return;
            }
            
            if (!_isDocumentPresent) 
            {
                Debug.LogWarning("No document present on StampTable");
                return;
            }

            OpenStampWindow();
            
        }
        
        // Размещение документа на столе
        private void OpenStampWindow()
        {
            Debug.Log($"Open stamp window");
            isActive = true;
            ChangeEmptyPos(1);

            if (!isCoroutineRunning)
            {
                currCoroutine = StartCoroutine(StampDocument(currentDocument));
            }
            Debug.Log($"Теперь нажмите E еще раз закрытия");
        }
        
        // Штамповка документа
        private IEnumerator StampDocument(Document document)
        {
            isCoroutineRunning = true;
            Debug.Log($"StampTable: Stamping document {document}");
            while (!shouldCoroutineStop)
            {
                if (!isActive)
                {
                    Debug.Log("Stamp window was closed, stopping coroutine");
                    break;
                }
                yield return new WaitForSeconds(0.1f);
            }
            if (shouldCoroutineStop && isActive)
            {
                // Визуальные эффекты
                if (_stampParticleEffect != null)
                    _stampParticleEffect.Play();
                
                // Звуковой эффект
                if (_stampSound != null)
                    AudioSource.PlayClipAtPoint(_stampSound, transform.position);
                
                // Выдаем штампованный документ
                if (!document.IsStamped)
                {
                    document.StampType = stampType;
                    document.StampPos = stampPos;
                    Debug.Log($"Документ {document} был штампован с типом {stampType}");
                }
                document.IsStamped = true;
            }
            
            ResetTable();
        }
        
        public override void ResetTable()
        {
            _movementEmpty.transform.localPosition = originalEmptyPosition;

            Stamp2D.isStumped = false;
            
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
            
            Debug.Log("Stamp table reset to original state");
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
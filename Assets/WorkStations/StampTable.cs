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
        
        private bool _isDoucmenPresent = false;
        private bool isCoroutineRunning = false;
        public static bool shouldCoroutineStop = false;
        private Document currentDocument;
        private Coroutine currCoroutine;
        

        void Start()
        {
            
        }
        void Update()
        {
            if (TaskManager.Instance.GetCurrentDocument() != null)
            {
                _isDoucmenPresent = true;
            }
            else
            {
                _isDoucmenPresent = false;
            }
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
        public override void UseStation()
        {
            Debug.Log("StampTable: Checking for interaction");
            Debug.LogWarning($" Document: {_isDoucmenPresent}");
            if (!_isDoucmenPresent) return;

            OpenStampWindow();
            
        }
        
        // Размещение документа на столе
        private void OpenStampWindow()
        {
            Debug.Log($"Open stamp window");
            Document _currentDocument = TaskManager.Instance.GetCurrentDocument();
            ChangeEmptyPos(1);
            if (!isCoroutineRunning)
            {
                currCoroutine = StartCoroutine(StampDocument(_currentDocument));
            }
            Debug.Log($"Теперь нажмите E еще раз для штамповки документа");
            ResetTable();
        }
        
        // Штамповка документа
        private IEnumerator StampDocument(Document document)
        {
            StampPosition stampPos = StampPosition.Левая_сторона;
            StampType stampType = StampType.На_рассмотрении;
            isCoroutineRunning = true;
            Debug.Log($"StampTable: Stamping document {document}");
            while (!shouldCoroutineStop)
            {
                yield return new WaitForSeconds(0.1f);
            }
            // Визуальные эффекты
            if (_stampParticleEffect != null)
                _stampParticleEffect.Play();
            
            // Звуковой эффект
            if (_stampSound != null)
                AudioSource.PlayClipAtPoint(_stampSound, transform.position);
            
            // Выдаем штампованный документ
            document.IsStamped = true;
            document.StampType = stampType;
            document.StampPos = stampPos;

            isCoroutineRunning = false;
            
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
            Debug.Log("Nihuya ne rabotaet");
            Vector2 tempPos = _movementEmpty.transform.position;
            tempPos.y = _movementEmpty.transform.position.y + 10 * direction;
            _movementEmpty.transform.position = tempPos;
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
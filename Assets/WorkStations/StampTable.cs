using UnityEngine;
using System.Collections;

using TaskSystem;

namespace InteractiveObjects
{
    // Муштаков А.Ю.

    /// <summary>
    /// Класс рабочей станции для штампования документов.
    /// Управляет процессом постановки штампа на документ.
    /// </summary>
    public class StampTable : Workstation
    {
        [Header("Stamp Table Settings")]
        public GameObject _movementEmpty; // Пустой объект для управления позицией выдвигающегося окна
        
        private bool _isDocumentPresent = false; // Флаг наличия документа на столе
        private bool isCoroutineRunning = false; // Флаг выполнения корутины
        /// <summary>
        /// Статический флаг остановки корутины штампования.
        /// В данном случае использование static оправдано, так как в сцене только один стол для штамповки.
        /// </summary>
        public static bool shouldCoroutineStop = false;
        private Document currentDocument; // Текущий документ для штампования
        private Coroutine currCoroutine; // Ссылка на текущую корутину
        /// <summary>
        /// Статическое свойство для хранения позиции штампа.
        /// </summary>
        public static StampPosition stampPos; // Нет нормальной реализации
        /// <summary>
        /// Статическое свойство для хранения типа штампа.
        /// </summary>
        public static StampType stampType; // Нет нормальной реализации
        private Vector3 originalEmptyPosition; // Исходная позиция пустого объекта
        
        /// <summary>
        /// Сохраняет исходную позицию пустого объекта при старте.
        /// </summary>
        void Start()
        {
            originalEmptyPosition = _movementEmpty.transform.localPosition;
        }
        
        /// <summary>
        /// Обновляет состояние наличия документа на столе каждый кадр.
        /// Проверяет TaskManager на наличие текущего документа.
        /// </summary>
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
        
        /// <summary>
        /// Возвращает текстовую подсказку для взаимодействия со станцией.
        /// </summary>
        /// <returns>Строка с текстом подсказки.</returns>
        public string GetInteractionHint()
        {
            return "Выберите штамп и поставьте на бумагу"; //TODO Должна быть как диалоговое окно снизу при приближении к станции (не реализовано)
        }
        
        /// <summary>
        /// Переопределяет метод UseStation для конкретной логики штамповки.
        /// Открывает или закрывает окно штамповки в зависимости от текущего состояния.
        /// </summary>
        public override void UseStation()
        {
            Debug.Log("StampTable: Checking for interaction");
            
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
        
        /// <summary>
        /// Открывает окно штамповки и запускает процесс штампования.
        /// </summary>
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
        
        /// <summary>
        /// Корутина для процесса штампования документа.
        /// Ожидает флаг остановки и применяет эффекты при штамповке.
        /// </summary>
        /// <param name="document">Документ для штампования.</param>
        /// <returns>IEnumerator для корутины.</returns>
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
        
        /// <summary>
        /// Сбрасывает состояние стола к исходному.
        /// Останавливает корутину, сбрасывает флаги и возвращает объекты в исходные позиции.
        /// </summary>
        public override void ResetTable()
        {
            _movementEmpty.transform.localPosition = originalEmptyPosition;

            Stamp2D.isStumped = false;
            
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

        /// <summary>
        /// Изменяет позицию пустого объекта для визуального эффекта открытия/закрытия окна.
        /// </summary>
        /// <param name="direction">Направление движения (1 - вверх, -1 - вниз).</param>
        private void ChangeEmptyPos(int direction)
        {
            _movementEmpty.transform.localPosition += new Vector3(0, 10 * direction, 0);
        }
        
        /// <summary>
        /// Останавливает корутину при уничтожении объекта.
        /// </summary>
        void OnDestroy()
        {
            if (currCoroutine != null)
            {
                StopCoroutine(currCoroutine);
            }
        }
    }
}
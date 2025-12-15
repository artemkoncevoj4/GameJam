using UnityEngine;
using System.Collections;
using TaskSystem;

namespace InteractiveObjects
{
    // Муштаков А.Ю.

    /// <summary>
    /// Класс рабочей станции для выбора и оформления бланков документов.
    /// Управляет процессом выбора типа бумаги для документа и его обработкой.
    /// </summary>
    public class BlankTable : Workstation
    {
        [Header("Blank Table Settings")]
        /// <summary>
        /// Статическое поле для хранения типа бумаги, выбранного для текущего документа.
        /// </summary>
        public static PaperType paperType; // Нет нормальной реализации
        [Header("Empty")]
        public GameObject _movementEmpty; // Пустой объект для управления анимацией окна
        
        private bool _isDocumentPresent = false; // Флаг наличия активного документа
        private bool isCoroutineRunning = false; // Флаг выполнения корутины
        /// <summary>
        /// Статический флаг остановки корутины выбора бланка.
        /// Один стол для бланков
        /// </summary>
        public static bool shouldCoroutineStop = false;
        private Document currentDocument; // Ссылка на текущий обрабатываемый документ
        private Coroutine currCoroutine; // Ссылка на текущую выполняемую корутину
        private Vector3 originalEmptyPosition; // Исходная позиция пустого объекта для анимации
        
        /// <summary>
        /// Сохраняет исходную позицию пустого объекта при инициализации.
        /// </summary>
        void Start()
        {
            originalEmptyPosition = _movementEmpty.transform.localPosition;
        }
        
        /// <summary>
        /// Обновляет состояние наличия документа на столе каждый кадр.
        /// Проверяет TaskManager на наличие активного документа и обновляет ссылку на него.
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
        /// Возвращает текстовую подсказку для взаимодействия со станцией выбора бланков.
        /// </summary>
        /// <returns>Строка с текстом подсказки.</returns>
        public string GetInteractionHint()
        {
            return "Выберите бланк"; //TODO Должна быть как диалоговое окно снизу при приближении к станции (не реализовано)
        }

        /// <summary>
        /// Переопределяет метод UseStation для реализации логики выбора бланка.
        /// Открывает или закрывает окно выбора бланка в зависимости от текущего состояния станции.
        /// </summary>
        public override void UseStation()
        {
            Debug.Log("BlankTable: Checking for interaction");
            
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
        
        /// <summary>
        /// Открывает окно выбора бланка и запускает процесс ожидания выбора.
        /// Активирует анимацию окна и запускает корутину ожидания выбора.
        /// </summary>
        private void OpenBlankWindow()
        {
            Debug.Log($"Open blank window");
            isActive = true;
            ChangeEmptyPos(-1);
            
            if (!isCoroutineRunning)
            {
                currCoroutine = StartCoroutine(GetBlank(currentDocument));
            }
        }
        
        /// <summary>
        /// Корутина ожидания выбора бланка.
        /// Ожидает установки флага shouldCoroutineStop, затем применяет выбранный тип бумаги к документу.
        /// </summary>
        /// <param name="document">Документ, для которого выбирается тип бумаги.</param>
        /// <returns>IEnumerator для управления корутиной.</returns>
        private IEnumerator GetBlank(Document document)
        {
            Debug.Log($"BlankTable: Getting blank for document {document}");
            isCoroutineRunning = true;
            
            while (!shouldCoroutineStop)
            {
                if (!isActive)
                {
                    Debug.Log("Blank window was closed, stopping coroutine");
                    break;
                }
                yield return new WaitForSeconds(0.1f);
            }
            
            if (shouldCoroutineStop && isActive)
            {
                
                document.PaperType = paperType;
                Debug.Log($"Документ {document} получил тип бумаги {paperType}");
            }
            
            ResetTable();
        }
        
        /// <summary>
        /// Сбрасывает состояние стола к исходному.
        /// Возвращает анимационные элементы в исходное положение, останавливает корутины и сбрасывает флаги.
        /// </summary>
        public override void ResetTable()
        {

            _movementEmpty.transform.localPosition = originalEmptyPosition;
            
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

        /// <summary>
        /// Изменяет позицию пустого объекта для визуальной анимации открытия/закрытия окна.
        /// </summary>
        /// <param name="direction">Направление движения (1 - вверх, -1 - вниз).</param>
        private void ChangeEmptyPos(int direction)
        {
            _movementEmpty.transform.localPosition += new Vector3(0, 10 * direction, 0);
        }
        
        /// <summary>
        /// Останавливает выполняющуюся корутину при уничтожении объекта для предотвращения утечек памяти.
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
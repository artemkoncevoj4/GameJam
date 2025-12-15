using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Player;
namespace TaskSystem {

    // Муштаков А.Ю.

    /// <summary>
    /// Управляет системой заданий в игре, включая генерацию, отслеживание и валидацию задач.
    /// Реализует паттерн Singleton для централизованного управления заданиями.
    /// </summary>
    public class TaskManager : MonoBehaviour
    {
        /// <summary>
        /// Статический экземпляр для реализации паттерна Singleton.
        /// </summary>
        public static TaskManager Instance { get; private set; }

        [Header("Настройки заданий")]
        [SerializeField] private float _baseTaskTime = 60f;
        [SerializeField] private float _timeReductionPerTask = 4f;
        [SerializeField] private float _minTaskTime = 23f;
        [SerializeField] private float _urgentTaskChance = 0.2f;

        [Header("Предметы и станции")]
        [SerializeField] private List<string> _availableInkTypes = new List<string> { "ink_black", "ink_red", "ink_green", "ink_purple" };
        [SerializeField] private List<string> _availablePaperTypes = new List<string> { "form_7b", "form_aay", "parchment", "card" };
        [SerializeField] private List<string> _availableStampTypes = new List<string> { "stamp_approve", "stamp_reject", "stamp_review", "stamp_official", "stamp_secret" };

        [Header("Рабочие станции")]
        [SerializeField] private List<InteractiveObjects.Workstation> _workstations = new List<InteractiveObjects.Workstation>();

        private TaskSystem.BureaucraticTask _currentTask = null;
        private TaskSystem.Document _currentDocument = null;
        private int _totalTasksCompleted = 0;
        private float _currentTaskTimeLimit;
        private bool _isTaskActive = false;

        /// <summary>
        /// Событие создания нового задания.
        /// </summary>
        public event Action<TaskSystem.BureaucraticTask> OnNewTask;
        
        /// <summary>
        /// Событие успешного выполнения задания.
        /// </summary>
        public event Action<TaskSystem.BureaucraticTask> OnTaskCompleted;
        
        /// <summary>
        /// Событие провала задания.
        /// </summary>
        public event Action<TaskSystem.BureaucraticTask> OnTaskFailed;
        
        /// <summary>
        /// Событие искажения задания кроликом.
        /// </summary>
        public event Action<TaskSystem.BureaucraticTask> OnTaskCorrupted;
        
        /// <summary>
        /// Событие обновления таймера задания.
        /// </summary>
        public event Action<float> OnTaskTimerUpdated;

        /// <summary>
        /// Инициализирует Singleton и обеспечивает единственность экземпляра.
        /// </summary>
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            //Debug.Log("<color=green>TaskManager: Awake called, Instance set</color>");
        }

        /// <summary>
        /// Настраивает начальные параметры и подписывается на события кролика.
        /// </summary>
        void Start()
        {
            //Debug.Log("<color=green>TaskManager: Start called</color>");
            
            _currentTaskTimeLimit = _baseTaskTime;
            
            StartCoroutine(FindBunnyAndSubscribe());
        }
        
        //! Метод написан ИИ
        
        /// <summary>
        /// Находит объект кролика в сцене и подписывается на его события.
        /// </summary>
        private IEnumerator FindBunnyAndSubscribe()
        {
            yield return new WaitForSeconds(0.1f);
            
            var bunny = FindAnyObjectByType<Bunny.Bunny>();
            if (bunny != null)
            {
                //Debug.Log("<color=green>TaskManager: Found Bunny, subscribing to events</color>");
                bunny.OnRabbitActive += HandleRabbitInterference;
            }
            else
            {
                //Debug.LogWarning("<color=red>TaskManager: Bunny not found in scene</color>");
            }
        }

        /// <summary>
        /// Отписывается от событий кролика при уничтожении объекта.
        /// </summary>
        void OnDestroy()
        {
            if (Bunny.Bunny.Instance != null)
            {
                Bunny.Bunny.Instance.OnRabbitActive -= HandleRabbitInterference;
            }
        }

        /// <summary>
        /// Обновляет таймер задания и проверяет истечение времени.
        /// </summary>
        void Update()
        {
            if (GameCycle.Instance == null || GameCycle.Instance.CurrentState != GameCycle.GameState.Playing)
                return;

            if (_currentTask != null && !_currentTask.IsCompleted && !_currentTask.IsFailed)
            {
                if (_currentTask.UpdateTimer(Time.deltaTime))
                {
                    FailCurrentTask("Время вышло!");
                    OnTaskTimerUpdated?.Invoke(0f);
                }
                else
                {
                    OnTaskTimerUpdated?.Invoke(_currentTask.TimeRemaining);
                }
            }
        }
        
        //! Метод написан ИИ с изменением _currentTaskTimeLimit

        /// <summary>
        /// Создает и запускает новое задание со случайными параметрами.
        /// </summary>
        public void StartNewTask()
        {
            if (_isTaskActive)
            {
                Debug.LogWarning("<color=yellow>Попытка начать новое задание, когда текущее еще активно!</color>");
                return;
            }

            _currentDocument = new TaskSystem.Document();

            bool isUrgent = Random.value < _urgentTaskChance;
            float timeLimit = isUrgent ? _currentTaskTimeLimit * 0.7f : _currentTaskTimeLimit;

            TaskSystem.DocumentRequirement req = GenerateRandomRequirements();

            _currentTask = new TaskSystem.BureaucraticTask(
                isUrgent ? $"СРОЧНО: {GetRandomTaskTitle()}" : GetRandomTaskTitle(),
                req,
                timeLimit,
                isUrgent
            );

            _currentTaskTimeLimit = Mathf.Max(_minTaskTime, Random.Range(_currentTaskTimeLimit - 2 * _timeReductionPerTask, _currentTaskTimeLimit - _timeReductionPerTask));

            _isTaskActive = true;
            OnNewTask?.Invoke(_currentTask);

            Debug.Log($"<color=cyan>Новое задание: {_currentTask.Title}</color>");
            Debug.Log($"<color=white>{_currentTask.Description}</color>");
            Debug.Log($"<color=white>время: {_currentTaskTimeLimit} </color>");
            
            StartCoroutine(ForceUIUpdate());
        }
        
        /// <summary>
        /// Принудительно обновляет пользовательский интерфейс после создания задания.
        /// </summary>
        private IEnumerator ForceUIUpdate()
        {
            yield return new WaitForSeconds(0.1f);
            
            // Обновляем таймер
            if (OnTaskTimerUpdated != null)
            {
                OnTaskTimerUpdated.Invoke(_currentTask.TimeRemaining);
            }
            
            Debug.Log("<color=cyan>TaskManager: UI обновлен после создания задания</color>");
        }

        /// <summary>
        /// Генерирует случайные требования к документу.
        /// </summary>
        /// <returns>Сгенерированные требования DocumentRequirement.</returns>
        private TaskSystem.DocumentRequirement GenerateRandomRequirements()
        {
            TaskSystem.DocumentRequirement req = new TaskSystem.DocumentRequirement();

            req.requiredInkColor = (TaskSystem.InkColor)Random.Range(0, 4);
            req.requiredStampPos = (TaskSystem.StampPosition)Random.Range(0, 4);
            req.requiredPaperType = (TaskSystem.PaperType)Random.Range(0, 4);
            req.requiredStampType = (TaskSystem.StampType)Random.Range(0, 5);
            req.isStamped = Random.value > 0.3f;
            req.isSigned = Random.value > 0.1f;
            req.timePenalty = 15f;

            return req;
        }

        //! Метод написан ИИ

        /// <summary>
        /// Возвращает случайный заголовок для задания.
        /// </summary>
        /// <returns>Строка с названием задания.</returns>
        private string GetRandomTaskTitle()
        {
            string[] titles = {
                "Заполнить форму 7-Б",
                "Подписать договор А-42",
                "Заверить копию документа",
                "Оформить разрешение 3-Г",
                "Зарегистрировать обращение",
                "Составить отчет по разделу 4",
                "Утвердить смету расходов",
                "Заполнить налоговую декларацию"
            };
            return titles[Random.Range(0, titles.Length)];
        }


        /// <summary>
        /// Отправляет документ на проверку и завершает задание.
        /// </summary>
        public void SubmitDocument()
        {
            if (_currentTask == null || _currentTask.IsCompleted || _currentTask.IsFailed)
            {
                Debug.Log("<color=red>Нет активного задания!</color>");
                return;
            }

            if (_currentDocument == null)
            {
                Debug.Log("<color=red>Документ не создан!</color>");
                return;
            }

            if (_currentTask.Validate(_currentDocument))
            {
                CompleteCurrentTask();
            }
            else
            {
                FailCurrentTask("<color=red>Неверно заполнен документ!</color>");
                //? paper rip fast. DONE?
                AudioManager.Instance?.PlaySoundByName("paperripfast");
            }
        }

        /// <summary>
        /// Завершает текущее задание как успешно выполненное.
        /// </summary>
        private void CompleteCurrentTask()
        {
            _currentTask.Complete();
            _totalTasksCompleted++;
            _isTaskActive = false;

            OnTaskCompleted?.Invoke(_currentTask);

            if (GameCycle.Instance != null)
            {
                GameCycle.Instance.CompleteTask();
            }
            if (Bunny.Bunny.Instance != null)
            {
                Bunny.Bunny.Instance.ChangeToNoTask();
            }
        }

        /// <summary>
        /// Проваливает текущее задание с указанной причиной.
        /// </summary>
        private void FailCurrentTask(string reason)
        {
            _currentTask.Fail();
            _isTaskActive = false;
            _currentDocument = null;

            OnTaskFailed?.Invoke(_currentTask);

            Debug.Log($"<color=red>Задание провалено: {reason}</color>");

            if (GameCycle.Instance != null)
            {
                GameCycle.Instance.FailTask(_currentTask.Requirements.timePenalty);
            }
        }

        /// <summary>
        /// Обрабатывает вмешательство кролика, искажающего требования задания.
        /// </summary>
        public void HandleRabbitInterference()
        {
            if (_currentTask != null && !_currentTask.IsCompleted)
            {
                _currentTask.Corrupt();
                OnTaskCorrupted?.Invoke(_currentTask);

                //Debug.Log("<color=yellow>Кролик изменил требования задания!</color>");
                //Debug.Log($"<color=white>Новые требования: {_currentTask.Description}</color>");

                if (GameCycle.Instance != null)
                {
                    GameCycle.Instance.AddStress(5f);
                }
            }
        }

        /// <summary>
        /// Возвращает текущее активное задание.
        /// </summary>
        /// <returns>Текущее задание BureaucraticTask или null.</returns>
        public TaskSystem.BureaucraticTask GetCurrentTask()
        {
            return _currentTask;
        }

        /// <summary>
        /// Возвращает общее количество выполненных заданий.
        /// </summary>
        /// <returns>Количество завершенных заданий.</returns>
        public int GetCompletedTasks()
        {
            return _totalTasksCompleted;
        }

        /// <summary>
        /// Возвращает заголовок текущего задания.
        /// </summary>
        /// <returns>Заголовок задания или null.</returns>
        public string GetTaskTitle()
        {
            return _currentTask?.Title;
        }

        /// <summary>
        /// Возвращает описание текущего задания.
        /// </summary>
        /// <returns>Описание задания или null.</returns>
        public string GetTaskDescription()
        {
            return _currentTask?.Description;
        }

        /// <summary>
        /// Возвращает текущий обрабатываемый документ.
        /// </summary>
        /// <returns>Текущий документ Document или null.</returns>
        public Document GetCurrentDocument()
        {
            if (_currentDocument != null) return _currentDocument;
            return null;
        }

        /// <summary>
        /// Возвращает флаг активности текущего задания.
        /// </summary>
        public bool IsTaskActive => _isTaskActive;
    }
}
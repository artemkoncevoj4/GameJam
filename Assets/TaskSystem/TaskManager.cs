using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Player;
namespace TaskSystem {
    public class TaskManager : MonoBehaviour
    {
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

        // Текущее состояние
        private TaskSystem.BureaucraticTask _currentTask = null;
        private TaskSystem.Document _currentDocument = null;
        private int _totalTasksCompleted = 0;
        private float _currentTaskTimeLimit;
        private bool _isTaskActive = false;

        // События
        public event Action<TaskSystem.BureaucraticTask> OnNewTask;
        public event Action<TaskSystem.BureaucraticTask> OnTaskCompleted;
        public event Action<TaskSystem.BureaucraticTask> OnTaskFailed;
        public event Action<TaskSystem.BureaucraticTask> OnTaskCorrupted;
        public event Action<float> OnTaskTimerUpdated;

        public string ReturnTaskTime()
        {
            return Convert.ToString(_currentTaskTimeLimit);
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            Debug.Log("<color=green>TaskManager: Awake called, Instance set</color>");
        }

        void Start()
        {
            Debug.Log("<color=green>TaskManager: Start called</color>");
            
            _currentTaskTimeLimit = _baseTaskTime;
            
            StartCoroutine(FindBunnyAndSubscribe());
            
            if (PlayerInventory.Instance != null)
            {
                PlayerInventory.Instance.OnInventoryChanged += CheckTaskRequirements;
            }
        }

        private IEnumerator FindBunnyAndSubscribe()
        {
            yield return new WaitForSeconds(0.1f);
            
            var bunny = FindAnyObjectByType<Bunny.Bunny>();
            if (bunny != null)
            {
                Debug.Log("<color=green>TaskManager: Found Bunny, subscribing to events</color>");
                bunny.OnRabbitActive += HandleRabbitInterference;
            }
            else
            {
                Debug.LogWarning("<color=red>TaskManager: Bunny not found in scene</color>");
            }
        }

        void OnDestroy()
        {
            if (Bunny.Bunny.Instance != null)
            {
                Bunny.Bunny.Instance.OnRabbitActive -= HandleRabbitInterference;
            }

            if (PlayerInventory.Instance != null)
            {
                PlayerInventory.Instance.OnInventoryChanged -= CheckTaskRequirements;
            }
        }

        void Update()
        {
            if (GameCycle.Instance == null || GameCycle.Instance.CurrentState != GameCycle.GameState.Playing)
                return;

            if (_currentTask != null && !_currentTask.IsCompleted && !_currentTask.IsFailed)
            {
                if (_currentTask.UpdateTimer(Time.deltaTime))
                {
                    FailCurrentTask("Время вышло!");
                    // [!] Важно: при провале по времени сразу обновляем UI
                    OnTaskTimerUpdated?.Invoke(0f);
                }
                else
                {
                    // [!] Всегда обновляем UI с текущим временем
                    OnTaskTimerUpdated?.Invoke(_currentTask.TimeRemaining);
                }
            }
        }
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
            // [!] ВАЖНО: Принудительно обновляем UI
            StartCoroutine(ForceUIUpdate());
        }
        
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


        private void UpdateDocumentFromUsage(string stationType, string itemType)
        {
            if (_currentDocument == null) return;

            if (itemType.Contains("ink"))
            {
                _currentDocument.InkColor = itemType switch
                {
                    "ink_black" => TaskSystem.InkColor.Черные,
                    "ink_red" => TaskSystem.InkColor.Красные,
                    "ink_green" => TaskSystem.InkColor.Зеленые,
                    "ink_purple" => TaskSystem.InkColor.Фиолетовые,
                    _ => _currentDocument.InkColor
                };
            }
            else if (itemType.Contains("form") || itemType.Contains("parchment") || itemType.Contains("card"))
            {
                _currentDocument.PaperType = itemType switch
                {
                    "form_7b" => TaskSystem.PaperType.Бланк_формы_7_Б,
                    "form_aay" => TaskSystem.PaperType.Бланк_формы_АА_Я,
                    "parchment" => TaskSystem.PaperType.Пергамент,
                    "card" => TaskSystem.PaperType.Карточка,
                    _ => _currentDocument.PaperType
                };
            }

            if (stationType == "signing_desk")
            {
                _currentDocument.IsSigned = true;
                if (_currentTask != null)
                {
                    _currentDocument.StampPos = _currentTask.Requirements.requiredStampPos;
                }
            }
            else if (stationType.Contains("stamping_desk") && itemType.Contains("stamp"))
            {
                _currentDocument.IsStamped = true;
                _currentDocument.StampType = itemType switch
                {
                    "stamp_approve" => TaskSystem.StampType.Одобрено,
                    "stamp_reject" => TaskSystem.StampType.Отклонено,
                    "stamp_review" => TaskSystem.StampType.На_рассмотрении,
                    "stamp_official" => TaskSystem.StampType.Официальная_печать,
                    "stamp_secret" => TaskSystem.StampType.Секретная_печать,
                    _ => _currentDocument.StampType
                };
            }

            Debug.Log($"<color=yellow>Документ обновлен: Чернила={_currentDocument.InkColor}, Подпись={_currentDocument.IsSigned}, Штамп={_currentDocument.IsStamped}</color>");
        }

        private void CheckTaskRequirements()
        {
            if (_currentTask == null || _currentTask.IsCompleted) return;
        }

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
                AudioManager.Instance?.PlaySpecialSoundByIndex(3);
            }
        }

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

        public void HandleRabbitInterference()
        {
            if (_currentTask != null && !_currentTask.IsCompleted)
            {
                _currentTask.Corrupt();
                OnTaskCorrupted?.Invoke(_currentTask);

                Debug.Log("<color=yellow>Кролик изменил требования задания!</color>");
                Debug.Log($"<color=white>Новые требования: {_currentTask.Description}</color>");

                if (GameCycle.Instance != null)
                {
                    GameCycle.Instance.AddStress(5f);
                }
            }
        }

        public TaskSystem.BureaucraticTask GetCurrentTask()
        {
            return _currentTask;
        }

        public int GetCompletedTasks()
        {
            return _totalTasksCompleted;
        }

        public string GetTaskTitle()
        {
            return _currentTask?.Title;
        }

        public string GetTaskDescription()
        {
            return _currentTask?.Description;
        }

        public List<string> GetRequiredItemsForCurrentTask()
        {
            List<string> items = new List<string>();

            if (_currentTask != null)
            {
                TaskSystem.DocumentRequirement req = _currentTask.Requirements;

                items.Add($"Чернила: {req.requiredInkColor}");
                items.Add($"Бумага: {req.requiredPaperType}");
                items.Add($"Подпись: {req.requiredStampPos}");
                if (req.isStamped)
                    items.Add($"Штамп: {req.requiredStampType}");
            }

            return items;
        }
        public Document GetCurrentDocument()
        {
            if (_currentDocument != null) return _currentDocument;
            return null;
        }

        public bool IsTaskActive => _isTaskActive;
    }
}
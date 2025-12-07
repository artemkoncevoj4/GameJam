using System;
using System.Collections;
using System.Collections.Generic;
using TaskSystem;
using UnityEngine;
using Random = UnityEngine.Random;
using Bunny;

namespace TaskSystem
{
    [System.Serializable]
    public class DocumentRequirement // требования к документу
    {
        public string description;
        public InkColor requiredInkColor;
        public SignaturePosition requiredSignaturePos;
        public PaperType requiredPaperType;
        public StampType requiredStampType;
        public bool isStamped;
        public bool isSigned;
        public float timePenalty = 15f; // Штраф времени при неправильном выполнении
    }

    public enum InkColor { Черные, Красные, Зеленые, Фиолетовые }
    public enum SignaturePosition { Левый_нижний, Правый_нижний, Центр, Левая_сторона }
    public enum PaperType { Бланк_формы_7_Б, Бланк_формы_АА_Я, Пергамент, Карточка }
    public enum StampType { Одобрено, Отклонено, На_рассмотрении, Официальная_печать, Секретная_печать }

    public class BureaucraticTask // Хр-ки задания
    {
        public int TaskID { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public DocumentRequirement Requirements { get; private set; }
        public float TimeAssigned { get; private set; }
        public float TimeRemaining { get; private set; }
        public bool IsCompleted { get; private set; }
        public bool IsFailed { get; private set; }
        public bool IsCorrupted { get; private set; }
        public bool IsUrgent { get; private set; }

        public BureaucraticTask(string title, DocumentRequirement req, float timeLimit, bool urgent = false) // Инициализация задания
        {
            Title = title;
            Requirements = req;
            Description = GenerateDescription(Requirements);
            TimeAssigned = timeLimit;
            TimeRemaining = timeLimit;
            IsUrgent = urgent;
        }

        public bool UpdateTimer(float deltaTime) // Изменение таймера задания
        {
            if (IsCompleted || IsFailed) return false;

            TimeRemaining -= deltaTime;
            if (TimeRemaining <= 0)
            {
                IsFailed = true;
                return true;
            }
            return false;
        }

        public void Corrupt() // Изменение задания
        {
            if (IsCorrupted || IsCompleted) return;

            IsCorrupted = true;

            // Случайно меняем одно или несколько требований
            int changes = Random.Range(1, 3);
            for (int i = 0; i < changes; i++)
            {
                switch (Random.Range(0, 4))
                {
                    case 0:
                        Requirements.requiredInkColor = (InkColor)Random.Range(0, 4);
                        break;
                    case 1:
                        Requirements.requiredSignaturePos = (SignaturePosition)Random.Range(0, 4);
                        break;
                    case 2:
                        Requirements.requiredPaperType = (PaperType)Random.Range(0, 4);
                        break;
                    case 3:
                        Requirements.requiredStampType = (StampType)Random.Range(0, 5);
                        break;
                }
            }

            Description = GenerateDescription(Requirements) + " (Требования изменены Кроликом!)";
        }

        public bool Validate(Document document) // Проверка документа на правльность
        {
            if (IsCompleted || IsFailed) return false;

            bool isValid =
                document.InkColor == Requirements.requiredInkColor &&
                document.SignaturePos == Requirements.requiredSignaturePos &&
                document.PaperType == Requirements.requiredPaperType &&
                document.IsSigned == Requirements.isSigned && 
                (Requirements.isStamped ? (document.IsStamped && document.StampType == Requirements.requiredStampType) : !document.IsStamped);

            return isValid;
        }

        public void Complete() // Выполнение задания
        {
            IsCompleted = true;
            IsFailed = false;
        }

        public void Fail() // Провал задания
        {
            IsFailed = true;
            IsCompleted = false;
        }

        private string GenerateDescription(DocumentRequirement req) // Создание описания
        {
            return $"Заполнить {req.requiredPaperType} {req.requiredInkColor} чернилами. " +
                   $"Подпись: {req.requiredSignaturePos}. " +
                   $"{(req.isStamped ? $"Штамп: {req.requiredStampType}." : "Без штампа.")} " +
                   $"Дедлайн: {TimeAssigned:F0} секунд.";
        }
    }

    public class Document // Документ
    {
        public InkColor InkColor { get; set; }
        public SignaturePosition SignaturePos { get; set; }
        public PaperType PaperType { get; set; }
        public StampType StampType { get; set; }
        public bool IsSigned { get; set; }
        public bool IsStamped { get; set; }
    }
}

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance { get; private set; }

    [Header("Настройки заданий")]
    [SerializeField] private float _baseTaskTime = 60f;
    [SerializeField] private float _timeReductionPerTask = 5f;
    [SerializeField] private float _minTaskTime = 20f;
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
    public event Action<float> OnTaskTimerUpdated; // Оставшееся время текущего задания

    void Awake() // Единственность TaskManager'a
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Не уничтожаем при загрузке новой сцены
        DontDestroyOnLoad(gameObject);
        
        Debug.Log("TaskManager: Awake called, Instance set");
    }

    void Start()
    {
        Debug.Log("TaskManager: Start called");
        
        _currentTaskTimeLimit = _baseTaskTime;
        
        // Подписка на события GameCycle
        // Используем безопасный поиск вместо прямой ссылки
        StartCoroutine(FindBunnyAndSubscribe());
        
        // Подписка на события инвентаря
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.OnInventoryChanged += CheckTaskRequirements;
        }
    }
    private IEnumerator FindBunnyAndSubscribe()
    {
        // Ждем пока Bunny загрузится и инициализируется
        yield return new WaitForSeconds(0.1f);
        
        var bunny = FindObjectOfType<Bunny.Bunny>();
        if (bunny != null)
        {
            Debug.Log("TaskManager: Found Bunny, subscribing to events");
            bunny.OnRabbitActive += HandleRabbitInterference;
        }
        else
        {
            Debug.LogWarning("TaskManager: Bunny not found in scene");
        }
    }
    void OnDestroy() //Конец
    {
        if(Bunny.Bunny.Instance != null)
        {
            Bunny.Bunny.Instance.OnRabbitActive -= HandleRabbitInterference;
        }

        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.OnInventoryChanged -= CheckTaskRequirements;
        }
    }

    void Update() // Итерация цикла (OnTaskUpdated?)
    {
        if (GameCycle.Instance == null || GameCycle.Instance.CurrentState != GameCycle.GameState.Playing)
            return;

        // Обновление таймера текущего задания
        if (_currentTask != null && !_currentTask.IsCompleted && !_currentTask.IsFailed)
        {
            if (_currentTask.UpdateTimer(Time.deltaTime))
            {
                FailCurrentTask("Время вышло!");
            }

            OnTaskTimerUpdated?.Invoke(_currentTask.TimeRemaining);
        }
    }

    public void StartNewTask() // Создание задания
    {
        if (_isTaskActive)
        {
            Debug.LogWarning("Попытка начать новое задание, когда текущее еще активно!");
            return;
        }

        //ClearSpawnedItems();
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

        _currentTaskTimeLimit = Mathf.Max(_minTaskTime, _currentTaskTimeLimit - _timeReductionPerTask);

        _isTaskActive = true;
        OnNewTask?.Invoke(_currentTask);

        Debug.Log($"<color=cyan>Новое задание: {_currentTask.Title}</color>");
        Debug.Log($"<color=white>{_currentTask.Description}</color>");

        // Генерируем необходимые предметы в мире
        SpawnRequiredItems(req);
    }

    private TaskSystem.DocumentRequirement GenerateRandomRequirements()
    {
        TaskSystem.DocumentRequirement req = new TaskSystem.DocumentRequirement();

        // Случайные требования
        req.requiredInkColor = (TaskSystem.InkColor)Random.Range(0, 4);
        req.requiredSignaturePos = (TaskSystem.SignaturePosition)Random.Range(0, 4);
        req.requiredPaperType = (TaskSystem.PaperType)Random.Range(0, 4);
        req.requiredStampType = (TaskSystem.StampType)Random.Range(0, 5);
        req.isStamped = Random.value > 0.3f; // 70% шанс что нужен штамп
        req.isSigned = Random.value > 0.1f; // 90% шанс что нужна подпись
        req.timePenalty = 15f;

        return req;
    } // Рандомные требования (дописать изменение штрафа)

    private string GetRandomTaskTitle() // Случайные названия
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

    private void SpawnRequiredItems(TaskSystem.DocumentRequirement req) // Спавн предметов для задания (пока пусто)
    {
        // Здесь должна быть логика спавна предметов в мире
        // Например, создание нужных чернил, бумаги и штампа

        Debug.Log($"Для задания нужны: {req.requiredInkColor} чернила, {req.requiredPaperType}, {(req.isStamped ? $"штамп {req.requiredStampType}" : "штамп не нужен")}");
    }
    /*
    // Вызывается при взаимодействии с предметом
    public void ReportItemCollected(string itemType, int itemID)
    {
        Debug.Log($"Собран предмет: {itemType} (ID: {itemID})");
        CheckTaskRequirements();
    }

    // Вызывается при использовании рабочей станции
    public void ReportStationUsed(string stationType, string itemType, int stationID)
    {
        Debug.Log($"Станция {stationType} использована с предметом {itemType}");

        // Проверяем, соответствует ли использование текущему заданию
        if (_currentTask != null && !_currentTask.IsCompleted)
        {
            // Создаем "документ" на основе использованных предметов
            TaskSystem.Document doc = CreateDocumentFromUsage(stationType, itemType);

            if (_currentTask.Validate(doc))
            {
                CompleteCurrentTask();
            }
            else
            {
                // Неправильное использование - штраф
                if (GameCycle.Instance != null)
                {
                    GameCycle.Instance.AddStress(10f);
                    Debug.Log("<color=orange>Неправильное использование предмета! +10 стресса</color>");
                }
            }
        }
    }*/

    private void UpdateDocumentFromUsage(string stationType, string itemType)
    {
        if (_currentDocument == null) return;

        // Маппинг для чернил
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
        // Маппинг для бумаги (если станция используется для выбора бумаги)
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

        // Действия на станциях
        if (stationType == "signing_desk")
        {
            _currentDocument.IsSigned = true;
            // Здесь может быть логика выбора SignaturePos, пока берем по умолчанию
            _currentDocument.SignaturePos = _currentTask.Requirements.requiredSignaturePos;
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
        // Проверяем, есть ли у игрока все необходимые предметы для текущего задания
        // Это упрощенная проверка - в реальной игре нужно проверять точные требования

        if (_currentTask == null || _currentTask.IsCompleted) return;

        // Здесь можно добавить логику автоматической проверки готовности документа
        // Например, если все предметы собраны и использованы на правильных станциях
    }

    public void SubmitDocument()
    {
        if (_currentTask == null || _currentTask.IsCompleted || _currentTask.IsFailed)
        {
            Debug.Log("Нет активного задания!");
            return;
        }

        if (_currentDocument == null) // Добавил проверку на null
        {
            Debug.Log("Документ не создан!");
            return;
        }

        if (_currentTask.Validate(_currentDocument))
        {
            CompleteCurrentTask();
        }
        else
        {
            FailCurrentTask("Неверно заполнен документ!");
        }
    }

    private void CompleteCurrentTask()
    {
        _currentTask.Complete();
        _totalTasksCompleted++;
        _isTaskActive = false;

        OnTaskCompleted?.Invoke(_currentTask);


        // Награда за выполнение
        if (GameCycle.Instance != null)
        {
            GameCycle.Instance.CompleteTask();
        }
        if (Bunny.Bunny.Instance != null) // Добавил уведомление кролика
        {
            Bunny.Bunny.Instance.ChangeToNoTask();
        }
    }

    private void FailCurrentTask(string reason)
    {
        _currentTask.Fail();
        _isTaskActive = false;

        OnTaskFailed?.Invoke(_currentTask);

        Debug.Log($"<color=red>Задание провалено: {reason}</color>");

        // Штраф за провал
        if (GameCycle.Instance != null)
        {
            GameCycle.Instance.FailTask(_currentTask.Requirements.timePenalty);
        }

        // Новое задание (возможно, то же самое с новыми требованиями)
        StartNewTask();
    }

    public void HandleRabbitInterference()
    {
        if (_currentTask != null && !_currentTask.IsCompleted)
        {
            _currentTask.Corrupt();
            OnTaskCorrupted?.Invoke(_currentTask);

            Debug.Log("<color=red>Кролик изменил требования задания!</color>");
            Debug.Log($"<color=yellow>Новые требования: {_currentTask.Description}</color>");

            // Штрафной стресс за вмешательство
            if (GameCycle.Instance != null)
            {
                GameCycle.Instance.AddStress(5f);
            }
        }
    }

    // Публичные методы для UI и других систем
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
            items.Add($"Подпись: {req.requiredSignaturePos}");
            if (req.isStamped)
                items.Add($"Штамп: {req.requiredStampType}");
        }

        return items;
    }
    public bool IsTaskActive => _isTaskActive;
}
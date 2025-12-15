using System.Collections;
using UnityEngine;
using DialogueManager;
using System;
using TaskSystem;
using Shaders.ScreenEffects;

namespace Bunny {
    // Муштаков А.Ю. (логика появления и влияния)
    // Идрисов Д.С. (диалоги и хаос эффекты)

    /// <summary>
    /// Управляет поведением кролика в игре, включая его появление, влияние на задания и создание хаотических эффектов.
    /// Реализует паттерн Singleton для глобального доступа и координации поведения кролика.
    /// </summary>
    public class Bunny : MonoBehaviour
    {
        /// <summary>
        /// Статический экземпляр для реализации паттерна Singleton.
        /// </summary>
        public static Bunny Instance { get; private set; }
        
        [Header("Эффекты экрана")]
        // Кэшированные ссылки на эффекты
        private ScreenShake _cachedScreenShake;
        private Fire_text _cachedFireText;
        private ScreenFliskers _cachedScreenFliskers;
        
        [Header("Позиция появления")]
        [SerializeField] private Transform _appearPoint_Window;

        [Header("Настройки поведения")]
        [SerializeField] private float _shoutDuration = 3f; // Сколько секунд заяц "кричит"
        [SerializeField] private float _peekChance = 0.8f; // Шанс подглядывания вместо крика
        [SerializeField] private float _peekDuration = 2f; // Длительность подглядывания
        private SpriteRenderer _spriteRenderer;
        private bool _isActive = false;
        private bool _isTaskPresent = false;
        private Coroutine _currentBehavior;
        private BunnyDialogueManager _bunnyDialogueManager;
        
        [Header("Диалоги Зайца")] 
        [SerializeField] private Dialogue _shoutDialogue;
        
        // Хранит индекс следующего предложения для _shoutDialogue
        private int _currentDialogueIndex = 0;
        public bool IsActive => _isActive; // Публичный геттер для _isActive
        public event Action OnRabbitActive; // Событие, когда заяц становится активным
        
        /// <summary>
        /// Статический флаг, указывающий, находится ли кролик в режиме подглядывания.
        /// </summary>
        public static bool Peeking = false;
        
        public int CurrentDialogueIndex 
        { 
            get => _currentDialogueIndex; 
            set => _currentDialogueIndex = value; 
        }

        /// <summary>
        /// Инициализирует Singleton при создании объекта.
        /// Уничтожает дублирующиеся экземпляры для обеспечения единственности.
        /// </summary>
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// Инициализирует компоненты, подписывается на события и настраивает начальное состояние.
        /// Выполняет отложенную подписку на события TaskManager для гарантии его инициализации.
        /// </summary>
        void Start()
        {
            if (GameCycle.Instance != null)
            {
                GameCycle.Instance.OnRabbitAppearing += Appear;
                GameCycle.Instance.OnRabbitLeaving += Leave;
            }
            
            StartCoroutine(SubscribeToTaskManagerEvents());
            
            _spriteRenderer = GetComponent<SpriteRenderer>();
            
            InitializeShaderObjects();
            
            SetVisible(false);
            if (_appearPoint_Window != null)
            {
                transform.position = _appearPoint_Window.position;
                transform.rotation = _appearPoint_Window.rotation;
            }

            _bunnyDialogueManager = FindAnyObjectByType<BunnyDialogueManager>();
            if (_bunnyDialogueManager == null)
            {
                Debug.LogError("<color=red>BunnyDialogueManager не найден в сцене!</color>");
            }
        }
        
        /// <summary>
        /// Обновляет позицию кролика каждый кадр, если он не находится в режиме подглядывания.
        /// Подглядывание может изменять позицию кролика независимо от базовой точки появления.
        /// </summary>
        void Update() // TODO Сделать peeking появление глаз
        {
            if (!Peeking)
            {
                transform.position = _appearPoint_Window.position;
                transform.rotation = _appearPoint_Window.rotation;
            }
        }
        
        //! Метод сгенерирован ИИ
        /// <summary>
        /// Инициализирует ссылки на шейдерные эффекты с использованием многоуровневого поиска.
        /// Выполняет поиск существующих объектов, а при их отсутствии создает новые.
        /// </summary>
        private void InitializeShaderObjects()
        {
            _cachedScreenShake = FindAnyObjectByType<ScreenShake>();
            _cachedFireText = FindAnyObjectByType<Fire_text>();
            _cachedScreenFliskers = FindAnyObjectByType<ScreenFliskers>();
            
            if (_cachedScreenShake == null)
                _cachedScreenShake = FindObjectInScene<ScreenShake>();
            if (_cachedFireText == null)
                _cachedFireText = FindObjectInScene<Fire_text>();
            if (_cachedScreenFliskers == null)
                _cachedScreenFliskers = FindObjectInScene<ScreenFliskers>();
            
            Debug.Log($"Screen_Shake: {(_cachedScreenShake != null ? $"<color=green>найден ({_cachedScreenShake.gameObject.name})</color>" : "<color=red>не найден</color>")}");
            Debug.Log($"Fire_text: {(_cachedFireText != null ? $"<color=green>найден ({_cachedFireText.gameObject.name})</color>" : "<color=red>не найден</color>")}");
            Debug.Log($"ScreenFliskers: {(_cachedScreenFliskers != null ? $"<color=green>найден ({_cachedScreenFliskers.gameObject.name})</color>" : "<color=red>не найден</color>")}");
            
            if (_cachedScreenShake == null)
            {
                Debug.LogWarning("<color=yellow>Screen_Shake не найден, создаю новый объект...</color>");
                _cachedScreenShake = CreateShaderObject<ScreenShake>("ScreenShake");
            }
            if (_cachedFireText == null)
            {
                Debug.LogWarning("<color=yellow>Fire_text не найден, создаю новый объект...</color>");
                _cachedFireText = CreateShaderObject<Fire_text>("FireText");
            }
            if (_cachedScreenFliskers == null)
            {
                Debug.LogWarning("<color=yellow>ScreenFliskers не найден, создаю новый объект...</color>");
                _cachedScreenFliskers = CreateShaderObject<ScreenFliskers>("ScreenFliskers");
            }
        }
        
        //! Метод сгенерирован ИИ
        /// <summary>
        /// Ищет объекты заданного типа во всех объектах сцены, включая неактивные.
        /// </summary>
        /// <typeparam name="T">Тип MonoBehaviour для поиска.</typeparam>
        /// <returns>Найденный объект или null.</returns>
        private T FindObjectInScene<T>() where T : MonoBehaviour
        {
            // Ищем среди всех объектов, включая неактивные
            T[] allObjects = Resources.FindObjectsOfTypeAll<T>();
            foreach (T obj in allObjects)
            {
                // Исключаем префабы
                if (obj.gameObject.scene.IsValid())
                {
                    // Активируем объект, если он неактивен
                    if (!obj.gameObject.activeSelf)
                    {
                        obj.gameObject.SetActive(true);
                        Debug.Log($"<color=green>Активирован объект: {obj.gameObject.name}</color>");
                    }
                    return obj;
                }
            }
            return null;
        }
        
        //! Метод сгенерирован ИИ
        /// <summary>
        /// Создает новый игровой объект с указанным компонентом.
        /// Используется как резервный механизм при отсутствии необходимых эффектов в сцене.
        /// </summary>
        /// <typeparam name="T">Тип компонента для добавления.</typeparam>
        /// <param name="name">Имя создаваемого объекта.</param>
        /// <returns>Созданный компонент.</returns>
        private T CreateShaderObject<T>(string name) where T : MonoBehaviour
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(transform.root); // Помещаем в корень сцены
            return obj.AddComponent<T>();
        }
        
        /// <summary>
        /// Ожидает инициализацию TaskManager и подписывается на его события.
        /// Гарантирует, что подписка произойдет только после полной инициализации TaskManager.
        /// </summary>
        private IEnumerator SubscribeToTaskManagerEvents()
        {
            // Ждем пока TaskManager инициализируется
            while (TaskManager.Instance == null)
            {
                yield return new WaitForSeconds(0.1f);
                Debug.Log("<color=cyan>Bunny: Waiting for TaskManager to initialize...</color>");
            }
            
            Debug.Log("<color=green>Bunny: TaskManager found, subscribing to events</color>");
            TaskManager.Instance.OnTaskCompleted += OnTaskCompletedHandler;
            TaskManager.Instance.OnTaskFailed += OnTaskCompletedHandler;
        }

        /// <summary>
        /// Отписывается от всех событий при уничтожении объекта.
        /// Предотвращает утечки памяти и попытки обращения к уничтоженным объектам.
        /// </summary>
        void OnDestroy()
        {
            if (GameCycle.Instance != null)
            {
                GameCycle.Instance.OnRabbitAppearing -= Appear; // Отписка от событий
                GameCycle.Instance.OnRabbitLeaving -= Leave;
            }

            if (TaskManager.Instance != null)
            {
                TaskManager.Instance.OnTaskCompleted -= OnTaskCompletedHandler;
                TaskManager.Instance.OnTaskFailed -= OnTaskCompletedHandler;
            }
        }

        /// <summary>
        /// Запускает появление кролика с выбором поведения (подглядывание или крик).
        /// Вызывает событие OnRabbitActive для уведомления других систем о появлении кролика.
        /// </summary>
        public void Appear()    
        {
            if (_isActive) return;
        
            _isActive = true;
            
            // Вызываем событие активности кролика
            OnRabbitActive?.Invoke();
        
            bool willPeek = _isTaskPresent ? UnityEngine.Random.value < _peekChance : false;
            Debug.Log($"<color=white>Шанс peek {willPeek}</color>");
        
            if (willPeek)
            {
                Peeking = true;
                _currentBehavior = StartCoroutine(PeekBehavior());
            }
            else
            {
                SetVisible(true);
                Peeking = false;
                transform.position = _appearPoint_Window.position;
                transform.rotation = _appearPoint_Window.rotation;
                _currentBehavior = StartCoroutine(ShoutBehavior());
            }
            Debug.Log($"<color=cyan>Заяц появился! Поведение: {(willPeek ? "Подглядывает" : "Кричит")}</color>");
        }
    
        /// <summary>
        /// Завершает активность кролика и скрывает его.
        /// Останавливает текущее поведение и сбрасывает состояние.
        /// </summary>
        public void Leave()
        {
            if (!_isActive) return;
        
            _isActive = false;
        
            if (_currentBehavior != null)
            {
                StopCoroutine(_currentBehavior);
                _currentBehavior = null;
            }

            SetVisible(false);

            Debug.Log("<color=cyan>Заяц ушёл</color>");
        }
        
        /// <summary>
        /// Поведение крика кролика: назначает или изменяет задание.
        /// </summary>
        private IEnumerator ShoutBehavior()
        {
            AssignOrModifyTask();
            yield return new WaitForSeconds(_shoutDuration);
        }
    
        /// <summary>
        /// Поведение подглядывания кролика: вызывает хаотические эффекты.
        /// </summary>
        private IEnumerator PeekBehavior()
        {
            TriggerChaosEffect();
        
            yield return new WaitForSeconds(_peekDuration);

            Peeking = false;
        }
    
    
        /// <summary>
        /// Возвращает имя кролика из диалога или стандартное значение.
        /// </summary>
        public string BunnyName => _shoutDialogue != null ? _shoutDialogue.name : "Заяц";

        /// <summary>
        /// Назначает новое задание или искажает существующее в зависимости от состояния игры.
        /// Определяет поведение кролика относительно системы заданий.
        /// </summary>
        private void AssignOrModifyTask()
        {
            //Debug.Log($"<color=cyan>Bunny: AssignOrModifyTask called. _bunnyDialogueManager is</color> {(_bunnyDialogueManager == null ? "<color=red>null</color>" : "<color=green>not null</color>")}");
            
            if (TaskManager.Instance == null) 
            {
                //Debug.LogError("<color=red>Bunny: TaskManager not found! Trying to find it...</color>");
                TaskManager taskManager = FindAnyObjectByType<TaskManager>();
                if (taskManager == null)
                {
                    //Debug.LogError("<color=red>Bunny: TaskManager not found in scene! Cannot assign task.</color>");
                    Leave();
                    return;
                }
                else
                {
                    //Debug.Log("<color=green>Bunny: Found TaskManager in scene</color>");
                }
            }

            var currentTask = TaskManager.Instance.GetCurrentTask();

            if (_isTaskPresent == false)
            {
                //Debug.Log("<color=yellow>Bunny: No current task, starting new task</color>");
                TaskManager.Instance.StartNewTask();
                _isTaskPresent = true;
                currentTask = TaskManager.Instance.GetCurrentTask();
                
                StartCoroutine(ShowTaskDialogueWithDelay(0.1f));
            }
            else if (!currentTask.IsCorrupted)
            {
                if (UnityEngine.Random.value > 0.7f) 
                {
                    //Debug.Log("<color=cyan>Bunny: Corrupting current task</color>");
                    TaskManager.Instance.HandleRabbitInterference();
                    
                    StartCoroutine(ShowTaskDialogueWithDelay(0.1f));
                }
                else
                {
                    TriggerChaosEffect();
                    //Debug.Log("<color=cyan>Заяц решил не трогать задание</color>");
                    Leave();
                    return;
                }
            }
            else
            {
                //Debug.Log("Задание уже изменено ранее");
    
                if (UnityEngine.Random.value < 0.5f)
                {
                    //Debug.Log("<color=cyan>Bunny: Задание уже изменено ранее, заяц подглядывает</color>");
                    if (_currentBehavior != null)
                    {
                        StopCoroutine(_currentBehavior);
                    }

                        transform.position = _appearPoint_Window.position;
                        transform.rotation = _appearPoint_Window.rotation;
                    // Запускаем поведение подглядывания
                    _currentBehavior = StartCoroutine(PeekBehavior());
                    return; // Не продолжаем выполнение, т.к. уже запустили PeekBehavior
                }
                else
                {
                    //Debug.Log("<color=cyan>Bunny: Задание уже изменено ранее, заяц просто уходит</color>");
                    Leave();
                    return;
                }
            }
        }
        
        /// <summary>
        /// Откладывает показ диалога задания на указанное время.
        /// </summary>
        /// <param name="delay">Задержка в секундах.</param>
        private IEnumerator ShowTaskDialogueWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            ShowTaskDialogue();
        }
   
        /// <summary>
        /// Отображает диалог с заданием через BunnyDialogueManager.
        /// </summary>
        private void ShowTaskDialogue()
        {
            if (_bunnyDialogueManager == null) 
            {
                Debug.LogError("<color=red>BunnyDialogueManager не найден!</color>");
                Leave();
                return;
            }
            
            Dialogue taskDialogue = _bunnyDialogueManager.GetTaskDialogueForBunny(this);
            
            if (taskDialogue == null || taskDialogue.sentences.Length == 0)
            {
                Debug.Log($"<color=red>Bunny: Не удалось получить описание задания. Заяц уходит.</color>");
                Leave(); 
                return;
            }
            
            Debug.Log($"<color=green>Bunny: Starting dialogue with: {taskDialogue.sentences[0]}</color>");
            
            UpdateTaskUI();
            
            _bunnyDialogueManager.StartBunnyDialogue(taskDialogue, this);
        }
        
        /// <summary>
        /// Обновляет пользовательский интерфейс задания.
        /// Использует TaskUIManager при его наличии или выполняет ручное обновление.
        /// </summary>
        private void UpdateTaskUI()
        {
            // Используем TaskUIManager если есть, иначе ищем компоненты вручную
            if (UI.TaskUIManager.Instance != null)
            {
                UI.TaskUIManager.Instance.UpdateTaskUI();
            }
            else
            {
                UI.SimpleTaskTimer timer = FindAnyObjectByType<UI.SimpleTaskTimer>();
                UI.TaskDisplayUI display = FindAnyObjectByType<UI.TaskDisplayUI>();
                
                if (timer != null) timer.ForceStartTimer();
                if (display != null) display.ForceShowCurrentTask();
            }
        }
        
        /// <summary>
        /// Запускает случайный хаотический эффект при подглядывании кролика.
        /// Включает визуальные эффекты, изменение игровой механики и звуковое сопровождение.
        /// </summary>
        private void TriggerChaosEffect()
        {
            Debug.Log("<color=yellow>Заяц подглядывает и вызывает хаос!</color>");
            
            if (GameCycle.Instance != null)
            {
                GameCycle.Instance.AddStress(3f);
            }

            if (AudioManager.Instance == null)
            {
                Debug.LogError("AudioManager не найден");
                return;
            }

            float randomEffect = UnityEngine.Random.value;
            
            if (randomEffect < 0.1f) // 10% шанс - инверсия управления
            {
                Debug.Log("<color=white>Хаос: Инверсия управления на 3 секунды!</color>");
                Player.MovementPlayer.invertControls = true;
            }
            else if (randomEffect < 0.20f) // 10% шанс - замедление времени
            {
                Debug.Log("<color=white>Хаос: Временное замедление!</color>");
                Time.timeScale = 0.5f;
                Invoke(nameof(ResetTimeScale), 2f);
            }
            else if (randomEffect < 0.35f) // 15% шанс - тряска экрана
            {
                Debug.Log("<color=white>Хаос: Плывет экран!</color>");
                if (_cachedScreenShake != null)
                {
                    _cachedScreenShake.Start_shaking();
                    AudioManager.Instance.PlayRandomChaosSound();
                }
                else
                {
                    Debug.LogError("<color=red>Screen_Shake не найден!</color>");
                }
            }
            else if (randomEffect < 0.50f) // 15% шанс - затемнение экрана
            {
                Debug.Log("<color=white>Хаос: экран потемнел!</color>");
                StartCoroutine(QuickDarkenAndLighten());
                AudioManager.Instance.PlayRandomChaosSound();
            }
            else if (randomEffect < 0.65f) // 15% шанс - мигание текста
            {
                Debug.Log("<color=white>Хаос: FireText эффект!</color>");
                if (_cachedFireText != null)
                {
                    var currentTask = TaskSystem.TaskManager.Instance?.GetCurrentTask();
                    string taskText = currentTask != null ? currentTask.Description : "Current Task"; 
                    _cachedFireText.Fire(taskText);
                
                    Debug.Log($"<color=green>Хаос: FireText запущен для задачи: {taskText}!</color>");
                }
                else
                {
                    Debug.LogError("<color=red>Fire_text не найден!</color>");
                }
            }
            else if (randomEffect < 0.75f) // 10% шанс - мигание (ScreenFliskers)
            {
                Debug.Log("<color=white>Хаос: Мигание экрана!</color>");
                AudioManager.Instance.PlayRandomChaosSound();
                if (_cachedScreenFliskers != null)
                {
                    _cachedScreenFliskers.Start_flickers();
                    Debug.Log("<color=green>Вызван ScreenFliskers</color>");
                }
                else
                {
                    Debug.LogError("<color=red>ScreenFliskers не найден!</color>");
                }
            }
            else if (randomEffect < 0.90f) // 15% шанс - Эффект сердцебиения
            {
                if (Shaders.ScreenEffects.ScreenFadeManager.Instance != null)
                {
                    Debug.Log("<color=green> Вызван Эффект серцебиения </color>");
                    Shaders.ScreenEffects.ScreenFadeManager.Instance.BlinkScreen(0.3f, 2, Color.red);
                    AudioManager.Instance.PlaySoundByName("heartbeat");
                    
                }
                else
                {
                      Debug.LogError("<color=red>ScreenFadeManager.Instance.BlinkScreen не найден!</color>");
                }
            }
            else // 10% шанс - звуковой эффект
            {
                Debug.Log("<color=white>Хаос: Случайный звуковой эффект!</color>");
                AudioManager.Instance.PlayRandomSpecialSFX();
            }
            
            if (UnityEngine.Random.value < 0.3f) // 30% шанс на дополнительный эффект
            {
                Debug.Log("<color=white>Дополнительный эффект хаоса!</color>");
                float extraEffect = UnityEngine.Random.value;
                
                if (extraEffect < 0.5f) // 50%  - тряска экрана
                {
                    ScreenShake extraShake = FindAnyObjectByType<ScreenShake>();
                    if (extraShake != null)
                    {
                        extraShake.Start_shaking();
                        Debug.Log("<color=white>Хаос: Дополнительная тряска экрана!</color>");
                    }
                }
                else // 50% - мигание текста
                {
                    Fire_text extraFireText = FindAnyObjectByType<Fire_text>();
                    if (extraFireText != null)
                    {
                        var currentTask = TaskSystem.TaskManager.Instance?.GetCurrentTask();
                        string taskText = currentTask != null ? currentTask.Description : "Current Task";

                        _cachedFireText.Fire(taskText);
                        
                        Debug.Log($"<color=green>Хаос: FireText запущен для задачи: {taskText}!</color>");
                        Debug.Log("<color=white>Хаос: Дополнительный FireText!</color>");
                    }
                }
            }
        }

        /// <summary>
        /// Быстрое затемнение и осветление экрана с использованием ScreenFadeManager.
        /// Создает эффект мигания для усиления хаотического воздействия.
        /// </summary>
        private IEnumerator QuickDarkenAndLighten()
        {
            if (ScreenFadeManager.Instance == null)
            {
                Debug.LogError("<color=red>Bunny: QuickDarkenAndLighten НЕ ЗАПУЩЕН! ScreenFadeManager.Instance == null. Убедитесь, что объект с ScreenFadeManager активен на сцене.</color>");
                yield break;
            }

            Debug.Log("<color=green>Bunny: Запуск QuickDarkenAndLighten (успешная проверка ScreenFadeManager).</color>");

            // Быстрое затемнение до черного (0.8f) за 0.2 секунды
            ScreenFadeManager.StaticFadeIn(1f);
            yield return new WaitForSeconds(2f);

            // Осветление обратно (0f) за 0.2 секунды
            ScreenFadeManager.StaticFadeOut(1f);
        }

        /// <summary>
        /// Восстанавливает нормальную скорость игры после временного замедления.
        /// </summary>
        private void ResetTimeScale()
        {
            Time.timeScale = 1f;
        }
    
        /// <summary>
        /// Устанавливает видимость кролика.
        /// </summary>
        /// <param name="visible">Флаг видимости.</param>
        private void SetVisible(bool visible)
        {
            _spriteRenderer.enabled = visible;
        }

        /// <summary>
        /// Обработчик событий завершения или провала задания.
        /// </summary>
        /// <param name="task">Задание, которое было завершено или провалено.</param>
        private void OnTaskCompletedHandler(TaskSystem.BureaucraticTask task)
        {
            ChangeToNoTask();
        }

        /// <summary>
        /// Сбрасывает состояние наличия активного задания.
        /// Вызывается при завершении задания для подготовки к следующему появлению кролика.
        /// </summary>
        public void ChangeToNoTask()
        {
            _isTaskPresent = false;
        }
    }
}
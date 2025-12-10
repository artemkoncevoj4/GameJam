using System.Collections;
using UnityEngine;
using DialogueManager;
using System;
using TaskSystem;
using Shaders.ScreenEffects;

namespace Bunny {
    public class Bunny : MonoBehaviour
    {
        public static Bunny Instance { get; private set; } // Добавил статический экземпляр
        [Header("Эффекты экрана")]
        // Кэшированные ссылки на эффекты
        private ScreenShake _cachedScreenShake;
        private Fire_text _cachedFireText;
        private ScreenFliskers _cachedScreenFliskers;
        [Header("Позиция появления")]
        [SerializeField] private Transform _appearPoint_Window;
        [SerializeField] private Transform _appearPoint_Door1;
        [SerializeField] private Transform _appearPoint_Door2;// Дверь или окно, куда прибегает заяц

        [Header("Настройки поведения")]
        [SerializeField] private float _shoutDuration = 3f; // Сколько секунд заяц "кричит"
        [SerializeField] private float _peekChance = 0.8f; // Шанс подглядывания вместо крика //! Изменен с 0.6f до 0.8f
        [SerializeField] private float _peekDuration = 2f; // Длительность подглядывания
    
        [Header("Эффекты")]
        [SerializeField] private GameObject _chaosEffect; // Визуальный эффект хаоса
        private SpriteRenderer _spriteRenderer;
        private bool _isActive = false;
        private bool _isTaskPresent = false;
        private Coroutine _currentBehavior;
        private BunnyDialogueManager _bunnyDialogueManager;
        [Header("Диалоги Зайца")] 
        [SerializeField] private Dialogue _shoutDialogue;
        // [НОВОЕ] Хранит индекс следующего предложения для _shoutDialogue
        private int _currentDialogueIndex = 0;
        public bool IsActive => _isActive; // Публичный геттер для _isActive
        public event Action OnRabbitActive; // Событие, когда заяц становится активным
        // Start is called before the first frame update
        public int CurrentDialogueIndex 
        { 
            get => _currentDialogueIndex; 
            set => _currentDialogueIndex = value; 
        }

        void Awake() // Единственность
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            if (GameCycle.Instance != null)
            {
                GameCycle.Instance.OnRabbitAppearing += Appear;
                GameCycle.Instance.OnRabbitLeaving += Leave;
            }
            
            // Подписка на события TaskManager - отложим до его инициализации
            StartCoroutine(SubscribeToTaskManagerEvents());
            
            _spriteRenderer = GetComponent<SpriteRenderer>();
            
            // Инициализация эффектов с улучшенным поиском
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
        
        private void InitializeShaderObjects()
        {
            // Используем FindAnyObjectByType для поиска активных объектов
            _cachedScreenShake = FindAnyObjectByType<ScreenShake>();
            _cachedFireText = FindAnyObjectByType<Fire_text>();
            _cachedScreenFliskers = FindAnyObjectByType<ScreenFliskers>();
            
            // Если не найдены через FindAnyObjectByType, ищем среди всех объектов
            if (_cachedScreenShake == null)
                _cachedScreenShake = FindObjectInScene<ScreenShake>();
            if (_cachedFireText == null)
                _cachedFireText = FindObjectInScene<Fire_text>();
            if (_cachedScreenFliskers == null)
                _cachedScreenFliskers = FindObjectInScene<ScreenFliskers>();
            
            // Логирование найденных эффектов
            Debug.Log($"Screen_Shake: {(_cachedScreenShake != null ? $"<color=green>найден ({_cachedScreenShake.gameObject.name})</color>" : "<color=red>не найден</color>")}");
            Debug.Log($"Fire_text: {(_cachedFireText != null ? $"<color=green>найден ({_cachedFireText.gameObject.name})</color>" : "<color=red>не найден</color>")}");
            Debug.Log($"ScreenFliskers: {(_cachedScreenFliskers != null ? $"<color=green>найден ({_cachedScreenFliskers.gameObject.name})</color>" : "<color=red>не найден</color>")}");
            
            // Если все еще не найдены, создаем их динамически
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
        
        private T CreateShaderObject<T>(string name) where T : MonoBehaviour
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(transform.root); // Помещаем в корень сцены
            return obj.AddComponent<T>();
        }
        
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

        public void Appear()    
        {
            if (_isActive) return;
        
            _isActive = true;
            
            // Вызываем событие активности кролика
            OnRabbitActive?.Invoke();
            
            SetVisible(true);
        
            bool willPeek = _isTaskPresent ? UnityEngine.Random.value < _peekChance : false;
            Debug.Log($"<color=white>Шанс peek {willPeek}</color>");
            bool whichDoor = UnityEngine.Random.value < 0.5f;
        
            if (willPeek)
            {
                if (whichDoor)
                {
                    transform.position = _appearPoint_Door1.position;
                    transform.rotation = _appearPoint_Door1.rotation;
                }
                else
                {
                    transform.position = _appearPoint_Door2.position;
                    transform.rotation = _appearPoint_Door2.rotation;
                }
                _currentBehavior = StartCoroutine(PeekBehavior());
            }
            else
            {
                transform.position = _appearPoint_Window.position;
                transform.rotation = _appearPoint_Window.rotation;
                _currentBehavior = StartCoroutine(ShoutBehavior());
            }
            Debug.Log($"<color=cyan>Заяц появился! Поведение: {(willPeek ? "Подглядывает" : "Кричит")}</color>");
        }
    
        public void Leave()
        {
            if (!_isActive) return;
        
            _isActive = false;
        
            // Остановить текущее поведение
            if (_currentBehavior != null)
            {
                StopCoroutine(_currentBehavior);
                _currentBehavior = null;
            }

            // Анимация ухода
            //if (_animator != null)
            //    _animator.SetTrigger("Leave");

            // Скрыть через секунду (или после завершения анимации)
            SetVisible(false);

            Debug.Log("<color=cyan>Заяц ушёл</color>");
        }
        
        private IEnumerator ShoutBehavior()
        {
            AssignOrModifyTask();
            yield return new WaitForSeconds(_shoutDuration);
        
            // После крика - назначить новое задание или изменить текущее
            yield return new WaitForSeconds(1f);
            if (GameCycle.Instance != null)
            {
                // GameCycle сам вызовет Leave() через событие OnRabbitLeaving
            }
        }
    
        private IEnumerator PeekBehavior()
        {
            // Анимация подглядывания
            //if (_animator != null)
            //    _animator.SetTrigger("Peek");
            
            // Визуальный эффект хаоса
            //if (_chaosEffect != null)
            //    _chaosEffect.SetActive(true);
        
            // Вызвать хаос-эффект в игре
            TriggerChaosEffect();
        
            // Ждём
            yield return new WaitForSeconds(_peekDuration);

            // Выключить эффект
            //if (_chaosEffect != null)
            //    _chaosEffect.SetActive(false);

            // Уходим
            if (GameCycle.Instance != null)
            {
                // GameCycle сам вызовет Leave() через событие OnRabbitLeaving
            }
        }
    
       //* ========== ВОЗДЕЙСТВИЕ НА ИГРУ ==========
    
        public string BunnyName => _shoutDialogue != null ? _shoutDialogue.name : "Заяц";

        //! Далее идет более прокачанная логика порчи задания.
        private void AssignOrModifyTask()
        {
            Debug.Log($"<color=cyan>Bunny: AssignOrModifyTask called. _bunnyDialogueManager is</color> {(_bunnyDialogueManager == null ? "<color=red>null</color>" : "<color=green>not null</color>")}");
            
            // Ждем TaskManager, если он еще не инициализирован
            if (TaskManager.Instance == null) 
            {
                Debug.LogError("<color=red>Bunny: TaskManager not found! Trying to find it...</color>");
                TaskManager taskManager = FindAnyObjectByType<TaskManager>();
                if (taskManager == null)
                {
                    Debug.LogError("<color=red>Bunny: TaskManager not found in scene! Cannot assign task.</color>");
                    Leave();
                    return;
                }
                else
                {
                    Debug.Log("<color=green>Bunny: Found TaskManager in scene</color>");
                }
            }

            var currentTask = TaskManager.Instance.GetCurrentTask();
            Debug.Log($"<color=cyan>Bunny: Current task is {(currentTask == null ? "<color=red>null</color>" : currentTask.Title)}</color>");

            if (_isTaskPresent == false)
            {
                Debug.Log("<color=yellow>Bunny: No current task, starting new task</color>");
                TaskManager.Instance.StartNewTask();
                _isTaskPresent = true;
                currentTask = TaskManager.Instance.GetCurrentTask();
                
                // Показываем диалог только при создании нового задания
                StartCoroutine(ShowTaskDialogueWithDelay(0.1f));
            }
            else if (!currentTask.IsCorrupted)
            {
                // Портим задание с вероятностью 30%
                if (UnityEngine.Random.value > 0.7f)  // 30% chance
                {
                    Debug.Log("<color=cyan>Bunny: Corrupting current task</color>");
                    TaskManager.Instance.HandleRabbitInterference();
                    
                    // Показываем диалог только при изменении задания
                    StartCoroutine(ShowTaskDialogueWithDelay(0.1f));
                }
                else
                {
                    Debug.Log("<color=cyan>Заяц решил не трогать задание</color>");
                    // Не показываем диалог, просто уходим
                    Leave();
                    return;
                }
            }
            else
            {
                Debug.Log("Задание уже изменено ранее");
                
                // Если задание уже изменено, с шансом 50% заяц подглядывает вместо ухода
                if (UnityEngine.Random.value < 0.5f)
                {
                    Debug.Log("<color=cyan>Bunny: Задание уже изменено ранее, заяц подглядывает</color>");
                    // Прерываем текущее поведение (если есть)
                    if (_currentBehavior != null)
                    {
                        StopCoroutine(_currentBehavior);
                    }
                    // Устанавливаем позицию для подглядывания
                    bool whichDoor = UnityEngine.Random.value < 0.5f;
                    if (whichDoor)
                    {
                        transform.position = _appearPoint_Door1.position;
                        transform.rotation = _appearPoint_Door1.rotation;
                    }
                    else
                    {
                        transform.position = _appearPoint_Door2.position;
                        transform.rotation = _appearPoint_Door2.rotation;
                    }
                    // Запускаем поведение подглядывания
                    _currentBehavior = StartCoroutine(PeekBehavior());
                    return; // Не продолжаем выполнение, т.к. уже запустили PeekBehavior
                }
                else
                {
                    Debug.Log("<color=cyan>Bunny: Задание уже изменено ранее, заяц просто уходит</color>");
                    // Просто уходим, не показывая диалог
                    Leave();
                    return;
                }
            }
        }
        
        private IEnumerator ShowTaskDialogueWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            ShowTaskDialogue();
        }
   
        private void ShowTaskDialogue()
        {
            if (_bunnyDialogueManager == null) 
            {
                Debug.LogError("<color=red>BunnyDialogueManager не найден!</color>");
                Leave();
                return;
            }
            
            // Получаем диалог с заданием
            Dialogue taskDialogue = _bunnyDialogueManager.GetTaskDialogueForBunny(this);
            
            if (taskDialogue == null || taskDialogue.sentences.Length == 0)
            {
                Debug.Log($"<color=red>Bunny: Не удалось получить описание задания. Заяц уходит.</color>");
                Leave(); 
                return;
            }
            
            Debug.Log($"<color=green>Bunny: Starting dialogue with: {taskDialogue.sentences[0]}</color>");
            
            // [!] ОБНОВЛЯЕМ UI перед показом диалога
            UpdateTaskUI();
            
            // Запускаем диалог
            _bunnyDialogueManager.StartBunnyDialogue(taskDialogue, this);
        }
        
        private void UpdateTaskUI()
        {
            // Используем TaskUIManager если есть, иначе ищем компоненты вручную
            if (UI.TaskUIManager.Instance != null)
            {
                UI.TaskUIManager.Instance.UpdateTaskUI();
            }
            else
            {
                // Ручное обновление
                UI.SimpleTaskTimer timer = FindAnyObjectByType<UI.SimpleTaskTimer>();
                UI.TaskDisplayUI display = FindAnyObjectByType<UI.TaskDisplayUI>();
                
                if (timer != null) timer.ForceStartTimer();
                if (display != null) display.ForceShowCurrentTask();
            }
        }
        
        private void TriggerChaosEffect()
        {
            // Подглядывание вызывает хаос
            Debug.Log("<color=yellow>Заяц подглядывает и вызывает хаос!</color>");
            
            // 1. Увеличить стресс
            if (GameCycle.Instance != null)
            {
                GameCycle.Instance.AddStress(4f);
            }

            // 2. Случайная проблема для игрока
            float randomEffect = UnityEngine.Random.value;
            
            if (randomEffect < 0.1f) // 10% шанс - инверсия управления
            {
                Debug.Log("<color=white>Хаос: Инверсия управления на 3 секунды!</color>");
                Player.MovementPlayer.invertControls = true;
                // Здесь можно вызвать инверсию управления у игрока
            }
            else if (randomEffect < 0.20f) // 10% шанс - замедление времени
            {
                Debug.Log("<color=white>Хаос: Временное замедление!</color>");
                // Замедлить время на 2 секунды
                Time.timeScale = 0.5f;
                Invoke(nameof(ResetTimeScale), 2f);
            }
            else if (randomEffect < 0.35f) // 15% шанс - тряска экрана
            {
                Debug.Log("<color=white>Хаос: Плывет экран!</color>");
                if (_cachedScreenShake != null)
                {
                    _cachedScreenShake.Start_shaking();
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayRandomChaosSound();
                    }
                    else
                    {
                        Debug.LogError("<color=red>Bunny: AudioManager.Instance не найден! Невозможно воспроизвести звук хаоса.</color>");
                    }
                    Debug.Log("<color=green>Вызван Screen_Shake</color>");
                }
                else
                {
                    Debug.LogError("<color=red>Screen_Shake не найден!</color>");
                }
            }
            else if (randomEffect < 0.50f) // 15% шанс - затемнение экрана
            {
                Debug.Log("<color=white>Хаос: экран потемнел!</color>");
                // Вызываем затемнение через ScreenFader
                StartCoroutine(QuickDarkenAndLighten());
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayRandomChaosSound();
                    }
                    else
                    {
                        Debug.LogError("<color=red>Bunny: AudioManager.Instance не найден! Невозможно воспроизвести звук хаоса.</color>");
                    }
            }
            else if (randomEffect < 0.65f) // 15% шанс - FireText
            {
                Debug.Log("<color=white>Хаос: FireText эффект!</color>");
                if (_cachedFireText != null)
                {
                    var currentTask = TaskSystem.TaskManager.Instance?.GetCurrentTask();
                    string taskText = currentTask != null ? currentTask.Description : "Current Task"; // Используйте Title или Description

                    // 2. Запускаем FireText, передавая исходный текст
                    _cachedFireText.Fire(taskText);
                    
                    Debug.Log($"<color=green>Хаос: FireText запущен для задачи: {taskText}!</color>");
                
                }
                else
                {
                    Debug.LogError("<color=red>Fire_text не найден!</color>");
                }
            }
            else if (randomEffect < 0.80f) // 15% шанс - мигание (ScreenFliskers)
            {
                Debug.Log("<color=white>Хаос: Мигание экрана!</color>");
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayRandomChaosSound();
                }
                else
                {
                    Debug.LogError("<color=red>Bunny: AudioManager.Instance не найден! Невозможно воспроизвести звук хаоса.</color>");
                }
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
            else if (randomEffect < 0.90f) // 10% шанс - Эффект сердцебиения
            {
                // [ИСПРАВЛЕНИЕ ОШИБКИ] Вызываем через StaticBlink с параметрами для "сердцебиения"
                // Предполагаем, что ScreenFadeManager умеет передавать параметры в ScreenBlinker.Blink(duration, count, color)
                if (Shaders.ScreenEffects.ScreenFadeManager.Instance != null)
                {
                    // Длительность 0.3, 2 пульса, цвет по умолчанию
                    Debug.Log("<color=green> Вызван Эффект серцебиения </color>");
                    Shaders.ScreenEffects.ScreenFadeManager.Instance.BlinkScreen(0.3f, 2, Color.red);
                    if (AudioManager.Instance == null)
                    {
                        Debug.LogError("<color=red>Bunny: AudioManager.Instance не найден!...</color>");
                    }
                    else
                    {
                        AudioManager.Instance.PlaySpecialSoundByIndex(0);
                    }
                    
                }
                else
                {
                      Debug.LogError("<color=red>ScreenFadeManager.Instance.BlinkScreen не найден!</color>");
                }
            }
            else // 10% шанс - звуковой эффект
            {
                Debug.Log("<color=white>Хаос: Случайный звуковой эффект!</color>");
                if (AudioManager.Instance == null)
                    {
                        Debug.LogError("<color=red>Bunny: AudioManager.Instance не найден! Невозможно воспроизвести звук хаоса.</color>");
                        return;
                    }
                    else
                    {
                        AudioManager.Instance.PlayRandomSpecialSFX();
                        Debug.Log("<color=green>! 10% шанс - звуковой эффект !</color>");
                    }
            }
            
            // [!] ДОПОЛНИТЕЛЬНЫЕ ЭФФЕКТЫ С ШАНСОМ 30%
            if (UnityEngine.Random.value < 0.3f) // 30% шанс на дополнительный эффект
            {
                Debug.Log("<color=white>Дополнительный эффект хаоса!</color>");
                float extraEffect = UnityEngine.Random.value;
                
                if (extraEffect < 0.5f) // 50% из 30% - ScreenShake
                {
                    ScreenShake extraShake = FindAnyObjectByType<ScreenShake>();
                    if (extraShake != null)
                    {
                        extraShake.Start_shaking();
                        Debug.Log("<color=white>Хаос: Дополнительная тряска экрана!</color>");
                    }
                }
                else // 50% из 30% - FireText
                {
                    Fire_text extraFireText = FindAnyObjectByType<Fire_text>();
                    if (extraFireText != null)
                    {
                        var currentTask = TaskSystem.TaskManager.Instance?.GetCurrentTask();
                        string taskText = currentTask != null ? currentTask.Description : "Current Task"; // Используйте Title или Description

                        // 2. Запускаем FireText, передавая исходный текст
                        _cachedFireText.Fire(taskText);
                        
                        Debug.Log($"<color=green>Хаос: FireText запущен для задачи: {taskText}!</color>");
                        Debug.Log("<color=white>Хаос: Дополнительный FireText!</color>");
                    }
                }
            }
        }

        // Обновленная корутина для затемнения экрана
        private IEnumerator QuickDarkenAndLighten()
        {
            if (ScreenFadeManager.Instance == null)
            {
                Debug.LogError("<color=red>Bunny: QuickDarkenAndLighten НЕ ЗАПУЩЕН! ScreenFadeManager.Instance == null. Убедитесь, что объект с ScreenFadeManager активен на сцене.</color>");
                yield break;
            }

            Debug.Log("<color=green>Bunny: Запуск QuickDarkenAndLighten (успешная проверка ScreenFadeManager).</color>");

            // Быстрое затемнение до черного (0.8f) за 0.2 секунды
            ScreenFadeManager.StaticFadeIn(0.2f);
            yield return new WaitForSeconds(0.3f);

            // Осветление обратно (0f) за 0.2 секунды
            ScreenFadeManager.StaticFadeOut(0.2f);
        }

        private void ResetTimeScale()
        {
            Time.timeScale = 1f;
        }
    
        // ========== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ==========
    
        private void SetVisible(bool visible)
        {
            _spriteRenderer.enabled = visible;
        
            // Можно включить/выключить все дочерние объекты
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(visible);
            }
        }
    
        private void Hide()
        {
            SetVisible(false);
        }

        private void OnTaskCompletedHandler(TaskSystem.BureaucraticTask task)
        {
            ChangeToNoTask();
        }

        public void ChangeToNoTask()
        {
            _isTaskPresent = false;
        }
    }
}
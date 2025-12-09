using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogueManager;
using System;
using Shaders;
using TaskSystem;
using Shaders.ScreenEffects;
using Unity.VisualScripting.FullSerializer;
namespace Bunny {
    public class Bunny : MonoBehaviour
    {
        public static Bunny Instance { get; private set; } // Добавил статический экземпляр
        [Header("Эффекты экрана")]
        // Кэшированные ссылки на эффекты
        private Screen_Shake _cachedScreenShake;
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
        //[SerializeField] private AudioClip _shoutSound;
        //[SerializeField] private AudioClip _peekSound;
        [SerializeField] private GameObject _chaosEffect; // Визуальный эффект хаоса
        //private Animator _animator;
        //private AudioSource _audioSource;
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
        public event Action OnRabbitActive;
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
            
            //_animator = GetComponent<Animator>();
            //_audioSource = GetComponent<AudioSource>();
            _spriteRenderer = GetComponent<SpriteRenderer>();

            _cachedScreenShake = FindObjectInScene<Screen_Shake>();
            _cachedFireText = FindObjectInScene<Fire_text>();
            _cachedScreenFliskers = FindObjectInScene<ScreenFliskers>();
            
            // Логирование найденных эффектов
            Debug.Log($"<color=red>Screen_Shake: {(_cachedScreenShake != null ? "найден" : "не найден</color>")}");
            Debug.Log($"<color=red>Fire_text: {(_cachedFireText != null ? "найден" : "не найден</color>")}");
            Debug.Log($"<color=red>ScreenFliskers: {(_cachedScreenFliskers != null ? "найден" : "не найден</color>")}");


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
        private T FindObjectInScene<T>() where T : MonoBehaviour
        {
            // Сначала ищем активные объекты
            T foundObject = FindAnyObjectByType<T>();
            if (foundObject != null)
                return foundObject;
            
            // Если не нашли, ищем среди всех объектов сцены
            T[] allObjects = Resources.FindObjectsOfTypeAll<T>();
            foreach (T obj in allObjects)
            {
                // Исключаем префабы и объекты из других сцен
                if (obj.gameObject.scene.IsValid() && !obj.gameObject.CompareTag("EditorOnly"))
                {
                    return obj;
                }
            }
            
            return null;
        }
        private IEnumerator SubscribeToTaskManagerEvents()
        {
            // Ждем пока TaskManager инициализируется
            while (TaskManager.Instance == null)
            {
                yield return new WaitForSeconds(0.1f);
                Debug.Log("<color=green>Bunny: Waiting for TaskManager to initialize...</color>");
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
            SetVisible(true);
        
            bool willPeek = _isTaskPresent ? UnityEngine.Random.value < _peekChance : false;
            Debug.Log($"<color=red>Шанс пик {willPeek}</color>");
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
            Debug.Log($"Заяц появился! Поведение: {(willPeek ? "Подглядывает" : "Кричит")}");
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

            Debug.Log("Заяц ушёл");
        }
        private IEnumerator ShoutBehavior()
        {
            // Анимация крика
            //if (_animator != null)
            //    _animator.SetTrigger("Shout");
            
            // Звук крика
            //if (_audioSource != null && _shoutSound != null)
            //    _audioSource.PlayOneShot(_shoutSound);
        
            // Ждём
            yield return new WaitForSeconds(_shoutDuration);
        
            // После крика - назначить новое задание или изменить текущее
            AssignOrModifyTask();
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
            
            // Звук подглядывания
            //if (_audioSource != null && _peekSound != null)
            //    _audioSource.PlayOneShot(_peekSound);
        
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

        // private void AssignOrModifyTask()
        // {
        //     Debug.Log($"Bunny: AssignOrModifyTask called. _bunnyDialogueManager is {(_bunnyDialogueManager == null ? "null" : "not null")}");
            
        //     // Ждем TaskManager, если он еще не инициализирован
        //     if (TaskManager.Instance == null) 
        //     {
        //         Debug.LogError("Bunny: TaskManager not found! Trying to find it...");
        //         TaskManager taskManager = FindAnyObjectByType<TaskManager>();
        //         if (taskManager == null)
        //         {
        //             Debug.LogError("Bunny: TaskManager not found in scene! Cannot assign task.");
        //             Leave();
        //             return;
        //         }
        //         else
        //         {
        //             Debug.Log("Bunny: Found TaskManager in scene");
        //         }
        //     }

        //     var currentTask = TaskManager.Instance.GetCurrentTask();
        //     Debug.Log($"Bunny: Current task is {(currentTask == null ? "null" : currentTask.Title)}");

        //     if (currentTask == null)
        //     {
        //         Debug.Log("Bunny: No current task, starting new task");
        //         TaskManager.Instance.StartNewTask();
        //         _isTaskPresent = true;
        //         currentTask = TaskManager.Instance.GetCurrentTask();
        //     }
        //     else if (!currentTask.IsCorrupted)
        //     {
        //         //! Портим задание с вероятностью 30 (было 50, 0.5f)%
        //         if (UnityEngine.Random.value > 0.7f)
        //         {
        //             Debug.Log("Bunny: Corrupting current task");
        //             TaskManager.Instance.HandleRabbitInterference();
        //         }
        //         else
        //         {
        //             Debug.Log("Заяц решил не трогать задание");
        //         }
        //     }
        //     else
        //     {
        //         Debug.Log("Задание уже изменено");
        //     }
            
        //     if (_bunnyDialogueManager == null) 
        //     {
        //         Debug.LogError("BunnyDialogueManager не найден!");
        //         return;
        //     }
            
        //     // Получаем диалог с заданием
        //     Dialogue taskDialogue = _bunnyDialogueManager.GetTaskDialogueForBunny(this);
            
        //     if (taskDialogue == null || taskDialogue.sentences.Length == 0)
        //     {
        //         Debug.Log($"Bunny: Не удалось получить описание задания. Заяц уходит.");
        //         Leave(); 
        //         return;
        //     }
            
        //     Debug.Log($"Bunny: Starting dialogue with: {taskDialogue.sentences[0]}");
            
        //     // Запускаем диалог
        //     _bunnyDialogueManager.StartBunnyDialogue(taskDialogue, this);
        // }
        //! Далее идет более прокачанная логика порчи задания.
        private void AssignOrModifyTask()
        {
            Debug.Log($"Bunny: AssignOrModifyTask called. _bunnyDialogueManager is {(_bunnyDialogueManager == null ? "null" : "not null")}");
            
            // Ждем TaskManager, если он еще не инициализирован
            if (TaskManager.Instance == null) 
            {
                Debug.LogError("Bunny: TaskManager not found! Trying to find it...");
                TaskManager taskManager = FindAnyObjectByType<TaskManager>();
                if (taskManager == null)
                {
                    Debug.LogError("Bunny: TaskManager not found in scene! Cannot assign task.");
                    Leave();
                    return;
                }
                else
                {
                    Debug.Log("Bunny: Found TaskManager in scene");
                }
            }

            var currentTask = TaskManager.Instance.GetCurrentTask();
            Debug.Log($"Bunny: Current task is {(currentTask == null ? "null" : currentTask.Title)}");

            if (_isTaskPresent == false)
            {
                Debug.Log("Bunny: No current task, starting new task");
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
                    Debug.Log("Bunny: Corrupting current task");
                    TaskManager.Instance.HandleRabbitInterference();
                    
                    // Показываем диалог только при изменении задания
                    StartCoroutine(ShowTaskDialogueWithDelay(0.1f));
                }
                else
                {
                    Debug.Log("Заяц решил не трогать задание");
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
                    Debug.Log("Bunny: Задание уже изменено ранее, заяц подглядывает");
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
                    Debug.Log("Bunny: Задание уже изменено ранее, заяц просто уходит");
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
   
        // private void ShowTaskDialogue()
        // {
        //     if (_bunnyDialogueManager == null) 
        //     {
        //         Debug.LogError("BunnyDialogueManager не найден!");
        //         Leave();
        //         return;
        //     }
            
        //     // Получаем диалог с заданием
        //     Dialogue taskDialogue = _bunnyDialogueManager.GetTaskDialogueForBunny(this);
            
        //     if (taskDialogue == null || taskDialogue.sentences.Length == 0)
        //     {
        //         Debug.Log($"Bunny: Не удалось получить описание задания. Заяц уходит.");
        //         Leave(); 
        //         return;
        //     }
            
        //     Debug.Log($"Bunny: Starting dialogue with: {taskDialogue.sentences[0]}");
            
        //     // Запускаем диалог
        //     _bunnyDialogueManager.StartBunnyDialogue(taskDialogue, this);
        // }
             //! Новый метод для показа диалога с заданием
        private void ShowTaskDialogue()
        {
            if (_bunnyDialogueManager == null) 
            {
                Debug.LogError("BunnyDialogueManager не найден!");
                Leave();
                return;
            }
            
            // Получаем диалог с заданием
            Dialogue taskDialogue = _bunnyDialogueManager.GetTaskDialogueForBunny(this);
            
            if (taskDialogue == null || taskDialogue.sentences.Length == 0)
            {
                Debug.Log($"Bunny: Не удалось получить описание задания. Заяц уходит.");
                Leave(); 
                return;
            }
            
            Debug.Log($"Bunny: Starting dialogue with: {taskDialogue.sentences[0]}");
            
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
            Debug.Log("Заяц подглядывает и вызывает хаос!");
            
            // 1. Увеличить стресс
            if (GameCycle.Instance != null)
            {
                GameCycle.Instance.AddStress(4f);
            }
            
            // 2. Случайная проблема для игрока
            float randomEffect = UnityEngine.Random.value;
            
            if (randomEffect < 0.10f) // 10% шанс - инверсия управления
            {
                Debug.Log("Хаос: Инверсия управления на 3 секунды!");
                // Здесь можно вызвать инверсию управления у игрока
                // Например: PlayerController.Instance.InvertControls(3f);
            }
            else if (randomEffect < 0.20f) // 10% шанс - замедление времени
            {
                Debug.Log("Хаос: Временное замедление!");
                // Замедлить время на 2 секунды
                Time.timeScale = 0.5f;
                Invoke(nameof(ResetTimeScale), 2f);
            }
            else if (randomEffect < 0.35f) // 15% шанс - тряска экрана
            {
                Debug.Log("Хаос: Плывет экран!");
                if (_cachedScreenShake != null)
                {
                    _cachedScreenShake.Start_shaking();
                }
            }
            else if (randomEffect < 0.50f) // 15% шанс - затемнение экрана
            {
                Debug.Log("Хаос: экран потемнел!");
                // Вызываем затемнение через ScreenFader
                StartCoroutine(QuickDarkenAndLighten());
            }
            else if (randomEffect < 0.65f) // 15% шанс - FireText
            {
                Debug.Log("Хаос: FireText эффект!");
                if (_cachedFireText != null)
                {
                    _cachedFireText.Fire();
                }
            }
            else if (randomEffect < 0.80f) // 15% шанс - мигание (ScreenFliskers)
            {
                Debug.Log("Хаос: Мигание экрана!");
                if (_cachedScreenFliskers != null)
                {
                    _cachedScreenFliskers.Start_flickers();
                }
            }
            else if (randomEffect < 0.90f) // 10% шанс - ScreenBlinker
            {
                Debug.Log("Хаос: Эффект сердцебиения!");
                // Используем ScreenBlinker с эффектом сердцебиения
                if (Shaders.ScreenEffects.ScreenBlinker.Instance != null)
                {
                    Shaders.ScreenEffects.ScreenBlinker.Instance.HeartbeatEffect(0.3f, 2, 0.15f);
                }
                else
                {
                    Debug.LogWarning("ScreenBlinker.Instance не найден!");
                }
            }
            else // 10% шанс - звуковой эффект
            {
                Debug.Log("Хаос: Случайный звуковой эффект!");
                // Воспроизвести странный звук
                // Например: AudioManager.Instance.PlayRandomChaosSound();
            }
            
            // [!] ДОПОЛНИТЕЛЬНЫЕ ЭФФЕКТЫ С ШАНСОМ 30%
            if (UnityEngine.Random.value < 0.3f) // 30% шанс на дополнительный эффект
            {
                Debug.Log("Дополнительный эффект хаоса!");
                float extraEffect = UnityEngine.Random.value;
                
                if (extraEffect < 0.5f) // 50% из 30% - ScreenShake
                {
                    Screen_Shake extraShake = FindAnyObjectByType<Screen_Shake>();
                    if (extraShake != null)
                    {
                        extraShake.Start_shaking();
                        Debug.Log("Хаос: Дополнительная тряска экрана!");
                    }
                }
                else // 50% из 30% - FireText
                {
                    Fire_text extraFireText = FindAnyObjectByType<Fire_text>();
                    if (extraFireText != null)
                    {
                        extraFireText.Fire();
                        Debug.Log("Хаос: Дополнительный FireText!");
                    }
                }
            }
        }

        // Обновленная корутина для затемнения экрана
        private IEnumerator QuickDarkenAndLighten()
        {
            if (Shaders.ScreenEffects.ScreenFader.Instance == null) yield break;
            
            // Быстрое затемнение до черного (0.2 секунды)
            Shaders.ScreenEffects.ScreenFader.Instance.StartFade(0.8f, 0.2f);
            yield return new WaitForSeconds(0.3f);
            
            // Осветление обратно (0.2 секунды)
            Shaders.ScreenEffects.ScreenFader.Instance.StartFade(0f, 0.2f);
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
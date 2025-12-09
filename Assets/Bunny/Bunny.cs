using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogueManager;
using System;
using Shaders;
using TaskSystem;
using Shaders.ScreenEffects;
namespace Bunny {
    public class Bunny : MonoBehaviour
    {
        public static Bunny Instance { get; private set; } // Добавил статический экземпляр

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

            SetVisible(false);
            if (_appearPoint_Window != null)
            {
                transform.position = _appearPoint_Window.position;
                transform.rotation = _appearPoint_Window.rotation;
            }

            _bunnyDialogueManager = FindAnyObjectByType<BunnyDialogueManager>();
            if (_bunnyDialogueManager == null)
            {
                Debug.LogError("BunnyDialogueManager не найден в сцене!");
            }
        }
        private IEnumerator SubscribeToTaskManagerEvents()
        {
            // Ждем пока TaskManager инициализируется
            while (TaskManager.Instance == null)
            {
                yield return new WaitForSeconds(0.1f);
                Debug.Log("Bunny: Waiting for TaskManager to initialize...");
            }
            
            Debug.Log("Bunny: TaskManager found, subscribing to events");
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

            if (currentTask == null)
            {
                Debug.Log("Bunny: No current task, starting new task");
                TaskManager.Instance.StartNewTask();
                _isTaskPresent = true;
                currentTask = TaskManager.Instance.GetCurrentTask();
                
                // Показываем диалог только при создании нового задания
                ShowTaskDialogue();
            }
            else if (!currentTask.IsCorrupted)
            {
                // Портим задание с вероятностью 30%
                if (UnityEngine.Random.value > 0.7f)  // 30% chance
                {
                    Debug.Log("Bunny: Corrupting current task");
                    TaskManager.Instance.HandleRabbitInterference();
                    
                    // Показываем диалог только при изменении задания
                    ShowTaskDialogue();
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
            
            // Запускаем диалог
            _bunnyDialogueManager.StartBunnyDialogue(taskDialogue, this);
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
        
            if (randomEffect < 0.15f) //! было 0.2f
            {
                Debug.Log("Хаос: Инверсия управления на 3 секунды!");
                // Здесь можно вызвать инверсию управления у игрока
            }
            else if (randomEffect < 0.3f) //! было 0.4
            {
                Debug.Log("Хаос: Временное замедление!");
                // Замедлить время на 2 секунды
                Time.timeScale = 0.5f;
                Invoke(nameof(ResetTimeScale), 2f);
            }
            else if (randomEffect < 0.45f) //! было 0.6
            {
                Debug.Log("Хаос: Плывет экран!");
            }
            else if (randomEffect < 0.85f) //! было 0.8 (now 40%)
            {
                if (Shaders.ScreenEffects.ScreenBlinker.Instance != null)
                {
                    // Черный цвет, 0.3 секунды на фазу, 2 мигания
                    //Shaders.ScreenEffects.ScreenBlinker.Instance.Blink(0.3f, 2, Color.black);
                    StartCoroutine(QuickDarkenAndLighten()); //! ПРОБЛЕМА НЕ РЕШЕНА
                    Debug.Log("эффект хаоса был запущен успешно");
                }
                else
                {
                    Debug.Log("Не получилось включить затемнение");
                }
                Debug.Log("Хаос: экран потемнел!");

            }
            else //! Шанс 15%
            {
                Debug.Log("Хаос: Случайный звуковой эффект!");
                // Воспроизвести странный звук
            }
        }
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
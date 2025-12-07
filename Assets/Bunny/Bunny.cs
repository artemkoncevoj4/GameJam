using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogueManager;
using System;
using Shaders;
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
        [SerializeField] private float _peekChance = 0.6f; // Шанс подглядывания вместо крика
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

            _bunnyDialogueManager = FindObjectOfType<BunnyDialogueManager>();
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
                TaskManager.Instance.OnTaskCompleted -= OnTaskCompletedHandler; // Отписка
            }
        }

        public void Appear()    
        {
            if (_isActive) return;
        
            _isActive = true;
            SetVisible(true);
        
            bool willPeek = _isTaskPresent ? UnityEngine.Random.value < _peekChance : false;
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

        private void AssignOrModifyTask()
        {
            Debug.Log($"Bunny: AssignOrModifyTask called. _bunnyDialogueManager is {(_bunnyDialogueManager == null ? "null" : "not null")}");
            
            // Ждем TaskManager, если он еще не инициализирован
            if (TaskManager.Instance == null) 
            {
                Debug.LogError("Bunny: TaskManager not found! Trying to find it...");
                TaskManager taskManager = FindObjectOfType<TaskManager>();
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
            }
            else if (!currentTask.IsCorrupted)
            {
                // Портим задание с вероятностью 50%
                if (UnityEngine.Random.value > 0.5f)
                {
                    Debug.Log("Bunny: Corrupting current task");
                    TaskManager.Instance.HandleRabbitInterference();
                }
                else
                {
                    Debug.Log("Заяц решил не трогать задание");
                }
            }
            else
            {
                Debug.Log("Задание уже изменено");
            }
            
            if (_bunnyDialogueManager == null) 
            {
                Debug.LogError("BunnyDialogueManager не найден!");
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
        
            if (randomEffect < 0.2f)
            {
                Debug.Log("Хаос: Инверсия управления на 3 секунды!");
                // Здесь можно вызвать инверсию управления у игрока
            }
            else if (randomEffect < 0.4f)
            {
                Debug.Log("Хаос: Временное замедление!");
                // Замедлить время на 2 секунды
                Time.timeScale = 0.5f;
                Invoke(nameof(ResetTimeScale), 2f);
            }
            else if (randomEffect < 0.6f)
            {
                Debug.Log("Хаос: Плывет экран!");
            }
            else if (randomEffect < 0.8f)
            {
                ScreenFader screenFader = FindObjectOfType<ScreenFader>();
                if (screenFader != null)
                {
                    StartCoroutine(screenFader.FadeScreenCoroutine());
                }
                else
                {
                    Debug.LogWarning("ScreenFader не найден в сцене!");
                }
            }
            else
            {
                Debug.Log("Хаос: Случайный звуковой эффект!");
                // Воспроизвести странный звук
            }
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
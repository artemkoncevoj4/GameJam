using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogueManager;
using System;
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
                GameCycle.Instance.OnRabbitAppearing += Appear; // Подписал на события GameCycle
                GameCycle.Instance.OnRabbitLeaving += Leave;
            }
            if (TaskManager.Instance != null)
            {
                TaskManager.Instance.OnTaskCompleted += OnTaskCompletedHandler; // Изменил имя метода
            }
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
            if (TaskManager.Instance == null) return;

            var currentTask = TaskManager.Instance.GetCurrentTask();

            if (currentTask == null)
            {
                TaskManager.Instance.StartNewTask();
                _isTaskPresent = true;
            }
            else if (!currentTask.IsCorrupted)
            {
                OnRabbitActive?.Invoke();
            }
            else
            {
                Debug.Log("Задание уже изменено");
            }
            if (_bunnyDialogueManager == null) return; // Проверка на Null
        
            string sentenceToStart = string.Empty;
        
            if (TestTaskManager.Instance != null)
            {
                // 1. [КЛЮЧЕВОЙ МОМЕНТ] Вызываем функцию у TaskManager для получения строки.
                sentenceToStart = TaskManager.Instance.GetTaskDescription();
            }

            // 2. Проверяем, есть ли что говорить.
            if (string.IsNullOrEmpty(sentenceToStart))
            {
                Debug.Log($"Bunny: Все реплики диалога исчерпаны (индекс {_currentDialogueIndex}). Заяц уходит.");
                Leave(); 
                return;
            }

            // 3. Создаем временный объект Dialogue только с одной текущей строкой.
            Dialogue singleSentenceDialogue = new Dialogue
            {
                name = BunnyName, // Используем имя Зайца из старого объекта Dialogue
                sentences = new string[] { sentenceToStart }
            };
        
            // 4. Запускаем диалог
            _bunnyDialogueManager.StartBunnyDialogue(singleSentenceDialogue, this);
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
        
            if (randomEffect < 0.25f)
            {
                Debug.Log("Хаос: Инверсия управления на 3 секунды!");
                // Здесь можно вызвать инверсию управления у игрока
            }
            else if (randomEffect < 0.5f)
            {
                Debug.Log("Хаос: Временное замедление!");
                // Замедлить время на 2 секунды
                Time.timeScale = 0.5f;
                Invoke(nameof(ResetTimeScale), 2f);
            }
            else if (randomEffect < 0.75f)
            {
                Debug.Log("Хаос: Плывет экран!");
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
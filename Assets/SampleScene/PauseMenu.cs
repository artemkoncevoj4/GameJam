using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections; // Добавлено для работы с корутинами, если будете их использовать

namespace SampleScene {
    public class PauseMenu : MonoBehaviour
    {
        public static PauseMenu Instance { get; private set; }

        [Header("UI элементы")]
        [SerializeField] private GameObject _pauseMenuPanel;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _mainMenuButton;

        [Header("Настройки")]
        [SerializeField] private string _mainMenuSceneName = "MainMenu";
        [SerializeField] private float _pauseBackgroundAlpha = 0.7f;

        private bool _isPaused = false;
        private Image _backgroundImage;
        [Header("Анимация")]
        [SerializeField] private PauseMenuAnimator _menuAnimator;
        [SerializeField] private float _animationDelay = 0.3f;
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            if (_menuAnimator == null)
            _menuAnimator = GetComponentInChildren<PauseMenuAnimator>();
        }

        void Start()
        {
            InitializeUI();
            SetupEventListeners();
            
            // Скрываем меню при старте
            if (_pauseMenuPanel != null)
                _pauseMenuPanel.SetActive(false);
        }

        private void InitializeUI()
        {
            // Находим или создаем необходимые компоненты
            if (_pauseMenuPanel != null)
            {
                // Настраиваем полупрозрачный фон
                _backgroundImage = _pauseMenuPanel.GetComponent<Image>();
                if (_backgroundImage == null)
                {
                    _backgroundImage = _pauseMenuPanel.AddComponent<Image>();
                }
                
                Color bgColor = Color.black;
                bgColor.a = _pauseBackgroundAlpha;
                _backgroundImage.color = bgColor;
                
                // Убедимся, что панель не блокирует клики на кнопки
                _backgroundImage.raycastTarget = true;
            }
        }

        private void SetupEventListeners()
        {
            if (_resumeButton != null)
                _resumeButton.onClick.AddListener(ResumeGame);
            
            if (_mainMenuButton != null)
                _mainMenuButton.onClick.AddListener(GoToMainMenu);
            
            // Подписываемся на события GameCycle для синхронизации
            // (Предполагается, что GameCycle.Instance существует)
            if (GameCycle.Instance != null)
            {
                GameCycle.Instance.OnGameEnded += OnGameEnded;
            }
        }

        public void TogglePause()
        {
            Debug.Log("TogglePause called, current state: " + _isPaused);
            
            if (_isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        // --- ИСПРАВЛЕННЫЙ МЕТОД PAUSEGAME ---
        public void PauseGame()
        {
            if (_isPaused) return;
            
            _isPaused = true;
            Time.timeScale = 0f;
            
            if (InputHandler.Instance != null)
                InputHandler.Instance.DisableInput();
            
            if (GameCycle.Instance != null)
                GameCycle.Instance.PauseGame();
            
            // КРИТИЧЕСКОЕ ИСПРАВЛЕНИЕ: 
            // Сначала включаем GameObject, чтобы Корутины могли запуститься!
            if (_pauseMenuPanel != null)
                _pauseMenuPanel.SetActive(true);
            
            // Теперь вызываем анимацию на АКТИВНОМ объекте
            if (_menuAnimator != null)
                _menuAnimator.ShowMenu();
        }

        public void ResumeGame()
        {
            if (!_isPaused) return;
            
            // Скрываем с анимацией
            if (_menuAnimator != null)
            {
                _menuAnimator.HideMenu();
                // Запускаем CompleteResume через задержку, чтобы анимация успела отработать
                Invoke(nameof(CompleteResume), _animationDelay);
            }
            else
            {
                // Если аниматора нет, выполняем сразу
                CompleteResume();
            }
        }
        
        // --- ИСПРАВЛЕННЫЙ МЕТОД COMPLETERESUME ---
        private void CompleteResume()
        {
            _isPaused = false;
            Time.timeScale = 1f;
            
            if (InputHandler.Instance != null)
                InputHandler.Instance.EnableInput();
            
            if (GameCycle.Instance != null)
                GameCycle.Instance.ResumeGame();
            
            // КРИТИЧЕСКОЕ ИСПРАВЛЕНИЕ: 
            // Панель должна быть выключена всегда, когда меню скрыто, 
            // независимо от того, использовалась анимация или нет.
            // (Эта функция вызывается с задержкой, позволяя анимации завершиться)
            if (_pauseMenuPanel != null)
                _pauseMenuPanel.SetActive(false);
        }
        
        private void GoToMainMenu()
        {
            Debug.Log("Going to main menu...");
            
            // Восстанавливаем нормальную скорость времени
            Time.timeScale = 1f;
            
            // Снимаем паузу перед выходом
            ResumeGame();
            
            // Загружаем главное меню
            SceneManager.LoadScene(_mainMenuSceneName);
            
            Debug.Log("Переход в главное меню");
        }

        private void OnGameEnded(GameCycle.GameResult result)
        {
            // Если игра закончилась, отключаем меню паузы
            _isPaused = false;
            if (_pauseMenuPanel != null)
                _pauseMenuPanel.SetActive(false);
        }

        void OnDestroy()
        {
            // Отписываемся от событий
            if (_resumeButton != null)
                _resumeButton.onClick.RemoveListener(ResumeGame);
            
            if (_mainMenuButton != null)
                _mainMenuButton.onClick.RemoveListener(GoToMainMenu);
            
            // (Предполагается, что GameCycle.Instance существует)
            if (GameCycle.Instance != null)
            {
                GameCycle.Instance.OnGameEnded -= OnGameEnded;
            }
            
            // Восстанавливаем нормальное время при уничтожении
            Time.timeScale = 1f;
        }

        public bool IsPaused => _isPaused;
    }
}
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

namespace SampleScene {
    public class PauseMenu : MonoBehaviour
    {
        public static PauseMenu Instance { get; private set; }

        [Header("UI элементы")]
        [SerializeField] private GameObject _pauseMenuPanel;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private Button _restartButton;

        [Header("Настройки")]
        [SerializeField] private string _mainMenuSceneName = "MainMenu";
        [SerializeField] private float _pauseBackgroundAlpha = 0.7f;
        
        private string _currentSceneName;
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
            
            _currentSceneName = SceneManager.GetActiveScene().name;
            
            Debug.Log("PauseMenu Awake. Scene: " + _currentSceneName);
        }

        void Start()
        {
            InitializeUI();
            SetupEventListeners();
            
            if (_pauseMenuPanel != null)
                _pauseMenuPanel.SetActive(false);
            
            Debug.Log("PauseMenu Start. Buttons initialized.");
        }

        private void InitializeUI()
        {
            if (_pauseMenuPanel != null)
            {
                _backgroundImage = _pauseMenuPanel.GetComponent<Image>();
                if (_backgroundImage == null)
                {
                    _backgroundImage = _pauseMenuPanel.AddComponent<Image>();
                }
                
                Color bgColor = Color.black;
                bgColor.a = _pauseBackgroundAlpha;
                _backgroundImage.color = bgColor;
                _backgroundImage.raycastTarget = true;
            }
        }

        private void SetupEventListeners()
        {
            Debug.Log("Setting up button listeners...");
            
            if (_resumeButton != null)
            {
                _resumeButton.onClick.AddListener(ResumeGame);
                Debug.Log("Resume button listener added.");
            }
            else
            {
                Debug.LogError("Resume button is not assigned!");
            }
            
            if (_mainMenuButton != null)
            {
                _mainMenuButton.onClick.AddListener(GoToMainMenu);
                Debug.Log("Main Menu button listener added.");
            }
            else
            {
                Debug.LogError("Main Menu button is not assigned!");
            }
            
            if (_restartButton != null)
            {
                _restartButton.onClick.AddListener(RestartGame);
                Debug.Log("Restart button listener added.");
            }
            else
            {
                Debug.LogError("Restart button is not assigned!");
            }
            
            if (GameCycle.Instance != null)
            {
                GameCycle.Instance.OnGameEnded += OnGameEnded;
                Debug.Log("Subscribed to GameEnded event.");
            }
            else
            {
                Debug.LogError("GameCycle instance not found!");
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

        public void PauseGame()
        {
            if (_isPaused) return;
            
            _isPaused = true;
            
            // СНАЧАЛА ставим игру на паузу в GameCycle
            if (GameCycle.Instance != null)
                GameCycle.Instance.PauseGame();
            
            // Затем выключаем инпуты и показываем меню
            if (InputHandler.Instance != null)
                InputHandler.Instance.DisableInput();
            
            if (_pauseMenuPanel != null)
                _pauseMenuPanel.SetActive(true);
            
            if (_menuAnimator != null)
                _menuAnimator.ShowMenu();
        }

        public void ResumeGame()
        {
            if (!_isPaused) return;
            
            // Сразу снимаем флаг паузы
            _isPaused = false;
            
            // Сразу снимаем паузу в GameCycle
            if (GameCycle.Instance != null)
                GameCycle.Instance.ResumeGame();
            
            // Сразу восстанавливаем инпуты
            if (InputHandler.Instance != null)
                InputHandler.Instance.EnableInput();
            
            // Скрываем меню с анимацией
            if (_menuAnimator != null)
            {
                _menuAnimator.HideMenu();
                Invoke(nameof(DisableMenuPanel), _animationDelay);
            }
            else
            {
                DisableMenuPanel();
            }
        }
        
        private void DisableMenuPanel()
        {
            if (_pauseMenuPanel != null)
                _pauseMenuPanel.SetActive(false);
        }
        
        private void RestartGame()
        {
            Debug.Log("=== RESTART GAME CALLED ===");
            Debug.Log("Current scene: " + _currentSceneName);
            Debug.Log("Current timeScale: " + Time.timeScale);
            Debug.Log("IsPaused: " + _isPaused);
            
            // Восстанавливаем нормальную скорость времени
            Time.timeScale = 1f;
            Debug.Log("Time.timeScale set to 1");
            
            // Снимаем все флаги паузы
            _isPaused = false;
            
            // Отключаем меню, если оно активно
            if (_pauseMenuPanel != null && _pauseMenuPanel.activeSelf)
            {
                _pauseMenuPanel.SetActive(false);
                Debug.Log("Pause menu panel disabled.");
            }
            
            // Сбрасываем состояние GameCycle
            if (GameCycle.Instance != null)
            {
                // Если GameCycle имеет метод ResetGame, вызываем его
                Debug.Log("GameCycle instance found. Starting new game...");
                GameCycle.Instance.StartGame();
            }
            
            // Получаем текущую сцену по индексу (более надежно)
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            Debug.Log("Loading scene with index: " + currentSceneIndex);
            
            // Загружаем сцену с опцией для сброса всех объектов
            SceneManager.LoadScene(currentSceneIndex, LoadSceneMode.Single);
            
            Debug.Log("Scene load initiated.");
        }

        private void GoToMainMenu()
        {
            Debug.Log("Going to main menu...");
            
            // Восстанавливаем нормальную скорость времени
            Time.timeScale = 1f;
            _isPaused = false;
            
            // Загружаем главное меню
            SceneManager.LoadScene(_mainMenuSceneName);
        }

        private void OnGameEnded(GameCycle.GameResult result)
        {
            _isPaused = false;
            if (_pauseMenuPanel != null)
                _pauseMenuPanel.SetActive(false);
        }

        void OnDestroy()
        {
            if (_resumeButton != null)
                _resumeButton.onClick.RemoveListener(ResumeGame);
            
            if (_mainMenuButton != null)
                _mainMenuButton.onClick.RemoveListener(GoToMainMenu);
            
            if (_restartButton != null)
                _restartButton.onClick.RemoveListener(RestartGame);
            
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
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using Shaders.ScreenEffects;
using UI;
namespace SampleScene {
    public class PauseMenu : MonoBehaviour
    {
        public static PauseMenu Instance { get; private set; }

        [Header("UI элементы")]
        [SerializeField] private GameObject _pauseMenuPanel;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private Button _restartButton;
        
        [Header("Тексты")]
        [SerializeField] private GameObject _pauseText; // Текст "PAUSE"
        [SerializeField] private GameObject _gameOverText; // Текст "GAME OVER"
        [SerializeField] private GameObject _victoryText; // Текст "VICTORY"

        [Header("Настройки")]
        [SerializeField] private string _mainMenuSceneName = "MainMenu";
        [SerializeField] private float _pauseBackgroundAlpha = 0.7f;
        [SerializeField] private float _gameOverBackgroundAlpha = 0.85f; // Более темный фон для Game Over
        private string _currentSceneName;
        private bool _isPaused = false;
        private bool _isGameOver = false;
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
            
            // Скрываем все тексты по умолчанию
            HideAllTexts();
        }

        void Start()
        {
            InitializeUI();
            SetupEventListeners();
            if (_pauseMenuPanel != null)
                _pauseMenuPanel.SetActive(false);
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
                
                // Начальный цвет - стандартный черный с прозрачностью
                Color bgColor = Color.black;
                bgColor.a = _pauseBackgroundAlpha;
                _backgroundImage.color = bgColor;
                _backgroundImage.raycastTarget = true;
            }
        }

        private void HideAllTexts()
        {
            if (_pauseText != null) 
            {
                _pauseText.SetActive(false);
                Debug.Log("Pause text hidden");
            }
            if (_gameOverText != null) 
            {
                _gameOverText.SetActive(false);
                Debug.Log("Game Over text hidden");
            }
            if (_victoryText != null) 
            {
                _victoryText.SetActive(false);
                Debug.Log("Victory text hidden");
            }
        }

        private void SetupEventListeners()
        {
            if (_resumeButton != null)
                _resumeButton.onClick.AddListener(ResumeGame);
            
            if (_mainMenuButton != null)
                _mainMenuButton.onClick.AddListener(GoToMainMenu);
            
            if (_restartButton != null)
                _restartButton.onClick.AddListener(RestartGame);
            
            if (GameCycle.Instance != null)
            {
                GameCycle.Instance.OnGameEnded += OnGameEnded;
            }
        }

        public void TogglePause()
        {
            if (_isGameOver) return;
            
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
            if (_isPaused || _isGameOver) return;
            
            Debug.Log("<color=cyan>PauseGame called</color>");
            _isPaused = true;
            // Ставим игру на паузу в GameCycle
            if (GameCycle.Instance != null)
                GameCycle.Instance.PauseGame();
            
            // Выключаем инпуты
            if (InputHandler.Instance != null)
                InputHandler.Instance.DisableInput();
            
            // Включаем панель
            if (_pauseMenuPanel != null)
                _pauseMenuPanel.SetActive(true);
            
            // Показываем только текст паузы
            HideAllTexts();
            if (_pauseText != null) 
            {
                _pauseText.SetActive(true);
                Debug.Log("Pause text shown");
            }
            
            // Все кнопки активны
            if (_resumeButton != null) _resumeButton.gameObject.SetActive(true);
            if (_mainMenuButton != null) _mainMenuButton.gameObject.SetActive(true);
            if (_restartButton != null) _restartButton.gameObject.SetActive(true);
            
            // Стандартный цвет фона
            if (_backgroundImage != null)
            {
                Color bgColor = Color.black;
                bgColor.a = _pauseBackgroundAlpha;
                _backgroundImage.color = bgColor;
            }
            
            // Запускаем анимацию
            if (_menuAnimator != null)
                _menuAnimator.ShowMenu();
        }

        /// <summary>
        /// Показать меню поражения
        /// </summary>
        public void ShowGameOverMenu()
        {
            Debug.Log("<color=cyan>ShowGameOverMenu called</color>");
            _isPaused = true;
            _isGameOver = true;
            
            // Включаем панель
            if (_pauseMenuPanel != null)
                _pauseMenuPanel.SetActive(true);
            
            // Показываем только текст Game Over
            HideAllTexts();
            if (_gameOverText != null) 
            {
                _gameOverText.SetActive(true);
                Debug.Log("Game Over text shown");
            }
            
            // Скрываем кнопку Resume, остальные активны
            if (_resumeButton != null) _resumeButton.gameObject.SetActive(false);
            if (_mainMenuButton != null) _mainMenuButton.gameObject.SetActive(true);
            if (_restartButton != null) _restartButton.gameObject.SetActive(true);
            
            // Темный фон с красным оттенком
            if (_backgroundImage != null)
            {
                Color bgColor = new Color(0.15f, 0f, 0f, _gameOverBackgroundAlpha);
                _backgroundImage.color = bgColor;
            }
            
            // Запускаем анимацию
            if (_menuAnimator != null)
                _menuAnimator.ShowMenu();
        }

        /// <summary>
        /// Показать меню победы
        /// </summary>
        public void ShowVictoryMenu()
        {
            Debug.Log("<color=cyan>ShowVictoryMenu called</color>");
            _isPaused = true;
            _isGameOver = true;
            
            if (_pauseMenuPanel != null)
                _pauseMenuPanel.SetActive(true);
            
            HideAllTexts();
            if (_victoryText != null) 
            {
                _victoryText.SetActive(true);
                Debug.Log("Victory text shown");
            }
            
            // Скрываем кнопку Resume, остальные активны
            if (_resumeButton != null) _resumeButton.gameObject.SetActive(false);
            if (_mainMenuButton != null) _mainMenuButton.gameObject.SetActive(true);
            if (_restartButton != null) _restartButton.gameObject.SetActive(true);
            
            // Темный фон с золотым оттенком
            if (_backgroundImage != null)
            {
                Color bgColor = new Color(0.2f, 0.15f, 0f, _gameOverBackgroundAlpha);
                _backgroundImage.color = bgColor;
            }
            
            if (_menuAnimator != null)
                _menuAnimator.ShowMenu();
        }

        public void ResumeGame()
        {
            if (!_isPaused || _isGameOver) return;
            
            Debug.Log("<color=cyan>ResumeGame called</color>");
            
            // Снимаем флаг паузы
            _isPaused = false;
            
            // Снимаем паузу в GameCycle
            if (GameCycle.Instance != null)
                GameCycle.Instance.ResumeGame();
            
            // Восстанавливаем инпуты
            if (InputHandler.Instance != null)
                InputHandler.Instance.EnableInput();
            
            // Скрываем текст паузы ПЕРЕД анимацией
            HideAllTexts();
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
            Debug.Log("<color=cyan>DisableMenuPanel called</color>");
            
            // Скрываем панель
            if (_pauseMenuPanel != null)
                _pauseMenuPanel.SetActive(false);
            
            // Гарантируем, что все тексты скрыты
            HideAllTexts();
            
            // Сбрасываем флаги
            _isPaused = false;
            _isGameOver = false;
            
            // Восстанавливаем стандартный цвет фона
            if (_backgroundImage != null)
            {
                Color bgColor = Color.black;
                bgColor.a = _pauseBackgroundAlpha;
                _backgroundImage.color = bgColor;
            }
            
            // Восстанавливаем все кнопки
            if (_resumeButton != null) _resumeButton.gameObject.SetActive(true);
            if (_mainMenuButton != null) _mainMenuButton.gameObject.SetActive(true);
            if (_restartButton != null) _restartButton.gameObject.SetActive(true);
            
            Debug.Log("Menu panel disabled and reset");
        }
        
        private void RestartGame()
        {
            Debug.Log("<color=yellow>=== RESTART GAME ===</color>");
            
            // Восстанавливаем нормальную скорость времени
            Time.timeScale = 1f;
            CleanUpBeforeSceneChange();
            // Загружаем текущую сцену заново
            SceneManager.LoadScene(_currentSceneName);
        }

        private void GoToMainMenu()
        {
            Debug.Log("<color=yellow>Going to main menu...</color>");
            
            // Восстанавливаем нормальную скорость времени
            Time.timeScale = 1f;
            CleanUpBeforeSceneChange();
            // Загружаем главное меню
            SceneManager.LoadScene(0);
        }
        private void CleanUpBeforeSceneChange()
        {
            Debug.Log("Cleaning up before scene change...");
            
            // Уничтожаем синглтоны, которые могут сохранять состояние
            if (TaskSystem.TaskManager.Instance != null)
            {
                Destroy(TaskSystem.TaskManager.Instance.gameObject);
            }
            
            if (GameCycle.Instance != null)
            {
                Destroy(GameCycle.Instance.gameObject);
            }
            
            if (Bunny.Bunny.Instance != null)
            {
                Destroy(Bunny.Bunny.Instance.gameObject);
            }
            
            if (Player.PlayerInventory.Instance != null)
            {
                Destroy(Player.PlayerInventory.Instance.gameObject);
            }
            
            //TODO Можно добавить очистку других синглтонов при необходимости
        }
        private void OnGameEnded(GameCycle.GameResult result)
        {
            Debug.Log($"PauseMenu: Game ended with result {result}");
            // Не обрабатываем здесь, так как мы показываем меню в GameCycle
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
        public bool IsGameOver => _isGameOver;
    }
}
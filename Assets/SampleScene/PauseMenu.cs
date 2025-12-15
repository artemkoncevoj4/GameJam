using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace SampleScene {
    // Муштаков А.Ю.

    //! Фактически сгенерировано ИИ с небольшими изменениями

    /// <summary>
    /// Управляет меню паузы, поражения и победы в игре.
    /// Реализует паттерн Singleton для глобального доступа к функционалу меню.
    /// Обрабатывает отображение различных состояний игры (пауза, поражение, победа) и предоставляет навигацию по меню.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        /// <summary>
        /// Статический экземпляр для реализации паттерна Singleton.
        /// Обеспечивает глобальный доступ к меню паузы из любых систем игры.
        /// </summary>
        public static PauseMenu Instance { get; private set; }

        [Header("UI элементы")]
        [SerializeField] private GameObject _pauseMenuPanel; // Основная панель меню паузы
        [SerializeField] private Button _resumeButton; // Кнопка возобновления игры
        [SerializeField] private Button _mainMenuButton; // Кнопка перехода в главное меню
        [SerializeField] private Button _restartButton; // Кнопка перезапуска уровня
        
        [Header("Тексты")]
        [SerializeField] private GameObject _pauseText; // Текстовый элемент "PAUSE"
        [SerializeField] private GameObject _gameOverText; // Текстовый элемент "GAME OVER"
        [SerializeField] private GameObject _victoryText; // Текстовый элемент "VICTORY"

        [Header("Настройки")]
        [SerializeField] private float _pauseBackgroundAlpha = 0.7f; // Прозрачность фона при паузе
        [SerializeField] private float _gameOverBackgroundAlpha = 0.85f; // Более темный фон для Game Over
        
        private string _currentSceneName; // Имя текущей сцены для перезагрузки
        private bool _isPaused = false; // Флаг состояния паузы
        private bool _isGameOver = false; // Флаг завершения игры (поражение или победа)
        private Image _backgroundImage; // Компонент Image для фона меню
        
        [Header("Анимация")]
        [SerializeField] private PauseMenuAnimator _menuAnimator; // Компонент анимации меню
        [SerializeField] private float _animationDelay = 0.3f; // Задержка перед отключением панели после анимации
        
        /// <summary>
        /// Инициализирует Singleton и компоненты меню при создании объекта.
        /// </summary>
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
            
            // Скрываем все текстовые элементы по умолчанию
            HideAllTexts();
        }

        /// <summary>
        /// Настраивает пользовательский интерфейс и подписывается на события при запуске.
        /// </summary>
        void Start()
        {
            InitializeUI();
            SetupEventListeners();
            if (_pauseMenuPanel != null)
                _pauseMenuPanel.SetActive(false);
        }

        /// <summary>
        /// Инициализирует компоненты пользовательского интерфейса, включая фон меню.
        /// </summary>
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

        /// <summary>
        /// Скрывает все текстовые элементы меню (пауза, поражение, победа).
        /// </summary>
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

        /// <summary>
        /// Настраивает обработчики событий для кнопок меню и подписывается на события GameCycle.
        /// </summary>
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

        /// <summary>
        /// Переключает состояние паузы игры.
        /// Если игра на паузе - возобновляет, если активна - ставит на паузу.
        /// Не действует, если игра завершена (поражение/победа).
        /// </summary>
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

        /// <summary>
        /// Ставит игру на паузу и отображает меню паузы.
        /// Приостанавливает игровое время, отключает ввод и показывает соответствующий интерфейс.
        /// </summary>
        public void PauseGame()
        {
            if (_isPaused || _isGameOver) return;
            
            Debug.Log("<color=cyan>PauseGame called</color>");
            _isPaused = true;
            if (GameCycle.Instance != null)
                GameCycle.Instance.PauseGame();
            
            if (InputHandler.Instance != null)
                InputHandler.Instance.DisableInput();
            
            if (_pauseMenuPanel != null)
                _pauseMenuPanel.SetActive(true);
            
            HideAllTexts();
            if (_pauseText != null) 
            {
                _pauseText.SetActive(true);
                Debug.Log("Pause text shown");
            }
            
            if (_resumeButton != null) _resumeButton.gameObject.SetActive(true);
            if (_mainMenuButton != null) _mainMenuButton.gameObject.SetActive(true);
            if (_restartButton != null) _restartButton.gameObject.SetActive(true);
            
            if (_backgroundImage != null)
            {
                Color bgColor = Color.black;
                bgColor.a = _pauseBackgroundAlpha;
                _backgroundImage.color = bgColor;
            }
            
            if (_menuAnimator != null)
                _menuAnimator.ShowMenu();
        }

        /// <summary>
        /// Отображает меню поражения.
        /// Вызывается при достижении предела стресса или других условиях поражения.
        /// </summary>
        public void ShowGameOverMenu()
        {
            Debug.Log("<color=white>ShowGameOverMenu called</color>");
            _isPaused = true;
            _isGameOver = true;
            
            if (_pauseMenuPanel != null)
                _pauseMenuPanel.SetActive(true);
            
            HideAllTexts();
            if (_gameOverText != null) 
            {
                _gameOverText.SetActive(true);
                Debug.Log("Game Over text shown");
            }
            
            if (_resumeButton != null) _resumeButton.gameObject.SetActive(false);
            if (_mainMenuButton != null) _mainMenuButton.gameObject.SetActive(true);
            if (_restartButton != null) _restartButton.gameObject.SetActive(true);

            if (_backgroundImage != null)
            {
                Color bgColor = new Color(0.15f, 0f, 0f, _gameOverBackgroundAlpha);
                _backgroundImage.color = bgColor;
            }

            if (_menuAnimator != null)
                _menuAnimator.ShowMenu();
        }

        /// <summary>
        /// Отображает меню победы.
        /// Вызывается при успешном выполнении всех заданий.
        /// </summary>
        public void ShowVictoryMenu()
        {
            Debug.Log("<color=white>ShowVictoryMenu called</color>");
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

            if (_resumeButton != null) _resumeButton.gameObject.SetActive(false);
            if (_mainMenuButton != null) _mainMenuButton.gameObject.SetActive(true);
            if (_restartButton != null) _restartButton.gameObject.SetActive(true);

            if (_backgroundImage != null)
            {
                Color bgColor = new Color(0.2f, 0.15f, 0f, _gameOverBackgroundAlpha);
                _backgroundImage.color = bgColor;
            }
            
            if (_menuAnimator != null)
                _menuAnimator.ShowMenu();
        }

        /// <summary>
        /// Возобновляет игру после паузы.
        /// Снимает паузу, восстанавливает ввод и скрывает меню с анимацией.
        /// </summary>
        public void ResumeGame()
        {
            if (!_isPaused || _isGameOver) return;
            
            Debug.Log("<color=cyan>ResumeGame called</color>");

            _isPaused = false;

            if (GameCycle.Instance != null)
                GameCycle.Instance.ResumeGame();

            if (InputHandler.Instance != null)
                InputHandler.Instance.EnableInput();

            HideAllTexts();

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
        
        /// <summary>
        /// Полностью отключает панель меню после завершения анимации скрытия.
        /// Сбрасывает состояние меню и восстанавливает стандартные настройки.
        /// </summary>
        private void DisableMenuPanel()
        {
            Debug.Log("<color=cyan>DisableMenuPanel called</color>");

            if (_pauseMenuPanel != null)
                _pauseMenuPanel.SetActive(false);

            HideAllTexts();

            _isPaused = false;
            _isGameOver = false;

            if (_backgroundImage != null)
            {
                Color bgColor = Color.black;
                bgColor.a = _pauseBackgroundAlpha;
                _backgroundImage.color = bgColor;
            }

            if (_resumeButton != null) _resumeButton.gameObject.SetActive(true);
            if (_mainMenuButton != null) _mainMenuButton.gameObject.SetActive(true);
            if (_restartButton != null) _restartButton.gameObject.SetActive(true);
            
            Debug.Log("Menu panel disabled and reset");
        }
        
        /// <summary>
        /// Перезапускает текущий уровень.
        /// Восстанавливает нормальную скорость времени и загружает текущую сцену заново.
        /// Выполняет очистку синглтонов для предотвращения конфликтов состояния.
        /// </summary>
        private void RestartGame()
        {
            Debug.Log("<color=yellow>=== RESTART GAME ===</color>");

            Time.timeScale = 1f;
            CleanUpBeforeSceneChange();
            SceneManager.LoadScene(_currentSceneName);
        }

        /// <summary>
        /// Переходит в главное меню игры.
        /// Восстанавливает нормальную скорость времени и загружает сцену главного меню.
        /// Выполняет очистку синглтонов для предотвращения конфликтов состояния.
        /// </summary>
        private void GoToMainMenu()
        {
            Debug.Log("<color=yellow>Going to main menu...</color>");

            Time.timeScale = 1f;
            CleanUpBeforeSceneChange();
            SceneManager.LoadScene(0);
        }
        
        /// <summary>
        /// Очищает синглтоны и другие объекты, сохраняющие состояние, перед сменой сцены.
        /// Предотвращает конфликты и утечки памяти при перезагрузке сцены или переходе в меню.
        /// </summary>
        private void CleanUpBeforeSceneChange()
        {
            Debug.Log("Cleaning up before scene change...");

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
            
            //TODO Можно добавить очистку других синглтонов при необходимости
        }
        
        /// <summary>
        /// Обработчик события завершения игры.
        /// Вызывается при победе, поражении или выходе из игры.
        /// </summary>
        /// <param name="result">Результат завершения игры.</param>
        private void OnGameEnded(GameCycle.GameResult result)
        {
            Debug.Log($"<color=cyan>PauseMenu: Game ended with result {result}</color>");
        }

        /// <summary>
        /// Отписывается от событий и восстанавливает нормальную скорость времени при уничтожении объекта.
        /// Предотвращает утечки памяти и некорректное поведение при перезагрузке сцен.
        /// </summary>
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

            Time.timeScale = 1f;
        }

        /// <summary>
        /// Возвращает флаг, указывающий, находится ли игра в состоянии паузы.
        /// </summary>
        public bool IsPaused => _isPaused;
        
        /// <summary>
        /// Возвращает флаг, указывающий, завершена ли игра (поражение или победа).
        /// </summary>
        public bool IsGameOver => _isGameOver;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using Random = UnityEngine.Random;
using SampleScene;
using Shaders.ScreenEffects;

// Муштаков А.Ю.

/// <summary>
/// Управляет основным игровым циклом, состоянием игры и обработкой завершения игры.
/// Реализует паттерн Singleton для глобального доступа к игровой логике.
/// </summary>
public class GameCycle : MonoBehaviour
{
    /// <summary>
    /// Статический экземпляр для реализации паттерна Singleton.
    /// </summary>
    public static GameCycle Instance { get; private set; }
    
    [Header("Game Settings")]
    [SerializeField] private int _tasksToWin = 10;
    [SerializeField] private float _maxRabbitSpawnInterval = 20f;
    [SerializeField] private float _minRabbitSpawnInterval = 5f;
    
    [Header("End Game Effects")]
    [SerializeField] private float victoryFadeDuration = 2f;
    [SerializeField] private float defeatFadeDuration = 1.5f;
    [SerializeField] private Color victoryColor = new Color(1f, 0.8f, 0f, 1f); // Золотой
    [SerializeField] private Color defeatColor = new Color(0.8f, 0.1f, 0.1f, 1f); // Красный
    
    /// <summary>
    /// Событие обновления прогресса выполнения задач.
    /// Параметры: выполненные задачи, общее количество задач для победы.
    /// </summary>
    public event Action<int, int> OnProgressUpdated;
    
    /// <summary>
    /// Событие изменения уровня стресса.
    /// Параметр: текущий уровень стресса (0-100).
    /// </summary>
    public event Action<float> OnStressLevelChanged;
    
    /// <summary>
    /// Событие появления кролика на сцене.
    /// </summary>
    public event Action OnRabbitAppearing;
    
    /// <summary>
    /// Событие ухода кролика со сцены.
    /// </summary>
    public event Action OnRabbitLeaving;
    
    /// <summary>
    /// Событие завершения игры с указанием результата.
    /// </summary>
    public event Action<GameResult> OnGameEnded;
    
    private GameState _currentState = GameState.Playing;
    private float _timer;
    private float _rabbitTimer;
    private float _stressLevel = 0f;
    private int _completedTasks = 0;
    private bool _isRabbitHere = false;
    private float _rabbitInterval;

    /// <summary>
    /// Определяет возможные состояния игры.
    /// </summary>
    public enum GameState
    {
        Playing,
        Menu,
        Pause,
        GameOver
    }

    /// <summary>
    /// Определяет возможные результаты завершения игры.
    /// </summary>
    public enum GameResult
    {
        Victory, 
        Defeat, 
        Quit
    }

    /// <summary>
    /// Метод инициализации Singleton при создании объекта.
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
    /// Начинает игру при старте сцены.
    /// </summary>
    void Start()
    {
        StartGame();
    }
    
    /// <summary>
    /// Обновляет игровой цикл каждый кадр, если игра не на паузе.
    /// </summary>
    void Update()
    {
        if (PauseMenu.Instance != null && PauseMenu.Instance.IsPaused)
            return;
        
        if (_currentState != GameState.Playing) return;
        
        GameCycleUpdate();
    }
    
    /// <summary>
    /// Основной метод обновления игровой логики.
    /// Управляет таймерами, стрессом и проверяет условия завершения игры.
    /// </summary>
    void GameCycleUpdate() 
    {
        if (_currentState != GameState.Playing) return;

        UpdateTimer();

        if (!_isRabbitHere) 
        {
            UpdateRabbitSpawnTimer();
            UpdateStress(0.1f);
        }
        else
        {
            UpdateRabbitTimer();
            UpdateStress(0.5f);
        }

        CheckIsGameOver();
    }
    
    /// <summary>
    /// Приостанавливает игру, устанавливая состояние паузы.
    /// </summary>
    public void PauseGame()
    {
        if (_currentState != GameState.Playing) return;
        //Debug.Log("PauseGame");
        _currentState = GameState.Pause;
        Time.timeScale = 0f;
    }
    
    /// <summary>
    /// Возобновляет игру после паузы.
    /// </summary>
    public void ResumeGame()
    {
        if (_currentState != GameState.Pause) return;
        //Debug.Log("ResumeGame");
        _currentState = GameState.Playing;
        Time.timeScale = 1f;
    }
    
    /// <summary>
    /// Завершает игру по желанию игрока.
    /// </summary>
    public void QuitGame()
    {
        EndGame(GameResult.Quit);
    }
    
    /// <summary>
    /// Инициализирует игровые переменные перед началом игры.
    /// Сбрасывает таймеры, прогресс и уровень стресса.
    /// </summary>
    private void InitializeGame()
    {
        _rabbitInterval = _minRabbitSpawnInterval;
        _rabbitTimer = 0f;
        _stressLevel = 0f;
        _timer = 0f;
        _completedTasks = 0;
        _isRabbitHere = false;

        OnStressLevelChanged?.Invoke(_stressLevel);
        OnProgressUpdated?.Invoke(_completedTasks, _tasksToWin);
        
        // Сбрасываем цвет затемнения на черный при начале игры
        if (ScreenFadeManager.Instance != null)
        {
            ScreenFadeManager.Instance.SetFaderColor(Color.black);
        }
    }

    /// <summary>
    /// Начинает новую игру, сбрасывая состояние и запуская игровой цикл.
    /// </summary>
    public void StartGame()
    {
        InitializeGame();
        _currentState = GameState.Playing;
        Time.timeScale = 1f;
        
        //Debug.Log("<color=white>Игра началась!</color>");
    }
    
    /// <summary>
    /// Обновляет общий игровой таймер.
    /// </summary>
    private void UpdateTimer()
    {
        _timer += Time.deltaTime;
    }

    /// <summary>
    /// Обновляет уровень стресса с заданным множителем.
    /// </summary>
    /// <param name="multiplier">Множитель скорости накопления стресса.</param>
    private void UpdateStress(float multiplier)
    {
        _stressLevel += Time.deltaTime * multiplier;
        _stressLevel = Mathf.Clamp(_stressLevel, 0f, 100f);
        OnStressLevelChanged?.Invoke(_stressLevel);
    }

    /// <summary>
    /// Добавляет указанное количество стресса.
    /// </summary>
    /// <param name="stress">Количество добавляемого стресса.</param>
    public void AddStress(float stress)
    {
        _stressLevel += stress;
        _stressLevel = Mathf.Clamp(_stressLevel, 0f, 100f);
        OnStressLevelChanged?.Invoke(_stressLevel);
    }

    /// <summary>
    /// Обновляет таймер появления кролика, если кролик отсутствует на сцене.
    /// </summary>
    private void UpdateRabbitSpawnTimer()
    {
        _rabbitTimer += Time.deltaTime;
        if (_rabbitTimer >= _rabbitInterval)
        {
            RabbitAppear();
        }
    }
    
    /// <summary>
    /// Обновляет таймер присутствия кролика на сцене.
    /// Кролик уходит через 5 секунд после появления.
    /// </summary>
    private void UpdateRabbitTimer()
    {
        _rabbitTimer += Time.deltaTime;
        if (_rabbitTimer >= 5f)
        {
            RabbitLeave();
        }
    }
    
    /// <summary>
    /// Вызывает появление кролика на сцене.
    /// </summary>
    private void RabbitAppear()
    {
        if (_isRabbitHere) return;

        _isRabbitHere = true;
        _rabbitTimer = 0f;
        OnRabbitAppearing?.Invoke();
        //Debug.Log("<color=white>Кролик появился</color>");
    }
    
    /// <summary>
    /// Убирает кролика со сцены и устанавливает новый интервал появления.
    /// </summary>
    private void RabbitLeave()
    {
        if (!_isRabbitHere) return;

        _isRabbitHere = false;
        _rabbitTimer = 0f;
        _rabbitInterval = Random.Range(_minRabbitSpawnInterval, _maxRabbitSpawnInterval);
        OnRabbitLeaving?.Invoke();
        //Debug.Log("<color=white>Кролик ушёл</color>");
    }

    /// <summary>
    /// Регистрирует выполнение задачи и обновляет прогресс.
    /// </summary>
    public void CompleteTask()
    {
        _completedTasks++;
        OnProgressUpdated?.Invoke(_completedTasks, _tasksToWin);
    }

    /// <summary>
    /// Обрабатывает провал задачи, добавляя штрафной стресс.
    /// </summary>
    /// <param name="timePenalty">Штрафное время, конвертируемое в стресс.</param>
    public void FailTask(float timePenalty)
    {
        AddStress(timePenalty * 1f);
    }

    /// <summary>
    /// Проверяет условия завершения игры (победа или поражение).
    /// </summary>
    private void CheckIsGameOver()
    {
        if (_completedTasks >= _tasksToWin)
        {
            StartCoroutine(VictorySequence());
            return;
        }
        if (_stressLevel >= 100f)
        {
            StartCoroutine(DefeatSequence());
            return;
        }
    }
    
    //! Метод написан ИИ

    /// <summary>
    /// Последовательность действий при победе в игре.
    /// Включает визуальные эффекты и отображение меню победы.
    /// </summary>
    private IEnumerator DefeatSequence()
    {
        //Debug.Log("<color=cyan>Starting defeat sequence...</color>");
        
        _currentState = GameState.GameOver;
        
        // 1. Быстрое затемнение экрана красным цветом
        if (ScreenFadeManager.Instance != null)
        {
            ScreenFadeManager.Instance.SetFaderColor(defeatColor);
            ScreenFadeManager.Instance.QuickFadeToBlack(defeatFadeDuration);
        }
        
        // 2. Мигаем красным несколько раз
        if (ScreenFadeManager.Instance != null)
        {
            ScreenFadeManager.Instance.BlinkScreen(0.3f, 3, defeatColor);
        }
        
        // 3. Дополнительные эффекты для поражения
        if (ScreenFadeManager.Instance != null)
        {
            // Сердцебиение при поражении
            yield return new WaitForSeconds(0.5f);
            ScreenFadeManager.Instance.HeartbeatEffect(0.4f, 4, 0.1f);
        }
        
        // 4. Ждем, чтобы затемнение началось
        yield return new WaitForSecondsRealtime(0.8f);
        
        // 5. Показываем меню Game Over
        if (PauseMenu.Instance != null)
        {
            PauseMenu.Instance.ShowGameOverMenu();
        }
        
        // 6. Вызываем EndGame с задержкой
        EndGame(GameResult.Defeat);
    }

    //! Метод написан ИИ

    /// <summary>
    /// Последовательность действий при поражении в игре.
    /// Включает визуальные эффекты и отображение меню поражения.
    /// </summary>
    private IEnumerator VictorySequence()
    {
        //Debug.Log("<color=cyan>Starting victory sequence...</color>");
        
        _currentState = GameState.GameOver;
        
        // 1. Затемнение экрана с золотым оттенком
        if (ScreenFadeManager.Instance != null)
        {
            ScreenFadeManager.Instance.SetFaderColor(victoryColor);
            ScreenFadeManager.Instance.QuickFadeToBlack(victoryFadeDuration);
        }
        
        // 2. Мигаем золотым несколько раз
        if (ScreenFadeManager.Instance != null)
        {
            ScreenFadeManager.Instance.BlinkScreen(0.4f, 3, victoryColor);
        }
        
        // 3. Дополнительные эффекты для победы
        if (ScreenFadeManager.Instance != null)
        {
            // Пульсация золотым при победе
            yield return new WaitForSeconds(0.5f);
            for (int i = 0; i < 2; i++)
            {
                ScreenFadeManager.Instance.BlinkScreen(0.5f, 1, victoryColor);
                yield return new WaitForSeconds(0.3f);
            }
        }
        
        // 4. Ждем
        yield return new WaitForSecondsRealtime(1f);
        
        // 5. Показываем меню победы
        if (PauseMenu.Instance != null)
        {
            PauseMenu.Instance.ShowVictoryMenu();
        }
        
        // 6. Вызываем EndGame
        EndGame(GameResult.Victory);
    }
    
    /// <summary>
    /// Вызывается при уничтожении объекта.
    /// Сбрасывает цвет затемнения экрана на черный.
    /// </summary>
    void OnDestroy()
    {
        // Сброс цвета затемнения при уничтожении
        ScreenFadeManager.StaticSetFaderColor(Color.black);
    }
    
    /// <summary>
    /// Завершает игру с указанным результатом.
    /// Останавливает время и вызывает событие завершения игры.
    /// </summary>
    /// <param name="result">Результат завершения игры.</param>
    private void EndGame(GameResult result)
    {
        if (_currentState == GameState.GameOver) return;
        
        _currentState = GameState.GameOver;
        Time.timeScale = 0f;
        
        OnGameEnded?.Invoke(result);
        
        string resultText = result switch
        {
            GameResult.Victory => "ПОБЕДА! Все задания выполнены.",
            GameResult.Defeat => "ПОРАЖЕНИЕ! Стресс достиг предела.",
            _ => "Игра завершена."
        };
        //Debug.Log(resultText);
    }
    
    /// <summary>
    /// Возвращает true, если кролик в настоящее время находится на сцене.
    /// </summary>
    public bool IsRabbitHere => _isRabbitHere;
    
    /// <summary>
    /// Возвращает текущий уровень стресса (0-100).
    /// </summary>
    public float StressLevel => _stressLevel;
    
    /// <summary>
    /// Возвращает количество выполненных задач.
    /// </summary>
    public int CompletedTasks => _completedTasks;
    
    /// <summary>
    /// Возвращает общее количество задач, необходимых для победы.
    /// </summary>
    public int TotalTasksToWin => _tasksToWin;
    
    /// <summary>
    /// Возвращает текущее состояние игры.
    /// </summary>
    public GameState CurrentState => _currentState;
}
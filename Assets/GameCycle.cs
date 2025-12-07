using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using Random = UnityEngine.Random;

public class GameCycle : MonoBehaviour
{
    public static GameCycle Instance { get; private set; }
    [SerializeField] private int _tasksToWin = 10;
    [SerializeField] private float _maxRabbitSpawnInterval = 30f;
    [SerializeField] private float _minRabbitSpawnInterval = 8f;

    public event Action<int, int> OnProgressUpdated; // Прогресс: выполнено/всего
    public event Action<float> OnStressLevelChanged; // Изменение уровня стресса (0-100%)
    public event Action OnRabbitAppearing; // Кролик появляется (визуальный сигнал)
    public event Action OnRabbitLeaving; // Кролик уходит
    public event Action<GameResult> OnGameEnded; // Игра завершена (победа/поражение)
    
    private GameState _currentState = GameState.Playing; // menu
    private float _timer;
    private float _rabbitTimer;
    private float _stressLevel = 0f;
    private int _completedTasks = 0;
    private bool _isRabbitHere = false;
    private float _rabbitInterval;
    //private TaskManager _taskManager;
    //private UIManager _uiManager;
    
    public enum GameState
    {
        Playing,
        Menu,
        Pause,
        GameOver
    }

    public enum GameResult
    {
        Victory, 
        Defeat, 
        Quit
    }
    // Start is called before the first frame update
    /*
    void Start()
    {
        // Поиск зависимостей (можно сделать через инспектор)
        _taskManager = FindObjectOfType<TaskManager>();
        _uiManager = FindObjectOfType<UIManager>();
        
        // Подписка на события TaskManager
        if (_taskManager != null)
        {
            _taskManager.OnTaskCompleted += HandleTaskCompleted;
            _taskManager.OnTaskFailed += HandleTaskFailed;
            _taskManager.OnTaskCorrupted += HandleTaskCorrupted;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
*/
    void Awake()
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
        StartGame();
    }
    
    void Update()
    {
        if (_currentState != GameState.Playing) return;
        
        GameCycleUpdate();
    }
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
    
    private void TogglePause()
    {
        Debug.Log($"TogglePause вызван. Текущее состояние: {_currentState}");
        if (_currentState == GameState.Playing)
        {
            PauseGame();
        }
        else if (_currentState == GameState.Pause)
        {
            ResumeGame();
        }
    }
    
    public void PauseGame()
    {
        if (_currentState != GameState.Playing) return;
        Debug.Log("PauseGame");
        _currentState = GameState.Pause;
        Time.timeScale = 0f;
    }
    
    // Возобновить игру
    public void ResumeGame()
    {
        if (_currentState != GameState.Pause) return;
        Debug.Log("ResumeGame");
        _currentState = GameState.Playing;
        Time.timeScale = 1f;
    }
    
    public void QuitGame()
    {
        EndGame(GameResult.Quit);
    }
    
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
    }

    public void StartGame()
    {
        InitializeGame();
        _currentState = GameState.Playing;
        Time.timeScale = 1f;
        
        Debug.Log("Игра началась!");
    }
    
    private void UpdateTimer()
    {
        _timer += Time.deltaTime;
    }

    private void UpdateStress(float multiplier)
    {
        _stressLevel += Time.deltaTime * multiplier;
        _stressLevel = Mathf.Clamp(_stressLevel, 0f, 100f);
        OnStressLevelChanged?.Invoke(_stressLevel);
        //Debug.Log($"Stress Level {_stressLevel}");
    }

    public void AddStress(float stress)
    {
        _stressLevel += stress;
        _stressLevel = Mathf.Clamp(_stressLevel, 0f, 100f);
        OnStressLevelChanged?.Invoke(_stressLevel);
        //Debug.Log($"<color=orange>Стресс увеличен на {stress:F1}. Текущий уровень: {_stressLevel:F1}</color>");
    }

    private void UpdateRabbitSpawnTimer()
    {
        _rabbitTimer += Time.deltaTime;
        if (_rabbitTimer >= _rabbitInterval)
        {
            RabbitAppear();
        }
    }
    private void UpdateRabbitTimer()
    {
        _rabbitTimer += Time.deltaTime;
        if (_rabbitTimer >= 5f)
        {
            RabbitLeave();
        }
    }
    
    private void RabbitAppear()
    {
        if (_isRabbitHere) return;

        _isRabbitHere = true;
        _rabbitTimer = 0f;
        OnRabbitAppearing?.Invoke();
        Debug.Log("Кролик появился!");
    }
    
    private void RabbitLeave()
    {
        if (!_isRabbitHere) return;

        _isRabbitHere = false;
        _rabbitTimer = 0f;
        // Обновляем интервал для следующего появления
        _rabbitInterval = Random.Range(_minRabbitSpawnInterval, _maxRabbitSpawnInterval);
        OnRabbitLeaving?.Invoke();
        Debug.Log("Кролик ушёл");
    }

    public void CompleteTask()
    {
        _completedTasks++;
        OnProgressUpdated?.Invoke(_completedTasks, _tasksToWin);
    }

    public void FailTask(float timePenalty)
    {
        // Штраф за провал - конвертируем время в стресс
        AddStress(timePenalty * 1.5f);
    }

    private void CheckIsGameOver()
    {
        if (_completedTasks >= _tasksToWin)
        {
            EndGame(GameResult.Victory);
            Debug.Log("You Win!");
            return;
        }
        if (_stressLevel >= 100f)
        {
            EndGame(GameResult.Defeat);
            Debug.Log("Game Over (Heartatack)");
            return;
        }
    }

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
        Debug.Log(resultText);
    }
    public bool IsRabbitHere => _isRabbitHere;
    public float StressLevel => _stressLevel;
    public int CompletedTasks => _completedTasks;
    public int TotalTasksToWin => _tasksToWin;
    public GameState CurrentState => _currentState;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using Random = UnityEngine.Random;
using SampleScene;
using Shaders.ScreenEffects; // Добавлено для доступа к ScreenFadeManager

public class GameCycle : MonoBehaviour
{
    public static GameCycle Instance { get; private set; }
    
    [Header("Game Settings")]
    [SerializeField] private int _tasksToWin = 10;
    [SerializeField] private float _maxRabbitSpawnInterval = 30f;
    [SerializeField] private float _minRabbitSpawnInterval = 8f;
    
    [Header("End Game Effects")]
    [SerializeField] private float victoryFadeDuration = 2f;
    [SerializeField] private float defeatFadeDuration = 1.5f;
    [SerializeField] private Color victoryColor = new Color(1f, 0.8f, 0f, 1f); // Золотой
    [SerializeField] private Color defeatColor = new Color(0.8f, 0.1f, 0.1f, 1f); // Красный
    
    public event Action<int, int> OnProgressUpdated;
    public event Action<float> OnStressLevelChanged;
    public event Action OnRabbitAppearing;
    public event Action OnRabbitLeaving;
    public event Action<GameResult> OnGameEnded;
    
    private GameState _currentState = GameState.Playing;
    private float _timer;
    private float _rabbitTimer;
    private float _stressLevel = 0f;
    private int _completedTasks = 0;
    private bool _isRabbitHere = false;
    private float _rabbitInterval;

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
        if (PauseMenu.Instance != null && PauseMenu.Instance.IsPaused)
            return;
        
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
        Debug.Log($"<color=cyan>TogglePause вызван. Текущее состояние: {_currentState}</color>");
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
        
        // Сбрасываем цвет затемнения на черный при начале игры
        if (ScreenFadeManager.Instance != null)
        {
            ScreenFadeManager.Instance.SetFaderColor(Color.black);
        }
    }

    public void StartGame()
    {
        InitializeGame();
        _currentState = GameState.Playing;
        Time.timeScale = 1f;
        
        Debug.Log("<color=white>Игра началась!</color>");
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
    }

    public void AddStress(float stress)
    {
        _stressLevel += stress;
        _stressLevel = Mathf.Clamp(_stressLevel, 0f, 100f);
        OnStressLevelChanged?.Invoke(_stressLevel);
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
        Debug.Log("<color=white>Кролик появился</color>");
    }
    
    private void RabbitLeave()
    {
        if (!_isRabbitHere) return;

        _isRabbitHere = false;
        _rabbitTimer = 0f;
        _rabbitInterval = Random.Range(_minRabbitSpawnInterval, _maxRabbitSpawnInterval);
        OnRabbitLeaving?.Invoke();
        Debug.Log("<color=white>Кролик ушёл</color>");
    }

    public void CompleteTask()
    {
        _completedTasks++;
        OnProgressUpdated?.Invoke(_completedTasks, _tasksToWin);
    }

    public void FailTask(float timePenalty)
    {
        AddStress(timePenalty * 1f);
    }

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
    
    private IEnumerator DefeatSequence()
    {
        Debug.Log("<color=cyan>Starting defeat sequence...</color>");
        
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

    private IEnumerator VictorySequence()
    {
        Debug.Log("<color=cyan>Starting victory sequence...</color>");
        
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
    
    void OnDestroy()
    {
        // Сброс цвета затемнения при уничтожении
        ScreenFadeManager.StaticSetFaderColor(Color.black);
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
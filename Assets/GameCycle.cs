using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCycle : MonoBehaviour
{
    private GameState _currentState = GameState.Menu;
    private float _timer;
    private float _rabbitTimer;
    private float _taskTimer;
    private float _timerInterval;
    private float _stressLevel = 0f;
    private int _completedTasks = 0;
    private float _isRabbitHere = 0;
    private float _rabbitInterval;
    //!private TaskManager _taskManager;
    //!private UIManager _uiManager;
    
    private enum GameState
    {
        Playing,
        Menu,
        Pause,
        GameOver
    }

    public enum GameResult
    {
        Victory, 
        TimeOut, 
        StressOverload, 
        Quit
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void InitializeGame()
    {
        _rabbitInterval = 20f;
        _rabbitTimer = 0f;
        _stressLevel = 0f;
        _taskTimer = 0f;
        _timer = 0f;
        _completedTasks = 0;
        //!_isRabbitHere = false;
    }
    //! void GameCycleUpdate() 
    // {
    //     if (_currentState != GameState.Playing) return;
    //     UpdateTimer();
    //     if (!_isRabbitHere) 
    //     {
    //         UpdateRabbitTimer();
    //         UpdateStress(0.2);
    //     }
    //     else
    //     {
    //         UpdateNoRabbitTimer();
    //         UpdateStress(0.1);
    //    }

    //     CheckIsGameOver();
    // }

    public void StartGame()
    {
        InitializeGame();
        _currentState = GameState.Playing;
    }
    // public void PauseGame()
    // {
    //     if (_currentState == GameState.Playing)
    //     {
    //         _currentState = GameState.Paused;
    //         Time.timeScale = 0f;
    //     }
    // }
    
    // Возобновить игру
    // public void ResumeGame()
    // {
    //     if (_currentState == GameState.Paused)
    //     {
    //         _currentState = GameState.Playing;
    //         Time.timeScale = 1f;
    //     }
    // }
    // public void QuitGame()
    // {
    //     EndGame(GameResult.Quit);
    // }

    private void UpdateTimer()
    {
        _timer += Time.deltaTime;
    }

    private void UpdateStress(float multiplier)
    {
        _stressLevel += Time.deltaTime * multiplier;
    }

    private void UpdateRabbitTimer()
    {
        
    }
    private void UpdateNoRabbitTimer()
    {
        
    }

    private void CheckIsGameOver()
    {
        if (_stressLevel == 100)
        {
            Debug.Log("Game Over (Heartatack)");
        }
    }
}

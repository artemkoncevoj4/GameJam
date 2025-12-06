using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InputHandler : MonoBehaviour
{
    public static InputHandler Instance { get; private set; }
    public event Action OnInteractPressed;
    public event Action OnPausePressed;
    public event Action OnCancelPressed;
    
    // Для отладки/тестирования
    public event Action OnDebugKeyPressed;
    
    // Состояние
    private bool _inputEnabled = true;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    void Update()
    {
        if (!_inputEnabled) return;
        
        // Основные игровые действия
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
        {
            OnInteractPressed?.Invoke();
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnPausePressed?.Invoke();
        }
        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            OnCancelPressed?.Invoke();
        }
        
        // Для тестирования (можно быстро вызывать Кролика)
        if (Input.GetKeyDown(KeyCode.F1))
        {
            OnDebugKeyPressed?.Invoke();
        }
    }
    
    public void EnableInput() => _inputEnabled = true;
    public void DisableInput() => _inputEnabled = false;
    // Start is called before the first frame updat
    /*
    void Start()
    {
        
    }
    S
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (CurrentState == GameState.Playing)
            {
                PauseGame();
            }
            else
            {
                ResumeGame();
            }
            Debug.Log("Game paused");
        }
    }*/
}

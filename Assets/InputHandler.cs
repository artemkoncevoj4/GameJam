using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using SampleScene;
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
        if (Input.GetKeyDown(KeyCode.E))
        {
            OnInteractPressed?.Invoke();
        }
        
        // Обработка паузы - ИСПРАВЛЕННАЯ ЛОГИКА
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("ESC pressed");
            
            // Попробуем найти меню паузы, если оно еще не инициализировано
            if (PauseMenu.Instance == null)
            {
                Debug.LogWarning("<color=yellow>PauseMenu.Instance is null, trying to find it...</color>");
                PauseMenu pauseMenu = FindAnyObjectByType<PauseMenu>();
                if (pauseMenu != null)
                {
                    Debug.Log("<color=green>Found PauseMenu in scene: </color>" + pauseMenu.gameObject.name);
                }
            }
            
            if (PauseMenu.Instance != null)
            {
                Debug.Log("<color=cyan>Calling PauseMenu.Instance.TogglePause()</color>");
                PauseMenu.Instance.TogglePause();
            }
            else
            {
                Debug.LogError("<color=red>PauseMenu.Instance is null! Cannot pause game.</color>");
            }
            
            // Также вызываем событие для других систем
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
    
    // Публичные геттеры для проверки состояния
    public bool IsInputEnabled => _inputEnabled;
}
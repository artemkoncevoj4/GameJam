using UnityEngine;
using SampleScene;

// Муштаков А.Ю.

/// <summary>
/// Централизованный обработчик ввода, управляющий игровыми действиями через клавиатуру.
/// Реализует паттерн Singleton для глобального доступа к состоянию ввода и событиям ввода.
/// </summary>
public class InputHandler : MonoBehaviour
{
    /// <summary>
    /// Статический экземпляр для реализации паттерна Singleton.
    /// Обеспечивает глобальный доступ к обработчику ввода из любых частей кода.
    /// </summary>
    public static InputHandler Instance { get; private set; }
    
    /// <summary>
    /// Флаг, указывающий, активен ли ввод в данный момент.
    /// Когда false, все обработки ввода игнорируются.
    /// </summary>
    private bool _inputEnabled = true;
    
    /// <summary>
    /// Инициализирует Singleton при создании объекта.
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
    
    //! Метод сгенерирован ИИ

    /// <summary>
    /// Обрабатывает ввод с клавиатуры каждый кадр.
    /// Проверяет нажатия клавиш и вызывает соответствующие события.
    /// Содержит специальную логику для обработки паузы с поиском PauseMenu в сцене.
    /// </summary>
    void Update()
    {
        if (!_inputEnabled) return;
        
        
        // Обработка паузы
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("ESC pressed");
            
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
            
        }
    }
    
    /// <summary>
    /// Включает обработку ввода.
    /// </summary>
    public void EnableInput() => _inputEnabled = true;
    
    /// <summary>
    /// Отключает обработку ввода.
    /// </summary>
    public void DisableInput() => _inputEnabled = false;
    
    /// <summary>
    /// Возвращает текущее состояние обработки ввода.
    /// </summary>
    /// <value>True, если ввод включен, иначе False.</value>
    public bool IsInputEnabled => _inputEnabled;
}
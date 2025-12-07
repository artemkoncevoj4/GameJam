using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Bunny {
// Определяем простой класс для задачи (не ScriptableObject, а обычный класс)
public class GameTask
{
    public string Name;
    public string Description;
    public bool IsCorrupted = false; // Может ли Заяц испортить задание
}

public class TestTaskManager : MonoBehaviour
{
    // Singleton: статический экземпляр для доступа через TestTaskManager.Instance
    public static TestTaskManager Instance { get; private set; }

    [Header("Настройки")]
    [SerializeField] private float taskCompletionTime = 10f; // Время на выполнение тестового задания
    
    private Queue<GameTask> _taskQueue;
    private GameTask _currentTask = null;
    
    // Ссылка на BunnyManager для вызова Зайца
    private BunnyManager _bunnyManager; 

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            InitializeTasks();
        }
    }

    private void Start()
    {
        _bunnyManager = FindObjectOfType<BunnyManager>();
        if (_bunnyManager == null)
        {
            Debug.LogError("BunnyManager не найден в сцене! Заяц не сможет появиться.");
        }
        
        // [ТРЕБОВАНИЕ] Сразу после старта сцены Заяц выдает первое задание.
        StartCoroutine(InitialTaskSetup());
    }

    private void InitializeTasks()
    {
        // Создаем тестовую очередь заданий
        _taskQueue = new Queue<GameTask>();
        _taskQueue.Enqueue(new GameTask { Name = "Task 1", Description = "Найти и кликнуть на красную кнопку." });
        _taskQueue.Enqueue(new GameTask { Name = "Task 2", Description = "Включить все лампочки на панели." });
        _taskQueue.Enqueue(new GameTask { Name = "Task 3", Description = "Провести 5 секунд под столом." });
    }

    // Инициализация. Вызываем Зайца после того, как все Start() сработают
    private IEnumerator InitialTaskSetup()
    {
        yield return null; // Ждем один кадр для инициализации
        
        Debug.Log("TaskManager: Запуск первого задания.");
        
        // BunnyManager.TestSpawnBunny() вызывает Bunny.Appear()
        if (_bunnyManager != null)
        {
            _bunnyManager.TestSpawnBunny(); 
        }
    }
    
    // ============== Методы, вызываемые Зайцем (BunnyDialogueManager) ==============
    
    // Возвращает текущее задание (или null)
    public GameTask GetCurrentTask()
    {
        return _currentTask;
    }

    // [BunnyDialogueManager] Назначает новое задание из очереди
    public void AssignNewTask()
    {
        if (_taskQueue.Count > 0)
        {
            _currentTask = _taskQueue.Dequeue();
            _currentTask.IsCorrupted = false; // Убедимся, что новая задача чиста
            Debug.Log($"TaskManager: Назначено новое задание: {_currentTask.Name}");
            
            // Начинаем таймер на выполнение задания
            StartCoroutine(SimulateTaskCompletion(taskCompletionTime));
        }
        else
        {
            _currentTask = null;
            Debug.Log("TaskManager: Заданий больше нет!"); // Последнее сообщение
        }
    }

    // [BunnyDialogueManager] Испортить текущее задание
    public void CorruptCurrentTask()
    {
        if (_currentTask != null)
        {
            _currentTask.IsCorrupted = true;
            Debug.Log($"TaskManager: Задание {_currentTask.Name} ИСПОРЧЕНО Зайцем!");
        }
    }

    // Вызывается для завершения текущего задания (после 10 секунд)
    public void CompleteCurrentTask()
    {
        if (_currentTask != null)
        {
            Debug.Log($"TaskManager: Задание {_currentTask.Name} выполнено. Статус порчи: {_currentTask.IsCorrupted}");
            _currentTask = null;
            
            // Если остались задания, вызываем Зайца, чтобы он выдал следующее
            if (_taskQueue.Count > 0)
            {
                Debug.Log("TaskManager: Задание выполнено. Вызываем Зайца для следующего задания.");
                if (_bunnyManager != null)
                {
                    _bunnyManager.TestSpawnBunny(); 
                }
            }
            else
            {
                Debug.Log("TaskManager: Заданий больше нет! Конец игры.");
            }
        }
    }
    
    // ============== Тестовая логика таймера ==============

    // Симуляция выполнения задания через заданную задержку
    private IEnumerator SimulateTaskCompletion(float delay)
    {
        Debug.Log($"TaskManager: Задание {_currentTask.Name} активно. Таймер на {delay} секунд запущен.");
        yield return new WaitForSeconds(delay);
        
        if (_currentTask != null)
        {
            Debug.Log($"TaskManager: Симуляция завершилась. Вызываем CompleteCurrentTask.");
            CompleteCurrentTask();
        }
    }
}
}
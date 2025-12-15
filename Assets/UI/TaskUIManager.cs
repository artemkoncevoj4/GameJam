using UnityEngine;

// * Жаркова Т.В.

namespace UI
{
    /// <summary>
    /// TaskUIManager управляет всеми компонентами UI, связанными с отображением заданий,
    /// такими как текст задания (<see cref="TaskDisplayUI"/>) и таймер (<see cref="SimpleTaskTimer"/>).
    /// Реализует паттерн Singleton для обеспечения единой точки доступа к управлению UI заданий.
    /// </summary>
    public class TaskUIManager : MonoBehaviour
    {
        [SerializeField] private SimpleTaskTimer _taskTimer;
        [SerializeField] private TaskDisplayUI _taskDisplay;
        
        /// <summary>
        /// Единственный экземпляр TaskUIManager.
        /// </summary>
        public static TaskUIManager Instance { get; private set; }
        
        /// <summary>
        /// Инициализация Singleton.
        /// </summary>
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        /// <summary>
        /// Принудительно обновляет все элементы UI, связанные с заданием (текст задания и таймер).
        /// Используется для синхронизации UI с текущим состоянием задания, например, после загрузки сцены или при паузе.
        /// </summary>
        public void ForceUpdateUI()
        {
            if (_taskTimer != null)
            {
                _taskTimer.ForceStartTimer();
                _taskTimer.ForceUpdate();
            }
            
            if (_taskDisplay != null)
            {
                _taskDisplay.ForceShowCurrentTask();
                _taskDisplay.ForceUpdate();
            }
            
            Debug.Log("<color=green>TaskUIManager: UI принудительно обновлен</color>");
        }
        
        /// <summary>
        /// Вызывает принудительное обновление UI с небольшой задержкой.
        /// Используется для обеспечения того, чтобы все менеджеры успели инициализироваться после создания нового задания.
        /// </summary>
        public void UpdateTaskUI()
        {
            Invoke(nameof(ForceUpdateUI), 0.2f); // Задержка для гарантии
        }
    }
}
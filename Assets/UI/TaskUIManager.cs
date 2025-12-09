using UnityEngine;

namespace UI
{
    public class TaskUIManager : MonoBehaviour
    {
        [SerializeField] private SimpleTaskTimer _taskTimer;
        [SerializeField] private TaskDisplayUI _taskDisplay;
        
        public static TaskUIManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        // [!] Метод для принудительного обновления UI
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
            
            Debug.Log("TaskUIManager: UI принудительно обновлен");
        }
        
        // [!] Метод для вызова из Bunny после создания задания
        public void UpdateTaskUI()
        {
            Invoke(nameof(ForceUpdateUI), 0.2f); // Задержка для гарантии
        }
    }
}
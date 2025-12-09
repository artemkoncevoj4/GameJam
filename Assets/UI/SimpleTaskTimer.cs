using UnityEngine;
using TMPro;
using System.Collections;
using TaskSystem;

namespace UI
{
    public class SimpleTaskTimer : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _timerText;
        
        private bool _isSubscribed = false;
        private bool _hasActiveTask = false;

        void Start()
        {
            // Начинаем с выключенного таймера
            if (_timerText != null)
            {
                _timerText.gameObject.SetActive(false);
            }
            
            // Ждем перед подпиской, чтобы убедиться, что все менеджеры инициализированы
            Invoke(nameof(SubscribeToEvents), 0.5f);
        }
        public void ForceStartTimer()
        {
            if (TaskManager.Instance != null)
            {
                var task = TaskManager.Instance.GetCurrentTask();
                if (task != null && !task.IsCompleted && !task.IsFailed)
                {
                    _hasActiveTask = true;
                    if (_timerText != null)
                    {
                        _timerText.gameObject.SetActive(true);
                        UpdateTimerDisplay(task.TimeRemaining);
                        Debug.Log("<color=green>SimpleTaskTimer: Таймер принудительно запущен</color>");
                    }
                }
            }
        }
        void OnEnable()
        {
            // Если объект переактивируется, проверяем подписку
            if (!_isSubscribed)
            {
                SubscribeToEvents();
            }
            
            // Обновляем отображение при активации
            if (_hasActiveTask && TaskManager.Instance != null)
            {
                var task = TaskManager.Instance.GetCurrentTask();
                if (task != null && !task.IsCompleted && !task.IsFailed)
                {
                    UpdateTimerDisplay(task.TimeRemaining);
                }
            }
        }

        void OnDisable()
        {
            // Отписываемся только если объект деактивируется
            UnsubscribeFromEvents();
        }

        void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            if (_isSubscribed || TaskManager.Instance == null) return;
            
            TaskManager.Instance.OnNewTask += OnNewTask;
            TaskManager.Instance.OnTaskTimerUpdated += OnTimerUpdated;
            TaskManager.Instance.OnTaskCompleted += OnTaskCompleted;
            TaskManager.Instance.OnTaskFailed += OnTaskFailed;
            TaskManager.Instance.OnTaskCorrupted += OnTaskCorrupted;
            
            _isSubscribed = true;
            Debug.Log("<color=green>SimpleTaskTimer: Подписался на события</color>");
            
            // Проверяем, нет ли уже активного задания
            CheckForExistingTask();
        }

        private void UnsubscribeFromEvents()
        {
            if (!_isSubscribed || TaskManager.Instance == null) return;
            
            TaskManager.Instance.OnNewTask -= OnNewTask;
            TaskManager.Instance.OnTaskTimerUpdated -= OnTimerUpdated;
            TaskManager.Instance.OnTaskCompleted -= OnTaskCompleted;
            TaskManager.Instance.OnTaskFailed -= OnTaskFailed;
            TaskManager.Instance.OnTaskCorrupted -= OnTaskCorrupted;
            
            _isSubscribed = false;
            Debug.Log("<color=yellow>SimpleTaskTimer: Отписался от событий</color>");
        }

        private void CheckForExistingTask()
        {
            if (TaskManager.Instance == null) return;
            
            var currentTask = TaskManager.Instance.GetCurrentTask();
            if (currentTask != null && TaskManager.Instance.IsTaskActive)
            {
                _hasActiveTask = true;
                if (_timerText != null)
                {
                    _timerText.gameObject.SetActive(true);
                    UpdateTimerDisplay(currentTask.TimeRemaining);
                    Debug.Log("SimpleTaskTimer: Нашел активное задание при старте");
                }
            }
        }

        private void OnNewTask(BureaucraticTask task)
        {
            _hasActiveTask = true;
            
            if (_timerText != null)
            {
                _timerText.gameObject.SetActive(true);
                UpdateTimerDisplay(task.TimeRemaining);
                Debug.Log($"SimpleTaskTimer: Новое задание, время: {task.TimeRemaining}");
            }
        }

        private void OnTimerUpdated(float timeRemaining)
        {
            if (_hasActiveTask && _timerText != null)
            {
                UpdateTimerDisplay(timeRemaining);
            }
        }

        private void OnTaskCompleted(BureaucraticTask task)
        {
            _hasActiveTask = false;
            
            if (_timerText != null)
            {
                _timerText.gameObject.SetActive(false);
                Debug.Log("SimpleTaskTimer: Задание завершено, скрываю таймер");
            }
        }

        private void OnTaskFailed(BureaucraticTask task)
        {
            _hasActiveTask = false;
            
            if (_timerText != null)
            {
                _timerText.gameObject.SetActive(false);
                Debug.Log("SimpleTaskTimer: Задание провалено, скрываю таймер");
            }
        }

        private void OnTaskCorrupted(BureaucraticTask task)
        {
            // При изменении задания просто обновляем время (оно могло поменяться)
            if (_hasActiveTask && _timerText != null && task != null)
            {
                UpdateTimerDisplay(task.TimeRemaining);
                Debug.Log("SimpleTaskTimer: Задание изменено, обновляю таймер");
            }
        }

        private void UpdateTimerDisplay(float timeRemaining)
        {
            if (_timerText == null) return;

            // Форматируем MM:SS
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            _timerText.text = $"{minutes:00}:{seconds:00}";
            
            // Меняем цвет при низком времени
            if (timeRemaining < 10f)
            {
                _timerText.color = Color.red;
                // Пульсация при критическом времени
                StartPulseEffect();
            }
            else if (timeRemaining < 30f)
            {
                _timerText.color = Color.yellow;
                StopPulseEffect();
            }
            else
            {
                _timerText.color = Color.white;
                StopPulseEffect();
            }
        }

        private Coroutine _pulseCoroutine;
        
        private void StartPulseEffect()
        {
            StopPulseEffect();
            _pulseCoroutine = StartCoroutine(PulseEffect());
        }

        private void StopPulseEffect()
        {
            if (_pulseCoroutine != null)
            {
                StopCoroutine(_pulseCoroutine);
                _pulseCoroutine = null;
                if (_timerText != null)
                {
                    _timerText.transform.localScale = Vector3.one;
                }
            }
        }

        private IEnumerator PulseEffect()
        {
            while (true)
            {
                // Пульсация от 0.9 до 1.1 размера
                float pulse = Mathf.PingPong(Time.time * 2f, 0.2f) + 0.9f;
                if (_timerText != null)
                {
                    _timerText.transform.localScale = Vector3.one * pulse;
                }
                yield return null;
            }
        }

        // Метод для ручного обновления (например, при паузе/возобновлении)
        public void ForceUpdate()
        {
            if (TaskManager.Instance != null && _hasActiveTask)
            {
                var task = TaskManager.Instance.GetCurrentTask();
                if (task != null)
                {
                    UpdateTimerDisplay(task.TimeRemaining);
                }
            }
        }
    }
}
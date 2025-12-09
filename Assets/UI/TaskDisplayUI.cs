using UnityEngine;
using TMPro;
using TaskSystem;
using System.Collections;

namespace UI
{
    public class TaskDisplayUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _taskText;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _fadeInDuration = 0.5f;
        [SerializeField] private float _fadeOutDuration = 0.3f;
        [SerializeField] private Color _normalColor = new Color(1f, 0.8f, 0f, 1f); // Золотой цвет
        [SerializeField] private Color _urgentColor = new Color(1f, 0.2f, 0.2f, 1f); // Красный для срочных
        [SerializeField] private Color _corruptedColor = new Color(1f, 1f, 0f, 1f); // Желтый для измененных

         [Header("Связь с FireText")]
        [SerializeField] private Shaders.ScreenEffects.Fire_text _fireTextEffect;

        [Header("Эффекты")]
        [SerializeField] private bool _enableGlow = true;
        [SerializeField] private float _glowIntensity = 0.8f;
        [SerializeField] private float _pulseSpeed = 1.5f;
        
        private bool _isSubscribed = false;
        private bool _isVisible = false;
        private Coroutine _fadeCoroutine;
        private Coroutine _pulseCoroutine;
        private BureaucraticTask _pendingTask = null; // Задание в ожидании
        private bool _waitingForDialogueEnd = false; // Флаг ожидания окончания диалога
        
        void Start()
        {
            // Начинаем с прозрачного текста
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.gameObject.SetActive(false);
            }
            
            // Находим FireText, если не назначен
            if (_fireTextEffect == null)
            {
                _fireTextEffect = FindAnyObjectByType<Shaders.ScreenEffects.Fire_text>();
            }
            
            // Ждем перед подпиской
            Invoke(nameof(Initialize), 0.5f);
        }
        
        void OnEnable()
        {
            if (!_isSubscribed)
            {
                Initialize();
            }
        }
        
        void OnDisable()
        {
            UnsubscribeFromEvents();
        }
        
        void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        void Update()
        {
            // Проверяем, закончился ли диалог зайца, если мы ждем
            if (_waitingForDialogueEnd && !IsBunnyDialogueActive())
            {
                _waitingForDialogueEnd = false;
                
                // Если есть задание в ожидании, показываем его
                if (_pendingTask != null)
                {
                    ShowTask(_pendingTask);
                    _pendingTask = null;
                }
            }
        }
        
        private void Initialize()
        {
            // Подписываемся на события TaskManager
            if (TaskManager.Instance != null)
            {
                SubscribeToEvents();
                
                // Проверяем, есть ли уже активное задание
                var currentTask = TaskManager.Instance.GetCurrentTask();
                if (currentTask != null && TaskManager.Instance.IsTaskActive)
                {
                    // Проверяем, активен ли диалог зайца
                    if (IsBunnyDialogueActive())
                    {
                        // Ждем окончания диалога
                        _pendingTask = currentTask;
                        _waitingForDialogueEnd = true;
                    }
                    else
                    {
                        // Диалог не активен, показываем сразу
                        ShowTask(currentTask);
                    }
                }
            }
            else
            {
                // Если TaskManager еще не готов, ждем
                Invoke(nameof(Initialize), 0.2f);
            }
        }
        
        private void SubscribeToEvents()
        {
            if (_isSubscribed || TaskManager.Instance == null) return;
            
            TaskManager.Instance.OnNewTask += OnNewTask;
            TaskManager.Instance.OnTaskCompleted += OnTaskCompleted;
            TaskManager.Instance.OnTaskFailed += OnTaskFailed;
            TaskManager.Instance.OnTaskCorrupted += OnTaskCorrupted;
            
            _isSubscribed = true;
        }
        
        private void UnsubscribeFromEvents()
        {
            if (!_isSubscribed || TaskManager.Instance == null) return;
            
            TaskManager.Instance.OnNewTask -= OnNewTask;
            TaskManager.Instance.OnTaskCompleted -= OnTaskCompleted;
            TaskManager.Instance.OnTaskFailed -= OnTaskFailed;
            TaskManager.Instance.OnTaskCorrupted -= OnTaskCorrupted;
            
            _isSubscribed = false;
        }
        
        private void OnNewTask(BureaucraticTask task)
        {
            ShowTask(task);
            Debug.Log($"<color=green>TaskDisplayUI: Новое задание получено: {task.Title}</color>");
        }
        
        private void OnTaskCompleted(BureaucraticTask task)
        {
            HideTask();
            // Сбрасываем ожидание
            _pendingTask = null;
            _waitingForDialogueEnd = false;
        }
        
        private void OnTaskFailed(BureaucraticTask task)
        {
            HideTask();
            // Сбрасываем ожидание
            _pendingTask = null;
            _waitingForDialogueEnd = false;
        }
        
        private void OnTaskCorrupted(BureaucraticTask task)
        {
            // Если задание уже отображается, обновляем его
            if (_isVisible)
            {
                UpdateTaskDisplay(task);
                StartPulseEffect(_corruptedColor, 3); // Пульсация при изменении
            }
            // Если задание в ожидании, обновляем его
            else if (_pendingTask != null)
            {
                _pendingTask = task;
            }
        }
        
        private void ShowTask(BureaucraticTask task)
        {
            if (task == null || _taskText == null) return;
            
            UpdateTaskDisplay(task);
             if (_fireTextEffect != null)
            {
               // _fireTextEffect.SetTargetText(_taskText.text);
            }
            if (_canvasGroup != null)
            {
                if (_fadeCoroutine != null)
                    StopCoroutine(_fadeCoroutine);
                
                _canvasGroup.gameObject.SetActive(true);
                _fadeCoroutine = StartCoroutine(FadeCanvasGroup(0f, 1f, _fadeInDuration));
            }
            
            _isVisible = true;
            
            // Запускаем эффект свечения
            if (_enableGlow)
            {
                StartGlowEffect();
            }
            
            Debug.Log($"<color=green>TaskDisplayUI: Показываю задание: {task.Title}</color>");
        }
        
        private void HideTask()
        {
            if (!_isVisible) return;
            
            if (_canvasGroup != null)
            {
                if (_fadeCoroutine != null)
                    StopCoroutine(_fadeCoroutine);
                
                _fadeCoroutine = StartCoroutine(FadeCanvasGroup(_canvasGroup.alpha, 0f, _fadeOutDuration, true));
            }
            
            StopGlowEffect();
            StopPulseEffect();
            
            _isVisible = false;
            Debug.Log("<color=yellow>TaskDisplayUI: Скрываю задание</color>");
        }
        
        private void UpdateTaskDisplay(BureaucraticTask task)
        {
            if (task == null || _taskText == null) return;
            
            // Форматируем описание задания
            string description = FormatTaskDescription(task);
            _taskText.text = description;
             if (_fireTextEffect != null)
            {
              //  _fireTextEffect.SetTargetText(description);
            }
            // Устанавливаем цвет в зависимости от типа задания
            if (task.IsCorrupted)
            {
                _taskText.color = _corruptedColor;
            }
            else if (task.IsUrgent)
            {
                _taskText.color = _urgentColor;
            }
            else
            {
                _taskText.color = _normalColor;
            }
        }
        
        private string FormatTaskDescription(BureaucraticTask task)
        {
            if (task == null) return "Задание не найдено";
            
            // Получаем чистое описание задания (без дедлайна и меток)
            // Описание уже содержит: "Заполнить {paper} {ink} чернилами. Подпись: {signature}. Штамп: {stamp}."
            
            // Можем добавить небольшие форматирование
            return task.Description.TrimEnd(' ', '.') + ".";
        }
        
        private bool IsBunnyDialogueActive()
        {
            // Ищем активный BunnyDialogueManager
            var bunnyDialogue = FindAnyObjectByType<Bunny.BunnyDialogueManager>();
            if (bunnyDialogue != null)
            {
                return bunnyDialogue.IsDialogueActive();
            }
            return false;
        }
        
        private IEnumerator FadeCanvasGroup(float startAlpha, float endAlpha, float duration, bool disableAfterFade = false)
        {
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            _canvasGroup.alpha = endAlpha;
            
            if (disableAfterFade && _canvasGroup != null)
            {
                _canvasGroup.gameObject.SetActive(false);
            }
            
            _fadeCoroutine = null;
        }
        
        private void StartGlowEffect()
        {
            if (_pulseCoroutine != null)
                StopCoroutine(_pulseCoroutine);
            
            _pulseCoroutine = StartCoroutine(GlowPulseEffect());
        }
        
        private void StopGlowEffect()
        {
            if (_pulseCoroutine != null)
            {
                StopCoroutine(_pulseCoroutine);
                _pulseCoroutine = null;
            }
            
            if (_taskText != null)
            {
                _taskText.fontMaterial.SetFloat(ShaderUtilities.ID_GlowPower, 0f);
            }
        }
        
        private IEnumerator GlowPulseEffect()
        {
            while (true)
            {
                float glow = (Mathf.Sin(Time.time * _pulseSpeed) * 0.5f + 0.5f) * _glowIntensity;
                
                if (_taskText != null)
                {
                    _taskText.fontMaterial.SetFloat(ShaderUtilities.ID_GlowPower, glow);
                }
                
                yield return null;
            }
        }
        
        private void StartPulseEffect(Color pulseColor, int pulseCount)
        {
            if (_pulseCoroutine != null)
                StopCoroutine(_pulseCoroutine);
            
            _pulseCoroutine = StartCoroutine(ColorPulseEffect(pulseColor, pulseCount));
        }
        
        private void StopPulseEffect()
        {
            if (_pulseCoroutine != null)
            {
                StopCoroutine(_pulseCoroutine);
                _pulseCoroutine = null;
            }
        }
        
        private IEnumerator ColorPulseEffect(Color pulseColor, int pulseCount)
        {
            Color originalColor = _taskText.color;
            
            for (int i = 0; i < pulseCount; i++)
            {
                // Плавно меняем на цвет пульса
                float elapsedTime = 0f;
                while (elapsedTime < 0.2f)
                {
                    _taskText.color = Color.Lerp(originalColor, pulseColor, elapsedTime / 0.2f);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                
                // Возвращаем обратно
                elapsedTime = 0f;
                while (elapsedTime < 0.2f)
                {
                    _taskText.color = Color.Lerp(pulseColor, originalColor, elapsedTime / 0.2f);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                
                yield return new WaitForSeconds(0.1f);
            }
            
            // Восстанавливаем нормальный цвет
            _taskText.color = originalColor;
            _pulseCoroutine = null;
            
            // Возобновляем обычное свечение
            if (_enableGlow)
            {
                StartGlowEffect();
            }
        }
        
        // Публичный метод для ручного обновления (например, при паузе)
        public void ForceUpdate()
        {
            if (TaskManager.Instance != null && _isVisible)
            {
                var task = TaskManager.Instance.GetCurrentTask();
                if (task != null)
                {
                    UpdateTaskDisplay(task);
                }
            }
        }
        
        // Публичный метод для принудительного показа (например, из других скриптов)
        public void ForceShowCurrentTask()
        {
            if (TaskManager.Instance != null)
            {
                var task = TaskManager.Instance.GetCurrentTask();
                if (task != null && !task.IsCompleted && !task.IsFailed)
                {
                    ShowTask(task);
                }
            }
        }
    }
}
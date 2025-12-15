using UnityEngine;
using TMPro;
using TaskSystem;
using System.Collections;

// * Жаркова Т.В.

namespace UI
{
    /// <summary>
    /// Компонент для отображения текущего бюрократического задания в пользовательском интерфейсе.
    /// Управляет отображением текста задания, его цветом в зависимости от статуса (срочное, измененное)
    /// и эффектами плавного появления/исчезновения и свечения.
    /// </summary>
    public class TaskDisplayUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _taskText;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _fadeInDuration = 0.5f;
        [SerializeField] private float _fadeOutDuration = 0.3f;
        [SerializeField] private Color _normalColor = new Color(1f, 0.8f, 0f, 1f); // Золотой цвет
        [SerializeField] private Color _urgentColor = new Color(1f, 0.2f, 0.2f, 1f); // Красный для срочных
        [SerializeField] private Color _corruptedColor = new Color(1f, 1f, 0f, 1f); // Желтый для измененных

         /// <summary>
         /// Ссылка на эффект "горящего текста" (FireText).
         /// </summary>
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
        
        /// <summary>
        /// Вызывается при запуске сцены. Инициализирует состояние UI и запускает процесс подписки.
        /// </summary>
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
        
        /// <summary>
        /// Вызывается при включении объекта. Повторно подписывается на события, если это необходимо.
        /// </summary>
        void OnEnable()
        {
            if (!_isSubscribed)
            {
                Initialize();
            }
        }
        
        /// <summary>
        /// Вызывается при выключении объекта. Отписывается от событий TaskManager.
        /// </summary>
        void OnDisable()
        {
            UnsubscribeFromEvents();
        }
        
        /// <summary>
        /// Вызывается при уничтожении объекта. Отписывается от событий TaskManager.
        /// </summary>
        void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        /// <summary>
        /// Вызывается каждый кадр. Проверяет, закончился ли диалог, чтобы отобразить задание в ожидании.
        /// </summary>
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
        
        /// <summary>
        /// Инициализирует компонент, пытаясь получить доступ к TaskManager и подписаться на его события.
        /// </summary>
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
        
        /// <summary>
        /// Подписывается на события TaskManager.
        /// </summary>
        private void SubscribeToEvents()
        {
            if (_isSubscribed || TaskManager.Instance == null) return;
            
            TaskManager.Instance.OnNewTask += OnNewTask;
            TaskManager.Instance.OnTaskCompleted += OnTaskCompleted;
            TaskManager.Instance.OnTaskFailed += OnTaskFailed;
            TaskManager.Instance.OnTaskCorrupted += OnTaskCorrupted;
            
            _isSubscribed = true;
        }
        
        /// <summary>
        /// Отписывается от событий TaskManager.
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (!_isSubscribed || TaskManager.Instance == null) return;
            
            TaskManager.Instance.OnNewTask -= OnNewTask;
            TaskManager.Instance.OnTaskCompleted -= OnTaskCompleted;
            TaskManager.Instance.OnTaskFailed -= OnTaskFailed;
            TaskManager.Instance.OnTaskCorrupted -= OnTaskCorrupted;
            
            _isSubscribed = false;
        }
        
        /// <summary>
        /// Обработчик события появления нового задания.
        /// </summary>
        /// <param name="task">Новое задание.</param>
        private void OnNewTask(BureaucraticTask task)
        {
            ShowTask(task);
            Debug.Log($"<color=green>TaskDisplayUI: Новое задание получено: {task.Title}</color>");
        }
        
        /// <summary>
        /// Обработчик события успешного завершения задания.
        /// </summary>
        /// <param name="task">Завершенное задание.</param>
        private void OnTaskCompleted(BureaucraticTask task)
        {
            HideTask();
            // Сбрасываем ожидание
            _pendingTask = null;
            _waitingForDialogueEnd = false;
        }
        
        /// <summary>
        /// Обработчик события провала задания.
        /// </summary>
        /// <param name="task">Проваленное задание.</param>
        private void OnTaskFailed(BureaucraticTask task)
        {
            HideTask();
            // Сбрасываем ожидание
            _pendingTask = null;
            _waitingForDialogueEnd = false;
        }
        
        /// <summary>
        /// Обработчик события изменения (порчи) задания.
        /// </summary>
        /// <param name="task">Измененное задание.</param>
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
        
        /// <summary>
        /// Отображает указанное задание в UI с эффектом плавного появления.
        /// </summary>
        /// <param name="task">Задание для отображения.</param>
        private void ShowTask(BureaucraticTask task)
        {
            if (task == null || _taskText == null) return;
            
            UpdateTaskDisplay(task);
            
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
        
        /// <summary>
        /// Скрывает текущее задание из UI с эффектом плавного исчезновения.
        /// </summary>
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
        
        /// <summary>
        /// Обновляет текстовое содержимое и цвет задания в UI.
        /// </summary>
        /// <param name="task">Задание, отображаемое в данный момент.</param>
        private void UpdateTaskDisplay(BureaucraticTask task)
        {
            if (task == null || _taskText == null) return;
            
            // Форматируем описание задания
            string description = FormatTaskDescription(task);
            _taskText.text = description;
            
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
        
        /// <summary>
        /// Форматирует описание задания для отображения.
        /// </summary>
        /// <param name="task">Задание для форматирования.</param>
        /// <returns>Отформатированная строка описания.</returns>
        private string FormatTaskDescription(BureaucraticTask task)
        {
            if (task == null) return "Задание не найдено";
            
            // Описание уже содержит: "Заполнить {paper} {ink} чернилами. Подпись: {signature}. Штамп: {stamp}."
            
            // Добавляем завершающую точку, если ее нет, и убираем лишние пробелы.
            return task.Description.TrimEnd(' ', '.') + ".";
        }
        
        /// <summary>
        /// Проверяет, активен ли диалог "зайца" (BunnyDialogueManager).
        /// </summary>
        /// <returns>True, если диалог активен, иначе False.</returns>
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
        
        /// <summary>
        /// Корутина для плавного изменения прозрачности (alpha) CanvasGroup.
        /// </summary>
        /// <param name="startAlpha">Начальное значение alpha.</param>
        /// <param name="endAlpha">Конечное значение alpha.</param>
        /// <param name="duration">Длительность перехода.</param>
        /// <param name="disableAfterFade">Отключить GameObject после завершения исчезновения.</param>
        /// <returns>IEnumerator для корутины.</returns>
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
        
        /// <summary>
        /// Запускает эффект пульсирующего свечения для текста.
        /// </summary>
        private void StartGlowEffect()
        {
            if (_pulseCoroutine != null)
                StopCoroutine(_pulseCoroutine);
            
            _pulseCoroutine = StartCoroutine(GlowPulseEffect());
        }
        
        /// <summary>
        /// Останавливает эффект пульсирующего свечения и сбрасывает силу свечения.
        /// </summary>
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
        
        /// <summary>
        /// Корутина для создания эффекта пульсирующего свечения текста.
        /// </summary>
        /// <returns>IEnumerator для корутины.</returns>
        private IEnumerator GlowPulseEffect()
        {
            while (true)
            {
                // Вычисляем силу свечения на основе синусоиды
                float glow = (Mathf.Sin(Time.time * _pulseSpeed) * 0.5f + 0.5f) * _glowIntensity;
                
                if (_taskText != null)
                {
                    _taskText.fontMaterial.SetFloat(ShaderUtilities.ID_GlowPower, glow);
                }
                
                yield return null;
            }
        }
        
        /// <summary>
        /// Запускает эффект кратковременного пульсации цвета.
        /// </summary>
        /// <param name="pulseColor">Цвет пульсации.</param>
        /// <param name="pulseCount">Количество циклов пульсации.</param>
        private void StartPulseEffect(Color pulseColor, int pulseCount)
        {
            if (_pulseCoroutine != null)
                StopCoroutine(_pulseCoroutine);
            
            _pulseCoroutine = StartCoroutine(ColorPulseEffect(pulseColor, pulseCount));
        }
        
        /// <summary>
        /// Останавливает текущий эффект пульсации.
        /// </summary>
        private void StopPulseEffect()
        {
            if (_pulseCoroutine != null)
            {
                StopCoroutine(_pulseCoroutine);
                _pulseCoroutine = null;
            }
        }
        
        /// <summary>
        /// Корутина для создания эффекта кратковременной пульсации цвета текста.
        /// </summary>
        /// <param name="pulseColor">Цвет для пульсации.</param>
        /// <param name="pulseCount">Количество циклов пульсации.</param>
        /// <returns>IEnumerator для корутины.</returns>
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
        
        /// <summary>
        /// Публичный метод для принудительного обновления отображения текущего задания.
        /// </summary>
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
        
        /// <summary>
        /// Публичный метод для принудительного отображения текущего задания.
        /// </summary>
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
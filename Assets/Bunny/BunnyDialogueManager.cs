using UnityEngine;
using DialogueManager;
using TaskSystem;
using System.Collections;

namespace Bunny 
{
    public class BunnyDialogueManager : DialogueManager.DialogueManager
    {
        private global::Bunny.Bunny _activeBunny;
        private Coroutine _timerUpdateCoroutine;
        private BureaucraticTask _currentDisplayedTask;
        private bool _isTaskDialogue = false;
        
        protected override void OnSentencePrinted()
        {
            base.OnSentencePrinted();
            
            // [!] Запускаем обновление времени ТОЛЬКО ПОСЛЕ того, как текст напечатан
            if (_isTaskDialogue && _currentDisplayedTask != null)
            {
                StartTaskTimerUpdates();
            }
        }
        
        public void StartBunnyDialogue(Dialogue dialogue, global::Bunny.Bunny bunny)
        {
            Debug.Log($"BunnyDialogueManager: StartBunnyDialogue called with bunny {bunny}");
            
            UseTimerForClosing = true;
            
            _activeBunny = bunny;
            _isTaskDialogue = true;
            
            // [!] Получаем задание ДО начала диалога
            if (TaskManager.Instance != null)
            {
                _currentDisplayedTask = TaskManager.Instance.GetCurrentTask();
                
                // [!] Если задания нет, создаем новое
                if (_currentDisplayedTask == null)
                {
                    TaskManager.Instance.StartNewTask();
                    _currentDisplayedTask = TaskManager.Instance.GetCurrentTask();
                }
            }
            
            // Показываем textCloud перед началом диалога
            if (textCloud != null)
            {
                textCloud.SetActive(true);
                Debug.Log("BunnyDialogueManager: TextCloud activated");
            }
            
            base.StartDialogue(dialogue);
            
            // [!] НЕ запускаем обновление времени здесь - дождемся окончания печати текста
            // [!] Подписываемся на события завершения задания для автоматического закрытия
            if (TaskManager.Instance != null)
            {
                TaskManager.Instance.OnTaskCompleted += OnTaskEnded;
                TaskManager.Instance.OnTaskFailed += OnTaskEnded;
                TaskManager.Instance.OnTaskCorrupted += OnTaskCorruptedHandler;
            }
        }
        
        // [!] Обработчик изменения задания
        private void OnTaskCorruptedHandler(BureaucraticTask task)
        {
            // Обновляем отображаемое задание
            if (TaskManager.Instance != null)
            {
                _currentDisplayedTask = TaskManager.Instance.GetCurrentTask();
            }
        }
        
        public override void EndDialogue()
        {
            Debug.Log("BunnyDialogueManager: EndDialogue called");
            
            UseTimerForClosing = false;
            _isTaskDialogue = false;
            
            // [!] Останавливаем обновление времени
            StopTaskTimerUpdates();
            
            // [!] Отписываемся от событий завершения задания
            if (TaskManager.Instance != null)
            {
                TaskManager.Instance.OnTaskCompleted -= OnTaskEnded;
                TaskManager.Instance.OnTaskFailed -= OnTaskEnded;
                TaskManager.Instance.OnTaskCorrupted -= OnTaskCorruptedHandler;
            }
            
            base.EndDialogue();
            
            if (_activeBunny != null)
            {
                _activeBunny.CurrentDialogueIndex++;
                _activeBunny = null;
            }
            _currentDisplayedTask = null;
        }
        
        // [!] Обработчик завершения задания (успешного или проваленного)
        private void OnTaskEnded(BureaucraticTask task)
        {
            Debug.Log($"BunnyDialogueManager: Задание завершено ({task?.Title}), закрываю диалог");
            
            // Если диалог еще активен, закрываем его
            if (IsDialogueActive())
            {
                EndDialogue();
            }
        }
        
        // [!] Переопределяем CheckDialogueEnd для увеличения времени отображения
        protected override void CheckDialogueEnd()
        {
            if (continueText == null) return;
            
            if(_isPermanentDisplay)
            {
                continueText.gameObject.SetActive(false);
                return;
            }
            
            if (sentences.Count == 0)
            {
                // Последнее предложение
                if (!_useTimerForClosing)
                {
                    continueText.text = "Нажмите ПРОБЕЛ, чтобы закрыть диалог.";
                    continueText.gameObject.SetActive(true);
                }
                else
                {
                    // [!] Увеличиваем время отображения до 60 секунд вместо 10
                    continueText.gameObject.SetActive(false);
                    _currentTimer = 10f; // [!] Увеличенное время
                }
            }
            else
            {
                // Не последнее предложение
                if (!_useTimerForClosing)
                {
                    continueText.text = "Нажмите ПРОБЕЛ для продолжения...";
                    continueText.gameObject.SetActive(true);
                }
                else
                {
                    // [!] Увеличиваем время отображения до 60 секунд вместо 10
                    continueText.gameObject.SetActive(false);
                    _currentTimer = 10f; // [!] Увеличенное время
                }
            }
        }
        
        // [!] Новый метод для обновления времени задания
        private void StartTaskTimerUpdates()
        {
            StopTaskTimerUpdates();
            
            if (_currentDisplayedTask != null)
            {
                _timerUpdateCoroutine = StartCoroutine(UpdateTaskTimerRoutine());
            }
        }
        
        // [!] Остановка обновления времени
        private void StopTaskTimerUpdates()
        {
            if (_timerUpdateCoroutine != null)
            {
                StopCoroutine(_timerUpdateCoroutine);
                _timerUpdateCoroutine = null;
            }
        }
        
        // [!] Корутина для обновления времени
        private IEnumerator UpdateTaskTimerRoutine()
        {
            while (_currentDisplayedTask != null && 
                   !_currentDisplayedTask.IsCompleted && 
                   !_currentDisplayedTask.IsFailed)
            {
                // [!] Обновляем описание задания с текущим временем
                UpdateDialogueWithCurrentTime();
                yield return new WaitForSeconds(1f); // Обновляем каждую секунду
            }
        }
        
        // [!] Обновление диалога с актуальным временем
        private void UpdateDialogueWithCurrentTime()
        {
            if (_currentDisplayedTask == null || dialogueText == null) return;
            
            // [!] Форматируем текст с текущим временем
            string descriptionWithTime = GetTaskDescriptionWithTime(_currentDisplayedTask);
            
            // [!] Обновляем текст только если он изменился
            if (dialogueText.text != descriptionWithTime)
            {
                dialogueText.text = descriptionWithTime;
            }
        }
        
        // ========== ЛОГИКА ОТОБРАЖЕНИЯ ЗАДАНИЯ ==========
        public string GetTaskDescriptionForDialogue()
        {
            Debug.Log("BunnyDialogueManager: GetTaskDescriptionForDialogue called");
            
            if (TaskManager.Instance == null)
            {
                Debug.LogWarning("TaskManager не найден!");
                return "Ошибка: система заданий не найдена";
            }
            
            var currentTask = TaskManager.Instance.GetCurrentTask();
            Debug.Log($"BunnyDialogueManager: Current task is {(currentTask == null ? "null" : currentTask.Title)}");
            
            // Если задания нет, создаем новое
            if (currentTask == null)
            {
                Debug.Log("BunnyDialogueManager: No current task, starting new task");
                TaskManager.Instance.StartNewTask();
                currentTask = TaskManager.Instance.GetCurrentTask();
                
                if (currentTask == null)
                {
                    return "Новое задание создается...";
                }
            }
            
            // [!] Получаем описание задания БЕЗ времени (время добавится позже)
            string description = GetTaskDescriptionWithoutTime(currentTask);
            
            // Добавляем срочность, если задание срочное
            if (currentTask.IsUrgent)
            {
                description = $"<color=red>СРОЧНО!!!</color> " + description;
            }
            
            // Добавляем пометку об изменении, если задание испорчено
            if (currentTask.IsCorrupted)
            {
                description = $"<color=yellow>! ВНИМАНИЕ: Заяц изменил требования!</color> " + description;
            }
            
            Debug.Log($"BunnyDialogueManager: Task description: {description}");
            return description;
        }
        
        // [!] Новый метод: получает описание задания БЕЗ времени
        private string GetTaskDescriptionWithoutTime(BureaucraticTask task)
        {
            if (task == null) return "Задание не найдено";
            
            // [!] Возвращаем только базовое описание без времени
            return task.Description;
        }
        
        // [!] Метод: получает описание задания С временем
        private string GetTaskDescriptionWithTime(BureaucraticTask task)
        {
            if (task == null) return "Задание не найдено";
            
            // [!] Форматируем оставшееся время
            string timeText = FormatTime(task.TimeRemaining);
            
            // [!] Берем базовое описание из задания и добавляем текущее время
            return $"{task.Description}Дедлайн: {timeText}";
        }
        
        // [!] Метод форматирования времени
        private string FormatTime(float timeInSeconds)
        {
            if (timeInSeconds <= 0) return "Время вышло!";
            
            int minutes = Mathf.FloorToInt(timeInSeconds / 60);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60);
            return $"{minutes:00}:{seconds:00}";
        }
        
        // Метод для Bunny.cs для получения диалога с заданием
        public Dialogue GetTaskDialogueForBunny(global::Bunny.Bunny bunny)
        {
            Debug.Log($"BunnyDialogueManager: GetTaskDialogueForBunny called for {bunny.BunnyName}");
            
            string taskDescription = GetTaskDescriptionForDialogue();
            
            if (string.IsNullOrEmpty(taskDescription))
            {
                taskDescription = "Новое задание будет готово через мгновение...";
            }
            
            Dialogue dialogue = new Dialogue
            {
                name = bunny.BunnyName,
                sentences = new string[] { taskDescription }
            };
            
            Debug.Log($"BunnyDialogueManager: Created dialogue with sentence: {taskDescription}");
            return dialogue;
        }
    }
}
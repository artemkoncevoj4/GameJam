using UnityEngine;
using DialogueManager;
using TaskSystem;
using System.Collections;

namespace Bunny 
{
    public class BunnyDialogueManager : DialogueManager.DialogueManager
    {
        private global::Bunny.Bunny _activeBunny;
        private bool _isTaskDialogue = false;
        
        protected override void OnSentencePrinted()
        {
            base.OnSentencePrinted();
        }
        
        public void StartBunnyDialogue(Dialogue dialogue, global::Bunny.Bunny bunny)
        {
            Debug.Log($"BunnyDialogueManager: StartBunnyDialogue called with bunny {bunny}");
            
            UseTimerForClosing = true;
            
            _activeBunny = bunny;
            _isTaskDialogue = true;
            
            // Показываем textCloud перед началом диалога
            if (textCloud != null)
            {
                textCloud.SetActive(true);
                Debug.Log("BunnyDialogueManager: TextCloud activated");
            }
            
            base.StartDialogue(dialogue);
            
            // Подписываемся на события завершения задания для автоматического закрытия
            if (TaskManager.Instance != null)
            {
                TaskManager.Instance.OnTaskCompleted += OnTaskEnded;
                TaskManager.Instance.OnTaskFailed += OnTaskEnded;
            }
        }
        
        public override void EndDialogue()
        {
            Debug.Log("BunnyDialogueManager: EndDialogue called");
            
            UseTimerForClosing = false;
            _isTaskDialogue = false;
            
            // Отписываемся от событий завершения задания
            if (TaskManager.Instance != null)
            {
                TaskManager.Instance.OnTaskCompleted -= OnTaskEnded;
                TaskManager.Instance.OnTaskFailed -= OnTaskEnded;
            }
            
            base.EndDialogue();
            
            if (_activeBunny != null)
            {
                _activeBunny.CurrentDialogueIndex++;
                _activeBunny = null;
            }
        }
        
        // Обработчик завершения задания (успешного или проваленного)
        private void OnTaskEnded(BureaucraticTask task)
        {
            Debug.Log($"BunnyDialogueManager: Задание завершено ({task?.Title}), закрываю диалог");
            
            // Если диалог еще активен, закрываем его
            if (IsDialogueActive())
            {
                EndDialogue();
            }
        }
        
        // Переопределяем CheckDialogueEnd для увеличения времени отображения
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
                    // Увеличиваем время отображения до 10 секунд
                    continueText.gameObject.SetActive(false);
                    _currentTimer = 10f;
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
                    // Увеличиваем время отображения до 10 секунд
                    continueText.gameObject.SetActive(false);
                    _currentTimer = 10f;
                }
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
            
            // Получаем описание задания с фиксированным временем
            string description = GetTaskDescriptionWithFixedTime(currentTask);
            
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
        
        // Метод: получает описание задания с фиксированным временем (не обновляется)
        private string GetTaskDescriptionWithFixedTime(BureaucraticTask task)
        {
            if (task == null) return "Задание не найдено";
            
            // Форматируем начальное время задания (фиксированное, не обновляется)
            string timeText = FormatTime(task.TimeRemaining);
            
            // Берем базовое описание из задания и добавляем фиксированное время
            return $"{task.Description}Дедлайн: {timeText}";
        }
        
        // Метод форматирования времени
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
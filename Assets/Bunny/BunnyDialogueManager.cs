using UnityEngine;
using DialogueManager;
using TaskSystem;

namespace Bunny 
{
    public class BunnyDialogueManager : DialogueManager.DialogueManager
    {
        private global::Bunny.Bunny _activeBunny;
        protected override void OnSentencePrinted()
        {
            base.OnSentencePrinted();
        }
        
        public void StartBunnyDialogue(Dialogue dialogue, global::Bunny.Bunny bunny)
        {
            Debug.Log($"BunnyDialogueManager: StartBunnyDialogue called with bunny {bunny}");
            
            UseTimerForClosing = true;
            if (GameCycle.Instance != null)
            {
                Debug.Log("BunnyDialogueManager: Pausing game");
            }
            
            _activeBunny = bunny;
            
            // Показываем textCloud перед началом диалога
           if (textCloud != null)
            {
                textCloud.SetActive(true);
                Debug.Log("BunnyDialogueManager: TextCloud activated");
            }
            
            base.StartDialogue(dialogue);
        }
        
        public override void EndDialogue()
        {
            Debug.Log("BunnyDialogueManager: EndDialogue called");
            
            UseTimerForClosing = false;
            base.EndDialogue();
            
            if (_activeBunny != null)
            {
                _activeBunny.CurrentDialogueIndex++;
                _activeBunny = null;
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
            
            // Получаем описание задания
            string description = currentTask.Description;
            
            // Добавляем срочность, если задание срочное
            if (currentTask.IsUrgent)
            {
                description = $"<color=red>СРОЧНО!!!</color>\n" + description;
            }
            
            // Добавляем пометку об изменении, если задание испорчено
            if (currentTask.IsCorrupted)
            {
                description = $"<color=yellow>! ВНИМАНИЕ: Заяц изменил требования!</color>\n" + description;
            }
            
            Debug.Log($"BunnyDialogueManager: Task description: {description}");
            return description;
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
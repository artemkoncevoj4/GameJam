using UnityEngine;
using DialogueManager;
using TaskSystem;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System;

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
            
            // [!] ИЗМЕНЕНО: Используем стандартное форматирование времени
            string timeText = FormatTime(currentTask.TimeRemaining);
            
            string description = $"{currentTask.Description}Дедлайн: {timeText}";
            
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

        // [!] НОВОЕ: Восстановлен метод форматирования времени (нужен для GetTaskDescriptionForDialogue)
        private string FormatTime(float timeInSeconds)
        {
            if (timeInSeconds <= 0) return "Время вышло!";
            
            int minutes = Mathf.FloorToInt(timeInSeconds / 60);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60);
            return $"{minutes:00}:{seconds:00}";
        }
        
        // Метод форматирования времени
        
        // Метод для Bunny.cs для получения диалога с заданием
        public Dialogue GetTaskDialogueForBunny(global::Bunny.Bunny bunny)
        {
            Debug.Log($"BunnyDialogueManager: GetTaskDialogueForBunny called for {bunny.BunnyName}");
            
            string taskDescription = GetTaskDescriptionForDialogue();
            
            if (string.IsNullOrEmpty(taskDescription))
            {
                taskDescription = "Новое задание будет готово через мгновение...";
            }
            
            // [!] НОВОЕ: Получаем дразнящую фразу вместо сухого описания
            string tauntingPhrase = GetTauntingTaskPhrase();
            
            Dialogue dialogue = new Dialogue
            {
                name = bunny.BunnyName,
                sentences = new string[] { tauntingPhrase } // [!] Используем дразнящую фразу
            };
            
            Debug.Log($"BunnyDialogueManager: Created dialogue with sentence: {tauntingPhrase}");
            return dialogue;
        }

        // [!] НОВЫЙ МЕТОД: Генерация дразнящей фразы с заданием
        private string GetTauntingTaskPhrase()
        {
            if (TaskManager.Instance == null)
            {
                return "Хе-хе, система заданий сломана!";
            }
            
            var currentTask = TaskManager.Instance.GetCurrentTask();
            
            if (currentTask == null)
            {
                return "Хм, кажется, у тебя нет задания... Как скучно!";
            }
            
            // Получаем базовое описание задания
            string taskDescription = GetTaskDescriptionForDialogue();
            
            // Выбираем случайный шаблон дразнилки
            string tauntTemplate = GetRandomTauntTemplate();
            
            // Заменяем плейсхолдеры на детали задания
            string finalPhrase = FormatTauntPhrase(tauntTemplate, currentTask, taskDescription);
            
            return finalPhrase;
        }

        // [!] НОВЫЙ МЕТОД: Случайный выбор шаблона дразнилки
        private string GetRandomTauntTemplate()
        {
            // Различные шаблоны дразнилок с разным настроением
            string[] tauntTemplates = {
                "Ха-ха! Тебе нужно сделать: {TASK}. Думаешь, справишься? {EMOJI}",
                "Ох, какое сложное задание! {TASK} Тебе точно по силам? {EMOJI}",
                "Смотри-ка, что у меня для тебя: {TASK}. Не запутайся! {EMOJI}",
                "Мне нравится смотреть, как ты пытаешься сделать {SHORT_TASK}... Удачи! {EMOJI}",
                "Знаешь, я мог бы сделать {SHORT_TASK} быстрее! Но ты попробуй... {EMOJI}",
                "Опять бумажная работа? {TASK} Как же тебе не повезло! {EMOJI}",
                "Твои любимые чернила и бумага! Нужно: {SHORT_TASK}. Веселись! {EMOJI}",
                "Я бы помог, но... нет. Сделай {SHORT_TASK} сам! {EMOJI}",
                "Специально для тебя: {TASK}. Не благодари! {EMOJI}",
                "Помнишь, как ненавидел {SIMILAR_TASK}? А теперь вот: {SHORT_TASK}! {EMOJI}"
            };
            
            return tauntTemplates[UnityEngine.Random.Range(0, tauntTemplates.Length)];
        }

        // [!] НОВЫЙ МЕТОД: Форматирование дразнящей фразы с подстановкой значений
        private string FormatTauntPhrase(string template, BureaucraticTask task, string taskDescription)
        {
            // Извлекаем ключевые элементы задания для подстановки
            string shortTask = ExtractShortTaskDescription(task);
            string similarTask = GetSimilarTaskReference(task);
            
            // Эмодзи для разных ситуаций
            string emoji = GetRandomEmoji(task);
            
            // Заменяем плейсхолдеры
            string result = template
                .Replace("{TASK}", taskDescription)
                .Replace("{SHORT_TASK}", shortTask)
                .Replace("{SIMILAR_TASK}", similarTask)
                .Replace("{EMOJI}", emoji);
            
            // Ограничиваем количество предложений (максимум 5)
            result = LimitSentences(result, 5);
            
            return result;
        }

        // [!] НОВЫЙ МЕТОД: Извлечение краткого описания задания
        private string ExtractShortTaskDescription(BureaucraticTask task)
        {
            if (task == null || task.Requirements == null) 
                return "что-то непонятное";
            
            var req = task.Requirements;
            string[] parts = new string[3];
            int index = 0;
            
            // Всегда добавляем бумагу и чернила
            parts[index++] = $"{req.requiredPaperType.ToString().ToLower()}";
            parts[index++] = $"{req.requiredInkColor.ToString().ToLower()} чернилами";
            
            // Случайно добавляем либо подпись, либо штамп
            if (UnityEngine.Random.value > 0.5f && req.isSigned)
                parts[index++] = $"подпись {req.requiredSignaturePos.ToString().ToLower()}";
            else if (req.isStamped)
                parts[index++] = $"штамп {req.requiredStampType.ToString().ToLower()}";
            
            // Собираем строку
            string result = string.Join(", ", parts, 0, index);
            
            // Добавляем время, если задание срочное
            if (task.IsUrgent)
                result += " СРОЧНО!";
            
            return result;
        }

        // [!] НОВЫЙ МЕТОД: Получение ссылки на похожее задание
        private string GetSimilarTaskReference(BureaucraticTask task)
        {
            // Список похожих заданий для разных типов
            var similarTasks = new Dictionary<string, string[]>
            {
                { "form", new[] { "формы 7-Б", "бланки АА-Я", "пергаменты", "карточки" } },
                { "ink", new[] { "черные чернила", "красные чернила", "зеленые чернила", "фиолетовые чернила" } },
                { "stamp", new[] { "штамп 'Одобрено'", "штамп 'Отклонено'", "официальную печать" } }
            };
            
            // Выбираем случайную ссылку на основе типа задания
            string[] references = similarTasks["form"];
            if (task.Requirements.requiredInkColor.ToString().Contains("Красные"))
                references = similarTasks["ink"];
            else if (task.Requirements.isStamped)
                references = similarTasks["stamp"];
            
            return references[UnityEngine.Random.Range(0, references.Length)];
        }

        // [!] НОВЫЙ МЕТОД: Выбор случайного эмодзи в зависимости от ситуации
        private string GetRandomEmoji(BureaucraticTask task)
        {
            if (task.IsCorrupted)
            {
                string[] corruptedEmojis = { "😈", "👹", "😏", "🦹", "🤪" };
                return corruptedEmojis[UnityEngine.Random.Range(0, corruptedEmojis.Length)];
            }
            else if (task.IsUrgent)
            {
                string[] urgentEmojis = { "🔥", "⏰", "🚨", "💥", "⚡" };
                return urgentEmojis[UnityEngine.Random.Range(0, urgentEmojis.Length)];
            }
            else
            {
                string[] normalEmojis = { "🐰", "😄", "🤭", "😉", "🃏", "🎭", "🤡" };
                return normalEmojis[UnityEngine.Random.Range(0, normalEmojis.Length)];
            }
        }

        // [!] НОВЫЙ МЕТОД: Ограничение количества предложений
        private string LimitSentences(string text, int maxSentences)
        {
            if (string.IsNullOrEmpty(text)) return text;
            
            // Разделяем на предложения
            char[] sentenceSeparators = { '.', '!', '?', ';' };
            string[] sentences = text.Split(sentenceSeparators, StringSplitOptions.RemoveEmptyEntries);
            
            // Ограничиваем количество
            if (sentences.Length > maxSentences)
            {
                sentences = sentences.Take(maxSentences).ToArray();
            }
            
            // Собираем обратно
            return string.Join(". ", sentences.Select(s => s.Trim())) + ".";
        }
    }
}
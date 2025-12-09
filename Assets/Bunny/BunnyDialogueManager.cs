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
                Debug.Log("<color=green>BunnyDialogueManager: TextCloud activated</color>");
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
            Debug.Log("<color=cyan>BunnyDialogueManager: EndDialogue called</color>");
            
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
            Debug.Log($"<color=green>BunnyDialogueManager: Задание завершено ({task?.Title}), закрываю диалог</color>");
            
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
            Debug.Log("<color=cyan>BunnyDialogueManager: GetTaskDescriptionForDialogue called</color>");
            
            if (TaskManager.Instance == null)
            {
                Debug.LogWarning("<color=red>TaskManager не найден!</color>");
                return "Ошибка: система заданий не найдена";
            }
            
            var currentTask = TaskManager.Instance.GetCurrentTask();
            Debug.Log($"BunnyDialogueManager: Current task is {(currentTask == null ? "null" : currentTask.Title)}");
            
            // Если задания нет, создаем новое
            if (currentTask == null)
            {
                Debug.Log("BunnyDialogueManager: No current task, returning default message");
                return "Новое задание создается...";
            }
            
            // [!] ИЗМЕНЕНО: Используем стандартное форматирование времени
            string timeText = FormatTime(currentTask.TimeRemaining);
            
             string shortDescription = $"{currentTask.Description} Время: {timeText}.";
    
            // Добавляем срочность, если задание срочное
            if (currentTask.IsUrgent)
            {
                shortDescription = $"СРОЧНО! {shortDescription}";
            }
            
            // Добавляем пометку об изменении, если задание испорчено
            if (currentTask.IsCorrupted)
            {
                shortDescription = $"ВНИМАНИЕ: Заяц изменил требования! {shortDescription}";
            }
            
            Debug.Log($"BunnyDialogueManager: Task description: {shortDescription}");
            return shortDescription;
        }

        // [!] НОВОЕ: Восстановлен метод форматирования времени (нужен для GetTaskDescriptionForDialogue)
        private string FormatTime(float timeInSeconds)
        {
            if (timeInSeconds <= 0) return "00:00";
            
            int minutes = Mathf.FloorToInt(timeInSeconds / 60);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60);
            return $"{minutes:00}:{seconds:00}";
        }
        
        // Метод форматирования времени
        
        // Метод для Bunny.cs для получения диалога с заданием
        public Dialogue GetTaskDialogueForBunny(global::Bunny.Bunny bunny)
        {
            Debug.Log($"<color=green>BunnyDialogueManager: GetTaskDialogueForBunny called for {bunny.BunnyName}</color>");
            
            string taskDescription = GetTaskDescriptionForDialogue();
            
            if (string.IsNullOrEmpty(taskDescription))
            {
                taskDescription = "Новое задание будет готово через мгновение...";
            }
            
            // [!] НОВОЕ: Получаем дразнящую фразу со сюжетными элементами
            string tauntingPhrase = GetEnhancedTauntingPhrase();
            
            Dialogue dialogue = new Dialogue
            {
                name = bunny.BunnyName,
                sentences = new string[] { tauntingPhrase }
            };
            
            Debug.Log($"<color=green>BunnyDialogueManager: Created dialogue with sentence: {tauntingPhrase}</color>");
            return dialogue;
        }

        // [!] НОВЫЙ МЕТОД: Улучшенная генерация дразнящих фраз
        private string GetEnhancedTauntingPhrase()
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
            
            // [!] НОВОЕ: Рандомное количество предложений от 4 до 7
            int sentenceCount = UnityEngine.Random.Range(4, 8);
            
            // [!] НОВОЕ: Определяем, будет ли сюжетная фраза (40% шанс)
            bool includeStory = UnityEngine.Random.value < 0.4f;
            
            // [!] НОВОЕ: Определяем, будет ли задание (всегда должно быть, но может быть в конце)
            bool includeTask = true;
            
            // [!] НОВОЕ: Собираем список всех возможных частей фразы
            List<string> phraseParts = new List<string>();
            
            // 1. Сначала сюжетная фраза (если есть)
            if (includeStory)
            {
                phraseParts.Add(GetRandomStoryPhrase());
            }
            
            // 2. Добавляем дразнилки (от 2 до 4 штук)
            int tauntCount = UnityEngine.Random.Range(2, Mathf.Min(5, sentenceCount - (includeStory ? 1 : 0) - 1));
            for (int i = 0; i < tauntCount; i++)
            {
                phraseParts.Add(GetRandomTauntTemplate());
            }
            
            // 3. Всегда добавляем задание в конце
            if (includeTask)
            {
                phraseParts.Add(GetTaskDescriptionForDialogue());
            }
            
            // [!] НОВОЕ: Ограничиваем общее количество предложений и соединяем
            phraseParts = LimitPhraseParts(phraseParts, sentenceCount);
            
            // [!] НОВОЕ: Добавляем эмодзи в конец
            string finalPhrase = string.Join(" ", phraseParts) + " " + GetRandomEmoji(currentTask);
            
            return finalPhrase;
        }

        // [!] НОВЫЙ МЕТОД: Ограничение количества частей фразы
        private List<string> LimitPhraseParts(List<string> parts, int maxSentences)
        {
            if (parts.Count <= maxSentences) return parts;
            
            // Оставляем первую (сюжетную) и последнюю (задание) части
            List<string> result = new List<string>();
            
            if (parts.Count > 0)
            {
                result.Add(parts[0]); // Первая часть (обычно сюжетная)
            }
            
            // Добавляем случайные дразнилки до ограничения
            int availableSlots = maxSentences - (result.Count + 1); // +1 для задания
            if (availableSlots > 0 && parts.Count > 1)
            {
                // Берем случайные дразнилки из середины списка
                List<string> taunts = parts.Skip(1).Take(parts.Count - 2).ToList();
                taunts = taunts.OrderBy(x => UnityEngine.Random.value).Take(availableSlots).ToList();
                result.AddRange(taunts);
            }
            
            // Всегда добавляем задание в конце
            if (parts.Count > 0)
            {
                result.Add(parts.Last());
            }
            
            return result;
        }

        // [!] НОВЫЙ МЕТОД: Случайные сюжетные фразы
        private string GetRandomStoryPhrase()
        {
            string[] storyPhrases = {
                "Знаешь, эта комната напоминает мне мою старую нору... только скучнее.",
                "Интересно, что будет, если я перегрызу все провода? Может, попробовать?",
                "Ты когда-нибудь задумывался, почему мы все здесь? Я — постоянно.",
                "Помнишь тот раз, когда я испортил все чернила? Это было весело!",
                "Мой дед всегда говорил: 'Не доверяй людям с бумагами'. Мудрый был кролик.",
                "Иногда мне кажется, что этот офис — одна большая клетка. Для всех нас.",
                "А ты знал, что морковку можно использовать как печать? Проверено!",
                "Когда-нибудь я расскажу тебе, откуда берутся эти задания... Или нет.",
                "Ш-ш-ш... Ты слышал эти звуки за стеной? Или это только мне кажется?",
                "Знаешь, что общего между бюрократией и морковкой? И то, и другое можно грызть!",
                "Мне снилось, что я стал начальником. Ужасный сон, правда?",
                "Ты не видел мои часы? Кажется, я их где-то оставил... Или украли?",
                "Интересно, что будет, если нажать ВСЕ кнопки сразу? Давай попробуем!",
                "Ты веришь в призраков офиса? Я — да. Один живет в копировальной машине.",
                "Мой хвост сегодня особенно пушистый. Это к неприятностям, знаешь ли.",
                "Когда-то я пытался вести себя прилично. Скучно было ужасно!",
                "А ты пробовал писать фиолетовыми чернилами? Это меняет мировоззрение.",
                "Знаешь, почему кролики такие быстрые? Чтобы убегать от таких заданий!",
                "Этот запах старой бумаги... Он напоминает мне что-то важное. Или нет.",
                "Иногда я заглядываю в окна других офисов. Там тоже скучно, но по-другому."
            };
            
            return storyPhrases[UnityEngine.Random.Range(0, storyPhrases.Length)];
        }

        // [!] НОВЫЙ МЕТОД: Больше шаблонов дразнилок
        private string GetRandomTauntTemplate()
        {
            string[] tauntTemplates = {
                "Ха-ха! Думаешь, справишься?",
                "Ох, какое сложное задание! Тебе точно по силам?",
                "Смотри-ка, что у меня для тебя! Не запутайся!",
                "Мне нравится смотреть, как ты пытаешься... Удачи!",
                "Знаешь, я мог бы сделать это быстрее! Но ты попробуй...",
                "Опять бумажная работа? Как же тебе не повезло!",
                "Твои любимые чернила и бумага! Веселись!",
                "Я бы помог, но... нет. Сделай сам!",
                "Специально для тебя! Не благодари!",
                "Помнишь, как ненавидел похожие задания? А теперь вот это!",
                "Ты так старательно работаешь! Жаль, что это бесполезно...",
                "Сколько времени ты уже здесь? Задумывался?",
                "Интересно, сколько заданий ты сможешь выполнить до конца дня?",
                "Ты когда-нибудь пробовал просто... уйти?",
                "Знаешь, что самое смешное? Это только начинается!",
                "Твои усилия такие милые... и бесполезные!",
                "Я ставлю на то, что ты не успеешь! Держу пари!",
                "Ты действительно думаешь, что это имеет значение?",
                "Когда-нибудь ты скажешь мне спасибо за такое развлечение!",
                "Смотри не перетрудись... Хотя, какая разница?"
            };
            
            return tauntTemplates[UnityEngine.Random.Range(0, tauntTemplates.Length)];
        }

        // [!] ОБНОВЛЕННЫЙ МЕТОД: Эмодзи с большим выбором
        private string GetRandomEmoji(BureaucraticTask task)
        {
            if (task.IsCorrupted)
            {
                string[] corruptedEmojis = { "😈", "👹", "😏", "🦹", "🤪", "😼", "🃏", "🎭", "🤡", "👻" };
                return corruptedEmojis[UnityEngine.Random.Range(0, corruptedEmojis.Length)];
            }
            else if (task.IsUrgent)
            {
                string[] urgentEmojis = { "🔥", "⏰", "🚨", "💥", "⚡", "💢", "‼️", "⚠️", "🎯", "💣" };
                return urgentEmojis[UnityEngine.Random.Range(0, urgentEmojis.Length)];
            }
            else
            {
                string[] normalEmojis = { "🐰", "😄", "🤭", "😉", "🃏", "🎭", "🤡", "👀", "🎩", "✨", "🌟", "💫", "🎪", "🎲" };
                return normalEmojis[UnityEngine.Random.Range(0, normalEmojis.Length)];
            }
        }
    }
}
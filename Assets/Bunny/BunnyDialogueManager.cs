using UnityEngine;
using DialogueManager;
using TaskSystem;
using System.Linq;
using System.Collections.Generic;

namespace Bunny 
{
    // Идрисов Д.С.

    /// <summary>
    /// Специализированный менеджер диалогов для кролика, управляющий отображением диалогов с заданиями.
    /// Наследует базовый функционал DialogueManager и расширяет его для работы с заданиями и хаотическими эффектами.
    /// </summary>
    public class BunnyDialogueManager : DialogueManager.DialogueManager
    {
        private global::Bunny.Bunny _activeBunny; // Текущий активный кролик
        public GameObject dialogueContainer; // Контейнер для элементов диалога
        
        /// <summary>
        /// Переопределяет обработку завершения печати предложения.
        /// </summary>
        protected override void OnSentencePrinted()
        {
            base.OnSentencePrinted();
        }
        
        /// <summary>
        /// Начинает диалог с кроликом, связанный с заданием.
        /// Настраивает визуальные элементы и таймеры для отображения задания.
        /// </summary>
        /// <param name="dialogue">Диалог для отображения.</param>
        /// <param name="bunny">Кролик, инициировавший диалог.</param>
        public void StartBunnyDialogue(Dialogue dialogue, global::Bunny.Bunny bunny)
        {
            Debug.Log($"<color=green>BunnyDialogueManager: StartBunnyDialogue called with bunny {bunny}</color>");
            
            UseTimerForClosing = true;
            
            _activeBunny = bunny;
            
            if (dialogueContainer != null)
            {
                dialogueContainer.SetActive(true);
                Debug.Log("<color=green>BunnyDialogueManager: DialogueContainer activated</color>");
            }
            else
            {
                Debug.LogWarning("<color=yellow>BunnyDialogueManager: DialogueContainer not assigned!</color>");
            }
            
            if (textCloud != null)
            {
                textCloud.SetActive(true);
                Debug.Log("<color=green>BunnyDialogueManager: TextCloud activated</color>");
            }
            
            base.StartDialogue(dialogue);
        
            if (TaskManager.Instance != null)
            {
                TaskManager.Instance.OnTaskCompleted += OnTaskEnded;
                TaskManager.Instance.OnTaskFailed += OnTaskEnded;
            }
        }
        
        /// <summary>
        /// Завершает диалог кролика, деактивирует визуальные элементы и отписывается от событий.
        /// </summary>
        public override void EndDialogue()
        {
            Debug.Log("<color=cyan>BunnyDialogueManager: EndDialogue called</color>");
            
            UseTimerForClosing = false;
            
            if (TaskManager.Instance != null)
            {
                TaskManager.Instance.OnTaskCompleted -= OnTaskEnded;
                TaskManager.Instance.OnTaskFailed -= OnTaskEnded;
            }
            
            if (dialogueContainer != null)
            {
                dialogueContainer.SetActive(false);
                Debug.Log("<color=green>BunnyDialogueManager: DialogueContainer deactivated</color>");
            }
            
            base.EndDialogue();
            
            if (_activeBunny != null)
            {
                _activeBunny.CurrentDialogueIndex++;
                _activeBunny = null;
            }
        }
        
        /// <summary>
        /// Обработчик событий завершения задания.
        /// Автоматически закрывает активный диалог при завершении задания.
        /// </summary>
        /// <param name="task">Задание, которое было завершено.</param>
        private void OnTaskEnded(BureaucraticTask task)
        {
            Debug.Log($"<color=green>BunnyDialogueManager: Задание завершено ({task?.Title}), закрываю диалог</color>");
            
            if (IsDialogueActive())
            {
                EndDialogue();
            }
        }
        
        /// <summary>
        /// Переопределяет проверку завершения диалога для увеличения времени отображения заданий.
        /// Увеличивает время отображения до 10 секунд для лучшего восприятия задания.
        /// </summary>
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
                if (!_useTimerForClosing)
                {
                    continueText.text = "Нажмите ПРОБЕЛ, чтобы закрыть диалог.";
                    continueText.gameObject.SetActive(true);
                }
                else
                {
                    continueText.gameObject.SetActive(false);
                    _currentTimer = 10f;
                }
            }
            else
            {
                if (!_useTimerForClosing)
                {
                    continueText.text = "Нажмите ПРОБЕЛ для продолжения...";
                    continueText.gameObject.SetActive(true);
                }
                else
                {
                    continueText.gameObject.SetActive(false);
                    _currentTimer = 10f;
                }
            }
        }
        
        // ========== ЛОГИКА ОТОБРАЖЕНИЯ ЗАДАНИЯ ==========
        
        /// <summary>
        /// Генерирует описание текущего задания для отображения в диалоге.
        /// Включает форматированное время, срочность и информацию об искажении задания.
        /// </summary>
        /// <returns>Строка с описанием задания.</returns>
        public string GetTaskDescriptionForDialogue()
        {
            Debug.Log("<color=cyan>BunnyDialogueManager: GetTaskDescriptionForDialogue called</color>");
            
            if (TaskManager.Instance == null)
            {
                Debug.LogWarning("<color=red>TaskManager не найден!</color>");
                return "Ошибка: система заданий не найдена";
            }
            
            var currentTask = TaskManager.Instance.GetCurrentTask();
            Debug.Log($"<color=cyan>BunnyDialogueManager: Current task is {(currentTask == null ? "null" : currentTask.Title)}</color>");
            
            // Если задания нет, создаем новое
            if (currentTask == null)
            {
                Debug.Log("<color=red>BunnyDialogueManager: No current task, returning default message</color>");
                return "Новое задание создается...";
            }
            
            string timeText = FormatTime(currentTask.TimeRemaining);
            
            string shortDescription = $"{currentTask.Description} Время: {timeText}.";
    
            if (currentTask.IsUrgent)
            {
                shortDescription = $"СРОЧНО! {shortDescription}";
            }
            
            if (currentTask.IsCorrupted)
            {
                shortDescription = $"ВНИМАНИЕ: Заяц изменил требования! {shortDescription}";
            }
            
            Debug.Log($"<color=white>BunnyDialogueManager: Task description: {shortDescription}</color>");
            return shortDescription;
        }

        /// <summary>
        /// Форматирует время в секундах в строку формата "мм:сс".
        /// </summary>
        /// <param name="timeInSeconds">Время в секундах.</param>
        /// <returns>Строка с форматированным временем.</returns>
        private string FormatTime(float timeInSeconds)
        {
            if (timeInSeconds <= 0) return "00:00";
            
            int minutes = Mathf.FloorToInt(timeInSeconds / 60);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60);
            return $"{minutes:00}:{seconds:00}";
        }
        
        /// <summary>
        /// Создает диалог с заданием для конкретного кролика.
        /// Генерирует дразнящую фразу с описанием задания и визуальными элементами.
        /// </summary>
        /// <param name="bunny">Кролик, для которого создается диалог.</param>
        /// <returns>Созданный диалог с заданием.</returns>
        public Dialogue GetTaskDialogueForBunny(global::Bunny.Bunny bunny)
        {
            Debug.Log($"<color=green>BunnyDialogueManager: GetTaskDialogueForBunny called for {bunny.BunnyName}</color>");
            
            string taskDescription = GetTaskDescriptionForDialogue();
            
            if (string.IsNullOrEmpty(taskDescription))
            {
                taskDescription = "Новое задание будет готово через мгновение...";
            }
            
            string tauntingPhrase = GetEnhancedTauntingPhrase();
            
            Dialogue dialogue = new Dialogue
            {
                name = bunny.BunnyName,
                sentences = new string[] { tauntingPhrase }
            };
            
            Debug.Log($"<color=green>BunnyDialogueManager: Created dialogue with sentence: {tauntingPhrase}</color>");
            return dialogue;
        }

        /// <summary>
        /// Генерирует улучшенную дразнящую фразу, включающую сюжетные элементы, дразнилки и описание задания.
        /// Использует рандомизацию для создания разнообразных комбинаций.
        /// </summary>
        /// <returns>Сгенерированная дразнящая фраза.</returns>
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
            
            // Рандомное количество предложений от 4 до 7
            int sentenceCount = UnityEngine.Random.Range(4, 8);
            
            //Определяем, будет ли сюжетная фраза (40% шанс)
            bool includeStory = UnityEngine.Random.value < 0.4f;
            
            //Определяем, будет ли задание (всегда должно быть, но может быть в конце)
            bool includeTask = true;
            
            //Собираем список всех возможных частей фразы
            List<string> phraseParts = new List<string>();
            
            //Сначала сюжетная фраза (если есть)
            if (includeStory)
            {
                phraseParts.Add(GetRandomStoryPhrase());
            }
            
            //Добавляем дразнилки (от 2 до 4 штук)
            int tauntCount = UnityEngine.Random.Range(2, Mathf.Min(5, sentenceCount - (includeStory ? 1 : 0) - 1));
            for (int i = 0; i < tauntCount; i++)
            {
                phraseParts.Add(GetRandomTauntTemplate());
            }
            
            //Добавляем задание в конце
            if (includeTask)
            {
                phraseParts.Add(GetTaskDescriptionForDialogue());
            }
            
            //Ограничиваем общее количество предложений и соединяем
            phraseParts = LimitPhraseParts(phraseParts, sentenceCount);
            
            //Добавляем эмодзи в конец
            string finalPhrase = string.Join(" ", phraseParts) + " " + GetRandomEmoji(currentTask);
            
            return finalPhrase;
        }

        /// <summary>
        /// Ограничивает количество частей фразы до указанного максимума, сохраняя первую и последнюю части.
        /// </summary>
        /// <param name="parts">Список частей фразы.</param>
        /// <param name="maxSentences">Максимальное количество предложений.</param>
        /// <returns>Урезанный список частей фразы.</returns>
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

        //! ИИ
        /// <summary>
        /// Возвращает случайную сюжетную фразу для добавления атмосферы в диалог.
        /// </summary>
        /// <returns>Случайная сюжетная фраза.</returns>
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

        //! ИИ
        /// <summary>
        /// Возвращает случайный шаблон дразнилки для использования в диалогах.
        /// </summary>
        /// <returns>Случайная дразнящая фраза.</returns>
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

        //! ИИ
        /// <summary>
        /// Возвращает случайный эмодзи в зависимости от типа задания.
        /// </summary>
        /// <param name="task">Задание для определения типа эмодзи.</param>
        /// <returns>Случайный эмодзи, соответствующий типу задания.</returns>
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
using UnityEngine;
using DialogueManager;
using TaskSystem;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System;

// * Идрисов Д.С

namespace Bunny 
{
    /// <summary>
    /// Специализированный менеджер диалогов для "Кроликов" (Bunny).
    /// Расширяет базовый <see cref="DialogueManager"/> для управления диалогами, связанными с заданиями.
    /// Автоматически использует таймер, отслеживает активного кролика и закрывает диалог при завершении задания.
    /// </summary>
    public class BunnyDialogueManager : DialogueManager.DialogueManager
    {
        private global::Bunny.Bunny _activeBunny;
        private bool _isTaskDialogue = false;
        
        /// <summary>
        /// Главный объект-контейнер для UI диалога, который должен быть активирован/деактивирован вместе с диалогом.
        /// </summary>
        public GameObject dialogueContainer;
        
        protected override void OnSentencePrinted()
        {
            base.OnSentencePrinted();
        }
        
        /// <summary>
        /// Начинает диалог, специфичный для кролика и связанный с заданием.
        /// Устанавливает использование таймера и подписывается на события завершения заданий.
        /// </summary>
        /// <param name="dialogue">Объект диалога для отображения.</param>
        /// <param name="bunny">Объект <see cref="global::Bunny.Bunny"/>, который инициирует диалог.</param>
         public void StartBunnyDialogue(Dialogue dialogue, global::Bunny.Bunny bunny)
        {
            Debug.Log($"<color=green>BunnyDialogueManager: StartBunnyDialogue called with bunny {bunny}</color>");
            
            // Настройка: всегда используем таймер для диалогов кролика
            UseTimerForClosing = true;
            
            _activeBunny = bunny;
            _isTaskDialogue = true;
            
            // Активируем контейнер диалога
            if (dialogueContainer != null)
            {
                dialogueContainer.SetActive(true);
                Debug.Log("<color=green>BunnyDialogueManager: DialogueContainer activated</color>");
            }
            else
            {
                Debug.LogWarning("<color=yellow>BunnyDialogueManager: DialogueContainer not assigned!</color>");
            }
            
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
        
        /// <summary>
        /// Завершает диалог, отписывается от событий заданий и деактивирует контейнер.
        /// Обновляет индекс диалога у активного кролика.
        /// </summary>
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
            
            // Деактивируем контейнер диалога
            if (dialogueContainer != null)
            {
                dialogueContainer.SetActive(false);
                Debug.Log("<color=green>BunnyDialogueManager: DialogueContainer deactivated</color>");
            }
            
            base.EndDialogue();
            
            // Увеличиваем индекс диалога у кролика
            if (_activeBunny != null)
            {
                _activeBunny.CurrentDialogueIndex++;
                _activeBunny = null;
            }
        }
        
        /// <summary>
        /// Обработчик, вызываемый при завершении (успешном или провальном) бюрократического задания.
        /// Закрывает диалог, если он активен.
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
        /// Переопределяет логику проверки завершения диалога для управления таймером.
        /// Увеличивает время отображения последнего предложения до 10 секунд.
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
        
        /// <summary>
        /// Формирует короткое описание текущего задания, включая статус срочности, повреждения и оставшееся время.
        /// </summary>
        /// <returns>Строка с форматированным описанием текущего задания.</returns>
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
        /// Преобразует время в секундах в формат "MM:SS".
        /// </summary>
        /// <param name="timeInSeconds">Время в секундах.</param>
        /// <returns>Отформатированная строка времени.</returns>
        private string FormatTime(float timeInSeconds)
        {
            if (timeInSeconds <= 0) return "00:00";
            
            int minutes = Mathf.FloorToInt(timeInSeconds / 60);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60);
            return $"{minutes:00}:{seconds:00}";
        }
        
        /// <summary>
        /// Создает объект <see cref="Dialogue"/> для кролика, содержащий сгенерированную фразу, 
        /// включающую дразнилки, сюжетные элементы и описание задания.
        /// </summary>
        /// <param name="bunny">Кролик, для которого создается диалог.</param>
        /// <returns>Новый объект <see cref="Dialogue"/>.</returns>
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
        /// Генерирует длинную, составную дразнящую фразу для диалога. 
        /// Включает случайное количество сюжетных фраз и дразнилок, а также описание текущего задания.
        /// </summary>
        /// <returns>Полная составная фраза.</returns>
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
            
            int sentenceCount = UnityEngine.Random.Range(4, 8);
            bool includeStory = UnityEngine.Random.value < 0.4f;
            bool includeTask = true; 
            
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
            
            // Ограничиваем общее количество предложений и соединяем
            phraseParts = LimitPhraseParts(phraseParts, sentenceCount);
            
            // Добавляем эмодзи в конец
            string finalPhrase = string.Join(" ", phraseParts) + " " + GetRandomEmoji(currentTask);
            
            return finalPhrase;
        }

        /// <summary>
        /// Ограничивает количество частей фразы заданным максимальным числом, 
        /// сохраняя первую часть (сюжет) и последнюю часть (задание).
        /// </summary>
        /// <param name="parts">Список частей фразы.</param>
        /// <param name="maxSentences">Максимально допустимое количество предложений.</param>
        /// <returns>Ограниченный список частей фразы.</returns>
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

        /// <summary>
        /// Выбирает случайную сюжетную фразу из предопределенного списка.
        /// </summary>
        /// <returns>Сюжетная фраза.</returns>
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

        /// <summary>
        /// Выбирает случайный шаблон дразнящей фразы из предопределенного списка.
        /// </summary>
        /// <returns>Дразнящая фраза.</returns>
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

        /// <summary>
        /// Выбирает случайный символ в зависимости от статуса задания (испорчено, срочно или нормально).
        /// Заменены сложные символы Юникода, которые могут не отображаться в шрифтах SDF, на более простые аналоги.
        /// </summary>
        /// <param name="task">Текущее бюрократическое задание.</param>
        /// <returns>Случайный эмодзи или его текстовый аналог.</returns>
        private string GetRandomEmoji(BureaucraticTask task)
        {
            if (task.IsCorrupted)
            {
                // Заменены: 👹, 🦹, 🃏, 🎭, 🤡, 👻
                string[] corruptedEmojis = { "😈", "(Зло)", "😏", "(Жулик)", "🤪", "😼", "(Карта)", "(Маска)", "(Клоун)", "(Бу)" };
                return corruptedEmojis[UnityEngine.Random.Range(0, corruptedEmojis.Length)];
            }
            else if (task.IsUrgent)
            {
                // Заменены: 🎯, 💣
                string[] urgentEmojis = { "🔥", "⏰", "🚨", "💥", "⚡", "💢", "‼️", "⚠️", "(!)", "(БОМБА)" };
                return urgentEmojis[UnityEngine.Random.Range(0, urgentEmojis.Length)];
            }
            else
            {
                // Заменены: 🃏, 🎭, 🤡, 🎩, 💫, 🎪, 🎲
                string[] normalEmojis = { "🐰", "😄", "🤭", "😉", "(Карта)", "(Маска)", "(Клоун)", "👀", "(Шляпа)", "✨", "🌟", "(О)", "(Цирк)", "(Кубик)" };
                return normalEmojis[UnityEngine.Random.Range(0, normalEmojis.Length)];
            }
        }
    }
}
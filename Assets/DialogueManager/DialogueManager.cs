using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// * Идрисов Д.С

namespace DialogueManager {
    /// <summary>
    /// Управляет отображением диалога в игре, включая текст, имя персонажа и логику перехода между предложениями.
    /// Поддерживает как управление с помощью клавиши ПРОБЕЛ, так и автоматическое закрытие по таймеру.
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        // =================================================================================================
        // Ссылки на UI элементы (публичные поля для назначения в Инспекторе)
        // =================================================================================================

        /// <summary>
        /// Главный объект, содержащий все элементы диалога (текстовое облако).
        /// </summary>
        public GameObject textCloud;
        
        /// <summary>
        /// Объект, содержащий имя персонажа.
        /// </summary>
        public GameObject nameObject;
        
        /// <summary>
        /// Объект, содержащий текст диалога.
        /// </summary>
        public GameObject dialogueObject;
        
        /// <summary>
        /// Элемент <see cref="TextMeshProUGUI"/>, отображающий индикатор продолжения/завершения диалога (например, "Нажмите ПРОБЕЛ").
        /// Должен быть назначен в Инспекторе.
        /// </summary>
        public TextMeshProUGUI continueText; 

        // =================================================================================================
        // Приватные поля и Свойства
        // =================================================================================================

        private TextMeshProUGUI nameText;
        
        /// <summary>
        /// Элемент <see cref="TextMeshProUGUI"/> для отображения текста диалога.
        /// </summary>
        public TextMeshProUGUI dialogueText;
        
        /// <summary>
        /// Очередь предложений для текущего диалога.
        /// </summary>
        public Queue<string> sentences;
        
        private string currentSentence;
        private Coroutine typeCoroutine;
        private bool isTyping = false;
        private bool isDialogueActive = false;

        /// <summary>
        /// Возвращает индекс текущего отображаемого предложения в диалоге.
        /// </summary>
        public int CurrentSentenceIndex => _currentSentenceIndex; 
        
        /// <summary>
        /// Индекс текущего предложения.
        /// </summary>
        protected int _currentSentenceIndex = -1; 

        // Логика управления закрытием (ПРОБЕЛ vs Таймер)
        protected bool _useTimerForClosing = false;

        /// <summary>
        /// Определяет, использовать ли таймер для автоматического перехода/закрытия диалога, или ждать нажатия ПРОБЕЛ.
        /// </summary>
        public bool UseTimerForClosing
        {
            get => _useTimerForClosing;
            set => _useTimerForClosing = value;
        }
        
        [SerializeField] 
        /// <summary>
        /// Продолжительность (в секундах) до автоматического закрытия или перехода к следующему предложению, 
        /// если <see cref="UseTimerForClosing"/> установлен в true.
        /// </summary>
        protected float _autoCloseDuration = 100f; // Примечание: 100 секунд, возможно, слишком много.
        
        /// <summary>
        /// Текущее значение таймера обратного отсчета.
        /// </summary>
        public float _currentTimer = 0f;

        // Логика "постоянного" отображения
        protected bool _isPermanentDisplay = false;
        
        /// <summary>
        /// Если установлено в true, диалог не будет закрываться автоматически 
        /// и не будет реагировать на ввод (ПРОБЕЛ) или таймер для закрытия.
        /// </summary>
        public bool IsPermanentDisplay
        {
            get => _isPermanentDisplay;
            set => _isPermanentDisplay = value;
        }

        // =================================================================================================
        // Методы Unity
        // =================================================================================================

        void Awake()
        {
            Debug.Log("<color=cyan>=== DialogueManager Awake ===</color>");

            sentences = new Queue<string>();

            // Получаем TextMeshProUGUI компоненты
            if (nameObject != null)
            {
                nameText = nameObject.GetComponent<TextMeshProUGUI>();
                if (nameText == null)
                    Debug.LogError("<color=red>Name object не имеет компонента TextMeshProUGUI!</color>");
            }
            
            if (dialogueObject != null)
            {
                dialogueText = dialogueObject.GetComponent<TextMeshProUGUI>();
                if (dialogueText == null)
                    Debug.LogError("<color=red>Dialogue object не имеет компонента TextMeshProUGUI!</color>");
            }
            
            // Скрываем TextCloud и индикатор
            if (textCloud != null)
            {
                textCloud.SetActive(false);
            }
            else
            {
                Debug.LogError("<color=red>TextCloud не установлен!</color>");
            }

            if (continueText != null)
                continueText.gameObject.SetActive(false);
        }
        
        void Update()
        {
            if (!isDialogueActive || _isPermanentDisplay) return;

            if (!_useTimerForClosing)
            {
                // Логика закрытия/перехода по ПРОБЕЛУ
                if (Input.GetKeyDown(KeyCode.Space)) 
                {
                    if (isTyping)
                    {
                        // Пропускаем печать до конца
                        if (typeCoroutine != null)
                        {
                            StopCoroutine(typeCoroutine);
                        }
                        
                        dialogueText.text = currentSentence; 
                        isTyping = false;
                        OnSentencePrinted();
                        
                        // Показываем индикатор или проверяем завершение, если предложений больше нет.
                        if (continueText != null && sentences.Count > 0)
                            continueText.gameObject.SetActive(true);
                        else
                            CheckDialogueEnd();
                    }
                    else
                    {
                        // Переходим к следующему предложению
                        DisplayNextSentence(); 
                    }
                }
            }
            else
            {
                // Логика закрытия по ТАЙМЕРУ
                if (!isTyping)
                {
                    _currentTimer -= Time.deltaTime;
                    if (_currentTimer <= 0)
                    {
                        DisplayNextSentence();
                    }
                }
            }
        }
        
        // =================================================================================================
        // Публичные методы
        // =================================================================================================

        /// <summary>
        /// Начинает отображение нового диалога.
        /// </summary>
        /// <param name="dialogue">Объект <c>Dialogue</c>, содержащий имя и список предложений.</param>
        public virtual void StartDialogue(Dialogue dialogue)
        {
            Debug.Log($"<color=green>DialogueManager: StartDialogue called with {dialogue.name}</color>");
            
            if (textCloud == null || nameText == null || dialogueText == null)
            {
                Debug.LogError("<color=red>DialogueManager: Не установлены все компоненты UI!</color>");
                return;
            }

            textCloud.SetActive(true);
            isDialogueActive = true;

            if (continueText != null && !_useTimerForClosing)
                continueText.gameObject.SetActive(false);
            
            if (dialogueText != null) 
            {
                dialogueText.text = ""; 
            }

            nameText.text = dialogue.name;

            sentences.Clear();
            foreach (string sentence in dialogue.sentences)
            {
                sentences.Enqueue(sentence);
            }
            
            _currentSentenceIndex = -1; 
            
            DisplayNextSentence();
        }

       /// <summary>
       /// Отображает следующее предложение из очереди. 
       /// Если очередь пуста и диалог не является постоянным, вызывает <see cref="EndDialogue"/>.
       /// </summary>
       public virtual void DisplayNextSentence()
        {
           if (sentences.Count == 0)
            {   
                if(!_isPermanentDisplay)
                    EndDialogue();
                return;
            }

            _currentSentenceIndex++;
            
            if (continueText != null && !_useTimerForClosing)
                continueText.gameObject.SetActive(false);

            currentSentence = sentences.Dequeue();
            
            if (typeCoroutine != null)
                StopCoroutine(typeCoroutine);

            typeCoroutine = StartCoroutine(TypeText(currentSentence));
        }
        
        /// <summary>
        /// Завершает текущий диалог, скрывая UI элементы.
        /// Игнорируется, если <see cref="IsPermanentDisplay"/> равно true.
        /// </summary>
        public virtual void EndDialogue()
        {
            if(_isPermanentDisplay)
            {
                Debug.LogWarning("<color=yellow>Попытка закрыть постоянный диалог. Игнорируем</color>");
                return;
            }
            Debug.Log("<color=green>Диалог завершен</color>");
            isDialogueActive = false;
            _currentSentenceIndex = -1; 
            if (textCloud != null)
                textCloud.SetActive(false);

            if (continueText != null)
                continueText.gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Проверяет, активен ли в данный момент диалог.
        /// </summary>
        /// <returns>True, если диалог активен; иначе False.</returns>
        public bool IsDialogueActive()
        {
            return isDialogueActive;
        }

        // =================================================================================================
        // Приватные/Защищенные методы
        // =================================================================================================
        
        /// <summary>
        /// Корутина для посимвольного "печатания" текста.
        /// </summary>
        /// <param name="sentence">Предложение для отображения.</param>
        IEnumerator TypeText(string sentence)
        {
            isTyping = true;
            dialogueText.text = "";
            
            foreach (char letter in sentence.ToCharArray())
            {
                if (!isDialogueActive || dialogueText == null) break; 
                dialogueText.text += letter;
                yield return null;
            }
            
            isTyping = false;
            OnSentencePrinted();

            CheckDialogueEnd();
        }

        /// <summary>
        /// Проверяет, является ли текущее предложение последним, и устанавливает соответствующий текст и логику для индикатора продолжения.
        /// </summary>
        protected virtual void CheckDialogueEnd()
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
                    // Если таймер, запускаем таймер на закрытие
                    continueText.gameObject.SetActive(false);
                    _currentTimer = _autoCloseDuration;
                }
            }
            else
            {
                // Есть еще предложения
                if (!_useTimerForClosing)
                {
                    continueText.text = "Нажмите ПРОБЕЛ для продолжения...";
                    continueText.gameObject.SetActive(true);
                }
                else
                {
                    // Если таймер, запускаем таймер на следующее предложение
                    continueText.gameObject.SetActive(false);
                    _currentTimer = _autoCloseDuration;
                }
            }
        }
        
        /// <summary>
        /// Вызывается после того, как текущее предложение полностью "напечатано". 
        /// Может быть переопределен в дочерних классах для добавления дополнительной логики.
        /// </summary>
        protected virtual void OnSentencePrinted()
        {
            // Метод-заглушка для переопределения
        }
    }
}
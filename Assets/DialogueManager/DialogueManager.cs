using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Добавляем using для TextMeshPro

namespace DialogueManager {
    public class DialogueManager : MonoBehaviour
    {


        // Ссылки на UI элементы
        public GameObject textCloud;
        public GameObject nameObject;
        public GameObject dialogueObject;
        public TextMeshProUGUI continueText; // Индикатор продолжения (должен быть назначен в Инспекторе)
        public int CurrentSentenceIndex => _currentSentenceIndex; // Публичный геттер

        private TextMeshProUGUI nameText;
        private TextMeshProUGUI dialogueText;
        private Queue<string> sentences;
        private string currentSentence;
        private Coroutine typeCoroutine;
        private bool isTyping = false;
        private bool isDialogueActive = false;

        // [NEW] Индекс текущего предложения. Доступен только для чтения (getter)
        protected int _currentSentenceIndex = -1; 

        //* Работа с таймером и переключение между ПРОБЕЛ и таймер
        protected bool _useTimerForClosing = false;

        public bool UseTimerForClosing
        {
            get => _useTimerForClosing;
            set => _useTimerForClosing = value;
        }
        
        [SerializeField] protected float _autoCloseDuration = 3f;
        private float _currentTimer = 0f;
        //*

        //* ЕСЛИ МЫ ХОТИМ ЧТОБЫ ЗАДАНИЕ НЕ ЗАКРЫВАЛОСЬ
        protected bool _isPermanentDisplay = false;
        public bool IsPermanentDisplay
        {
            get => _isPermanentDisplay;
            set => _isPermanentDisplay = value;
        }
        //*
        void Awake()
        {
            Debug.Log("=== DialogueManager Awake ===");

            // Инициализация очереди
            sentences = new Queue<string>();

            // Получаем TextMeshProUGUI компоненты
            if (nameObject != null)
            {
                nameText = nameObject.GetComponent<TextMeshProUGUI>();
                if (nameText == null)
                    Debug.LogError("Name object не имеет компонента TextMeshProUGUI!");
                else
                    Debug.Log("Name TextMeshProUGUI найден");
            }
            
            if (dialogueObject != null)
            {
                dialogueText = dialogueObject.GetComponent<TextMeshProUGUI>();
                if (dialogueText == null)
                    Debug.LogError("Dialogue object не имеет компонента TextMeshProUGUI!");
                else
                    Debug.Log("Dialogue TextMeshProUGUI найден");
            }
            
            // Скрываем TextCloud и индикатор
            if (textCloud != null)
            {
                textCloud.SetActive(false);
                Debug.Log("TextCloud скрыт");
            }
            else
            {
                Debug.LogError("TextCloud не установлен!");
            }

            if (continueText != null)
                continueText.gameObject.SetActive(false);
        }
        
        void Update()
        {
            if (!isDialogueActive) return;
            if (_isPermanentDisplay) return;


            if (!_useTimerForClosing)
            {
                //* Логика закрытия по ПРОБЕЛУ
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
                        
                        // Показываем индикатор (он будет скрыт на следующем шаге DisplayNextSentence)
                        if (continueText != null && sentences.Count > 0)
                            continueText.gameObject.SetActive(true);
                        else
                            CheckDialogueEnd(); // Проверяем, если это последнее предложение
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
                //* Логика закрытия по ТАЙМЕРУ (только если печать завершена)
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
        
        public virtual void StartDialogue(Dialogue dialogue)
        {
            Debug.Log($"DialogueManager: StartDialogue called with {dialogue.name}");
            Debug.Log($"DialogueManager: TextCloud is {textCloud}");
            
            // Проверяем ссылки
            if (textCloud == null || nameText == null || dialogueText == null)
            {
                Debug.LogError("DialogueManager: Не установлены все компоненты UI!");
                return;
            }

            // Показываем TextCloud. Индикатор продолжения скрыт, если _useTimerForClosing = true.
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

       public virtual void DisplayNextSentence()
        {
            // ПРОВЕРКА: Если предложений не осталось
           if (sentences.Count == 0)
            {   
                if(!_isPermanentDisplay)
                    EndDialogue();
                return;
            }

            _currentSentenceIndex++;
            // Скрываем индикатор только если мы не используем таймер
            if (continueText != null && !_useTimerForClosing)
                continueText.gameObject.SetActive(false);

            currentSentence = sentences.Dequeue();
            
            if (typeCoroutine != null)
                StopCoroutine(typeCoroutine);

            typeCoroutine = StartCoroutine(TypeText(currentSentence));
        }
        
        IEnumerator TypeText(string sentence)
        {
            isTyping = true;
            dialogueText.text = "";
            
            // Печатаем по буквам
            foreach (char letter in sentence.ToCharArray())
            {
                // Добавляем проверку на наличие компонентов и активности диалога
                if (!isDialogueActive || dialogueText == null) break; 
                dialogueText.text += letter;
                yield return null;
            }
            
            isTyping = false;
            OnSentencePrinted();

            CheckDialogueEnd();
        }
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
                    // Если таймер, диалог закончится сам
                    continueText.gameObject.SetActive(false);
                    // Запускаем таймер на закрытие
                    _currentTimer = _autoCloseDuration;
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
                    // Если таймер, переходим к следующему предложению
                    continueText.gameObject.SetActive(false);
                    // Запускаем таймер на следующее предложение
                    _currentTimer = _autoCloseDuration;
                }
            }
        }
        protected virtual void OnSentencePrinted()
        {
            
        }
        public virtual void EndDialogue()
        {
            if(_isPermanentDisplay)
            {
                Debug.LogWarning("Попытка закрыть постоянный диалог. Игнорируем");
                return;
            }
            Debug.Log("Диалог завершен");
            isDialogueActive = false;
            _currentSentenceIndex = -1; 
            if (textCloud != null)
                textCloud.SetActive(false);

            if (continueText != null)
                continueText.gameObject.SetActive(false);
        }
        
        public bool IsDialogueActive()
        {
            return isDialogueActive;
        }
    }
}
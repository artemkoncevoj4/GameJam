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

        private TextMeshProUGUI nameText;
        private TextMeshProUGUI dialogueText;
        
       private Queue<string> sentences;
        private string currentSentence;
        private Coroutine typeCoroutine;

        private bool isTyping = false;
        private bool isDialogueActive = false;
        
        // [NEW] Индекс текущего предложения. Доступен только для чтения (getter)
        protected int _currentSentenceIndex = -1; 
        public int CurrentSentenceIndex => _currentSentenceIndex; // Публичный геттер
        
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
            if (isDialogueActive && Input.GetKeyDown(KeyCode.Space)) 
            {
                if (isTyping)
                {
                    // [FIX 1] Если текст печатается, пропускаем печать до конца.
                    // Останавливаем корутину, чтобы текст появился мгновенно
                    if (typeCoroutine != null)
                    {
                        StopCoroutine(typeCoroutine);
                    }
                    
                    // Выводим весь текст сразу
                    dialogueText.text = currentSentence; 
                    isTyping = false;
                    
                    // Вызываем хук после завершения печати
                    OnSentencePrinted();
                    
                    // Показываем индикатор продолжения (он будет скрыт на следующем шаге)
                    if (continueText != null)
                        continueText.gameObject.SetActive(true);
                }
                else
                {
                    // [FIX 2] Если текст НЕ печатается, переходим к следующему предложению
                    DisplayNextSentence(); 
                }
            }
        }
        
        public virtual void StartDialogue(Dialogue dialogue)
        {
            Debug.Log($"DialogueManager: StartDialogue called with {dialogue.name}");
            Debug.Log($"DialogueManager: TextCloud is {textCloud}");
            
            // Проверяем ссылки
            if (textCloud == null)
            {
                Debug.LogError("TextCloud не установлен!");
                return;
            }
            
            if (nameText == null || dialogueText == null)
            {
                Debug.LogError("TextMeshProUGUI компоненты не найдены!");
                return;
            }
            
            // Показываем TextCloud, скрываем индикатор
            textCloud.SetActive(true);
            isDialogueActive = true;
            Debug.Log($"DialogueManager: TextCloud set active, isDialogueActive = {isDialogueActive}");

            if (continueText != null)
                continueText.gameObject.SetActive(false);
            
            if (dialogueText != null) 
            {
                dialogueText.text = ""; 
            }
            // Устанавливаем имя
            nameText.text = dialogue.name;

            // Очищаем и заполняем очередь
            sentences.Clear();
            foreach (string sentence in dialogue.sentences)
            {
                sentences.Enqueue(sentence);
            }
            
            _currentSentenceIndex = -1; // Сброс индекса перед началом
            
            DisplayNextSentence();
        }

        public virtual void DisplayNextSentence()
        {
            // ПРОВЕРКА: Если предложений не осталось
           if (sentences.Count == 0)
            {
                EndDialogue();
                return;
            }
            // [MODIFIED] Увеличиваем индекс ПЕРЕД извлечением предложения
            _currentSentenceIndex++;
            // Скрываем индикатор
            if (continueText != null)
                continueText.gameObject.SetActive(false);

            currentSentence = sentences.Dequeue();
            
            // Запускаем печать текста
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
                dialogueText.text += letter;
                yield return null;
            }
            
            isTyping = false;
            // [NEW HOOK] Вызываем хук после завершения печати
            OnSentencePrinted();
            // Показываем индикатор, когда печать завершена
            if (continueText != null)
            {
                // Если это последнее предложение, текст будет "Нажмите ПРОБЕЛ, чтобы закрыть диалог."
                // Иначе: "Нажмите ПРОБЕЛ для продолжения..."
                if (sentences.Count == 0)
                {
                    continueText.text = "Нажмите ПРОБЕЛ, чтобы закрыть диалог.";
                }
                else
                {
                    continueText.text = "Нажмите ПРОБЕЛ для продолжения...";
                }
                
                continueText.gameObject.SetActive(true);
            }
        }
        protected virtual void OnSentencePrinted()
        {
            
        }
        public virtual void EndDialogue()
        {
            Debug.Log("Диалог завершен");
            isDialogueActive = false;
            _currentSentenceIndex = -1; // Сброс индекса
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
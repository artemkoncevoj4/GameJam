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
        
        private Queue<string> sentences; // Очередь для хранения предложений
        private string currentSentence;
        private Coroutine typeCoroutine;

        private bool isTyping = false;
        private bool isDialogueActive = false;
        
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
                    // 1. Пропускаем анимацию
                    if (typeCoroutine != null)
                        StopCoroutine(typeCoroutine);

                    if (dialogueText != null)
                        dialogueText.text = currentSentence;
                    
                    isTyping = false;
                    
                    if (continueText != null)
                        continueText.gameObject.SetActive(true); // Показываем индикатор
                }
                else
                {
                    // 2. Переходим к следующему предложению ИЛИ завершаем диалог
                    if (sentences.Count == 0)
                    {
                        EndDialogue(); // Завершаем, если предложений нет
                    }
                    else
                    {
                        DisplayNextSentence(); // Иначе, показываем следующее
                    }
                }
            }
        }
        
        public void StartDialogue(Dialogue dialogue)
        {
            Debug.Log("Начинаем диалог с " + dialogue.name);
            
            // Проверяем ссылки
            if (textCloud == null)
            {
                Debug.LogError("TextCloud не установлен!");
                return;
            }
            
            if (nameText == null || dialogueText == null)
            {
                Debug.LogError("TextMeshProUGUI компоненты не найдены! Проверьте, что объекты Name и Dialogue имеют компонент TextMeshPro - Text");
                return;
            }
            
            // Показываем TextCloud, скрываем индикатор
            textCloud.SetActive(true);
            isDialogueActive = true;
            if (continueText != null)
                continueText.gameObject.SetActive(false);
            
            // Устанавливаем имя
            nameText.text = dialogue.name;

            // Очищаем и заполняем очередь
            sentences.Clear();
            foreach (string sentence in dialogue.sentences)
            {
                sentences.Enqueue(sentence);
            }
            
            // Начинаем с первого предложения
            DisplayNextSentence();
        }

        public void DisplayNextSentence()
        {
            // ПРОВЕРКА: Если предложений не осталось
            if (sentences.Count == 0)
            {
                // Устанавливаем текст для завершения диалога
                if (continueText != null)
                {
                    continueText.text = "Нажмите ПРОБЕЛ, чтобы закрыть диалог.";
                    continueText.gameObject.SetActive(true);
                }
                
                // EndDialogue() теперь вызывается через Update() после нажатия пробела
                return;
            }
            
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
                yield return new WaitForSeconds(0.01f); // Используем ту же задержку, что и раньше
            }
            
            isTyping = false;
            
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
        
        void EndDialogue()
        {
            Debug.Log("Диалог завершен");
            isDialogueActive = false;
            
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
using UnityEngine;
using DialogueManager;

// Наследуемся от базового DialogueManager
// [ВНИМАНИЕ: Для работы требуется, чтобы DialogueManager.cs имел public virtual методы!]
public class BunnyDialogueManager : DialogueManager.DialogueManager
{
   //! [Header("Настройки Зайца")]
    // [Опционально] Если логика задания в Bunny.cs, здесь будет ссылка на него:
    // [SerializeField] private Bunny bunnyLogic; 
    
    // Метод Awake базового класса сработает автоматически

    // ========== ХУК ПОЛИМОРФИЗМА ==========
    
    // Переопределяем метод, который вызывается после завершения печати предложения
    // (Этот метод нужно было добавить в DialogueManager.cs как 'protected virtual void OnSentencePrinted()')
    protected override void OnSentencePrinted()
    {
        // Вызываем базовый метод
        base.OnSentencePrinted(); 
        
        // Проверяем индекс предложения
        // Индекс 1 - это второе предложение
        if (CurrentSentenceIndex == 1) // Используем публичный геттер CurrentSentenceIndex
        {
            ApplyTaskLogic();
        }
    }
    
    // ========== ЛОГИКА ЗАДАНИЯ ==========
    
    // Метод, который выполняет основную логику назначения/порчи задания
    private void ApplyTaskLogic()
    {
        // [MODIFIED] Используем новый TaskManager
        if (TestTaskManager.Instance != null) 
        {
            // [MODIFIED] Получаем задание
            GameTask CurrentTask = TestTaskManager.Instance.GetCurrentTask(); 
            
            // Вариант 1: Назначить новое задание
            if (CurrentTask == null)
            {
                TestTaskManager.Instance.AssignNewTask();
                Debug.Log("ЗАДАНИЕ: Заяц назначил новое задание!");
            }

            // Вариант 2: Испортить текущее задание (50% шанс)
            // (Проверяем, что задание существует и шанс сработал)
            else if (UnityEngine.Random.value > 0.5f && CurrentTask != null)
            {
                TestTaskManager.Instance.CorruptCurrentTask();
                Debug.Log("ЗАДАНИЕ: Заяц испортил задание!");
            }
            else
            {
                // Если задание есть, но шанс порчи не сработал
                Debug.Log("ЗАДАНИЕ: Заяц ничего не тронул.");
            }
        }
        else
        {
            Debug.LogWarning("TestTaskManager не найден!");
        }
    }
}
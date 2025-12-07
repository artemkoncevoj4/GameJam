using UnityEngine;
using DialogueManager;

public class BunnyDialogueManager : DialogueManager.DialogueManager
{
    private Bunny _activeBunny;
    // [Опционально] Если логика задания в Bunny.cs, здесь будет ссылка на него:
    // [SerializeField] private Bunny bunnyLogic; 
    
    // Метод Awake базового класса сработает автоматически

    // ========== ХУК ПОЛИМОРФИЗМА ==========
    
    // Переопределяем метод, который вызывается после завершения печати предложения
    // (Этот метод нужно было добавить в DialogueManager.cs как 'protected virtual void OnSentencePrinted()')
    protected override void OnSentencePrinted()
    {
        base.OnSentencePrinted(); 
        
        // Так как у нас всегда одно предложение, логика задания должна запускаться здесь
        ApplyTaskLogic();
    }
    public void StartBunnyDialogue(Dialogue dialogue, Bunny bunny)
    {
        GameCycle.Instance.PauseGame();
        
        // 1. Сохраняем ссылку на Зайца, который инициировал диалог
        _activeBunny = bunny; // Сохраняем ссылку на Зайца
        
        // **ВАЖНО:** Если логика задания должна сработать ДО того, как Заяц уйдёт,
        // убедитесь, что ApplyTaskLogic() сработает на первом (и единственном) предложении.
        base.StartDialogue(dialogue);
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
    public override void EndDialogue() 
    {
            base.EndDialogue(); // Закрывает UI и возобновляет время
            
            // [FIX] 1. Возобновляем игровое время
            if (GameCycle.Instance != null)
            {
                GameCycle.Instance.ResumeGame();
            }

            // [FIX] 2. УВЕЛИЧИВАЕМ ИНДЕКС в Bunny.cs и проверяем, нужно ли уйти.
            if (_activeBunny != null)
            {
                // Увеличиваем индекс, чтобы следующее появление начало со следующего предложения
                _activeBunny.CurrentDialogueIndex++; // <-- Требуется публичный сеттер/метод в Bunny.cs
                
                // Если все предложения исчерпаны, Bunny.Leave() будет вызван при следующем вызове AssignOrModifyTask.
            }
            
            _activeBunny = null; // Сброс ссылки
    }
    }
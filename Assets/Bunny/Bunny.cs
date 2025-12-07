using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogueManager;
public class Bunny : MonoBehaviour
{
    [Header("Позиция появления")]
    [SerializeField] private Transform _appearPoint_Window;
    [SerializeField] private Transform _appearPoint_Door1;
    [SerializeField] private Transform _appearPoint_Door2;// Дверь или окно, куда прибегает заяц

    [Header("Настройки поведения")]
    [SerializeField] private float _shoutDuration = 3f; // Сколько секунд заяц "кричит"
    [SerializeField] private float _peekChance = 0.6f; // Шанс подглядывания вместо крика
    [SerializeField] private float _peekDuration = 2f; // Длительность подглядывания
    
    [Header("Эффекты")]
    //[SerializeField] private AudioClip _shoutSound;
    //[SerializeField] private AudioClip _peekSound;
    [SerializeField] private GameObject _chaosEffect; // Визуальный эффект хаоса
    //private Animator _animator;
    //private AudioSource _audioSource;
    private SpriteRenderer _spriteRenderer;
    private bool _isActive = false;
    private Coroutine _currentBehavior;

    private BunnyDialogueManager _bunnyDialogueManager;
    public bool IsActive => _isActive; // Публичный геттер для _isActive
    // Start is called before the first frame update
    void Start()
    {
        //_animator = GetComponent<Animator>();
        //_audioSource = GetComponent<AudioSource>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        SetVisible(false);
        if (_appearPoint_Window != null)
        {
            transform.position = _appearPoint_Window.position;
            transform.rotation = _appearPoint_Window.rotation;
        }

        _bunnyDialogueManager = FindObjectOfType<BunnyDialogueManager>();
        if (_bunnyDialogueManager == null)
        {
            Debug.LogError("BunnyDialogueManager не найден в сцене!");
        }
    }

    public void Appear()    
    {
        if (_isActive) return;
        
        _isActive = true;
        SetVisible(true);
        
        bool willPeek = UnityEngine.Random.value < _peekChance;
        bool whichDoor = UnityEngine.Random.value < 0.5f;
        
        if (willPeek)
        {
            if (whichDoor)
            {
                transform.position = _appearPoint_Door1.position;
                transform.rotation = _appearPoint_Door1.rotation;
            }
            else
            {
                transform.position = _appearPoint_Door2.position;
                transform.rotation = _appearPoint_Door2.rotation;
            }
            _currentBehavior = StartCoroutine(PeekBehavior());
        }
        else
        {
            transform.position = _appearPoint_Window.position;
            transform.rotation = _appearPoint_Window.rotation;
            _currentBehavior = StartCoroutine(ShoutBehavior());
        }
        Debug.Log($"Заяц появился! Поведение: {(willPeek ? "Подглядывает" : "Кричит")}");
    }
    
    public void Leave()
    {
        if (!_isActive) return;
        
        _isActive = false;
        
        // Остановить текущее поведение
        if (_currentBehavior != null)
        {
            StopCoroutine(_currentBehavior);
            _currentBehavior = null;
        }
        
        // Анимация ухода
        //if (_animator != null)
        //    _animator.SetTrigger("Leave");
            
        // Скрыть через секунду (или после завершения анимации)
        Invoke(nameof(Hide), 1f);
        
        Debug.Log("Заяц ушёл");
    }
    private IEnumerator ShoutBehavior()
    {
        // Анимация крика
        //if (_animator != null)
        //    _animator.SetTrigger("Shout");
            
        // Звук крика
        //if (_audioSource != null && _shoutSound != null)
        //    _audioSource.PlayOneShot(_shoutSound);
        
        // Ждём
        yield return new WaitForSeconds(_shoutDuration);
        
        // После крика - назначить новое задание или изменить текущее
      //  AssignOrModifyTask();
        
        // Уходим
        Leave();
    }
    
    private IEnumerator PeekBehavior()
    {
        // Анимация подглядывания
        //if (_animator != null)
        //    _animator.SetTrigger("Peek");
            
        // Звук подглядывания
        //if (_audioSource != null && _peekSound != null)
        //    _audioSource.PlayOneShot(_peekSound);
        
        // Визуальный эффект хаоса
        //if (_chaosEffect != null)
        //    _chaosEffect.SetActive(true);
        
        // Вызвать хаос-эффект в игре
        TriggerChaosEffect();
        
        // Ждём
        yield return new WaitForSeconds(_peekDuration);
        
        // Выключить эффект
        //if (_chaosEffect != null)
        //    _chaosEffect.SetActive(false);
        
        // Уходим
        Leave();
    }
    
   //* ========== ВОЗДЕЙСТВИЕ НА ИГРУ ==========
    
private void AssignOrModifyTask()
    {
        if (_bunnyDialogueManager == null)
        {
            Debug.LogError("BunnyDialogueManager не найден!");
            return;
        }
        
        // 1. [FIXED] Создаем экземпляр класса Dialogue с помощью стандартного конструктора (new).
        Dialogue dialogue = new Dialogue();
        
        // 2. Заполняем данными. Логика назначения/порчи задания будет в BunnyDialogueManager.
        dialogue.name = "Заяц-Нарушитель";
        dialogue.sentences = new string[] 
        {
            "У меня для тебя есть новое задание!", // Индекс 0
            "Сейчас я решу: дать тебе его или испортить!", // Индекс 1 (Здесь сработает логика)
            "Жду твоих действий..." // Индекс 2
        };

        if (!_bunnyDialogueManager.IsDialogueActive())
        {
            // Вызываем StartDialogue на ДОЧЕРНЕМ КЛАССЕ
            _bunnyDialogueManager.StartDialogue(dialogue);
        }
    }
    
    private void TriggerChaosEffect()
    {
        // Подглядывание вызывает хаос
        Debug.Log("Заяц подглядывает и вызывает хаос!");
        
        // 1. Увеличить стресс
        if (GameCycle.Instance != null)
        {
            GameCycle.Instance.AddStress(4f);
        }
        
        // 2. Случайная проблема для игрока
        float randomEffect = UnityEngine.Random.value;
        
        if (randomEffect < 0.25f)
        {
            Debug.Log("Хаос: Инверсия управления на 3 секунды!");
            // Здесь можно вызвать инверсию управления у игрока
        }
        else if (randomEffect < 0.5f)
        {
            Debug.Log("Хаос: Временное замедление!");
            // Замедлить время на 2 секунды
            Time.timeScale = 0.5f;
            Invoke(nameof(ResetTimeScale), 2f);
        }
        else if (randomEffect < 0.75f)
        {
            Debug.Log("Хаос: Плывет экран!");
        }
        else
        {
            Debug.Log("Хаос: Случайный звуковой эффект!");
            // Воспроизвести странный звук
        }
    }
    
    private void ResetTimeScale()
    {
        Time.timeScale = 1f;
    }
    
    // ========== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ==========
    
    private void SetVisible(bool visible)
    {
        _spriteRenderer.enabled = visible;
        
        // Можно включить/выключить все дочерние объекты
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(visible);
        }
    }
    
    private void Hide()
    {
        SetVisible(false);
    }
    
}

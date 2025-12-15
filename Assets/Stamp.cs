using UnityEngine;
using TaskSystem;
using InteractiveObjects;

// Муштаков А.Ю.

/// <summary>
/// Класс, представляющий интерактивный 2D штамп, который можно выбрать для установки на документ.
/// Управляет визуальной обратной связью при взаимодействии с мышью и обновляет глобальное состояние выбора штампа.
/// </summary>
public class Stamp2D : MonoBehaviour
{
    [Header("Настройки штампа")]
    public string stampName = "Штамп"; // Отображаемое название штампа
    public Color stampColor = Color.red; // Цвет штампа для визуального отображения
    public StampType stampType = StampType.Одобрено; // Тип штампа, представленный этим объектом
    
    [Header("Визуальная обратная связь")]
    public float hoverScale = 1.1f; // Масштаб при наведении курсора
    public float clickScale = 0.95f; // Масштаб при клике
    
    private Vector2 originalScale; // Исходный масштаб объекта
    private SpriteRenderer spriteRenderer; // Компонент для визуального отображения
    private Color originalColor; // Исходный цвет спрайта
    /// <summary>
    /// Статический флаг, указывающий, выбран ли штамп для установки.
    /// Был ли выбран какой-то штамп
    /// </summary>
    public static bool isStumped = false; //TODO Сделать нормально
    
    /// <summary>
    /// Инициализирует компоненты и сохраняет исходные значения при запуске.
    /// Устанавливает начальный цвет штампа в соответствии с настройками.
    /// </summary>
    void Start()
    {
        // Сохраняем оригинальные значения
        originalScale = transform.localScale;
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            spriteRenderer.color = stampColor;
        }
    }
    
    /// <summary>
    /// Обрабатывает клик мыши по штампу.
    /// Переключает состояние выбора штампа и обновляет глобальное состояние типа штампа в StampTable.
    /// </summary>
    void OnMouseDown()
    {
        Debug.Log($"Кликнут штамп: {stampName}");
        
        transform.localScale = originalScale * clickScale;
        isStumped = !isStumped;
        StampTable.stampType = GetStamp();
    }
    
    /// <summary>
    /// Восстанавливает исходный масштаб после отпускания кнопки мыши.
    /// </summary>
    void OnMouseUp()
    {
        transform.localScale = originalScale;
    }
    
    /// <summary>
    /// Увеличивает масштаб штампа при наведении курсора для визуальной обратной связи.
    /// </summary>
    void OnMouseEnter()
    {
        transform.localScale = originalScale * hoverScale;
    }
    
    /// <summary>
    /// Восстанавливает исходный масштаб при уходе курсора со штампа.
    /// </summary>
    void OnMouseExit()
    {
        transform.localScale = originalScale;
    }
    
    /// <summary>
    /// Возвращает тип штампа, представленный этим объектом.
    /// Метод может быть переопределен в дочерних классах для реализации дополнительной логики.
    /// </summary>
    /// <returns>Тип штампа, соответствующий этому объекту.</returns>
    public StampType GetStamp()
    {
        Debug.Log($"Применен штамп: {stampName}");
        return stampType;
    }
    
    /// <summary>
    /// Сбрасывает глобальное состояние при уничтожении объекта.
    /// Устанавливает значения по умолчанию для предотвращения сохранения устаревших состояний между сессиями.
    /// </summary>
    void OnDestroy()
    {
        StampTable.stampType = StampType.На_рассмотрении;
        StampTable.shouldCoroutineStop = false;
    }
}
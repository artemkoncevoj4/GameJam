using UnityEngine;
using TaskSystem;
using InteractiveObjects;

// Муштаков А.Ю.

/// <summary>
/// Класс, представляющий отдельный бланк для выбора типа бумаги.
/// Обрабатывает взаимодействие с бланком через мышь и обновляет глобальное состояние выбора.
/// </summary>
public class Blank : MonoBehaviour
{
    [Header("Настройки бланка")]
    [SerializeField] private PaperType _paperType = PaperType.Бланк_формы_7_Б; // Тип бумаги, представленный этим бланком
    
    private float hoverScale = 1.1f; // Масштаб при наведении курсора
    private float clickScale = 0.95f; // Масштаб при клике
    private Vector2 originalScale; // Исходный масштаб объекта

    /// <summary>
    /// Инициализирует компоненты и сохраняет исходные значения.
    /// </summary>
    void Start()
    {
        // Сохраняем оригинальные значения
        originalScale = transform.localScale;
    }
    
    /// <summary>
    /// Обрабатывает клик мыши по бланку.
    /// Устанавливает выбранный тип бумаги и обновляет глобальное состояние.
    /// </summary>
    void OnMouseDown()
    {
        AudioManager.Instance?.PlaySoundByName("paper");

        transform.localScale = originalScale * clickScale;
        ResetBlank();
        
        BlankTable.paperType = _paperType;
        BlankTable.shouldCoroutineStop = true;
    }
    
    /// <summary>
    /// Восстанавливает исходный масштаб после отпускания кнопки мыши.
    /// </summary>
    void OnMouseUp()
    {
        transform.localScale = originalScale;
    }
    
    /// <summary>
    /// Увеличивает масштаб бланка при наведении курсора.
    /// </summary>
    void OnMouseEnter()
    {
        transform.localScale = originalScale * hoverScale;
    }
    
    /// <summary>
    /// Восстанавливает исходный масштаб при уходе курсора с бланка.
    /// </summary>
    void OnMouseExit()
    {
        transform.localScale = originalScale;
    }
    
    /// <summary>
    /// Сбрасывает состояние текущего документа к значениям по умолчанию для выбранного типа бумаги.
    /// Обновляет все свойства документа в соответствии с выбранным бланком.
    /// </summary>
    private void ResetBlank()
    {
        Document _currDoc = TaskManager.Instance.GetCurrentDocument();
        _currDoc.PaperType = _paperType;
        _currDoc.StampPos = StampPosition.Левый_верхний;
        _currDoc.StampType = StampType.На_рассмотрении;
        _currDoc.IsSigned = false;
        _currDoc.IsStamped = false;
        _currDoc.InkColor = InkColor.Зеленые;
    }
    
    /// <summary>
    /// Сбрасывает глобальное состояние при уничтожении объекта.
    /// Важно для предотвращения сохранения устаревших значений между сессиями.
    /// </summary>
    void OnDestroy()
    {
        BlankTable.paperType = PaperType.Бланк_формы_7_Б;
        BlankTable.shouldCoroutineStop = false;
    }
}
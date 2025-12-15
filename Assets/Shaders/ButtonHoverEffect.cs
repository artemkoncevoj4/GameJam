using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

// ! Сгенерировано ИИ

namespace Shaders {
/// <summary>
/// **Эффект наведения курсора для кнопок.**
/// <para>Реализует интерактивный визуальный эффект при наведении курсора на кнопку (Hover Effect), 
/// включая масштабирование, изменение тени и свечение текста. 
/// Требует наличия компонентов <see cref="Button"/> и <see cref="Image"/> на том же GameObject.</para>
/// </summary>
[RequireComponent(typeof(Button), typeof(Image))]
public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Scale Effect")]
    /// <summary>
    /// Коэффициент, на который увеличивается кнопка при наведении (например, 1.05f).
    /// </summary>
    [SerializeField] private float _hoverScale = 1.05f;
    /// <summary>
    /// Скорость плавного возврата масштаба кнопки к исходному состоянию (Lerp speed).
    /// </summary>
    [SerializeField] private float _scaleSpeed = 8f;
    
    [Header("Glow Effect")]
    /// <summary>
    /// Цвет, используемый для "свечения" (Glow) тени при наведении курсора. 
    /// Обычно это яркий цвет с низким альфа-каналом.
    /// </summary>
    [SerializeField] private Color _glowColor = new Color(1f, 1f, 1f, 0.3f);
    
    [Header("Shadow Effect")]
    /// <summary>
    /// Смещение тени, применяемое при наведении курсора для создания эффекта "углубления" или "выделения".
    /// </summary>
    [SerializeField] private Vector2 _shadowOffset = new Vector2(5, -5);
    /// <summary>
    /// Скорость плавного возврата смещения тени к исходному состоянию (Lerp speed).
    /// </summary>
    [SerializeField] private float _shadowSpeed = 8f;
    
    // --- Внутренние поля для хранения исходных значений ---
    private Vector3 _originalScale;
    private Vector2 _originalShadowOffset;
    private Color _originalShadowColor;
    
    // --- Ссылки на компоненты ---
    private Shadow _shadow;
    private Image _image;
    private TextMeshProUGUI _buttonText;
    
    /// <summary>
    /// Инициализация: Сохраняет исходные значения масштаба, смещения и цвета тени. 
    /// Получает ссылки на необходимые компоненты.
    /// </summary>
    void Start()
    {
        _originalScale = transform.localScale;
        
        // Получаем компоненты
        _shadow = GetComponent<Shadow>();
        _image = GetComponent<Image>();
        _buttonText = GetComponentInChildren<TextMeshProUGUI>();
        
        // Сохраняем оригинальные параметры тени, если компонент Shadow присутствует
        if (_shadow != null)
        {
            _originalShadowOffset = _shadow.effectDistance;
            _originalShadowColor = _shadow.effectColor;
        }
    }
    
    /// <summary>
    /// Вызывается каждый кадр: Отвечает за плавную анимацию возврата кнопки 
    /// (масштаб и тень) к исходному состоянию после того, как курсор покинул область кнопки.
    /// </summary>
    void Update()
    {
        // Плавная анимация возврата масштаба к исходному состоянию
        transform.localScale = Vector3.Lerp(transform.localScale, _originalScale, Time.unscaledDeltaTime * _scaleSpeed);
        
        // Плавная анимация возврата тени к исходному смещению
        if (_shadow != null)
        {
            _shadow.effectDistance = Vector2.Lerp(
                _shadow.effectDistance, 
                _originalShadowOffset, 
                Time.unscaledDeltaTime * _shadowSpeed
            );
        }
    }
    
    /// <summary>
    /// **Обработчик события IPointerEnterHandler.**
    /// Вызывается, когда курсор мыши входит в область кнопки. 
    /// Применяет эффект: увеличивает масштаб, усиливает и подсвечивает тень, меняет цвет текста.
    /// </summary>
    /// <param name="eventData">Данные события указателя.</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Увеличиваем кнопку мгновенно (обратный Lerp происходит в Update)
        transform.localScale = _originalScale * _hoverScale;
        
        // Усиливаем тень и меняем ее цвет на свечение
        if (_shadow != null)
        {
            _shadow.effectDistance = _shadowOffset;
            _shadow.effectColor = _glowColor;
        }
        
        // Меняем цвет текста на желтый
        if (_buttonText != null)
        {
            _buttonText.color = Color.yellow;
        }
    }
    
    /// <summary>
    /// **Обработчик события IPointerExitHandler.**
    /// Вызывается, когда курсор мыши покидает область кнопки.
    /// Инициирует возврат к исходному состоянию: меняет цвет текста, а анимация масштаба и тени 
    /// обрабатывается в <see cref="Update"/>.
    /// </summary>
    /// <param name="eventData">Данные события указателя.</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        // Возвращаем текст к исходному цвету (белый)
        if (_buttonText != null)
        {
            _buttonText.color = Color.white;
        }
        
        // Тень и масштаб вернутся в исходное состояние с помощью Lerp в методе Update.
        
        // Дополнительно, мгновенно возвращаем цвет тени к оригиналу, чтобы свечение не "тянулось"
        if (_shadow != null)
        {
            _shadow.effectColor = _originalShadowColor;
        }
    }
}
}
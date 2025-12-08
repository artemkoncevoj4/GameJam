using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
namespace Shaders {
[RequireComponent(typeof(Button), typeof(Image))]
public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Scale Effect")]
    [SerializeField] private float _hoverScale = 1.05f;
    [SerializeField] private float _scaleSpeed = 8f;
    
    [Header("Glow Effect")]
    [SerializeField] private Color _glowColor = new Color(1f, 1f, 1f, 0.3f);
    [SerializeField] private float _glowSpeed = 10f;
    
    [Header("Shadow Effect")]
    [SerializeField] private Vector2 _shadowOffset = new Vector2(5, -5);
    [SerializeField] private float _shadowSpeed = 8f;
    
    private Vector3 _originalScale;
    private Vector2 _originalShadowOffset;
    private Color _originalShadowColor;
    
    private Shadow _shadow;
    private Image _image;
    private TextMeshProUGUI _buttonText;
    
    void Start()
    {
        _originalScale = transform.localScale;
        
        // Получаем компоненты
        _shadow = GetComponent<Shadow>();
        _image = GetComponent<Image>();
        _buttonText = GetComponentInChildren<TextMeshProUGUI>();
        
        if (_shadow != null)
        {
            _originalShadowOffset = _shadow.effectDistance;
            _originalShadowColor = _shadow.effectColor;
        }
    }
    
    void Update()
    {
        // Плавная анимация возврата к исходному состоянию
        transform.localScale = Vector3.Lerp(transform.localScale, _originalScale, Time.unscaledDeltaTime * _scaleSpeed);
        
        if (_shadow != null)
        {
            _shadow.effectDistance = Vector2.Lerp(
                _shadow.effectDistance, 
                _originalShadowOffset, 
                Time.unscaledDeltaTime * _shadowSpeed
            );
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Увеличиваем кнопку
        transform.localScale = _originalScale * _hoverScale;
        
        // Усиливаем тень
        if (_shadow != null)
        {
            _shadow.effectDistance = _shadowOffset;
            _shadow.effectColor = _glowColor;
        }
        
        // Меняем цвет текста
        if (_buttonText != null)
        {
            _buttonText.color = Color.yellow;
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        // Возвращаем текст к исходному цвету
        if (_buttonText != null)
        {
            _buttonText.color = Color.white;
        }
    }
}
}
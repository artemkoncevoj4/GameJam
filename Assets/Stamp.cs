using UnityEngine;
using TaskSystem;
using System;
public class Stamp2D : MonoBehaviour
{
    [Header("Настройки штампа")]
    public string stampName = "Штамп";
    public Color stampColor = Color.red;
    public StampType stampType = StampType.Одобрено;
    
    [Header("Визуальная обратная связь")]
    public float hoverScale = 1.1f;
    public float clickScale = 0.95f;
    
    private Vector2 originalScale;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    public Action<StampType> OnStampPress;
    
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
    
    void OnMouseDown()
    {
        // Обработка клика
        Debug.Log($"Кликнут штамп: {stampName}");
        
        // Визуальная обратная связь
        transform.localScale = originalScale * clickScale;
        
        // Вызываем действие штампа
        OnStampPress?.Invoke(GetStamp());
    }
    
    void OnMouseUp()
    {

        transform.localScale = originalScale;
    }
    
    void OnMouseEnter()
    {

        transform.localScale = originalScale * hoverScale;
    }
    
    void OnMouseExit()
    {
        transform.localScale = originalScale;
    }
    
    // Метод, который можно переопределить для разных штампов
    public StampType GetStamp()
    {
        // Базовая логика - можно переопределить в дочерних классах
        Debug.Log($"Применен штамп: {stampName}");
        return stampType;
    }
}
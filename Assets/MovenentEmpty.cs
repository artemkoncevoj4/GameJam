using UnityEngine;

// Муштаков А.Ю.

/// <summary>
/// Класс, реализующий движение объекта по траектории в форме символа бесконечности.
/// </summary>
public class SimpleInfinity : MonoBehaviour
{
    /// <summary>
    /// Скорость движения объекта по траектории.
    /// Чем выше значение, тем быстрее объект движется по пути.
    /// </summary>
    public float speed = 3f;
    
    /// <summary>
    /// Масштаб траектории движения.
    /// Определяет размер фигуры бесконечности в пространстве.
    /// </summary>
    public float scale = 200f;
    
    /// <summary>
    /// Обновляет позицию объекта каждый кадр, перемещая его по траектории бесконечности.
    /// Позиция рассчитывается на основе времени, скорости и масштаба с использованием синусоидальных функций.
    /// </summary>
    void Update()
    {
        float t = Time.time * speed;
        
        transform.position = new Vector3(
            Mathf.Sin(t) * scale + 734.5f,  
            Mathf.Sin(2f * t) * (scale * 0.5f) + 576f, 
            0                                   
        );
    }
}
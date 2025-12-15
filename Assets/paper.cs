using UnityEngine;
using TaskSystem;
using InteractiveObjects;

// Муштаков А.Ю.

/// <summary>
/// Класс, представляющий интерактивный лист бумаги для размещения штампа.
/// Обрабатывает клики мыши по бумаге для определения позиции штампа и взаимодействует со станцией штамповки.
/// </summary>
public class Paper : MonoBehaviour
{
    //! Метод сгенерирован ИИ

    /// <summary>
    /// Обрабатывает клик мыши по бумаге для определения позиции штампа.
    /// Преобразует экранные координаты в нормализованные и определяет положение штампа на документе.
    /// Взаимодействует со статическими полями Stamp2D и StampTable для управления процессом штамповки.
    /// </summary>
    void OnMouseDown()
    {
        Vector3 viewportMousePosition = Camera.main.ScreenToViewportPoint(Input.mousePosition);

        Debug.Log($"Нормализованные координаты экрана: {viewportMousePosition}");

        if (Stamp2D.isStumped) {
            float normalizedY = viewportMousePosition.y;
            float normalizedX = viewportMousePosition.x;
            
            if (normalizedY < 0.6f && normalizedY > 0.4f)
            {
                StampTable.stampPos = StampPosition.Центр;
            }
            else if (normalizedX < 0.5f) 
            {
                if (normalizedY <= 0.4f)
                {
                    StampTable.stampPos = StampPosition.Левый_нижний;
                }
                else if (normalizedY >= 0.6f)
                {
                    StampTable.stampPos = StampPosition.Левый_верхний;
                }
            }
            else if (normalizedY < 0.4f)
            {
                StampTable.stampPos = StampPosition.Правый_нижний;
            }
            
            StampTable.shouldCoroutineStop = true;
        }
        
        AudioManager.Instance?.PlaySoundByName("stamp");
    }
    
    /// <summary>
    /// Меняет спрайт бумаги в зависимости от её типа (заглушка для будущей реализации).
    /// В текущей реализации метод не выполняет действий, но структура готова для расширения.
    /// </summary>
    /// <param name="paperType">Тип бумаги, который определяет используемый спрайт.</param>
    private void ChangeSprite(PaperType paperType)
    {
        switch (paperType)
        {
            case PaperType.Бланк_формы_7_Б:
                // TODO: Реализовать смену спрайта для формы 7-Б
                break;
            case PaperType.Бланк_формы_АА_Я:
                // TODO: Реализовать смену спрайта для формы АА-Я
                break;
            case PaperType.Карточка:
                // TODO: Реализовать смену спрайта для карточки
                break;
            case PaperType.Пергамент:
                // TODO: Реализовать смену спрайта для пергамента
                break;
            default:

                break;
        }
    }
}
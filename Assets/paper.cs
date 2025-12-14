using UnityEngine;
using TaskSystem;
using InteractiveObjects;
public class Paper : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    void Start()
    {
        // Сохраняем оригинальные значения
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnMouseDown()
{
    // 1. Преобразуем экранные координаты в нормализованные (0.0 до 1.0)
    // Не рекомендуется, если Paper - это игровой объект, но работает для экрана.
    Vector3 viewportMousePosition = Camera.main.ScreenToViewportPoint(Input.mousePosition);

    Debug.Log($"Нормализованные координаты экрана: {viewportMousePosition}");

    if (Stamp2D.isStumped) {
        // Ваши пороговые значения теперь должны быть от 0.0 до 1.0
        float normalizedY = viewportMousePosition.y;
        float normalizedX = viewportMousePosition.x;
        // Примерная адаптация ваших старых значений к Viewport (требует настройки!)
        if (normalizedY < 0.6f && normalizedY > 0.4f) // Центр по Y
        {
            StampTable.stampPos = StampPosition.Центр;
        }
        else if (normalizedX < 0.5f) // Левая сторона
        {
            if (normalizedY <= 0.4f)
            {
                StampTable.stampPos = StampPosition.Левый_нижний;
            }
            else if (normalizedY >= 0.6f) // Это условие выглядит странно, но адаптируем
            {
                StampTable.stampPos = StampPosition.Левый_верхний;
            }
        }
        else if (normalizedY < 0.4f) // Правый нижний
        {
            StampTable.stampPos = StampPosition.Правый_нижний;
        }
        StampTable.shouldCoroutineStop = true;
    }
    AudioManager.Instance?.PlaySoundByName("stamp");
}
    //TODO сделать замену спрайтов
    private void ChangeSprite(PaperType paperType)
    {
        switch (paperType)
        {
            case PaperType.Бланк_формы_7_Б:
                break;
            case PaperType.Бланк_формы_АА_Я:
                break;
            case PaperType.Карточка:
                break;
            case PaperType.Пергамент:
                break;
            default:
                break;
        }
    }

}
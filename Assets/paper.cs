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
        Vector3 mousePosition = Input.mousePosition;
        Debug.Log($"Экранные координаты: {mousePosition}");
        if (Stamp2D.isStumped) {
            if (mousePosition.y < 420 && mousePosition.y > 270)
            {
                StampTable.stampPos = StampPosition.Центр;
            }
            else if (mousePosition.x < 610)
            {
                if (mousePosition.y <= 270)
                {
                    StampTable.stampPos = StampPosition.Левый_нижний;
                }
                else if (mousePosition.x >= 420)
                {
                    StampTable.stampPos = StampPosition.Левый_верхний;
                }
            }
            else if (mousePosition.y < 270)
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
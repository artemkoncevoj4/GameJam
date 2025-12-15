using UnityEngine;

// Муштаков А.Ю.

/// <summary>
/// Управляет порядком отрисовки спрайта предмета относительно игрока для создания эффекта глубины.
/// Изменяет sortingOrder спрайта в зависимости от вертикального положения игрока относительно предмета.
/// </summary>
public class spriteRender : MonoBehaviour
{
    [Header("Preferences")]
    [SerializeField] private Transform playerPos; 
    [SerializeField] private Transform itemPos;    
    [SerializeField] private SpriteRenderer itemSprite;
    [SerializeField] private float offset = 0f; 

    /// <summary>
    /// Обновляет порядок отрисовки спрайта каждый кадр.
    /// Сравнивает вертикальную позицию игрока и предмета (с учетом смещения) и изменяет sortingOrder,
    /// чтобы создать иллюзию глубины: когда игрок ниже предмета, предмет рисуется поверх игрока, и наоборот.
    /// </summary>
    void Update()
    {

        if (playerPos.position.y <= itemPos.position.y + offset)
        {
            itemSprite.sortingOrder = 3; 
        }
        else 
        {
            itemSprite.sortingOrder = 5; 
        }
    }
}
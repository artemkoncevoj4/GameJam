using UnityEngine;

public class spriteRender : MonoBehaviour
{
    [Header("Preferences")]
    [SerializeField] private Transform playerPos;
    [SerializeField] private Transform itemPos;
    [SerializeField] private SpriteRenderer itemSprite;
    [SerializeField] private float offset = 0f;

    void Update()
    {
        if (playerPos.position.y <= itemPos.position.y+offset)
        {
            itemSprite.sortingOrder = 3;
        }
        else
        {
            itemSprite.sortingOrder = 5;
        }
    }
}

using UnityEngine;
using TaskSystem;
using System;
using InteractiveObjects;
public class Blank : MonoBehaviour
{
    [Header("Настройки бланка")]
    public string stampName = "Бланк";
    [SerializeField] private PaperType _paperType = PaperType.Бланк_формы_7_Б;
    private float hoverScale = 1.1f;
    private float clickScale = 0.95f;
    
    private Vector2 originalScale;
    private SpriteRenderer spriteRenderer;
    public static event Action<PaperType> OnBlankUpdate;
    void Start()
    {
        // Сохраняем оригинальные значения
        originalScale = transform.localScale;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    void OnMouseDown()
    {
        // Обработка клика
        Debug.Log($"Кликнут штамп: {stampName}");
        //? paper. DONE?
        AudioManager.Instance?.PlaySoundByName("paper");

        // Визуальная обратная связь
        transform.localScale = originalScale * clickScale;
        ResetBlank();
        BlankTable.paperType = _paperType;
        OnBlankUpdate?.Invoke(_paperType);
        BlankTable.shouldCoroutineStop = true;
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
    void OnDestroy()
    {
        BlankTable.paperType = PaperType.Бланк_формы_7_Б;
        BlankTable.shouldCoroutineStop = false;
    }
    
}
using UnityEngine;
using System.Collections;

// ! Сгенерировано ИИ

public class ScreenShake : MonoBehaviour
{
    [SerializeField] private RectTransform shakeTransform;
    [SerializeField] float shiftAmount = 15f;
    [SerializeField] float shakeDuration = 0.3f;

    void Awake()
    {
        if (shakeTransform == null)
        {
            shakeTransform = GetComponent<RectTransform>();
            if (shakeTransform == null)
            {
                Debug.LogError("<color=red>ScreenShake: RectTransform component is missing! Невозможно работать без RectTransform!</color>");
                return;
            }
            Debug.LogWarning("<color=yellow>ScreenShake: shakeTransform не был назначен вручную. Использование найденного GetComponent<RectTransform>().</color>");
        }
        Debug.Log("<color=green>ScreenShake: Компонент инициализирован.</color>");
    }

    /// <summary>
    /// Запускает эффект тряски.
    /// </summary>
    public void Start_shaking()
    {
        if (shakeTransform == null) // ДОБАВЛЕНО
        {
            Debug.LogError("<color=red>ScreenShake: Start_shaking НЕ ЗАПУЩЕН! RectTransform не назначен. См. лог Awake().</color>");
            return;
        }
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(ShakingCoroutine());
        Debug.Log($"<color=green>ScreenShake: Эффект тряски запущен. Длительность: {shakeDuration:F2} сек, Сила: {shiftAmount:F2}.</color>"); // ИЗМЕНЕНО
    }

    private IEnumerator ShakingCoroutine()
    {
        Vector2 originalPos = shakeTransform.anchoredPosition;
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-shiftAmount, shiftAmount);
            float y = Random.Range(-shiftAmount, shiftAmount);
            
            shakeTransform.anchoredPosition = originalPos + new Vector2(x, y);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Возвращаем в исходную позицию
        shakeTransform.anchoredPosition = originalPos;
    }
}
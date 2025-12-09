using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenBlinker : MonoBehaviour
{
    [SerializeField] private Image blinkImage;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private int blinkCount = 3;
    [SerializeField] private Color blinkColor = Color.red;

    void Awake()
    {
        if (blinkImage == null)
        {
            blinkImage = GetComponent<Image>();
            if (blinkImage == null)
            {
                Debug.LogError("ScreenBlinker: Image component is missing!");
                return;
            }
        }
        SetAlpha(0f);
    }

    /// <summary>
    /// Запускает эффект мигания с настройками по умолчанию (из инспектора).
    /// </summary>
    public void BlinkScreen()
    {
        StopAllCoroutines();
        StartCoroutine(BlinkCoroutine(fadeDuration, blinkCount, blinkColor));
    }

    // --- Методы для ScreenFadeManager ---

    /// <summary>
    /// Запускает мигание с кастомными параметрами.
    /// </summary>
    public void Blink(float duration, int count, Color color)
    {
        StopAllCoroutines();
        StartCoroutine(BlinkCoroutine(duration, count, color));
    }

    /// <summary>
    /// Эффект сердцебиения (пульсация прозрачности).
    /// </summary>
    public void HeartbeatEffect(float intensity, int pulses, float pulseSpeed)
    {
        StopAllCoroutines();
        StartCoroutine(HeartbeatCoroutine(intensity, pulses, pulseSpeed));
    }

    private IEnumerator BlinkCoroutine(float duration, int count, Color color)
    {
        Color originalColor = blinkImage.color;
        blinkImage.color = new Color(color.r, color.g, color.b, 0f);

        for (int i = 0; i < count; i++)
        {
            // Fade in (почти непрозрачный)
            yield return FadeToAlpha(0.7f, duration);
            
            // Fade out
            yield return FadeToAlpha(0f, duration);
        }

        // Возврат к оригинальной прозрачности и цвету
        SetAlpha(0f);
        blinkImage.color = originalColor;
    }

    private IEnumerator HeartbeatCoroutine(float maxAlpha, int pulses, float speed)
    {
        Color originalColor = blinkImage.color;
        // Для сердцебиения часто используют красный оттенок текущего image, или переданный цвет.
        // Здесь используем текущий цвет image, но управляем альфой.
        
        for (int i = 0; i < pulses; i++)
        {
            // Удар (появление)
            yield return FadeToAlpha(maxAlpha, speed);
            // Затухание
            yield return FadeToAlpha(0f, speed);
            // Пауза между ударами
            yield return new WaitForSecondsRealtime(speed * 0.5f);
        }

        SetAlpha(0f);
        blinkImage.color = originalColor;
    }

    private IEnumerator FadeToAlpha(float targetAlpha, float duration)
    {
        float startAlpha = blinkImage.color.a;
        float time = 0f;

        while (time < duration)
        {
            // Используем unscaledDeltaTime для работы при TimeScale = 0 (GameOver)
            time += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(time / duration);
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(targetAlpha);
    }

    private void SetAlpha(float alpha)
    {
        Color c = blinkImage.color;
        c.a = alpha;
        blinkImage.color = c;
    }

    void Update()
    {
        // Нажать B для Blink
        if (Input.GetKeyDown(KeyCode.Z))
        {
            BlinkScreen();
        }
    }
}
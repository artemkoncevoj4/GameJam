using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenBlinker : MonoBehaviour
{
    // ⚠️ Убедитесь, что вы перетащили компонент Image сюда в Инспекторе
    [SerializeField] private Image blinkImage;
    // ⚠️ Назначьте сюда ваш Material (RadialVignetteMaterial)
    [SerializeField] private Material vignetteMaterial; 
    
    // Имя свойства в шейдере для управления интенсивностью
    private const string ALPHA_PROPERTY_NAME = "_AlphaMultiplier"; 
    // Имя свойства в шейдере для управления цветом
    private const string COLOR_PROPERTY_NAME = "_VignetteColor"; 

    [Header("Настройки мигания")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private int blinkCount = 3;
    // Менее яркий, приглушенный красный
    [SerializeField] private Color blinkColor = new Color(0.8f, 0.2f, 0.2f); 
    [SerializeField] private float maxVignetteIntensity = 0.7f; // Максимальная интенсивность (Max Alpha Multiplier)

    private Material runtimeMaterial; // Инстанс материала для безопасного изменения

    void Awake()
    {
        if (blinkImage == null)
        {
            blinkImage = GetComponent<Image>();
            if (blinkImage == null)
            {
                Debug.LogError("<color=red>ScreenBlinker: Image component is missing!</color>");
                return;
            }
        }

        if (vignetteMaterial == null)
        {
            Debug.LogError("<color=red>ScreenBlinker: vignetteMaterial не назначен! Назначьте ваш RadialVignetteMaterial!</color>");
            return;
        }
        
        // 1. Создаем инстанс материала, чтобы не менять оригинальный ассет
        runtimeMaterial = new Material(vignetteMaterial);
        // 2. Назначаем инстанс на UI Image
        blinkImage.material = runtimeMaterial;
        
        // 3. Устанавливаем цвет и сбрасываем интенсивность
        runtimeMaterial.SetColor(COLOR_PROPERTY_NAME, blinkColor); 
        SetAlphaMultiplier(0f);
        
        // Скрываем стандартный цвет Image, чтобы не мешал шейдеру
        blinkImage.color = Color.white; 
        
        Debug.Log("<color=green>ScreenBlinker: Инициализация завершена. Использование шейдера виньетки.</color>");
    }

    /// <summary>
    /// Запускает эффект мигания с настройками по умолчанию (из инспектора).
    /// </summary>
    public void BlinkScreen()
    {
        StopAllCoroutines();
        StartCoroutine(BlinkCoroutine(fadeDuration, blinkCount, blinkColor, maxVignetteIntensity));
    }

    /// <summary>
    /// Запускает мигание с кастомными параметрами.
    /// </summary>
    public void Blink(float duration, int count, Color color, float maxIntensity)
    {
        StopAllCoroutines();
        StartCoroutine(BlinkCoroutine(duration, count, color, maxIntensity));
    }

    /// <summary>
    /// Эффект сердцебиения (пульсация интенсивности виньетки).
    /// </summary>
    public void HeartbeatEffect(float intensity, int pulses, float pulseSpeed)
    {
        StopAllCoroutines();
        StartCoroutine(HeartbeatCoroutine(intensity, pulses, pulseSpeed));
    }

    private IEnumerator BlinkCoroutine(float duration, int count, Color color, float maxIntensity)
    {
        runtimeMaterial.SetColor(COLOR_PROPERTY_NAME, color);

        for (int i = 0; i < count; i++)
        {
            yield return FadeToAlphaMultiplier(maxIntensity, duration);
            yield return FadeToAlphaMultiplier(0f, duration);
        }

        SetAlphaMultiplier(0f);
    }

    private IEnumerator HeartbeatCoroutine(float maxIntensity, int pulses, float speed)
    {
        runtimeMaterial.SetColor(COLOR_PROPERTY_NAME, blinkColor);
        
        for (int i = 0; i < pulses; i++)
        {
            yield return FadeToAlphaMultiplier(maxIntensity, speed);
            yield return FadeToAlphaMultiplier(0f, speed);
            yield return new WaitForSecondsRealtime(speed * 0.5f);
        }

        SetAlphaMultiplier(0f);
    }

    private IEnumerator FadeToAlphaMultiplier(float targetAlpha, float duration)
    {
        float startAlpha = runtimeMaterial.GetFloat(ALPHA_PROPERTY_NAME);
        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(time / duration);
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
            SetAlphaMultiplier(alpha);
            yield return null;
        }
        
        SetAlphaMultiplier(targetAlpha);
    }

    private void SetAlphaMultiplier(float alpha)
    {
        if (runtimeMaterial != null)
        {
            // Устанавливаем значение для свойства _AlphaMultiplier в шейдере
            runtimeMaterial.SetFloat(ALPHA_PROPERTY_NAME, alpha);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            BlinkScreen();
        }
        
        if (Input.GetKeyDown(KeyCode.X))
        {
            HeartbeatEffect(0.5f, 5, 0.3f);
        }
    }
}
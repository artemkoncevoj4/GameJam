using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;

    void Awake()
    {
        // Попытка найти Image, если не установлен вручную
        if (fadeImage == null)
        {
            fadeImage = GetComponent<Image>();
            if (fadeImage == null)
            {
                Debug.LogError("<color=red>ScreenFader: Image component is missing! Невозможно работать без Image!</color>");
                return;
            }
            Debug.LogWarning("<color=yellow>ScreenFader: fadeImage не был назначен вручную. Использование найденного GetComponent<Image>().</color>");
        }
        
        SetAlpha(0f);
        Debug.Log("<color=green>ScreenFader: Компонент инициализирован. Alpha сброшена к 0.</color>");
    }

    /// <summary>
    /// Затемняет экран до полной непрозрачности (Alpha = 1), используя стандартную длительность.
    /// </summary>
    public void Fade_in()
    {
        StopAllCoroutines();
        StartCoroutine(Fade(1f, fadeDuration));
    }

    /// <summary>
    /// Осветляет экран до полной прозрачности (Alpha = 0), используя стандартную длительность.
    /// </summary>
    public void Fade_out()
    {
        StopAllCoroutines();
        StartCoroutine(Fade(0f, fadeDuration));
    }
    
    // --- Методы для ScreenFadeManager ---

    /// <summary>
    /// Запуск эффекта с указанной длительностью.
    /// </summary>
    public void StartFade(float targetAlpha, float duration)
    {
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(Fade(targetAlpha, duration));
    }

    /// <summary>
    /// Быстрое затемнение до черного (или текущего цвета) за заданное время.
    /// </summary>
    public void QuickFadeToBlack(float duration)
    {
        StartFade(1f, duration);
    }

    /// <summary>
    /// Устанавливает цвет затемнения.
    /// </summary>
    public void SetFadeColor(Color color)
    {
        if (fadeImage != null)
        {
            // Сохраняем текущую прозрачность, меняем только RGB
            float currentAlpha = fadeImage.color.a;
            fadeImage.color = new Color(color.r, color.g, color.b, currentAlpha);
        }
    }

    private IEnumerator Fade(float targetAlpha, float duration)
    {
        float startAlpha = fadeImage.color.a;
        float time = 0f;
          Debug.Log("<color=green>Fade эффект запущен</color>");
        while (time < duration)
        {
            // Используем unscaledDeltaTime на случай, если игра на паузе или TimeScale=0 при GameOver
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
        Color c = fadeImage.color;
        c.a = alpha;
        fadeImage.color = c;
    }

    void Update()
    {
        // Нажать F для Fade_out (осветления)
        if (Input.GetKeyDown(KeyCode.F))
        {
            Fade_out();
        }
        // Нажать G для Fade_in (затемнения)
        if (Input.GetKeyDown(KeyCode.X))
        {
            Fade_in();
        }
    }
}
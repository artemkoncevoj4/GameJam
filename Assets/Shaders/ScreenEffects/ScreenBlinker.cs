using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Shaders.ScreenEffects
{
    public class ScreenBlinker : MonoBehaviour
    {
        public static ScreenBlinker Instance { get; private set; }

        [Header("Blink Settings")]
        [SerializeField] private Image Image;
        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private float pauseDuration = 0.2f;
        [SerializeField] private int blinkCount = 3;
        [SerializeField] private Color blinkColor = Color.red;

        private Coroutine blinkCoroutine;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (Image == null)
            {
                Image = GetComponent<Image>();
                if (Image == null)
                {
                    Debug.LogError("ScreenBlinker: No Image component found!");
                    return;
                }
            }

            SetAlpha(0f);
        }

        public static void StartBlink()
        {
            if (Instance != null)
            {
                Instance.Blink();
            }
            else
            {
                Debug.LogError("ScreenBlinker instance not found!");
            }
        }

        /// <summary>
        /// Запускает эффект сердцебиения (короткие пульсации)
        /// </summary>
        public static void StartHeartbeatEffect(float intensity = 0.3f, int pulses = 2, float pulseSpeed = 0.15f)
        {
            if (Instance != null)
            {
                Instance.HeartbeatEffect(intensity, pulses, pulseSpeed);
            }
        }

        public void Blink()
        {
            if (blinkCoroutine != null)
                StopCoroutine(blinkCoroutine);
            
            blinkCoroutine = StartCoroutine(BlinkCoroutine());
        }

        public void Blink(float customDuration, int customCount, Color customColor)
        {
            fadeDuration = customDuration;
            blinkCount = customCount;
            blinkColor = customColor;
            Blink();
        }

        /// <summary>
        /// Эффект сердцебиения - быстрые пульсации
        /// </summary>
        public void HeartbeatEffect(float intensity = 0.3f, int pulses = 2, float pulseSpeed = 0.15f)
        {
            if (blinkCoroutine != null)
                StopCoroutine(blinkCoroutine);
            
            blinkCoroutine = StartCoroutine(HeartbeatCoroutine(intensity, pulses, pulseSpeed));
        }

        private IEnumerator BlinkCoroutine()
        {
            Color originalColor = Image.color;
            Image.color = new Color(blinkColor.r, blinkColor.g, blinkColor.b, 0f);

            for (int i = 0; i < blinkCount; i++)
            {
                // Fade in
                yield return FadeToAlpha(0.7f, fadeDuration);
                
                // Pause
                yield return new WaitForSeconds(pauseDuration);
                
                // Fade out
                yield return FadeToAlpha(0f, fadeDuration);
                
                // Pause between blinks
                if (i < blinkCount - 1)
                    yield return new WaitForSeconds(pauseDuration);
            }

            // Return to original color
            Image.color = originalColor;
        }

        private IEnumerator HeartbeatCoroutine(float intensity, int pulses, float pulseSpeed)
        {
            // Цвет для эффекта сердцебиения - темный, почти черный
            Color heartbeatColor = new Color(0.1f, 0.1f, 0.1f, 0f);
            
            // Сохраняем оригинальный цвет
            Color originalColor = Image.color;
            
            // Устанавливаем цвет для сердцебиения
            Image.color = heartbeatColor;
            
            for (int i = 0; i < pulses; i++)
            {
                // Быстрое затемнение (сокращенное время)
                yield return FadeToAlpha(intensity, pulseSpeed);
                
                // Быстрое осветление (сокращенное время)
                yield return FadeToAlpha(0f, pulseSpeed);
                
                // Короткая пауза между пульсациями (меньше, чем в обычном мигании)
                if (i < pulses - 1)
                    yield return new WaitForSeconds(pulseSpeed * 0.5f);
            }
            
            // Возвращаем прозрачность
            SetAlpha(0f);
        }

        private IEnumerator FadeToAlpha(float targetAlpha, float duration)
        {
            float startAlpha = Image.color.a;
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
                SetAlpha(alpha);
                yield return null;
            }

            SetAlpha(targetAlpha);
        }

        private void SetAlpha(float alpha)
        {
            Color c = Image.color;
            c.a = alpha;
            Image.color = c;
        }

        public void StopBlink()
        {
            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
                blinkCoroutine = null;
            }
            SetAlpha(0f);
        }
    }
}
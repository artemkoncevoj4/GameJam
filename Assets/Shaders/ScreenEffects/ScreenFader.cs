using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Shaders.ScreenEffects
{
    public class ScreenFader : MonoBehaviour, IScreenFader
    {
        [Header("Fade Settings")]
        [SerializeField] private Image fadeImage;
        [SerializeField] private float fadeDuration = 2f;
        [SerializeField] private Color fadeColor = Color.black;
        
        [Header("Advanced Features")]
        [SerializeField] private bool useCurve = false;
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private bool disableRaycastWhenTransparent = true;

        private bool isFading = false;
        private Coroutine currentFadeCoroutine;

        public static ScreenFader Instance { get; private set; }

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

            InitializeImage();
        }

        private void InitializeImage()
        {
            if (fadeImage == null)
            {
                fadeImage = GetComponent<Image>();
                if (fadeImage == null)
                {
                    Debug.LogError("ScreenFader: No Image component found!");
                    return;
                }
            }

            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
            UpdateRaycastTarget();
        }

        private void UpdateRaycastTarget()
        {
            if (disableRaycastWhenTransparent)
            {
                fadeImage.raycastTarget = fadeImage.color.a > 0.01f;
            }
        }

        public void FadeIn() => StartFade(1f);
        public void FadeOut() => StartFade(0f);
        
        public void FadeScreen()
        {
            if (currentFadeCoroutine != null)
                StopCoroutine(currentFadeCoroutine);
            
            currentFadeCoroutine = StartCoroutine(FadeScreenSequence());
        }

        private IEnumerator FadeScreenSequence()
        {
            FadeIn();
            yield return new WaitForSeconds(1f);
            FadeOut();
        }

        public void StartFade() => StartFade(1f);
        
        public void StartFade(float duration)
        {
            fadeDuration = duration;
            StartFade(1f);
        }

        public void StartFade(float targetAlpha, float? customDuration = null)
        {
            if (isFading && currentFadeCoroutine != null)
                StopCoroutine(currentFadeCoroutine);
            
            currentFadeCoroutine = StartCoroutine(FadeCoroutine(targetAlpha, customDuration));
        }

        private IEnumerator FadeCoroutine(float targetAlpha, float? customDuration = null)
        {
            isFading = true;
            float duration = customDuration ?? fadeDuration;
            float timer = 0f;
            
            Color startColor = fadeImage.color;
            Color targetColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, targetAlpha);

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float progress = Mathf.Clamp01(timer / duration);
                
                if (useCurve)
                    progress = fadeCurve.Evaluate(progress);
                
                fadeImage.color = Color.Lerp(startColor, targetColor, progress);
                UpdateRaycastTarget();
                yield return null;
            }

            fadeImage.color = targetColor;
            UpdateRaycastTarget();
            isFading = false;
        }

        public void ResetFade()
        {
            if (currentFadeCoroutine != null)
                StopCoroutine(currentFadeCoroutine);
            
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
            UpdateRaycastTarget();
            isFading = false;
        }

        public bool IsFading() => isFading;

        // Статический метод для быстрого доступа
        public static void FadeToBlack(float duration = 2f)
        {
            if (Instance != null)
                Instance.StartFade(1f, duration);
        }

        public static void FadeFromBlack(float duration = 2f)
        {
            if (Instance != null)
                Instance.StartFade(0f, duration);
        }

        // Дополнительные методы
        public void SetFadeColor(Color color)
        {
            fadeColor = color;
            float currentAlpha = fadeImage.color.a;
            fadeImage.color = new Color(color.r, color.g, color.b, currentAlpha);
        }

        public float GetCurrentAlpha() => fadeImage.color.a;
        
        public void InstantFade(float alpha)
        {
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            UpdateRaycastTarget();
        }
    }
}
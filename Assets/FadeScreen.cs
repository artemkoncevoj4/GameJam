using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shaders
{
    public class ScreenFader : MonoBehaviour
    {
        public ScreenFader Instance { get; private set; }

        [SerializeField] private Image fadeImage;
        [SerializeField] private float fadeDuration = 1f;

        void Awake()
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

            if (fadeImage == null)
            {
                Debug.LogError("ScreenFader: fadeImage doesn't exist!");
                return;
            }

            SetAlpha(0f);
        }
        /*
        public static void FadeScreen()
        {
            if (Instance != null)
            {
                Instance.StartCoroutine(Instance.FadeScreenCoroutine());
            }
            else
            {
                Debug.LogError("ScreenFader instance not found!");
            }
        }*/

        public IEnumerator FadeScreenCoroutine()
        {
            Fade_in();
            yield return new WaitForSeconds(3f);
            Fade_out();
        }

        public void Fade_in()
        {
            StartCoroutine(Fade(0.7f));
        }

        public void Fade_out()
        {
            StartCoroutine(Fade(0f));
        }

        private IEnumerator Fade(float targetAlpha)
        {
            float startAlpha = fadeImage.color.a;
            float time = 0f;

            while (time < fadeDuration)
            {
                float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
                SetAlpha(alpha);
                time += Time.deltaTime;
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
    }
}
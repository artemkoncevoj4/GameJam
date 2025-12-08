using UnityEngine;

namespace ScreenEffects
{
    public class ScreenFadeManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ScreenFader screenFader;
        [SerializeField] private ScreenBlinker screenBlinker;

        public static ScreenFadeManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // Публичные методы для Fade
        public void FadeIn(float duration = 2f) => screenFader?.StartFade(1f, duration);
        public void FadeOut(float duration = 2f) => screenFader?.StartFade(0f, duration);
        
        // Публичные методы для Blink
        public void BlinkScreen() => screenBlinker?.Blink();
        public void BlinkScreen(float duration, int count, Color color) => 
            screenBlinker?.Blink(duration, count, color);
        
        // Статические методы для удобства
        public static void StaticFadeIn(float duration = 2f) => 
            Instance?.FadeIn(duration);
        
        public static void StaticFadeOut(float duration = 2f) => 
            Instance?.FadeOut(duration);
        
        public static void StaticBlink() => 
            Instance?.BlinkScreen();
    }
}
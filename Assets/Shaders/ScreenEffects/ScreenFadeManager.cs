using UnityEngine;

namespace Shaders.ScreenEffects
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

        // --- Fade Methods (Public) ---

        // Общее затемнение (FadeIn/FadeOut)
        public void FadeIn(float duration = 2f) => screenFader?.StartFade(1f, duration);
        public void FadeOut(float duration = 2f) => screenFader?.StartFade(0f, duration);

        /// <summary>
        /// Быстрое затемнение до полного альфа (используется для завершения игры).
        /// </summary>
        public void QuickFadeToBlack(float duration) => screenFader?.QuickFadeToBlack(duration);

        /// <summary>
        /// Установка цвета для затемняющего Image (например, красный для поражения).
        /// </summary>
        public void SetFaderColor(Color color) => screenFader?.SetFadeColor(color);
        
        // --- Blink Methods (Public) ---

        // Стандартное мигание (использует настройки из инспектора Blinker)
        public void BlinkScreen() => screenBlinker?.BlinkScreen();
        
        // Мигание с параметрами (перегрузка для динамических эффектов)
        public void BlinkScreen(float duration, int count, Color color) => 
            screenBlinker?.Blink(duration, count, color);
            
        /// <summary>
        /// Эффект сердцебиения (используется в GameCycle).
        /// </summary>
        public void HeartbeatEffect(float intensity, int pulses, float pulseSpeed) => 
            screenBlinker?.HeartbeatEffect(intensity, pulses, pulseSpeed);
        
        // --- Static Methods (Convenience) ---

        public static void StaticFadeIn(float duration = 2f) => 
            Instance?.FadeIn(duration);
        
        public static void StaticFadeOut(float duration = 2f) => 
            Instance?.FadeOut(duration);
        
        public static void StaticQuickFadeToBlack(float duration) => 
            Instance?.QuickFadeToBlack(duration);
        
        public static void StaticSetFaderColor(Color color) => 
            Instance?.SetFaderColor(color);
        
        public static void StaticBlink() => 
            Instance?.BlinkScreen();
            
        public static void StaticBlink(float duration, int count, Color color) => 
            Instance?.BlinkScreen(duration, count, color);

        public static void StaticHeartbeat(float intensity, int pulses, float pulseSpeed) => 
            Instance?.HeartbeatEffect(intensity, pulses, pulseSpeed);
    }
}
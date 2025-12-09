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
       public void FadeIn(float duration = 2f)
        {
            if (screenFader == null) {
                Debug.LogError("<color=red>ScreenFadeManager: FadeIn НЕ ЗАПУЩЕН! Ссылка на ScreenFader не назначена в Инспекторе!</color>");
                return;
            }
            screenFader.StartFade(1f, duration);
            Debug.Log($"<color=green>ScreenFadeManager: Запуск FadeIn (к Alpha=1) за {duration:F2} сек.</color>");
        }
        public void FadeOut(float duration = 2f)
        {
            if (screenFader == null) {
                Debug.LogError("<color=red>ScreenFadeManager: FadeOut НЕ ЗАПУЩЕН! Ссылка на ScreenFader не назначена в Инспекторе!</color>");
                return;
            }
            screenFader.StartFade(0f, duration);
            Debug.Log($"<color=green>ScreenFadeManager: Запуск FadeOut (к Alpha=0) за {duration:F2} сек.</color>");
        }

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
       public void BlinkScreen()
        {
            if (screenBlinker == null) {
                Debug.LogError("<color=red>ScreenFadeManager: BlinkScreen НЕ ЗАПУЩЕН! Ссылка на ScreenBlinker не назначена в Инспекторе!</color>");
                return;
            }
            screenBlinker.BlinkScreen();
            Debug.Log("<color=green>ScreenFadeManager: Запуск BlinkScreen (стандартные параметры).</color>");
        }
        
        // Мигание с параметрами (перегрузка для динамических эффектов)
       public void BlinkScreen(float duration, int count, Color color)
        {
            if (screenBlinker == null) {
                Debug.LogError("<color=red>ScreenFadeManager: BlinkScreen(params) НЕ ЗАПУЩЕН! Ссылка на ScreenBlinker не назначена в Инспекторе!</color>");
                return;
            }
            //* Новая версия, с интенсивностью
            screenBlinker.Blink(duration, count, color); 
            Debug.Log($"<color=green>ScreenFadeManager: Запуск BlinkScreen. Длительность: {duration:F2}, Кол-во: {count}.</color>");
        }
            
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
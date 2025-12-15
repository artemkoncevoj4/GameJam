using UnityEngine;

// * Идрисов Д.С

namespace Shaders.ScreenEffects
{
    /// <summary>
    /// **Менеджер визуальных эффектов экрана (Singleton Фасад).**
    /// <para>Работает как централизованный Singleton-фасад, предоставляя удобный API для управления основными 
    /// полноэкранными визуальными эффектами: плавным затемнением/проявлением (<see cref="ScreenFader"/>) 
    /// и краткосрочными эффектами мигания/пульсации (<see cref="ScreenBlinker"/>).</para>
    /// <para>Скрывает прямые ссылки на компоненты шейдеров и обеспечивает простой, 
    /// глобально доступный способ управления визуальным состоянием экрана из любого места кода.</para>
    /// </summary>
    public class ScreenFadeManager : MonoBehaviour
    {
        [Header("References")]
        /// <summary>
        /// Компонент, отвечающий за плавное изменение альфа-канала полноэкранного изображения (Fade In/Fade Out).
        /// </summary>
        [SerializeField] private ScreenFader screenFader;
        /// <summary>
        /// Компонент, отвечающий за краткосрочные эффекты мигания или пульсации (Blink/Heartbeat) экрана.
        /// </summary>
        [SerializeField] private ScreenBlinker screenBlinker;

        /// <summary>
        /// Статический экземпляр Singleton, обеспечивающий глобальный доступ к менеджеру.
        /// </summary>
        public static ScreenFadeManager Instance { get; private set; }

        /// <summary>
        /// Инициализирует экземпляр Singleton. Гарантирует, что в сцене существует только один менеджер.
        /// </summary>
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

        /// <summary>
        /// Запускает плавное затемнение экрана (увеличение альфа-канала до полной непрозрачности, Alpha = 1).
        /// </summary>
        /// <param name="duration">Продолжительность перехода в секундах (по умолчанию 2 секунды).</param>
       public void FadeIn(float duration = 2f)
        {
            if (screenFader == null) {
                Debug.LogError("<color=red>ScreenFadeManager: FadeIn НЕ ЗАПУЩЕН! Ссылка на ScreenFader не назначена в Инспекторе!</color>");
                return;
            }
            screenFader.StartFade(1f, duration);
            Debug.Log($"<color=green>ScreenFadeManager: Запуск FadeIn (к Alpha=1) за {duration:F2} сек.</color>");
        }
        
        /// <summary>
        /// Запускает плавное проявление экрана (уменьшение альфа-канала до полной прозрачности, Alpha = 0).
        /// </summary>
        /// <param name="duration">Продолжительность перехода в секундах (по умолчанию 2 секунды).</param>
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
        /// Быстрое, мгновенное затемнение до полной непрозрачности (Alpha = 1).
        /// Идеально подходит для мгновенной смены сцены или критического завершения игры.
        /// </summary>
        /// <param name="duration">Продолжительность перехода.</param>
        public void QuickFadeToBlack(float duration) => screenFader?.QuickFadeToBlack(duration);

        /// <summary>
        /// Устанавливает цвет для полноэкранного затемняющего изображения.
        /// Используется, например, для красного затемнения при поражении или белого для флешбэков.
        /// </summary>
        /// <param name="color">Целевой цвет (RGB).</param>
        public void SetFaderColor(Color color) => screenFader?.SetFadeColor(color);
        
        // --- Blink Methods (Public) ---

        /// <summary>
        /// Запускает эффект мигания экрана, используя параметры, установленные в инспекторе компонента <see cref="ScreenBlinker"/>.
        /// </summary>
       public void BlinkScreen()
        {
            if (screenBlinker == null) {
                Debug.LogError("<color=red>ScreenFadeManager: BlinkScreen НЕ ЗАПУЩЕН! Ссылка на ScreenBlinker не назначена в Инспекторе!</color>");
                return;
            }
            screenBlinker.BlinkScreen();
            Debug.Log("<color=green>ScreenFadeManager: Запуск BlinkScreen (стандартные параметры).</color>");
        }
        
        /// <summary>
        /// Запускает эффект мигания экрана с заданными параметрами.
        /// </summary>
        /// <param name="duration">Общая длительность всего цикла мигания.</param>
        /// <param name="count">Общее количество миганий.</param>
        /// <param name="color">Цвет, используемый для мигания.</param>
       public void BlinkScreen(float duration, int count, Color color)
        {
            if (screenBlinker == null) {
                Debug.LogError("<color=red>ScreenFadeManager: BlinkScreen(params) НЕ ЗАПУЩЕН! Ссылка на ScreenBlinker не назначена в Инспекторе!</color>");
                return;
            }
            screenBlinker.Blink(duration, count, color); 
            Debug.Log($"<color=green>ScreenFadeManager: Запуск BlinkScreen. Длительность: {duration:F2}, Кол-во: {count}.</color>");
        }
            
        /// <summary>
        /// Запускает эффект пульсации экрана, имитирующий сердцебиение.
        /// Используется для критических игровых ситуаций (например, в <c>GameCycle</c>).
        /// </summary>
        /// <param name="intensity">Максимальная сила пульсации (значение альфа).</param>
        /// <param name="pulses">Количество импульсов пульсации.</param>
        /// <param name="pulseSpeed">Скорость каждого отдельного импульса.</param>
        public void HeartbeatEffect(float intensity, int pulses, float pulseSpeed) => 
            screenBlinker?.HeartbeatEffect(intensity, pulses, pulseSpeed);
        
        // --- Static Methods (Convenience) ---
        // Эти статические методы позволяют обращаться к менеджеру без явного получения его экземпляра.

        /// <summary>
        /// Статический метод для вызова <see cref="FadeIn(float)"/> через экземпляр Singleton.
        /// </summary>
        /// <param name="duration">Продолжительность перехода в секундах (по умолчанию 2 секунды).</param>
        public static void StaticFadeIn(float duration = 2f) => 
            Instance?.FadeIn(duration);
        
        /// <summary>
        /// Статический метод для вызова <see cref="FadeOut(float)"/> через экземпляр Singleton.
        /// </summary>
        /// <param name="duration">Продолжительность перехода в секундах (по умолчанию 2 секунды).</param>
        public static void StaticFadeOut(float duration = 2f) => 
            Instance?.FadeOut(duration);
        
        /// <summary>
        /// Статический метод для вызова <see cref="QuickFadeToBlack(float)"/> через экземпляр Singleton.
        /// </summary>
        /// <param name="duration">Продолжительность перехода.</param>
        public static void StaticQuickFadeToBlack(float duration) => 
            Instance?.QuickFadeToBlack(duration);
        
        /// <summary>
        /// Статический метод для вызова <see cref="SetFaderColor(Color)"/> через экземпляр Singleton.
        /// </summary>
        /// <param name="color">Целевой цвет (RGB).</param>
        public static void StaticSetFaderColor(Color color) => 
            Instance?.SetFaderColor(color);
        
        /// <summary>
        /// Статический метод для вызова <see cref="BlinkScreen()"/> через экземпляр Singleton.
        /// </summary>
        public static void StaticBlink() => 
            Instance?.BlinkScreen();
            
        /// <summary>
        /// Статический метод для вызова <see cref="BlinkScreen(float, int, Color)"/> через экземпляр Singleton.
        /// </summary>
        /// <param name="duration">Общая длительность всего цикла мигания.</param>
        /// <param name="count">Общее количество миганий.</param>
        /// <param name="color">Цвет, используемый для мигания.</param>
        public static void StaticBlink(float duration, int count, Color color) => 
            Instance?.BlinkScreen(duration, count, color);

        /// <summary>
        /// Статический метод для вызова <see cref="HeartbeatEffect(float, int, float)"/> через экземпляр Singleton.
        /// </summary>
        /// <param name="intensity">Максимальная сила пульсации (значение альфа).</param>
        /// <param name="pulses">Количество импульсов пульсации.</param>
        /// <param name="pulseSpeed">Скорость каждого отдельного импульса.</param>
        public static void StaticHeartbeat(float intensity, int pulses, float pulseSpeed) => 
            Instance?.HeartbeatEffect(intensity, pulses, pulseSpeed);
    }
}
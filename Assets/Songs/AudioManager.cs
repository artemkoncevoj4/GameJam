using UnityEngine;
using UnityEngine.Audio;
// Класс, который будет управлять всеми звуками
public class AudioManager : MonoBehaviour
{
    // Статическая переменная для доступа к AudioManager из любого места (Singleton Pattern)
  public static AudioManager Instance;

    // Ссылки на Audio Source
    [Header("Источники звука")]
    [Tooltip("Источник для коротких звуковых эффектов (PlayOneShot)")]
    private AudioSource sfxSource; // Главный AudioSource для SFX (будем искать его)
    
    [Tooltip("Источник для фоновой музыки")]
    public AudioSource musicSource; // Второй AudioSource для музыки (нужно назначить вручную)
    [Header("Настройки Аудио Mixer")]
    [Tooltip("Ссылка на ваш главный Audio Mixer")]
    public AudioMixer masterMixer; // <-- НОВАЯ ПУБЛИЧНАЯ ПЕРЕМЕННАЯ
    [Header("Звуковые Клипы (SFX)")]
    public AudioClip spawnSound;
    public AudioClip clickSound;
    public AudioClip explosionSound;

    // [НОВОЕ] Клипы для эффекта Хаоса
    [Header("Звуковые Клипы (Хаос)")]
    public AudioClip glitchSfx;
    public AudioClip horrorSting;
    public AudioClip creepyTensionRise;
    public AudioClip scaryHitsRisers;

    // [НОВОЕ] Клип для фоновой музыки
    [Header("Музыка")]
    public AudioClip backgroundMusicClip; // action-music-loop-with-dark-ambient-drones.wav

    // Список всех клипов Хаоса для случайного выбора
    private AudioClip[] chaosClips;

   private void Awake()
    {
        // Устанавливаем себя как единственный экземпляр
        if (Instance == null)
        {
            Instance = this;
            // Ищем первый AudioSource (он будет для SFX)
            AudioSource[] sources = GetComponents<AudioSource>();
            
            // [ИСПРАВЛЕНО] Логика получения SFX Source
            if (sources.Length > 0)
            {
                sfxSource = sources[0];
            }
            else
            {
                // Если нет ни одного AudioSource, добавляем для SFX
                sfxSource = gameObject.AddComponent<AudioSource>();
                Debug.LogWarning("<color=orange>AudioManager: AudioSource для SFX добавлен динамически.</color>");
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // [ИСПРАВЛЕНО] actionMusicLoop (которого нет в полях) заменен на backgroundMusicClip
        // Инициализация массива клипов Хаоса
        chaosClips = new AudioClip[]
        {
            glitchSfx,
            horrorSting,
            creepyTensionRise,
            scaryHitsRisers
        };
    }
    
    // [НОВОЕ] Start для автоматического запуска фоновой музыки
    void Start()
    {
        PlayBackgroundMusic();
    }
    
    // Метод для запуска фоновой музыки
    public void PlayBackgroundMusic()
    {
        if (musicSource == null)
        {
            Debug.LogError("<color=red>AudioManager: Music Source не назначен! Фоновая музыка не будет воспроизводиться.</color>");
            return;
        }

        if (backgroundMusicClip == null)
        {
             Debug.LogWarning("<color=orange>AudioManager: Клип фоновой музыки не назначен.</color>");
             return;
        }

        // Настройка и запуск музыки на Music Source
        musicSource.clip = backgroundMusicClip;
        musicSource.loop = true; // Убеждаемся, что музыка повторяется
        musicSource.Play();
        Debug.Log("<color=green>AudioManager: Фоновая музыка запущена.</color>");
    }

    public void PlayRandomChaosSound()
    {
        // Проверка на ошибки (Guard Clause)
        if (sfxSource == null)
        {
            Debug.LogError("<color=red>AudioManager: AudioSource для SFX не найден! Невозможно воспроизвести звук.</color>");
            return;
        }

        // Выбираем случайный клип из массива
        if (chaosClips.Length > 0)
        {
            // Убираем null-ссылки, если не все клипы были назначены в Inspector
            int validClipCount = 0;
            for (int i = 0; i < chaosClips.Length; i++)
            {
                if (chaosClips[i] != null)
                {
                    validClipCount++;
                }
            }

            if (validClipCount == 0)
            {
                Debug.LogWarning("<color=orange>AudioManager: В массиве chaosClips нет назначенных AudioClip. Звук хаоса не воспроизведен.</color>");
                return;
            }

            // Выбираем случайный *валидный* клип
            AudioClip clipToPlay = null;
            // Удостоверяемся, что выбранный клип не null
            int attempts = 0;
            while (clipToPlay == null && attempts < 100) // Ограничиваем попытки на всякий случай
            {
                clipToPlay = chaosClips[UnityEngine.Random.Range(0, chaosClips.Length)];
                attempts++;
            }

            if (clipToPlay != null)
            {
                 // Воспроизведение звука один раз
                sfxSource.PlayOneShot(clipToPlay);
                Debug.Log($"<color=green>AudioManager: Воспроизведен звук хаоса: {clipToPlay.name}</color>");
            }
            else
            {
                 Debug.LogWarning("<color=orange>AudioManager: Не удалось найти валидный клип в массиве chaosClips.</color>");
            }
        }
        else
        {
            Debug.LogWarning("<color=orange>AudioManager: Массив chaosClips пуст. Звук хаоса не воспроизведен.</color>");
        }
    }
    // --- 2. Публичный метод для воспроизведения звука ---
    // Этот метод вызывается из других скриптов
    public void PlaySpawnSound()
    {
        // Проверяем, что AudioSource и AudioClip существуют
        if (sfxSource != null && spawnSound != null)
        {
            sfxSource.PlayOneShot(spawnSound);
        }
    }
    
    public void PlayClickSound()
    {
        if (sfxSource != null && clickSound != null)
        {
            sfxSource.PlayOneShot(clickSound);
        }
    }
}
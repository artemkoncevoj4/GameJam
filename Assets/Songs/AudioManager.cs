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
    private AudioSource sfxSource; // Главный AudioSource для SFX
    
    [Tooltip("Источник для фоновой музыки")]
    public AudioSource musicSource; // Второй AudioSource для музыки (Назначается в Inspector)

    [Header("Настройки Аудио Mixer")]
    [Tooltip("Ссылка на ваш главный Audio Mixer")]
    public AudioMixer masterMixer; 

    // Основные SFX (примеры)
    [Header("1. Основные SFX (по умолчанию)")]
    public AudioClip spawnSound;
    public AudioClip clickSound;
    public AudioClip explosionSound;

    // НОВЫЕ ПОЛЯ ДЛЯ ОСОБЫХ ЗВУКОВ
    [Header("2. Особые SFX (по триггеру)")]
    [Tooltip("Специальный звук для действия зайца")]
    public AudioClip bunnySpecialSound;
    [Tooltip("Звук активации особого события")]
    public AudioClip eventActivationSound;
    [Tooltip("Звук проигрыша/критический SFX")]
    public AudioClip criticalFailureSound;

    // Клипы для эффекта Хаоса
    [Header("3. Звуковые Клипы (Хаос)")]
    public AudioClip glitchSfx;
    public AudioClip horrorSting;
    public AudioClip creepyTensionRise;
    public AudioClip scaryHitsRisers;
    private AudioClip[] chaosClips;

    // Клип для фоновой музыки
    [Header("4. Музыка")]
    public AudioClip backgroundMusicClip;

  private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
            // Ищем все AudioSource на объекте
            AudioSource[] sources = GetComponents<AudioSource>();
            
            // Мы предполагаем, что на объекте есть два AudioSource:
            // Первый - для SFX (мы делаем его приватным)
            // Второй - для Музыки (он публичный и должен быть назначен)
            
            if (sources.Length > 0)
            {
                sfxSource = sources[0];
            }
            else
            {
                // Если нет, создаем его
                sfxSource = gameObject.AddComponent<AudioSource>();
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }

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
    
    // =========================================================================================
    // НОВЫЙ УНИВЕРСАЛЬНЫЙ МЕТОД ДЛЯ ОСОБЫХ SFX
    // Вызывается извне, чтобы воспроизвести любой конкретный SFX
    // =========================================================================================

    /// <summary>
    /// Воспроизводит один короткий звуковой клип, используя SFX-источник.
    /// </summary>
    /// <param name="clip">Клип, который необходимо воспроизвести.</param>
    public void PlaySpecificSFX(AudioClip clip)
    {
        if (sfxSource == null)
        {
            Debug.LogError("<color=red>AudioManager: AudioSource для SFX не найден!</color>");
            return;
        }
        
        if (clip != null)
        {
            // Используем PlayOneShot для коротких звуков, чтобы не прерывать текущие SFX
            sfxSource.PlayOneShot(clip);
            // Debug.Log($"<color=green>AudioManager: Воспроизведен особый SFX: {clip.name}</color>");
        }
        else
        {
            Debug.LogWarning("<color=orange>AudioManager: Попытка воспроизвести пустой AudioClip.</color>");
        }
    }
    
    // =========================================================================================
    // ДОПОЛНИТЕЛЬНЫЕ ПУБЛИЧНЫЕ МЕТОДЫ ДЛЯ ОСОБЫХ ТРИГГЕРОВ (Опционально)
    // Эти методы вызывают универсальный метод PlaySpecificSFX с назначенным клипом
    // =========================================================================================
    
    public void PlayBunnySpecialSound()
    {
        PlaySpecificSFX(bunnySpecialSound);
    }
    
    public void PlayEventActivationSound()
    {
        PlaySpecificSFX(eventActivationSound);
    }

    public void PlayCriticalFailureSound()
    {
        PlaySpecificSFX(criticalFailureSound);
    }

    // =========================================================================================
    // СУЩЕСТВУЮЩИЕ МЕТОДЫ (Музыка и Хаос)
    // =========================================================================================
    
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
        musicSource.clip = backgroundMusicClip;
        musicSource.loop = true; 
        musicSource.Play();
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
            int attempts = 0;
            while (clipToPlay == null && attempts < 100) 
            {
                clipToPlay = chaosClips[UnityEngine.Random.Range(0, chaosClips.Length)];
                attempts++;
            }

            if (clipToPlay != null)
            {
                 sfxSource.PlayOneShot(clipToPlay);
                 // Debug.Log($"<color=green>AudioManager: Воспроизведен звук хаоса: {clipToPlay.name}</color>");
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
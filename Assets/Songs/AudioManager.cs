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

    // НОВЫЕ ПОЛЯ ДЛЯ ОСОБЫХ ЗВУКОВ - РАСШИРЕНО ДО 10
    [Header("2. Особые SFX (по триггеру)")]
    [Tooltip("Специальный звук для действия зайца")]
    public AudioClip bunnySpecialSound;
    
    [Tooltip("Звук активации особого события")]
    public AudioClip eventActivationSound;
    
    [Tooltip("Звук проигрыша/критический SFX")]
    public AudioClip criticalFailureSound;
    
    [Tooltip("Звук достижения успеха")]
    public AudioClip achievementUnlockedSound;
    
    [Tooltip("Звук получения бонуса")]
    public AudioClip bonusCollectedSound;
    
    [Tooltip("Звук телепортации/перемещения")]
    public AudioClip teleportSound;
    
    [Tooltip("Звук открытия двери/сундука")]
    public AudioClip doorOpenSound;
    
    [Tooltip("Звук сбора предмета")]
    public AudioClip itemPickupSound;
    
    [Tooltip("Звук подсказки/напоминания")]
    public AudioClip hintSound;
    
    [Tooltip("Звук завершения уровня")]
    public AudioClip levelCompleteSound;

    // Массив для удобного доступа к особым звукам
    private AudioClip[] specialClipsArray;

    // Клипы для эффекта Хаоса
    [Header("3. Звуковые Клипы (Хаос)")]
    public AudioClip glitchSfx;
    public AudioClip horrorSting;
    public AudioClip creepyTensionRise;
    public AudioClip scaryHitsRisers;
    private AudioClip[] chaosClips;

    [Header("4. Клипы для СЛУЧАЙНЫХ СПЕЦ. SFX (Хаос)")]
    [Tooltip("Клип 1 из 2 для случайного воспроизведения")]
    public AudioClip specialRandomSFX1; 
    
    [Tooltip("Клип 2 из 2 для случайного воспроизведения")]
    public AudioClip specialRandomSFX2;
    
    private AudioClip[] specialRandomClips; // Новый массив для случайного выбора

    // Клип для фоновой музыки
    [Header("5. Музыка")]
    public AudioClip backgroundMusicClip;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
            // Ищем все AudioSource на объекте
            AudioSource[] sources = GetComponents<AudioSource>();
            
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
        
        // Инициализация массива случайных клипов
        specialRandomClips = new AudioClip[]
        {
            specialRandomSFX1,
            specialRandomSFX2,
        };
        
        // Инициализация массива клипов Хаоса
        chaosClips = new AudioClip[]
        {
            glitchSfx,
            horrorSting,
            creepyTensionRise,
            scaryHitsRisers
        };
        
        // Инициализация массива особых звуков (10 штук)
        specialClipsArray = new AudioClip[]
        {
            bunnySpecialSound,
            eventActivationSound,
            criticalFailureSound,
            achievementUnlockedSound,
            bonusCollectedSound,
            teleportSound,
            doorOpenSound,
            itemPickupSound,
            hintSound,
            levelCompleteSound
        };
    }
    
    // [НОВОЕ] Start для автоматического запуска фоновой музыки
    void Start()
    {
        PlayBackgroundMusic();
    }
    
    // =========================================================================================
    // УНИВЕРСАЛЬНЫЕ МЕТОДЫ ДЛЯ ВОСПРОИЗВЕДЕНИЯ SFX
    // =========================================================================================

    /// <summary>
    /// Воспроизводит один короткий звуковой клип
    /// </summary>
    public void PlaySpecificSFX(AudioClip clip)
    {
        if (sfxSource == null)
        {
            Debug.LogError("<color=red>AudioManager: AudioSource для SFX не найден!</color>");
            return;
        }
        
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning("<color=orange>AudioManager: Попытка воспроизвести пустой AudioClip.</color>");
        }
    }
    
    /// <summary>
    /// Воспроизводит специальный звук по индексу (0-9)
    /// </summary>
    /// <param name="index">Индекс в массиве specialClipsArray (0-9)</param>
    public void PlaySpecialSoundByIndex(int index)
    {
        if (index < 0 || index >= specialClipsArray.Length)
        {
            Debug.LogError($"<color=red>AudioManager: Недопустимый индекс {index}. Допустимый диапазон: 0-{specialClipsArray.Length-1}</color>");
            return;
        }
        
        PlaySpecificSFX(specialClipsArray[index]);
    }
    
    /// <summary>
    /// Воспроизводит специальный звук по имени
    /// </summary>
    public void PlaySpecialSoundByName(string soundName)
    {
        // Можно использовать switch или словарь для сопоставления имен
        switch (soundName.ToLower())
        {
            case "bunny":
            case "bunnyspecial":
                PlayBunnySpecialSound();
                break;
            case "event":
            case "eventactivation":
                PlayEventActivationSound();
                break;
            case "critical":
            case "criticalfailure":
                PlayCriticalFailureSound();
                break;
            case "achievement":
                PlayAchievementUnlockedSound();
                break;
            case "bonus":
                PlayBonusCollectedSound();
                break;
            case "teleport":
                PlayTeleportSound();
                break;
            case "door":
                PlayDoorOpenSound();
                break;
            case "item":
                PlayItemPickupSound();
                break;
            case "hint":
                PlayHintSound();
                break;
            case "levelcomplete":
                PlayLevelCompleteSound();
                break;
            default:
                Debug.LogWarning($"<color=orange>AudioManager: Звук с именем '{soundName}' не найден</color>");
                break;
        }
    }

    public void PlayRandomSpecialSFX()
    {
        if (sfxSource == null)
        {
            Debug.LogError("<color=red>AudioManager: AudioSource для SFX не найден!</color>");
            return;
        }

        if (specialRandomClips.Length > 0)
        {
            AudioClip clipToPlay = null;
            int attempts = 0;
            
            while (clipToPlay == null && attempts < 100) 
            {
                clipToPlay = specialRandomClips[UnityEngine.Random.Range(0, specialRandomClips.Length)];
                attempts++;
            }

            if (clipToPlay != null)
            {
                 sfxSource.PlayOneShot(clipToPlay);
            }
            else
            {
                 Debug.LogWarning("<color=orange>AudioManager: В массиве specialRandomClips не назначены AudioClip.</color>");
            }
        }
        else
        {
            Debug.LogWarning("<color=orange>AudioManager: Массив specialRandomClips пуст.</color>");
        }
    }
    
    // =========================================================================================
    // ПУБЛИЧНЫЕ МЕТОДЫ ДЛЯ КАЖДОГО СПЕЦИАЛЬНОГО ЗВУКА
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
    
    public void PlayAchievementUnlockedSound()
    {
        PlaySpecificSFX(achievementUnlockedSound);
    }
    
    public void PlayBonusCollectedSound()
    {
        PlaySpecificSFX(bonusCollectedSound);
    }
    
    public void PlayTeleportSound()
    {
        PlaySpecificSFX(teleportSound);
    }
    
    public void PlayDoorOpenSound()
    {
        PlaySpecificSFX(doorOpenSound);
    }
    
    public void PlayItemPickupSound()
    {
        PlaySpecificSFX(itemPickupSound);
    }
    
    public void PlayHintSound()
    {
        PlaySpecificSFX(hintSound);
    }
    
    public void PlayLevelCompleteSound()
    {
        PlaySpecificSFX(levelCompleteSound);
    }

    // =========================================================================================
    // ОСТАЛЬНЫЕ МЕТОДЫ
    // =========================================================================================
    
    public void PlayBackgroundMusic()
    {
        if (musicSource == null)
        {
            Debug.LogError("<color=red>AudioManager: Music Source не назначен!</color>");
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
        if (sfxSource == null)
        {
            Debug.LogError("<color=red>AudioManager: AudioSource для SFX не найден!</color>");
            return;
        }

        if (chaosClips.Length > 0)
        {
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
            }
            else
            {
                 Debug.LogWarning("<color=orange>AudioManager: Не удалось найти валидный клип в массиве chaosClips.</color>");
            }
        }
        else
        {
            Debug.LogWarning("<color=orange>AudioManager: Массив chaosClips пуст.</color>");
        }
    }
    
    public void PlaySpawnSound()
    {
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
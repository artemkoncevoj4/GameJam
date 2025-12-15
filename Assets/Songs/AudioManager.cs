using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

// * Буйначева В.А.

public class AudioManager : MonoBehaviour
{
    // Статическая переменная для доступа к AudioManager из любого места (Singleton Pattern)
    public static AudioManager Instance;

    // Ссылки на Audio Source
    [Header("Источники звука")]
    [Tooltip("Источник для коротких звуковых эффектов (PlayOneShot)")]
    public AudioSource sfxSource;
    
    [Tooltip("Источник для фоновой музыки")]
    public AudioSource musicSource;

    [Header("Настройки Аудио Mixer")]
    [Tooltip("Ссылка на ваш главный Audio Mixer")]
    public AudioMixer masterMixer; 
    [Tooltip("Имя группы микшера для SFX")]
    public string sfxMixerGroupName = "SFX";
    [Tooltip("Имя группы микшера для музыки")]
    public string musicMixerGroupName = "Music";


    [Header("1. Основные SFX (по умолчанию)")]
    public AudioClip spawnSound;
    public AudioClip clickSound;
    public AudioClip explosionSound;
    [Header("2. Особые SFX (по триггеру)")]
    [Tooltip("Звук сердцебиения")]
    public AudioClip HeartbeatSound;
    [Tooltip("Звук использования канцелярской ручки")]
    public AudioClip PenSound;
    [Tooltip("Звук использования штампа")]
    public AudioClip StampSound;
    [Tooltip("Звук разрыва документа (неравильно выполнено задание)")]
    public AudioClip PaperRipFastSound;    
    [Tooltip("Звук использования бумаги")]
    public AudioClip PaperSound;
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
    // Клип для фоновой музыки
    [Header("5. Музыка")]
    public AudioClip backgroundMusicClip;


    private Dictionary<string, AudioClip> soundClips; // Словарь для удобного доступа к звукам по имени
    private AudioClip[] specialRandomClips; // Массив для случайного выбора спец. звуков
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
            if (sfxSource == null)
            {
                Debug.LogError("<color=red>AudioManager: sfxSource не назначен в инспекторе!</color>");
            }
            if (musicSource == null)
            {
                Debug.LogError("<color=red>AudioManager: musicSource не назначен в инспекторе!</color>");
            }

            if (masterMixer != null)
            {
                
                if (sfxSource != null)
                {
                    AudioMixerGroup[] sfxGroups = masterMixer.FindMatchingGroups(sfxMixerGroupName);
                    if (sfxGroups.Length > 0)
                    {
                        sfxSource.outputAudioMixerGroup = sfxGroups[0];
                    }
                    else
                    {
                        Debug.LogWarning($"<color=orange>AudioManager: Mixer group с именем '{sfxMixerGroupName}' не найдена.</color>");
                    }
                }

                if (musicSource != null)
                {
                    AudioMixerGroup[] musicGroups = masterMixer.FindMatchingGroups(musicMixerGroupName);
                    if (musicGroups.Length > 0)
                    {
                        musicSource.outputAudioMixerGroup = musicGroups[0];
                    }
                    else
                    {
                        Debug.LogWarning($"<color=orange>AudioManager: Mixer group с именем '{musicMixerGroupName}' не найдена.</color>");
                    }
                }
            }
            else
            {
                Debug.LogWarning("<color=orange>AudioManager: masterMixer не назначен. Звуки будут воспроизводиться без микшера.</color>");
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        /// <summary>
        /// Инициализации словаря и массивов.
        /// </summary>
        soundClips = new Dictionary<string, AudioClip>(StringComparer.OrdinalIgnoreCase)
        {
            // Основные SFX
            { "spawn", spawnSound },
            { "click", clickSound },
            { "explosion", explosionSound },

            // Особые SFX
            { "heartbeat", HeartbeatSound },
            { "pen", PenSound },
            { "paper", PaperSound },
            { "paperripfast", PaperRipFastSound },
            { "stamp", StampSound }
        };
        
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
    }
    void Start()
    {
        PlayBackgroundMusic();
    }
    /// <summary>
    /// МЕТОДЫ ДЛЯ ВОСПРОИЗВЕДЕНИЯ ЗВУКОВ.
    /// </summary>
    private void PlaySpecificSFX(AudioClip clip)
    {
        if (sfxSource == null)
        {
            Debug.LogError("<color=red>AudioManager: AudioSource для SFX не назначен!</color>");
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
    
    public void PlaySoundByName(string soundName)
    {
        if (soundClips.TryGetValue(soundName, out AudioClip clip))
        {
            PlaySpecificSFX(clip);
        }
        else
        {
            Debug.LogWarning($"<color=orange>AudioManager: Звук с именем '{soundName}' не найден</color>");
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
}